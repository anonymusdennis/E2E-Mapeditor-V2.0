using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CopyUnlockedAchievementByAchievementIdOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_UserId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_AchievementId;

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

	public void Dispose()
	{
	}
}
