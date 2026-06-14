using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct QueryLeaderboardRanksOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_LeaderboardId;

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

	public string LeaderboardId
	{
		get
		{
			return m_LeaderboardId;
		}
		set
		{
			m_LeaderboardId = value;
		}
	}

	public void Dispose()
	{
	}
}
