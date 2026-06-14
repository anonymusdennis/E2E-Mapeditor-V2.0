using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CreateLobbyOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	private uint m_MaxLobbyMembers;

	private LobbyPermissionLevel m_PermissionLevel;

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

	public uint MaxLobbyMembers
	{
		get
		{
			return m_MaxLobbyMembers;
		}
		set
		{
			m_MaxLobbyMembers = value;
		}
	}

	public LobbyPermissionLevel PermissionLevel
	{
		get
		{
			return m_PermissionLevel;
		}
		set
		{
			m_PermissionLevel = value;
		}
	}

	public void Dispose()
	{
	}
}
