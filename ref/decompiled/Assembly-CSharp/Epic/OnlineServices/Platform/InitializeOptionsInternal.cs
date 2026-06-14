using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Platform;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct InitializeOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private AllocateMemoryFunc m_AllocateMemoryFunction;

	private ReallocateMemoryFunc m_ReallocateMemoryFunction;

	private ReleaseMemoryFunc m_ReleaseMemoryFunction;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_ProductName;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_ProductVersion;

	private IntPtr m_Reserved;

	private IntPtr m_SystemInitializeOptions;

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

	public AllocateMemoryFunc AllocateMemoryFunction
	{
		get
		{
			return m_AllocateMemoryFunction;
		}
		set
		{
			m_AllocateMemoryFunction = value;
		}
	}

	public ReallocateMemoryFunc ReallocateMemoryFunction
	{
		get
		{
			return m_ReallocateMemoryFunction;
		}
		set
		{
			m_ReallocateMemoryFunction = value;
		}
	}

	public ReleaseMemoryFunc ReleaseMemoryFunction
	{
		get
		{
			return m_ReleaseMemoryFunction;
		}
		set
		{
			m_ReleaseMemoryFunction = value;
		}
	}

	public string ProductName
	{
		get
		{
			return m_ProductName;
		}
		set
		{
			m_ProductName = value;
		}
	}

	public string ProductVersion
	{
		get
		{
			return m_ProductVersion;
		}
		set
		{
			m_ProductVersion = value;
		}
	}

	public IntPtr Reserved
	{
		get
		{
			return m_Reserved;
		}
		set
		{
			m_Reserved = value;
		}
	}

	public IntPtr SystemInitializeOptions
	{
		get
		{
			return m_SystemInitializeOptions;
		}
		set
		{
			m_SystemInitializeOptions = value;
		}
	}

	public void Dispose()
	{
	}
}
