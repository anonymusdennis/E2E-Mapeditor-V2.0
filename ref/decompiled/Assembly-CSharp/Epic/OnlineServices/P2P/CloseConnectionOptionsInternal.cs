using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.P2P;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CloseConnectionOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	private IntPtr m_RemoteUserId;

	private IntPtr m_SocketId;

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

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_SocketId);
	}
}
