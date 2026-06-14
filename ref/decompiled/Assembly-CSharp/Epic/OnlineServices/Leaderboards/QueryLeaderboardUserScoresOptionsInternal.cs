using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct QueryLeaderboardUserScoresOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_UserIds;

	private uint m_UserIdsCount;

	private IntPtr m_StatInfo;

	private uint m_StatInfoCount;

	private long m_StartTime;

	private long m_EndTime;

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

	public ProductUserId[] UserIds
	{
		get
		{
			return Helper.GetHandleArrayAllocation<ProductUserId>(m_UserIds, (int)m_UserIdsCount);
		}
		set
		{
			Helper.RegisterHandleArrayAllocation(ref m_UserIds, value, out m_UserIdsCount);
		}
	}

	public UserScoresQueryStatInfoInternal[] StatInfo
	{
		get
		{
			return Helper.GetAllocation<UserScoresQueryStatInfoInternal[]>(m_StatInfo, (int)m_StatInfoCount);
		}
		set
		{
			Helper.RegisterArrayAllocation(ref m_StatInfo, value, out m_StatInfoCount);
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

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_UserIds);
		Helper.ReleaseAllocation(ref m_StatInfo);
	}
}
