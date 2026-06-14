using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.PlayerDataStorage;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct WriteFileDataCallbackInfoInternal : ICallbackInfo, IDisposable
{
	private IntPtr m_ClientData;

	private IntPtr m_LocalUserId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_Filename;

	private uint m_DataBufferLengthBytes;

	public object ClientData => Helper.GetClientData(m_ClientData);

	public IntPtr ClientDataAddress => m_ClientData;

	public ProductUserId LocalUserId => Helper.GetHandle<ProductUserId>(m_LocalUserId);

	public string Filename => m_Filename;

	public uint DataBufferLengthBytes => m_DataBufferLengthBytes;

	public void Dispose()
	{
	}
}
