using UnityEngine;

public class SelectPrivateVersusFEMenu : FrontendMenuBehaviour
{
	public T17Button m_StartButton;

	public UIJoinFriendCarousel m_FriendGamesCarousel;

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		bool result = base.Show(currentGamer, parent, invoker, hideInvoker);
		VersusFrontendMenu.RoomType = T17NetRoomGameView.GameRoomType.Private;
		Platform.GetInstance().SetPresenceTag("Text.Presence.VersusOnline");
		FrontEndFlow.Instance.SetInMultiUserMenu(this);
		m_StartButton.interactable = true;
		m_FriendGamesCarousel.PopulateWithFriends();
		return result;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		T17NetLobbyManager.Instance.CancelFindRoom();
		Platform.GetInstance().CancelFriendsListRequest();
		if (FrontEndFlow.Instance != null)
		{
			FrontEndFlow.Instance.SetInMultiUserMenu(null);
		}
		return base.Hide(restoreInvokerState, isTabSwitch);
	}

	public void NewDisallowedUserCallback()
	{
		FrontendMenuBehaviour frontendMenuBehaviour = FrontEndFlow.Instance.InMultiUserMenu();
		if (frontendMenuBehaviour != null)
		{
			frontendMenuBehaviour.Hide();
		}
		NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
		Platform.GetInstance().ExitOnlineArea();
	}

	public void OnJoinedRoomResult(bool isConnected)
	{
		if (isConnected && PhotonNetwork.room != null)
		{
			Hide();
			FrontEndFlow.Instance.OpenChildOnTopOfMenu(3);
		}
		else
		{
			ErrorDialogHandler.ShowError(ErrorDialogHandler.Type.InviteJoinFailed);
		}
	}

	public void OnJoinButtonPressed(Platform.DisplayableFriend friend)
	{
		NetJoinRoomHelper.FindAndJoinRoom(friend.m_OnlineID, showReconnectPrompt: false, OnJoinedRoomResult);
	}

	public void On_CancelButtonPressed()
	{
		NavigateOnUICancel component = GetComponent<NavigateOnUICancel>();
		if (component != null)
		{
			component.m_DoThisOnUICancel.Invoke();
		}
	}

	public void OnCreateButtonPressed()
	{
		Hide();
		Debug.Log(" **** OnCreatePrivateRoom ***");
		FrontEndFlow.Instance.OpenChildOnTopOfMenu(3);
	}
}
