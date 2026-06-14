using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Stats;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct StatInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_Name;

	private long m_StartTime;

	private long m_EndTime;

	private int m_Value;

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

	public string Name
	{
		get
		{
			return m_Name;
		}
		set
		{
			m_Name = value;
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

	public int Value
	{
		get
		{
			return m_Value;
		}
		set
		{
			m_Value = value;
		}
	}

	public void Dispose()
	{
	}
}
