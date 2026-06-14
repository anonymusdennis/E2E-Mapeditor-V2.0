using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SessionModificationSetMaxPlayersOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private uint m_MaxPlayers;

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

	public uint MaxPlayers
	{
		get
		{
			return m_MaxPlayers;
		}
		set
		{
			m_MaxPlayers = value;
		}
	}

	public void Dispose()
	{
	}
}
