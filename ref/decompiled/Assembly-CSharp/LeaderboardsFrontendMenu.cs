using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;

public class LeaderboardsFrontendMenu : FrontendMenuBehaviour
{
	public GameObject m_LoadingBackground;

	public GameObject m_LoadingIcon;

	public T17ScrollView m_ScrollView;

	public GameObject m_LBInfoObjectPrefab;

	public GameObject m_DefaultSelectableOnBadNavigation;

	public T17Text m_PrisonFilter;

	public T17Text m_LBTypeFilter;

	public T17Text m_LBGameTypeFilter;

	public T17Text m_LBNoEntriesText;

	public T17Text m_ToolTip;

	public int m_MaxEmptySpaces = 10;

	private List<LeaderboardsEntry> m_CurrentAttachedInfoObjects = new List<LeaderboardsEntry>();

	private LevelScript.LEADERBOARD_PRISON_ENUM m_CurrentPrison = LevelScript.LEADERBOARD_PRISON_ENUM.Centre_Perks;

	private Platform.LeaderboardType m_CurrentLBType;

	private Platform.LeaderboardGameType m_CurrentLBGameType;

	private const float WAIT_TIMEOUT = 0.5f;

	private float m_RequestTimeout;

	private int m_CurrentLeaderboardIndex = 1;

	private bool m_bResetToTop = true;

	private bool m_bUpPressed;

	private bool m_bInvalidUpPressed;

	private bool m_bDownPressed;

	private bool m_bInvalidDownPressed;

	private bool m_bCancelRequested;

	private const int kMaxNumEntries = 100;

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		ClearLeaderboard();
		Platform.GetInstance().SetPresenceTag("Text.Presence.FrontEndLeaderboards");
		if (m_LoadingIcon != null)
		{
			m_LoadingIcon.SetActive(value: true);
		}
		if (m_LoadingBackground != null)
		{
			m_LoadingBackground.SetActive(value: true);
		}
		m_CurrentPrison = LevelScript.LEADERBOARD_PRISON_ENUM.Centre_Perks;
		m_CurrentLBType = Platform.LeaderboardType.Overall;
		m_CurrentLBGameType = Platform.LeaderboardGameType.SinglePlayer;
		m_CurrentLeaderboardIndex = 1;
		m_PrisonFilter.SetLocalisedTextCatchAll("Text.Prison." + m_CurrentPrison);
		m_LBTypeFilter.SetLocalisedTextCatchAll("Text.Leaderboards." + m_CurrentLBType);
		m_LBGameTypeFilter.SetLocalisedTextCatchAll("Text.LeaderboardsGameType." + m_CurrentLBGameType);
		m_ToolTip.SetLocalisedTextCatchAll("Text.Leaderboards.ToolTip." + m_CurrentLBGameType);
		Platform.GetInstance().RequestLeaderboard(m_CurrentLBType, LevelScript.MapLeaderboardToPrisonEnum(m_CurrentPrison), m_CurrentLBGameType, OnResultsLoaded, m_CurrentLeaderboardIndex);
		if (m_ScrollView != null)
		{
			m_ScrollView.Show(currentGamer, this, null, hideInvoker: false);
		}
		SetNoEntriesTextActive(bActive: false);
		return true;
	}

	private void OnResultsLoaded(bool bOK, Platform.DisplayableRank[] ranks, int totalRows, bool bShowError, bool bContentBlocked)
	{
		if (m_LoadingIcon != null)
		{
			m_LoadingIcon.SetActive(value: false);
		}
		if (m_LoadingBackground != null)
		{
			m_LoadingBackground.SetActive(value: false);
		}
		ClearLeaderboard();
		if (!bOK)
		{
			SetNoEntriesTextActive(bActive: false);
		}
		else if (ranks != null && ranks.Length > 0)
		{
			SetNoEntriesTextActive(bActive: false);
			string platformUniqueID = base.CurrentGamer.m_PlatformUniqueID;
			bool flag = false;
			int i;
			for (i = 0; i < ranks.Length; i++)
			{
				Platform.DisplayableRank displayableRank = ranks[i];
				if (displayableRank != null)
				{
					if (displayableRank.m_OnlineID == platformUniqueID)
					{
						displayableRank.m_bMyScore = true;
						flag = true;
					}
					AddLeaderboardEntry(displayableRank, m_CurrentLBType == Platform.LeaderboardType.MyScore);
				}
			}
			for (; i < m_MaxEmptySpaces; i++)
			{
				AddEmptyLeaderboardEntry();
			}
			if (base.CachedEventSystem != null)
			{
				bool flag2 = false;
				if (base.CachedEventSystem.currentSelectedGameObject == null)
				{
					flag2 = true;
				}
				else
				{
					LeaderboardsEntry component = base.CachedEventSystem.currentSelectedGameObject.GetComponent<LeaderboardsEntry>();
					if (component != null && !m_CurrentAttachedInfoObjects.Contains(component))
					{
						flag2 = true;
					}
				}
				if (flag2 && m_CurrentLBType != Platform.LeaderboardType.MyScore && flag)
				{
					m_ScrollView.ResetSelection(isTop: true);
					if (base.CachedEventSystem.currentSelectedGameObject == null && m_DefaultSelectableOnBadNavigation != null)
					{
						base.CachedEventSystem.SetSelectedGameObject(m_DefaultSelectableOnBadNavigation);
					}
				}
			}
		}
		else
		{
			SetNoEntriesTextActive(bActive: true);
		}
		if (!bShowError)
		{
			return;
		}
		Platform.GetInstance().GetOnlineAccessCode(delegate(Platform.OnlineAccessErrorCode onlineAccessCode, bool bShowSystemDialogues)
		{
			switch (onlineAccessCode)
			{
			case Platform.OnlineAccessErrorCode.OnlineAccessOK:
			{
				T17DialogBox dialog2 = T17DialogBoxManager.GetDialog(forSingleUser: false);
				if (dialog2 != null)
				{
					dialog2.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Leaderboards.FailedToRead", "Text.Leaderboards.FailedToRead.Body", "Text.Dialog.Prompt.Ok", string.Empty, string.Empty);
					dialog2.SetSymbol(T17DialogBox.Symbols.Error);
					dialog2.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog2.OnConfirm, new T17DialogBox.DialogEvent(OnFailedConfirmed));
					dialog2.Show();
				}
				break;
			}
			default:
				if (Platform.GetInstance().DisplayNativeDialogForOnlineAccessCode(onlineAccessCode))
				{
					break;
				}
				goto case Platform.OnlineAccessErrorCode.NotConnectedToNet;
			case Platform.OnlineAccessErrorCode.NotConnectedToNet:
			{
				T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
				if (dialog != null)
				{
					dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Dialog.NoConnection", "Text.Dialog.NoConnection.Body", "Text.Dialog.Prompt.Ok", string.Empty, string.Empty);
					dialog.SetSymbol(T17DialogBox.Symbols.Error);
					dialog.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnConfirm, new T17DialogBox.DialogEvent(OnFailedConfirmed));
					dialog.Show();
				}
				break;
			}
			}
		}, bShowSystemDialogues: true);
	}

	private void AddLeaderboardEntry(Platform.DisplayableRank data, bool shouldfocusMyScore = false)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(m_LBInfoObjectPrefab);
		m_ScrollView.AddNewObject(gameObject);
		LeaderboardsEntry component = gameObject.GetComponent<LeaderboardsEntry>();
		if (component != null)
		{
			component.SetLBInfo(data);
			m_CurrentAttachedInfoObjects.Add(component);
			if (shouldfocusMyScore && data.m_bMyScore)
			{
				m_ScrollView.ScrollToEntry(gameObject, bSelect: true);
				m_ScrollView.ResetSelection(isTop: false);
			}
		}
	}

	private void AddEmptyLeaderboardEntry()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(m_LBInfoObjectPrefab);
		m_ScrollView.AddNewObject(gameObject);
		LeaderboardsEntry component = gameObject.GetComponent<LeaderboardsEntry>();
		if (component != null)
		{
			component.SetBlank();
			m_CurrentAttachedInfoObjects.Add(component);
		}
	}

	private void ClearLeaderboard()
	{
		m_CurrentAttachedInfoObjects.Clear();
		m_ScrollView.ResetPosition();
		m_ScrollView.ClearContents();
	}

	private void OnFailedConfirmed(T17DialogBox box)
	{
		FrontEndFlow.Instance.SwitchBackToMainMenu();
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		Platform.GetInstance().SetPresenceTag("Text.Presence.FrontEndGeneral");
		Platform.GetInstance().CancelRequestLeaderboard(null);
		ClearLeaderboard();
		m_CurrentLeaderboardIndex = 1;
		m_bResetToTop = true;
		return true;
	}

	public void ChangePrisonFilterPos()
	{
		m_CurrentPrison++;
		if (m_CurrentPrison >= LevelScript.LEADERBOARD_PRISON_ENUM.kMaxLeaderboardPrisons)
		{
			m_CurrentPrison = LevelScript.LEADERBOARD_PRISON_ENUM.Centre_Perks;
		}
		if (m_CurrentLBGameType == Platform.LeaderboardGameType.Versus)
		{
			while (LevelDataManager.GetInstance().IsDLCLevel(LevelScript.MapLeaderboardToPrisonEnum(m_CurrentPrison)))
			{
				m_CurrentPrison++;
				if (m_CurrentPrison >= LevelScript.LEADERBOARD_PRISON_ENUM.kMaxLeaderboardPrisons)
				{
					m_CurrentPrison = LevelScript.LEADERBOARD_PRISON_ENUM.Centre_Perks;
				}
			}
		}
		UpdateLeaderBoard();
		m_PrisonFilter.SetLocalisedTextCatchAll("Text.Prison." + m_CurrentPrison);
	}

	public void ChangePrisonFilterNeg()
	{
		m_CurrentPrison--;
		if (m_CurrentPrison <= (LevelScript.LEADERBOARD_PRISON_ENUM)0)
		{
			m_CurrentPrison = LevelScript.LEADERBOARD_PRISON_ENUM.DLC06;
		}
		if (m_CurrentLBGameType == Platform.LeaderboardGameType.Versus)
		{
			while (LevelDataManager.GetInstance().IsDLCLevel(LevelScript.MapLeaderboardToPrisonEnum(m_CurrentPrison)))
			{
				m_CurrentPrison--;
				if (m_CurrentPrison <= (LevelScript.LEADERBOARD_PRISON_ENUM)0)
				{
					m_CurrentPrison = LevelScript.LEADERBOARD_PRISON_ENUM.DLC06;
				}
			}
		}
		UpdateLeaderBoard();
		m_PrisonFilter.SetLocalisedTextCatchAll("Text.Prison." + m_CurrentPrison);
	}

	public void ChangeLBTypePos()
	{
		m_CurrentLBType++;
		if (m_CurrentLBType >= Platform.LeaderboardType.COUNT)
		{
			m_CurrentLBType = Platform.LeaderboardType.Overall;
		}
		UpdateLeaderBoard();
		m_LBTypeFilter.SetLocalisedTextCatchAll("Text.Leaderboards." + m_CurrentLBType);
	}

	public void ChangeLBTypeNeg()
	{
		m_CurrentLBType--;
		if (m_CurrentLBType < Platform.LeaderboardType.Overall)
		{
			m_CurrentLBType = Platform.LeaderboardType.Friends;
		}
		UpdateLeaderBoard();
		m_LBTypeFilter.SetLocalisedTextCatchAll("Text.Leaderboards." + m_CurrentLBType);
	}

	public void ChangeLBGameTypePos()
	{
		m_CurrentLBGameType++;
		if (m_CurrentLBGameType >= Platform.LeaderboardGameType.COUNT)
		{
			m_CurrentLBGameType = Platform.LeaderboardGameType.SinglePlayer;
		}
		if (m_CurrentLBGameType == Platform.LeaderboardGameType.Versus)
		{
			while (LevelDataManager.GetInstance().IsDLCLevel(LevelScript.MapLeaderboardToPrisonEnum(m_CurrentPrison)))
			{
				m_CurrentPrison++;
				if (m_CurrentPrison >= LevelScript.LEADERBOARD_PRISON_ENUM.kMaxLeaderboardPrisons)
				{
					m_CurrentPrison = LevelScript.LEADERBOARD_PRISON_ENUM.Centre_Perks;
				}
				m_PrisonFilter.SetLocalisedTextCatchAll("Text.Prison." + m_CurrentPrison);
			}
		}
		UpdateLeaderBoard();
		m_LBGameTypeFilter.SetLocalisedTextCatchAll("Text.LeaderboardsGameType." + m_CurrentLBGameType);
		m_ToolTip.SetLocalisedTextCatchAll("Text.Leaderboards.ToolTip." + m_CurrentLBGameType);
	}

	public void ChangeLBGameTypeNeg()
	{
		m_CurrentLBGameType--;
		if (m_CurrentLBGameType < Platform.LeaderboardGameType.SinglePlayer)
		{
			m_CurrentLBGameType = Platform.LeaderboardGameType.Versus;
		}
		if (m_CurrentLBGameType == Platform.LeaderboardGameType.Versus)
		{
			while (LevelDataManager.GetInstance().IsDLCLevel(LevelScript.MapLeaderboardToPrisonEnum(m_CurrentPrison)))
			{
				m_CurrentPrison++;
				if (m_CurrentPrison >= LevelScript.LEADERBOARD_PRISON_ENUM.kMaxLeaderboardPrisons)
				{
					m_CurrentPrison = LevelScript.LEADERBOARD_PRISON_ENUM.Centre_Perks;
				}
				m_PrisonFilter.SetLocalisedTextCatchAll("Text.Prison." + m_CurrentPrison);
			}
		}
		UpdateLeaderBoard();
		m_LBGameTypeFilter.SetLocalisedTextCatchAll("Text.LeaderboardsGameType." + m_CurrentLBGameType);
		m_ToolTip.SetLocalisedTextCatchAll("Text.Leaderboards.ToolTip." + m_CurrentLBGameType);
	}

	public void LeaderboardPageUp()
	{
		if (m_ScrollView != null)
		{
			m_ScrollView.PageUp();
		}
	}

	public void LeaderboardPageDown()
	{
		if (m_ScrollView != null)
		{
			m_ScrollView.PageDown();
		}
	}

	public void CancelRequestCompleted()
	{
		m_bCancelRequested = false;
	}

	public void UpdateLeaderBoard()
	{
		m_bCancelRequested = true;
		Platform.GetInstance().CancelRequestLeaderboard(CancelRequestCompleted);
		ClearLeaderboard();
		m_CurrentLeaderboardIndex = 1;
		m_bResetToTop = true;
		SetNoEntriesTextActive(bActive: false);
		if (!Platform.GetInstance().OnlineCheck())
		{
			T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
			if (dialog != null)
			{
				dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Dialog.NoConnection", "Text.Dialog.NoConnection.Body", "Text.Dialog.Prompt.Ok", string.Empty, string.Empty);
				dialog.SetSymbol(T17DialogBox.Symbols.Error);
				dialog.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnConfirm, new T17DialogBox.DialogEvent(OnFailedConfirmed));
				dialog.Show();
			}
		}
		else
		{
			if (m_LoadingIcon != null)
			{
				m_LoadingIcon.SetActive(value: true);
			}
			if (m_LoadingBackground != null)
			{
				m_LoadingBackground.SetActive(value: true);
			}
			m_RequestTimeout = 0.5f;
		}
	}

	public void SetNoEntriesTextActive(bool bActive)
	{
		if (m_LBNoEntriesText != null && m_LBNoEntriesText.gameObject != null)
		{
			m_LBNoEntriesText.gameObject.SetActive(bActive);
		}
	}

	protected override void Update()
	{
		base.Update();
		Rewired.Player currentRewiredPlayer = base.CurrentRewiredPlayer;
		if (currentRewiredPlayer != null)
		{
			if (currentRewiredPlayer.GetButtonUp("UI_PrevPage"))
			{
				if (m_ScrollView.GetCurrentSelected() == 0 && m_CurrentLeaderboardIndex > 1 && m_CurrentLBType != Platform.LeaderboardType.MyScore)
				{
					m_CurrentLeaderboardIndex -= 100;
					m_CurrentLeaderboardIndex = Mathf.Max(1, m_CurrentLeaderboardIndex);
					if (m_LoadingIcon != null)
					{
						m_LoadingIcon.SetActive(value: true);
					}
					if (m_LoadingBackground != null)
					{
						m_LoadingBackground.SetActive(value: true);
					}
					m_bResetToTop = false;
					Platform.GetInstance().RequestLeaderboard(m_CurrentLBType, LevelScript.MapLeaderboardToPrisonEnum(m_CurrentPrison), m_CurrentLBGameType, OnResultsLoaded, m_CurrentLeaderboardIndex);
				}
				else
				{
					LeaderboardPageUp();
				}
			}
			if (currentRewiredPlayer.GetButtonUp("UI_NextPage"))
			{
				if (m_ScrollView.GetCurrentSelected() == 99 && m_CurrentLBType != Platform.LeaderboardType.MyScore)
				{
					m_CurrentLeaderboardIndex += 100;
					if (m_LoadingIcon != null)
					{
						m_LoadingIcon.SetActive(value: true);
					}
					if (m_LoadingBackground != null)
					{
						m_LoadingBackground.SetActive(value: true);
					}
					m_bResetToTop = true;
					Platform.GetInstance().RequestLeaderboard(m_CurrentLBType, LevelScript.MapLeaderboardToPrisonEnum(m_CurrentPrison), m_CurrentLBGameType, OnResultsLoaded, m_CurrentLeaderboardIndex);
				}
				else
				{
					LeaderboardPageDown();
				}
			}
			if (m_CurrentLBType != Platform.LeaderboardType.MyScore)
			{
				if (currentRewiredPlayer.GetAxis("UI_Vertical") > 0.01f)
				{
					if (!m_bInvalidUpPressed && m_ScrollView.GetCurrentSelected() == 0 && m_CurrentLeaderboardIndex > 1)
					{
						m_bUpPressed = true;
					}
					else
					{
						m_bInvalidUpPressed = true;
					}
				}
				if (currentRewiredPlayer.GetAxis("UI_Vertical") > -0.01f && currentRewiredPlayer.GetAxis("UI_Vertical") < 0.01f)
				{
					if (m_bUpPressed)
					{
						m_CurrentLeaderboardIndex -= 100;
						m_CurrentLeaderboardIndex = Mathf.Max(1, m_CurrentLeaderboardIndex);
						if (m_LoadingIcon != null)
						{
							m_LoadingIcon.SetActive(value: true);
						}
						if (m_LoadingBackground != null)
						{
							m_LoadingBackground.SetActive(value: true);
						}
						m_bResetToTop = false;
						Platform.GetInstance().RequestLeaderboard(m_CurrentLBType, LevelScript.MapLeaderboardToPrisonEnum(m_CurrentPrison), m_CurrentLBGameType, OnResultsLoaded, m_CurrentLeaderboardIndex);
					}
					m_bUpPressed = false;
					m_bInvalidUpPressed = false;
				}
				if (currentRewiredPlayer.GetAxis("UI_Vertical") < -0.01f)
				{
					if (!m_bInvalidDownPressed && m_ScrollView.GetCurrentSelected() == 99)
					{
						m_bDownPressed = true;
					}
					else
					{
						m_bInvalidDownPressed = true;
					}
				}
				if (currentRewiredPlayer.GetAxis("UI_Vertical") > -0.01f && currentRewiredPlayer.GetAxis("UI_Vertical") < 0.01f)
				{
					if (m_bDownPressed)
					{
						m_CurrentLeaderboardIndex += 100;
						if (m_LoadingIcon != null)
						{
							m_LoadingIcon.SetActive(value: true);
						}
						if (m_LoadingBackground != null)
						{
							m_LoadingBackground.SetActive(value: true);
						}
						m_bResetToTop = true;
						Platform.GetInstance().RequestLeaderboard(m_CurrentLBType, LevelScript.MapLeaderboardToPrisonEnum(m_CurrentPrison), m_CurrentLBGameType, OnResultsLoaded, m_CurrentLeaderboardIndex);
					}
					m_bInvalidDownPressed = false;
					m_bDownPressed = false;
				}
			}
		}
		if (!m_bCancelRequested && m_RequestTimeout > 0f)
		{
			m_RequestTimeout -= Time.deltaTime;
			if (m_RequestTimeout <= 0f)
			{
				Platform.GetInstance().RequestLeaderboard(m_CurrentLBType, LevelScript.MapLeaderboardToPrisonEnum(m_CurrentPrison), m_CurrentLBGameType, OnResultsLoaded, m_CurrentLeaderboardIndex);
			}
		}
	}

	private bool GetLeaderboardEntryHasFocus()
	{
		T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(base.CurrentGamer);
		GameObject currentSelectedGameObject = eventSystemForGamer.currentSelectedGameObject;
		for (int num = m_ScrollView.m_ContentParent.transform.childCount - 1; num >= 0; num--)
		{
			if (m_ScrollView.m_ContentParent.GetChild(num).gameObject == currentSelectedGameObject)
			{
				return true;
			}
		}
		return false;
	}
}
