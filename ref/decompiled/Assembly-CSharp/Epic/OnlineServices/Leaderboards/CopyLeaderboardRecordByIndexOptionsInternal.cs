using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CopyLeaderboardRecordByIndexOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private uint m_LeaderboardRecordIndex;

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

	public uint LeaderboardRecordIndex
	{
		get
		{
			return m_LeaderboardRecordIndex;
		}
		set
		{
			m_LeaderboardRecordIndex = value;
		}
	}

	public void Dispose()
	{
	}
}
