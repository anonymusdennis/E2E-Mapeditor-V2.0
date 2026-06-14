using Rewired;
using UnityEngine;

public class CoopFrontEndMenu : FrontendMenuBehaviour
{
	private enum GAMESTAGE
	{
		IDLE,
		DISCOVER_OTHERS,
		WAIT_FOR_SIGNIN_OTHER,
		POST_SIGNIN_OTHER,
		SPAWN_PLAYERS,
		SPAWN_PLAYERS2,
		SPAWN_REST,
		RUN
	}

	private const int MAX_PLAYER_COUNT = 4;

	private GAMESTAGE m_GameStage;

	public GameObject m_PlayerListParentObject;

	private CoopPlayerObject[] m_PlayerList;

	private int m_SlotsTaken;

	private int m_CurrentDicoveryControllerIndex = -1;

	protected override void SingleTimeInitialize()
	{
		base.SingleTimeInitialize();
		GetPlayerList();
	}

	protected override void Update()
	{
		base.Update();
		if (T17DialogBoxManager.HasAnyOpenDialogs())
		{
			return;
		}
		switch (m_GameStage)
		{
		case GAMESTAGE.IDLE:
			break;
		case GAMESTAGE.DISCOVER_OTHERS:
		{
			Rewired.Player player = Platform.GetInstance().CheckForAPress();
			if (player != null)
			{
				m_CurrentDicoveryControllerIndex = player.id;
				if (!Platform.GetInstance().EndDiscovery(player.id, bIsPrimary: false))
				{
					m_GameStage = GAMESTAGE.WAIT_FOR_SIGNIN_OTHER;
				}
				else
				{
					m_GameStage = GAMESTAGE.POST_SIGNIN_OTHER;
				}
			}
			break;
		}
		case GAMESTAGE.WAIT_FOR_SIGNIN_OTHER:
			if (!Platform.GetInstance().FinishedDiscovery(m_CurrentDicoveryControllerIndex))
			{
				break;
			}
			switch (Platform.GetInstance().GetPlatformError())
			{
			case Platform.PlatformError.None:
				m_GameStage = GAMESTAGE.POST_SIGNIN_OTHER;
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
				m_GameStage = GAMESTAGE.DISCOVER_OTHERS;
				Platform.GetInstance().RemoveRewiredPlayer(m_CurrentDicoveryControllerIndex);
				Platform.GetInstance().RemoveUnusedUsers();
				m_CurrentDicoveryControllerIndex = -1;
				break;
			}
			case Platform.PlatformError.UserCancelled:
				m_GameStage = GAMESTAGE.DISCOVER_OTHERS;
				Platform.GetInstance().RemoveRewiredPlayer(m_CurrentDicoveryControllerIndex);
				Platform.GetInstance().RemoveUnusedUsers();
				m_CurrentDicoveryControllerIndex = -1;
				break;
			}
			break;
		case GAMESTAGE.POST_SIGNIN_OTHER:
		{
			for (int i = 1; i < m_PlayerList.Length; i++)
			{
				if (m_PlayerList[i].IsWaitingForJoin())
				{
					string userNameByControllerIndex = Platform.GetInstance().GetUserNameByControllerIndex(m_CurrentDicoveryControllerIndex);
					m_PlayerList[i].SetJoined(userNameByControllerIndex, m_CurrentDicoveryControllerIndex);
					m_SlotsTaken++;
					Gamer gamer = Gamer.InsertGamer(m_CurrentDicoveryControllerIndex, T17NetManager.PhotonPlayerID, -1, userNameByControllerIndex, null, bPrimary: false);
					T17EventSystemsManager.Instance.AssignFreeEventSystemToGamer(gamer);
					m_CurrentDicoveryControllerIndex = -1;
					if (m_SlotsTaken < 4)
					{
						m_GameStage = GAMESTAGE.DISCOVER_OTHERS;
					}
					else
					{
						m_GameStage = GAMESTAGE.IDLE;
					}
					break;
				}
			}
			break;
		}
		case GAMESTAGE.SPAWN_PLAYERS:
			if (T17NetManager.NetOfflineMode)
			{
				Gamer[] allGamers = Gamer.GetAllGamers();
				for (int num = allGamers.Length - 1; num >= 0; num--)
				{
					if (allGamers[num] != null)
					{
						allGamers[num].UpdateGamer(allGamers[num].m_iControllerIndex, allGamers[num].m_PhotonID, allGamers[num].m_NetViewID, null, null, bPrimarySet: false, bPrimary: false, allGamers[num].m_PlatformUniqueID);
					}
				}
				T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.Gamers, NetUserManager.GamerRoomProperty);
			}
			T17NetManager.CanDisplayDisconnectionDialog = false;
			GlobalStart.GetInstance().StartGameWithModeAndCurrentConfig(GlobalStart.GLOBALSTART_GAME_MODES.LOCAL);
			GoogleAnalyticsV3.LogCommericalAnalyticEvent("Versus Game Started", "Offline Started", string.Empty, 0L);
			m_GameStage = GAMESTAGE.SPAWN_PLAYERS2;
			break;
		case GAMESTAGE.SPAWN_PLAYERS2:
			m_GameStage = GAMESTAGE.SPAWN_REST;
			break;
		case GAMESTAGE.SPAWN_REST:
			m_GameStage = GAMESTAGE.RUN;
			break;
		case GAMESTAGE.RUN:
			break;
		}
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		VersusFrontendMenu.RoomType = T17NetRoomGameView.GameRoomType.Offline;
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		Platform.GetInstance().SetPresenceTag("Text.Presence.VersusLocal");
		Platform.GetInstance().StartDiscovery(bMainUser: false);
		FrontEndFlow.Instance.SetInMultiUserMenu(this);
		m_SlotsTaken = 0;
		int num = 0;
		Gamer[] allGamers = Gamer.GetAllGamers();
		for (int num2 = allGamers.Length - 1; num2 >= 0; num2--)
		{
			if (allGamers[num2] != null)
			{
				string userNameByControllerIndex = Platform.GetInstance().GetUserNameByControllerIndex(allGamers[num2].m_iControllerIndex);
				m_PlayerList[num].SetJoined(userNameByControllerIndex, allGamers[num2].m_iControllerIndex);
				m_SlotsTaken++;
				num++;
			}
		}
		if (m_SlotsTaken < 4)
		{
			m_GameStage = GAMESTAGE.DISCOVER_OTHERS;
		}
		else
		{
			m_GameStage = GAMESTAGE.IDLE;
		}
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		FrontEndFlow instance = FrontEndFlow.Instance;
		if (instance != null)
		{
			instance.SetInMultiUserMenu(null);
		}
		if (Platform.GetInstance() != null)
		{
			Platform.GetInstance().SetPresenceTag("Text.Presence.FrontEndGeneral");
		}
		return true;
	}

	public void OnStart()
	{
		if (m_SlotsTaken > 1)
		{
			m_GameStage = GAMESTAGE.SPAWN_PLAYERS;
		}
	}

	public void RemoveGamer(int controllerIndex)
	{
		Gamer[] allGamers = Gamer.GetAllGamers();
		for (int num = allGamers.Length - 1; num >= 0; num--)
		{
			if (allGamers[num] != null && !allGamers[num].m_bPrimaryLocal && allGamers[num].m_iControllerIndex == controllerIndex)
			{
				Gamer.DeleteGamer(num, clearRewiredMaps: true);
				m_SlotsTaken--;
			}
		}
		for (int i = 0; i < m_PlayerList.Length; i++)
		{
			if (m_PlayerList[i].m_PlayerControllerIndex == controllerIndex)
			{
				m_PlayerList[i].SetWaitingForJoin();
				break;
			}
		}
		if (m_SlotsTaken < 4)
		{
			m_GameStage = GAMESTAGE.DISCOVER_OTHERS;
		}
	}

	public void OnLeave()
	{
		if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetClientDisconnect))
		{
		}
		if (T17NetManager.NetOfflineMode && Helpers.IsInFrontEndScene())
		{
			PlayersReset();
			NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
			m_GameStage = GAMESTAGE.IDLE;
		}
		Hide();
	}

	private void PlayersReset()
	{
		m_SlotsTaken = 0;
		for (int i = 0; i < m_PlayerList.Length; i++)
		{
			m_PlayerList[i].SetWaitingForJoin();
		}
		Gamer primaryGamer = Gamer.GetPrimaryGamer();
		if (primaryGamer != null && primaryGamer.m_RewiredPlayer != null)
		{
			T17EventSystem.ApplyCategories(primaryGamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.Frontend);
		}
		Gamer[] allGamers = Gamer.GetAllGamers();
		for (int num = allGamers.Length - 1; num >= 0; num--)
		{
			Gamer gamer = allGamers[num];
			if (gamer != null && !gamer.m_bPrimaryLocal && gamer.m_RewiredPlayer != null)
			{
				T17EventSystem.ApplyCategories(gamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.Assignment);
				Gamer.DeleteGamer(num);
			}
		}
		Platform.GetInstance().RemoveAllSecondaryUsers();
	}

	private void GetPlayerList()
	{
		PerPlatformActivator component = m_PlayerListParentObject.GetComponent<PerPlatformActivator>();
		if (component != null)
		{
			component.DoActivate();
		}
		if (m_PlayerListParentObject != null)
		{
			m_PlayerList = m_PlayerListParentObject.GetComponentsInChildren<CoopPlayerObject>(includeInactive: true);
			for (int i = 0; i < m_PlayerList.Length; i++)
			{
				m_PlayerList[i].SetWaitingForJoin();
			}
		}
	}
}
