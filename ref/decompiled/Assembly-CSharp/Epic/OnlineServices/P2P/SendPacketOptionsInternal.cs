using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.P2P;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SendPacketOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	private IntPtr m_RemoteUserId;

	private IntPtr m_SocketId;

	private byte m_Channel;

	private uint m_DataLengthBytes;

	private IntPtr m_Data;

	private int m_AllowDelayedDelivery;

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

	public ProductUserId RemoteUserId
	{
		get
		{
			return Helper.GetHandle<ProductUserId>(m_RemoteUserId);
		}
		set
		{
			m_RemoteUserId = Helper.GetInnerHandle(value);
		}
	}

	public SocketIdInternal SocketId
	{
		get
		{
			return Helper.GetAllocation<SocketIdInternal>(m_SocketId);
		}
		set
		{
			Helper.RegisterAllocation(ref m_SocketId, value);
		}
	}

	public byte Channel
	{
		get
		{
			return m_Channel;
		}
		set
		{
			m_Channel = value;
		}
	}

	public byte[] Data
	{
		get
		{
			return Helper.GetAllocation<byte[]>(m_Data, (int)m_DataLengthBytes);
		}
		set
		{
			Helper.RegisterArrayAllocation(ref m_Data, value, out m_DataLengthBytes);
		}
	}

	public bool AllowDelayedDelivery
	{
		get
		{
			return Helper.GetBoolFromInt(m_AllowDelayedDelivery);
		}
		set
		{
			m_AllowDelayedDelivery = Helper.GetIntFromBool(value);
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_SocketId);
		Helper.ReleaseAllocation(ref m_Data);
	}
}
