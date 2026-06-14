using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct JoinLobbyOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LobbyDetailsHandle;

	private IntPtr m_LocalUserId;

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

	public LobbyDetails LobbyDetailsHandle
	{
		get
		{
			return Helper.GetHandle<LobbyDetails>(m_LobbyDetailsHandle);
		}
		set
		{
			m_LobbyDetailsHandle = Helper.GetInnerHandle(value);
		}
	}

	public ProductUserId LocalUserId
	{
		get
		{
			return Helper.GetHandle<ProductUserId>(m_LocalUserId);
		}
		set
		{
			m_LocalUserId = Helper.GetInnerHandle(value);
		}
	}

	public void Dispose()
	{
	}
}
