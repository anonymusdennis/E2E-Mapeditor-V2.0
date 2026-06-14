using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct QueryEntitlementsOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	private IntPtr m_EntitlementNames;

	private uint m_EntitlementNameCount;

	private int m_IncludeRedeemed;

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

	public string[] EntitlementNames
	{
		get
		{
			return Helper.GetAllocation<string[]>(m_EntitlementNames, (int)m_EntitlementNameCount);
		}
		set
		{
			Helper.RegisterArrayAllocation(ref m_EntitlementNames, value, out m_EntitlementNameCount);
		}
	}

	public bool IncludeRedeemed
	{
		get
		{
			return Helper.GetBoolFromInt(m_IncludeRedeemed);
		}
		set
		{
			m_IncludeRedeemed = Helper.GetIntFromBool(value);
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_EntitlementNames);
	}
}
