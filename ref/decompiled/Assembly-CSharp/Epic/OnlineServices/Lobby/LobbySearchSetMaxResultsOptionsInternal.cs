using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct LobbySearchSetMaxResultsOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private uint m_MaxResults;

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

	public uint MaxResults
	{
		get
		{
			return m_MaxResults;
		}
		set
		{
			m_MaxResults = value;
		}
	}

	public void Dispose()
	{
	}
}
