using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Platform;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct ClientCredentialsInternal : IDisposable
{
	[MarshalAs(UnmanagedType.LPStr)]
	private string m_ClientId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_ClientSecret;

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

	public string ClientSecret
	{
		get
		{
			return m_ClientSecret;
		}
		set
		{
			m_ClientSecret = value;
		}
	}

	public void Dispose()
	{
	}
}
