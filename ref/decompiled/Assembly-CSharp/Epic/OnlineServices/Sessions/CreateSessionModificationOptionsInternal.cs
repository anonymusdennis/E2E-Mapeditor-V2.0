using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CreateSessionModificationOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_SessionName;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_BucketId;

	private uint m_MaxPlayers;

	private IntPtr m_LocalUserId;

	private int m_PresenceEnabled;

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

	public string SessionName
	{
		get
		{
			return m_SessionName;
		}
		set
		{
			m_SessionName = value;
		}
	}

	public string BucketId
	{
		get
		{
			return m_BucketId;
		}
		set
		{
			m_BucketId = value;
		}
	}

	public uint MaxPlayers
	{
		get
		{
			return m_MaxPlayers;
		}
		set
		{
			m_MaxPlayers = value;
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

	public bool PresenceEnabled
	{
		get
		{
			return Helper.GetBoolFromInt(m_PresenceEnabled);
		}
		set
		{
			m_PresenceEnabled = Helper.GetIntFromBool(value);
		}
	}

	public void Dispose()
	{
	}
}
