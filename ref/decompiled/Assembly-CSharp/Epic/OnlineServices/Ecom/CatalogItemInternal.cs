using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CatalogItemInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_CatalogNamespace;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_Id;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_EntitlementName;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_TitleText;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_DescriptionText;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_LongDescriptionText;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_TechnicalDetailsText;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_DeveloperText;

	private EcomItemType m_ItemType;

	private long m_EntitlementEndTimestamp;

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

	public string Id
	{
		get
		{
			return m_Id;
		}
		set
		{
			m_Id = value;
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

	public string TitleText
	{
		get
		{
			return m_TitleText;
		}
		set
		{
			m_TitleText = value;
		}
	}

	public string DescriptionText
	{
		get
		{
			return m_DescriptionText;
		}
		set
		{
			m_DescriptionText = value;
		}
	}

	public string LongDescriptionText
	{
		get
		{
			return m_LongDescriptionText;
		}
		set
		{
			m_LongDescriptionText = value;
		}
	}

	public string TechnicalDetailsText
	{
		get
		{
			return m_TechnicalDetailsText;
		}
		set
		{
			m_TechnicalDetailsText = value;
		}
	}

	public string DeveloperText
	{
		get
		{
			return m_DeveloperText;
		}
		set
		{
			m_DeveloperText = value;
		}
	}

	public EcomItemType ItemType
	{
		get
		{
			return m_ItemType;
		}
		set
		{
			m_ItemType = value;
		}
	}

	public long EntitlementEndTimestamp
	{
		get
		{
			return m_EntitlementEndTimestamp;
		}
		set
		{
			m_EntitlementEndTimestamp = value;
		}
	}

	public void Dispose()
	{
	}
}
