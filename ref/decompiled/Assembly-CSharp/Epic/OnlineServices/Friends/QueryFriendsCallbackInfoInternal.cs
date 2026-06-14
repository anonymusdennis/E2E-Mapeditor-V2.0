using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Friends;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct QueryFriendsCallbackInfoInternal : ICallbackInfo, IDisposable
{
	private Result m_ResultCode;

	private IntPtr m_ClientData;

	private IntPtr m_LocalUserId;

	public Result ResultCode => m_ResultCode;

	public object ClientData => Helper.GetClientData(m_ClientData);

	public IntPtr ClientDataAddress => m_ClientData;

	public EpicAccountId LocalUserId => Helper.GetHandle<EpicAccountId>(m_LocalUserId);

	public void Dispose()
	{
	}
}
