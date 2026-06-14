using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct QueryExternalAccountMappingsOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	private ExternalAccountType m_AccountIdType;

	private IntPtr m_ExternalAccountIds;

	private uint m_ExternalAccountIdCount;

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

	public string[] ExternalAccountIds
	{
		get
		{
			return Helper.GetAllocation<string[]>(m_ExternalAccountIds, (int)m_ExternalAccountIdCount);
		}
		set
		{
			Helper.RegisterArrayAllocation(ref m_ExternalAccountIds, value, out m_ExternalAccountIdCount);
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_ExternalAccountIds);
	}
}
