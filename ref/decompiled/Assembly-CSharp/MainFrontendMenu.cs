using System.Collections.Generic;
using UnityEngine;

public class MainFrontendMenu : FrontendMenuBehaviour
{
	public GameObject m_LeaderboardsButton;

	public GameObject m_CustomPrisonsButton;

	public GameObject m_DLCStoreButton;

	public DLCPopup m_DLCPopup;

	public DLCStoreFrontendMenu m_DLCStoreFrontendMenu;

	public GameObject m_ChinaNetworkMessage;

	public GameObject m_RussianNetworkMessage;

	private T17DialogBox m_ConnectingDialog;

	private Platform.OnlineAreaEntryCheckCallback m_RequestResponse;

	private void OnOnlineRequestResponse(bool bAllowedToProgress, Platform.OnlineAccessErrorCode returnCode, bool failureHandledPlatformside)
	{
		if (bAllowedToProgress)
		{
			T17NetManager.m_DefaultConnectionState = NetConnectionState.OnlineMode_Idle;
			NetConnectAndJoinRoom.RequestConnectionState(T17NetManager.m_DefaultConnectionState, silentErrorDialogMode: true);
		}
		else
		{
			T17NetManager.m_DefaultConnectionState = NetConnectionState.OfflineMode;
		}
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		if (m_RequestResponse == null)
		{
			m_RequestResponse = OnOnlineRequestResponse;
		}
		GlobalStart instance = GlobalStart.GetInstance();
		bool flag = true;
		if (instance != null && instance.m_ReturnToFrontendRoute != 0)
		{
			flag = false;
		}
		Platform instance2 = Platform.GetInstance();
		if (flag && !T17NetInvites.HasInvite() && !NetGoOnlineHelper.IsActive && instance2 != null && !instance2.GetAgeHasBeenChecked())
		{
			instance2.SetAgeChecked(bValue: true);
			instance2.OnlineAreaEntryCheckRequest(isLeaderboard: true, m_RequestResponse, isModal: false, bShowSystemErrors: false);
		}
		if (m_RussianNetworkMessage != null)
		{
			if (Localization.GetLanguageIndex() == Localization.GameSupportedLanguages.Chinese && NetConnectAndJoinRoom.PhotonRegion != CloudRegionCode.rue)
			{
				m_RussianNetworkMessage.SetActive(value: true);
			}
			else
			{
				m_RussianNetworkMessage.SetActive(value: false);
			}
		}
		GlobalSave.GetInstance().RequestSave();
		if (m_DLCStoreButton != null)
		{
			m_DLCStoreButton.SetActive(value: true);
		}
		if (m_LeaderboardsButton != null)
		{
			m_LeaderboardsButton.SetActive(value: true);
		}
		if (m_CustomPrisonsButton != null)
		{
			m_CustomPrisonsButton.SetActive(value: true);
		}
		Platform.GetInstance().SetPresenceTag("Text.Presence.FrontEndGeneral");
		if (Gamer.GetPrimaryGamer() != null && Gamer.GetPrimaryGamer().m_RewiredPlayer != null)
		{
			T17EventSystem.ApplyCategories(Gamer.GetPrimaryGamer().m_RewiredPlayer, T17EventSystem.InputCateogryStates.MainFrontend);
		}
		CheckForDLCPopups();
		return true;
	}

	public void OnLeaderboardsButtonPressed()
	{
		Platform.GetInstance().OnlineAreaEntryCheckRequest(isLeaderboard: true, OnlinePublicEntryRequestCallback);
	}

	public void ShowConnectingDialog()
	{
		if (m_ConnectingDialog == null)
		{
			m_ConnectingDialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
			m_ConnectingDialog.InitializeSpinner(hasCancelButton: false, "Text.Dialog.Net.GoOnline.StartTitle", "Text.Dialog.Net.GoOnline.StartBody", string.Empty);
			m_ConnectingDialog.Show();
		}
	}

	private void OnlinePublicEntryRequestCallback(bool allowed, Platform.OnlineAccessErrorCode returnCode, bool failureHandledPlatformside)
	{
		if (allowed)
		{
			Platform.GetInstance().EnterOnlineArea(bIsLeaderboard: true, NewDisallowedUserCallback);
			FrontEndFlow.Instance.m_MainMenu.Hide();
			FrontEndFlow.Instance.m_MainMenu.SetFrontEndMenuTypeToOpen(FrontendRootMenu.FrontendMenuTypeToOpen.Leaderboards);
			FrontEndFlow.Instance.m_MainMenu.Show(Gamer.GetPrimaryGamer(), null, null);
		}
		else if (!failureHandledPlatformside)
		{
			ErrorDialogHandler.ShowDisconnectedDialog();
		}
	}

	public void NewDisallowedUserCallback()
	{
		FrontEndFlow.Instance.m_MainMenu.Hide();
		FrontEndFlow.Instance.m_MainMenu.SetFrontEndMenuTypeToOpen(FrontendRootMenu.FrontendMenuTypeToOpen.MainMenu);
		FrontEndFlow.Instance.m_MainMenu.Show(Gamer.GetPrimaryGamer(), null, null);
		Platform.GetInstance().ExitOnlineArea();
	}

	private void CheckForDLCPopups()
	{
		GlobalSave instance = GlobalSave.GetInstance();
		if (!(m_DLCStoreFrontendMenu != null) || !(instance != null) || !(m_DLCPopup != null))
		{
			return;
		}
		bool flag = false;
		SaveManager instance2 = SaveManager.GetInstance();
		flag = instance2 != null && instance2.AreThereAnySavesAllPrisons();
		List<DLCFrontendData> list = new List<DLCFrontendData>(m_DLCStoreFrontendMenu.m_DLCList);
		list.RemoveAll((DLCFrontendData x) => x == null);
		list.Sort((DLCFrontendData a, DLCFrontendData b) => a.m_PopupOrder.CompareTo(b.m_PopupOrder));
		Debug.Log("Processing " + list.Count + " potential dlc popups");
		for (int i = 0; i < list.Count; i++)
		{
			DLCFrontendData dLCFrontendData = list[i];
			if (!(dLCFrontendData != null))
			{
				continue;
			}
			if (!dLCFrontendData.IsAvailableOnThisPlatform() || !dLCFrontendData.ShowPopupOnThisPlatform())
			{
				Debug.Log("Ignoring dlc " + dLCFrontendData.m_DLCID + " because its not available on our platform");
				continue;
			}
			bool value = false;
			instance.Get("DLC:SeenPopup" + dLCFrontendData.m_DLCID, out value, def: false);
			if (!value)
			{
				instance.Set("DLC:SeenPopup" + dLCFrontendData.m_DLCID, value: true);
				instance.RequestSave();
				if (!dLCFrontendData.m_bOnlyShowIfPrisonSavesExist || flag)
				{
					Debug.Log("Showing DLC popup " + dLCFrontendData.m_DLCID);
					m_DLCPopup.ShowDLCPopup(dLCFrontendData);
					FrontEndFlow instance3 = FrontEndFlow.Instance;
					if (instance3 != null)
					{
						instance3.OpenChildOnTopOfMenu(1);
					}
					else
					{
						Debug.Log("Cannot show flow instance as it is null ");
					}
					break;
				}
				Debug.Log("Ignoring dlc " + dLCFrontendData.m_DLCID + " due to save reasons");
			}
			else
			{
				Debug.Log("We have already seen dlc " + dLCFrontendData.m_DLCID);
			}
		}
	}

	public void CloseDLCPopup()
	{
		if (m_DLCPopup != null)
		{
			m_DLCPopup.OnOKClicked();
			CheckForDLCPopups();
		}
	}
}
