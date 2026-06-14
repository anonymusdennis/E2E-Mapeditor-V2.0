using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct TokenInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_App;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_ClientId;

	private IntPtr m_AccountId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_AccessToken;

	private double m_ExpiresIn;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_ExpiresAt;

	private AuthTokenType m_AuthType;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_RefreshToken;

	private double m_RefreshExpiresIn;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_RefreshExpiresAt;

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

	public string App
	{
		get
		{
			return m_App;
		}
		set
		{
			m_App = value;
		}
	}

	public string ClientId
	{
		get
		{
			return m_ClientId;
		}
		set
		{
			m_ClientId = value;
		}
	}

	public EpicAccountId AccountId
	{
		get
		{
			return Helper.GetHandle<EpicAccountId>(m_AccountId);
		}
		set
		{
			m_AccountId = Helper.GetInnerHandle(value);
		}
	}

	public string AccessToken
	{
		get
		{
			return m_AccessToken;
		}
		set
		{
			m_AccessToken = value;
		}
	}

	public double ExpiresIn
	{
		get
		{
			return m_ExpiresIn;
		}
		set
		{
			m_ExpiresIn = value;
		}
	}

	public string ExpiresAt
	{
		get
		{
			return m_ExpiresAt;
		}
		set
		{
			m_ExpiresAt = value;
		}
	}

	public AuthTokenType AuthType
	{
		get
		{
			return m_AuthType;
		}
		set
		{
			m_AuthType = value;
		}
	}

	public string RefreshToken
	{
		get
		{
			return m_RefreshToken;
		}
		set
		{
			m_RefreshToken = value;
		}
	}

	public double RefreshExpiresIn
	{
		get
		{
			return m_RefreshExpiresIn;
		}
		set
		{
			m_RefreshExpiresIn = value;
		}
	}

	public string RefreshExpiresAt
	{
		get
		{
			return m_RefreshExpiresAt;
		}
		set
		{
			m_RefreshExpiresAt = value;
		}
	}

	public void Dispose()
	{
	}
}
