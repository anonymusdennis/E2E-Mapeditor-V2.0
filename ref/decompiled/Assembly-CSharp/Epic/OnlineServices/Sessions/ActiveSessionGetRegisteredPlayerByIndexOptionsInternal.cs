using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct ActiveSessionGetRegisteredPlayerByIndexOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private uint m_PlayerIndex;

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

	public uint PlayerIndex
	{
		get
		{
			return m_PlayerIndex;
		}
		set
		{
			m_PlayerIndex = value;
		}
	}

	public void Dispose()
	{
	}
}
