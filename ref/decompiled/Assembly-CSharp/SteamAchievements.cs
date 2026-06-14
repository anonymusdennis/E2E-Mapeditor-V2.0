using Steamworks;
using UnityEngine;

public class SteamAchievements : MonoBehaviour
{
	protected Callback<UserStatsReceived_t> m_UserStatsResult;

	protected Callback<UserAchievementStored_t> m_AchievementResult;

	private bool m_bStatsInitialised;

	private void Start()
	{
		m_bStatsInitialised = false;
		m_UserStatsResult = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
		m_AchievementResult = Callback<UserAchievementStored_t>.Create(OnAchievementStored);
	}

	private void Update()
	{
	}

	public void SetAchievementProgress(string APIName, int progress)
	{
		if (!string.IsNullOrEmpty(APIName))
		{
			bool flag = SteamUserStats.SetStat(APIName, progress);
			flag = SteamUserStats.StoreStats();
		}
	}

	public void UnlockAchievement(string APIName)
	{
		if (!string.IsNullOrEmpty(APIName))
		{
			bool flag = SteamUserStats.SetAchievement(APIName);
			flag = SteamUserStats.StoreStats();
		}
	}

	private void OnUserStatsReceived(UserStatsReceived_t pCallback)
	{
		Debug.Log("STEAM ACHIEVEMENTS: OnUserStatsReceived result=" + pCallback.m_eResult);
		if (!m_bStatsInitialised)
		{
			m_bStatsInitialised = true;
		}
	}

	private void OnAchievementStored(UserAchievementStored_t pCallback)
	{
		Debug.Log("STEAM ACHIEVEMENTS: OnAchievementStored ");
	}
}
