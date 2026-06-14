using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SessionDetailsCopySessionAttributeByKeyOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_AttrKey;

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

	public string AttrKey
	{
		get
		{
			return m_AttrKey;
		}
		set
		{
			m_AttrKey = value;
		}
	}

	public void Dispose()
	{
	}
}
