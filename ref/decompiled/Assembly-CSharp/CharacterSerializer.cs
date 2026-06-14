using System.Collections.Generic;
using BitStream;
using NetworkLoadable;
using UnityEngine;

public class CharacterSerializer : T17MonoBehaviour, IControlledUpdate, INetworkLoadable
{
	public enum CharacterSerializerListType
	{
		Players,
		High,
		Medium,
		Low,
		Force,
		COUNT
	}

	public class SerializedCharacterList
	{
		private float m_SerializeDelay;

		private float m_NextSendTime;

		private float m_KeyFrameDelay = 1f;

		private int m_SendCount;

		public FastList<Character> m_Characters = new FastList<Character>();

		private float m_TimeSpent;

		private int m_SendTotalBits;

		private bool m_bClearAfterComplete;

		private bool m_HasSerialized;

		public CharacterSerializerListType m_ListType = CharacterSerializerListType.Low;

		private int m_CurrentCharacter;

		public SerializedCharacterList(float serializeDelay, float keyFrameDelay, CharacterSerializerListType listType, bool bClearAfterComplete)
		{
			m_SerializeDelay = serializeDelay;
			m_KeyFrameDelay = keyFrameDelay;
			m_ListType = listType;
			m_bClearAfterComplete = bClearAfterComplete;
		}

		public void AddCharacter(Character character)
		{
			if (!m_Characters.Contains(character))
			{
				m_Characters.Add(character);
				character.m_SerializeRate = m_ListType;
				character.m_KeyFrameDelay = m_KeyFrameDelay;
				float num = T17NetManager.RealTime + m_KeyFrameDelay;
				if (character.m_NextKeyFrame > num)
				{
					character.m_NextKeyFrame = num;
				}
			}
			else
			{
				Debug.Log(" DOES THIS GET HIT?");
			}
		}

		public void RemoveCharacter(Character character)
		{
			m_Characters.Remove(character);
		}

		public void ClearCharacterList()
		{
			m_Characters.Clear();
		}

		public void UpdateTimeStampsIfSerializedThisFrame(float currentTime)
		{
			if (m_HasSerialized)
			{
				m_HasSerialized = false;
				m_NextSendTime = currentTime + m_SerializeDelay;
				m_CurrentCharacter = 0;
				if (m_bClearAfterComplete)
				{
					m_Characters.Clear();
				}
			}
		}

		public bool Serialize(BitStreamWriter bitWriter, float currentTime, int MTU, ref int largestSize, Character keyFrameCharacter)
		{
			bool result = true;
			if (currentTime > m_NextSendTime)
			{
				m_HasSerialized = true;
				int usedBitCount = bitWriter.GetUsedBitCount();
				for (int i = m_CurrentCharacter; i < m_Characters.Count; i++)
				{
					Character character = m_Characters[i];
					int usedBitCount2 = bitWriter.GetUsedBitCount();
					character.NetworkSerializeWrite(bitWriter, character == keyFrameCharacter);
					if (character.m_SerializeRateOverride != CharacterSerializerListType.COUNT)
					{
						character.m_SerializeRateOverride = CharacterSerializerListType.COUNT;
					}
					int num = bitWriter.GetUsedBitCount() - usedBitCount2;
					if (num > largestSize)
					{
						largestSize = num;
					}
					if (bitWriter.GetUsedBitCount() + largestSize > MTU)
					{
						result = false;
						m_CurrentCharacter = i + 1;
						break;
					}
				}
				m_SendTotalBits += bitWriter.GetUsedBitCount() - usedBitCount;
			}
			return result;
		}

		public string GetDebugString()
		{
			return m_ListType.ToString() + " Character count:" + m_Characters.Count + " Time Spent: " + m_TimeSpent + " Bits Sent: " + m_SendCount;
		}

		public void ResetStats()
		{
			m_TimeSpent = 0f;
			m_SendCount = 0;
		}

		public float GetTimeSpent()
		{
			return m_TimeSpent;
		}

		public int GetTotalBitsSent()
		{
			return m_SendTotalBits;
		}
	}

	public const int BitsForIndex = 8;

	public const int InterestGroup = 5;

	private float m_SortListsTime;

	public static float CharacterTeleportDistance = 3f;

	private int m_SerializationNumber;

	private T17NetView m_NetView;

	private FastList<Character> m_allCharacters;

	private FastList<SerializedCharacterList> m_SerializedCharacterLists = new FastList<SerializedCharacterList>();

	private bool m_bCreateCharacterLists;

	private FastList<byte> m_serializeByteList;

	private BitStreamReader m_bitReader;

	private BitStreamWriter m_bitWriter;

	private bool m_bActive;

	private static CharacterSerializer ms_instance;

	private int m_sends;

	private int m_receives;

	private int m_largestSent;

	private int m_lastSentPacketSize;

	private float m_timeBetweenSerializations;

	private float m_nextSerializeTime;

	private int m_SentTimesLength = 3;

	private float[,] m_SentTimes = new float[3, 2];

	private string m_LoadError = string.Empty;

	private LOADSTATE m_LoadState;

	protected override void Awake()
	{
		base.Awake();
		if (m_NetView == null)
		{
			m_NetView = GetComponent<T17NetView>();
		}
	}

	public static CharacterSerializer GetInstance()
	{
		return ms_instance;
	}

	public void ControlledUpdate()
	{
	}

	private void CreateSerializedCharacterLists()
	{
		m_SerializedCharacterLists.Clear();
		m_SerializedCharacterLists.Add(new SerializedCharacterList(0f, 1f, CharacterSerializerListType.Players, bClearAfterComplete: false));
		m_SerializedCharacterLists.Add(new SerializedCharacterList(0f, 2f, CharacterSerializerListType.High, bClearAfterComplete: false));
		m_SerializedCharacterLists.Add(new SerializedCharacterList(0.1f, 2f, CharacterSerializerListType.Medium, bClearAfterComplete: false));
		m_SerializedCharacterLists.Add(new SerializedCharacterList(1f, 5f, CharacterSerializerListType.Low, bClearAfterComplete: false));
		SerializedCharacterList serializedCharacterList = GetSerializedCharacterList(CharacterSerializerListType.Low);
		SerializedCharacterList serializedCharacterList2 = GetSerializedCharacterList(CharacterSerializerListType.Players);
		for (int i = 0; i < m_allCharacters.Count; i++)
		{
			Character character = m_allCharacters[i];
			if (!character.IsPlayer())
			{
				if (character.m_NetView.isMine)
				{
					serializedCharacterList.AddCharacter(character);
				}
			}
			else if (character.m_NetView.isMine)
			{
				Player player = character as Player;
				if ((player != null && player.m_Gamer == null) || player.m_Gamer.IsLocal())
				{
					serializedCharacterList2.AddCharacter(character);
				}
			}
		}
	}

	public void AddPlayerCharacter(Character character)
	{
		m_SerializedCharacterLists[0].AddCharacter(character);
	}

	public void AddHighFrequencyCharacter(Character character)
	{
		m_SerializedCharacterLists[1].AddCharacter(character);
	}

	public void AddLowFrequencyCharacter(Character character)
	{
		m_SerializedCharacterLists[3].AddCharacter(character);
	}

	public void RemovePlayerCharacter(Character character)
	{
		m_SerializedCharacterLists[0].RemoveCharacter(character);
	}

	public void RemoveHighFrequencyCharacter(Character character)
	{
		m_SerializedCharacterLists[1].RemoveCharacter(character);
	}

	public void RemoveLowFrequencyCharacter(Character character)
	{
		m_SerializedCharacterLists[3].RemoveCharacter(character);
	}

	private SerializedCharacterList GetSerializedCharacterList(CharacterSerializerListType type)
	{
		if ((int)type < m_SerializedCharacterLists.Count)
		{
			return m_SerializedCharacterLists[(int)type];
		}
		return null;
	}

	private void SortCharacterLists()
	{
		List<Player> allPlayers = Player.GetAllPlayers();
		SerializedCharacterList serializedCharacterList = GetSerializedCharacterList(CharacterSerializerListType.High);
		SerializedCharacterList serializedCharacterList2 = GetSerializedCharacterList(CharacterSerializerListType.Medium);
		SerializedCharacterList serializedCharacterList3 = GetSerializedCharacterList(CharacterSerializerListType.Low);
		int count = m_allCharacters.Count;
		for (int num = m_allCharacters.Count - 1; num >= 0; num--)
		{
			Character character = m_allCharacters[num];
			if (!character.IsPlayer())
			{
				character.m_PreviousSerializeRate = character.m_SerializeRate;
				character.m_SerializeRate = CharacterSerializerListType.Low;
			}
		}
		serializedCharacterList.ClearCharacterList();
		serializedCharacterList2.ClearCharacterList();
		serializedCharacterList3.ClearCharacterList();
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Character character2 = allPlayers[i];
			if (null != character2.m_CharacterTarget)
			{
				character2.m_CharacterTarget.m_SerializeRateOverride = CharacterSerializerListType.High;
			}
			else if (character2.IsBeingCarried())
			{
				character2.GetPickedUpByCharacter().m_SerializeRateOverride = CharacterSerializerListType.High;
			}
		}
		for (int j = 0; j < allPlayers.Count; j++)
		{
			Character character3 = allPlayers[j];
			Vector2 vector = character3.m_Transform.position;
			if (character3.m_NetView.isMine)
			{
				continue;
			}
			for (int num2 = m_allCharacters.Count - 1; num2 >= 0; num2--)
			{
				Character character4 = m_allCharacters[num2];
				if (!character4.IsPlayer())
				{
					Vector2 vector2 = character4.transform.position;
					if (character4.m_SerializeRateOverride != CharacterSerializerListType.COUNT)
					{
						SerializedCharacterList serializedCharacterList4 = GetSerializedCharacterList(character4.m_SerializeRateOverride);
						serializedCharacterList4.AddCharacter(character4);
					}
					else if (character4.m_SerializeRate == CharacterSerializerListType.Low && Vector2.SqrMagnitude(vector2 - vector) < 324f)
					{
						if (character4.m_PreviousSerializeRate == CharacterSerializerListType.Low)
						{
							character4.ForcePhotonSerialiseView();
						}
						if (character4.m_CharacterAnimator.GetLockedOn())
						{
							serializedCharacterList.AddCharacter(character4);
						}
						else
						{
							serializedCharacterList2.AddCharacter(character4);
						}
					}
				}
			}
		}
		for (int num3 = m_allCharacters.Count - 1; num3 >= 0; num3--)
		{
			Character character5 = m_allCharacters[num3];
			if (!character5.IsPlayer() && character5.m_NetView.isMine && character5.m_SerializeRate == CharacterSerializerListType.Low)
			{
				serializedCharacterList3.AddCharacter(character5);
			}
		}
	}

	public void ControlledLateUpdate()
	{
		if (!m_bActive || m_allCharacters == null || PhotonNetwork.playerList == null || !(UpdateManager.time > m_nextSerializeTime))
		{
			return;
		}
		float num = 0f;
		for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
		{
			float fLatency = 0f;
			if (PeerLatency.GetPeerLatency(PhotonNetwork.playerList[i].ID, ref fLatency))
			{
				num = Mathf.Max(num, fLatency);
			}
		}
		if (num > 1f)
		{
			m_timeBetweenSerializations = 1f;
		}
		else if (num > 0.5f)
		{
			m_timeBetweenSerializations = 0.1f;
		}
		else if (num > 0.2f)
		{
			m_timeBetweenSerializations = 1f / 15f;
		}
		else if (num < 0.09f)
		{
			m_timeBetweenSerializations = 1f / 30f;
		}
		m_nextSerializeTime = UpdateManager.time + m_timeBetweenSerializations;
		T17NetLoadSync instance = T17NetLoadSync.Instance;
		if (!(instance != null) || !instance.AnyClientsReadyForClientSerialization())
		{
			return;
		}
		if (m_bCreateCharacterLists)
		{
			m_bCreateCharacterLists = false;
			CreateSerializedCharacterLists();
		}
		bool isMasterClient = T17NetManager.IsMasterClient;
		if (isMasterClient)
		{
			SortCharacterLists();
		}
		float realTime = T17NetManager.RealTime;
		int mTU = (PhotonNetwork.networkingPeer.MaximumTransferUnit - 256) * 8;
		int j = 0;
		bool flag = true;
		Character character = null;
		float num2 = float.MaxValue;
		int count = m_allCharacters.Count;
		if (count != 0)
		{
			for (int k = 0; k < count; k++)
			{
				Character character2 = m_allCharacters[k];
				if (character2.m_NetView.isMine && character2.m_NextKeyFrame < num2)
				{
					num2 = character2.m_NextKeyFrame;
					character = character2;
				}
			}
			if (null != character)
			{
				if (num2 < realTime)
				{
					character.m_NextKeyFrame = realTime + character.m_KeyFrameDelay;
				}
				else
				{
					character = null;
				}
			}
		}
		while (flag)
		{
			m_serializeByteList.Clear();
			m_bitWriter.Reset(m_serializeByteList);
			m_bitWriter.Write(realTime);
			m_bitWriter.Write((byte)NetBluePrintDetails.Instance.MapRotationIndex, 4);
			int usedBitCount = m_bitWriter.GetUsedBitCount();
			if (isMasterClient)
			{
				flag = false;
				for (; j < m_SerializedCharacterLists.Count; j++)
				{
					if (!m_SerializedCharacterLists[j].Serialize(m_bitWriter, realTime, mTU, ref m_largestSent, character))
					{
						flag = true;
						break;
					}
				}
			}
			else
			{
				flag = !GetSerializedCharacterList(CharacterSerializerListType.Players).Serialize(m_bitWriter, realTime, mTU, ref m_largestSent, character);
			}
			if (m_bitWriter.GetUsedBitCount() > usedBitCount)
			{
				RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
				raiseEventOptions.InterestGroup = 5;
				raiseEventOptions.CachingOption = EventCaching.DoNotCache;
				raiseEventOptions.SequenceChannel = (byte)(1 + m_SerializationNumber % 7);
				raiseEventOptions.Encrypt = true;
				m_lastSentPacketSize = m_serializeByteList.Count;
				PhotonNetwork.RaiseEvent(18, m_serializeByteList.ToArray(), sendReliable: false, raiseEventOptions);
				m_SerializationNumber++;
				PhotonNetwork.networkingPeer.SendOutgoingCommands();
				m_sends++;
			}
			else
			{
				m_lastSentPacketSize = 0;
			}
		}
		for (j = 0; j < m_SerializedCharacterLists.Count; j++)
		{
			m_SerializedCharacterLists[j].UpdateTimeStampsIfSerializedThisFrame(realTime);
		}
	}

	private void OnEvent(T17NetConfig.NetEventTypes roomSignal, object payload, int senderId, bool isUs)
	{
		if (roomSignal != T17NetConfig.NetEventTypes.CharacterSerialization)
		{
			return;
		}
		m_receives++;
		if (!m_bActive || payload == null || m_allCharacters == null)
		{
			return;
		}
		byte[] array = (byte[])payload;
		if (array == null)
		{
			return;
		}
		m_bitReader.Reset(array);
		int num = array.Length - 1;
		int count = m_allCharacters.Count;
		float num2 = m_bitReader.ReadFloat32();
		byte b = m_bitReader.ReadByte(4);
		bool flag = false;
		for (int num3 = m_SentTimesLength - 1; num3 >= 0; num3--)
		{
			if (m_SentTimes[num3, 0] == (float)senderId)
			{
				float num4 = m_SentTimes[num3, 1];
				if (b != (byte)NetBluePrintDetails.Instance.MapRotationIndex)
				{
					T17NetManager.LogGoogleException("Discarding previous character network update as it is from a previous prison! " + b + " vs " + (byte)NetBluePrintDetails.Instance.MapRotationIndex);
					break;
				}
				if (num4 <= num2 || num4 - num2 > 10000f)
				{
					flag = true;
					m_SentTimes[num3, 1] = num2;
					break;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		while (m_bitReader.CurrentIndex < num)
		{
			int num5 = (int)m_bitReader.ReadUInt32(8);
			if (num5 >= 0 && num5 < count)
			{
				Character character = m_allCharacters[num5];
				if (character.m_NetView.isMine)
				{
					break;
				}
				character.NetworkSerializeRead(m_bitReader, num2);
			}
		}
	}

	private void OnPropertyChanged(T17NetRoomGameView.CustomProperty propertyThatChanged, string strValue)
	{
		if (propertyThatChanged == T17NetRoomGameView.CustomProperty.Gamers)
		{
			m_bCreateCharacterLists = true;
		}
	}

	private void Start()
	{
		ms_instance = this;
		UpdateManager instance = UpdateManager.GetInstance();
		if (instance != null)
		{
			instance.Register(this, UpdateCategory.RapidPeriodic);
		}
		m_serializeByteList = new FastList<byte>();
		m_bitWriter = new BitStreamWriter(m_serializeByteList);
		m_bitReader = new BitStreamReader(new byte[1]);
		T17NetRoomGameView.OnRoomSignalEvent += OnEvent;
		T17NetRoomGameView.OnPropertyChanged += OnPropertyChanged;
		T17NetManager.OnJoinedRoomEvent += OnJoinedPhotonRoom;
		T17NetManager.OnLeftRoomEvent += OnLeftPhotonRoom;
		T17NetManager.OnPhotonPlayerConnectedEvent += PlayerConnectionChange;
		T17NetManager.OnPhotonPlayerDisconnectedEvent += PlayerConnectionChange;
		NetLoadManagerSync.m_AllNetworkLoadables.Add(this);
		DisableSerialization();
	}

	protected virtual void OnDestroy()
	{
		T17NetManager.OnLeftRoomEvent -= OnLeftPhotonRoom;
		T17NetManager.OnJoinedRoomEvent -= OnJoinedPhotonRoom;
		T17NetManager.OnPhotonPlayerConnectedEvent -= PlayerConnectionChange;
		T17NetManager.OnPhotonPlayerDisconnectedEvent -= PlayerConnectionChange;
		T17NetRoomGameView.OnRoomSignalEvent -= OnEvent;
		T17NetRoomGameView.OnPropertyChanged -= OnPropertyChanged;
		NetLoadManagerSync.m_AllNetworkLoadables.Remove(this);
		DisableSerialization();
		UpdateManager instance = UpdateManager.GetInstance();
		if (instance != null)
		{
			instance.Unregister(this, UpdateCategory.RapidPeriodic);
		}
		m_NetView = null;
		ms_instance = null;
	}

	public void EnableSerialization()
	{
		if (!m_bActive)
		{
			m_bActive = true;
			m_allCharacters = new FastList<Character>(Character.GetAllCharacters());
			m_allCharacters.Sort((Character a, Character b) => a.m_NetView.viewID.CompareTo(b.m_NetView.viewID));
			for (int num = m_allCharacters.Count - 1; num >= 0; num--)
			{
				m_allCharacters[num].SetCharacterSerializerIndex(num);
			}
			PhotonNetwork.SetReceivingEnabled(5, enabled: true);
			PhotonNetwork.SetSendingEnabled(5, enabled: true);
			ResetReceiveCounts();
			m_bCreateCharacterLists = true;
		}
	}

	public void DisableSerialization()
	{
		if (m_bActive)
		{
			m_bActive = false;
			PhotonNetwork.networkingPeer.DispatchIncomingCommands();
			PhotonNetwork.networkingPeer.SendOutgoingCommands();
			PhotonNetwork.SetReceivingEnabled(5, enabled: false);
			PhotonNetwork.SetSendingEnabled(5, enabled: false);
			m_SerializedCharacterLists.Clear();
		}
	}

	public void SetSerializeRate(float value)
	{
		m_timeBetweenSerializations = value;
	}

	public float GetSerializeRate()
	{
		return m_timeBetweenSerializations;
	}

	public int GetLargestEventSize()
	{
		return m_largestSent;
	}

	public int GetSendCount()
	{
		return m_sends;
	}

	public int GetReceiveCount()
	{
		return m_receives;
	}

	public int GetLastSentPacketSize()
	{
		return m_lastSentPacketSize;
	}

	public void OnJoinedPhotonRoom(short result)
	{
		if (GlobalStart.GetInstance().GetMode() == GlobalStart.GLOBALSTART_MODE.IN_LEVEL && result == 0)
		{
			DisableSerialization();
			EnableSerialization();
		}
		ResetReceiveCounts();
	}

	private void ResetReceiveCounts()
	{
		int num = 0;
		PhotonPlayer[] playerList = PhotonNetwork.playerList;
		if (playerList == null)
		{
			return;
		}
		for (int num2 = playerList.Length - 1; num2 >= 0; num2--)
		{
			if (playerList[num2] != null && !playerList[num2].IsLocal)
			{
				num++;
			}
		}
		m_SentTimes = new float[num, 2];
		m_SentTimesLength = num;
		int num3 = 0;
		for (int num4 = playerList.Length - 1; num4 >= 0; num4--)
		{
			if (playerList[num4] != null && !playerList[num4].IsLocal)
			{
				m_SentTimes[num3, 0] = (uint)playerList[num4].ID;
				num3++;
			}
		}
	}

	public Character GetSortedCharacter(int index)
	{
		if (index >= 0 && index < m_allCharacters.Count)
		{
			return m_allCharacters[index];
		}
		return null;
	}

	public FastList<Character> GetSortedCharacterList()
	{
		return m_allCharacters;
	}

	private void PlayerConnectionChange(PhotonPlayer player)
	{
		ResetReceiveCounts();
		m_bCreateCharacterLists = true;
	}

	public void OnLeftPhotonRoom()
	{
		DisableSerialization();
	}

	public void ControlledFixedUpdate()
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
		return false;
	}

	public bool RequiresControlledFixedUpdate()
	{
		return false;
	}

	public bool RequiresControlledLateUpdate()
	{
		return true;
	}

	public bool RequiresControlledPreUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreFixedUpdate()
	{
		return false;
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
			List<short> payload = new List<short>();
			for (int num = m_allCharacters.Count - 1; num >= 0; num--)
			{
				WriteInteractiveObjectData(ref payload, m_allCharacters[num]);
			}
			short[] array = payload.ToArray();
			m_NetView.RPC("RPC_RequestStateResponce_Yes_LoadCharacter", player, array);
		}
		else
		{
			m_NetView.RPC("RPC_RequestStateResponce_No_LoadCharacter", player);
		}
	}

	[PunRPC]
	public void RPC_RequestStateResponce_No_LoadCharacter(PhotonMessageInfo info)
	{
		m_LoadError = "Generator RPC_RequestStateResponce_No - Error on MasterClient";
		m_LoadState = LOADSTATE.Finished_Error;
	}

	[PunRPC]
	public void RPC_RequestStateResponce_Yes_LoadCharacter(short[] payloadArray, PhotonMessageInfo info)
	{
		ReadInteractiveObjectData(payloadArray);
		m_LoadState = LOADSTATE.Finished_OK;
	}

	private void WriteInteractiveObjectData(ref List<short> payload, Character character)
	{
		InteractiveObject interactiveObject = character.GetInteractiveObject();
		if (interactiveObject == null)
		{
			interactiveObject = character.GetRemoteInteractiveObject();
		}
		if (interactiveObject != null && interactiveObject.m_NetViewID != null)
		{
			short item = (short)character.GetCharacterSerializerIndex();
			short item2 = (short)interactiveObject.m_NetViewID.viewID;
			short item3 = (short)interactiveObject.GetLocalInteractionId();
			payload.Add(item);
			payload.Add(item2);
			payload.Add(item3);
		}
	}

	private void ReadInteractiveObjectData(short[] payload)
	{
		if (payload == null || payload.Length == 0)
		{
			return;
		}
		int num = payload.Length;
		if (num % 3 != 0)
		{
			return;
		}
		for (int i = 0; i < num; i += 3)
		{
			short num2 = payload[i];
			short netViewID = payload[i + 1];
			short interactionID = payload[i + 2];
			if (num2 >= 0 && num2 < m_allCharacters.Count)
			{
				Character character = m_allCharacters[num2];
				character.SetRemoteInteractiveObject(netViewID, interactionID);
				InteractiveObject remoteInteractiveObject = character.GetRemoteInteractiveObject();
				if (remoteInteractiveObject != null)
				{
					remoteInteractiveObject.OnLateJoiningInteractionCatchup(character);
				}
			}
		}
	}

	public string GetDebugText()
	{
		string text = string.Empty;
		for (int i = 0; i < m_SerializedCharacterLists.Count; i++)
		{
			text = text + "\n" + m_SerializedCharacterLists[i].GetDebugString();
		}
		text = text + "\nSorting Time : " + m_SortListsTime;
		text += "\n";
		for (int j = 0; j < m_SerializedCharacterLists.Count; j++)
		{
			SerializedCharacterList serializedCharacterList = m_SerializedCharacterLists[j];
			if (serializedCharacterList != null)
			{
				text = text + "\n" + serializedCharacterList.m_ListType.ToString() + ":";
				for (int k = 0; k < serializedCharacterList.m_Characters.Count; k++)
				{
					text = text + "\n" + serializedCharacterList.m_Characters[k].name;
				}
			}
		}
		return text;
	}
}
