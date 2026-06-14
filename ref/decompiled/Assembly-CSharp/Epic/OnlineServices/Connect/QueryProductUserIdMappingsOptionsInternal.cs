using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct QueryProductUserIdMappingsOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	private ExternalAccountType m_AccountIdType;

	private IntPtr m_ProductUserIds;

	private uint m_ProductUserIdCount;

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

	public ProductUserId[] ProductUserIds
	{
		get
		{
			return Helper.GetHandleArrayAllocation<ProductUserId>(m_ProductUserIds, (int)m_ProductUserIdCount);
		}
		set
		{
			Helper.RegisterHandleArrayAllocation(ref m_ProductUserIds, value, out m_ProductUserIdCount);
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_ProductUserIds);
	}
}
