using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SessionSearchSetMaxResultsOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private uint m_MaxSearchResults;

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

	public uint MaxSearchResults
	{
		get
		{
			return m_MaxSearchResults;
		}
		set
		{
			m_MaxSearchResults = value;
		}
	}

	public void Dispose()
	{
	}
}
