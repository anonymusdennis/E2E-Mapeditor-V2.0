using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct JoinSessionOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_SessionName;

	private IntPtr m_SessionHandle;

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

	public SessionDetails SessionHandle
	{
		get
		{
			return Helper.GetHandle<SessionDetails>(m_SessionHandle);
		}
		set
		{
			m_SessionHandle = Helper.GetInnerHandle(value);
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
