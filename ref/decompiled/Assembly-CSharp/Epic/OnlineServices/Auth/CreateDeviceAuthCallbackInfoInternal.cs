using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CreateDeviceAuthCallbackInfoInternal : ICallbackInfo, IDisposable
{
	private Result m_ResultCode;

	private IntPtr m_ClientData;

	private IntPtr m_LocalUserId;

	private IntPtr m_Credentials;

	public Result ResultCode => m_ResultCode;

	public object ClientData => Helper.GetClientData(m_ClientData);

	public IntPtr ClientDataAddress => m_ClientData;

	public EpicAccountId LocalUserId => Helper.GetHandle<EpicAccountId>(m_LocalUserId);

	public CredentialsInternal Credentials => Helper.GetAllocation<CredentialsInternal>(m_Credentials);

	public void Dispose()
	{
	}
}
