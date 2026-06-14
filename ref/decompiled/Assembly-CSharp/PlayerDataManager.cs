using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour, IControlledUpdate, IDeserializable, Saveable
{
	[Serializable]
	public class PlayerSpecificInfo
	{
		public Color colour = Color.white;

		public Sprite mapIcon;

		public Sprite homeIcon;

		public GameObject tag;

		public string tagPrefabName;
	}

	[Serializable]
	public class NetSaveData
	{
		[Serializable]
		public class NetPlayerAppearance
		{
			public int playerViewID = -1;

			public string appearance = string.Empty;
		}

		public List<NetPlayerAppearance> m_SerializedData = new List<NetPlayerAppearance>();
	}

	[Serializable]
	private class SaveData_PlayerData_V1 : PrisonSnapshotIO.SnapshotData_Base
	{
		public short PG_VID;

		public int[] PG_PFD;

		public SaveData_PlayerData_V1()
		{
			m_Version = 1;
		}
	}

	public PlayerSpecificInfo[] m_PlayerSpecificInfo = new PlayerSpecificInfo[4];

	private List<Player> m_Players = new List<Player>();

	private Dictionary<int, string> m_PlayerAppearances = new Dictionary<int, string>();

	private int m_PrimaryPlayerID = -1;

	private int[] m_PrimaryPlayerFightData;

	private T17NetView m_NetView;

	private NetSaveData m_NetSaveData = new NetSaveData();

	private bool m_IsSerializing;

	private bool m_ShouldReserialize;

	private SaveDataRegister m_SaveData;

	private static PlayerDataManager m_Instance;

	public static PlayerDataManager GetInstance()
	{
		return m_Instance;
	}

	private void Awake()
	{
		m_Instance = this;
	}

	public void Initialise()
	{
		m_NetView = GetComponent<T17NetView>();
		m_SaveData = new SaveDataRegister(this, 19090, bIsMajorManagerComponent: true, 9);
		if (UpdateManager.GetInstance() != null)
		{
			UpdateManager.GetInstance().Register(this, UpdateCategory.SlowPeriodic);
		}
		if (LevelScript.GetInstance().m_PreBuildSwapPrefabRefs)
		{
			for (int i = 0; i < m_PlayerSpecificInfo.Length; i++)
			{
				m_PlayerSpecificInfo[i].tag = Resources.Load(m_PlayerSpecificInfo[i].tagPrefabName) as GameObject;
			}
		}
	}

	protected virtual void OnDestroy()
	{
		LevelScript instance = LevelScript.GetInstance();
		if (instance != null && instance.m_PreBuildSwapPrefabRefs)
		{
			for (int i = 0; i < m_PlayerSpecificInfo.Length; i++)
			{
				if (m_PlayerSpecificInfo[i] != null)
				{
					m_PlayerSpecificInfo[i].tag = null;
				}
			}
		}
		m_Instance = null;
		UpdateManager instance2 = UpdateManager.GetInstance();
		if (instance2 != null)
		{
			instance2.Unregister(this, UpdateCategory.SlowPeriodic);
		}
		if (m_SaveData != null)
		{
			m_SaveData.Dispose();
		}
		m_NetView = null;
	}

	public void ControlledUpdate()
	{
		if (m_ShouldReserialize && !m_IsSerializing)
		{
			if (T17NetManager.IsMasterClient)
			{
				UpdateNetPrisonViewData();
			}
			m_ShouldReserialize = false;
		}
	}

	public void ControlledFixedUpdate()
	{
	}

	public void ControlledLateUpdate()
	{
	}

	public void ControlledPreFixedUpdate()
	{
	}

	public void ControlledPreUpdate()
	{
	}

	public bool RequiresControlledUpdate()
	{
		return true;
	}

	public bool RequiresControlledFixedUpdate()
	{
		return false;
	}

	public bool RequiresControlledLateUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreFixedUpdate()
	{
		return false;
	}

	public void RegisterPlayer(Player player)
	{
		m_Players.Add(player);
	}

	public void UnregisterPlayer(Player player)
	{
		m_Players.Remove(player);
	}

	public void RequestSetPlayerAppearanceRPC(Player player, Customisation appearance)
	{
		Platform.GetInstance().IsUGCRestrictedRequest(delegate(bool isRestricted, Platform.OnlineAccessErrorCode returnCode, bool failureHandledPlatformside)
		{
			if (isRestricted)
			{
				appearance.bUseSafeName = true;
			}
			string text = CustomisationSerialiser.SerializeCustomisation_ToJSON(appearance);
			if (m_NetView != null && !string.IsNullOrEmpty(text))
			{
				m_NetView.RPC("RPC_RequestSetPlayerAppearance", NetTargets.MasterClient, player.m_NetView.viewID, text);
			}
		});
	}

	[PunRPC]
	protected void RPC_RequestSetPlayerAppearance(int playerViewID, string newAppearance, PhotonMessageInfo info)
	{
		if (!m_PlayerAppearances.ContainsKey(playerViewID))
		{
			m_PlayerAppearances.Add(playerViewID, null);
		}
		m_PlayerAppearances[playerViewID] = newAppearance;
		m_ShouldReserialize = true;
		m_NetView.RPC("RPC_SetPlayerAppearance", NetTargets.All, playerViewID, newAppearance);
	}

	[PunRPC]
	protected void RPC_SetPlayerAppearance(int playerViewID, string newAppearance, PhotonMessageInfo info)
	{
		Player player = T17NetView.Find<Player>(playerViewID);
		if (!(player != null))
		{
			return;
		}
		Customisation customisation = CustomisationSerialiser.DeserializeCustomisation_FromJSON(newAppearance);
		if (customisation != null)
		{
			if (!m_PlayerAppearances.ContainsKey(playerViewID))
			{
				m_PlayerAppearances.Add(playerViewID, null);
			}
			m_PlayerAppearances[playerViewID] = newAppearance;
			player.m_CharacterCustomisation.SetCustomisation(customisation);
		}
	}

	public int GetPrimaryPlayerID()
	{
		return m_PrimaryPlayerID;
	}

	public int[] GetPrimaryPlayerFightData()
	{
		return m_PrimaryPlayerFightData;
	}

	public PlayerSpecificInfo GetPlayerSpecificStuff(int playerNumber)
	{
		if (playerNumber >= 0 && playerNumber < m_PlayerSpecificInfo.Length)
		{
			return m_PlayerSpecificInfo[playerNumber];
		}
		return null;
	}

	private void UpdateNetPrisonViewData()
	{
		string playerAppearanceData = Serialize();
		if (NetPrisonViewDetails.Instance != null)
		{
			NetPrisonViewDetails.Instance.PlayerAppearanceData = playerAppearanceData;
		}
	}

	private string Serialize()
	{
		m_IsSerializing = true;
		m_NetSaveData.m_SerializedData.Clear();
		for (int i = 0; i < m_Players.Count; i++)
		{
			int viewID = m_Players[i].m_NetView.viewID;
			if (m_PlayerAppearances.ContainsKey(viewID))
			{
				NetSaveData.NetPlayerAppearance netPlayerAppearance = new NetSaveData.NetPlayerAppearance();
				netPlayerAppearance.playerViewID = viewID;
				netPlayerAppearance.appearance = m_PlayerAppearances[viewID];
				m_NetSaveData.m_SerializedData.Add(netPlayerAppearance);
			}
		}
		m_IsSerializing = false;
		return JsonUtility.ToJson(m_NetSaveData);
	}

	public bool Deserialize(string data, ref string error)
	{
		if (string.IsNullOrEmpty(data))
		{
			return true;
		}
		m_IsSerializing = true;
		NetSaveData netSaveData = null;
		try
		{
			netSaveData = JsonUtility.FromJson<NetSaveData>(data);
		}
		catch
		{
			error += "PlayerDataManager: JSON Data is currupt.";
			return false;
		}
		bool result = true;
		if (netSaveData != null && netSaveData.m_SerializedData != null)
		{
			for (int i = 0; i < netSaveData.m_SerializedData.Count; i++)
			{
				int playerViewID = netSaveData.m_SerializedData[i].playerViewID;
				string appearance = netSaveData.m_SerializedData[i].appearance;
				if (!m_PlayerAppearances.ContainsKey(playerViewID))
				{
					m_PlayerAppearances.Add(playerViewID, null);
				}
				m_PlayerAppearances[playerViewID] = appearance;
				Player player = T17NetView.Find<Player>(playerViewID);
				Customisation customisation = CustomisationSerialiser.DeserializeCustomisation_FromJSON(appearance);
				if (player != null && customisation != null)
				{
					player.m_CharacterCustomisation.SetCustomisation(customisation);
				}
				else
				{
					result = false;
				}
			}
		}
		m_IsSerializing = false;
		return result;
	}

	public string GetSerializationData()
	{
		return NetPrisonViewDetails.Instance.PlayerAppearanceData;
	}

	public string CreateSnapshot()
	{
		SaveData_PlayerData_V1 saveData_PlayerData_V = new SaveData_PlayerData_V1();
		saveData_PlayerData_V.PG_VID = -1;
		for (int i = 0; i < m_Players.Count; i++)
		{
			Gamer gamer = m_Players[i].m_Gamer;
			if (gamer != null && gamer.IsLocal() && gamer.m_bPrimaryLocal)
			{
				saveData_PlayerData_V.PG_VID = (short)m_Players[i].m_NetView.viewID;
				saveData_PlayerData_V.PG_PFD = m_Players[i].m_PlayerFightSaveData.m_KnockedOutInmateViewIDs.ToArray();
				break;
			}
		}
		return JsonUtility.ToJson(saveData_PlayerData_V);
	}

	public void StartedFromSnapshot()
	{
		if (m_SaveData == null || string.IsNullOrEmpty(m_SaveData.GetSaveData()))
		{
			return;
		}
		SaveData_PlayerData_V1 saveData_PlayerData_V = null;
		try
		{
			saveData_PlayerData_V = JsonUtility.FromJson<SaveData_PlayerData_V1>(m_SaveData.GetSaveData());
		}
		catch
		{
		}
		if (saveData_PlayerData_V != null && saveData_PlayerData_V.m_Version == 1)
		{
			int pG_VID = saveData_PlayerData_V.PG_VID;
			if (pG_VID != -1)
			{
				m_PrimaryPlayerID = pG_VID;
			}
			int[] pG_PFD = saveData_PlayerData_V.PG_PFD;
			if (pG_PFD != null && pG_PFD.Length > 0)
			{
				m_PrimaryPlayerFightData = pG_PFD;
			}
		}
	}
}
