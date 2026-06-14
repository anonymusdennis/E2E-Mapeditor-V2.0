using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct JoinGameAcceptedCallbackInfoInternal : ICallbackInfo, IDisposable
{
	private IntPtr m_ClientData;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_JoinInfo;

	private IntPtr m_LocalUserId;

	private IntPtr m_TargetUserId;

	public object ClientData => Helper.GetClientData(m_ClientData);

	public IntPtr ClientDataAddress => m_ClientData;

	public string JoinInfo => m_JoinInfo;

	public EpicAccountId LocalUserId => Helper.GetHandle<EpicAccountId>(m_LocalUserId);

	public EpicAccountId TargetUserId => Helper.GetHandle<EpicAccountId>(m_TargetUserId);

	public void Dispose()
	{
	}
}
