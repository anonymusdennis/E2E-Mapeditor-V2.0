using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Epic.OnlineServices.PlayerDataStorage;

public sealed class PlayerDataStorageFileTransferRequest : Handle
{
	public PlayerDataStorageFileTransferRequest()
		: base(IntPtr.Zero)
	{
	}

	public PlayerDataStorageFileTransferRequest(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public Result GetFileRequestState()
	{
		return EOS_PlayerDataStorageFileTransferRequest_GetFileRequestState(base.InnerHandle);
	}

	public Result GetFilename(uint filenameStringBufferSizeBytes, StringBuilder outStringBuffer, out int outStringLength)
	{
		outStringLength = Helper.GetDefault<int>();
		return EOS_PlayerDataStorageFileTransferRequest_GetFilename(base.InnerHandle, filenameStringBufferSizeBytes, outStringBuffer, ref outStringLength);
	}

	public Result CancelRequest()
	{
		return EOS_PlayerDataStorageFileTransferRequest_CancelRequest(base.InnerHandle);
	}

	public void Release()
	{
		EOS_PlayerDataStorageFileTransferRequest_Release(base.InnerHandle);
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_PlayerDataStorageFileTransferRequest_Release(IntPtr playerDataStorageFileTransferHandle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_PlayerDataStorageFileTransferRequest_CancelRequest(IntPtr handle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_PlayerDataStorageFileTransferRequest_GetFilename(IntPtr handle, uint filenameStringBufferSizeBytes, StringBuilder outStringBuffer, ref int outStringLength);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_PlayerDataStorageFileTransferRequest_GetFileRequestState(IntPtr handle);
}
