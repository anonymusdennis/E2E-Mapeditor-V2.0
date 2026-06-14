using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct PresenceModificationSetJoinInfoOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_JoinInfo;

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

	public string JoinInfo
	{
		get
		{
			return m_JoinInfo;
		}
		set
		{
			m_JoinInfo = value;
		}
	}

	public void Dispose()
	{
	}
}
