using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CopyOfferImageInfoByIndexOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_OfferId;

	private uint m_ImageInfoIndex;

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

	public string OfferId
	{
		get
		{
			return m_OfferId;
		}
		set
		{
			m_OfferId = value;
		}
	}

	public uint ImageInfoIndex
	{
		get
		{
			return m_ImageInfoIndex;
		}
		set
		{
			m_ImageInfoIndex = value;
		}
	}

	public void Dispose()
	{
	}
}
