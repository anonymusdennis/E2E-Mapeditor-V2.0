using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LobbyPlayerObject : MonoBehaviour
{
	public T17Text m_Label;

	public GameObject m_ReadyContainer;

	public GameObject m_SpinnerContainer;

	public GameObject m_FilledPlayer;

	public GameObject m_OutlinePlayer;

	public UnityEvent OnSlotPressed;

	public GameObject m_UserTalkingIcon;

	public GameObject m_UserMutedIcon;

	public FriendsContextMenu m_ContextMenu;

	public Platform.DisplayableFriend m_DisplayableFriend;

	public void SetPlayerReady()
	{
		if (m_ReadyContainer != null)
		{
			m_ReadyContainer.SetActive(value: true);
		}
		if (m_SpinnerContainer != null)
		{
			m_SpinnerContainer.SetActive(value: false);
		}
		if (m_FilledPlayer != null)
		{
			m_FilledPlayer.SetActive(value: true);
		}
		if (m_OutlinePlayer != null)
		{
			m_OutlinePlayer.SetActive(value: false);
		}
	}

	public void SetPlayerEmpty()
	{
		if (m_ReadyContainer != null)
		{
			m_ReadyContainer.SetActive(value: false);
		}
		if (m_SpinnerContainer != null)
		{
			m_SpinnerContainer.SetActive(value: true);
		}
		if (m_FilledPlayer != null)
		{
			m_FilledPlayer.SetActive(value: false);
		}
		if (m_OutlinePlayer != null)
		{
			m_OutlinePlayer.SetActive(value: true);
		}
		if (m_Label != null)
		{
			m_Label.m_LocalizationTag = "XXX.YYY.ZZZ";
			m_Label.m_PlaceholderText = string.Empty;
			m_Label.m_bNeedsLocalization = true;
		}
		m_DisplayableFriend = null;
	}

	public virtual void OnSelectButtonPressed()
	{
		if (OnSlotPressed != null && m_DisplayableFriend == null)
		{
			OnSlotPressed.Invoke();
		}
		else if (m_ContextMenu != null && m_DisplayableFriend != null)
		{
			RectTransform rectTransform = m_ContextMenu.transform as RectTransform;
			RectTransform rectTransform2 = base.transform as RectTransform;
			Vector2 anchoredPosition = rectTransform2.anchoredPosition;
			anchoredPosition.x = rectTransform2.anchoredPosition.x - rectTransform2.rect.width * 0.5f;
			anchoredPosition.y = rectTransform2.anchoredPosition.y + rectTransform2.rect.height * 0.5f;
			rectTransform.anchoredPosition = anchoredPosition;
			if (m_ContextMenu != null)
			{
				m_ContextMenu.ShowContextMenu(ref m_DisplayableFriend, Gamer.GetPrimaryGamer());
			}
		}
	}

	public void UpdateTalkingIconsWithTalkingGamers(List<Platform.VoiceChatGamer> m_VoiceChatGamers)
	{
		if (m_DisplayableFriend != null && m_DisplayableFriend.m_Gamer != null)
		{
			for (int num = m_VoiceChatGamers.Count - 1; num >= 0; num--)
			{
				if (m_VoiceChatGamers[num] != null && m_VoiceChatGamers[num].m_Gamer == m_DisplayableFriend.m_Gamer)
				{
					bool bIsMuted = m_VoiceChatGamers[num].m_bIsMuted;
					bool bIsTalking = m_VoiceChatGamers[num].m_bIsTalking;
					if (bIsMuted || bIsTalking)
					{
						if (m_UserTalkingIcon != null)
						{
							m_UserTalkingIcon.SetActive(bIsTalking && !bIsMuted);
						}
						if (m_UserMutedIcon != null)
						{
							m_UserMutedIcon.SetActive(bIsMuted);
						}
					}
					else
					{
						if (m_UserTalkingIcon != null)
						{
							m_UserTalkingIcon.SetActive(value: false);
						}
						if (m_UserMutedIcon != null)
						{
							m_UserMutedIcon.SetActive(value: false);
						}
					}
					return;
				}
			}
		}
		if (m_UserTalkingIcon != null)
		{
			m_UserTalkingIcon.SetActive(value: false);
		}
		if (m_UserMutedIcon != null)
		{
			m_UserMutedIcon.SetActive(value: false);
		}
	}
}
