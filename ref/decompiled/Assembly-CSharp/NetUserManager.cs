using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NetUserManager : T17NetSendMonoMessageTarget
{
	private static NetUserManager m_instance;

	public T17NetView m_NetView;

	public const int MaxNumPlayers = 4;

	public const int MinNumOnlinePlayers = 2;

	public const int MaxNumOnlinePlayers = 4;

	public const int MaxNumSinglePlayers = 1;

	public static NetUserManager Instance => m_instance;

	public static string GamerRoomProperty
	{
		get
		{
			using StringWriter stringWriter = new StringWriter();
			string empty = string.Empty;
			int num = Gamer.GetAllGamers().Length;
			Gamer[] allGamers = Gamer.GetAllGamers();
			for (int i = 0; i < num; i++)
			{
				if (allGamers[i] != null)
				{
					empty = string.Empty;
					empty = empty + allGamers[i].m_PhotonID + ",";
					empty = empty + allGamers[i].m_NetViewID + ",";
					empty = empty + allGamers[i].m_iControllerIndex + ",";
					empty = empty + allGamers[i].m_GamerName + ",";
					empty = empty + allGamers[i].m_PlatformUniqueID + ",";
					stringWriter.WriteLine(empty);
				}
			}
			return stringWriter.ToString();
		}
		set
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogGroup.PlayerGamer))
			{
			}
			if (T17NetManager.IsMasterClient || value == null)
			{
				return;
			}
			using StringReader stringReader = new StringReader(value);
			string text = stringReader.ReadLine();
			bool flag = false;
			while (!string.IsNullOrEmpty(text))
			{
				string[] array = text.Split(',');
				int num = int.Parse(array[0]);
				int netViewID = int.Parse(array[1]);
				int num2 = int.Parse(array[2]);
				string text2 = ((!(array[3] == string.Empty)) ? array[3] : null);
				string text3 = ((!(array[4] == string.Empty)) ? array[4] : null);
				if (T17NetManager.NetOfflineMode || (T17NetManager.IsConnectedOnline() && num == T17NetManager.PhotonPlayerID))
				{
					Gamer gamer = Gamer.FindGamer(num2, num, netViewID, Gamer.Location.LOCAL, text3);
					if (gamer == null)
					{
						Gamer[] allGamers = Gamer.GetAllGamers();
						for (int num3 = 3; num3 >= 0; num3--)
						{
							Gamer gamer2 = allGamers[num3];
							if (gamer2 != null && gamer2.m_PhotonID == -1 && num2 == gamer2.m_iControllerIndex && gamer2.IsLocal())
							{
								gamer = gamer2;
							}
						}
					}
					gamer?.UpdateGamer(num2, num, netViewID, text2, null, bPrimarySet: false, bPrimary: false, text3);
				}
				else if (T17NetManager.NetOnlineMode)
				{
					Gamer gamer3 = Gamer.FindGamer(num2, num, netViewID, Gamer.Location.REMOTE);
					if (gamer3 == null)
					{
						gamer3 = Gamer.GetGamerByPlayerID(num, -1, Gamer.Location.REMOTE);
					}
					if (gamer3 != null)
					{
						gamer3.UpdateGamer(num2, num, netViewID, text2, null, bPrimarySet: false, bPrimary: false, text3);
					}
					else
					{
						gamer3 = Gamer.InsertGamer(num2, num, netViewID, text2, null, bPrimary: false);
					}
				}
				if (text2 == null || num2 == -1)
				{
					flag = true;
				}
				text = stringReader.ReadLine();
			}
			if (flag)
			{
				SetGamerInfoRPC();
			}
		}
	}

	public static void SetGamerInfoRPC()
	{
		Gamer[] allGamers = Gamer.GetAllGamers();
		for (int num = allGamers.Length - 1; num >= 0; num--)
		{
			if (allGamers[num] != null && allGamers[num].IsLocal())
			{
				RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
				raiseEventOptions.Receivers = ReceiverGroup.MasterClient;
				object eventContent = new object[4]
				{
					allGamers[num].m_NetViewID,
					allGamers[num].m_GamerName,
					allGamers[num].m_iControllerIndex,
					allGamers[num].m_PlatformUniqueID
				};
				raiseEventOptions.Encrypt = true;
				PhotonNetwork.RaiseEvent(11, eventContent, sendReliable: true, raiseEventOptions);
			}
		}
	}

	private void OnEvent(byte eventcode, object content, int senderid)
	{
		if (eventcode == 11 && content is object[] array)
		{
			EventReceive_SetGamerInfo((int)array[0], (string)array[1], (int)array[2], (string)array[3], senderid);
		}
	}

	public void EventReceive_SetGamerInfo(int iNetView, string name, int iControllerIndex, string platformUniqueID, int senderid)
	{
		Gamer[] allGamers = Gamer.GetAllGamers();
		for (int num = allGamers.Length - 1; num >= 0; num--)
		{
			Gamer gamer = Gamer.FindGamer(iControllerIndex, senderid, iNetView, Gamer.Location.REMOTE);
			if (gamer == null)
			{
				gamer = Gamer.FindGamer(-1, senderid, iNetView, Gamer.Location.REMOTE);
			}
			if (gamer != null)
			{
				gamer.UpdateGamer(iControllerIndex, gamer.m_PhotonID, gamer.m_NetViewID, name, null, bPrimarySet: false, bPrimary: false, platformUniqueID);
			}
			else if (Gamer.m_GamerCount < 4)
			{
				Gamer.InsertGamer(iControllerIndex, senderid, iNetView, name, null, bPrimary: false);
			}
		}
	}

	public void KickGamer(Gamer gamer)
	{
		if (!T17NetManager.IsMasterClient || gamer == null)
		{
			return;
		}
		int photonID = gamer.m_PhotonID;
		PhotonPlayer[] playerList = PhotonNetwork.playerList;
		for (int num = playerList.Length - 1; num >= 0; num--)
		{
			PhotonPlayer photonPlayer = playerList[num];
			if (photonPlayer != null && photonPlayer.ID == photonID)
			{
				m_NetView.RPC("RPC_HandleKicked", playerList[num]);
				GoogleAnalyticsV3.LogCommericalAnalyticEvent("User Kicked", "User Kicked in Online Game", string.Empty, 0L);
				break;
			}
		}
	}

	public bool IsGamerInRoom(string onlineID)
	{
		PhotonPlayer[] playerList = PhotonNetwork.playerList;
		for (int num = playerList.Length - 1; num >= 0; num--)
		{
			PhotonPlayer photonPlayer = playerList[num];
			if (photonPlayer != null && photonPlayer.UserId == onlineID)
			{
				return true;
			}
		}
		return false;
	}

	[PunRPC]
	private void RPC_HandleKicked()
	{
		ErrorDialogHandler.ShowError(ErrorDialogHandler.Type.Kicked);
	}

	protected override void Awake()
	{
		base.Awake();
		if (m_instance != null)
		{
			Debug.LogError("More than one UserManager instance has been created, it expects to be a singleton.", this);
			return;
		}
		m_instance = this;
		if (m_NetView == null)
		{
			m_NetView = GetComponent<T17NetView>();
			if (m_NetView.viewID == 0)
			{
				m_NetView.viewID = T17NetConfig.GetReservedNetID(T17NetConfig.ReservedNetID.PersistentScripts);
			}
			if (m_NetView == null)
			{
				Debug.LogErrorFormat("NetUserManager: Failed to find NetView : {0}", base.gameObject.name);
			}
		}
		Gamer.OnDeleted += OnGamerDeleted;
		Gamer.OnCreate += OnGamerCreated;
		Gamer.OnUpdated += OnGamerUpdated;
		T17NetManager.OnCreatedRoomEvent += OnCreatedRoomEvent;
		T17NetManager.OnJoinedRoomEvent += OnJoinedRoomEvent;
		T17NetManager.OnLeftRoomEvent += OnLeftRoomEvent;
		T17NetManager.OnPhotonPlayerConnectedEvent += OnPhotonPlayerConnectedEvent;
		PhotonNetwork.OnEventCall = (PhotonNetwork.EventCallback)Delegate.Combine(PhotonNetwork.OnEventCall, new PhotonNetwork.EventCallback(OnEvent));
	}

	protected virtual void OnDestroy()
	{
		if (m_instance == this)
		{
			m_instance = null;
		}
		Gamer.OnDeleted -= OnGamerDeleted;
		Gamer.OnCreate -= OnGamerCreated;
		Gamer.OnUpdated -= OnGamerUpdated;
		PhotonNetwork.OnEventCall = (PhotonNetwork.EventCallback)Delegate.Remove(PhotonNetwork.OnEventCall, new PhotonNetwork.EventCallback(OnEvent));
		T17NetManager.OnCreatedRoomEvent -= OnCreatedRoomEvent;
		T17NetManager.OnJoinedRoomEvent -= OnJoinedRoomEvent;
		T17NetManager.OnLeftRoomEvent -= OnLeftRoomEvent;
		T17NetManager.OnPhotonPlayerConnectedEvent -= OnPhotonPlayerConnectedEvent;
		m_NetView = null;
	}

	private static void OnGamerDeleted()
	{
		SetGamerRoomProperties(GamerRoomProperty, Gamer.m_GamerCount);
		if (T17NetManager.IsMasterClient)
		{
			T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.GamerCount, Gamer.m_GamerCount);
		}
	}

	private static void OnGamerCreated(Gamer gamer)
	{
		SetGamerRoomProperties(GamerRoomProperty, Gamer.m_GamerCount);
		if (T17NetManager.IsMasterClient)
		{
			T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.GamerCount, Gamer.m_GamerCount);
		}
	}

	private static void OnGamerUpdated(Gamer gamer)
	{
		if (T17NetManager.IsMasterClient)
		{
			SetGamerRoomProperties(GamerRoomProperty, Gamer.m_GamerCount);
		}
		else if (gamer.IsLocal())
		{
			SetGamerInfoRPC();
		}
	}

	private static void SetGamerRoomProperties(string GamerProperties, int roomPlayerCount)
	{
		if (T17NetManager.IsMasterClient)
		{
			T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.Gamers, GamerProperties);
		}
	}

	private void AssignPlayerIds()
	{
		int num = -1;
		string text = null;
		string text2 = null;
		if (null != Platform.GetInstance())
		{
			text2 = Platform.GetInstance().GetPrimaryUserName();
		}
		Gamer[] allGamers = Gamer.GetAllGamers();
		for (int num2 = allGamers.Length - 1; num2 >= 0; num2--)
		{
			Gamer gamer = allGamers[num2];
			if (gamer != null)
			{
				gamer.UpdateGamer(GamerName: (!gamer.m_bPrimaryLocal) ? null : text2, PlayerID: (!gamer.IsLocal()) ? gamer.m_PhotonID : T17NetManager.PhotonPlayerID, iControllerIndex: gamer.m_iControllerIndex, NetViewID: gamer.m_NetViewID, PlayerObject: null, bPrimarySet: false, bPrimary: false, uniquePlatformID: gamer.m_PlatformUniqueID);
			}
		}
	}

	private void UpdatePlayerNetViews()
	{
		List<Player> allPlayers = Player.GetAllPlayers();
		if (allPlayers != null)
		{
			for (int i = 0; i < allPlayers.Count; i++)
			{
				Player player = allPlayers[i];
				if (null != player && null != player.m_NetView && player.m_NetView.ownerId != 0)
				{
					player.m_NetView.TransferOwnership(0);
				}
			}
		}
		Gamer[] localGamers = Gamer.GetLocalGamers();
		if (localGamers != null)
		{
			for (int j = 0; j < localGamers.Length; j++)
			{
				Player playerObject = localGamers[j].m_PlayerObject;
				if (null != playerObject && null != playerObject.m_NetView && playerObject.m_Gamer.m_PhotonID != -1)
				{
					playerObject.m_NetView.TransferOwnership(playerObject.m_Gamer.m_PhotonID);
				}
			}
		}
		T17NetManager.Instance.OnMasterClientSwitched(PhotonNetwork.player);
	}

	public void OnCreatedRoomEvent(bool bResult)
	{
		if (bResult)
		{
			Gamer.DeleteRemoteGamers();
			AssignPlayerIds();
			UpdatePlayerNetViews();
		}
	}

	public void OnJoinedRoomEvent(short bResult)
	{
		if (bResult != 0)
		{
			return;
		}
		if (T17NetManager.IsMasterClient)
		{
			AssignPlayerIds();
			return;
		}
		Gamer.DeleteRemoteGamers();
		Gamer.InvalidateOnlineIds(bNetView: true, bPlayerId: true);
		Gamer.DumpGamers();
		string outValue = null;
		T17NetRoomGameView.GetCustomPropertyAsString(T17NetRoomGameView.CustomProperty.Gamers, ref outValue);
		if (!string.IsNullOrEmpty(outValue))
		{
			GamerRoomProperty = outValue;
		}
		SetGamerInfoRPC();
	}

	public virtual void OnLeftRoomEvent()
	{
		Gamer.DeleteRemoteGamers();
	}

	public virtual void OnPhotonPlayerConnectedEvent(PhotonPlayer newPlayer)
	{
		Gamer.InsertGamer(-1, newPlayer.ID, -1, null, null, bPrimary: false);
	}
}
