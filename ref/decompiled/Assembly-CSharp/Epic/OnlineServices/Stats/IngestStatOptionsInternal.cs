using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Stats;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct IngestStatOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_UserId;

	private IntPtr m_Stats;

	private uint m_StatsCount;

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

	public IngestDataInternal[] Stats
	{
		get
		{
			return Helper.GetAllocation<IngestDataInternal[]>(m_Stats, (int)m_StatsCount);
		}
		set
		{
			Helper.RegisterArrayAllocation(ref m_Stats, value, out m_StatsCount);
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_Stats);
	}
}
