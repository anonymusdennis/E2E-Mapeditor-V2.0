using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.PlayerDataStorage;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct ReadFileCallbackInfoInternal : ICallbackInfo, IDisposable
{
	private Result m_ResultCode;

	private IntPtr m_ClientData;

	private IntPtr m_LocalUserId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_Filename;

	public Result ResultCode => m_ResultCode;

	public object ClientData => Helper.GetClientData(m_ClientData);

	public IntPtr ClientDataAddress => m_ClientData;

	public ProductUserId LocalUserId => Helper.GetHandle<ProductUserId>(m_LocalUserId);

	public string Filename => m_Filename;

	public void Dispose()
	{
	}
}
