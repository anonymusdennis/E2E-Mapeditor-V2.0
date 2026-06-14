using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SessionInviteReceivedCallbackInfoInternal : ICallbackInfo, IDisposable
{
	private IntPtr m_ClientData;

	private IntPtr m_LocalUserId;

	private IntPtr m_TargetUserId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_InviteId;

	public object ClientData => Helper.GetClientData(m_ClientData);

	public IntPtr ClientDataAddress => m_ClientData;

	public ProductUserId LocalUserId => Helper.GetHandle<ProductUserId>(m_LocalUserId);

	public ProductUserId TargetUserId => Helper.GetHandle<ProductUserId>(m_TargetUserId);

	public string InviteId => m_InviteId;

	public void Dispose()
	{
	}
}
