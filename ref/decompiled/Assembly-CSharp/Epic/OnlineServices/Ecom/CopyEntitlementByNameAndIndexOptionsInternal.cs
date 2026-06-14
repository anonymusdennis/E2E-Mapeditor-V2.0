using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CopyEntitlementByNameAndIndexOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_EntitlementName;

	private uint m_Index;

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

	public string EntitlementName
	{
		get
		{
			return m_EntitlementName;
		}
		set
		{
			m_EntitlementName = value;
		}
	}

	public uint Index
	{
		get
		{
			return m_Index;
		}
		set
		{
			m_Index = value;
		}
	}

	public void Dispose()
	{
	}
}
