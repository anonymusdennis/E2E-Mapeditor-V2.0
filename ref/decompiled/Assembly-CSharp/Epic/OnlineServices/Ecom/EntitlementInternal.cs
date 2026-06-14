using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct EntitlementInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_EntitlementName;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_EntitlementId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_CatalogItemId;

	private int m_ServerIndex;

	private int m_Redeemed;

	private long m_EndTimestamp;

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

	public string EntitlementId
	{
		get
		{
			return m_EntitlementId;
		}
		set
		{
			m_EntitlementId = value;
		}
	}

	public string CatalogItemId
	{
		get
		{
			return m_CatalogItemId;
		}
		set
		{
			m_CatalogItemId = value;
		}
	}

	public int ServerIndex
	{
		get
		{
			return m_ServerIndex;
		}
		set
		{
			m_ServerIndex = value;
		}
	}

	public bool Redeemed
	{
		get
		{
			return Helper.GetBoolFromInt(m_Redeemed);
		}
		set
		{
			m_Redeemed = Helper.GetIntFromBool(value);
		}
	}

	public long EndTimestamp
	{
		get
		{
			return m_EndTimestamp;
		}
		set
		{
			m_EndTimestamp = value;
		}
	}

	public void Dispose()
	{
	}
}
