using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.PlayerDataStorage;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CopyFileMetadataByFilenameOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_Filename;

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

	public string Filename
	{
		get
		{
			return m_Filename;
		}
		set
		{
			m_Filename = value;
		}
	}

	public void Dispose()
	{
	}
}
