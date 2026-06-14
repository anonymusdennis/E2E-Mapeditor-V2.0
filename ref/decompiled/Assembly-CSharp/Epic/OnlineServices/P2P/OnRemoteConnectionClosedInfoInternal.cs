using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.P2P;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct OnRemoteConnectionClosedInfoInternal : ICallbackInfo, IDisposable
{
	private IntPtr m_ClientData;

	private IntPtr m_LocalUserId;

	private IntPtr m_RemoteUserId;

	private IntPtr m_SocketId;

	private ConnectionClosedReason m_Reason;

	public object ClientData => Helper.GetClientData(m_ClientData);

	public IntPtr ClientDataAddress => m_ClientData;

	public ProductUserId LocalUserId => Helper.GetHandle<ProductUserId>(m_LocalUserId);

	public ProductUserId RemoteUserId => Helper.GetHandle<ProductUserId>(m_RemoteUserId);

	public SocketIdInternal SocketId => Helper.GetAllocation<SocketIdInternal>(m_SocketId);

	public ConnectionClosedReason Reason => m_Reason;

	public void Dispose()
	{
	}
}
