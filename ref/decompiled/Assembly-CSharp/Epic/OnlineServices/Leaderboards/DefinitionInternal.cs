using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct DefinitionInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_LeaderboardId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_StatName;

	private LeaderboardAggregation m_Aggregation;

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
