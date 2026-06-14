using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.PlayerDataStorage;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct ReadFileOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_Filename;

	private uint m_ReadChunkLengthBytes;

	private OnReadFileDataCallbackInternal m_ReadFileDataCallback;

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

	public uint ReadChunkLengthBytes
	{
		get
		{
			return m_ReadChunkLengthBytes;
		}
		set
		{
			m_ReadChunkLengthBytes = value;
		}
	}

	public OnReadFileDataCallback ReadFileDataCallback
	{
		get
		{
			return null;
		}
		set
		{
			m_ReadFileDataCallback = PlayerDataStorageInterface.OnReadFileData;
		}
	}

	public OnReadFileDataCallbackInternal ReadFileDataCallbackInternal => m_ReadFileDataCallback;

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
