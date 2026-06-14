using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NetworkLoadable;
using UnityEngine;

public class VisitorCustomisationManager : T17MonoBehaviour, IDeserializable, Saveable, INetworkLoadable
{
	public enum VisitorState
	{
		Invalid,
		GoToChair,
		Speak,
		GiveItem,
		Leave
	}

	public class VisitorInfo
	{
		public VisitorSetup setupInfo;

		public UnityEngine.Random.State seed = default(UnityEngine.Random.State);

		public Customisation appearance = new Customisation();

		public int giftDataID = -1;

		public VisitorState state;
	}

	[Serializable]
	public class NetSaveData
	{
		[Serializable]
		public class NetVisitorSaveData
		{
			public int viewID = -1;

			public int setupIndex = -1;

			public UnityEngine.Random.State seed = default(UnityEngine.Random.State);

			public int state = -1;
		}

		public List<NetVisitorSaveData> m_VisitorData = new List<NetVisitorSaveData>();

		public List<int> m_PlayerGiftData = new List<int>();
	}

	public List<VisitorSetup> m_VisitorSetups = new List<VisitorSetup>();

	private Dictionary<int, VisitorInfo> m_VisitorInfo = new Dictionary<int, VisitorInfo>();

	private List<Character> m_AlreadyRecievedGift = new List<Character>();

	private List<int> m_ShuffledVisitorIndexes = new List<int>();

	private int m_NextVisitorIndex;

	private T17NetView m_NetView;

	private NetSaveData m_NetSaveData = new NetSaveData();

	private SaveDataRegister m_SaveData;

	private static VisitorCustomisationManager m_TheInstance;

	private string m_LoadError = string.Empty;

	private LOADSTATE m_LoadState;

	public static VisitorCustomisationManager GetInstance()
	{
		return m_TheInstance;
	}

	protected override void Awake()
	{
		base.Awake();
		m_NetView = GetComponent<T17NetView>();
		if (m_TheInstance == null)
		{
			m_TheInstance = this;
		}
	}

	private void Start()
	{
		m_SaveData = new SaveDataRegister(this, m_NetView.viewID, bIsMajorManagerComponent: true, 14);
		NetLoadManagerSync.m_AllNetworkLoadables.Add(this);
	}

	protected virtual void OnDestroy()
	{
		RoutineManager instance = RoutineManager.GetInstance();
		if (instance != null)
		{
			instance.OnRoutineChanged -= OnFreeTime_Reset;
		}
		if (m_SaveData != null)
		{
			m_SaveData.Dispose();
		}
		NetLoadManagerSync.m_AllNetworkLoadables.Remove(this);
		if (m_TheInstance == this)
		{
			m_TheInstance = null;
		}
		m_NetView = null;
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		for (int i = 0; i < m_VisitorSetups.Count; i++)
		{
			m_ShuffledVisitorIndexes.Add(i);
		}
		RoutineManager instance = RoutineManager.GetInstance();
		if (instance != null)
		{
			instance.OnRoutineChanged += OnFreeTime_Reset;
		}
		return base.StartInit();
	}

	private void OnFreeTime_Reset(RoutinesData.Routine oldRoutine, RoutinesData.Routine newRoutine, bool forceEnd)
	{
		if (newRoutine.m_BaseRoutineType == Routines.FreeTime)
		{
			m_ShuffledVisitorIndexes.Shuffle();
			m_AlreadyRecievedGift.Clear();
		}
	}

	public bool SetupNextVisitorRPC(Character character)
	{
		if (character == null || m_ShuffledVisitorIndexes.Count <= 0)
		{
			return false;
		}
		int num = m_ShuffledVisitorIndexes[m_NextVisitorIndex];
		m_NextVisitorIndex++;
		if (m_NextVisitorIndex >= m_ShuffledVisitorIndexes.Count)
		{
			m_NextVisitorIndex = 0;
		}
		UnityEngine.Random.State state = UnityEngine.Random.state;
		if (m_NetView != null)
		{
			m_NetView.RPC("RPC_SetupNextVisitor", NetTargets.All, character.m_NetView.viewID, num, JsonUtility.ToJson(state));
		}
		return true;
	}

	[PunRPC]
	private void RPC_SetupNextVisitor(int viewID, int setupIndex, string seed)
	{
		Character character = T17NetView.Find<Character>(viewID);
		VisitorSetup visitorSetup = null;
		if (setupIndex >= 0 && setupIndex < m_VisitorSetups.Count)
		{
			visitorSetup = m_VisitorSetups[setupIndex];
		}
		if (character != null && visitorSetup != null && !string.IsNullOrEmpty(seed))
		{
			RandomiseVisitor(character, visitorSetup, JsonUtility.FromJson<UnityEngine.Random.State>(seed));
		}
		if (!T17NetManager.IsMasterClient && character != null)
		{
			VisitorInfo infoForVisitor = GetInfoForVisitor(character);
			if (infoForVisitor != null)
			{
				character.m_CharacterCustomisation.SetCustomisation(infoForVisitor.appearance);
				character.m_CharacterCustomisation.SetOutfit(infoForVisitor.appearance.defaultOutfit);
			}
		}
	}

	private void RandomiseVisitor(Character character, VisitorSetup setup, UnityEngine.Random.State seed)
	{
		VisitorInfo value = null;
		int viewID = character.m_NetView.viewID;
		if (!m_VisitorInfo.TryGetValue(viewID, out value))
		{
			value = new VisitorInfo();
			m_VisitorInfo.Add(viewID, value);
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
		ItemData randomItem = setup.m_GiftItems.GetRandomItem(bUniqueItems: true);
		value.giftDataID = randomItem.m_ItemDataID;
		UnityEngine.Random.state = state;
	}

	public VisitorInfo GetInfoForVisitor(Character character)
	{
		VisitorInfo result = null;
		int viewID = character.m_NetView.viewID;
		if (m_VisitorInfo.ContainsKey(viewID))
		{
			result = m_VisitorInfo[viewID];
		}
		return result;
	}

	public bool HasPlayerRecievedGift(Character character)
	{
		return character != null && m_AlreadyRecievedGift.Contains(character);
	}

	public void SetPlayerRecievedGiftRPC(Character character)
	{
		if (!HasPlayerRecievedGift(character) && m_NetView != null)
		{
			m_NetView.RPC("RPC_SetPlayerRecievedGift", NetTargets.All, character.m_NetView.viewID);
		}
	}

	[PunRPC]
	private void RPC_SetPlayerRecievedGift(int viewID, PhotonMessageInfo info)
	{
		Character character = T17NetView.Find<Character>(viewID);
		if (character != null && !m_AlreadyRecievedGift.Contains(character))
		{
			m_AlreadyRecievedGift.Add(character);
		}
	}

	public VisitorState GetVisitorState(Character character)
	{
		return GetInfoForVisitor(character)?.state ?? VisitorState.Invalid;
	}

	public void SetVisitorStateRPC(Character character, VisitorState state)
	{
		VisitorState visitorState = GetVisitorState(character);
		if (state != visitorState && m_NetView != null)
		{
			m_NetView.RPC("RPC_SetVisitorState", NetTargets.All, character.m_NetView.viewID, (int)state);
		}
	}

	[PunRPC]
	private void RPC_SetVisitorState(int viewID, int state, PhotonMessageInfo info)
	{
		Character character = T17NetView.Find<Character>(viewID);
		if (character != null)
		{
			VisitorInfo infoForVisitor = GetInfoForVisitor(character);
			if (infoForVisitor != null)
			{
				infoForVisitor.state = (VisitorState)state;
			}
		}
	}

	private void Serialize()
	{
		m_NetSaveData.m_VisitorData.Clear();
		foreach (int key in m_VisitorInfo.Keys)
		{
			VisitorInfo visitorInfo = m_VisitorInfo[key];
			NetSaveData.NetVisitorSaveData netVisitorSaveData = new NetSaveData.NetVisitorSaveData();
			netVisitorSaveData.viewID = key;
			netVisitorSaveData.setupIndex = m_VisitorSetups.IndexOf(visitorInfo.setupInfo);
			netVisitorSaveData.seed = visitorInfo.seed;
			netVisitorSaveData.state = (int)visitorInfo.state;
			m_NetSaveData.m_VisitorData.Add(netVisitorSaveData);
		}
		m_NetSaveData.m_PlayerGiftData.Clear();
		for (int i = 0; i < m_AlreadyRecievedGift.Count; i++)
		{
			int viewID = m_AlreadyRecievedGift[i].m_NetView.viewID;
			m_NetSaveData.m_PlayerGiftData.Add(viewID);
		}
	}

	public string CreateSnapshot()
	{
		Serialize();
		return JsonUtility.ToJson(m_NetSaveData);
	}

	public void StartedFromSnapshot()
	{
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
			error += "VisitorManager: JSON Data is currupt.";
			return false;
		}
		return DeserializeNetSaveData(m_NetSaveData, ref error);
	}

	public bool DeserializeNetSaveData(NetSaveData visitors, ref string error)
	{
		bool result = true;
		if (visitors != null && visitors.m_VisitorData != null)
		{
			m_VisitorInfo.Clear();
			for (int i = 0; i < visitors.m_VisitorData.Count; i++)
			{
				NetSaveData.NetVisitorSaveData netVisitorSaveData = visitors.m_VisitorData[i];
				if (netVisitorSaveData == null)
				{
					continue;
				}
				Character character = T17NetView.Find<Character>(netVisitorSaveData.viewID);
				VisitorSetup visitorSetup = null;
				if (netVisitorSaveData.setupIndex >= 0 && netVisitorSaveData.setupIndex < m_VisitorSetups.Count)
				{
					visitorSetup = m_VisitorSetups[netVisitorSaveData.setupIndex];
				}
				if (character != null && visitorSetup != null)
				{
					RandomiseVisitor(character, visitorSetup, netVisitorSaveData.seed);
					VisitorInfo infoForVisitor = GetInfoForVisitor(character);
					if (infoForVisitor != null)
					{
						character.m_CharacterCustomisation.SetCustomisation(infoForVisitor.appearance);
						character.m_CharacterCustomisation.SetOutfit(infoForVisitor.appearance.defaultOutfit);
						infoForVisitor.state = (VisitorState)netVisitorSaveData.state;
					}
				}
			}
			m_AlreadyRecievedGift.Clear();
			for (int j = 0; j < visitors.m_PlayerGiftData.Count; j++)
			{
				Character character2 = T17NetView.Find<Character>(visitors.m_PlayerGiftData[j]);
				if (character2 != null)
				{
					m_AlreadyRecievedGift.Add(character2);
				}
			}
		}
		return result;
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
			m_NetView.RPC("RPC_RequestStateResponse_Yes_VisitorManager", player, memoryStream.ToArray());
			return;
		}
		m_NetView.RPC("RPC_RequestStateResponse_No_VisitorManager", player);
	}

	[PunRPC]
	private void RPC_RequestStateResponse_Yes_VisitorManager(byte[] questData, PhotonMessageInfo info)
	{
		string error = string.Empty;
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using (MemoryStream serializationStream = new MemoryStream(questData))
		{
			m_NetSaveData = (NetSaveData)binaryFormatter.Deserialize(serializationStream);
		}
		if (DeserializeNetSaveData(m_NetSaveData, ref error))
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
	private void RPC_RequestStateResponse_No_VisitorManager(PhotonMessageInfo info)
	{
		m_LoadError = "VisitorManager RPC_RequestStateResponce_No - Error on MasterClient";
		m_LoadState = LOADSTATE.Finished_Error;
	}
}
