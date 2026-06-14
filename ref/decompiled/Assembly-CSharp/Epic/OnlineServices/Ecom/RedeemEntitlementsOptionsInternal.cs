using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct RedeemEntitlementsOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	private uint m_EntitlementIdCount;

	private IntPtr m_EntitlementIds;

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

	public string[] EntitlementIds
	{
		get
		{
			return Helper.GetAllocation<string[]>(m_EntitlementIds, (int)m_EntitlementIdCount);
		}
		set
		{
			Helper.RegisterArrayAllocation(ref m_EntitlementIds, value, out m_EntitlementIdCount);
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_EntitlementIds);
	}
}
