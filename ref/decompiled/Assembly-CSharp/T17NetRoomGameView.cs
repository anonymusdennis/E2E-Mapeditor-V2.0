using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using ExtensionMethods;
using UnityEngine;

public class T17NetRoomGameView : T17NetSendMonoMessageTarget
{
	public enum RoomPlatformProperty
	{
		Undefined = -1,
		PS4,
		XBoxOne,
		Windows
	}

	public enum GameRoomType
	{
		Undefined = -1,
		Offline,
		Public,
		Private
	}

	public enum EscapeState
	{
		NotEscaped,
		Escaped
	}

	public enum CustomProperty
	{
		RoomPlatformType,
		RoomType,
		ConfigType,
		AppVersion,
		Gamers,
		GamerCount,
		LoadStates,
		GameState,
		EscapeState,
		PrisonBlueprint,
		PlaylistId,
		PrisonEnum,
		DisplayName,
		PrisonDay,
		PrisonHour,
		PrisonDetails,
		CarriedObjectsData,
		ToiletData,
		SolitaryData,
		OpinionData,
		PlayerAppearanceData,
		PlayerTagData,
		PlayerItemTrackingData,
		CCTVCameraData,
		HostName,
		HostKey,
		Password,
		MAX
	}

	public delegate void PropertyChangedDelegate(CustomProperty propertyThatChanged, string strValue);

	public delegate void RoomSignalHandler(T17NetConfig.NetEventTypes roomSignal, object payload, int senderId, bool isUs);

	private const string LOGTITLE = "NetRoomGameView";

	private static bool[] m_updateCustomProperty = new bool[27];

	private static Dictionary<CustomProperty, string> m_EncodedKeys = new Dictionary<CustomProperty, string>();

	private static ExitGames.Client.Photon.Hashtable m_properties = new ExitGames.Client.Photon.Hashtable();

	private T17NetView m_netView;

	private NetBluePrintDetails m_bluePrintDetails;

	private static T17NetRoomGameView m_instance = null;

	private static ExitGames.Client.Photon.Hashtable m_TempProperties = new ExitGames.Client.Photon.Hashtable();

	public static T17NetRoomGameView Instance => m_instance;

	public static event PropertyChangedDelegate OnPropertyChanged;

	public static event RoomSignalHandler OnRoomSignalEvent;

	public static bool GetCustomPropertyDirty(CustomProperty property)
	{
		return m_updateCustomProperty[(int)property];
	}

	public static void SetCustomPropertyDirtyFlag(CustomProperty property, bool bDirty)
	{
		m_updateCustomProperty[(int)property] = bDirty;
	}

	protected override void Awake()
	{
		base.Awake();
		if (m_instance != null)
		{
			throw new Exception("You cannot have multiple managers");
		}
		m_instance = this;
		for (int num = 26; num >= 0; num--)
		{
			m_updateCustomProperty[num] = false;
		}
		PhotonNetwork.OnEventCall = (PhotonNetwork.EventCallback)Delegate.Combine(PhotonNetwork.OnEventCall, new PhotonNetwork.EventCallback(OnEvent));
		T17NetManager.OnJoinedRoomEvent += OnJoinedRoom_Delayed;
		T17NetManager.OnBecameMasterClient += OnBecameMasterClient;
	}

	private void Start()
	{
		if (m_netView == null)
		{
			m_netView = GetComponent<T17NetView>();
			if (m_netView != null && !DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetViewID))
			{
			}
		}
		OnPropertyChanged += OnBluePrintDetailsPropertyChanged;
		OnPropertyChanged += OnPrisonViewDetailsPropertyChanged;
		OnPropertyChanged += OnGamersViewChanged;
		OnPropertyChanged += OnLoadStatesChanged;
		OnPropertyChanged += OnCarriedObjectDataChanged;
		OnPropertyChanged += OnToiletDataChanged;
		OnPropertyChanged += OnSolitaryDataChanged;
		OnPropertyChanged += OnOpinionDataChanged;
		OnPropertyChanged += OnPlayerAppearanceDataChanged;
		OnPropertyChanged += OnPlayerTagDataChanged;
		OnPropertyChanged += OnCCTVCamerasDataChanged;
		OnPropertyChanged += OnPlayerItemTrackingDataChanged;
		OnPropertyChanged += OnHostKeyChanged;
	}

	protected virtual void OnDestroy()
	{
		PhotonNetwork.OnEventCall = (PhotonNetwork.EventCallback)Delegate.Remove(PhotonNetwork.OnEventCall, new PhotonNetwork.EventCallback(OnEvent));
		OnPropertyChanged -= OnBluePrintDetailsPropertyChanged;
		OnPropertyChanged -= OnPrisonViewDetailsPropertyChanged;
		OnPropertyChanged -= OnGamersViewChanged;
		OnPropertyChanged -= OnLoadStatesChanged;
		OnPropertyChanged -= OnCarriedObjectDataChanged;
		OnPropertyChanged -= OnToiletDataChanged;
		OnPropertyChanged -= OnSolitaryDataChanged;
		OnPropertyChanged -= OnOpinionDataChanged;
		OnPropertyChanged -= OnPlayerAppearanceDataChanged;
		OnPropertyChanged -= OnPlayerTagDataChanged;
		OnPropertyChanged -= OnCCTVCamerasDataChanged;
		OnPropertyChanged -= OnPlayerItemTrackingDataChanged;
		OnPropertyChanged -= OnHostKeyChanged;
		T17NetManager.OnJoinedRoomEvent -= OnJoinedRoom_Delayed;
		T17NetManager.OnBecameMasterClient -= OnBecameMasterClient;
		m_netView = null;
	}

	private void Update()
	{
	}

	public void MasterSetNextLoadLevel(string levelName, int configID, bool LateJoined)
	{
		if (T17NetManager.ConnectionState == T17NetConnectState.Connected && T17NetManager.ConnectionDetailed == T17NetPeerState.Joined)
		{
			if (m_netView != null && T17NetManager.IsMasterClient && !LateJoined)
			{
				m_netView.RPC("RPC_SetMasterLoadLevel", NetTargets.Others, levelName, configID);
				T17NetLoadSync.Instance.Reset();
				T17NetLoadSync.Instance.UpdateStatesFromGamers();
			}
			else if (m_netView != null && !T17NetManager.IsMasterClient && LateJoined)
			{
				T17NetLoadSync.Instance.Reset();
				T17NetLoadSync.Instance.UpdateStatesFromGamers();
			}
			else
			{
				Debug.LogErrorFormat(this, "MasterSetNextLoadLevel : NetView component missing from {0}", base.gameObject.name);
			}
		}
	}

	public void ForceClientsToNextLevelRPC(string nextLevelToLoad)
	{
		if (!T17NetManager.IsMasterClient)
		{
			return;
		}
		if (m_netView != null)
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetLevelLoadState))
			{
			}
			m_netView.RPC("RPC_ForceClientsToNextLevel", NetTargets.Others, nextLevelToLoad);
			if (!DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetPhotonEvent))
			{
			}
		}
		else
		{
			Debug.LogErrorFormat(this, "T17NetRoomGameView.ForceClientsToNextLevelRPC : NetView component missing from {0}", base.gameObject.name);
		}
	}

	[PunRPC]
	public void RPC_ForceClientsToNextLevel(string nextLevelToLoad)
	{
		if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetPhotonEvent))
		{
		}
		if (!T17NetManager.IsMasterClient)
		{
			if (!DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetLevelLoadState))
			{
			}
		}
		else if (!DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetLevelLoadState))
		{
		}
	}

	[PunRPC]
	public void RPC_SetMasterLoadLevel(string levelName)
	{
		if (!DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetLevelLoadState))
		{
		}
	}

	public void TransmitNewMasterClientRPC()
	{
		if (!T17NetManager.IsMasterClient)
		{
			return;
		}
		if (m_netView != null)
		{
			PhotonPlayer masterClient = PhotonNetwork.masterClient;
			Instance.SetCustomProperty(CustomProperty.PrisonDay, RoutineManager.GetInstance().GetDaysElapsed());
			Instance.SetCustomProperty(CustomProperty.PrisonEnum, (int)LevelScript.GetCurrentLevelInfo().m_PrisonEnum);
			SetCustomProperty(CustomProperty.Gamers, NetUserManager.GamerRoomProperty);
			if (!DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetPhotonEvent))
			{
			}
		}
		else
		{
			Debug.LogErrorFormat(this, "T17NetRoomGameView.TransmitNewMasterClientRPC : NetView component missing from {0}", base.gameObject.name);
		}
	}

	private void OnEvent(byte eventcode, object content, int senderid)
	{
		T17NetConfig.NetEventTypes roomSignal = (T17NetConfig.NetEventTypes)eventcode;
		bool isUs = T17NetManager.PhotonPlayerID == senderid;
		if (T17NetRoomGameView.OnRoomSignalEvent != null)
		{
			T17NetRoomGameView.OnRoomSignalEvent(roomSignal, content, senderid, isUs);
		}
	}

	public void SignalToRoomEvent(T17NetConfig.NetEventTypes signal, object payload = null, bool viaServer = false)
	{
		if (PhotonNetwork.offlineMode)
		{
			if (PhotonNetwork.OnEventCall != null)
			{
				PhotonNetwork.OnEventCall((byte)signal, payload, PhotonNetwork.player.ID);
			}
		}
		else
		{
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			raiseEventOptions.Receivers = ReceiverGroup.All;
			raiseEventOptions.Encrypt = true;
			PhotonNetwork.RaiseEvent((byte)signal, payload, sendReliable: true, raiseEventOptions);
		}
	}

	public void SignalToRoomEvent(T17NetConfig.NetEventTypes signal, T17NetConfig.NetSequenceChannel channel, object payload = null, bool viaServer = false, bool bCached = false)
	{
		if (PhotonNetwork.offlineMode)
		{
			if (PhotonNetwork.OnEventCall != null)
			{
				PhotonNetwork.OnEventCall((byte)signal, payload, PhotonNetwork.player.ID);
			}
			return;
		}
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
		raiseEventOptions.Receivers = ReceiverGroup.All;
		raiseEventOptions.SequenceChannel = (byte)channel;
		if (bCached)
		{
			raiseEventOptions.CachingOption = EventCaching.AddToRoomCacheGlobal;
		}
		raiseEventOptions.Encrypt = true;
		PhotonNetwork.RaiseEvent((byte)signal, payload, sendReliable: true, raiseEventOptions);
	}

	public void SignalEventToPeers(T17NetConfig.NetEventTypes signal, T17NetConfig.NetSequenceChannel channel, int[] photonID, object payload = null, bool reliable = true)
	{
		if (PhotonNetwork.offlineMode)
		{
			if (PhotonNetwork.OnEventCall != null)
			{
				PhotonNetwork.OnEventCall((byte)signal, payload, PhotonNetwork.player.ID);
			}
		}
		else
		{
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			raiseEventOptions.TargetActors = photonID;
			raiseEventOptions.SequenceChannel = (byte)channel;
			raiseEventOptions.Encrypt = true;
			PhotonNetwork.RaiseEvent((byte)signal, payload, reliable, raiseEventOptions);
		}
	}

	private static void WriteLocalPropertiesToPhoton(ExitGames.Client.Photon.Hashtable properties)
	{
		if (!T17NetManager.IsMasterClient || PhotonNetwork.room == null)
		{
			return;
		}
		m_TempProperties.Clear();
		foreach (DictionaryEntry property in properties)
		{
			string text = property.Key as string;
			if (string.IsNullOrEmpty(text))
			{
				continue;
			}
			if (NetMatchmakingConfig.GetMatchmakingParameters().ContainsValue(text))
			{
				m_TempProperties[property.Key] = property.Value;
				continue;
			}
			byte result = 0;
			if (byte.TryParse(text, out result))
			{
				CustomProperty key = (CustomProperty)result;
				EncryptRoomProperty(key, property.Value, ref m_TempProperties);
				continue;
			}
			CustomProperty result2 = CustomProperty.RoomPlatformType;
			if (result2.TryParse<CustomProperty>(text, ignoreCase: true, out result2))
			{
				string key2 = ((byte)result2).ToString();
				if (!m_TempProperties.ContainsKey(key2))
				{
					EncryptRoomProperty(result2, property.Value, ref m_TempProperties);
				}
			}
		}
		PhotonNetwork.room.SetCustomProperties(m_TempProperties);
	}

	public static void OnKeyRefresh()
	{
		if (!T17NetManager.IsMasterClient)
		{
			return;
		}
		m_TempProperties.Clear();
		List<CustomProperty> list = new List<CustomProperty>(m_EncodedKeys.Keys);
		foreach (CustomProperty item in list)
		{
			string value = m_properties[((byte)item).ToString()] as string;
			EncryptRoomProperty(item, value, ref m_TempProperties);
		}
		PhotonNetwork.room.SetCustomProperties(m_TempProperties);
	}

	public static void EncryptRoomProperty(CustomProperty key, object value, ref ExitGames.Client.Photon.Hashtable hashTable)
	{
		string key2 = ((byte)key).ToString();
		hashTable.Add(key2, value);
	}

	private void DecryptRoomProperty(object encryptedKey, object encryptedValue, out string key, out string value)
	{
		key = encryptedKey as string;
		value = encryptedValue as string;
	}

	public bool SetCustomProperty(CustomProperty property, int value)
	{
		if (!string.IsNullOrEmpty(NetMatchmakingConfig.GetMatchmakingParameterFromCustomProperty(property)) || !NetMatchmakingConfig.IsEncrypted(((byte)property).ToString()))
		{
			return SetCustomPropertyInternal(property, value);
		}
		return SetCustomPropertyInternal(property, value.ToString());
	}

	public bool SetCustomProperty(CustomProperty property, string value)
	{
		return SetCustomPropertyInternal(property, value);
	}

	private bool SetCustomPropertyInternal(CustomProperty property, object value)
	{
		if (!T17NetManager.IsMasterClient)
		{
			return false;
		}
		if (property == CustomProperty.Password)
		{
			value = Encryption.Encrypt(value, "of all the flavours you choose to be salty", "SHA1", 2, 256, "default");
		}
		if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetCustomProperty))
		{
		}
		string matchmakingParameterFromCustomProperty = NetMatchmakingConfig.GetMatchmakingParameterFromCustomProperty(property);
		if (matchmakingParameterFromCustomProperty != null)
		{
			m_properties[matchmakingParameterFromCustomProperty] = value;
		}
		else
		{
			m_properties[((byte)property).ToString()] = value;
		}
		if (PhotonNetwork.room != null)
		{
			m_TempProperties.Clear();
			if (matchmakingParameterFromCustomProperty != null)
			{
				m_TempProperties[matchmakingParameterFromCustomProperty] = value;
			}
			else if (!NetMatchmakingConfig.IsEncrypted(((byte)property).ToString()))
			{
				m_TempProperties[((byte)property).ToString()] = value;
			}
			else
			{
				EncryptRoomProperty(property, value, ref m_TempProperties);
			}
			PhotonNetwork.room.SetCustomProperties(m_TempProperties);
		}
		return true;
	}

	public void SetCustomProperties(ExitGames.Client.Photon.Hashtable properties)
	{
		WriteLocalPropertiesToPhoton(properties);
		m_properties.Merge(properties);
	}

	public ExitGames.Client.Photon.Hashtable GetCustomProperties()
	{
		return m_properties;
	}

	public static bool GetCustomPropertyAsString(CustomProperty customProperty, ref string outValue, ExitGames.Client.Photon.Hashtable properties)
	{
		if (properties == null)
		{
			properties = m_properties;
		}
		string matchmakingParameterFromCustomProperty = NetMatchmakingConfig.GetMatchmakingParameterFromCustomProperty(customProperty);
		string key = matchmakingParameterFromCustomProperty ?? ((byte)customProperty).ToString();
		if (properties.ContainsKey(key))
		{
			outValue = (string)properties[key];
			return true;
		}
		return true;
	}

	public static bool GetCustomPropertyAsString(CustomProperty customProperty, ref string outValue)
	{
		return GetCustomPropertyAsString(customProperty, ref outValue, m_properties);
	}

	public static bool GetCustomPropertyAsEnum<T>(CustomProperty customProperty, ref T outValue, ExitGames.Client.Photon.Hashtable properties)
	{
		string matchmakingParameterFromCustomProperty = NetMatchmakingConfig.GetMatchmakingParameterFromCustomProperty(customProperty);
		string text = ((byte)customProperty).ToString();
		if (NetMatchmakingConfig.IsEncrypted(text))
		{
			string key = ((byte)customProperty).ToString();
			if (properties.ContainsKey(key))
			{
				string value = (string)properties[key];
				if (!string.IsNullOrEmpty(value))
				{
					outValue = (T)Enum.Parse(typeof(T), value);
					return true;
				}
				return false;
			}
		}
		else if (string.IsNullOrEmpty(matchmakingParameterFromCustomProperty))
		{
			if (properties.ContainsKey(text))
			{
				outValue = (T)properties[text];
				return true;
			}
		}
		else if (properties.ContainsKey(matchmakingParameterFromCustomProperty))
		{
			outValue = (T)properties[matchmakingParameterFromCustomProperty];
			return true;
		}
		return false;
	}

	public static bool GetCustomPropertyAsEnum<T>(CustomProperty customProperty, ref T outValue)
	{
		return GetCustomPropertyAsEnum(customProperty, ref outValue, m_properties);
	}

	public static bool GetCustomPropertyAsInt(CustomProperty customProperty, ref int outValue, ExitGames.Client.Photon.Hashtable properties)
	{
		string matchmakingParameterFromCustomProperty = NetMatchmakingConfig.GetMatchmakingParameterFromCustomProperty(customProperty);
		string text = ((byte)customProperty).ToString();
		if (NetMatchmakingConfig.IsEncrypted(text))
		{
			string key = ((byte)customProperty).ToString();
			if (properties.ContainsKey(key))
			{
				string s = (string)properties[key];
				outValue = int.Parse(s);
				return true;
			}
		}
		else if (string.IsNullOrEmpty(matchmakingParameterFromCustomProperty))
		{
			if (properties.ContainsKey(text))
			{
				outValue = (int)properties[text];
				return true;
			}
		}
		else if (properties.ContainsKey(matchmakingParameterFromCustomProperty))
		{
			outValue = (int)properties[matchmakingParameterFromCustomProperty];
			return true;
		}
		return false;
	}

	public static bool GetCustomPropertyAsInt(CustomProperty customProperty, ref int outValue)
	{
		return GetCustomPropertyAsInt(customProperty, ref outValue, m_properties);
	}

	public void ClearCustomProperties()
	{
		if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetCustomProperty))
		{
		}
		Debug.LogFormat("T17NetRoomGameView.ClearCustomProperties: !!!!!!!!");
		m_properties.Clear();
		if (PhotonNetwork.room != null)
		{
			WriteLocalPropertiesToPhoton(m_properties);
		}
	}

	public void ClearPrisonProperties()
	{
		ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
		foreach (DictionaryEntry property2 in m_properties)
		{
			Debug.Log(string.Concat(" *******    property  <", property2.Key, ">"));
			string text = property2.Key as string;
			if (!string.IsNullOrEmpty(text))
			{
				CustomProperty property = CustomProperty.RoomPlatformType;
				if (NetMatchmakingConfig.GetMatchmakingCustomPropertyFromString(text, ref property))
				{
					Debug.Log(" *******    keep  " + text);
					hashtable[text] = property2.Value;
					continue;
				}
				switch ((CustomProperty)Enum.Parse(typeof(CustomProperty), text))
				{
				case CustomProperty.PrisonDetails:
				case CustomProperty.CarriedObjectsData:
				case CustomProperty.ToiletData:
				case CustomProperty.SolitaryData:
				case CustomProperty.OpinionData:
				case CustomProperty.PlayerAppearanceData:
				case CustomProperty.PlayerTagData:
				case CustomProperty.PlayerItemTrackingData:
				case CustomProperty.CCTVCameraData:
					Debug.Log(" *******    drop  " + text);
					hashtable[text] = null;
					break;
				default:
					Debug.Log(" *******    KEEP AGAIN    " + text);
					hashtable[property2.Key] = property2.Value;
					break;
				}
			}
			else
			{
				Debug.Log(" *******    not a string   ");
				hashtable[property2.Key] = property2.Value;
			}
		}
		m_properties = hashtable;
		if (PhotonNetwork.room != null)
		{
			WriteLocalPropertiesToPhoton(m_properties);
		}
	}

	public static bool HasCustomProperty(CustomProperty property)
	{
		string matchmakingParameterFromCustomProperty = NetMatchmakingConfig.GetMatchmakingParameterFromCustomProperty(property);
		if (string.IsNullOrEmpty(matchmakingParameterFromCustomProperty))
		{
			return m_properties.ContainsKey(property);
		}
		return m_properties.ContainsKey(matchmakingParameterFromCustomProperty);
	}

	public void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
	{
		if (!T17NetEncryptionKeys.HaveEncryptionKey())
		{
			return;
		}
		foreach (DictionaryEntry item in propertiesThatChanged)
		{
			m_TempProperties.Clear();
			string text = MergeDictionaryEntryIntoHashtable(item, ref m_TempProperties);
			if (string.IsNullOrEmpty(text))
			{
				continue;
			}
			bool flag = false;
			CustomProperty property = CustomProperty.RoomPlatformType;
			byte result = 0;
			if (byte.TryParse(text, out result))
			{
				property = (CustomProperty)result;
				flag = true;
			}
			else if (NetMatchmakingConfig.GetMatchmakingCustomPropertyFromString(text, ref property))
			{
				flag = true;
			}
			if (flag)
			{
				m_properties[text] = m_TempProperties[text];
				if (T17NetRoomManager.IsInRoom() && T17NetRoomGameView.OnPropertyChanged != null)
				{
					T17NetRoomGameView.OnPropertyChanged(property, m_TempProperties[text] as string);
				}
			}
		}
	}

	private void OnLoadStatesChanged(CustomProperty propertyThatChanged, string strValue)
	{
		if (propertyThatChanged == CustomProperty.LoadStates)
		{
			T17NetLoadSync.Instance.OnLoadStatesChanged(strValue);
		}
	}

	private void OnBluePrintDetailsPropertyChanged(CustomProperty propertyThatChanged, string strValue)
	{
		if (propertyThatChanged == CustomProperty.PrisonBlueprint)
		{
			if (m_bluePrintDetails == null)
			{
				m_bluePrintDetails = NetBluePrintDetails.Instance;
			}
			if (m_bluePrintDetails != null)
			{
				m_bluePrintDetails.SerializeFromJson(strValue);
			}
		}
	}

	private void OnHostKeyChanged(CustomProperty propertyThatChanged, string strValue)
	{
		if (propertyThatChanged == CustomProperty.HostKey && string.IsNullOrEmpty(strValue))
		{
		}
	}

	private void OnCarriedObjectDataChanged(CustomProperty propertyThatChanged, string strValue)
	{
		if (propertyThatChanged == CustomProperty.CarriedObjectsData && NetPrisonViewDetails.Instance != null)
		{
			NetPrisonViewDetails.Instance.m_CarriedObjectsData = strValue;
		}
	}

	private void OnToiletDataChanged(CustomProperty propertyThatChanged, string strValue)
	{
		if (propertyThatChanged == CustomProperty.ToiletData && NetPrisonViewDetails.Instance != null)
		{
			NetPrisonViewDetails.Instance.m_ToiletData = strValue;
		}
	}

	private void OnCCTVCamerasDataChanged(CustomProperty propertyThatChanged, string strValue)
	{
		if (propertyThatChanged == CustomProperty.CCTVCameraData && NetPrisonViewDetails.Instance != null)
		{
			NetPrisonViewDetails.Instance.m_CctvCamerasData = strValue;
		}
	}

	private void OnPrisonViewDetailsPropertyChanged(CustomProperty propertyThatChanged, string strValue)
	{
		if (propertyThatChanged == CustomProperty.PrisonDetails && NetPrisonViewDetails.Instance != null)
		{
			NetPrisonViewDetails.Instance.SerializeFromJson(strValue);
		}
	}

	private void OnGamersViewChanged(CustomProperty propertyThatChanged, string strValue)
	{
		if (propertyThatChanged == CustomProperty.Gamers && strValue != null)
		{
			NetUserManager.GamerRoomProperty = strValue;
		}
	}

	private void OnSolitaryDataChanged(CustomProperty propertyThatChanged, string strValue)
	{
		if (propertyThatChanged == CustomProperty.SolitaryData && NetPrisonViewDetails.Instance != null)
		{
			NetPrisonViewDetails.Instance.m_SolitaryData = strValue;
		}
	}

	private void OnOpinionDataChanged(CustomProperty propertyThatChanged, string strValue)
	{
		if (propertyThatChanged == CustomProperty.OpinionData && NetPrisonViewDetails.Instance != null)
		{
			NetPrisonViewDetails.Instance.m_OpinionData = strValue;
		}
	}

	private void OnPlayerAppearanceDataChanged(CustomProperty propertyThatChanged, string strValue)
	{
		if (propertyThatChanged == CustomProperty.PlayerAppearanceData && NetPrisonViewDetails.Instance != null)
		{
			NetPrisonViewDetails.Instance.m_PlayerAppearanceData = strValue;
		}
	}

	private void OnPlayerTagDataChanged(CustomProperty propertyThatChanged, string strValue)
	{
		if (propertyThatChanged == CustomProperty.PlayerTagData && NetPrisonViewDetails.Instance != null)
		{
			NetPrisonViewDetails.Instance.m_PlayerTagData = strValue;
		}
	}

	private void OnPlayerItemTrackingDataChanged(CustomProperty propertyThatChanged, string strValue)
	{
		if (propertyThatChanged == CustomProperty.PlayerItemTrackingData && NetPrisonViewDetails.Instance != null)
		{
			NetPrisonViewDetails.Instance.m_PlayerItemTrackingData = strValue;
		}
	}

	private void OnJoinedRoom_Delayed(short result)
	{
		m_EncodedKeys.Clear();
		if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetCustomProperty))
		{
		}
		if (PhotonNetwork.room != null)
		{
			WriteLocalPropertiesToPhoton(m_properties);
			OnPhotonCustomRoomPropertiesChanged(PhotonNetwork.room.CustomProperties);
		}
	}

	private void OnLeftRoom()
	{
		m_EncodedKeys.Clear();
	}

	private string MergeDictionaryEntryIntoHashtable(DictionaryEntry property, ref ExitGames.Client.Photon.Hashtable hashTable)
	{
		string text = property.Key as string;
		if (string.IsNullOrEmpty(text))
		{
			Debug.Log("MergeDictionaryEntryIntoHashtable - ignoring non string key encountered " + property.Key.ToString() + " value " + property.Value.ToString());
			return null;
		}
		if (NetMatchmakingConfig.GetMatchmakingParameters().ContainsValue(text) || !NetMatchmakingConfig.IsEncrypted(text))
		{
			hashTable[text] = property.Value;
			return text;
		}
		DecryptRoomProperty(property.Key, property.Value, out var key, out var value);
		hashTable[key] = value;
		byte result = 0;
		if (byte.TryParse(key, out result))
		{
			CustomProperty key2 = (CustomProperty)result;
			m_EncodedKeys[key2] = text;
		}
		return key;
	}

	private void OnBecameMasterClient()
	{
		try
		{
			Instance.SetCustomProperty(CustomProperty.HostName, Platform.GetInstance().GetPrimaryUserName().ToLowerInvariant());
		}
		catch (Exception ex)
		{
			Debug.LogError("Error changing host name - " + ex.ToString());
		}
	}
}
