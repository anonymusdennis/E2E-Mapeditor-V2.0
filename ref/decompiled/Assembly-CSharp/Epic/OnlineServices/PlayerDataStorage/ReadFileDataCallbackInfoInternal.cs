using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.PlayerDataStorage;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct ReadFileDataCallbackInfoInternal : ICallbackInfo, IDisposable
{
	private IntPtr m_ClientData;

	private IntPtr m_LocalUserId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_Filename;

	private uint m_TotalFileSizeBytes;

	private int m_IsLastChunk;

	private uint m_DataChunkLengthBytes;

	private IntPtr m_DataChunk;

	public object ClientData => Helper.GetClientData(m_ClientData);

	public IntPtr ClientDataAddress => m_ClientData;

	public ProductUserId LocalUserId => Helper.GetHandle<ProductUserId>(m_LocalUserId);

	public string Filename => m_Filename;

	public uint TotalFileSizeBytes => m_TotalFileSizeBytes;

	public bool IsLastChunk => Helper.GetBoolFromInt(m_IsLastChunk);

	public byte[] DataChunk => Helper.GetAllocation<byte[]>(m_DataChunk, (int)m_DataChunkLengthBytes);

	public void Dispose()
	{
	}
}
