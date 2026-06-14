using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct PlayerAchievementInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_AchievementId;

	private double m_Progress;

	private long m_UnlockTime;

	private int m_StatInfoCount;

	private IntPtr m_StatInfo;

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

	public string AchievementId
	{
		get
		{
			return m_AchievementId;
		}
		set
		{
			m_AchievementId = value;
		}
	}

	public double Progress
	{
		get
		{
			return m_Progress;
		}
		set
		{
			m_Progress = value;
		}
	}

	public long UnlockTime
	{
		get
		{
			return m_UnlockTime;
		}
		set
		{
			m_UnlockTime = value;
		}
	}

	public PlayerStatInfoInternal[] StatInfo
	{
		get
		{
			return Helper.GetAllocation<PlayerStatInfoInternal[]>(m_StatInfo, m_StatInfoCount);
		}
		set
		{
			Helper.RegisterArrayAllocation(ref m_StatInfo, value, out m_StatInfoCount);
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_StatInfo);
	}
}
