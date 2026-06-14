using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct UnlockedAchievementInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_AchievementId;

	private long m_UnlockTime;

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

	public void Dispose()
	{
	}
}
