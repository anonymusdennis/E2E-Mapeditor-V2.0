using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct InfoInternal : IDisposable
{
	private int m_ApiVersion;

	private Status m_Status;

	private IntPtr m_UserId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_ProductId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_ProductVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_Platform;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_RichText;

	private int m_RecordsCount;

	private IntPtr m_Records;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_ProductName;

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

	public Status Status
	{
		get
		{
			return m_Status;
		}
		set
		{
			m_Status = value;
		}
	}

	public EpicAccountId UserId
	{
		get
		{
			return Helper.GetHandle<EpicAccountId>(m_UserId);
		}
		set
		{
			m_UserId = Helper.GetInnerHandle(value);
		}
	}

	public string ProductId
	{
		get
		{
			return m_ProductId;
		}
		set
		{
			m_ProductId = value;
		}
	}

	public string ProductVersion
	{
		get
		{
			return m_ProductVersion;
		}
		set
		{
			m_ProductVersion = value;
		}
	}

	public string Platform
	{
		get
		{
			return m_Platform;
		}
		set
		{
			m_Platform = value;
		}
	}

	public string RichText
	{
		get
		{
			return m_RichText;
		}
		set
		{
			m_RichText = value;
		}
	}

	public DataRecordInternal[] Records
	{
		get
		{
			return Helper.GetAllocation<DataRecordInternal[]>(m_Records, m_RecordsCount);
		}
		set
		{
			Helper.RegisterArrayAllocation(ref m_Records, value, out m_RecordsCount);
		}
	}

	public string ProductName
	{
		get
		{
			return m_ProductName;
		}
		set
		{
			m_ProductName = value;
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_Records);
	}
}
