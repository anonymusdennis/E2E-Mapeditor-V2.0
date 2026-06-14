using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CopyLeaderboardDefinitionByIndexOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private uint m_LeaderboardIndex;

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

	public uint LeaderboardIndex
	{
		get
		{
			return m_LeaderboardIndex;
		}
		set
		{
			m_LeaderboardIndex = value;
		}
	}

	public void Dispose()
	{
	}
}
