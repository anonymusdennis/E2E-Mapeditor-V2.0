using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CatalogOfferInternal : IDisposable
{
	private int m_ApiVersion;

	private int m_ServerIndex;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_CatalogNamespace;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_Id;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_TitleText;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_DescriptionText;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_LongDescriptionText;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_TechnicalDetailsText_DEPRECATED;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_CurrencyCode;

	private Result m_PriceResult;

	private uint m_OriginalPrice;

	private uint m_CurrentPrice;

	private byte m_DiscountPercentage;

	private long m_ExpirationTimestamp;

	private uint m_PurchasedCount;

	private int m_PurchaseLimit;

	private int m_AvailableForPurchase;

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

	public string TechnicalDetailsText_DEPRECATED
	{
		get
		{
			return m_TechnicalDetailsText_DEPRECATED;
		}
		set
		{
			m_TechnicalDetailsText_DEPRECATED = value;
		}
	}

	public string CurrencyCode
	{
		get
		{
			return m_CurrencyCode;
		}
		set
		{
			m_CurrencyCode = value;
		}
	}

	public Result PriceResult
	{
		get
		{
			return m_PriceResult;
		}
		set
		{
			m_PriceResult = value;
		}
	}

	public uint OriginalPrice
	{
		get
		{
			return m_OriginalPrice;
		}
		set
		{
			m_OriginalPrice = value;
		}
	}

	public uint CurrentPrice
	{
		get
		{
			return m_CurrentPrice;
		}
		set
		{
			m_CurrentPrice = value;
		}
	}

	public byte DiscountPercentage
	{
		get
		{
			return m_DiscountPercentage;
		}
		set
		{
			m_DiscountPercentage = value;
		}
	}

	public long ExpirationTimestamp
	{
		get
		{
			return m_ExpirationTimestamp;
		}
		set
		{
			m_ExpirationTimestamp = value;
		}
	}

	public uint PurchasedCount
	{
		get
		{
			return m_PurchasedCount;
		}
		set
		{
			m_PurchasedCount = value;
		}
	}

	public int PurchaseLimit
	{
		get
		{
			return m_PurchaseLimit;
		}
		set
		{
			m_PurchaseLimit = value;
		}
	}

	public bool AvailableForPurchase
	{
		get
		{
			return Helper.GetBoolFromInt(m_AvailableForPurchase);
		}
		set
		{
			m_AvailableForPurchase = Helper.GetIntFromBool(value);
		}
	}

	public void Dispose()
	{
	}
}
