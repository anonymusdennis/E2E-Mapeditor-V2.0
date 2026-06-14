using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NetworkLoadable;
using UnityEngine;

public class VendorManager : MonoBehaviour, IDeserializable, Saveable, INetworkLoadable
{
	[Serializable]
	public class WeightedItemGroup
	{
		public RandomItemGroup itemGroup;

		public int chance;
	}

	[Serializable]
	public class NetSaveData
	{
		public List<ulong> m_SerializedData = new List<ulong>();
	}

	[HideInInspector]
	public int m_MaxVendors = 6;

	[Range(0f, 2f)]
	[Header("Prison Settings")]
	public float m_ItemCostModifier = 1f;

	[Range(0f, 100f)]
	public int m_RequiredOpinion;

	[Range(0f, 12f)]
	public int m_MinItems;

	[Range(0f, 12f)]
	public int m_MaxItems;

	[Tooltip("Length of time vendors are allowed to sell their wares (in Game Minutes)")]
	[Header("Time Settings")]
	public int m_MinVendorDuration;

	public int m_MaxVendorDuration;

	[HideInInspector]
	public List<WeightedItemGroup> m_PossibleItemSets = new List<WeightedItemGroup>();

	private T17NetView m_NetView;

	private Vendor[] m_Vendors;

	private List<Character> m_PotentialVendors = new List<Character>();

	private NetSaveData m_NetSaveData = new NetSaveData();

	private SaveDataRegister m_SaveData;

	private static VendorManager m_Instance;

	private string m_LoadError = string.Empty;

	private LOADSTATE m_LoadState;

	public static VendorManager GetInstance()
	{
		return m_Instance;
	}

	private void Awake()
	{
		m_Instance = this;
		CreatePool();
	}

	private void Start()
	{
		m_NetView = GetComponent<T17NetView>();
		m_SaveData = new SaveDataRegister(this, m_NetView.viewID, bIsMajorManagerComponent: true, 7);
		NetLoadManagerSync.m_AllNetworkLoadables.Add(this);
	}

	protected virtual void OnDestroy()
	{
		if (m_SaveData != null)
		{
			m_SaveData.Dispose();
		}
		NetLoadManagerSync.m_AllNetworkLoadables.Remove(this);
		if (m_Instance == this)
		{
			m_Instance = null;
		}
		m_NetView = null;
	}

	private void CreatePool()
	{
		m_Vendors = GetComponentsInChildren<Vendor>(includeInactive: true);
	}

	public void Initialise()
	{
		if (ConfigManager.GetInstance() != null && ConfigManager.GetInstance().vendorConfig != null)
		{
			ApplyConfigData(ConfigManager.GetInstance().vendorConfig);
		}
		if (T17NetManager.IsMasterClient && !PrisonSnapshotIO.IsThereSaveData())
		{
			AssignInitialVendors();
		}
	}

	private void ApplyConfigData(VendorConfig config)
	{
		m_ItemCostModifier = config.m_ItemCostModifier;
		m_RequiredOpinion = config.m_RequiredOpinion;
		m_MinItems = config.m_MinItems;
		m_MaxItems = config.m_MaxItems;
		m_MaxVendors = config.m_MaxVendors;
		m_MaxVendorDuration = config.m_MaxVendorDuration;
		m_MinVendorDuration = config.m_MinVendorDuration;
		m_PossibleItemSets = config.m_PossibleItemSets;
	}

	public void ControlledFixedUpdate()
	{
	}

	private void AssignInitialVendors()
	{
		int num = Mathf.Min(m_Vendors.Length, m_MaxVendors);
		for (int i = 0; i < num; i++)
		{
			AssignRandomCharacter(m_Vendors[i]);
		}
	}

	private void AssignRandomCharacter(Vendor vendor)
	{
		List<Character> allPotentialVendors = GetAllPotentialVendors();
		int randomVendorDuration = GetRandomVendorDuration();
		int count = allPotentialVendors.Count;
		for (int i = 0; i < count; i++)
		{
			int index = UnityEngine.Random.Range(0, allPotentialVendors.Count);
			Character character = allPotentialVendors[index];
			if (character != null && AssignVendor(character, randomVendorDuration, vendor))
			{
				break;
			}
			allPotentialVendors.RemoveAt(index);
		}
	}

	public void RegisterPotentialVendor(Character character)
	{
		m_PotentialVendors.Add(character);
	}

	public void UnregisterPotentialVendor(Character character)
	{
		m_PotentialVendors.Remove(character);
	}

	private List<Character> GetAllPotentialVendors()
	{
		List<Character> list = new List<Character>();
		for (int i = 0; i < m_PotentialVendors.Count; i++)
		{
			Character character = m_PotentialVendors[i];
			if (QuestManager.GetInstance().IsCharacterSafeToUse(character))
			{
				list.Add(character);
			}
		}
		return list;
	}

	private int GetRandomVendorDuration()
	{
		return UnityEngine.Random.Range(m_MinVendorDuration, m_MaxVendorDuration);
	}

	public bool AssignVendor(Character character)
	{
		return AssignVendor(character, GetRandomVendorDuration());
	}

	public bool AssignVendor(Character character, int duration)
	{
		return AssignVendor(character, duration, GetFreeVendor());
	}

	public bool AssignVendor(Character character, int duration, Vendor vendor)
	{
		if (IsVendor(character))
		{
			return false;
		}
		QuestManager instance = QuestManager.GetInstance();
		if (instance != null && instance.CheckCharacterHasPrisonQuest(character))
		{
			return false;
		}
		RandomItemGroup randomItemGroup = GetRandomItemGroup();
		if (vendor == null || randomItemGroup == null)
		{
			return false;
		}
		int min = Mathf.Max(m_MinItems, 0);
		int max = Mathf.Max(m_MaxItems, m_MinItems, 0);
		int count = UnityEngine.Random.Range(min, max);
		vendor.RequestRefreshItems(randomItemGroup, count);
		vendor.RequestAssignCharacter(character, duration);
		return true;
	}

	public bool AssignQuestVendor(Character character, List<ItemData> items)
	{
		if (character == null || items == null || items.Count <= 0)
		{
			return false;
		}
		int count = items.Count;
		int[] array = new int[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = items[i].m_ItemDataID;
		}
		m_NetView.RPC("RPC_AssignQuestVendor", NetTargets.MasterClient, character.m_NetView.viewID, array);
		return true;
	}

	[PunRPC]
	private void RPC_AssignQuestVendor(int characterID, int[] itemDataIDs, PhotonMessageInfo info)
	{
		Character character = T17NetView.Find<Character>(characterID);
		ItemData[] array = new ItemData[itemDataIDs.Length];
		for (int i = 0; i < itemDataIDs.Length; i++)
		{
			array[i] = ItemManager.GetInstance().GetItemDataWithID(itemDataIDs[i]);
		}
		Vendor vendorForCharacter = GetVendorForCharacter(character, out var success);
		if (success)
		{
			vendorForCharacter.RequestRefreshItems(array);
			return;
		}
		vendorForCharacter = GetFreeVendor();
		if (!(vendorForCharacter == null))
		{
			vendorForCharacter.RequestRefreshItems(array);
			vendorForCharacter.RequestAssignCharacter(character, 0);
		}
	}

	public bool RemoveVendor(Character character)
	{
		bool success;
		Vendor vendorForCharacter = GetVendorForCharacter(character, out success);
		if (!success)
		{
			return false;
		}
		vendorForCharacter.RequestUnassignCharacter();
		return true;
	}

	private Vendor GetFreeVendor()
	{
		Vendor result = null;
		for (int i = 0; i < m_Vendors.Length; i++)
		{
			if (m_Vendors[i].GetCharacter() == null)
			{
				result = m_Vendors[i];
			}
		}
		return result;
	}

	private RandomItemGroup GetRandomItemGroup()
	{
		int num = UnityEngine.Random.Range(0, 100);
		for (int i = 0; i < m_PossibleItemSets.Count; i++)
		{
			num -= m_PossibleItemSets[i].chance;
			if (num <= 0)
			{
				return m_PossibleItemSets[i].itemGroup;
			}
		}
		return null;
	}

	public bool IsVendor(Character character)
	{
		GetVendorForCharacter(character, out var success);
		return success;
	}

	public Vendor GetVendorForCharacter(Character character, out bool success)
	{
		int num = m_Vendors.Length;
		for (int i = 0; i < num; i++)
		{
			if (m_Vendors[i].GetCharacter() == character)
			{
				success = true;
				return m_Vendors[i];
			}
		}
		success = false;
		return null;
	}

	public float GetItemCostModifier()
	{
		return m_ItemCostModifier;
	}

	public int GetRequiredOpinion()
	{
		return m_RequiredOpinion;
	}

	public void OnVendorExpired(Vendor vendor)
	{
		AssignRandomCharacter(vendor);
	}

	public void OnVendorUpdated()
	{
	}

	private string Serialize()
	{
		m_NetSaveData.m_SerializedData.Clear();
		for (int i = 0; i < m_Vendors.Length; i++)
		{
			if (!(m_Vendors[i] == null) && !(m_Vendors[i].GetCharacter() == null))
			{
				ulong item = m_Vendors[i].Serialize();
				m_NetSaveData.m_SerializedData.Add(item);
			}
		}
		return JsonUtility.ToJson(m_NetSaveData);
	}

	public string CreateSnapshot()
	{
		return Serialize();
	}

	public void StartedFromSnapshot()
	{
	}

	public string GetSerializationData()
	{
		return m_SaveData.GetSaveData();
	}

	public bool DeserializeBinary(NetSaveData vendors, ref string error)
	{
		bool result = true;
		if (vendors != null && vendors.m_SerializedData != null)
		{
			for (int i = 0; i < vendors.m_SerializedData.Count; i++)
			{
				ulong num = vendors.m_SerializedData[i];
				if (num == 0)
				{
					continue;
				}
				int num2 = Vendor.DeserializeVendorID(num);
				if (num2 < 0)
				{
					continue;
				}
				Vendor vendor = T17NetView.Find<Vendor>(num2);
				if (vendor != null)
				{
					if (!vendor.Deserialize(num, ref error))
					{
						result = false;
					}
					else
					{
						vendor.GetCharacter().ProcessPins();
					}
				}
			}
		}
		return result;
	}

	public bool Deserialize(string data, ref string error)
	{
		if (string.IsNullOrEmpty(data))
		{
			return true;
		}
		NetSaveData netSaveData = null;
		try
		{
			netSaveData = JsonUtility.FromJson<NetSaveData>(data);
		}
		catch
		{
			error += "VenderManager: JSON Data is currupt.";
			return false;
		}
		return DeserializeBinary(netSaveData, ref error);
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
		if (!T17NetManager.IsMasterClient || player.IsLocal)
		{
			return;
		}
		if (m_LoadState == LOADSTATE.Finished_OK)
		{
			Serialize();
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using MemoryStream memoryStream = new MemoryStream();
			binaryFormatter.Serialize(memoryStream, m_NetSaveData);
			m_NetView.RPC("RPC_RequestStateResponce_Yes_VendorManager", player, memoryStream.ToArray());
			return;
		}
		m_NetView.RPC("RPC_RequestStateResponce_No_VendorManager", player);
	}

	[PunRPC]
	private void RPC_RequestStateResponce_Yes_VendorManager(byte[] questData, PhotonMessageInfo info)
	{
		string error = string.Empty;
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using (MemoryStream serializationStream = new MemoryStream(questData))
		{
			m_NetSaveData = (NetSaveData)binaryFormatter.Deserialize(serializationStream);
		}
		if (DeserializeBinary(m_NetSaveData, ref error))
		{
			m_LoadState = LOADSTATE.Finished_OK;
		}
		else
		{
			m_LoadState = LOADSTATE.Finished_Error;
			m_LoadError += error;
		}
		m_LoadState = LOADSTATE.Finished_OK;
	}

	[PunRPC]
	private void RPC_RequestStateResponce_No_VendorManager(PhotonMessageInfo info)
	{
		m_LoadError = "QuestManager RPC_RequestStateResponce_No - Error on MasterClient";
		m_LoadState = LOADSTATE.Finished_Error;
	}
}
