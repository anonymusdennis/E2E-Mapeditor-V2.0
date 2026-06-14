using System;
using System.Text;
using Steamworks;
using UnityEngine;

public class SteamMatchmaking : MonoBehaviour
{
	private class LobbyData
	{
		public enum Status
		{
			NotJoined,
			Joining,
			Joined
		}

		private Status m_JoinedStatus;

		private string m_PhotonRoom;

		private bool m_IsMaster;

		private Platform.SessionType m_SessionType;

		private bool m_InvitesEnabled;

		private CSteamID m_LobbyId;

		private CSteamID m_FriendToJoinID;

		public bool joined
		{
			get
			{
				return m_JoinedStatus == Status.Joined;
			}
			set
			{
				m_JoinedStatus = (value ? Status.Joined : Status.NotJoined);
			}
		}

		public CSteamID lobbyId
		{
			get
			{
				return m_LobbyId;
			}
			set
			{
				m_LobbyId = value;
			}
		}

		public Status joinedStatus => m_JoinedStatus;

		public string photonRoom
		{
			get
			{
				return m_PhotonRoom;
			}
			set
			{
				m_PhotonRoom = value;
			}
		}

		public bool isMaster => m_IsMaster;

		public Platform.SessionType sessionType
		{
			get
			{
				return m_SessionType;
			}
			set
			{
				m_SessionType = value;
			}
		}

		public CSteamID friendId
		{
			get
			{
				return m_FriendToJoinID;
			}
			set
			{
				m_FriendToJoinID = value;
			}
		}

		public LobbyData()
		{
			Reset();
		}

		public void Reset(bool bClearLobbyId = true)
		{
			m_JoinedStatus = Status.NotJoined;
			if (bClearLobbyId)
			{
				m_LobbyId.Clear();
				m_FriendToJoinID.Clear();
			}
		}

		public void SetToJoining(string photonRoom, Platform.SessionType sessionType, bool isMaster, bool invitesEnabled)
		{
			m_JoinedStatus = Status.Joining;
			m_PhotonRoom = photonRoom;
			m_SessionType = sessionType;
			m_IsMaster = isMaster;
			m_InvitesEnabled = invitesEnabled;
			m_LobbyId.Clear();
		}

		public void SetToJoining(string photonRoom, Platform.SessionType sessionType, bool isMaster, bool invitesEnabled, CSteamID lobbyId)
		{
			SetToJoining(photonRoom, sessionType, isMaster, invitesEnabled);
			m_LobbyId = lobbyId;
		}

		public void SetToJoined(CSteamID lobbyId)
		{
			if (m_JoinedStatus != Status.Joined)
			{
				m_LobbyId = lobbyId;
				m_JoinedStatus = Status.Joined;
			}
		}

		public void Leave()
		{
			Reset(bClearLobbyId: false);
		}
	}

	private const string LOBBYDATAKEY_PHOTON_ROOM = "PhotonRoom";

	private LobbyData m_CurrentLobbyData = new LobbyData();

	private CSteamID m_AspiringLobbyId;

	private Callback<LobbyEnter_t> m_LobbyEnteredCallback;

	private Callback<LobbyDataUpdate_t> m_LobbyDataUpdateCallback;

	private Callback<LobbyKicked_t> m_LobbyKickedCallback;

	private Callback<LobbyChatUpdate_t> m_LobbyUpdateCallback;

	private Callback<LobbyInvite_t> m_LobbyInviteCallback;

	private Callback<GameLobbyJoinRequested_t> m_LobbyJoinRequestedCallback;

	private Callback<GameRichPresenceJoinRequested_t> m_RichPresenceJoinRequested;

	private CallResult<LobbyCreated_t> m_LobbyCreatedResult;

	private CallResult<LobbyEnter_t> m_LobbyEnterResult;

	private CallResult<LobbyMatchList_t> m_LobbyMatchResult;

	private static bool m_CheckedForBootInvite;

	private CSteamID m_PendingLobbyInvite;

	private bool m_bIsJoiningSessionAsMaster;

	protected CrossplayLobbyManager m_CrossplayLobbyManager;

	private void Start()
	{
		m_LobbyEnteredCallback = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
		m_LobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
		m_LobbyKickedCallback = Callback<LobbyKicked_t>.Create(OnLobbyKicked);
		m_LobbyUpdateCallback = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
		m_LobbyInviteCallback = Callback<LobbyInvite_t>.Create(OnLobbyInvite);
		m_LobbyJoinRequestedCallback = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequested);
		m_RichPresenceJoinRequested = Callback<GameRichPresenceJoinRequested_t>.Create(OnRichPresenceJoinRequested);
		m_LobbyCreatedResult = CallResult<LobbyCreated_t>.Create(OnLobbyCreated);
		m_LobbyEnterResult = CallResult<LobbyEnter_t>.Create(OnLobbyEnter);
		m_LobbyMatchResult = CallResult<LobbyMatchList_t>.Create(OnLobbyMatch);
		T17NetManager.OnJoinedRoomPlatformEvent += OnJoinedSession;
		T17NetManager.OnLeftRoomEvent += LeaveSession;
		m_CrossplayLobbyManager = base.gameObject.GetComponent<CrossplayLobbyManager>();
	}

	private void Update()
	{
		if (!m_CheckedForBootInvite && GlobalStart.GetInstance() != null && GlobalStart.GetInstance().CurrentGlobalStartMode == GlobalStart.GLOBALSTART_MODE.SHOW_FRONTEND)
		{
			CheckForBootInvite();
			m_CheckedForBootInvite = true;
		}
	}

	protected virtual void OnDestroy()
	{
		T17NetManager.OnJoinedRoomPlatformEvent -= OnJoinedSession;
		T17NetManager.OnLeftRoomEvent -= LeaveSession;
	}

	public void OnJoinedSession(string photonRoom, Platform.SessionType sessionType, bool isMaster, bool invitesEnabled, PrisonConfig.ConfigType configType)
	{
		m_bIsJoiningSessionAsMaster = isMaster;
		if (isMaster)
		{
			m_CurrentLobbyData.SetToJoining(photonRoom, sessionType, isMaster, invitesEnabled);
			ELobbyType eLobbyType = ELobbyType.k_ELobbyTypePrivate;
			eLobbyType = ((sessionType != Platform.SessionType.SESSION_TYPE_PRIVATE) ? (invitesEnabled ? ELobbyType.k_ELobbyTypePublic : ELobbyType.k_ELobbyTypePrivate) : ELobbyType.k_ELobbyTypePrivate);
			SteamAPICall_t hAPICall = Steamworks.SteamMatchmaking.CreateLobby(eLobbyType, 250);
			m_LobbyCreatedResult.Set(hAPICall);
			return;
		}
		if (!m_CurrentLobbyData.lobbyId.IsValid() && !m_AspiringLobbyId.IsValid())
		{
			m_CurrentLobbyData.SetToJoining(photonRoom, sessionType, isMaster, invitesEnabled);
			Steamworks.SteamMatchmaking.AddRequestLobbyListStringFilter("PhotonRoom", Base64Encode(photonRoom), ELobbyComparison.k_ELobbyComparisonEqual);
			SteamAPICall_t hAPICall2 = Steamworks.SteamMatchmaking.RequestLobbyList();
			m_LobbyMatchResult.Set(hAPICall2);
		}
		else
		{
			CSteamID cSteamID = ((!m_AspiringLobbyId.IsValid()) ? m_CurrentLobbyData.lobbyId : m_AspiringLobbyId);
			m_CurrentLobbyData.SetToJoining(photonRoom, sessionType, isMaster, invitesEnabled, cSteamID);
			SteamAPICall_t hAPICall3 = Steamworks.SteamMatchmaking.JoinLobby(cSteamID);
			m_LobbyEnterResult.Set(hAPICall3);
		}
		m_CrossplayLobbyManager.RegisterLobbyMemberRPC();
	}

	public void JoinPlatformLobby()
	{
		Steamworks.SteamMatchmaking.AddRequestLobbyListStringFilter("PhotonRoom", Base64Encode(m_CurrentLobbyData.photonRoom), ELobbyComparison.k_ELobbyComparisonEqual);
		SteamAPICall_t hAPICall = Steamworks.SteamMatchmaking.RequestLobbyList();
		m_LobbyMatchResult.Set(hAPICall);
	}

	public void LeaveSession()
	{
		if (m_CurrentLobbyData.joined)
		{
			Steamworks.SteamMatchmaking.LeaveLobby(m_CurrentLobbyData.lobbyId);
			m_CurrentLobbyData.Leave();
			m_CurrentLobbyData.Reset();
			m_CrossplayLobbyManager.OnSessionLeft();
		}
	}

	public void OpenInviteOverlay()
	{
		if (m_CurrentLobbyData != null && m_CurrentLobbyData.joined)
		{
			Steamworks.SteamFriends.ActivateGameOverlayInviteDialog(m_CurrentLobbyData.lobbyId);
		}
	}

	public void OnSessionTypeChanged(Platform.SessionType sessionType)
	{
		if (m_CurrentLobbyData.joined)
		{
			ELobbyType eLobbyType = ((sessionType != Platform.SessionType.SESSION_TYPE_PRIVATE) ? ELobbyType.k_ELobbyTypePublic : ELobbyType.k_ELobbyTypePrivate);
			if (Steamworks.SteamMatchmaking.SetLobbyType(m_CurrentLobbyData.lobbyId, eLobbyType))
			{
			}
		}
	}

	public void SendInvite(string OnlineID)
	{
		if (m_CurrentLobbyData.joined)
		{
			ulong result = 0uL;
			if (ulong.TryParse(OnlineID, out result))
			{
				string textToEncode = m_CurrentLobbyData.lobbyId.ToString() + "+" + m_CurrentLobbyData.photonRoom;
				Steamworks.SteamFriends.InviteUserToGame(new CSteamID(result), Base64EncodeUrl(textToEncode));
			}
		}
	}

	private void CheckForBootInvite()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		ulong result = 0uL;
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (!(commandLineArgs[i] == "+connect_lobby") || i + 1 >= commandLineArgs.Length || string.IsNullOrEmpty(commandLineArgs[i + 1]))
			{
				continue;
			}
			string s = commandLineArgs[i + 1];
			if (ulong.TryParse(s, out result))
			{
				CSteamID cSteamID = new CSteamID(result);
				if (cSteamID.IsValid() && Steamworks.SteamMatchmaking.RequestLobbyData(cSteamID))
				{
					m_PendingLobbyInvite = cSteamID;
					return;
				}
			}
		}
		string text = null;
		if (commandLineArgs.Length <= 1)
		{
			return;
		}
		text = commandLineArgs[1];
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		try
		{
			string text2 = Base64DecodeUrl(text);
			int num = text2.IndexOf('+');
			if (num > 0)
			{
				string s2 = text2.Substring(0, num);
				m_CurrentLobbyData.lobbyId = new CSteamID(ulong.Parse(s2));
				string roomInfoJSON = text2.Substring(num + 1);
				T17NetInvites.InviteReceived(roomInfoJSON);
			}
		}
		catch (Exception)
		{
		}
	}

	private string Base64Encode(string textToEncode)
	{
		try
		{
			byte[] bytes = Encoding.UTF8.GetBytes(textToEncode);
			return Convert.ToBase64String(bytes);
		}
		catch
		{
			return textToEncode;
		}
	}

	private string Base64Decode(string encodedText)
	{
		try
		{
			byte[] bytes = Convert.FromBase64String(encodedText);
			return Encoding.UTF8.GetString(bytes);
		}
		catch
		{
			return encodedText;
		}
	}

	private string Base64EncodeUrl(string textToEncode)
	{
		string text = Base64Encode(textToEncode);
		return text.Replace('/', '_');
	}

	private string Base64DecodeUrl(string textToEncode)
	{
		textToEncode = textToEncode.Trim('/');
		textToEncode = textToEncode.Replace('_', '/');
		return Base64Decode(textToEncode);
	}

	private void OnLobbyCreated(LobbyCreated_t pCallback, bool bFailure)
	{
		if (bFailure || pCallback.m_eResult != EResult.k_EResultOK || pCallback.m_ulSteamIDLobby == 0)
		{
			m_CurrentLobbyData.Reset();
		}
		else
		{
			m_CurrentLobbyData.SetToJoined((CSteamID)pCallback.m_ulSteamIDLobby);
			Steamworks.SteamMatchmaking.SetLobbyData(m_CurrentLobbyData.lobbyId, "PhotonRoom", Base64Encode(m_CurrentLobbyData.photonRoom));
		}
		m_AspiringLobbyId.Clear();
		m_CrossplayLobbyManager.OnPlatformLobbyMade(!bFailure);
	}

	private void OnLobbyEnter(LobbyEnter_t pCallback)
	{
		ClearSteamIdIfMatching(ref m_AspiringLobbyId, pCallback.m_ulSteamIDLobby);
	}

	private void OnLobbyEnter(LobbyEnter_t pCallback, bool bFailure)
	{
		if (!bFailure || !m_CrossplayLobbyManager.IsWaitingForPlatformLobby() || m_bIsJoiningSessionAsMaster)
		{
			m_CurrentLobbyData.SetToJoined((CSteamID)pCallback.m_ulSteamIDLobby);
			ClearSteamIdIfMatching(ref m_AspiringLobbyId, pCallback.m_ulSteamIDLobby);
		}
	}

	private void OnLobbyDataUpdate(LobbyDataUpdate_t pCallback)
	{
		CSteamID cSteamID = new CSteamID(pCallback.m_ulSteamIDLobby);
		if (!m_PendingLobbyInvite.IsValid() || !(cSteamID == m_PendingLobbyInvite))
		{
			return;
		}
		if (pCallback.m_bSuccess != 0)
		{
			string text = Base64Decode(Steamworks.SteamMatchmaking.GetLobbyData(cSteamID, "PhotonRoom"));
			if (!string.IsNullOrEmpty(text))
			{
				m_AspiringLobbyId = cSteamID;
				T17NetInvites.InviteReceived(text);
			}
		}
		m_PendingLobbyInvite.Clear();
	}

	private void OnLobbyMatch(LobbyMatchList_t pCallback, bool bFailure)
	{
		if (bFailure || pCallback.m_nLobbiesMatching == 0)
		{
			if (m_CurrentLobbyData.friendId.IsValid())
			{
				if (Steamworks.SteamFriends.GetFriendGamePlayed(m_CurrentLobbyData.friendId, out var pFriendGameInfo))
				{
					SteamAPICall_t hAPICall = Steamworks.SteamMatchmaking.JoinLobby(pFriendGameInfo.m_steamIDLobby);
					m_LobbyEnterResult.Set(hAPICall);
				}
				else if (!m_CrossplayLobbyManager.IsWaitingForPlatformLobby() || m_bIsJoiningSessionAsMaster)
				{
					m_CurrentLobbyData.Reset();
				}
			}
			else if (!m_CrossplayLobbyManager.IsWaitingForPlatformLobby() || m_bIsJoiningSessionAsMaster)
			{
				m_CurrentLobbyData.Reset();
			}
		}
		else
		{
			CSteamID lobbyByIndex = Steamworks.SteamMatchmaking.GetLobbyByIndex(0);
			if (!lobbyByIndex.IsValid())
			{
				m_CurrentLobbyData.Reset();
				return;
			}
			SteamAPICall_t hAPICall2 = Steamworks.SteamMatchmaking.JoinLobby(lobbyByIndex);
			m_LobbyEnterResult.Set(hAPICall2);
		}
	}

	private void OnLobbyKicked(LobbyKicked_t pCallback)
	{
		if ((CSteamID)pCallback.m_ulSteamIDLobby == m_CurrentLobbyData.lobbyId)
		{
			m_CurrentLobbyData.Leave();
			m_CrossplayLobbyManager.OnSessionLeft();
		}
	}

	private void OnLobbyChatUpdate(LobbyChatUpdate_t pCallback)
	{
		if ((CSteamID)pCallback.m_ulSteamIDLobby == m_CurrentLobbyData.lobbyId && (CSteamID)pCallback.m_ulSteamIDUserChanged == SteamUser.GetSteamID())
		{
			EChatMemberStateChange rgfChatMemberStateChange = (EChatMemberStateChange)pCallback.m_rgfChatMemberStateChange;
			if (rgfChatMemberStateChange == EChatMemberStateChange.k_EChatMemberStateChangeEntered)
			{
				m_CurrentLobbyData.SetToJoined((CSteamID)pCallback.m_ulSteamIDLobby);
				return;
			}
			m_CurrentLobbyData.Leave();
			m_CrossplayLobbyManager.OnSessionLeft();
		}
	}

	private void OnLobbyInvite(LobbyInvite_t pCallback)
	{
	}

	private void OnLobbyJoinRequested(GameLobbyJoinRequested_t pCallback)
	{
		m_bIsJoiningSessionAsMaster = false;
		CSteamID steamIDLobby = pCallback.m_steamIDLobby;
		if (steamIDLobby.IsValid() && Steamworks.SteamMatchmaking.RequestLobbyData(steamIDLobby))
		{
			m_PendingLobbyInvite = steamIDLobby;
		}
	}

	private void OnRichPresenceJoinRequested(GameRichPresenceJoinRequested_t pCallback)
	{
		if (!string.IsNullOrEmpty(pCallback.m_rgchConnect))
		{
			m_CurrentLobbyData.friendId = pCallback.m_steamIDFriend;
			string text = Base64DecodeUrl(pCallback.m_rgchConnect);
			int num = text.IndexOf('+');
			if (num > 0)
			{
				string s = text.Substring(0, num);
				m_AspiringLobbyId = new CSteamID(ulong.Parse(s));
				string roomInfoJSON = text.Substring(num + 1);
				T17NetInvites.InviteReceived(roomInfoJSON);
			}
		}
	}

	private void ClearSteamIdIfMatching(ref CSteamID steamId, ulong comparison)
	{
		if (steamId.m_SteamID == comparison)
		{
			steamId.Clear();
		}
	}

	public void CreatePlatformLobbyForJoinedSession()
	{
		if (m_CurrentLobbyData.joinedStatus == LobbyData.Status.Joining)
		{
			SteamAPICall_t hAPICall = Steamworks.SteamMatchmaking.CreateLobby((m_CurrentLobbyData.sessionType == Platform.SessionType.SESSION_TYPE_PUBLIC) ? ELobbyType.k_ELobbyTypePublic : ELobbyType.k_ELobbyTypePrivate, 250);
			m_LobbyCreatedResult.Set(hAPICall);
		}
	}
}
