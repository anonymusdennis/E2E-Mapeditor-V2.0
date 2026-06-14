using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct GetExternalAccountMappingsOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	private ExternalAccountType m_AccountIdType;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_TargetExternalUserId;

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

	public ExternalAccountType AccountIdType
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

	public string TargetExternalUserId
	{
		get
		{
			return m_TargetExternalUserId;
		}
		set
		{
			m_TargetExternalUserId = value;
		}
	}

	public void Dispose()
	{
	}
}
