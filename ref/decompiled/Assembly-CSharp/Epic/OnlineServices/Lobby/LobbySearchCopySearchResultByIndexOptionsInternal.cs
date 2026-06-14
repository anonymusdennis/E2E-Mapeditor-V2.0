using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct LobbySearchCopySearchResultByIndexOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private uint m_LobbyIndex;

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

	public uint LobbyIndex
	{
		get
		{
			return m_LobbyIndex;
		}
		set
		{
			m_LobbyIndex = value;
		}
	}

	public void Dispose()
	{
	}
}
