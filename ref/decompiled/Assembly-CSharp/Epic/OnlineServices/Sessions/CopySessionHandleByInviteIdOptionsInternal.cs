using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CopySessionHandleByInviteIdOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_InviteId;

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

	public string InviteId
	{
		get
		{
			return m_InviteId;
		}
		set
		{
			m_InviteId = value;
		}
	}

	public void Dispose()
	{
	}
}
