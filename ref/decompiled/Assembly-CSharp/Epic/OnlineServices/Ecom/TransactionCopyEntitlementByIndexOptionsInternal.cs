using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct TransactionCopyEntitlementByIndexOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private uint m_EntitlementIndex;

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

	public uint EntitlementIndex
	{
		get
		{
			return m_EntitlementIndex;
		}
		set
		{
			m_EntitlementIndex = value;
		}
	}

	public void Dispose()
	{
	}
}
