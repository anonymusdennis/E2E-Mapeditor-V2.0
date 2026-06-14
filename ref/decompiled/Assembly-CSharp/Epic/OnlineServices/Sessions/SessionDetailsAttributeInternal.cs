using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SessionDetailsAttributeInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_Data;

	private SessionAttributeAdvertisementType m_AdvertisementType;

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

	public AttributeDataInternal Data
	{
		get
		{
			return Helper.GetAllocation<AttributeDataInternal>(m_Data);
		}
		set
		{
			Helper.RegisterAllocation(ref m_Data, value);
		}
	}

	public SessionAttributeAdvertisementType AdvertisementType
	{
		get
		{
			return m_AdvertisementType;
		}
		set
		{
			m_AdvertisementType = value;
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_Data);
	}
}
