using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct QueryDefinitionsOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_UserId;

	private IntPtr m_EpicUserId;

	private IntPtr m_HiddenAchievementIds;

	private uint m_HiddenAchievementsCount;

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

	public EpicAccountId EpicUserId
	{
		get
		{
			return Helper.GetHandle<EpicAccountId>(m_EpicUserId);
		}
		set
		{
			m_EpicUserId = Helper.GetInnerHandle(value);
		}
	}

	public string[] HiddenAchievementIds
	{
		get
		{
			return Helper.GetAllocation<string[]>(m_HiddenAchievementIds, (int)m_HiddenAchievementsCount);
		}
		set
		{
			Helper.RegisterArrayAllocation(ref m_HiddenAchievementIds, value, out m_HiddenAchievementsCount);
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_HiddenAchievementIds);
	}
}
