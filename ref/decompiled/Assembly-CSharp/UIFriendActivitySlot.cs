using UnityEngine;

public class UIFriendActivitySlot : MonoBehaviour
{
	public T17Text m_FriendLabel;

	public T17Text m_StatusLabel;

	public T17Text m_PresenceLabel;

	public T17Image m_GameStatus;

	public Sprite m_SpriteAvailable;

	public Sprite m_SpriteUnavailable;

	public Sprite m_SpriteOnline;

	public GameObject m_FavoriteFriend;

	public GameObject m_ContextMenu;

	private Platform.DisplayableFriend m_FriendReference;

	private bool m_ClickEnabled = true;

	public void Disable()
	{
		base.gameObject.SetActive(value: false);
	}

	public void SetBlank()
	{
		base.gameObject.SetActive(value: true);
		m_FriendLabel.text = string.Empty;
		m_StatusLabel.text = string.Empty;
		m_PresenceLabel.text = string.Empty;
		if ((bool)m_GameStatus)
		{
			m_GameStatus.gameObject.SetActive(value: false);
			m_GameStatus.sprite = m_SpriteUnavailable;
		}
	}

	public void EnableAndSetTo(Platform.DisplayableFriend activity)
	{
		if (base.gameObject == null)
		{
			return;
		}
		base.gameObject.SetActive(value: true);
		if (activity != null)
		{
			m_FriendReference = activity;
			if (m_FriendLabel != null)
			{
				m_FriendLabel.m_bNeedsLocalization = false;
				m_FriendLabel.text = activity.m_Name;
			}
			string text = string.Empty;
			switch (activity.m_ActivityState)
			{
			case Platform.DisplayableFriend.ActivityState.Unknown:
				text = "Text.OnlineState.Unknown";
				break;
			case Platform.DisplayableFriend.ActivityState.Offline:
				text = "Text.OnlineState.Offline";
				break;
			case Platform.DisplayableFriend.ActivityState.Online:
				text = "Text.OnlineState.Online";
				break;
			case Platform.DisplayableFriend.ActivityState.Away:
				text = "Text.OnlineState.Away";
				break;
			case Platform.DisplayableFriend.ActivityState.Ingame_Public:
				text = "Text.OnlineState.Online";
				break;
			case Platform.DisplayableFriend.ActivityState.Ingame_Private:
				text = "Text.OnlineState.Online";
				break;
			case Platform.DisplayableFriend.ActivityState.Ingame_Menus:
				text = "Text.OnlineState.Ingame_Menus";
				break;
			case Platform.DisplayableFriend.ActivityState.Ingame:
				text = "Text.OnlineState.Ingame";
				break;
			}
			if (m_StatusLabel != null)
			{
				m_StatusLabel.m_bNeedsLocalization = true;
				m_StatusLabel.SetNewPlaceHolder(text);
				m_StatusLabel.SetNewLocalizationTag(text);
			}
			if (m_PresenceLabel != null)
			{
				m_PresenceLabel.m_bNeedsLocalization = false;
				m_PresenceLabel.text = activity.m_Presence;
			}
		}
	}

	public void OnButtonClicked()
	{
		if (m_FriendReference == null || string.IsNullOrEmpty(m_FriendReference.m_OnlineID) || !m_ClickEnabled)
		{
			return;
		}
		if (Helpers.IsInGameplayScene() && T17NetManager.ConnectionState == T17NetConnectState.Connected && PhotonNetwork.playerList.Length < 4 && m_ContextMenu != null)
		{
			RectTransform rectTransform = m_ContextMenu.transform as RectTransform;
			RectTransform rectTransform2 = base.transform as RectTransform;
			Vector2 anchoredPosition = SwitchToRectTransform(rectTransform2, rectTransform);
			anchoredPosition.x -= rectTransform2.rect.width * 0.5f;
			rectTransform.anchoredPosition = anchoredPosition;
			FriendsContextMenu component = m_ContextMenu.GetComponent<FriendsContextMenu>();
			if (component != null)
			{
				component.ShowContextMenu(ref m_FriendReference, Gamer.GetPrimaryGamer());
			}
		}
		else
		{
			m_ContextMenu.gameObject.SetActive(value: false);
			Platform.GetInstance().ShowGamerCard(Gamer.GetPrimaryGamer(), m_FriendReference.m_OnlineID);
		}
	}

	private Vector2 SwitchToRectTransform(RectTransform from, RectTransform to)
	{
		Vector2 vector = new Vector2(from.rect.width * from.pivot.x + from.rect.xMin, from.rect.height * from.pivot.y + from.rect.yMin);
		Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, from.position);
		screenPoint += vector;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(to, screenPoint, null, out var localPoint);
		Vector2 vector2 = new Vector2(to.rect.width * to.pivot.x + to.rect.xMin, to.rect.height * to.pivot.y + to.rect.yMin);
		return to.anchoredPosition + localPoint - vector2;
	}

	public void EnableContextClick(bool enable)
	{
		m_ClickEnabled = enable;
	}
}
