using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct QueryLeaderboardDefinitionsOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private long m_StartTime;

	private long m_EndTime;

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

	public long StartTime
	{
		get
		{
			return m_StartTime;
		}
		set
		{
			m_StartTime = value;
		}
	}

	public long EndTime
	{
		get
		{
			return m_EndTime;
		}
		set
		{
			m_EndTime = value;
		}
	}

	public void Dispose()
	{
	}
}
