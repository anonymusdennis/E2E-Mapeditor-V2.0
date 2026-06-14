using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NetworkLoadable;
using UnityEngine;

public class ScoreManager : T17MonoBehaviour, INetworkLoadable, Saveable
{
	[Serializable]
	public class PlayerScorePODO
	{
		public ushort m_CharactersKnockedOut;

		public ushort m_ItemsCrafted;

		public ushort m_FavoursCompleted;

		public ushort m_TilesDestroyed;

		public uint m_Steps;

		public float m_IngameSecondsTakenToEscape;

		public int m_PlayerIndex;

		public void Reset()
		{
			m_CharactersKnockedOut = 0;
			m_ItemsCrafted = 0;
			m_FavoursCompleted = 0;
			m_TilesDestroyed = 0;
			m_Steps = 0u;
			m_IngameSecondsTakenToEscape = 0f;
		}
	}

	public enum Events
	{
		FavourCompleted,
		ItemCrafted,
		KnockedOutCharacter,
		PrisonTileDestroyed,
		EscapeTriggerReached,
		SpecialEscapeTriggerReached,
		FootstepsTaken
	}

	[Serializable]
	private class SaveData_ScoreManager_V1 : PrisonSnapshotIO.SnapshotData_Base
	{
		public PlayerScorePODO[] O;

		public SaveData_ScoreManager_V1()
		{
			m_Version = 1;
		}
	}

	private static ScoreManager s_Instance;

	[Header("Misc")]
	public float m_NumStepsInKilometer = 500f;

	public const int FOOTSTEPS_SEND_THRESHOLD = 20;

	private PlayerScorePODO[] m_PlayerScores = new PlayerScorePODO[4];

	private T17NetView m_NetView;

	private SaveDataRegister m_SaveData;

	private string m_LoadError = string.Empty;

	private LOADSTATE m_LoadState;

	public static ScoreManager GetInstance()
	{
		return s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		s_Instance = this;
		m_NetView = GetComponent<T17NetView>();
		for (int i = 0; i < m_PlayerScores.Length; i++)
		{
			m_PlayerScores[i] = new PlayerScorePODO();
			m_PlayerScores[i].m_PlayerIndex = i;
		}
		m_SaveData = new SaveDataRegister(this, m_NetView.viewID, bIsMajorManagerComponent: true, 8);
		NetLoadManagerSync.m_AllNetworkLoadables.Add(this);
		Gamer.OnDeleteImminent += Gamer_OnDeleteImminent;
	}

	protected virtual void OnDestroy()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
		}
		if (m_SaveData != null)
		{
			m_SaveData.Dispose();
		}
		NetLoadManagerSync.m_AllNetworkLoadables.Remove(this);
		Gamer.OnDeleteImminent -= Gamer_OnDeleteImminent;
		m_NetView = null;
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		return base.StartInit();
	}

	private void Gamer_OnDeleteImminent(Gamer gamer)
	{
		ConfigManager instance = ConfigManager.GetInstance();
		if (!(instance != null) || instance.gameType != PrisonConfig.ConfigType.Versus || gamer == null)
		{
			return;
		}
		Player playerObject = gamer.m_PlayerObject;
		if (playerObject != null && s_Instance != null)
		{
			int spawnIndex = playerObject.m_SpawnIndex;
			if (spawnIndex >= 0 && spawnIndex < s_Instance.m_PlayerScores.Length)
			{
				m_PlayerScores[spawnIndex].Reset();
			}
		}
	}

	public int GetPlayerIndexMostCrafted()
	{
		PlayerScorePODO firstWithHighestValue = GetFirstWithHighestValue((PlayerScorePODO x) => x.m_ItemsCrafted);
		if (firstWithHighestValue != null && firstWithHighestValue.m_ItemsCrafted > 0)
		{
			return firstWithHighestValue.m_PlayerIndex;
		}
		return -1;
	}

	public int GetPlayerIndexMostKnockouts()
	{
		PlayerScorePODO firstWithHighestValue = GetFirstWithHighestValue((PlayerScorePODO x) => x.m_CharactersKnockedOut);
		if (firstWithHighestValue != null && firstWithHighestValue.m_CharactersKnockedOut > 0)
		{
			return firstWithHighestValue.m_PlayerIndex;
		}
		return -1;
	}

	public int GetPlayerIndexMostFavoursCompleted()
	{
		PlayerScorePODO firstWithHighestValue = GetFirstWithHighestValue((PlayerScorePODO x) => x.m_FavoursCompleted);
		if (firstWithHighestValue != null && firstWithHighestValue.m_FavoursCompleted > 0)
		{
			return firstWithHighestValue.m_PlayerIndex;
		}
		return -1;
	}

	public int GetPlayerIndexMostTilesDestroyed()
	{
		PlayerScorePODO firstWithHighestValue = GetFirstWithHighestValue((PlayerScorePODO x) => x.m_TilesDestroyed);
		if (firstWithHighestValue != null && firstWithHighestValue.m_TilesDestroyed > 0)
		{
			return firstWithHighestValue.m_PlayerIndex;
		}
		return -1;
	}

	public PlayerScorePODO GetFirstWithHighestValue<T>(Func<PlayerScorePODO, T> function, bool unique = true) where T : IComparable<T>
	{
		T other = function(m_PlayerScores[0]);
		PlayerScorePODO playerScorePODO = m_PlayerScores[0];
		for (int i = 1; i < m_PlayerScores.Length; i++)
		{
			T val = function(m_PlayerScores[i]);
			if (val.CompareTo(other) > 0)
			{
				other = val;
				playerScorePODO = m_PlayerScores[i];
			}
		}
		if (unique)
		{
			List<Player> allPlayers = Player.GetAllPlayers();
			for (int j = 0; j < m_PlayerScores.Length; j++)
			{
				if (m_PlayerScores[j] == playerScorePODO)
				{
					continue;
				}
				bool flag = false;
				for (int k = 0; k < allPlayers.Count; k++)
				{
					if (allPlayers[k].m_SpawnIndex == m_PlayerScores[j].m_PlayerIndex)
					{
						flag = allPlayers[k] != null && allPlayers[k].m_Gamer != null;
						break;
					}
				}
				if (flag && function(m_PlayerScores[j]).CompareTo(other) == 0)
				{
					playerScorePODO = null;
					break;
				}
			}
		}
		return playerScorePODO;
	}

	public static PlayerScorePODO GetScorePodoForCharacter(Character character)
	{
		if (s_Instance != null && character != null && character.m_CharacterStats.m_bIsPlayer)
		{
			Player player = character as Player;
			int spawnIndex = player.m_SpawnIndex;
			if (spawnIndex >= 0 && spawnIndex < s_Instance.m_PlayerScores.Length)
			{
				return s_Instance.m_PlayerScores[spawnIndex];
			}
		}
		return null;
	}

	public static string GetGradedScore(float elapsedIngameSeconds, out Sprite gradeSprite, out int gradeLevel)
	{
		gradeSprite = null;
		gradeLevel = int.MaxValue;
		ScoreSystemConfig scoreSystemConfig = ConfigManager.GetInstance().ScoreSystemConfig;
		if (scoreSystemConfig != null)
		{
			List<ScoreSystemConfig.GradeTimeRange> grades = scoreSystemConfig.m_Grades;
			if (grades != null && grades.Count > 0)
			{
				grades.Sort(delegate(ScoreSystemConfig.GradeTimeRange x, ScoreSystemConfig.GradeTimeRange y)
				{
					TimeSpan timeSpan3 = new TimeSpan(x.m_Days, x.m_Hours, x.m_Minutes, 0);
					TimeSpan timeSpan4 = new TimeSpan(y.m_Days, y.m_Hours, y.m_Minutes, 0);
					if (timeSpan3 < timeSpan4)
					{
						return -1;
					}
					return (timeSpan3 > timeSpan4) ? 1 : 0;
				});
				string localisedGradeText = grades[grades.Count - 1].m_LocalisedGradeText;
				gradeSprite = grades[grades.Count - 1].m_GradeSprite;
				gradeLevel = grades.Count - 1;
				if (elapsedIngameSeconds > 0f)
				{
					TimeSpan timeSpan = TimeSpan.FromSeconds(elapsedIngameSeconds);
					for (int num = grades.Count - 2; num >= 0; num--)
					{
						ScoreSystemConfig.GradeTimeRange gradeTimeRange = grades[num];
						TimeSpan timeSpan2 = new TimeSpan(gradeTimeRange.m_Days, gradeTimeRange.m_Hours, gradeTimeRange.m_Minutes, 0);
						if (timeSpan < timeSpan2)
						{
							localisedGradeText = gradeTimeRange.m_LocalisedGradeText;
							gradeSprite = grades[num].m_GradeSprite;
							gradeLevel = num;
						}
					}
				}
				return localisedGradeText;
			}
		}
		return null;
	}

	public static void EventRPC(Events theEvent, Character characterWhoDidEvent)
	{
		if (s_Instance == null || !(characterWhoDidEvent != null) || !characterWhoDidEvent.m_CharacterStats.m_bIsPlayer)
		{
			return;
		}
		Player player = characterWhoDidEvent as Player;
		int spawnIndex = player.m_SpawnIndex;
		if (spawnIndex < 0 || spawnIndex >= s_Instance.m_PlayerScores.Length)
		{
			return;
		}
		if (theEvent == Events.EscapeTriggerReached || theEvent == Events.SpecialEscapeTriggerReached)
		{
			float num = float.MaxValue;
			if (RoutineManager.GetInstance() != null)
			{
				num = RoutineManager.GetInstance().GetSecondsSinceInitialTime();
			}
			s_Instance.m_NetView.RPC("RPC_EscapeEvent", NetTargets.All, theEvent, spawnIndex, num);
		}
		else
		{
			s_Instance.m_NetView.PostLevelLoadRPC("RPC_Event", NetTargets.All, theEvent, spawnIndex);
		}
	}

	[PunRPC]
	protected void RPC_Event(Events theEvent, int playerIndex)
	{
		PlayerScorePODO playerScorePODO = m_PlayerScores[playerIndex];
		switch (theEvent)
		{
		case Events.ItemCrafted:
			playerScorePODO.m_ItemsCrafted++;
			break;
		case Events.FavourCompleted:
			playerScorePODO.m_FavoursCompleted++;
			break;
		case Events.KnockedOutCharacter:
			playerScorePODO.m_CharactersKnockedOut++;
			break;
		case Events.PrisonTileDestroyed:
			playerScorePODO.m_TilesDestroyed++;
			break;
		case Events.FootstepsTaken:
			playerScorePODO.m_Steps += 20u;
			break;
		case Events.EscapeTriggerReached:
		case Events.SpecialEscapeTriggerReached:
			break;
		}
	}

	[PunRPC]
	protected void RPC_EscapeEvent(Events theEvent, int playerIndex, float escapeTime)
	{
		PlayerScorePODO playerScorePODO = m_PlayerScores[playerIndex];
		playerScorePODO.m_IngameSecondsTakenToEscape = escapeTime;
	}

	public string CreateSnapshot()
	{
		SaveData_ScoreManager_V1 saveData_ScoreManager_V = new SaveData_ScoreManager_V1();
		saveData_ScoreManager_V.O = m_PlayerScores;
		return JsonUtility.ToJson(saveData_ScoreManager_V);
	}

	public void StartedFromSnapshot()
	{
		SaveData_ScoreManager_V1 snapshotData = GetSnapshotData();
		if (snapshotData != null && snapshotData.O != null && snapshotData.O.Length > 0)
		{
			m_PlayerScores = snapshotData.O;
		}
	}

	private SaveData_ScoreManager_V1 GetSnapshotData()
	{
		if (m_SaveData == null)
		{
			return null;
		}
		if (string.IsNullOrEmpty(m_SaveData.GetSaveData()))
		{
			return null;
		}
		PrisonSnapshotIO.SnapshotData_Base snapshotData_Base = null;
		try
		{
			snapshotData_Base = JsonUtility.FromJson<PrisonSnapshotIO.SnapshotData_Base>(m_SaveData.GetSaveData());
		}
		catch
		{
		}
		if (snapshotData_Base != null && snapshotData_Base.m_Version == 1)
		{
			string saveData = m_SaveData.GetSaveData();
			SaveData_ScoreManager_V1 saveData_ScoreManager_V = null;
			try
			{
				saveData_ScoreManager_V = JsonUtility.FromJson<SaveData_ScoreManager_V1>(saveData);
			}
			catch
			{
			}
			if (saveData_ScoreManager_V != null)
			{
				return saveData_ScoreManager_V;
			}
		}
		return null;
	}

	public void ResetLoadState()
	{
		if (T17NetManager.IsMasterClient)
		{
			m_LoadState = LOADSTATE.Finished_OK;
			m_LoadError = string.Empty;
		}
		else
		{
			m_LoadState = LOADSTATE.NotStarted;
			m_LoadError = string.Empty;
		}
	}

	public LOADSTATE GetLoadState()
	{
		return m_LoadState;
	}

	public string GetLoadError()
	{
		return m_LoadError;
	}

	public void SendLoadDataToClientRPC(PhotonPlayer player)
	{
		if (T17NetManager.IsMasterClient && !player.IsLocal)
		{
			PlayerScorePODO[] playerScores = m_PlayerScores;
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using MemoryStream memoryStream = new MemoryStream();
			binaryFormatter.Serialize(memoryStream, playerScores);
			m_NetView.RPC("RPC_ClientRecieveScores", player, memoryStream.ToArray());
		}
	}

	[PunRPC]
	private void RPC_ClientRecieveScores(byte[] objects, PhotonMessageInfo info)
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using (MemoryStream serializationStream = new MemoryStream(objects))
		{
			PlayerScorePODO[] array = binaryFormatter.Deserialize(serializationStream) as PlayerScorePODO[];
			if (array.Length == 0)
			{
			}
			m_PlayerScores = array;
		}
		m_LoadState = LOADSTATE.Finished_OK;
	}
}
