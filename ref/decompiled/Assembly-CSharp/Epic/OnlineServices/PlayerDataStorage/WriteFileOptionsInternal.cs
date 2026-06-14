using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.PlayerDataStorage;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct WriteFileOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_Filename;

	private uint m_ChunkLengthBytes;

	private OnWriteFileDataCallbackInternal m_WriteFileDataCallback;

	private OnFileTransferProgressCallbackInternal m_FileTransferProgressCallback;

	public int ApiVersion
	{
		get
		{
			return m_ApiVersion;
		}
		set
		{
			m_ApiVersion = value;
		}
	}

	public ProductUserId LocalUserId
	{
		get
		{
			return Helper.GetHandle<ProductUserId>(m_LocalUserId);
		}
		set
		{
			m_LocalUserId = Helper.GetInnerHandle(value);
		}
	}

	public string Filename
	{
		get
		{
			return m_Filename;
		}
		set
		{
			m_Filename = value;
		}
	}

	public uint ChunkLengthBytes
	{
		get
		{
			return m_ChunkLengthBytes;
		}
		set
		{
			m_ChunkLengthBytes = value;
		}
	}

	public OnWriteFileDataCallback WriteFileDataCallback
	{
		get
		{
			return null;
		}
		set
		{
			m_WriteFileDataCallback = PlayerDataStorageInterface.OnWriteFileData;
		}
	}

	public OnWriteFileDataCallbackInternal WriteFileDataCallbackInternal => m_WriteFileDataCallback;

	public OnFileTransferProgressCallback FileTransferProgressCallback
	{
		get
		{
			return null;
		}
		set
		{
			m_FileTransferProgressCallback = PlayerDataStorageInterface.OnFileTransferProgress;
		}
	}

	public OnFileTransferProgressCallbackInternal FileTransferProgressCallbackInternal => m_FileTransferProgressCallback;

	public void Dispose()
	{
	}
}
