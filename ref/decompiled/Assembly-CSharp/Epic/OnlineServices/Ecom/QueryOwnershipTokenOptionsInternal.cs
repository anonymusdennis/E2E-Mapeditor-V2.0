using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct QueryOwnershipTokenOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	private IntPtr m_CatalogItemIds;

	private uint m_CatalogItemIdCount;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_CatalogNamespace;

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

	public string[] CatalogItemIds
	{
		get
		{
			return Helper.GetAllocation<string[]>(m_CatalogItemIds, (int)m_CatalogItemIdCount);
		}
		set
		{
			Helper.RegisterArrayAllocation(ref m_CatalogItemIds, value, out m_CatalogItemIdCount);
		}
	}

	public string CatalogNamespace
	{
		get
		{
			return m_CatalogNamespace;
		}
		set
		{
			m_CatalogNamespace = value;
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_CatalogItemIds);
	}
}
