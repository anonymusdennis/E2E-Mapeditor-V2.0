using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CopyLeaderboardUserScoreByIndexOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private uint m_LeaderboardUserScoreIndex;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_StatName;

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

	public uint LeaderboardUserScoreIndex
	{
		get
		{
			return m_LeaderboardUserScoreIndex;
		}
		set
		{
			m_LeaderboardUserScoreIndex = value;
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

	public void Dispose()
	{
	}
}
