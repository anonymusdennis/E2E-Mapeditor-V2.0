using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct LoginStatusChangedCallbackInfoInternal : ICallbackInfo, IDisposable
{
	private IntPtr m_ClientData;

	private IntPtr m_LocalUserId;

	private LoginStatus m_PrevStatus;

	private LoginStatus m_CurrentStatus;

	public object ClientData => Helper.GetClientData(m_ClientData);

	public IntPtr ClientDataAddress => m_ClientData;

	public EpicAccountId LocalUserId => Helper.GetHandle<EpicAccountId>(m_LocalUserId);

	public LoginStatus PrevStatus => m_PrevStatus;

	public LoginStatus CurrentStatus => m_CurrentStatus;

	public void Dispose()
	{
	}
}
