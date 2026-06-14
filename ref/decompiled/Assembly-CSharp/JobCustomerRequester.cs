using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NetworkLoadable;
using UnityEngine;

[RequireComponent(typeof(T17NetView))]
public class JobCustomerRequester : T17MonoBehaviour, IDeserializable, Saveable, INetworkLoadable
{
	public class CustomerAstheticInfo
	{
		public VisitorSetup setupInfo;

		public UnityEngine.Random.State seed = default(UnityEngine.Random.State);

		public Customisation appearance = new Customisation();
	}

	[Serializable]
	public class NetSaveData
	{
		[Serializable]
		public class NetCustomerAstheticSaveData
		{
			public int viewID = -1;

			public int setupIndex = -1;

			public UnityEngine.Random.State seed = default(UnityEngine.Random.State);
		}

		public List<int> m_AvailableCustomerIds = new List<int>();

		public List<int> m_TakenCustomerIds = new List<int>();

		public List<NetCustomerAstheticSaveData> m_CustomerAstheticData = new List<NetCustomerAstheticSaveData>();

		public int m_NextCustomerAstheticIndex;
	}

	[Serializable]
	private class SaveData_JobCustomerRequester_V1 : PrisonSnapshotIO.SnapshotData_Base
	{
		public NetSaveData m_SaveData;

		public SaveData_JobCustomerRequester_V1()
		{
			m_Version = 1;
		}
	}

	private static JobCustomerRequester s_Instance;

	public Transform m_CustomerWaitingPoint;

	public List<AICharacter_JobCustomer> m_AvailableCustomers = new List<AICharacter_JobCustomer>();

	public List<VisitorSetup> m_CustomerAstehticSetups = new List<VisitorSetup>();

	private List<AICharacter_JobCustomer> m_TakenCustomers = new List<AICharacter_JobCustomer>();

	private T17NetView m_NetView;

	private Dictionary<int, CustomerAstheticInfo> m_CustomerAstheticInfo = new Dictionary<int, CustomerAstheticInfo>();

	private List<int> m_ShuffledCustomerAstheticIndexes = new List<int>();

	private int m_NextCustomerAstheticIndex;

	private NetSaveData m_NetSaveData = new NetSaveData();

	private SaveDataRegister m_SaveData;

	private string m_LoadError = string.Empty;

	private LOADSTATE m_LoadState;

	public static JobCustomerRequester GetInstance()
	{
		return s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		if (s_Instance == null)
		{
			s_Instance = this;
		}
		m_NetView = GetComponent<T17NetView>();
		if (m_CustomerWaitingPoint == null)
		{
		}
		if (m_CustomerWaitingPoint == null && m_AvailableCustomers.Count > 0)
		{
			GameObject gameObject = new GameObject("Customer Waiting Point");
			gameObject.transform.position = m_AvailableCustomers[0].m_Transform.position;
			m_CustomerWaitingPoint = gameObject.transform;
		}
	}

	private void Start()
	{
		m_SaveData = new SaveDataRegister(this, m_NetView.viewID, bIsMajorManagerComponent: true, 17);
		NetLoadManagerSync.m_AllNetworkLoadables.Add(this);
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		for (int num = m_AvailableCustomers.Count - 1; num >= 0; num--)
		{
			m_AvailableCustomers[num].SetWaitingPoint(m_CustomerWaitingPoint.gameObject);
		}
		for (int i = 0; i < m_CustomerAstehticSetups.Count; i++)
		{
			m_ShuffledCustomerAstheticIndexes.Add(i);
		}
		RoutineManager instance = RoutineManager.GetInstance();
		if (instance != null)
		{
			instance.OnRoutineChanged += OnRoutineChanged;
		}
		return base.StartInit();
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
		RoutineManager instance = RoutineManager.GetInstance();
		if (instance != null)
		{
			instance.OnRoutineChanged -= OnRoutineChanged;
		}
		m_NetView = null;
	}

	private void OnRoutineChanged(RoutinesData.Routine oldRoutine, RoutinesData.Routine newRoutine, bool forceEnd)
	{
		if (newRoutine.m_BaseRoutineType == Routines.JobTime)
		{
			m_ShuffledCustomerAstheticIndexes.Shuffle();
		}
	}

	public void RegisterCustomerWithSystem(AICharacter_JobCustomer customer)
	{
		if (!m_AvailableCustomers.Contains(customer) && !m_TakenCustomers.Contains(customer))
		{
			m_AvailableCustomers.Add(customer);
			customer.SetWaitingPoint(m_CustomerWaitingPoint.gameObject);
		}
	}

	public int GetAllCustomersOfType(AICharacter_JobCustomer.PatronTypes type, ref List<AICharacter_JobCustomer> outCustomersList)
	{
		int num = 0;
		outCustomersList.Clear();
		for (int num2 = m_AvailableCustomers.Count - 1; num2 >= 0; num2--)
		{
			AICharacter_JobCustomer aICharacter_JobCustomer = m_AvailableCustomers[num2];
			if (aICharacter_JobCustomer.m_PatronType == type)
			{
				outCustomersList.Add(aICharacter_JobCustomer);
				num++;
			}
		}
		for (int num3 = m_TakenCustomers.Count - 1; num3 >= 0; num3--)
		{
			AICharacter_JobCustomer aICharacter_JobCustomer2 = m_TakenCustomers[num3];
			if (aICharacter_JobCustomer2.m_PatronType == type)
			{
				outCustomersList.Add(aICharacter_JobCustomer2);
				num++;
			}
		}
		return num;
	}

	public AICharacter_JobCustomer TakeAvailableCustomerRPC(AICharacter_JobCustomer.PatronTypes customerType, bool wantsRandomCustomisation)
	{
		int i = 0;
		for (int count = m_AvailableCustomers.Count; i < count; i++)
		{
			AICharacter_JobCustomer aICharacter_JobCustomer = m_AvailableCustomers[i];
			if (!(aICharacter_JobCustomer == null) && aICharacter_JobCustomer.m_PatronType == customerType)
			{
				MarkCustomerAsTakenRPC(aICharacter_JobCustomer, wantsRandomCustomisation);
				return aICharacter_JobCustomer;
			}
		}
		return null;
	}

	public void MarkCustomerAsTakenRPC(AICharacter_JobCustomer character, bool wantsRandomCustomisation)
	{
		m_NetView.RPC("RPC_ALL_MarkCustomerAsTaken", NetTargets.All, character.m_NetView.viewID, true);
		if (wantsRandomCustomisation)
		{
			SetupNextCustomerCustomisationRPC(character.m_Character);
		}
	}

	public void MarkCustomerAsFreeRPC(AICharacter_JobCustomer character)
	{
		m_NetView.RPC("RPC_ALL_MarkCustomerAsTaken", NetTargets.All, character.m_NetView.viewID, false);
	}

	[PunRPC]
	private void RPC_ALL_MarkCustomerAsTaken(int characterViewId, bool isTaken)
	{
		AICharacter_JobCustomer aICharacter_JobCustomer = T17NetView.Find<AICharacter_JobCustomer>(characterViewId);
		if (aICharacter_JobCustomer != null)
		{
			Local_MarkCustomerAsTaken(isTaken, aICharacter_JobCustomer);
		}
	}

	private void Local_MarkCustomerAsTaken(bool isTaken, AICharacter_JobCustomer customer)
	{
		if (isTaken)
		{
			if (!m_AvailableCustomers.Remove(customer))
			{
			}
			if (!m_TakenCustomers.Contains(customer))
			{
				m_TakenCustomers.Add(customer);
			}
		}
		else
		{
			if (!m_TakenCustomers.Remove(customer))
			{
			}
			if (!m_AvailableCustomers.Contains(customer))
			{
				m_AvailableCustomers.Add(customer);
			}
		}
	}

	public bool SetupNextCustomerCustomisationRPC(Character character)
	{
		if (character == null || m_ShuffledCustomerAstheticIndexes.Count <= 0)
		{
			return false;
		}
		int num = m_ShuffledCustomerAstheticIndexes[m_NextCustomerAstheticIndex];
		m_NextCustomerAstheticIndex++;
		if (m_NextCustomerAstheticIndex >= m_ShuffledCustomerAstheticIndexes.Count)
		{
			m_NextCustomerAstheticIndex = 0;
		}
		UnityEngine.Random.State state = UnityEngine.Random.state;
		if (m_NetView != null)
		{
			m_NetView.RPC("RPC_SetupNextCustomerAsthetic", NetTargets.All, character.m_NetView.viewID, num, JsonUtility.ToJson(state));
		}
		return true;
	}

	[PunRPC]
	private void RPC_SetupNextCustomerAsthetic(int viewID, int setupIndex, string seed)
	{
		Character character = T17NetView.Find<Character>(viewID);
		VisitorSetup visitorSetup = null;
		if (setupIndex >= 0 && setupIndex < m_CustomerAstehticSetups.Count)
		{
			visitorSetup = m_CustomerAstehticSetups[setupIndex];
		}
		if (character != null && visitorSetup != null && !string.IsNullOrEmpty(seed))
		{
			RandomiseCustomer(character, visitorSetup, JsonUtility.FromJson<UnityEngine.Random.State>(seed));
		}
		if (!T17NetManager.IsMasterClient && character != null)
		{
			CustomerAstheticInfo infoForCustomer = GetInfoForCustomer(character);
			if (infoForCustomer != null)
			{
				character.m_CharacterCustomisation.SetCustomisation(infoForCustomer.appearance);
				character.m_CharacterCustomisation.SetOutfit(infoForCustomer.appearance.defaultOutfit);
			}
		}
	}

	private void RandomiseCustomer(Character character, VisitorSetup setup, UnityEngine.Random.State seed)
	{
		CustomerAstheticInfo value = null;
		int viewID = character.m_NetView.viewID;
		if (!m_CustomerAstheticInfo.TryGetValue(viewID, out value))
		{
			value = new CustomerAstheticInfo();
			m_CustomerAstheticInfo.Add(viewID, value);
		}
		value.setupInfo = setup;
		value.seed = seed;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.state = value.seed;
		if (setup.m_CustomisationPool != null)
		{
			CustomisationManager.RandomiseFromPool(ref value.appearance, setup.m_CustomisationPool);
		}
		else
		{
			value.appearance = new Customisation(setup.m_CharacterCustomisation);
		}
		UnityEngine.Random.state = state;
	}

	public CustomerAstheticInfo GetInfoForCustomer(Character character)
	{
		CustomerAstheticInfo result = null;
		int viewID = character.m_NetView.viewID;
		if (m_CustomerAstheticInfo.ContainsKey(viewID))
		{
			result = m_CustomerAstheticInfo[viewID];
		}
		return result;
	}

	private void Serialise()
	{
		m_NetSaveData.m_AvailableCustomerIds.Clear();
		m_NetSaveData.m_TakenCustomerIds.Clear();
		int count = m_AvailableCustomers.Count;
		for (int i = 0; i < count; i++)
		{
			m_NetSaveData.m_AvailableCustomerIds.Add(m_AvailableCustomers[i].m_NetView.viewID);
		}
		count = m_TakenCustomers.Count;
		for (int j = 0; j < count; j++)
		{
			m_NetSaveData.m_TakenCustomerIds.Add(m_TakenCustomers[j].m_NetView.viewID);
		}
		m_NetSaveData.m_CustomerAstheticData.Clear();
		foreach (int key in m_CustomerAstheticInfo.Keys)
		{
			CustomerAstheticInfo customerAstheticInfo = m_CustomerAstheticInfo[key];
			NetSaveData.NetCustomerAstheticSaveData netCustomerAstheticSaveData = new NetSaveData.NetCustomerAstheticSaveData();
			netCustomerAstheticSaveData.viewID = key;
			netCustomerAstheticSaveData.setupIndex = m_CustomerAstehticSetups.IndexOf(customerAstheticInfo.setupInfo);
			netCustomerAstheticSaveData.seed = customerAstheticInfo.seed;
			m_NetSaveData.m_CustomerAstheticData.Add(netCustomerAstheticSaveData);
		}
	}

	public bool DeserialiseNetSaveData(NetSaveData saveData, ref string error)
	{
		if (saveData != null)
		{
			m_AvailableCustomers.Clear();
			m_TakenCustomers.Clear();
			int count = saveData.m_AvailableCustomerIds.Count;
			for (int i = 0; i < count; i++)
			{
				AICharacter_JobCustomer aICharacter_JobCustomer = T17NetView.Find<AICharacter_JobCustomer>(saveData.m_AvailableCustomerIds[i]);
				if (aICharacter_JobCustomer != null)
				{
					m_AvailableCustomers.Add(aICharacter_JobCustomer);
				}
			}
			count = saveData.m_TakenCustomerIds.Count;
			for (int j = 0; j < count; j++)
			{
				AICharacter_JobCustomer aICharacter_JobCustomer2 = T17NetView.Find<AICharacter_JobCustomer>(saveData.m_TakenCustomerIds[j]);
				if (aICharacter_JobCustomer2 != null)
				{
					m_TakenCustomers.Add(aICharacter_JobCustomer2);
				}
			}
			m_CustomerAstheticInfo.Clear();
			for (int k = 0; k < saveData.m_CustomerAstheticData.Count; k++)
			{
				NetSaveData.NetCustomerAstheticSaveData netCustomerAstheticSaveData = saveData.m_CustomerAstheticData[k];
				if (netCustomerAstheticSaveData == null)
				{
					continue;
				}
				Character character = T17NetView.Find<Character>(netCustomerAstheticSaveData.viewID);
				VisitorSetup visitorSetup = null;
				if (netCustomerAstheticSaveData.setupIndex >= 0 && netCustomerAstheticSaveData.setupIndex < m_CustomerAstehticSetups.Count)
				{
					visitorSetup = m_CustomerAstehticSetups[netCustomerAstheticSaveData.setupIndex];
				}
				if (character != null && visitorSetup != null)
				{
					RandomiseCustomer(character, visitorSetup, netCustomerAstheticSaveData.seed);
					CustomerAstheticInfo infoForCustomer = GetInfoForCustomer(character);
					if (infoForCustomer != null)
					{
						character.m_CharacterCustomisation.SetCustomisation(infoForCustomer.appearance);
						character.m_CharacterCustomisation.SetOutfit(infoForCustomer.appearance.defaultOutfit);
					}
				}
			}
			return true;
		}
		return false;
	}

	public string GetSerializationData()
	{
		return m_SaveData.GetSaveData();
	}

	public bool Deserialize(string data, ref string error)
	{
		if (string.IsNullOrEmpty(data))
		{
			return true;
		}
		m_NetSaveData = null;
		try
		{
			m_NetSaveData = JsonUtility.FromJson<NetSaveData>(data);
		}
		catch
		{
			error += "JobCustomerRequester: JSON Data is currupt.";
			return false;
		}
		return DeserialiseNetSaveData(m_NetSaveData, ref error);
	}

	public string CreateSnapshot()
	{
		Serialise();
		SaveData_JobCustomerRequester_V1 saveData_JobCustomerRequester_V = new SaveData_JobCustomerRequester_V1();
		saveData_JobCustomerRequester_V.m_SaveData = m_NetSaveData;
		return JsonUtility.ToJson(saveData_JobCustomerRequester_V);
	}

	public void StartedFromSnapshot()
	{
		if (m_SaveData == null || string.IsNullOrEmpty(m_SaveData.GetSaveData()))
		{
			return;
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
			SaveData_JobCustomerRequester_V1 saveData_JobCustomerRequester_V = null;
			try
			{
				saveData_JobCustomerRequester_V = JsonUtility.FromJson<SaveData_JobCustomerRequester_V1>(m_SaveData.GetSaveData());
			}
			catch
			{
			}
			if (saveData_JobCustomerRequester_V != null)
			{
				string error = null;
				DeserialiseNetSaveData(saveData_JobCustomerRequester_V.m_SaveData, ref error);
			}
		}
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
			Serialise();
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using MemoryStream memoryStream = new MemoryStream();
			binaryFormatter.Serialize(memoryStream, m_NetSaveData);
			m_NetView.RPC("RPC_RequestStateResponse_Yes_JobCustomerRequester", player, memoryStream.ToArray());
			return;
		}
		m_NetView.RPC("RPC_RequestStateResponse_No_JobCustomerRequester", player);
	}

	[PunRPC]
	private void RPC_RequestStateResponse_Yes_JobCustomerRequester(byte[] customerData, PhotonMessageInfo info)
	{
		string error = string.Empty;
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using (MemoryStream serializationStream = new MemoryStream(customerData))
		{
			m_NetSaveData = (NetSaveData)binaryFormatter.Deserialize(serializationStream);
		}
		if (DeserialiseNetSaveData(m_NetSaveData, ref error))
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
	private void RPC_RequestStateResponse_No_JobCustomerRequester(PhotonMessageInfo info)
	{
		m_LoadError = "JobCustomerRequester RPC_RequestStateResponce_No - Error on MasterClient";
		m_LoadState = LOADSTATE.Finished_Error;
	}
}
