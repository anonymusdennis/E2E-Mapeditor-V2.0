using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct UserScoresQueryStatInfoInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_StatName;

	private LeaderboardAggregation m_Aggregation;

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

	public string StatName
	{
		get
		{
			return m_StatName;
		}
		set
		{
			m_StatName = value;
		}
	}

	public LeaderboardAggregation Aggregation
	{
		get
		{
			return m_Aggregation;
		}
		set
		{
			m_Aggregation = value;
		}
	}

	public void Dispose()
	{
	}
}
