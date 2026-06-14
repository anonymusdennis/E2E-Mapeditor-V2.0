using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SessionDetailsCopySessionAttributeByIndexOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private uint m_AttrIndex;

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

	public uint AttrIndex
	{
		get
		{
			return m_AttrIndex;
		}
		set
		{
			m_AttrIndex = value;
		}
	}

	public void Dispose()
	{
	}
}
