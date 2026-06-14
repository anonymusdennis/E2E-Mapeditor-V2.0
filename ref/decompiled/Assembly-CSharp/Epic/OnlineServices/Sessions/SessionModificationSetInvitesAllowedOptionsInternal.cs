using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SessionModificationSetInvitesAllowedOptionsInternal : IDisposable
{
	private int m_ApiVersion;

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
