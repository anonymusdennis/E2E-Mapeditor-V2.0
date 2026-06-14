using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct LobbyDetailsCopyAttributeByIndexOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private uint m_AttrIndex;

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

	public uint AttrIndex
	{
		get
		{
			return m_AttrIndex;
		}
		set
		{
			m_AttrIndex = value;
		}
	}

	public void Dispose()
	{
	}
}
