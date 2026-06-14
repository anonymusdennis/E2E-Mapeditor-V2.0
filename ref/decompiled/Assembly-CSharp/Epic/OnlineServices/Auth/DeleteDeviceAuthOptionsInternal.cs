using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct DeleteDeviceAuthOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

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

	public EpicAccountId LocalUserId
	{
		get
		{
			return Helper.GetHandle<EpicAccountId>(m_LocalUserId);
		}
		set
		{
			m_LocalUserId = Helper.GetInnerHandle(value);
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
