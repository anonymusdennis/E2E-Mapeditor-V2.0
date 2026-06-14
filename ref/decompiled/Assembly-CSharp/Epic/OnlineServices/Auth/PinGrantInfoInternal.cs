using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct PinGrantInfoInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_UserCode;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_VerificationURI;

	private int m_ExpiresIn;

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

	public string UserCode
	{
		get
		{
			return m_UserCode;
		}
		set
		{
			m_UserCode = value;
		}
	}

	public string VerificationURI
	{
		get
		{
			return m_VerificationURI;
		}
		set
		{
			m_VerificationURI = value;
		}
	}

	public int ExpiresIn
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

	public void Dispose()
	{
	}
}
