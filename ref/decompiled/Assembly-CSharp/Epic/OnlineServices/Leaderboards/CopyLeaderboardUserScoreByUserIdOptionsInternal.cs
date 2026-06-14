using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CopyLeaderboardUserScoreByUserIdOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_UserId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_StatName;

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

	public string StatName
	{
		get
		{
			return m_StatName;
		}
		set
		{
			m_StatName = value;
		}
	}

	public void Dispose()
	{
	}
}
