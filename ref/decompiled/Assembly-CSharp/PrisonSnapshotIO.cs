using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;

public class PrisonSnapshotIO : T17MonoBehaviour
{
	public enum ManagerSaveSecondaryIds
	{
		RoutineManager = 1,
		ItemManager,
		PrisonCustomisationManager,
		FloorManager,
		RoomManager,
		JobsManager,
		VendorManager,
		ScoreManager,
		PlayerDataManager,
		PrisonAlertnessManager,
		QuestManager,
		ObjectiveManager,
		NPCManager,
		VisitorManager,
		SolitaryManager,
		GuardTowerManager,
		JobCustomerRequester
	}

	[Serializable]
	public class SnapshotData_Base
	{
		public int m_Version = -1;
	}

	[Serializable]
	public class SnapshotData_SaveGame_Base : SnapshotData_Base
	{
		public int m_iDaysInPrison;

		public string m_strDateSaved = "Never";

		public int m_iDataVersion = -1;

		public int m_GameRoomType;

		public int m_StartingGameRoomType = -1;

		public bool m_bCanPostOnSingleplayerLeaderboard = true;

		public long m_iDateAsLong;
	}

	[Serializable]
	public class SnapshotData_SaveGame_V2 : SnapshotData_SaveGame_Base
	{
		public string m_RoomPassword = string.Empty;
	}

	[Serializable]
	private class SnapshotDataTest_V1 : SnapshotData_SaveGame_Base
	{
		public List<int> m_GameObjectKeyList = new List<int>();

		public List<int> m_GameObjectKeyListSecondary = new List<int>();

		public List<string> m_GameObjectValueList = new List<string>();

		public List<string> m_BoolKeyList = new List<string>();

		public List<bool> m_BoolValueList = new List<bool>();

		public List<string> m_IntKeyList = new List<string>();

		public List<int> m_IntValueList = new List<int>();

		public List<string> m_FloatKeyList = new List<string>();

		public List<float> m_FloatValueList = new List<float>();

		public List<string> m_StringKeyList = new List<string>();

		public List<string> m_StringValueList = new List<string>();

		public List<byte> m_PrisonLevel = new List<byte>();

		public SnapshotDataTest_V1()
		{
			m_Version = 1;
		}
	}

	[Serializable]
	private class SnapshotDataTest_V2 : SnapshotDataTest_V1
	{
		public string m_HostKeyHash = string.Empty;

		public SnapshotDataTest_V2()
		{
			m_Version = 2;
		}
	}

	private class ObjectsCustomData
	{
		public int m_iPhotonID;

		public int m_SecondaryId;

		public string m_strSerializedData;
	}

	private static int c_iDataVersionNumber = 15;

	private static T17NetRoomGameView.CustomProperty[] m_DontSave = new T17NetRoomGameView.CustomProperty[2]
	{
		T17NetRoomGameView.CustomProperty.GameState,
		T17NetRoomGameView.CustomProperty.PlayerItemTrackingData
	};

	private static List<SaveData> m_SaveData = new List<SaveData>();

	[SerializeField]
	private static List<ObjectsCustomData> m_GameObjectData = new List<ObjectsCustomData>();

	private static int m_iLastGoodEntry = -1;

	private static T17NetRoomGameView.GameRoomType m_CurrentPrisonOriginalMode = T17NetRoomGameView.GameRoomType.Undefined;

	private static bool m_bCurrentPrisonAllowedToPostSPLeaderboard = true;

	private static string m_HostKeyHash = string.Empty;

	public static string RegisterSaveData(SaveData saveData, int iID, bool bIsMajorManagerComponent, int secondaryId = -1)
	{
		m_SaveData.Add(saveData);
		for (int i = 0; i <= m_iLastGoodEntry; i++)
		{
			if (m_GameObjectData[i].m_iPhotonID == iID && m_GameObjectData[i].m_SecondaryId == secondaryId)
			{
				string strSerializedData = m_GameObjectData[i].m_strSerializedData;
				if (i != m_iLastGoodEntry)
				{
					ObjectsCustomData value = m_GameObjectData[i];
					ObjectsCustomData value2 = m_GameObjectData[m_iLastGoodEntry];
					m_GameObjectData[m_iLastGoodEntry] = value;
					m_GameObjectData[i] = value2;
				}
				m_iLastGoodEntry--;
				return strSerializedData;
			}
		}
		if (bIsMajorManagerComponent && secondaryId != -1)
		{
			for (int j = 0; j <= m_iLastGoodEntry; j++)
			{
				if (m_GameObjectData[j].m_SecondaryId == secondaryId)
				{
					return m_GameObjectData[j].m_strSerializedData;
				}
			}
		}
		return string.Empty;
	}

	public static void DeRegisterSaveData(int iID, int secondaryId = -1)
	{
		if (iID == -1)
		{
			return;
		}
		int count = m_SaveData.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_SaveData[i].GetPrimaryID() == iID && m_SaveData[i].GetSecondaryID() == secondaryId)
			{
				m_SaveData.RemoveAt(i);
				break;
			}
		}
	}

	public static int GetDataVersion()
	{
		return c_iDataVersionNumber;
	}

	public static void NotifySnapshotOfStart()
	{
		int count = m_SaveData.Count;
		for (int i = 0; i < count; i++)
		{
			m_SaveData[i].GetSaveable().StartedFromSnapshot();
		}
	}

	public static string CreatePrisonSnapshot()
	{
		int num = m_DontSave.Length;
		string[] array = new string[num];
		Type typeFromHandle = typeof(string);
		Type typeFromHandle2 = typeof(int);
		Type typeFromHandle3 = typeof(float);
		Type typeFromHandle4 = typeof(bool);
		SnapshotDataTest_V2 snapshotDataTest_V = new SnapshotDataTest_V2();
		string outValue = string.Empty;
		T17NetRoomGameView.GetCustomPropertyAsString(T17NetRoomGameView.CustomProperty.HostKey, ref outValue);
		m_HostKeyHash = outValue;
		snapshotDataTest_V.m_HostKeyHash = outValue;
		if (SaveManager.GetInstance() != null)
		{
			snapshotDataTest_V.m_strDateSaved = SaveManager.GetInstance().GetTodaysDateAndTime();
		}
		else
		{
			snapshotDataTest_V.m_strDateSaved = DateTime.Now.ToString("d/M/yy H:mm");
		}
		snapshotDataTest_V.m_iDateAsLong = DateTime.Now.ToFileTime();
		if (RoutineManager.GetInstance() != null)
		{
			snapshotDataTest_V.m_iDaysInPrison = RoutineManager.GetInstance().GetDaysElapsed();
		}
		else
		{
			snapshotDataTest_V.m_iDaysInPrison = 0;
		}
		snapshotDataTest_V.m_iDataVersion = GetDataVersion();
		T17NetRoomGameView.GameRoomType outValue2 = T17NetRoomGameView.GameRoomType.Undefined;
		if (T17NetRoomGameView.GetCustomPropertyAsEnum(T17NetRoomGameView.CustomProperty.RoomType, ref outValue2))
		{
			snapshotDataTest_V.m_GameRoomType = (int)outValue2;
			if (m_CurrentPrisonOriginalMode == T17NetRoomGameView.GameRoomType.Undefined)
			{
				m_CurrentPrisonOriginalMode = outValue2;
				snapshotDataTest_V.m_StartingGameRoomType = (int)outValue2;
				if (outValue2 != 0)
				{
					m_bCurrentPrisonAllowedToPostSPLeaderboard = false;
				}
			}
			else
			{
				snapshotDataTest_V.m_StartingGameRoomType = (int)m_CurrentPrisonOriginalMode;
			}
		}
		else
		{
			snapshotDataTest_V.m_GameRoomType = 0;
		}
		snapshotDataTest_V.m_bCanPostOnSingleplayerLeaderboard = m_bCurrentPrisonAllowedToPostSPLeaderboard;
		ExitGames.Client.Photon.Hashtable customProperties = T17NetRoomGameView.Instance.GetCustomProperties();
		for (int i = 0; i < num; i++)
		{
			array[i] = m_DontSave[i].ToString();
		}
		foreach (DictionaryEntry item2 in customProperties)
		{
			if (item2.Key.GetType() != typeFromHandle)
			{
				continue;
			}
			string text = (string)item2.Key;
			for (int j = 0; j < num; j++)
			{
				if (text == array[j])
				{
					text = string.Empty;
					break;
				}
			}
			if (!string.IsNullOrEmpty(text) && item2.Value != null)
			{
				Type type = item2.Value.GetType();
				if (type == typeFromHandle)
				{
					snapshotDataTest_V.m_StringKeyList.Add(text);
					string item = (string)item2.Value;
					snapshotDataTest_V.m_StringValueList.Add(item);
				}
				else if (type == typeFromHandle2)
				{
					snapshotDataTest_V.m_IntKeyList.Add(text);
					snapshotDataTest_V.m_IntValueList.Add((int)item2.Value);
				}
				else if (type == typeFromHandle3)
				{
					snapshotDataTest_V.m_FloatKeyList.Add(text);
					snapshotDataTest_V.m_FloatValueList.Add((float)item2.Value);
				}
				else if (type == typeFromHandle4)
				{
					snapshotDataTest_V.m_BoolKeyList.Add(text);
					snapshotDataTest_V.m_BoolValueList.Add((bool)item2.Value);
				}
			}
		}
		int count = m_SaveData.Count;
		for (int k = 0; k < count; k++)
		{
			string text2 = m_SaveData[k].GetSaveable().CreateSnapshot();
			if (!string.IsNullOrEmpty(text2))
			{
				snapshotDataTest_V.m_GameObjectKeyList.Add(m_SaveData[k].GetPrimaryID());
				snapshotDataTest_V.m_GameObjectKeyListSecondary.Add(m_SaveData[k].GetSecondaryID());
				snapshotDataTest_V.m_GameObjectValueList.Add(text2);
			}
		}
		if (LevelDetailsManager.GetInstance() != null)
		{
			LevelDetailsManager.GetInstance().GetEncryptedLevelData(ref snapshotDataTest_V.m_PrisonLevel);
		}
		return JsonUtility.ToJson(snapshotDataTest_V);
	}

	public static bool RestoreSnapshot(string strJsonedSnapshot, ref List<byte> levelData)
	{
		if (m_GameObjectData.Count > 0)
		{
			m_GameObjectData.Clear();
		}
		m_iLastGoodEntry = -1;
		if (string.IsNullOrEmpty(strJsonedSnapshot))
		{
			return true;
		}
		ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
		SnapshotData_Base snapshotData_Base = null;
		try
		{
			snapshotData_Base = JsonUtility.FromJson<SnapshotData_Base>(strJsonedSnapshot);
		}
		catch
		{
			return false;
		}
		if (snapshotData_Base != null)
		{
			SnapshotDataTest_V2 snapshotDataTest_V = null;
			SnapshotDataTest_V1 snapshotDataTest_V2 = null;
			if (snapshotData_Base.m_Version == 2)
			{
				try
				{
					snapshotDataTest_V = JsonUtility.FromJson<SnapshotDataTest_V2>(strJsonedSnapshot);
					snapshotDataTest_V2 = snapshotDataTest_V;
					if (snapshotDataTest_V != null)
					{
						m_HostKeyHash = snapshotDataTest_V.m_HostKeyHash;
					}
				}
				catch
				{
					return false;
				}
			}
			if (snapshotData_Base.m_Version == 1)
			{
				try
				{
					snapshotDataTest_V2 = JsonUtility.FromJson<SnapshotDataTest_V1>(strJsonedSnapshot);
				}
				catch
				{
					return false;
				}
			}
			if (snapshotDataTest_V2 != null)
			{
				levelData.Clear();
				m_CurrentPrisonOriginalMode = (T17NetRoomGameView.GameRoomType)snapshotDataTest_V2.m_StartingGameRoomType;
				m_bCurrentPrisonAllowedToPostSPLeaderboard = snapshotDataTest_V2.m_bCanPostOnSingleplayerLeaderboard;
				m_GameObjectData = new List<ObjectsCustomData>();
				int count = snapshotDataTest_V2.m_GameObjectKeyList.Count;
				for (int i = 0; i < count; i++)
				{
					ObjectsCustomData objectsCustomData = new ObjectsCustomData();
					objectsCustomData.m_iPhotonID = snapshotDataTest_V2.m_GameObjectKeyList[i];
					objectsCustomData.m_SecondaryId = snapshotDataTest_V2.m_GameObjectKeyListSecondary[i];
					objectsCustomData.m_strSerializedData = snapshotDataTest_V2.m_GameObjectValueList[i];
					m_GameObjectData.Add(objectsCustomData);
				}
				m_iLastGoodEntry = m_GameObjectData.Count - 1;
				int count2 = snapshotDataTest_V2.m_BoolKeyList.Count;
				for (int j = 0; j < count2; j++)
				{
					hashtable.Add(snapshotDataTest_V2.m_BoolKeyList[j], snapshotDataTest_V2.m_BoolValueList[j]);
				}
				count2 = snapshotDataTest_V2.m_IntKeyList.Count;
				for (int k = 0; k < count2; k++)
				{
					hashtable.Add(snapshotDataTest_V2.m_IntKeyList[k], snapshotDataTest_V2.m_IntValueList[k]);
				}
				count2 = snapshotDataTest_V2.m_FloatKeyList.Count;
				for (int l = 0; l < count2; l++)
				{
					hashtable.Add(snapshotDataTest_V2.m_FloatKeyList[l], snapshotDataTest_V2.m_FloatValueList[l]);
				}
				count2 = snapshotDataTest_V2.m_StringKeyList.Count;
				for (int m = 0; m < count2; m++)
				{
					string value = snapshotDataTest_V2.m_StringValueList[m];
					hashtable.Add(snapshotDataTest_V2.m_StringKeyList[m], value);
				}
				T17NetRoomGameView.Instance.SetCustomProperties(hashtable);
				if (snapshotDataTest_V2.m_PrisonLevel != null)
				{
					levelData.AddRange(snapshotDataTest_V2.m_PrisonLevel);
				}
				return true;
			}
		}
		return false;
	}

	public static bool IsThereSaveData()
	{
		return m_GameObjectData.Count > 0;
	}

	public static void ResetIOData()
	{
		m_CurrentPrisonOriginalMode = T17NetRoomGameView.GameRoomType.Undefined;
		m_bCurrentPrisonAllowedToPostSPLeaderboard = true;
	}

	public static bool IsCurrentPrisonAllowedToPostToSPLeaderboard()
	{
		return m_bCurrentPrisonAllowedToPostSPLeaderboard;
	}

	public static void SetCurrentPrisonSPLeaderboardNotAllowed()
	{
		m_bCurrentPrisonAllowedToPostSPLeaderboard = false;
	}

	public static T17NetRoomGameView.GameRoomType GetCurrentPrisonOriginalGameMode()
	{
		return m_CurrentPrisonOriginalMode;
	}

	public static string GetHostKeyHash()
	{
		string outValue = string.Empty;
		T17NetRoomGameView.GetCustomPropertyAsString(T17NetRoomGameView.CustomProperty.HostKey, ref outValue);
		if (!string.IsNullOrEmpty(outValue))
		{
			return outValue;
		}
		return m_HostKeyHash;
	}
}
