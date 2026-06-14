using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct DeviceInfoInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_Type;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_Model;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_OS;

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

	public string Type
	{
		get
		{
			return m_Type;
		}
		set
		{
			m_Type = value;
		}
	}

	public string Model
	{
		get
		{
			return m_Model;
		}
		set
		{
			m_Model = value;
		}
	}

	public string OS
	{
		get
		{
			return m_OS;
		}
		set
		{
			m_OS = value;
		}
	}

	public void Dispose()
	{
	}
}
