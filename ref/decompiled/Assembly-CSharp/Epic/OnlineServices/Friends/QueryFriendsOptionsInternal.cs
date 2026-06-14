using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Friends;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct QueryFriendsOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

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

	public void Dispose()
	{
	}
}
