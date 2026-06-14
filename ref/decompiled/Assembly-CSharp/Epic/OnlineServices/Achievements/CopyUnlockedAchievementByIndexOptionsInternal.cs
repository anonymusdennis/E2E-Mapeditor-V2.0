using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CopyUnlockedAchievementByIndexOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_UserId;

	private uint m_AchievementIndex;

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

	public uint AchievementIndex
	{
		get
		{
			return m_AchievementIndex;
		}
		set
		{
			m_AchievementIndex = value;
		}
	}

	public void Dispose()
	{
	}
}
