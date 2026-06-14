using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.PlayerDataStorage;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct FileTransferProgressCallbackInfoInternal : ICallbackInfo, IDisposable
{
	private IntPtr m_ClientData;

	private IntPtr m_LocalUserId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_Filename;

	private uint m_BytesTransferred;

	private uint m_TotalFileSizeBytes;

	public object ClientData => Helper.GetClientData(m_ClientData);

	public IntPtr ClientDataAddress => m_ClientData;

	public ProductUserId LocalUserId => Helper.GetHandle<ProductUserId>(m_LocalUserId);

	public string Filename => m_Filename;

	public uint BytesTransferred => m_BytesTransferred;

	public uint TotalFileSizeBytes => m_TotalFileSizeBytes;

	public void Dispose()
	{
	}
}
