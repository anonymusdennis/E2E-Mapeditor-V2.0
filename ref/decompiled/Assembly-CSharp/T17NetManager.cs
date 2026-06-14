using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using ExitGames.Client.Photon;
using UnityEngine;

public class T17NetManager : T17NetSendMonoMessageTarget
{
	public delegate void OnBecameMasterClientEventHandler();

	public delegate void OnNewMasterClientEventHandler(PhotonPlayer newMasterClient);

	public delegate void OnPhotonConnectionChangeHandler(bool isConnected);

	public delegate void OnJoinedRoomHandler(short result);

	public delegate void OnCreatedRoomHandler(bool result);

	public delegate void OnFriendsListObtainedHandler();

	public delegate void OnLobbyHandler(bool isInLobby);

	public delegate void OnJoinedRoomPlatformHandler(string photonRoom, Platform.SessionType sessionType, bool isMaster, bool bInvitesEnabled, PrisonConfig.ConfigType configType);

	public delegate void OnPhotonCustomRoomPropertiesChangedHandler(Hashtable propertiesThatChanged);

	public delegate void OnLeftRoomHandler();

	public delegate void OnRoomListUpdatedHandler();

	public delegate void OnPhotonPlayerConnectionChangeHandler(PhotonPlayer newPlayer);

	public delegate void OnLobbyStatisticsUpdatedHandler();

	private enum T17NetDisconnectCause
	{
		[NetDisconnectCause("DISCONNECT_CAUSE_EXCEPTIONONCONNECT")]
		ExceptionOnConnect = 1023,
		[NetDisconnectCause("DISCONNECT_CAUSE_SECURITYEXCEPTIONONCONNECT")]
		SecurityExceptionOnConnect = 1022,
		[NetDisconnectCause("DISCONNECT_CAUSE_DISCONNECTBYCLIENTTIMEOUT")]
		DisconnectByClientTimeout = 1040,
		[NetDisconnectCause("DISCONNECT_CAUSE_INTERNALRECEIVEEXCEPTION")]
		InternalReceiveException = 1039,
		[NetDisconnectCause("DISCONNECT_CAUSE_DISCONNECTBYSERVERTIMEOUT")]
		DisconnectByServerTimeout = 1041,
		[NetDisconnectCause("DISCONNECT_CAUSE_DISCONNECTBYSERVERLOGIC")]
		DisconnectByServerLogic = 1043,
		[NetDisconnectCause("DISCONNECT_CAUSE_DISCONNECTBYSERVERUSERLIMIT")]
		DisconnectByServerUserLimit = 1042,
		[NetDisconnectCause("DISCONNECT_CAUSE_EXCEPTION")]
		Exception = 1026,
		[NetDisconnectCause("DISCONNECT_CAUSE_INVALIDREGION")]
		InvalidRegion = 32756,
		[NetDisconnectCause("DISCONNECT_CAUSE_MAXCCUREACHED")]
		MaxCcuReached = 32757,
		[NetDisconnectCause("DISCONNECT_CAUSE_INVALIDAUTHENTICATION")]
		InvalidAuthentication = 32767,
		[NetDisconnectCause("DISCONNECT_CAUSE_AUTHENTICATIONTICKETEXPIRED")]
		AuthenticationTicketExpired = 32753
	}

	private class NetDisconnectCauseAttribute : Attribute
	{
		public string LocalisationID { get; set; }

		public NetDisconnectCauseAttribute(string localisationID)
		{
			LocalisationID = localisationID;
		}

		private NetDisconnectCauseAttribute()
		{
			throw new NotImplementedException();
		}
	}

	private static int m_LoadingTimePingInterval = 10000;

	private static int m_ConnectingTimePingInterval = 1000;

	private static T17NetConnectState m_netConnectionState = T17NetConnectState.Disconnected;

	private static T17NetPeerState m_netConnectionPeerState = T17NetPeerState.Uninitialized;

	private static T17NetManager m_instance = null;

	public static NetConnectionState m_DefaultConnectionState = NetConnectionState.OnlineMode_Idle;

	public const int INVALID_PLAYERID = -1;

	public const int INVALID_NETVIEWID = -1;

	public static bool IsConnectedToGameServer = false;

	public static bool IsConnectedToGameServerAndReady = false;

	public static bool IsMasterClient = true;

	public static int MasterClientID = -1;

	public static float RealTime;

	public static bool NetOnlineMode = true;

	public static bool NetOfflineMode = false;

	public static readonly byte[] memRPCWrapper = new byte[8192];

	public static bool CanDisplayDisconnectionDialog { get; set; }

	public static bool SilentErrorDialogMode { get; set; }

	public static T17NetManager Instance
	{
		get
		{
			if (!(m_instance == null) || !GlobalStart.IsApplicationQuitting)
			{
			}
			return m_instance;
		}
	}

	public static T17NetConnectState ConnectionState => (T17NetConnectState)PhotonNetwork.connectionState;

	public static T17NetPeerState ConnectionDetailed => (T17NetPeerState)PhotonNetwork.connectionStateDetailed;

	public static bool ConnectedAndReady => PhotonNetwork.connectedAndReady;

	public static bool OfflineMode => PhotonNetwork.offlineMode;

	public static bool AutoJoinLobby
	{
		get
		{
			return PhotonNetwork.autoJoinLobby;
		}
		set
		{
			PhotonNetwork.autoJoinLobby = value;
		}
	}

	public static bool AutomaticallySyncScene
	{
		get
		{
			return PhotonNetwork.automaticallySyncScene;
		}
		set
		{
			PhotonNetwork.automaticallySyncScene = value;
		}
	}

	public static bool IsConnectedToLobbyServer => PhotonNetwork.insideLobby;

	public static bool IsConnectedToNameAndMasterServer
	{
		get
		{
			if (ConnectionDetailed == T17NetPeerState.JoinedLobby || ConnectionDetailed == T17NetPeerState.ConnectedToNameServer || ConnectionDetailed == T17NetPeerState.ConnectedToMaster)
			{
				return true;
			}
			return false;
		}
	}

	public static int PhotonPlayerID
	{
		get
		{
			if (PhotonNetwork.player != null)
			{
				return PhotonNetwork.player.ID;
			}
			return -1;
		}
	}

	public static int CountOfPlayersOnMaster => PhotonNetwork.networkingPeer.PlayersOnMasterCount;

	public static int CountOfPlayersInRooms => PhotonNetwork.networkingPeer.PlayersInRoomsCount;

	public static int GetCountOfPlayersInOurRoom => PhotonNetwork.playerList.Length;

	public static int CountOfPlayersOnServer => PhotonNetwork.networkingPeer.PlayersInRoomsCount + PhotonNetwork.networkingPeer.PlayersOnMasterCount;

	public static float PingInSeconds => (float)PhotonNetwork.GetPing() * 0.001f;

	public static double NetworkTime => PhotonNetwork.time;

	public static int NetworkTickCount => SupportClass.GetTickCount();

	public static float NetServerTime => (float)PhotonNetwork.time;

	public static int ServerTimestamp => PhotonNetwork.ServerTimestamp;

	public static event OnBecameMasterClientEventHandler OnBecameMasterClient;

	public static event OnNewMasterClientEventHandler OnNewMasterClient;

	public static event OnPhotonConnectionChangeHandler OnPhotonConnectionChangeEvent;

	public static event OnJoinedRoomHandler OnJoinedRoomEvent;

	public static event OnCreatedRoomHandler OnCreatedRoomEvent;

	public static event OnFriendsListObtainedHandler OnFriendsListUpdated;

	public static event OnJoinedRoomPlatformHandler OnJoinedRoomPlatformEvent;

	public static event OnLobbyHandler OnLobbyEvent;

	public static event OnPhotonCustomRoomPropertiesChangedHandler OnPhotonCustomRoomPropertiesChangedHandlerEvent;

	public static event OnLeftRoomHandler OnLeftRoomEvent;

	public static event OnRoomListUpdatedHandler OnRoomListUpdatedEvent;

	public static event OnPhotonPlayerConnectionChangeHandler OnPhotonPlayerConnectedEvent;

	public static event OnPhotonPlayerConnectionChangeHandler OnPhotonPlayerDisconnectedEvent;

	public static event OnLobbyStatisticsUpdatedHandler OnLobbyStatisticsUpdatedEvent;

	protected override void Awake()
	{
		base.Awake();
		if (m_instance != null)
		{
			throw new Exception("You cannot have multiple managers");
		}
		T17CustomTypes.Register();
		m_instance = this;
		UnityEngine.Object.DontDestroyOnLoad(this);
		SilentErrorDialogMode = false;
		CanDisplayDisconnectionDialog = false;
		PhotonPeer.RegisterType(typeof(T17NetView.RPCWrapper), 82, SerializeRPCWrapper, DeserializeRPCWrapper);
		PhotonNetwork.logLevel = PhotonLogLevel.ErrorsOnly;
		PhotonNetwork.MaxResendsBeforeDisconnect = 10;
		PhotonNetwork.sendRate = 30;
		PhotonNetwork.sendRateOnSerialize = 30;
		PhotonNetwork.networkingPeer.TimePingInterval = m_ConnectingTimePingInterval;
		PhotonNetwork.networkingPeer.DisconnectTimeout = 60000;
		PhotonNetwork.networkingPeer.SentCountAllowance = 10;
		PhotonNetwork.networkingPeer.MaximumTransferUnit = 1200;
		PhotonNetwork.networkingPeer.ChannelCount = 19;
		PhotonNetwork.networkingPeer.LimitOfUnreliableCommands = 0;
		NetworkingPeer.ObjectsInOneUpdate = 25;
		PhotonNetwork.UseRpcMonoBehaviourCache = true;
		T17NetEncryptionKeys.OnKeysRetrieved += OnEncryptionKeysInitialised;
	}

	protected virtual void OnDestroy()
	{
		T17NetEncryptionKeys.OnKeysRetrieved -= OnEncryptionKeysInitialised;
	}

	public static void LogGoogleException(string strLog)
	{
	}

	public static void LogGoogleException(string strLog, string userData)
	{
	}

	public static bool IsValidNetViewId(int netViewId)
	{
		return netViewId > 0;
	}

	public static void UpdateStatus()
	{
		if (ConnectionDetailed == T17NetPeerState.Joining || ConnectionDetailed == T17NetPeerState.Joined)
		{
			IsConnectedToGameServer = true;
			if (PhotonPlayerID > 0)
			{
				IsConnectedToGameServerAndReady = true;
			}
			else
			{
				IsConnectedToGameServerAndReady = false;
			}
		}
		else
		{
			IsConnectedToGameServer = false;
			IsConnectedToGameServerAndReady = false;
		}
		if (!PhotonNetwork.inRoom)
		{
			IsMasterClient = true;
		}
		else
		{
			IsMasterClient = PhotonNetwork.isMasterClient;
		}
		if (PhotonNetwork.masterClient != null)
		{
			MasterClientID = PhotonNetwork.masterClient.ID;
		}
		else
		{
			MasterClientID = -1;
		}
		NetOfflineMode = PhotonNetwork.offlineMode;
		NetOnlineMode = !NetOfflineMode;
	}

	public static bool AnyPlayersEscaped()
	{
		T17NetRoomGameView.EscapeState outValue = T17NetRoomGameView.EscapeState.Escaped;
		return T17NetRoomGameView.Instance != null && T17NetRoomGameView.GetCustomPropertyAsEnum(T17NetRoomGameView.CustomProperty.EscapeState, ref outValue) && outValue == T17NetRoomGameView.EscapeState.Escaped;
	}

	public void Update()
	{
		RealTime = Time.time;
		if (m_netConnectionState != (T17NetConnectState)PhotonNetwork.connectionState)
		{
			string text = ((!NetOfflineMode) ? "ONLINE " : "OFFLINE");
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetConnectionState))
			{
			}
			m_netConnectionState = (T17NetConnectState)PhotonNetwork.connectionState;
		}
		if (m_netConnectionPeerState != (T17NetPeerState)PhotonNetwork.connectionStateDetailed)
		{
			string text2 = ((!NetOfflineMode) ? "ONLINE " : "OFFLINE");
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetConnectionState))
			{
			}
			m_netConnectionPeerState = (T17NetPeerState)PhotonNetwork.connectionStateDetailed;
		}
		UpdateStatus();
		List<T17NetView> questionNetViews = T17NetView.QuestionNetViews;
		float realTime = RealTime;
		for (int i = 0; i < questionNetViews.Count; i++)
		{
			questionNetViews[i].ProcessQuestions(realTime);
		}
	}

	public static void Service()
	{
		if (PhotonNetwork.networkingPeer != null)
		{
			PhotonNetwork.networkingPeer.Service();
			if (!NetLoadSync.AllClientsReadyToPlay())
			{
				T17NetLoadSync.Instance.Update();
			}
		}
	}

	public static bool IsApplicationRunning()
	{
		return !PhotonHandler.AppQuits;
	}

	public void OnDisconnectedFromPhoton()
	{
		if (T17NetManager.OnPhotonConnectionChangeEvent != null)
		{
			T17NetManager.OnPhotonConnectionChangeEvent(isConnected: false);
		}
		if (NetOnlineMode && !NetGoOnlineHelper.IsActive)
		{
			NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, SilentErrorDialogMode);
		}
	}

	public void OnFailedToConnectToPhoton(DisconnectCause cause)
	{
		if (NetConnectAndJoinRoom.GetRequestedConnectionState() != 0 && !NetGoOnlineHelper.IsActive)
		{
			ErrorDialogHandler.ShowError(ErrorDialogHandler.Type.OnFailedToConnectToPhoton);
			NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
		}
		T17NetRoomGameView.Instance.ClearCustomProperties();
	}

	public void OnConnectionFail(DisconnectCause cause)
	{
	}

	public void OnConnectedToPhoton()
	{
		if (T17NetManager.OnPhotonConnectionChangeEvent != null)
		{
			T17NetManager.OnPhotonConnectionChangeEvent(isConnected: true);
		}
	}

	public static void CriticalMessageNeedToSendNow()
	{
		if (PhotonNetwork.networkingPeer != null)
		{
			Debug.LogFormat("CriticalMessageNeedToSendNow: Causing Message Flush!!!");
			PhotonNetwork.networkingPeer.SendOutgoingCommands();
		}
	}

	public static void StopNetworkMessageWhileLoading(string levelName)
	{
		PhotonNetwork.LoadLevel(levelName);
	}

	public static int AllocateSceneViewID()
	{
		return PhotonNetwork.AllocateSceneViewID();
	}

	public static void UnallocateSceneViewID(int viewID)
	{
		PhotonNetwork.UnAllocateViewID(viewID);
	}

	public static void CloseConnection(Gamer gamer)
	{
		PhotonPlayer[] playerList = PhotonNetwork.playerList;
		if (playerList == null)
		{
			return;
		}
		for (int num = playerList.Length - 1; num >= 0; num--)
		{
			if (playerList[num].ID == gamer.m_PhotonID)
			{
				PhotonNetwork.CloseConnection(playerList[num]);
			}
		}
	}

	public static int AllocateViewID()
	{
		return PhotonNetwork.AllocateViewID();
	}

	public static bool IsConnectedOnline()
	{
		return NetOnlineMode && PhotonNetwork.connectedAndReady;
	}

	public static string NetNpIdToString(byte[] npId)
	{
		if (npId == null)
		{
			return string.Empty;
		}
		if (npId.Length == 0)
		{
			return string.Empty;
		}
		int num = 0;
		foreach (byte b in npId)
		{
			num++;
			if (b == 0)
			{
				Debug.LogFormat("NetNpIdToString: Actual Size npId.Length = {0} @ actualSize = {1}", npId.Length, num);
				num--;
				break;
			}
		}
		byte[] array = new byte[num];
		int num2 = 0;
		foreach (byte b2 in npId)
		{
			if (b2 == 0)
			{
				break;
			}
			array[num2] = b2;
			num2++;
		}
		return Encoding.UTF8.GetString(array);
	}

	public static string GenerateMd5Sum(string strToEncrypt)
	{
		UTF8Encoding uTF8Encoding = new UTF8Encoding();
		byte[] bytes = uTF8Encoding.GetBytes(strToEncrypt);
		MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
		byte[] array = mD5CryptoServiceProvider.ComputeHash(bytes);
		string text = string.Empty;
		for (int i = 0; i < array.Length; i++)
		{
			text += Convert.ToString(array[i], 16).PadLeft(2, '0');
		}
		return text.PadLeft(32, '0');
	}

	public static string Utf16ToUtf8(string utf16String)
	{
		string text = string.Empty;
		byte[] bytes = Encoding.Unicode.GetBytes(utf16String);
		byte[] array = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, bytes);
		for (int i = 0; i < array.Length; i++)
		{
			byte[] value = new byte[2]
			{
				array[i],
				0
			};
			text += BitConverter.ToChar(value, 0);
		}
		return text;
	}

	public void OnPhotonMaxCccuReached()
	{
		UpdateStatus();
	}

	public void OnMasterClientSwitched(PhotonPlayer newMasterClient)
	{
		Debug.Log(" *************    OnMasterClientSwitched   *******************   ");
		if (newMasterClient != null)
		{
			Debug.LogError("OnMasterClientSwitched: Master client is ID " + newMasterClient.ID + " local: " + newMasterClient.IsLocal.ToString() + " " + newMasterClient.NickName);
		}
		if (GlobalStart.GetInstance() != null && !GlobalStart.GetInstance().IsWithinLevel())
		{
			Gamer.DeleteRemoteGamers();
		}
		UpdateStatus();
		if (T17NetEncryptionKeys.HaveEncryptionKey())
		{
			if (newMasterClient == PhotonNetwork.player && !Platform.GetInstance().WasMasterClientPreviously() && T17NetManager.OnBecameMasterClient != null)
			{
				T17NetManager.OnBecameMasterClient();
			}
			if ((newMasterClient != PhotonNetwork.player || !Platform.GetInstance().WasMasterClientPreviously()) && T17NetManager.OnNewMasterClient != null)
			{
				T17NetManager.OnNewMasterClient(newMasterClient);
			}
		}
	}

	public virtual void OnLeftRoom()
	{
		UpdateStatus();
		if (T17NetManager.OnLeftRoomEvent != null)
		{
			T17NetManager.OnLeftRoomEvent();
		}
	}

	public virtual void OnPhotonCreateRoomFailed(object[] codeAndMsg)
	{
		UpdateStatus();
		if (T17NetManager.OnCreatedRoomEvent != null)
		{
			T17NetManager.OnCreatedRoomEvent(result: false);
		}
	}

	public virtual void OnPhotonJoinRoomFailed(object[] codeAndMsg)
	{
		UpdateStatus();
		if (T17NetManager.OnJoinedRoomEvent != null)
		{
			if (codeAndMsg != null && codeAndMsg.Length > 0)
			{
				T17NetManager.OnJoinedRoomEvent((short)codeAndMsg[0]);
			}
			else
			{
				T17NetManager.OnJoinedRoomEvent(-1);
			}
		}
	}

	public virtual void OnCreatedRoom()
	{
		UpdateStatus();
		if (T17NetManager.OnCreatedRoomEvent != null)
		{
			T17NetManager.OnCreatedRoomEvent(result: true);
		}
	}

	public virtual void OnJoinedLobby()
	{
		Gamer.DeleteRemoteGamers();
		UpdateStatus();
		if (T17NetManager.OnLobbyEvent != null)
		{
			T17NetManager.OnLobbyEvent(isInLobby: true);
		}
	}

	public virtual void OnLeftLobby()
	{
		UpdateStatus();
		if (T17NetManager.OnLobbyEvent != null)
		{
			T17NetManager.OnLobbyEvent(isInLobby: false);
		}
	}

	public virtual void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		UpdateStatus();
	}

	public virtual void OnReceivedRoomListUpdate()
	{
		UpdateStatus();
		if (T17NetManager.OnRoomListUpdatedEvent != null)
		{
			T17NetManager.OnRoomListUpdatedEvent();
		}
	}

	public void OnEncryptionKeysInitialised()
	{
		PhotonNetwork.isMessageQueueRunning = true;
		if (!Helpers.IsInGameplayScene())
		{
			NetworkingPeer.m_bDiscardSerializeOnViews = true;
			PhotonNetwork.SetReceivingEnabled(5, enabled: false);
			PhotonNetwork.SetSendingEnabled(5, enabled: false);
		}
		if (T17NetManager.OnJoinedRoomEvent != null)
		{
			T17NetManager.OnJoinedRoomEvent(0);
		}
		if (!IsConnectedOnline() || T17NetManager.OnJoinedRoomPlatformEvent == null)
		{
			return;
		}
		Platform.SessionType sessionType = Platform.SessionType.SESSION_TYPE_PUBLIC;
		sessionType = T17NetRoomManager.CurrentGameRoomType switch
		{
			T17NetRoomGameView.GameRoomType.Public => Platform.SessionType.SESSION_TYPE_PUBLIC, 
			T17NetRoomGameView.GameRoomType.Offline => Platform.SessionType.SESSION_TYPE_PRIVATE, 
			_ => Platform.SessionType.SESSION_TYPE_PRIVATE, 
		};
		bool bInvitesEnabled = true;
		T17NetRoomGameView.GameRoomType outValue = T17NetRoomGameView.GameRoomType.Undefined;
		PrisonConfig.ConfigType outValue2 = PrisonConfig.ConfigType.Cooperative;
		if (T17NetRoomGameView.GetCustomPropertyAsEnum(T17NetRoomGameView.CustomProperty.RoomType, ref outValue) && T17NetRoomGameView.GetCustomPropertyAsEnum(T17NetRoomGameView.CustomProperty.ConfigType, ref outValue2) && (outValue2 == PrisonConfig.ConfigType.Singleplayer || (outValue == T17NetRoomGameView.GameRoomType.Public && outValue2 == PrisonConfig.ConfigType.Versus)))
		{
			bInvitesEnabled = false;
		}
		T17NetInvites.InviteData inviteData = new T17NetInvites.InviteData();
		inviteData.n = PhotonNetwork.room.Name;
		inviteData.l = (int)PhotonNetwork.networkingPeer.CloudRegion;
		if (IsMasterClient)
		{
			PrisonData.LevelInfo currentLevelInfo = LevelScript.GetCurrentLevelInfo();
			if (currentLevelInfo != null)
			{
				inviteData.m = (int)currentLevelInfo.m_PrisonEnum;
			}
			else
			{
				inviteData.m = (int)GlobalStart.GetInstance().GetCurrentSelectedPrisonEnum();
			}
		}
		else
		{
			LevelScript.PRISON_ENUM outValue3 = LevelScript.PRISON_ENUM.Unassigned;
			T17NetRoomGameView.GetCustomPropertyAsEnum(T17NetRoomGameView.CustomProperty.PrisonEnum, ref outValue3);
			inviteData.m = (int)outValue3;
		}
		string photonRoom = JsonUtility.ToJson(inviteData);
		T17NetManager.OnJoinedRoomPlatformEvent(photonRoom, sessionType, IsMasterClient, bInvitesEnabled, outValue2);
	}

	public virtual void OnKeyRetrievalError()
	{
		NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
		PhotonNetwork.isMessageQueueRunning = true;
		if (T17NetManager.OnJoinedRoomEvent != null)
		{
			T17NetManager.OnJoinedRoomEvent(32755);
		}
	}

	public virtual void OnJoinedRoom()
	{
		UpdateStatus();
		T17NetEncryptionKeys.Instance.OnEnteredNewSession(PhotonNetwork.room.Name);
	}

	public virtual void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
	{
		UpdateStatus();
		if (T17NetManager.OnPhotonPlayerConnectedEvent != null)
		{
			T17NetManager.OnPhotonPlayerConnectedEvent(newPlayer);
		}
		NetBluePrintDetails.Instance.ForcePushBluePrint();
		NetPrisonViewDetails.Instance.ForcePushPrisonView();
		if (IsMasterClient)
		{
			LevelDataManager.GetInstance().NotifyClientToSelectPlaylistRPC(newPlayer, GlobalStart.GetInstance().GetCurrentPlaylistData());
		}
	}

	public virtual void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
	{
		UpdateStatus();
		if (T17NetManager.OnPhotonPlayerDisconnectedEvent != null)
		{
			T17NetManager.OnPhotonPlayerDisconnectedEvent(otherPlayer);
		}
		Gamer.DeleteGamerByPhotonPlayerID(otherPlayer.ID);
	}

	public virtual void OnPhotonRandomJoinFailed(object[] codeAndMsg)
	{
		UpdateStatus();
	}

	public virtual void OnConnectedToMaster()
	{
		UpdateStatus();
		if (NetOnlineMode)
		{
			CanDisplayDisconnectionDialog = true;
		}
	}

	public virtual void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged)
	{
		UpdateStatus();
		if (T17NetManager.OnPhotonCustomRoomPropertiesChangedHandlerEvent != null && T17NetRoomManager.IsInRoom())
		{
			T17NetManager.OnPhotonCustomRoomPropertiesChangedHandlerEvent(propertiesThatChanged);
		}
	}

	public virtual void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
	{
		UpdateStatus();
	}

	public virtual void OnUpdatedFriendList()
	{
		UpdateStatus();
		if (T17NetManager.OnFriendsListUpdated != null)
		{
			T17NetManager.OnFriendsListUpdated();
		}
	}

	public virtual void OnCustomAuthenticationFailed(string debugMessage)
	{
		UpdateStatus();
	}

	public virtual void OnCustomAuthenticationResponse(Dictionary<string, object> data)
	{
		UpdateStatus();
	}

	public virtual void OnWebRpcResponse(OperationResponse response)
	{
		UpdateStatus();
	}

	public virtual void OnOwnershipRequest(object[] viewAndPlayer)
	{
		UpdateStatus();
	}

	public virtual void OnLobbyStatisticsUpdate()
	{
		UpdateStatus();
		if (T17NetManager.OnLobbyStatisticsUpdatedEvent != null)
		{
			T17NetManager.OnLobbyStatisticsUpdatedEvent();
		}
	}

	private static short SerializeRPCWrapper(StreamBuffer outStream, object customobject)
	{
		T17NetView.RPCWrapper rPCWrapper = (T17NetView.RPCWrapper)customobject;
		int num = 0;
		lock (memRPCWrapper)
		{
			byte[] array = memRPCWrapper;
			int targetOffset = 0;
			byte[] array2 = Protocol.Serialize(rPCWrapper.MethodName);
			byte[] array3 = Protocol.Serialize(rPCWrapper.Parameters);
			Protocol.Serialize(rPCWrapper.ID, array, ref targetOffset);
			Protocol.Serialize(array2.Length, array, ref targetOffset);
			Protocol.Serialize(array3.Length, array, ref targetOffset);
			if (array2.Length > 0)
			{
				Array.Copy(array2, 0, array, targetOffset, array2.Length);
				targetOffset += array2.Length;
			}
			if (array3.Length > 0)
			{
				Array.Copy(array3, 0, array, targetOffset, array3.Length);
				targetOffset += array3.Length;
			}
			num = targetOffset;
			outStream.Write(array, 0, num);
		}
		return (short)num;
	}

	private static object DeserializeRPCWrapper(StreamBuffer inStream, short length)
	{
		T17NetView.RPCWrapper rPCWrapper = new T17NetView.RPCWrapper();
		lock (memRPCWrapper)
		{
			int offset = 0;
			int value = 0;
			int value2 = 0;
			inStream.Read(memRPCWrapper, 0, length);
			Protocol.Deserialize(out rPCWrapper.ID, memRPCWrapper, ref offset);
			Protocol.Deserialize(out value, memRPCWrapper, ref offset);
			Protocol.Deserialize(out value2, memRPCWrapper, ref offset);
			if (value > 0)
			{
				byte[] array = new byte[value];
				Array.Copy(memRPCWrapper, offset, array, 0, value);
				rPCWrapper.MethodName = (string)Protocol.Deserialize(array);
				offset += value;
			}
			if (value2 > 0)
			{
				byte[] array2 = new byte[value2];
				Array.Copy(memRPCWrapper, offset, array2, 0, value2);
				rPCWrapper.Parameters = (object[])Protocol.Deserialize(array2);
				offset += value2;
			}
		}
		return rPCWrapper;
	}

	public static void SetTimePingIntervalToConnectingRate()
	{
		SetTimePingInterval(m_ConnectingTimePingInterval);
	}

	public static void SetTimePingIntervalToLoadingRate()
	{
		SetTimePingInterval(m_LoadingTimePingInterval);
	}

	private static void SetTimePingInterval(int time)
	{
		PhotonNetwork.networkingPeer.TimePingInterval = time;
		Debug.Log("T17PhotonNetworking: Setting TimePingInterval to " + time + "ms");
	}
}
