using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Metrics;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct BeginPlayerSessionOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private MetricsAccountIdType m_AccountIdType;

	private IntPtr m_AccountId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_DisplayName;

	private UserControllerType m_ControllerType;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_ServerIp;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_GameSessionId;

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

	public MetricsAccountIdType AccountIdType
	{
		get
		{
			return m_AccountIdType;
		}
		set
		{
			m_AccountIdType = value;
		}
	}

	public IntPtr AccountId
	{
		get
		{
			return m_AccountId;
		}
		set
		{
			m_AccountId = value;
		}
	}

	public string DisplayName
	{
		get
		{
			return m_DisplayName;
		}
		set
		{
			m_DisplayName = value;
		}
	}

	public UserControllerType ControllerType
	{
		get
		{
			return m_ControllerType;
		}
		set
		{
			m_ControllerType = value;
		}
	}

	public string ServerIp
	{
		get
		{
			return m_ServerIp;
		}
		set
		{
			m_ServerIp = value;
		}
	}

	public string GameSessionId
	{
		get
		{
			return m_GameSessionId;
		}
		set
		{
			m_GameSessionId = value;
		}
	}

	public void Dispose()
	{
	}
}
