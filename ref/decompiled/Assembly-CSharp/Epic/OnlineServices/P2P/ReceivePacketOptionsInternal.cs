using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.P2P;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct ReceivePacketOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	private uint m_MaxDataSizeBytes;

	private byte m_RequestedChannel;

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

	public uint MaxDataSizeBytes
	{
		get
		{
			return m_MaxDataSizeBytes;
		}
		set
		{
			m_MaxDataSizeBytes = value;
		}
	}

	public byte RequestedChannel
	{
		get
		{
			return m_RequestedChannel;
		}
		set
		{
			m_RequestedChannel = value;
		}
	}

	public void Dispose()
	{
	}
}
