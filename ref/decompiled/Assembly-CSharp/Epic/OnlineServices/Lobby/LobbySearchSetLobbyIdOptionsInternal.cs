using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct LobbySearchSetLobbyIdOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_LobbyId;

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

	public string LobbyId
	{
		get
		{
			return m_LobbyId;
		}
		set
		{
			m_LobbyId = value;
		}
	}

	public void Dispose()
	{
	}
}
