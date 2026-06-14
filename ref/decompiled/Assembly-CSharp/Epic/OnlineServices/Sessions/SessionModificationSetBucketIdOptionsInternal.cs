using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SessionModificationSetBucketIdOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_BucketId;

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

	public string BucketId
	{
		get
		{
			return m_BucketId;
		}
		set
		{
			m_BucketId = value;
		}
	}

	public void Dispose()
	{
	}
}
