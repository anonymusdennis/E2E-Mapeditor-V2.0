using System.Collections.Generic;
using UnityEngine;

public class UIJoinFriendCarousel : UICarousel<UIJoinFriendCarousel.FriendGamesPage>
{
	public class FriendGamesPage
	{
		public Platform.DisplayableFriend[] m_FriendGames;
	}

	public Transform m_FriendGamesSlotContainer;

	public T17Text m_PageText;

	public UIFriendCampaignArrows m_PageArrows;

	private UIJoinFriendSlot[] m_FriendSlots;

	private List<Platform.DisplayableFriend> m_DisplayableFriends;

	private List<FriendGamesPage> m_FriendPages = new List<FriendGamesPage>();

	protected override void Awake()
	{
		base.Awake();
		if (m_FriendGamesSlotContainer == null)
		{
		}
		m_FriendSlots = m_FriendGamesSlotContainer.GetComponentsInChildren<UIJoinFriendSlot>(includeInactive: true);
	}

	protected override void OnDestroy()
	{
		Platform.GetInstance().CancelFriendsListRequest();
		base.OnDestroy();
	}

	public void DisableFriendGames()
	{
		m_FriendPages.Clear();
		if (m_Options != null && m_Options.Count > 0)
		{
			SelectIndex(0);
		}
		FriendGamesPage friendGamesPage = new FriendGamesPage();
		m_FriendPages.Add(friendGamesPage);
		int num = m_FriendSlots.Length;
		friendGamesPage.m_FriendGames = new Platform.DisplayableFriend[num];
		for (int i = 0; i < num; i++)
		{
			friendGamesPage.m_FriendGames[i] = new Platform.DisplayableFriend();
			Localization.Get("Text.UI.Empty", out friendGamesPage.m_FriendGames[i].m_Name);
		}
		SetCarouselOptions(m_FriendPages);
		if (m_PageArrows != null)
		{
			m_PageArrows.EnableArrows(enable: false);
		}
	}

	public void PopulateWithFriends()
	{
		DisableFriendGames();
		Platform.GetInstance().RequestFriendsList(OnFriendsListObtained);
	}

	private void OnFriendsListObtained(List<Platform.DisplayableFriend> list)
	{
		if (m_DisplayableFriends != null)
		{
			m_DisplayableFriends.Clear();
		}
		m_DisplayableFriends = new List<Platform.DisplayableFriend>();
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				m_DisplayableFriends.Add(list[i]);
			}
		}
		if (m_DisplayableFriends.Count > 0)
		{
			InternalPopulate();
		}
		else
		{
			DisableFriendGames();
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
			FriendGamesPage friendGamesPage = new FriendGamesPage();
			friendGamesPage.m_FriendGames = new Platform.DisplayableFriend[num];
			for (int j = 0; j < num; j++)
			{
				if (num3 < m_DisplayableFriends.Count)
				{
					friendGamesPage.m_FriendGames[j] = m_DisplayableFriends[num3];
					num3++;
				}
				else
				{
					friendGamesPage.m_FriendGames[j] = new Platform.DisplayableFriend();
				}
			}
			m_FriendPages.Add(friendGamesPage);
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
			FriendGamesPage friendGamesPage = m_FriendPages[index];
			if (m_FriendSlots.Length < friendGamesPage.m_FriendGames.Length)
			{
			}
			int i = 0;
			for (int j = 0; j < friendGamesPage.m_FriendGames.Length; j++)
			{
				m_FriendSlots[i++].EnableAndSetTo(friendGamesPage.m_FriendGames[j]);
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
}
