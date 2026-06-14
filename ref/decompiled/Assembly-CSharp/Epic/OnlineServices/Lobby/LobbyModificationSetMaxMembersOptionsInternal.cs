using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct LobbyModificationSetMaxMembersOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private uint m_MaxMembers;

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

	public uint MaxMembers
	{
		get
		{
			return m_MaxMembers;
		}
		set
		{
			m_MaxMembers = value;
		}
	}

	public void Dispose()
	{
	}
}
