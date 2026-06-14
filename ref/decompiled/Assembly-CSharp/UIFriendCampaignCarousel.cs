using System.Collections.Generic;
using UnityEngine;

public class UIFriendCampaignCarousel : UICarousel<UIFriendCampaignCarousel.FriendActivityPage>
{
	public class FriendActivityPage
	{
		public Platform.DisplayableFriend[] m_FriendsActivity;
	}

	public Transform m_FriendSlotContainer;

	public T17Text m_PageText;

	public UIFriendCampaignArrows m_PageArrows;

	private UIFriendActivitySlot[] m_FriendSlots;

	private List<Platform.DisplayableFriend> m_DisplayableFriends;

	private List<FriendActivityPage> m_FriendPages = new List<FriendActivityPage>();

	protected override void Awake()
	{
		base.Awake();
		if (m_FriendSlotContainer == null)
		{
		}
		m_FriendSlots = m_FriendSlotContainer.GetComponentsInChildren<UIFriendActivitySlot>(includeInactive: true);
	}

	protected override void OnDestroy()
	{
		Platform.GetInstance().CancelFriendsListRequest();
		base.OnDestroy();
	}

	public void DisableFriendFeed()
	{
		m_FriendPages.Clear();
		if (m_Options != null && m_Options.Count > 0)
		{
			SelectIndex(0);
		}
		FriendActivityPage friendActivityPage = new FriendActivityPage();
		m_FriendPages.Add(friendActivityPage);
		int num = m_FriendSlots.Length;
		friendActivityPage.m_FriendsActivity = new Platform.DisplayableFriend[num];
		for (int i = 0; i < num; i++)
		{
			friendActivityPage.m_FriendsActivity[i] = new Platform.DisplayableFriend();
			Localization.Get("Text.UI.Empty", out friendActivityPage.m_FriendsActivity[i].m_Name);
		}
		SetCarouselOptions(m_FriendPages);
		if (m_PageArrows != null)
		{
			m_PageArrows.EnableArrows(enable: false);
		}
	}

	public void PopulateWithFriends()
	{
		DisableFriendFeed();
		Platform.GetInstance().RequestFriendsList(OnFriendsListObtained);
	}

	private void OnFriendsListObtained(List<Platform.DisplayableFriend> list)
	{
		if (m_DisplayableFriends != null)
		{
			m_DisplayableFriends.Clear();
		}
		m_DisplayableFriends = null;
		m_DisplayableFriends = list;
		if (list != null && list.Count > 0)
		{
			InternalPopulate();
		}
		else
		{
			DisableFriendFeed();
		}
	}

	private void OnFriendsListObtainedPartial(List<Platform.DisplayableFriend> list)
	{
		m_DisplayableFriends = list;
		if (list != null && list.Count > 0)
		{
			InternalPopulate();
		}
		else
		{
			DisableFriendFeed();
		}
	}

	private void InternalPopulate()
	{
		m_FriendPages.Clear();
		int num = m_FriendSlots.Length;
		int num2 = Mathf.CeilToInt((float)m_DisplayableFriends.Count / (float)num);
		int num3 = 0;
		for (int i = 0; i < num2; i++)
		{
			FriendActivityPage friendActivityPage = new FriendActivityPage();
			friendActivityPage.m_FriendsActivity = new Platform.DisplayableFriend[num];
			for (int j = 0; j < num; j++)
			{
				if (num3 < m_DisplayableFriends.Count)
				{
					friendActivityPage.m_FriendsActivity[j] = m_DisplayableFriends[num3];
					num3++;
				}
				else
				{
					friendActivityPage.m_FriendsActivity[j] = new Platform.DisplayableFriend();
				}
			}
			m_FriendPages.Add(friendActivityPage);
		}
		if (m_PageArrows != null)
		{
			m_PageArrows.SetNumberOfPagesAvailable(num2);
		}
		SetCarouselOptions(m_FriendPages);
	}

	protected override void UpdateUIForSelectedIndex(int index)
	{
		if (m_FriendPages != null && m_FriendSlots != null && index < m_FriendPages.Count)
		{
			FriendActivityPage friendActivityPage = m_FriendPages[index];
			if (m_FriendSlots.Length < friendActivityPage.m_FriendsActivity.Length)
			{
			}
			int i = 0;
			for (int j = 0; j < friendActivityPage.m_FriendsActivity.Length; j++)
			{
				m_FriendSlots[i++].EnableAndSetTo(friendActivityPage.m_FriendsActivity[j]);
			}
			for (; i < m_FriendSlots.Length; i++)
			{
				m_FriendSlots[i].SetBlank();
			}
			if (m_PageText != null)
			{
				Localization.Get("Text.Menu.PageTitle", out var localized);
				m_PageText.m_bNeedsLocalization = false;
				m_PageText.text = localized + (index + 1) + "/" + m_FriendPages.Count;
			}
		}
		else
		{
			if (m_FriendPages == null)
			{
			}
			if (m_FriendSlots == null)
			{
			}
			if (index < m_FriendPages.Count)
			{
			}
		}
	}

	public void EnableContextClick(bool enable)
	{
		for (int i = 0; i < m_FriendSlots.Length; i++)
		{
			m_FriendSlots[i].EnableContextClick(enable);
		}
	}
}
