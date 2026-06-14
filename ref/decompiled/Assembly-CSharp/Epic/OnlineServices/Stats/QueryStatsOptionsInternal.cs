using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Stats;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct QueryStatsOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_UserId;

	private long m_StartTime;

	private long m_EndTime;

	private IntPtr m_StatNames;

	private uint m_StatNamesCount;

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

	public ProductUserId UserId
	{
		get
		{
			return Helper.GetHandle<ProductUserId>(m_UserId);
		}
		set
		{
			m_UserId = Helper.GetInnerHandle(value);
		}
	}

	public long StartTime
	{
		get
		{
			return m_StartTime;
		}
		set
		{
			m_StartTime = value;
		}
	}

	public long EndTime
	{
		get
		{
			return m_EndTime;
		}
		set
		{
			m_EndTime = value;
		}
	}

	public string[] StatNames
	{
		get
		{
			return Helper.GetAllocation<string[]>(m_StatNames, (int)m_StatNamesCount);
		}
		set
		{
			Helper.RegisterArrayAllocation(ref m_StatNames, value, out m_StatNamesCount);
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_StatNames);
	}
}
