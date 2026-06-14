using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct UnlockAchievementsOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_UserId;

	private IntPtr m_AchievementIds;

	private uint m_AchievementsCount;

	public int ApiVersion
	{
		get
		{
			return m_ApiVersion;
		}
		set
		{
			m_ApiVersion = value;
		}
	}

	public ProductUserId UserId
	{
		get
		{
			return Helper.GetHandle<ProductUserId>(m_UserId);
		}
		set
		{
			m_UserId = Helper.GetInnerHandle(value);
		}
	}

	public string[] AchievementIds
	{
		get
		{
			return Helper.GetAllocation<string[]>(m_AchievementIds, (int)m_AchievementsCount);
		}
		set
		{
			Helper.RegisterArrayAllocation(ref m_AchievementIds, value, out m_AchievementsCount);
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_AchievementIds);
	}
}
