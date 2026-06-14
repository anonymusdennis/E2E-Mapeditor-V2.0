using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SessionDetailsSettingsInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_BucketId;

	private uint m_NumPublicConnections;

	private int m_AllowJoinInProgress;

	private OnlineSessionPermissionLevel m_PermissionLevel;

	private int m_InvitesAllowed;

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

	public string BucketId
	{
		get
		{
			return m_BucketId;
		}
		set
		{
			m_BucketId = value;
		}
	}

	public uint NumPublicConnections
	{
		get
		{
			return m_NumPublicConnections;
		}
		set
		{
			m_NumPublicConnections = value;
		}
	}

	public bool AllowJoinInProgress
	{
		get
		{
			return Helper.GetBoolFromInt(m_AllowJoinInProgress);
		}
		set
		{
			m_AllowJoinInProgress = Helper.GetIntFromBool(value);
		}
	}

	public OnlineSessionPermissionLevel PermissionLevel
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

	public bool InvitesAllowed
	{
		get
		{
			return Helper.GetBoolFromInt(m_InvitesAllowed);
		}
		set
		{
			m_InvitesAllowed = Helper.GetIntFromBool(value);
		}
	}

	public void Dispose()
	{
	}
}
