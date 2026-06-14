using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Metrics;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct EndPlayerSessionOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private MetricsAccountIdType m_AccountIdType;

	private IntPtr m_AccountId;

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

	public void Dispose()
	{
	}
}
