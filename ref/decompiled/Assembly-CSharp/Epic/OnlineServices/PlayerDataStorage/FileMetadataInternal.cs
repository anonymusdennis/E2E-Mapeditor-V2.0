using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.PlayerDataStorage;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct FileMetadataInternal : IDisposable
{
	private int m_ApiVersion;

	private uint m_FileSizeBytes;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_MD5Hash;

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

	public uint FileSizeBytes
	{
		get
		{
			return m_FileSizeBytes;
		}
		set
		{
			m_FileSizeBytes = value;
		}
	}

	public string MD5Hash
	{
		get
		{
			return m_MD5Hash;
		}
		set
		{
			m_MD5Hash = value;
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
