using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SessionSearchCopySearchResultByIndexOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private uint m_SessionIndex;

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

	public uint SessionIndex
	{
		get
		{
			return m_SessionIndex;
		}
		set
		{
			m_SessionIndex = value;
		}
	}

	public void Dispose()
	{
	}
}
