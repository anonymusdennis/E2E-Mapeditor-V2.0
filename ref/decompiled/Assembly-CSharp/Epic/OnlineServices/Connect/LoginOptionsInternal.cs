using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct LoginOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_Credentials;

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

	public CredentialsInternal Credentials
	{
		get
		{
			return Helper.GetAllocation<CredentialsInternal>(m_Credentials);
		}
		set
		{
			Helper.RegisterAllocation(ref m_Credentials, value);
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_Credentials);
	}
}
