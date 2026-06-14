using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Rewired;
using UnityEngine;

public class T17NetworkManager : T17NetSendMonoMessageTarget
{
	private struct DeserializableManager
	{
		public IDeserializable manager;

		public string managerName;

		public bool critical;

		public DeserializableManager(IDeserializable _manager, string _managerName, bool bCritical)
		{
			manager = _manager;
			managerName = _managerName;
			critical = bCritical;
		}
	}

	private T17NetView m_NetView;

	private static T17NetworkManager m_Instance;

	public int m_LogInt = -1;

	private bool m_bWaitingOnSplitscreenResponse;

	private GameObject m_PlayerObjectPrefab;

	private int m_SplitscreenRequestCount;

	private float m_SplitscreenRequestTimer;

	private const float m_SplitscreenRequestResetTime = 10f;

	private bool m_bWaitingForSignIn;

	private int m_RewiredSignInIndex = -1;

	public static T17NetworkManager GetInstance()
	{
		return m_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		m_Instance = this;
		base.Awake();
		m_SplitscreenRequestCount = 0;
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
		m_NetView = null;
	}

	private void Start()
	{
		T17NetRoomGameView.Instance.ClearCustomProperties();
		m_NetView = GetComponent<T17NetView>();
		if (m_NetView != null)
		{
			m_NetView.viewID = 0;
			m_NetView.viewID = T17NetConfig.GetReservedNetID(T17NetConfig.ReservedNetID.T17NetworkManager);
		}
		else
		{
			Debug.LogErrorFormat("T17NetworkManager.Start: Failed to find T17NetView on {0}", base.gameObject.name);
		}
		m_PlayerObjectPrefab = Resources.Load("Prefabs/Characters/PlayerObject") as GameObject;
		m_SplitscreenRequestCount = 0;
	}

	public void Stop()
	{
		List<Player> allPlayers = Player.GetAllPlayers();
		foreach (Player item in allPlayers)
		{
			UnityEngine.Object.Destroy(item.gameObject);
			if (item.m_Gamer != null)
			{
				PhotonNetwork.UnAllocateViewID(item.m_Gamer.m_NetViewID);
			}
		}
		m_SplitscreenRequestCount = 0;
	}

	public void RequestPhotonSerialiseViewRPC()
	{
		if (T17NetManager.NetOnlineMode && null != m_NetView)
		{
			m_NetView.RPC("RPC_RequestPhotonSerialiseView", NetTargets.Others);
		}
	}

	[PunRPC]
	public void RPC_RequestPhotonSerialiseView(PhotonMessageInfo info)
	{
		List<Character> allCharacters = Character.GetAllCharacters();
		for (int i = 0; i < allCharacters.Count; i++)
		{
			Character character = allCharacters[i];
			if (character.m_NetView.isMine)
			{
				character.ForcePhotonSerialiseView();
			}
		}
	}

	public void EndLevelRPC()
	{
		if (T17NetManager.NetOnlineMode && null != m_NetView)
		{
			m_NetView.RPC("RPC_EndLevel", NetTargets.AllViaServer);
		}
		else
		{
			GlobalStart.GetInstance().EndLevel(bShowResults: true);
		}
	}

	[PunRPC]
	public void RPC_EndLevel(PhotonMessageInfo info)
	{
		GlobalStart.GetInstance().EndLevel(bShowResults: true);
	}

	internal void SendNetViewIDs(int sceneViewID, int[] viewsIDs)
	{
		int num = viewsIDs.Length;
		if (PhotonNetwork.isMasterClient && num > 1 && m_NetView != null)
		{
			StringBuilder stringBuilder = new StringBuilder(128);
			stringBuilder.Append(viewsIDs[0]);
			for (int i = 1; i < num; i++)
			{
				stringBuilder.AppendFormat(", {0}", viewsIDs[i]);
			}
			m_NetView.RPC("RPC_SetNetViewIDs", NetTargets.OthersBuffered, sceneViewID, num, stringBuilder.ToString());
		}
	}

	[PunRPC]
	public void RPC_SetNetViewIDs(int sceneViewID, int len, string strViewsIDs)
	{
		T17NetView t17NetView = T17NetView.Find<T17NetView>(sceneViewID);
		if (!(t17NetView != null) || !(t17NetView.gameObject != null))
		{
			return;
		}
		int[] array = new int[len];
		int num = 0;
		try
		{
			Regex regex = new Regex("\"[^\"\\r\\n]*\"|[^,\\r\\n]*", RegexOptions.Multiline);
			Match match = regex.Match(strViewsIDs);
			while (match.Success && num < len)
			{
				if (int.TryParse(match.Value, out array[num]))
				{
					num++;
				}
				match = match.NextMatch();
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		if (num == len)
		{
			t17NetView.gameObject.SetNetViewIDs(array);
		}
	}

	public void RequestPlayerOwnershipRPC()
	{
		Gamer[] allGamers = Gamer.GetAllGamers();
		for (int num = allGamers.Length - 1; num >= 0; num--)
		{
			Gamer gamer = allGamers[num];
			if (gamer != null && gamer.IsLocal() && gamer.m_NetViewID == -1)
			{
				T17NetConfig.ReservedNetID reservedNetID = T17NetConfig.ReservedNetID.PlayerStart;
				int num2 = 4;
				int num3 = -1;
				int num4 = -1;
				for (int i = 0; i < num2; i++)
				{
					num3 = T17NetConfig.GetReservedNetID(reservedNetID + i);
					if (Gamer.GetGamerByViewID(num3) == null)
					{
						num4 = num3;
						break;
					}
				}
				if (num4 != -1)
				{
					gamer.m_eCharacterSelectionStage = Gamer.CharacterSelectionStage.CharacterRequested;
					m_NetView.RPCQuestion("RPC_RequestPlayerOwnership", NetTargets.MasterClient, num4, gamer.m_iControllerIndex, PhotonNetwork.player.ID);
				}
				break;
			}
		}
	}

	public void RequestPlayerOwnershipRPC(int netView, Gamer playerGamer)
	{
		if (playerGamer != null && playerGamer.IsLocal())
		{
			int num = -1;
			if (Gamer.GetGamerByViewID(netView) == null)
			{
				num = netView;
			}
			playerGamer.m_eCharacterSelectionStage = Gamer.CharacterSelectionStage.CharacterRequested;
			m_NetView.RPCQuestion("RPC_RequestPlayerOwnership", NetTargets.MasterClient, num, playerGamer.m_iControllerIndex, PhotonNetwork.player.ID);
		}
	}

	[PunRPC]
	public void RPC_RequestPlayerOwnership(int RPCID, int viewID, int controllerID, int senderID)
	{
		PhotonPlayer photonPlayer = PhotonPlayer.Find(senderID);
		bool flag = false;
		Gamer gamerByViewID = Gamer.GetGamerByViewID(viewID);
		if (gamerByViewID == null)
		{
			List<Player> allPlayers = Player.GetAllPlayers();
			int count = allPlayers.Count;
			for (int i = 0; i < count; i++)
			{
				if (allPlayers[i].m_NetView.viewID == viewID)
				{
					flag = true;
					m_NetView.RPCQuestion("RPC_PlayerOwnershipChanged", NetTargets.All, photonPlayer.ID, viewID, controllerID);
					break;
				}
			}
		}
		if (!flag)
		{
			m_NetView.RPCQuestion("RequestPlayerOwnership_ResponseDeniedRPC", photonPlayer, viewID);
		}
		m_NetView.RPCResponse(null, RPCID);
	}

	[PunRPC]
	public void RequestPlayerOwnership_ResponseDeniedRPC(int RPCID, int viewID)
	{
		m_NetView.RPCResponse(null, RPCID);
		string outValue = null;
		T17NetRoomGameView.GetCustomPropertyAsString(T17NetRoomGameView.CustomProperty.Gamers, ref outValue);
		if (outValue != null)
		{
			NetUserManager.GamerRoomProperty = outValue;
		}
		if (Helpers.IsInGameplayScene())
		{
			RequestPlayerOwnershipRPC();
		}
	}

	[PunRPC]
	public void RPC_PlayerOwnershipChanged(int RPCID, int ownerID, int viewID, int controllerID)
	{
		m_NetView.RPCResponse(null, RPCID);
		List<Player> allPlayers = Player.GetAllPlayers();
		int count = allPlayers.Count;
		for (int i = 0; i < count; i++)
		{
			if (allPlayers[i].m_NetView.viewID != viewID)
			{
				continue;
			}
			Gamer[] allGamers = Gamer.GetAllGamers();
			for (int num = allGamers.Length - 1; num >= 0; num--)
			{
				Gamer gamer = allGamers[num];
				if (gamer != null && null == gamer.m_PlayerObject && gamer.m_PhotonID == ownerID && gamer.m_iControllerIndex == controllerID)
				{
					gamer.m_eCharacterSelectionStage = Gamer.CharacterSelectionStage.CharacterGranted;
					gamer.UpdateGamer(gamer.m_iControllerIndex, gamer.m_PhotonID, viewID, null, allPlayers[i], bPrimarySet: false, bPrimary: false, gamer.m_PlatformUniqueID);
					LinkGamerToPlayer(gamer, allPlayers[i]);
					return;
				}
			}
		}
	}

	public void LinkGamerToPlayer(Gamer gamer, Player player)
	{
		if (gamer != null && !(null == player))
		{
			GameObject gameObject = player.gameObject;
			bool flag = gamer.IsLocal();
			T17NetView netView = player.m_NetView;
			if (null != netView && null != gameObject)
			{
				gameObject.name = string.Format("P{0}-({1})", (gamer == null) ? "Unknown" : gamer.m_GamerName, (!flag) ? "Remote" : "Local");
			}
			player.SetUpGamer(gamer);
			player.SetGamer(gamer);
			if (flag)
			{
				player.InitHUD(gamer.m_PhotonID);
			}
			TryOpeningPlayerSelect(gamer);
		}
	}

	public static void TryOpeningPlayerSelect(Gamer gamer)
	{
		CameraManager.PlayerBindingID cameraIndexForGamer = InGameMenuFlow.Instance.GetCameraIndexForGamer(gamer);
		if (cameraIndexForGamer == CameraManager.PlayerBindingID.CM_PBID_UNSET || gamer.m_eCharacterSelectionStage != Gamer.CharacterSelectionStage.CharacterOwned)
		{
			return;
		}
		if (!CheckIfLastPlayerVersus())
		{
			if (gamer.m_bPrimaryLocal && T17NetManager.IsMasterClient && PrisonSnapshotIO.IsThereSaveData())
			{
				InGameMenuFlow.Instance.SkipPlayerSelect(gamer);
				return;
			}
			if (LevelScript.GetCurrentLevelInfo() == null || LevelScript.GetCurrentLevelInfo().m_PrisonType != LevelScript.PRISON_TYPE.Tutorial)
			{
				InGameMenuFlow.Instance.OpenPlayerSelect(gamer, cameraIndexForGamer);
				return;
			}
			InGameMenuFlow.Instance.SkipPlayerSelect(gamer);
			InGameMenuFlow.Instance.SetupPlayerForTutorial(gamer);
		}
		else
		{
			InGameMenuFlow.Instance.SkipPlayerSelect(gamer);
		}
	}

	public void SpawnPlayerObject(int viewID, int spawnPointIndex)
	{
		Player player = null;
		GameObject gameObject = null;
		gameObject = UnityEngine.Object.Instantiate(m_PlayerObjectPrefab, Vector3.zero, Quaternion.identity);
		gameObject.name = "Player_P" + T17NetManager.PhotonPlayerID;
		player = gameObject.GetComponent<Player>();
		if (null != player)
		{
			T17NetView netView = player.m_NetView;
			if (null != netView)
			{
				netView.viewID = viewID;
			}
		}
		player.SetSpawnIndex(spawnPointIndex);
	}

	public void RequestNativeSessionInviteInformationRPC()
	{
		Debug.Log("RequestNativeSessionInviteInformationRPC");
		m_NetView.RPC("RPC_MasterRespondWithSessionInviteInformation", NetTargets.MasterClient);
	}

	[PunRPC]
	private void RPC_MasterRespondWithSessionInviteInformation(PhotonMessageInfo info)
	{
		Platform.GetInstance().GetSessionJoinInformation(out var hasPassword);
		Debug.Log("RPC_MasterRespondWithSessionInviteInformation recieved from client " + info.sender.ToString() + ". Session created with password?: " + hasPassword);
		m_NetView.RPC("RPC_ClientRecieveSessionInviteInformation", info.sender, hasPassword);
	}

	[PunRPC]
	private void RPC_ClientRecieveSessionInviteInformation(bool requiresPassword)
	{
		Debug.Log("RPC_ClientRecieveSessionInviteInformation recieved. Session needs password?: " + requiresPassword);
		Platform.GetInstance().JoinInvitedSessionRoom(requiresPassword);
	}

	public void RequestJoinNativeSessionRPC()
	{
		Debug.Log("RequestJoinNativeSessionRPC");
		m_NetView.RPC("RPC_MasterRespondToClientWhenSessionSetup", NetTargets.MasterClient);
	}

	[PunRPC]
	private void RPC_MasterRespondToClientWhenSessionSetup(PhotonMessageInfo info)
	{
		Debug.Log("RPC_MasterRespondToClientWhenSessionSetup");
		Platform.GetInstance().PhotonPlayerRequestsAccessToRoom(info.sender);
	}

	public void SignalToClientThatSessionIsSetUpRPC(PhotonPlayer photonPlayer, string roomToSearchFor)
	{
		Debug.Log("SignalToClientThatSessionIsSetUpRPC");
		m_NetView.RPC("RPC_ClientRecieveSessionIsSetUpMessage", photonPlayer, roomToSearchFor);
	}

	[PunRPC]
	private void RPC_ClientRecieveSessionIsSetUpMessage(string roomToSearchFor, PhotonMessageInfo info)
	{
		Platform.GetInstance().JoinSessionViaSearch(roomToSearchFor);
	}

	public void BroadcastSessionTypeChangedRPC(bool isSessionPrivate)
	{
		if (T17NetManager.IsMasterClient)
		{
			Debug.Log("BroadcastSessionTypeChangedRPC isSessionPrivate: " + isSessionPrivate);
			m_NetView.RPC("RPC_AllRecieveUpdatedNativeSessionSetup", NetTargets.Others, isSessionPrivate);
		}
	}

	[PunRPC]
	private void RPC_AllRecieveUpdatedNativeSessionSetup(bool isSessionPrivate)
	{
		Debug.Log("RPC_AllRecieveUpdatedNativeSessionSetup isSessionPrivate: " + isSessionPrivate);
		Platform.GetInstance().UpdateJoinedSessionInformation(isSessionPrivate);
	}

	public static bool Deserialize(IDeserializable item, ref string errorStr)
	{
		return item.Deserialize(item.GetSerializationData(), ref errorStr);
	}

	public void DeserializeWorld(out bool bError, out bool bWait, ref string errorInformation)
	{
		bError = true;
		bool flag = true;
		if (PrisonSnapshotIO.IsThereSaveData() || !T17NetManager.IsMasterClient)
		{
			List<DeserializableManager> list = new List<DeserializableManager>();
			if (PrisonSnapshotIO.IsThereSaveData())
			{
				list.Add(new DeserializableManager(ItemManager.GetInstance(), "ItemManager", bCritical: false));
				list.Add(new DeserializableManager(FloorManager.GetInstance(), "FloorManager", bCritical: false));
				list.Add(new DeserializableManager(RoomManager.GetInstance(), "RoomManager", bCritical: false));
				list.Add(new DeserializableManager(JobsManager.GetInstance(), "JobsManager", bCritical: false));
				list.Add(new DeserializableManager(PrisonAlertnessManager.GetInstance(), "PrisonAlertnessManager", bCritical: false));
				list.Add(new DeserializableManager(QuestManager.GetInstance(), "QuestManager", bCritical: false));
				list.Add(new DeserializableManager(VendorManager.GetInstance(), "VendorManager", bCritical: false));
				list.Add(new DeserializableManager(SolitaryManager.GetInstance(), "SolitaryManager", bCritical: false));
				list.Add(new DeserializableManager(VisitorCustomisationManager.GetInstance(), "VisitorManager", bCritical: false));
			}
			list.Add(new DeserializableManager(new ToiletInteractionDeserializer(), "ToiletInteractionDeserializer", bCritical: false));
			list.Add(new DeserializableManager(new CarryObjectInteractionDeserialiser(), "CarryObjectInteractionDeserialiser", bCritical: false));
			list.Add(new DeserializableManager(OpinionManager.GetInstance(), "OpinionManager", bCritical: false));
			list.Add(new DeserializableManager(NPCManager.GetInstance(), "NPCManager", bCritical: false));
			list.Add(new DeserializableManager(PlayerDataManager.GetInstance(), "PlayerDataManager", bCritical: false));
			list.Add(new DeserializableManager(TagManager.GetInstance(), "PlayerTagManager", bCritical: false));
			list.Add(new DeserializableManager(new CCTVCamera.CCTVDeserializer(), "CCTVCamera", bCritical: false));
			list.Add(new DeserializableManager(new MapItemTracker.MapItemTrackerDeserializer(), "PlayerItemTracking", bCritical: false));
			bWait = false;
			string errorStr = string.Empty;
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				IDeserializable manager = list[i].manager;
				if (manager == null)
				{
					continue;
				}
				string serializationData = manager.GetSerializationData();
				if (list[i].critical && string.IsNullOrEmpty(serializationData) && (serializationData == null || serializationData != "Intentionally Left Blank"))
				{
					if (m_LogInt != i)
					{
						m_LogInt = i;
					}
					bWait = true;
					bError = false;
					return;
				}
			}
			if (m_LogInt != -1)
			{
			}
			m_LogInt = -1;
			for (int j = 0; j < count; j++)
			{
				IDeserializable manager2 = list[j].manager;
				if (manager2 != null)
				{
					if (!Deserialize(manager2, ref errorStr))
					{
						flag = false;
						errorInformation = errorInformation + manager2.GetType().ToString() + ": ";
						if (!string.IsNullOrEmpty(errorStr))
						{
							errorInformation += errorStr;
						}
						errorStr = string.Empty;
					}
				}
				else
				{
					flag = false;
					errorInformation = errorInformation + "\nUnable to deserialize " + list[j].managerName + " as it is null.";
				}
			}
		}
		Gamer.DumpGamers();
		bWait = false;
		bError = !flag;
	}

	public void RequestSplitscreenPlayer(Rewired.Player rewiredPlayer)
	{
		if (!m_bWaitingOnSplitscreenResponse)
		{
			m_bWaitingOnSplitscreenResponse = true;
			m_NetView.RPCQuestion("RPC_RequestSplitscreenPlayer", NetTargets.MasterClient, rewiredPlayer.id);
		}
	}

	[PunRPC]
	public void RPC_RequestSplitscreenPlayer(int RPCID, int rewiredID, PhotonMessageInfo info)
	{
		if (T17NetManager.IsMasterClient)
		{
			bool flag = Gamer.GetGamerCount() + m_SplitscreenRequestCount < 4;
			if (flag)
			{
				m_SplitscreenRequestCount++;
				m_SplitscreenRequestTimer = Time.realtimeSinceStartup + 10f;
			}
			m_NetView.RPCResponse("RPC_InsertGamerResponse", RPCID, flag, rewiredID);
		}
	}

	[PunRPC]
	private void RPC_InsertGamer(int iPhotonID, int iControllerIndex, PhotonMessageInfo info)
	{
		if (T17NetManager.IsMasterClient)
		{
			m_SplitscreenRequestCount--;
			if (m_SplitscreenRequestCount < 0)
			{
				m_SplitscreenRequestCount = 0;
			}
		}
		if (iPhotonID != T17NetManager.PhotonPlayerID)
		{
			Gamer.UpsertGamer(iControllerIndex, iPhotonID, -1, null, null, bPrimarySet: false, bPrimary: false);
		}
		else
		{
			if (iControllerIndex == -1)
			{
				return;
			}
			string gamerName = ((!(null != Platform.GetInstance())) ? string.Empty : Platform.GetInstance().GetUserNameByControllerIndex(iControllerIndex));
			bool bPrimary = null == Gamer.GetPrimaryGamer();
			Gamer gamer = Gamer.UpsertGamer(iControllerIndex, iPhotonID, -1, gamerName, null, bPrimarySet: true, bPrimary);
			T17EventSystemsManager.Instance.AssignFreeEventSystemToGamer(gamer);
			LevelScript.PRISON_ENUM prisonEnum = LevelScript.GetCurrentLevelInfo().m_PrisonEnum;
			bool flag = LevelDataManager.GetInstance().IsDLCLevel(prisonEnum);
			bool flag2 = LevelScript.GetCurrentLevelInfo().m_PrisonType == LevelScript.PRISON_TYPE.Transport;
			if (!T17NetManager.IsConnectedOnline() || !T17NetRoomManager.IsInRoom() || gamer == null || !gamer.IsLocal())
			{
				return;
			}
			PrisonConfig.ConfigType gameType = ConfigManager.GetInstance().gameType;
			if (!flag)
			{
				if (!flag2 && gameType == PrisonConfig.ConfigType.Cooperative)
				{
					StatSystem.GetInstance().IncStat(17, 1f, gamer, string.Empty);
				}
				if (gameType == PrisonConfig.ConfigType.Versus)
				{
					StatSystem.GetInstance().IncStat(16, 1f, gamer, string.Empty);
				}
			}
		}
	}

	[PunRPC]
	private void RPC_InsertGamerResponse(bool allowed, int rewiredID, PhotonMessageInfo info)
	{
		bool flag = false;
		m_RewiredSignInIndex = rewiredID;
		if (allowed)
		{
			if (!Platform.GetInstance().EndDiscovery(rewiredID, bIsPrimary: false))
			{
				m_bWaitingForSignIn = true;
			}
			else if (T17NetRoomManager.CurrentGameRoomType != 0)
			{
				Platform.GetInstance().OnlineAreaEntryCheckRequest(isLeaderboard: false, OnOnlineEntryCheckRequestDone);
				flag = true;
			}
			else
			{
				OnOnlineEntryCheckRequestDone(bAllowedToProgress: true, Platform.OnlineAccessErrorCode.OnlineAccessOK, failureHandledPlatformside: true);
				flag = true;
			}
		}
		else
		{
			T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
			if (dialog != null)
			{
				dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Dialog.LobbyFull", "Text.Dialog.LobbyFull.Body", "Text.Dialog.Prompt.Ok", string.Empty, string.Empty);
				dialog.OnConfirm = ConfirmedCantJoinCallback;
				dialog.Show();
			}
		}
		if (!flag)
		{
			m_bWaitingOnSplitscreenResponse = false;
		}
	}

	public void DeleteGamer(int id)
	{
		m_NetView.RPC("RPC_DeleteGamer", NetTargets.All, id);
	}

	[PunRPC]
	private void RPC_DeleteGamer(int iPhotonID, PhotonMessageInfo info)
	{
		Gamer.DeleteGamerByNetviewID(iPhotonID);
	}

	private void OnOnlineEntryCheckRequestDone(bool bAllowedToProgress, Platform.OnlineAccessErrorCode returnCode, bool failureHandledPlatformside)
	{
		if (bAllowedToProgress)
		{
			m_NetView.RPC("RPC_InsertGamer", NetTargets.All, PhotonNetwork.player.ID, m_RewiredSignInIndex);
		}
		else
		{
			ConfirmedCantJoinCallback(null);
		}
		m_bWaitingOnSplitscreenResponse = false;
	}

	private void ConfirmedCantJoinCallback(T17DialogBox dialog)
	{
		Platform.GetInstance().RemoveRewiredPlayer(m_RewiredSignInIndex);
		Platform.GetInstance().RemoveUnusedUsers();
	}

	private void Update()
	{
		if (m_SplitscreenRequestCount > 0 && Time.realtimeSinceStartup >= m_SplitscreenRequestTimer)
		{
			m_SplitscreenRequestCount = 0;
		}
		if (!m_bWaitingForSignIn || !Platform.GetInstance().FinishedDiscovery(m_RewiredSignInIndex))
		{
			return;
		}
		switch (Platform.GetInstance().GetPlatformError())
		{
		case Platform.PlatformError.None:
			if (T17NetRoomManager.CurrentGameRoomType != 0)
			{
				Platform.GetInstance().OnlineAreaEntryCheckRequest(isLeaderboard: false, OnOnlineEntryCheckRequestDone);
				break;
			}
			OnOnlineEntryCheckRequestDone(bAllowedToProgress: true, Platform.OnlineAccessErrorCode.OnlineAccessOK, failureHandledPlatformside: true);
			m_RewiredSignInIndex = -1;
			break;
		default:
		{
			T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
			if (dialog != null)
			{
				dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Dialog.UserAlreadyInUse", "Text.Dialog.UserAlreadyInUse.Body", "Text.Dialog.Prompt.Ok", string.Empty, string.Empty);
				dialog.SetSymbol(T17DialogBox.Symbols.Error);
				dialog.Show();
			}
			Platform.GetInstance().RemoveRewiredPlayer(m_RewiredSignInIndex);
			Platform.GetInstance().RemoveUnusedUsers();
			m_RewiredSignInIndex = -1;
			break;
		}
		case Platform.PlatformError.UserCancelled:
			OnOnlineEntryCheckRequestDone(bAllowedToProgress: false, Platform.OnlineAccessErrorCode.UserNoLongerLoggedIn, failureHandledPlatformside: true);
			Platform.GetInstance().RemoveRewiredPlayer(m_RewiredSignInIndex);
			Platform.GetInstance().RemoveUnusedUsers();
			m_RewiredSignInIndex = -1;
			break;
		}
		m_bWaitingForSignIn = false;
	}

	public static bool CheckIfLastPlayerVersus()
	{
		ConfigManager instance = ConfigManager.GetInstance();
		if (null != instance && instance.gameType == PrisonConfig.ConfigType.Versus && Gamer.GetGamerCount() <= 1)
		{
			T17NetRoomGameView.GameRoomType outValue = T17NetRoomGameView.GameRoomType.Undefined;
			if (T17NetRoomManager.IsInRoom() && T17NetRoomGameView.GetCustomPropertyAsEnum(T17NetRoomGameView.CustomProperty.RoomType, ref outValue) && (outValue == T17NetRoomGameView.GameRoomType.Offline || (T17NetManager.IsConnectedOnline() && outValue != 0)))
			{
				return true;
			}
		}
		return false;
	}
}
