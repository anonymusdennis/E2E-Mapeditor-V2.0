using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct LeaderboardRecordInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_UserId;

	private uint m_Rank;

	private int m_Score;

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

	public uint Rank
	{
		get
		{
			return m_Rank;
		}
		set
		{
			m_Rank = value;
		}
	}

	public int Score
	{
		get
		{
			return m_Score;
		}
		set
		{
			m_Score = value;
		}
	}

	public void Dispose()
	{
	}
}
