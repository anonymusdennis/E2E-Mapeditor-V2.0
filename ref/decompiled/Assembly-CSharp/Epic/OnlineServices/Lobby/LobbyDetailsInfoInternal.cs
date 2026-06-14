using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct LobbyDetailsInfoInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_LobbyId;

	private IntPtr m_LobbyOwnerUserId;

	private LobbyPermissionLevel m_PermissionLevel;

	private uint m_AvailableSlots;

	private uint m_MaxMembers;

	private int m_AllowInvites;

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

	public ProductUserId LobbyOwnerUserId
	{
		get
		{
			return Helper.GetHandle<ProductUserId>(m_LobbyOwnerUserId);
		}
		set
		{
			m_LobbyOwnerUserId = Helper.GetInnerHandle(value);
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

	public uint AvailableSlots
	{
		get
		{
			return m_AvailableSlots;
		}
		set
		{
			m_AvailableSlots = value;
		}
	}

	public uint MaxMembers
	{
		get
		{
			return m_MaxMembers;
		}
		set
		{
			m_MaxMembers = value;
		}
	}

	public bool AllowInvites
	{
		get
		{
			return Helper.GetBoolFromInt(m_AllowInvites);
		}
		set
		{
			m_AllowInvites = Helper.GetIntFromBool(value);
		}
	}

	public void Dispose()
	{
	}
}
