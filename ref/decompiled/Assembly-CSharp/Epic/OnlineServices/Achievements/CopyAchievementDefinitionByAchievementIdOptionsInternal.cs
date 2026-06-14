using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CopyAchievementDefinitionByAchievementIdOptionsInternal : IDisposable
{
	private int m_ApiVersion;

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
