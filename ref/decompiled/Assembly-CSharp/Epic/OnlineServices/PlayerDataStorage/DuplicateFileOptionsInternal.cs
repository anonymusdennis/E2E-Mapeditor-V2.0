using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.PlayerDataStorage;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct DuplicateFileOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_SourceFilename;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_DestinationFilename;

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

	public ProductUserId LocalUserId
	{
		get
		{
			return Helper.GetHandle<ProductUserId>(m_LocalUserId);
		}
		set
		{
			m_LocalUserId = Helper.GetInnerHandle(value);
		}
	}

	public string SourceFilename
	{
		get
		{
			return m_SourceFilename;
		}
		set
		{
			m_SourceFilename = value;
		}
	}

	public string DestinationFilename
	{
		get
		{
			return m_DestinationFilename;
		}
		set
		{
			m_DestinationFilename = value;
		}
	}

	public void Dispose()
	{
	}
}
