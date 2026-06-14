using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CheckoutOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_OverrideCatalogNamespace;

	private uint m_EntryCount;

	private IntPtr m_Entries;

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

	public string OverrideCatalogNamespace
	{
		get
		{
			return m_OverrideCatalogNamespace;
		}
		set
		{
			m_OverrideCatalogNamespace = value;
		}
	}

	public CheckoutEntryInternal[] Entries
	{
		get
		{
			return Helper.GetAllocation<CheckoutEntryInternal[]>(m_Entries, (int)m_EntryCount);
		}
		set
		{
			Helper.RegisterArrayAllocation(ref m_Entries, value, out m_EntryCount);
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_Entries);
	}
}
