using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct ItemOwnershipInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_Id;

	private OwnershipStatus m_OwnershipStatus;

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

	public OwnershipStatus OwnershipStatus
	{
		get
		{
			return m_OwnershipStatus;
		}
		set
		{
			m_OwnershipStatus = value;
		}
	}

	public void Dispose()
	{
	}
}
