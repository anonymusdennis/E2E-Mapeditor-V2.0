using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SessionModificationSetPermissionLevelOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private OnlineSessionPermissionLevel m_PermissionLevel;

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

	public void Dispose()
	{
	}
}
