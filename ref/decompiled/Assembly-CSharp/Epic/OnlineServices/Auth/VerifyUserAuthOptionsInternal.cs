using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct VerifyUserAuthOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_AuthToken;

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

	public TokenInternal AuthToken
	{
		get
		{
			return Helper.GetAllocation<TokenInternal>(m_AuthToken);
		}
		set
		{
			Helper.RegisterAllocation(ref m_AuthToken, value);
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_AuthToken);
	}
}
