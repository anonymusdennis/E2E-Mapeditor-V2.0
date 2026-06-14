using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.PlayerDataStorage;

public sealed class PlayerDataStorageInterface : Handle
{
	public PlayerDataStorageInterface()
		: base(IntPtr.Zero)
	{
	}

	public PlayerDataStorageInterface(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public void QueryFile(QueryFileOptions queryFileOptions, object clientData, OnQueryFileCompleteCallback completionCallback)
	{
		QueryFileOptionsInternal queryFileOptions2 = Helper.CopyPropertiesToNew<QueryFileOptionsInternal>(queryFileOptions);
		OnQueryFileCompleteCallbackInternal onQueryFileCompleteCallbackInternal = OnQueryFileComplete;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionCallback, onQueryFileCompleteCallbackInternal);
		EOS_PlayerDataStorage_QueryFile(base.InnerHandle, ref queryFileOptions2, clientDataAddress, onQueryFileCompleteCallbackInternal);
		queryFileOptions2.Dispose();
	}

	public void QueryFileList(QueryFileListOptions queryFileListOptions, object clientData, OnQueryFileListCompleteCallback completionCallback)
	{
		QueryFileListOptionsInternal queryFileListOptions2 = Helper.CopyPropertiesToNew<QueryFileListOptionsInternal>(queryFileListOptions);
		OnQueryFileListCompleteCallbackInternal onQueryFileListCompleteCallbackInternal = OnQueryFileListComplete;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionCallback, onQueryFileListCompleteCallbackInternal);
		EOS_PlayerDataStorage_QueryFileList(base.InnerHandle, ref queryFileListOptions2, clientDataAddress, onQueryFileListCompleteCallbackInternal);
		queryFileListOptions2.Dispose();
	}

	public Result CopyFileMetadataByFilename(CopyFileMetadataByFilenameOptions copyFileMetadataOptions, out FileMetadata outMetadata)
	{
		CopyFileMetadataByFilenameOptionsInternal copyFileMetadataOptions2 = Helper.CopyPropertiesToNew<CopyFileMetadataByFilenameOptionsInternal>(copyFileMetadataOptions);
		outMetadata = Helper.GetDefault<FileMetadata>();
		IntPtr outMetadata2 = IntPtr.Zero;
		Result result = EOS_PlayerDataStorage_CopyFileMetadataByFilename(base.InnerHandle, ref copyFileMetadataOptions2, ref outMetadata2);
		copyFileMetadataOptions2.Dispose();
		if (Helper.TryMarshal<FileMetadataInternal, FileMetadata>(outMetadata2, out outMetadata))
		{
			EOS_PlayerDataStorage_FileMetadata_Release(outMetadata2);
		}
		return result;
	}

	public Result GetFileMetadataCount(GetFileMetadataCountOptions getFileMetadataCountOptions, out int outFileMetadataCount)
	{
		GetFileMetadataCountOptionsInternal getFileMetadataCountOptions2 = Helper.CopyPropertiesToNew<GetFileMetadataCountOptionsInternal>(getFileMetadataCountOptions);
		outFileMetadataCount = Helper.GetDefault<int>();
		Result result = EOS_PlayerDataStorage_GetFileMetadataCount(base.InnerHandle, ref getFileMetadataCountOptions2, ref outFileMetadataCount);
		getFileMetadataCountOptions2.Dispose();
		return result;
	}

	public Result CopyFileMetadataAtIndex(CopyFileMetadataAtIndexOptions copyFileMetadataOptions, out FileMetadata outMetadata)
	{
		CopyFileMetadataAtIndexOptionsInternal copyFileMetadataOptions2 = Helper.CopyPropertiesToNew<CopyFileMetadataAtIndexOptionsInternal>(copyFileMetadataOptions);
		outMetadata = Helper.GetDefault<FileMetadata>();
		IntPtr outMetadata2 = IntPtr.Zero;
		Result result = EOS_PlayerDataStorage_CopyFileMetadataAtIndex(base.InnerHandle, ref copyFileMetadataOptions2, ref outMetadata2);
		copyFileMetadataOptions2.Dispose();
		if (Helper.TryMarshal<FileMetadataInternal, FileMetadata>(outMetadata2, out outMetadata))
		{
			EOS_PlayerDataStorage_FileMetadata_Release(outMetadata2);
		}
		return result;
	}

	public void DuplicateFile(DuplicateFileOptions duplicateOptions, object clientData, OnDuplicateFileCompleteCallback completionCallback)
	{
		DuplicateFileOptionsInternal duplicateOptions2 = Helper.CopyPropertiesToNew<DuplicateFileOptionsInternal>(duplicateOptions);
		OnDuplicateFileCompleteCallbackInternal onDuplicateFileCompleteCallbackInternal = OnDuplicateFileComplete;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionCallback, onDuplicateFileCompleteCallbackInternal);
		EOS_PlayerDataStorage_DuplicateFile(base.InnerHandle, ref duplicateOptions2, clientDataAddress, onDuplicateFileCompleteCallbackInternal);
		duplicateOptions2.Dispose();
	}

	public void DeleteFile(DeleteFileOptions deleteOptions, object clientData, OnDeleteFileCompleteCallback completionCallback)
	{
		DeleteFileOptionsInternal deleteOptions2 = Helper.CopyPropertiesToNew<DeleteFileOptionsInternal>(deleteOptions);
		OnDeleteFileCompleteCallbackInternal onDeleteFileCompleteCallbackInternal = OnDeleteFileComplete;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionCallback, onDeleteFileCompleteCallbackInternal);
		EOS_PlayerDataStorage_DeleteFile(base.InnerHandle, ref deleteOptions2, clientDataAddress, onDeleteFileCompleteCallbackInternal);
		deleteOptions2.Dispose();
	}

	public PlayerDataStorageFileTransferRequest ReadFile(ReadFileOptions readOptions, object clientData, OnReadFileCompleteCallback completionCallback)
	{
		ReadFileOptionsInternal readOptions2 = Helper.CopyPropertiesToNew<ReadFileOptionsInternal>(readOptions);
		OnReadFileCompleteCallbackInternal onReadFileCompleteCallbackInternal = OnReadFileComplete;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionCallback, onReadFileCompleteCallbackInternal, readOptions.ReadFileDataCallback, readOptions2.ReadFileDataCallbackInternal, readOptions.FileTransferProgressCallback, readOptions2.FileTransferProgressCallbackInternal);
		IntPtr innerHandle = EOS_PlayerDataStorage_ReadFile(base.InnerHandle, ref readOptions2, clientDataAddress, onReadFileCompleteCallbackInternal);
		readOptions2.Dispose();
		return Helper.GetHandle<PlayerDataStorageFileTransferRequest>(innerHandle);
	}

	public PlayerDataStorageFileTransferRequest WriteFile(WriteFileOptions writeOptions, object clientData, OnWriteFileCompleteCallback completionCallback)
	{
		WriteFileOptionsInternal writeOptions2 = Helper.CopyPropertiesToNew<WriteFileOptionsInternal>(writeOptions);
		OnWriteFileCompleteCallbackInternal onWriteFileCompleteCallbackInternal = OnWriteFileComplete;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionCallback, onWriteFileCompleteCallbackInternal, writeOptions.WriteFileDataCallback, writeOptions2.WriteFileDataCallbackInternal, writeOptions.FileTransferProgressCallback, writeOptions2.FileTransferProgressCallbackInternal);
		IntPtr innerHandle = EOS_PlayerDataStorage_WriteFile(base.InnerHandle, ref writeOptions2, clientDataAddress, onWriteFileCompleteCallbackInternal);
		writeOptions2.Dispose();
		return Helper.GetHandle<PlayerDataStorageFileTransferRequest>(innerHandle);
	}

	[MonoPInvokeCallback]
	internal static WriteResult OnWriteFileData(IntPtr address, IntPtr outDataBuffer, ref uint outDataWritten)
	{
		OnWriteFileDataCallback callDelegate = null;
		WriteFileDataCallbackInfo callbackInfo = null;
		if (Helper.TryGetAdditionalCallDelegate<OnWriteFileDataCallback, WriteFileDataCallbackInfoInternal, WriteFileDataCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			byte[] outDataBuffer2 = null;
			WriteResult result = callDelegate(callbackInfo, out outDataBuffer2, out outDataWritten);
			Marshal.Copy(outDataBuffer2, 0, outDataBuffer, (int)outDataWritten);
			return result;
		}
		return Helper.GetDefault<WriteResult>();
	}

	[MonoPInvokeCallback]
	internal static void OnFileTransferProgress(IntPtr address)
	{
		OnFileTransferProgressCallback callDelegate = null;
		FileTransferProgressCallbackInfo callbackInfo = null;
		if (Helper.TryGetAdditionalCallDelegate<OnFileTransferProgressCallback, FileTransferProgressCallbackInfoInternal, FileTransferProgressCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static ReadResult OnReadFileData(IntPtr address)
	{
		OnReadFileDataCallback callDelegate = null;
		ReadFileDataCallbackInfo callbackInfo = null;
		if (Helper.TryGetAdditionalCallDelegate<OnReadFileDataCallback, ReadFileDataCallbackInfoInternal, ReadFileDataCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			return callDelegate(callbackInfo);
		}
		return Helper.GetDefault<ReadResult>();
	}

	[MonoPInvokeCallback]
	internal static void OnWriteFileComplete(IntPtr address)
	{
		OnWriteFileCompleteCallback callDelegate = null;
		WriteFileCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnWriteFileCompleteCallback, WriteFileCallbackInfoInternal, WriteFileCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnReadFileComplete(IntPtr address)
	{
		OnReadFileCompleteCallback callDelegate = null;
		ReadFileCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnReadFileCompleteCallback, ReadFileCallbackInfoInternal, ReadFileCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnDeleteFileComplete(IntPtr address)
	{
		OnDeleteFileCompleteCallback callDelegate = null;
		DeleteFileCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnDeleteFileCompleteCallback, DeleteFileCallbackInfoInternal, DeleteFileCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnDuplicateFileComplete(IntPtr address)
	{
		OnDuplicateFileCompleteCallback callDelegate = null;
		DuplicateFileCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnDuplicateFileCompleteCallback, DuplicateFileCallbackInfoInternal, DuplicateFileCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnQueryFileListComplete(IntPtr address)
	{
		OnQueryFileListCompleteCallback callDelegate = null;
		QueryFileListCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnQueryFileListCompleteCallback, QueryFileListCallbackInfoInternal, QueryFileListCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnQueryFileComplete(IntPtr address)
	{
		OnQueryFileCompleteCallback callDelegate = null;
		QueryFileCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnQueryFileCompleteCallback, QueryFileCallbackInfoInternal, QueryFileCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_PlayerDataStorage_FileMetadata_Release(IntPtr fileMetadata);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern IntPtr EOS_PlayerDataStorage_WriteFile(IntPtr handle, ref WriteFileOptionsInternal writeOptions, IntPtr clientData, OnWriteFileCompleteCallbackInternal completionCallback);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern IntPtr EOS_PlayerDataStorage_ReadFile(IntPtr handle, ref ReadFileOptionsInternal readOptions, IntPtr clientData, OnReadFileCompleteCallbackInternal completionCallback);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_PlayerDataStorage_DeleteFile(IntPtr handle, ref DeleteFileOptionsInternal deleteOptions, IntPtr clientData, OnDeleteFileCompleteCallbackInternal completionCallback);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_PlayerDataStorage_DuplicateFile(IntPtr handle, ref DuplicateFileOptionsInternal duplicateOptions, IntPtr clientData, OnDuplicateFileCompleteCallbackInternal completionCallback);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_PlayerDataStorage_CopyFileMetadataAtIndex(IntPtr handle, ref CopyFileMetadataAtIndexOptionsInternal copyFileMetadataOptions, ref IntPtr outMetadata);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_PlayerDataStorage_GetFileMetadataCount(IntPtr handle, ref GetFileMetadataCountOptionsInternal getFileMetadataCountOptions, ref int outFileMetadataCount);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_PlayerDataStorage_CopyFileMetadataByFilename(IntPtr handle, ref CopyFileMetadataByFilenameOptionsInternal copyFileMetadataOptions, ref IntPtr outMetadata);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_PlayerDataStorage_QueryFileList(IntPtr handle, ref QueryFileListOptionsInternal queryFileListOptions, IntPtr clientData, OnQueryFileListCompleteCallbackInternal completionCallback);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_PlayerDataStorage_QueryFile(IntPtr handle, ref QueryFileOptionsInternal queryFileOptions, IntPtr clientData, OnQueryFileCompleteCallbackInternal completionCallback);
}
