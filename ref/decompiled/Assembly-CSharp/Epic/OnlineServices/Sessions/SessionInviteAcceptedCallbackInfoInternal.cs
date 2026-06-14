using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SessionInviteAcceptedCallbackInfoInternal : ICallbackInfo, IDisposable
{
	private IntPtr m_ClientData;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_SessionId;

	private IntPtr m_LocalUserId;

	private IntPtr m_TargetUserId;

	public object ClientData => Helper.GetClientData(m_ClientData);

	public IntPtr ClientDataAddress => m_ClientData;

	public string SessionId => m_SessionId;

	public ProductUserId LocalUserId => Helper.GetHandle<ProductUserId>(m_LocalUserId);

	public ProductUserId TargetUserId => Helper.GetHandle<ProductUserId>(m_TargetUserId);

	public void Dispose()
	{
	}
}
