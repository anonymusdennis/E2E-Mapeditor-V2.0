using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.P2P;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SocketIdInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 33)]
	private byte[] m_SocketName;

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

	public string SocketName
	{
		get
		{
			return Helper.StringFromByteArray(m_SocketName);
		}
		set
		{
			m_SocketName = Helper.StringToByteArray(value, 33);
		}
	}

	public void Dispose()
	{
	}
}
