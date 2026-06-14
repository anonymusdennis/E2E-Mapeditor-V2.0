using System;
using AUTOGEN_T17Wwise_Enums;
using Rewired.Integration.UnityUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FriendsContextMenu : MonoBehaviour
{
	public Platform.DisplayableFriend m_Friend;

	public Selectable m_DefaultButton;

	public T17Text m_MuteText;

	private GameObject m_SelectedBefore;

	public static bool IsContextMenuOpen;

	public T17Button m_kickPlayerButton;

	public T17Button m_mutePlayerButton;

	public T17Button m_invitePlayerButton;

	public T17Button m_showGamerCardButton;

	public T17Button m_joinGameButton;

	private Gamer m_openingGamer;

	public void ShowContextMenu(ref Platform.DisplayableFriend friend, Gamer openingGamer)
	{
		IsContextMenuOpen = true;
		base.gameObject.SetActive(value: true);
		m_Friend = friend;
		m_openingGamer = openingGamer;
		if (m_MuteText == null && null != m_mutePlayerButton)
		{
			m_MuteText = m_mutePlayerButton.GetComponentInChildren<T17Text>(includeInactive: true);
		}
		if (m_MuteText != null)
		{
			if (Platform.GetInstance().IsGamerMuted(m_Friend.m_OnlineID))
			{
				m_MuteText.SetNewPlaceHolder("TBT - UnMute");
				m_MuteText.SetNewLocalizationTag("Text.UI.Unmute");
			}
			else
			{
				m_MuteText.SetNewPlaceHolder("TBT - Mute");
				m_MuteText.SetNewLocalizationTag("Text.UI.Mute");
			}
		}
		if (null == m_kickPlayerButton)
		{
			GameObject gameObject = GameObject.Find("Kick");
			if (null != gameObject)
			{
				m_kickPlayerButton = gameObject.GetComponent<T17Button>();
			}
		}
		if (null != m_kickPlayerButton)
		{
			bool flag = false;
			ConfigManager instance = ConfigManager.GetInstance();
			if (null != instance && instance.gameType == PrisonConfig.ConfigType.Cooperative && T17NetManager.IsMasterClient && T17NetRoomManager.IsInRoom() && m_Friend.m_Gamer != null && !m_Friend.m_Gamer.IsLocal())
			{
				flag = true;
			}
			m_kickPlayerButton.interactable = flag;
			m_kickPlayerButton.gameObject.SetActive(flag);
		}
		if (null != m_mutePlayerButton)
		{
			bool active = false;
			if (T17NetManager.IsConnectedOnline() && T17NetRoomManager.IsInRoom() && m_Friend.m_Gamer != null && !m_Friend.m_Gamer.IsLocal())
			{
				active = true;
			}
			if (null != m_mutePlayerButton.gameObject)
			{
				m_mutePlayerButton.gameObject.SetActive(active);
			}
		}
		if (null != m_invitePlayerButton)
		{
			bool active2 = true;
			if (T17NetManager.IsConnectedOnline() && T17NetRoomManager.IsInRoom())
			{
				if (ConfigManager.GetInstance().gameType == PrisonConfig.ConfigType.Versus && T17NetRoomManager.CurrentGameRoomType == T17NetRoomGameView.GameRoomType.Public)
				{
					active2 = false;
				}
			}
			else
			{
				active2 = false;
			}
			if (null != m_invitePlayerButton.gameObject)
			{
				m_invitePlayerButton.gameObject.SetActive(active2);
			}
		}
		if (null != m_joinGameButton)
		{
			bool active3 = true;
			if (!T17NetManager.IsConnectedOnline() || T17NetRoomManager.IsInRoom())
			{
				active3 = false;
			}
			if (null != m_joinGameButton.gameObject)
			{
				m_joinGameButton.gameObject.SetActive(active3);
			}
		}
		if (T17NetManager.IsConnectedOnline() && T17NetRoomManager.IsInRoom())
		{
			m_showGamerCardButton.gameObject.SetActive(Platform.GetInstance().m_CrossplayLobbyManager.IsGamerForMyPlatform(m_Friend.m_Gamer));
		}
		if (RearrangeButtonsPC() == 0)
		{
			IsContextMenuOpen = false;
			base.gameObject.SetActive(value: false);
			return;
		}
		T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(openingGamer);
		if (eventSystemForGamer != null)
		{
			m_SelectedBefore = eventSystemForGamer.currentSelectedGameObject;
			eventSystemForGamer.SetSelectedGameObject(null);
			eventSystemForGamer.SetSelectedGameObject(m_DefaultButton.gameObject);
		}
		RewiredStandaloneInputModule.OnMouseClickDown = (RewiredStandaloneInputModule.MouseEventDelegate)Delegate.Combine(RewiredStandaloneInputModule.OnMouseClickDown, new RewiredStandaloneInputModule.MouseEventDelegate(MouseClick));
	}

	private void Update()
	{
		if (m_openingGamer != null && m_openingGamer.m_RewiredPlayer.GetButtonUp("UI_Cancel"))
		{
			Hide();
		}
	}

	private void Hide()
	{
		IsContextMenuOpen = false;
		T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_openingGamer);
		if (eventSystemForGamer != null)
		{
			eventSystemForGamer.SetSelectedGameObject(null);
			eventSystemForGamer.SetSelectedGameObject(m_SelectedBefore);
		}
		base.gameObject.SetActive(value: false);
		RewiredStandaloneInputModule.OnMouseClickDown = (RewiredStandaloneInputModule.MouseEventDelegate)Delegate.Remove(RewiredStandaloneInputModule.OnMouseClickDown, new RewiredStandaloneInputModule.MouseEventDelegate(MouseClick));
	}

	private int RearrangeButtons()
	{
		return 0;
	}

	private int RearrangeButtonsPC()
	{
		int num = 0;
		bool flag = m_joinGameButton != null && m_joinGameButton.IsActive();
		bool flag2 = m_kickPlayerButton != null && m_kickPlayerButton.IsActive();
		bool flag3 = m_mutePlayerButton != null && m_mutePlayerButton.IsActive();
		bool flag4 = m_invitePlayerButton != null && m_invitePlayerButton.IsActive();
		bool flag5 = m_showGamerCardButton != null && m_showGamerCardButton.IsActive();
		if (flag5)
		{
			m_showGamerCardButton.transform.SetAsLastSibling();
			Navigation navigation = m_showGamerCardButton.navigation;
			navigation.selectOnUp = (flag2 ? m_kickPlayerButton : (flag3 ? m_mutePlayerButton : (flag ? m_joinGameButton : ((!flag4) ? null : m_invitePlayerButton))));
			navigation.selectOnDown = (flag3 ? m_mutePlayerButton : (flag2 ? m_kickPlayerButton : (flag4 ? m_invitePlayerButton : ((!flag) ? null : m_joinGameButton))));
			m_showGamerCardButton.navigation = navigation;
			num++;
		}
		if (flag3)
		{
			m_mutePlayerButton.transform.SetAsLastSibling();
			Navigation navigation = m_mutePlayerButton.navigation;
			navigation.selectOnUp = (flag5 ? m_showGamerCardButton : ((!flag2) ? null : m_kickPlayerButton));
			navigation.selectOnDown = (flag2 ? m_kickPlayerButton : ((!flag5) ? null : m_showGamerCardButton));
			m_showGamerCardButton.navigation = navigation;
			num++;
		}
		if (flag2)
		{
			m_kickPlayerButton.transform.SetAsLastSibling();
			Navigation navigation = m_kickPlayerButton.navigation;
			navigation.selectOnUp = (flag3 ? m_mutePlayerButton : ((!flag5) ? null : m_showGamerCardButton));
			navigation.selectOnDown = (flag5 ? m_showGamerCardButton : ((!flag3) ? null : m_mutePlayerButton));
			m_kickPlayerButton.navigation = navigation;
			num++;
		}
		if (flag4)
		{
			m_invitePlayerButton.transform.SetAsLastSibling();
			Navigation navigation = m_invitePlayerButton.navigation;
			navigation.selectOnUp = (flag5 ? m_showGamerCardButton : ((!flag) ? null : m_joinGameButton));
			navigation.selectOnDown = (flag ? m_joinGameButton : ((!flag5) ? null : m_showGamerCardButton));
			m_invitePlayerButton.navigation = navigation;
			num++;
		}
		if (flag)
		{
			m_joinGameButton.transform.SetAsLastSibling();
			Navigation navigation = m_joinGameButton.navigation;
			navigation.selectOnUp = (flag4 ? m_invitePlayerButton : ((!flag5) ? null : m_showGamerCardButton));
			navigation.selectOnDown = (flag5 ? m_showGamerCardButton : ((!flag4) ? null : m_invitePlayerButton));
			m_joinGameButton.navigation = navigation;
			num++;
		}
		m_DefaultButton = (flag5 ? m_showGamerCardButton : (flag3 ? m_mutePlayerButton : (flag2 ? m_kickPlayerButton : (flag4 ? m_invitePlayerButton : ((!flag) ? null : m_joinGameButton)))));
		return num;
	}

	public bool IsEmpty(Platform.DisplayableFriend friend)
	{
		return true;
	}

	public void OnJoinGamePressed()
	{
	}

	public void OnShowGamerCard()
	{
		if (!string.IsNullOrEmpty(m_Friend.m_OnlineID))
		{
			Platform.GetInstance().ShowGamerCard(m_openingGamer, m_Friend.m_OnlineID);
		}
	}

	public void OnInvite()
	{
		Platform.GetInstance().SendInvite(m_Friend.m_OnlineID);
		Hide();
	}

	public void OnMute()
	{
		if (Platform.GetInstance().ToggleMuteForGamer(m_Friend.m_OnlineID))
		{
			if (m_MuteText != null)
			{
				m_MuteText.SetNewPlaceHolder("TBT - UnMute");
				m_MuteText.SetNewLocalizationTag("Text.UI.Unmute");
			}
		}
		else if (m_MuteText != null)
		{
			m_MuteText.SetNewPlaceHolder("TBT - Mute");
			m_MuteText.SetNewLocalizationTag("Text.UI.Mute");
		}
	}

	public void OnKick()
	{
		NetUserManager.Instance.KickGamer(m_Friend.m_Gamer);
		Hide();
	}

	protected virtual void OnDestroy()
	{
		IsContextMenuOpen = false;
		RewiredStandaloneInputModule.OnMouseClickDown = (RewiredStandaloneInputModule.MouseEventDelegate)Delegate.Remove(RewiredStandaloneInputModule.OnMouseClickDown, new RewiredStandaloneInputModule.MouseEventDelegate(MouseClick));
	}

	private void MouseClick(RewiredStandaloneInputModule module, PointerInputModule.MouseButtonEventData data)
	{
		Vector2 position = data.buttonData.position;
		RectTransform rect = (RectTransform)base.transform;
		if (!RectTransformUtility.RectangleContainsScreenPoint(rect, position))
		{
			Hide();
			NavigateOnUICancel component = GetComponent<NavigateOnUICancel>();
			if (component != null)
			{
				component.m_DoThisOnUICancel.Invoke();
			}
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Reject, AudioController.UI_Audio_GO);
		}
	}
}
