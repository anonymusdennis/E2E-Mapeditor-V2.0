using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct UpdateLobbyOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LobbyModificationHandle;

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

	public LobbyModification LobbyModificationHandle
	{
		get
		{
			return Helper.GetHandle<LobbyModification>(m_LobbyModificationHandle);
		}
		set
		{
			m_LobbyModificationHandle = Helper.GetInnerHandle(value);
		}
	}

	public void Dispose()
	{
	}
}
