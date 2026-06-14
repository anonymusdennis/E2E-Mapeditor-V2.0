using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class SteamLeaderboards : MonoBehaviour
{
	private static CallResult<LeaderboardFindResult_t> m_FindResult = new CallResult<LeaderboardFindResult_t>();

	private static CallResult<LeaderboardScoreUploaded_t> m_uploadResult = new CallResult<LeaderboardScoreUploaded_t>();

	private static CallResult<LeaderboardScoresDownloaded_t> m_DownloadResult = new CallResult<LeaderboardScoresDownloaded_t>();

	private Dictionary<string, SteamLeaderboard_t> m_LeaderboardIds = new Dictionary<string, SteamLeaderboard_t>();

	private bool m_CancelRequest;

	private string m_LastRequestedLeaderboardName = string.Empty;

	public static event Platform.LeaderboardPostComplete OnLeaderboardPostComplete;

	public static event Platform.LeaderboardReadComplete OnLeaderboardReadComplete;

	public static event Platform.CancelRequestLeaderboardCallback OnRequestLeaderboardCancelled;

	private void Start()
	{
	}

	private void Awake()
	{
	}

	private void Update()
	{
		if (!m_DownloadResult.IsActive() && SteamLeaderboards.OnRequestLeaderboardCancelled != null)
		{
			SteamLeaderboards.OnRequestLeaderboardCancelled();
			SteamLeaderboards.OnRequestLeaderboardCancelled = null;
		}
	}

	public void PostToLeaderboard(LevelScript.PRISON_ENUM ePrison, Platform.LeaderboardGameType lbGameType, Platform.LeaderboardPostComplete callback, int score, int extraData)
	{
		if (callback != null)
		{
			SteamLeaderboards.OnLeaderboardPostComplete = callback;
		}
		string leaderboardName = $"{Enum.GetName(typeof(LevelScript.PRISON_ENUM), ePrison)}_{Enum.GetName(typeof(Platform.LeaderboardGameType), lbGameType)}";
		StartCoroutine(PostToLeaderboard(leaderboardName, score, extraData));
	}

	private IEnumerator PostToLeaderboard(string leaderboardName, int score, int extraData)
	{
		SteamLeaderboard_t theLeaderboardId = new SteamLeaderboard_t(0uL);
		bool WaitingToGetLeaderboard = false;
		if (!m_LeaderboardIds.TryGetValue(leaderboardName, out theLeaderboardId))
		{
			WaitingToGetLeaderboard = true;
			SteamAPICall_t hAPICall = SteamUserStats.FindOrCreateLeaderboard(leaderboardName, ELeaderboardSortMethod.k_ELeaderboardSortMethodAscending, ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric);
			m_FindResult.Set(hAPICall, delegate(LeaderboardFindResult_t pCallback, bool failure)
			{
				if (pCallback.m_bLeaderboardFound == 0)
				{
					failure = true;
				}
				if (!failure)
				{
					theLeaderboardId = pCallback.m_hSteamLeaderboard;
					m_LeaderboardIds.Add(leaderboardName, theLeaderboardId);
				}
				WaitingToGetLeaderboard = false;
			});
		}
		while (WaitingToGetLeaderboard)
		{
			yield return null;
		}
		if (theLeaderboardId.m_SteamLeaderboard != 0)
		{
			SteamAPICall_t hAPICall2 = SteamUserStats.UploadLeaderboardScore(theLeaderboardId, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, score, new int[1] { extraData }, 1);
			m_uploadResult.Set(hAPICall2, OnLeaderboardUploadResult);
		}
		else
		{
			CallLeaderboardPostComplete(success: false);
		}
	}

	private void OnLeaderboardUploadResult(LeaderboardScoreUploaded_t pCallback, bool failure)
	{
		if (pCallback.m_bSuccess != 1)
		{
			failure = true;
		}
		CallLeaderboardPostComplete(!failure, (pCallback.m_bScoreChanged != 0) ? true : false);
	}

	private void CallLeaderboardPostComplete(bool success, bool beatenPreviousScore = false)
	{
		if (SteamLeaderboards.OnLeaderboardPostComplete != null)
		{
			SteamLeaderboards.OnLeaderboardPostComplete(beatenPreviousScore, success);
			SteamLeaderboards.OnLeaderboardPostComplete = null;
		}
	}

	public virtual void RequestLeaderboard(Platform.LeaderboardType lbType, LevelScript.PRISON_ENUM ePrison, Platform.LeaderboardGameType lbGameType, Platform.LeaderboardReadComplete callback, int firstRow = 1, int numRows = 100, bool bShowErrors = true)
	{
		if (callback != null)
		{
			SteamLeaderboards.OnLeaderboardReadComplete = callback;
		}
		StartCoroutine(RequestLeaderboard(m_LastRequestedLeaderboardName = $"{Enum.GetName(typeof(LevelScript.PRISON_ENUM), ePrison)}_{Enum.GetName(typeof(Platform.LeaderboardGameType), lbGameType)}", lbType, firstRow, numRows));
	}

	public void CancelRequestLeaderboard(Platform.CancelRequestLeaderboardCallback callback)
	{
		if (m_DownloadResult.IsActive())
		{
			m_CancelRequest = true;
			m_DownloadResult.Cancel();
			SteamLeaderboards.OnRequestLeaderboardCancelled = callback;
		}
		else
		{
			callback?.Invoke();
		}
	}

	private IEnumerator RequestLeaderboard(string leaderboardName, Platform.LeaderboardType lbType, int firstRow, int numRows)
	{
		SteamLeaderboard_t theLeaderboardId = new SteamLeaderboard_t(0uL);
		bool WaitingToGetLeaderboard = false;
		m_CancelRequest = false;
		if (!m_LeaderboardIds.TryGetValue(leaderboardName, out theLeaderboardId))
		{
			WaitingToGetLeaderboard = true;
			SteamAPICall_t hAPICall = SteamUserStats.FindOrCreateLeaderboard(leaderboardName, ELeaderboardSortMethod.k_ELeaderboardSortMethodAscending, ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric);
			m_FindResult.Set(hAPICall, delegate(LeaderboardFindResult_t pCallback, bool failure)
			{
				if (m_CancelRequest)
				{
					if (SteamLeaderboards.OnRequestLeaderboardCancelled != null)
					{
						SteamLeaderboards.OnRequestLeaderboardCancelled();
						SteamLeaderboards.OnRequestLeaderboardCancelled = null;
					}
				}
				else
				{
					if (pCallback.m_bLeaderboardFound == 0)
					{
						failure = true;
					}
					if (!failure)
					{
						theLeaderboardId = pCallback.m_hSteamLeaderboard;
						m_LeaderboardIds.Add(leaderboardName, theLeaderboardId);
					}
					WaitingToGetLeaderboard = false;
				}
			});
		}
		while (WaitingToGetLeaderboard)
		{
			if (m_CancelRequest)
			{
				SteamLeaderboards.OnLeaderboardReadComplete = null;
				if (SteamLeaderboards.OnRequestLeaderboardCancelled != null)
				{
					SteamLeaderboards.OnRequestLeaderboardCancelled();
					SteamLeaderboards.OnRequestLeaderboardCancelled = null;
				}
				yield break;
			}
			yield return null;
		}
		if (m_CancelRequest)
		{
			yield break;
		}
		if (theLeaderboardId.m_SteamLeaderboard != 0)
		{
			int num = firstRow + numRows;
			ELeaderboardDataRequest eLeaderboardDataRequest;
			switch (lbType)
			{
			case Platform.LeaderboardType.Friends:
				eLeaderboardDataRequest = ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends;
				break;
			case Platform.LeaderboardType.MyScore:
				num = ((numRows <= 1) ? 1 : (numRows / 2));
				firstRow = 1 - num;
				eLeaderboardDataRequest = ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser;
				break;
			default:
				eLeaderboardDataRequest = ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal;
				break;
			}
			SteamAPICall_t hAPICall2 = SteamUserStats.DownloadLeaderboardEntries(theLeaderboardId, eLeaderboardDataRequest, firstRow, num);
			m_DownloadResult.Set(hAPICall2, OnLeaderboardDownloadResult);
		}
		else
		{
			CallLeaderboardReadComplete(bSuccess: false, null, bShowError: true);
		}
	}

	private void OnLeaderboardDownloadResult(LeaderboardScoresDownloaded_t pCallback, bool bFailure)
	{
		string leaderboardName = SteamUserStats.GetLeaderboardName(pCallback.m_hSteamLeaderboard);
		if (m_CancelRequest || leaderboardName != m_LastRequestedLeaderboardName)
		{
			SteamLeaderboards.OnLeaderboardReadComplete = null;
			if (SteamLeaderboards.OnRequestLeaderboardCancelled != null)
			{
				SteamLeaderboards.OnRequestLeaderboardCancelled();
				SteamLeaderboards.OnRequestLeaderboardCancelled = null;
			}
		}
		else if (pCallback.m_cEntryCount > 0)
		{
			Platform.DisplayableRank[] array = new Platform.DisplayableRank[pCallback.m_cEntryCount];
			int[] array2 = new int[1];
			int num = 0;
			for (int i = 0; i < pCallback.m_cEntryCount; i++)
			{
				SteamUserStats.GetDownloadedLeaderboardEntry(pCallback.m_hSteamLeaderboardEntries, i, out var pLeaderboardEntry, array2, 1);
				if (pLeaderboardEntry.m_nScore <= 1)
				{
					num++;
					continue;
				}
				array[i] = new Platform.DisplayableRank();
				array[i].m_OnlineID = pLeaderboardEntry.m_steamIDUser.ToString();
				array[i].m_Rank = ((pLeaderboardEntry.m_nGlobalRank < num) ? 1 : (pLeaderboardEntry.m_nGlobalRank - num));
				array[i].m_Score = (ulong)pLeaderboardEntry.m_nScore;
				array[i].m_ExtraData = array2[0];
				if (pLeaderboardEntry.m_steamIDUser == SteamUser.GetSteamID())
				{
					array[i].m_Name = Steamworks.SteamFriends.GetPersonaName();
					array[i].m_bMyScore = true;
				}
				else
				{
					array[i].m_Name = Steamworks.SteamFriends.GetFriendPersonaName(pLeaderboardEntry.m_steamIDUser);
					array[i].m_bMyScore = false;
				}
			}
			CallLeaderboardReadComplete(!bFailure, array, bFailure);
		}
		else if (bFailure)
		{
			CallLeaderboardReadComplete(bSuccess: false, null, bShowError: true);
		}
		else
		{
			CallLeaderboardReadComplete(bSuccess: true);
		}
	}

	private void CallLeaderboardReadComplete(bool bSuccess, Platform.DisplayableRank[] theRanks = null, bool bShowError = false, bool contentBlocked = false)
	{
		if (m_CancelRequest)
		{
			if (SteamLeaderboards.OnRequestLeaderboardCancelled != null)
			{
				SteamLeaderboards.OnRequestLeaderboardCancelled();
				SteamLeaderboards.OnRequestLeaderboardCancelled = null;
			}
		}
		else if (SteamLeaderboards.OnLeaderboardReadComplete != null)
		{
			SteamLeaderboards.OnLeaderboardReadComplete(bSuccess, theRanks, (theRanks != null) ? theRanks.Length : 0, bShowError, contentBlocked);
			SteamLeaderboards.OnLeaderboardPostComplete = null;
		}
	}
}
