using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
	[Serializable]
	public class TutorialData
	{
		public TutorialSubject m_TutorialSubject = TutorialSubject.UNASSIGNED;

		public int m_RepetitionsRequired = 1;

		public int m_DaysUntilRepeat;

		public int m_StartAfterDay;

		public TextAsset m_TutorialTree;

		public Routines m_RoutineTrigger = Routines.UNASSIGNED;

		public RoomBlob.eLocation m_RoomTrigger;
	}

	public class TutorialSave
	{
		public int m_TutorialSaveSubject = -1;

		public int m_Repetitions;

		public int m_DaysSinceRepeat;

		public bool m_bCurrentlyActive;
	}

	private static TutorialManager m_Instance;

	public List<TutorialData> m_TutorialList = new List<TutorialData>();

	private Dictionary<TutorialSubject, TutorialSave>[] m_PlayerSaves = new Dictionary<TutorialSubject, TutorialSave>[4];

	public TutorialSubject[] m_SubjectsToIgnore = new TutorialSubject[0];

	public TutorialSubject[] m_SubjectsToIgnoreTransport = new TutorialSubject[0];

	private List<TutorialPopup> m_TutorialPopups;

	public List<BaseItemFunctionality.Functionality> m_ItemFunctionalitiesForUseTut = new List<BaseItemFunctionality.Functionality>();

	public T17NetView m_NetView;

	public static TutorialManager GetInstance()
	{
		return m_Instance;
	}

	private void Awake()
	{
		if (m_Instance == null)
		{
			m_Instance = this;
		}
	}

	private void Start()
	{
		if (m_TutorialPopups == null)
		{
			m_TutorialPopups = Resources.LoadAll<TutorialPopup>("Prefabs/TutorialPopups").ToList();
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_TutorialPopups != null)
		{
			m_TutorialPopups.Clear();
			Resources.UnloadUnusedAssets();
		}
		if (m_Instance == this)
		{
			m_Instance = null;
		}
		m_NetView = null;
	}

	public TutorialPopup GetTutorialPrefabByID(int groupID)
	{
		TutorialPopup result = null;
		for (int i = 0; i < m_TutorialPopups.Count; i++)
		{
			if (m_TutorialPopups[i].m_TutorialPopupID == groupID)
			{
				result = m_TutorialPopups[i];
				break;
			}
		}
		return result;
	}

	public void OnLevelStart()
	{
		if (RoutineManager.GetInstance() != null)
		{
			RoutineManager.GetInstance().OnDayChange += UpdateTutorialDays;
			RoutineManager.GetInstance().OnRoutineChanged += CheckRoutineTutorials;
		}
		for (int i = 0; i < m_TutorialList.Count; i++)
		{
			ObjectiveManager.GetInstance().CacheObjectTreeData(m_TutorialList[i].m_TutorialTree);
		}
	}

	public void OnLevelEnd()
	{
		ObjectiveManager.GetInstance().ClearObjectTreeDataCache();
		if (RoutineManager.GetInstance() != null)
		{
			RoutineManager.GetInstance().OnDayChange -= UpdateTutorialDays;
			RoutineManager.GetInstance().OnRoutineChanged -= CheckRoutineTutorials;
		}
	}

	private void UpdateTutorialDays()
	{
		GlobalSave instance = GlobalSave.GetInstance();
		for (int i = 0; i < m_PlayerSaves.Length; i++)
		{
			if (m_PlayerSaves[i] == null || m_PlayerSaves[i].Count <= 0)
			{
				continue;
			}
			int[] array = new int[m_TutorialList.Count];
			int[] array2 = new int[m_TutorialList.Count];
			int[] array3 = new int[m_TutorialList.Count];
			Dictionary<TutorialSubject, TutorialSave> dictionary = m_PlayerSaves[i];
			if (dictionary == null)
			{
				continue;
			}
			for (int j = 0; j < m_TutorialList.Count; j++)
			{
				TutorialSubject key = (TutorialSubject)j;
				if (dictionary.ContainsKey(key))
				{
					TutorialSave tutorialSave = dictionary[key];
					tutorialSave.m_DaysSinceRepeat++;
					array[j] = tutorialSave.m_TutorialSaveSubject;
					array2[j] = tutorialSave.m_Repetitions;
					array3[j] = tutorialSave.m_DaysSinceRepeat;
				}
				instance.Set("Tutorial:Subjects", array);
				instance.Set("Tutorial:Repetitions", array2);
				instance.Set("Tutorial:DaysSinceRep", array3);
				instance.RequestSave();
			}
		}
	}

	public void CheckRoomTutorials(Player player, RoomBlob previousRoom, RoomBlob newRoom)
	{
		if (!(newRoom != null))
		{
			return;
		}
		PrisonData.LevelInfo currentLevelInfo = LevelScript.GetCurrentLevelInfo();
		if (currentLevelInfo != null && player.GetMyCell() == previousRoom && currentLevelInfo.m_PrisonType != LevelScript.PRISON_TYPE.Transport)
		{
			StartTutorialRPC(player, TutorialSubject.Routines);
		}
		for (int i = 0; i < m_TutorialList.Count; i++)
		{
			if (m_TutorialList[i].m_RoomTrigger == newRoom.location && m_TutorialList[i].m_RoomTrigger != 0)
			{
				StartTutorialRPC(player, m_TutorialList[i].m_TutorialSubject);
			}
		}
	}

	public void CheckRoutineTutorials(RoutinesData.Routine previousRoutine, RoutinesData.Routine newRoutine, bool forced)
	{
		PrisonData.LevelInfo currentLevelInfo = LevelScript.GetCurrentLevelInfo();
		if (currentLevelInfo == null || currentLevelInfo.m_PrisonType == LevelScript.PRISON_TYPE.Transport)
		{
			return;
		}
		List<Player> allPlayers = Player.GetAllPlayers();
		for (int i = 0; i < allPlayers.Count; i++)
		{
			if (!(allPlayers[i] != null) || allPlayers[i].m_Gamer == null)
			{
				continue;
			}
			for (int j = 0; j < m_TutorialList.Count; j++)
			{
				if (m_TutorialList[j].m_RoutineTrigger == newRoutine.m_BaseRoutineType)
				{
					StartTutorialRPC(allPlayers[i], m_TutorialList[j].m_TutorialSubject);
				}
			}
		}
	}

	private void StartTutorial(Player player, TutorialSubject subject, bool forceShow)
	{
		if (subject == TutorialSubject.UNASSIGNED || !(player != null) || player.m_Gamer == null || !player.m_Gamer.IsLocal() || player.m_Gamer.m_RewiredPlayer == null || !player.bDisplayTutorials || CutsceneManagerBase.GetState() != CutsceneManagerBase.States.Idle || LevelScript.GetCurrentLevelInfo() == null || LevelScript.GetCurrentLevelInfo().m_PrisonType == LevelScript.PRISON_TYPE.Tutorial)
		{
			return;
		}
		TutorialData tutorialData = GetTutorialData(subject);
		Dictionary<TutorialSubject, TutorialSave> dictionary = m_PlayerSaves[player.m_Gamer.m_RewiredPlayer.id];
		if (dictionary == null || tutorialData == null)
		{
			return;
		}
		TutorialSave value = null;
		if (!dictionary.TryGetValue(tutorialData.m_TutorialSubject, out value))
		{
			TutorialSave tutorialSave = new TutorialSave();
			tutorialSave.m_TutorialSaveSubject = (int)subject;
			dictionary.Add(subject, tutorialSave);
			value = tutorialSave;
		}
		if (tutorialData == null || !(tutorialData.m_TutorialTree != null) || value == null)
		{
			return;
		}
		RoutineManager instance = RoutineManager.GetInstance();
		if (instance != null && instance.GetDaysElapsed() >= tutorialData.m_StartAfterDay && !value.m_bCurrentlyActive && ((value.m_Repetitions <= tutorialData.m_RepetitionsRequired && (value.m_DaysSinceRepeat == 0 || value.m_DaysSinceRepeat >= tutorialData.m_DaysUntilRepeat)) || forceShow))
		{
			List<ObjectiveTree> list = new List<ObjectiveTree>();
			if (ObjectiveManager.GetInstance().LoadObjectiveTreeData(tutorialData.m_TutorialTree, ref list))
			{
				list[0].BuildOrderList();
				list[0].MainBranch.SetBaseInfo(player, player);
				list[0].MainBranch.PickAllRandomTargets();
				list[0].m_bShowTrackingArrows = false;
				list[0].m_bShowTrackingPins = false;
				list[0].m_bIsTrackable = false;
				list[0].m_bShowInJournal = false;
			}
			for (int i = 0; i < list.Count; i++)
			{
				list[i].Initialize();
			}
			ObjectiveManager.GetInstance().AddActiveTrees(player, list);
			value.m_bCurrentlyActive = true;
		}
	}

	public void TutorialComplete(Player player, TutorialSubject subject)
	{
		if (!(player == null) && player.m_Gamer != null && player.m_Gamer.m_RewiredPlayer != null)
		{
			int id = player.m_Gamer.m_RewiredPlayer.id;
			if ((m_PlayerSaves[id] != null || m_PlayerSaves[id].Count > 0) && m_PlayerSaves[id].TryGetValue(subject, out var value))
			{
				value.m_Repetitions++;
				value.m_DaysSinceRepeat = 0;
				value.m_bCurrentlyActive = false;
				SavePlayerData(id);
			}
		}
	}

	public void StartTutorialRPC(Player player, TutorialSubject subject, bool forceShow = false)
	{
		for (int i = 0; i < m_SubjectsToIgnore.Length; i++)
		{
			if (m_SubjectsToIgnore[i] == subject)
			{
				return;
			}
		}
		PrisonData.LevelInfo currentLevelInfo = LevelScript.GetCurrentLevelInfo();
		if (currentLevelInfo != null && currentLevelInfo.m_PrisonType == LevelScript.PRISON_TYPE.Transport)
		{
			for (int j = 0; j < m_SubjectsToIgnoreTransport.Length; j++)
			{
				if (m_SubjectsToIgnoreTransport[j] == subject)
				{
					return;
				}
			}
		}
		if (player != null)
		{
			if (player.m_Gamer != null && player.m_Gamer.IsLocal())
			{
				StartTutorial(player, subject, forceShow);
			}
			else if (player.m_NetView != null)
			{
				m_NetView.RPC("RPC_StartTutorial", player.m_NetView, (byte)subject, player.m_NetView.viewID, forceShow);
			}
		}
	}

	[PunRPC]
	private void RPC_StartTutorial(byte eTutorialSubject, int playerID, bool forceShow)
	{
		Player player = T17NetView.Find<Player>(playerID);
		if (player != null)
		{
			StartTutorial(player, (TutorialSubject)eTutorialSubject, forceShow);
		}
	}

	public void AddPlayerSave(Player player)
	{
		if (player == null || player.m_Gamer == null || player.m_Gamer.m_RewiredPlayer == null)
		{
			return;
		}
		if (m_PlayerSaves[player.m_Gamer.m_RewiredPlayer.id] == null)
		{
			m_PlayerSaves[player.m_Gamer.m_RewiredPlayer.id] = new Dictionary<TutorialSubject, TutorialSave>();
		}
		int[] value = new int[m_TutorialList.Count];
		int[] value2 = new int[m_TutorialList.Count];
		int[] value3 = new int[m_TutorialList.Count];
		GlobalSave instance = GlobalSave.GetInstance();
		if (instance.Get("Tutorial:Subjects", out value, new int[m_TutorialList.Count]) && instance.Get("Tutorial:Repetitions", out value2, new int[m_TutorialList.Count]) && instance.Get("Tutorial:DaysSinceRep", out value3, new int[m_TutorialList.Count]) && player.m_Gamer.m_bPrimaryLocal)
		{
			for (int i = 0; i < value.Length; i++)
			{
				if (!m_PlayerSaves[player.m_Gamer.m_RewiredPlayer.id].ContainsKey((TutorialSubject)value[i]))
				{
					TutorialSave tutorialSave = new TutorialSave();
					tutorialSave.m_DaysSinceRepeat = value3[i];
					tutorialSave.m_Repetitions = value2[i];
					tutorialSave.m_TutorialSaveSubject = value[i];
					int id = player.m_Gamer.m_RewiredPlayer.id;
					if (m_PlayerSaves[id].ContainsKey((TutorialSubject)value[i]))
					{
						m_PlayerSaves[id][(TutorialSubject)value[i]] = tutorialSave;
					}
					else
					{
						m_PlayerSaves[id].Add((TutorialSubject)value[i], tutorialSave);
					}
				}
			}
			return;
		}
		for (int j = 0; j < m_TutorialList.Count; j++)
		{
			if (!m_PlayerSaves[player.m_Gamer.m_RewiredPlayer.id].ContainsKey(m_TutorialList[j].m_TutorialSubject))
			{
				TutorialSave tutorialSave2 = new TutorialSave();
				tutorialSave2.m_DaysSinceRepeat = 0;
				tutorialSave2.m_Repetitions = 0;
				tutorialSave2.m_TutorialSaveSubject = (int)m_TutorialList[j].m_TutorialSubject;
				int id2 = player.m_Gamer.m_RewiredPlayer.id;
				if (m_PlayerSaves[id2].ContainsKey(m_TutorialList[j].m_TutorialSubject))
				{
					m_PlayerSaves[id2][m_TutorialList[j].m_TutorialSubject] = tutorialSave2;
				}
				else
				{
					m_PlayerSaves[id2].Add(m_TutorialList[j].m_TutorialSubject, tutorialSave2);
				}
			}
		}
		if (player.m_Gamer.m_bPrimaryLocal)
		{
			instance.Set("Tutorial:Subjects", value);
			instance.Set("Tutorial:Repetitions", value2);
			instance.Set("Tutorial:DaysSinceRep", value3);
			instance.RequestSave();
		}
	}

	private TutorialData GetTutorialData(TutorialSubject subject)
	{
		for (int i = 0; i < m_TutorialList.Count; i++)
		{
			if (subject == m_TutorialList[i].m_TutorialSubject)
			{
				return m_TutorialList[i];
			}
		}
		return null;
	}

	private void SavePlayerData(int playerIndex)
	{
		int[] array = new int[m_TutorialList.Count];
		int[] array2 = new int[m_TutorialList.Count];
		int[] array3 = new int[m_TutorialList.Count];
		GlobalSave instance = GlobalSave.GetInstance();
		if (playerIndex >= m_PlayerSaves.Length || playerIndex < 0)
		{
			return;
		}
		for (int i = 0; i < m_TutorialList.Count; i++)
		{
			if (m_PlayerSaves[playerIndex].TryGetValue((TutorialSubject)i, out var value) && value != null)
			{
				array[i] = value.m_TutorialSaveSubject;
				array2[i] = value.m_Repetitions;
				array3[i] = value.m_DaysSinceRepeat;
			}
		}
		instance.Set("Tutorial:Subjects", array);
		instance.Set("Tutorial:Repetitions", array2);
		instance.Set("Tutorial:DaysSinceRep", array3);
		instance.RequestSave();
	}

	public static void ClearSaveDataDebug()
	{
		GetInstance().ClearSaveData();
	}

	public void ClearSaveData()
	{
		List<Player> allPlayers = Player.GetAllPlayers();
		for (int i = 0; i < allPlayers.Count; i++)
		{
			ClearSaveData(i);
		}
	}

	public void ClearSaveData(int playerIndex)
	{
		GlobalSave instance = GlobalSave.GetInstance();
		List<Player> allPlayers = Player.GetAllPlayers();
		if (playerIndex >= allPlayers.Count || allPlayers[playerIndex].m_Gamer == null || allPlayers[playerIndex].m_Gamer.m_RewiredPlayer == null || !allPlayers[playerIndex].m_Gamer.IsLocal())
		{
			return;
		}
		m_PlayerSaves[allPlayers[playerIndex].m_Gamer.m_RewiredPlayer.id] = new Dictionary<TutorialSubject, TutorialSave>();
		if (allPlayers[playerIndex].m_Gamer.m_bPrimaryLocal)
		{
			int[] array = new int[m_TutorialList.Count];
			int[] array2 = new int[m_TutorialList.Count];
			int[] array3 = new int[m_TutorialList.Count];
			for (int i = 0; i < m_TutorialList.Count; i++)
			{
				TutorialSave tutorialSave = new TutorialSave();
				tutorialSave.m_DaysSinceRepeat = 0;
				tutorialSave.m_Repetitions = 0;
				tutorialSave.m_TutorialSaveSubject = (int)m_TutorialList[i].m_TutorialSubject;
				m_PlayerSaves[allPlayers[playerIndex].m_Gamer.m_RewiredPlayer.id][m_TutorialList[i].m_TutorialSubject] = tutorialSave;
				array3[i] = tutorialSave.m_DaysSinceRepeat;
				array2[i] = tutorialSave.m_Repetitions;
				array[i] = tutorialSave.m_TutorialSaveSubject;
			}
			instance.Set("Tutorial:Subjects", array);
			instance.Set("Tutorial:Repetitions", array2);
			instance.Set("Tutorial:DaysSinceRep", array3);
			instance.RequestSave();
			return;
		}
		for (int j = 0; j < m_TutorialList.Count; j++)
		{
			TutorialSave tutorialSave2 = new TutorialSave();
			tutorialSave2.m_DaysSinceRepeat = 0;
			tutorialSave2.m_Repetitions = 0;
			tutorialSave2.m_TutorialSaveSubject = (int)m_TutorialList[j].m_TutorialSubject;
			if (m_PlayerSaves[allPlayers[playerIndex].m_Gamer.m_RewiredPlayer.id].ContainsKey(m_TutorialList[j].m_TutorialSubject))
			{
				m_PlayerSaves[allPlayers[playerIndex].m_Gamer.m_RewiredPlayer.id][m_TutorialList[j].m_TutorialSubject] = tutorialSave2;
			}
			else
			{
				m_PlayerSaves[allPlayers[playerIndex].m_Gamer.m_RewiredPlayer.id].Add(m_TutorialList[j].m_TutorialSubject, tutorialSave2);
			}
		}
	}

	public bool CheckTutorialNeeded(Player player, TutorialSubject subject)
	{
		if (player != null && player.m_Gamer != null && player.m_Gamer.m_RewiredPlayer != null && player.bDisplayTutorials)
		{
			if (!player.m_Gamer.IsLocal())
			{
				return true;
			}
			TutorialData tutorialData = GetTutorialData(subject);
			Dictionary<TutorialSubject, TutorialSave> dictionary = m_PlayerSaves[player.m_Gamer.m_RewiredPlayer.id];
			if (dictionary == null)
			{
				return false;
			}
			if (tutorialData == null)
			{
				return false;
			}
			TutorialSave value = null;
			if (!dictionary.TryGetValue(tutorialData.m_TutorialSubject, out value))
			{
				TutorialSave tutorialSave = new TutorialSave();
				tutorialSave.m_TutorialSaveSubject = (int)subject;
				dictionary.Add(subject, tutorialSave);
				value = tutorialSave;
			}
			if (tutorialData != null && tutorialData.m_TutorialTree != null && value != null)
			{
				RoutineManager instance = RoutineManager.GetInstance();
				if (instance != null && instance.GetDaysElapsed() >= tutorialData.m_StartAfterDay && !value.m_bCurrentlyActive && value.m_Repetitions <= tutorialData.m_RepetitionsRequired && (value.m_DaysSinceRepeat == 0 || value.m_DaysSinceRepeat >= tutorialData.m_DaysUntilRepeat))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool ItemFunctionalityCheck(Item item)
	{
		if (item != null && item.m_ItemData != null && item.m_ItemData.m_ItemFunctionalities != null)
		{
			for (int i = 0; i < item.m_ItemData.m_ItemFunctionalities.Count; i++)
			{
				ItemData.FunctionalityData functionalityData = item.m_ItemData.m_ItemFunctionalities[i];
				if (functionalityData != null && functionalityData.m_Functionality != null && m_ItemFunctionalitiesForUseTut != null && m_ItemFunctionalitiesForUseTut.Contains(functionalityData.m_Functionality.GetFunctionalityType()))
				{
					return true;
				}
			}
		}
		return false;
	}
}
