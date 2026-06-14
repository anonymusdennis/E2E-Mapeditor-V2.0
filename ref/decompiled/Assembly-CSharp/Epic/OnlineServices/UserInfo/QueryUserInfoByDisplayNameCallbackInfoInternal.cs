using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.UserInfo;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct QueryUserInfoByDisplayNameCallbackInfoInternal : ICallbackInfo, IDisposable
{
	private Result m_ResultCode;

	private IntPtr m_ClientData;

	private IntPtr m_LocalUserId;

	private IntPtr m_TargetUserId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_DisplayName;

	public Result ResultCode => m_ResultCode;

	public object ClientData => Helper.GetClientData(m_ClientData);

	public IntPtr ClientDataAddress => m_ClientData;

	public EpicAccountId LocalUserId => Helper.GetHandle<EpicAccountId>(m_LocalUserId);

	public EpicAccountId TargetUserId => Helper.GetHandle<EpicAccountId>(m_TargetUserId);

	public string DisplayName => m_DisplayName;

	public void Dispose()
	{
	}
}
