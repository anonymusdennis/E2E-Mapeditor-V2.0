using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CopyItemByIdOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_ItemId;

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

	public string ItemId
	{
		get
		{
			return m_ItemId;
		}
		set
		{
			m_ItemId = value;
		}
	}

	public void Dispose()
	{
	}
}
