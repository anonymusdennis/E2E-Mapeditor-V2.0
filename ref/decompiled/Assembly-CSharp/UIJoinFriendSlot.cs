using UnityEngine;

public class UIJoinFriendSlot : MonoBehaviour
{
	private static SelectPrivateVersusFEMenu s_SelectionMenu;

	public T17Text m_FriendLabel;

	public GameObject m_FavoriteFriend;

	private Platform.DisplayableFriend m_FriendReference;

	public void Awake()
	{
		if (!(s_SelectionMenu == null))
		{
			return;
		}
		Transform parent = base.transform.parent;
		while (parent != null)
		{
			GameObject gameObject = parent.gameObject;
			s_SelectionMenu = gameObject.GetComponent<SelectPrivateVersusFEMenu>();
			if (s_SelectionMenu != null)
			{
				break;
			}
			parent = parent.parent;
		}
	}

	public void Disable()
	{
		base.gameObject.SetActive(value: false);
	}

	public void SetBlank()
	{
		base.gameObject.SetActive(value: true);
		m_FriendLabel.text = "-";
	}

	public void EnableAndSetTo(Platform.DisplayableFriend friend)
	{
		if (base.gameObject == null)
		{
			return;
		}
		base.gameObject.SetActive(value: true);
		if (friend != null)
		{
			m_FriendReference = friend;
			if (m_FriendLabel != null)
			{
				m_FriendLabel.m_bNeedsLocalization = false;
				m_FriendLabel.text = friend.m_Name;
			}
		}
	}

	public void OnButtonClicked()
	{
		if (m_FriendReference != null && !string.IsNullOrEmpty(m_FriendReference.m_OnlineID))
		{
			Debug.Log("UIJoinFriendSlot::OnButtonClicked");
			s_SelectionMenu.OnJoinButtonPressed(m_FriendReference);
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
}
