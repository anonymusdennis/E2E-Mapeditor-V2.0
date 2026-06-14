using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct LobbyDetailsGetMemberByIndexOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private uint m_MemberIndex;

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

	public uint MemberIndex
	{
		get
		{
			return m_MemberIndex;
		}
		set
		{
			m_MemberIndex = value;
		}
	}

	public void Dispose()
	{
	}
}
