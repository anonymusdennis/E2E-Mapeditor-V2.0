using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct PageQueryInternal : IDisposable
{
	private int m_ApiVersion;

	private int m_StartIndex;

	private int m_MaxCount;

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

	public int StartIndex
	{
		get
		{
			return m_StartIndex;
		}
		set
		{
			m_StartIndex = value;
		}
	}

	public int MaxCount
	{
		get
		{
			return m_MaxCount;
		}
		set
		{
			m_MaxCount = value;
		}
	}

	public void Dispose()
	{
	}
}
