using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct LoginStatusChangedCallbackInfoInternal : ICallbackInfo, IDisposable
{
	private IntPtr m_ClientData;

	private IntPtr m_LocalUserId;

	private LoginStatus m_PreviousStatus;

	private LoginStatus m_CurrentStatus;

	public object ClientData => Helper.GetClientData(m_ClientData);

	public IntPtr ClientDataAddress => m_ClientData;

	public ProductUserId LocalUserId => Helper.GetHandle<ProductUserId>(m_LocalUserId);

	public LoginStatus PreviousStatus => m_PreviousStatus;

	public LoginStatus CurrentStatus => m_CurrentStatus;

	public void Dispose()
	{
	}
}
