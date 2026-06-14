using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Stats;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct IngestDataInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_StatName;

	private int m_IngestAmount;

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

	public int IngestAmount
	{
		get
		{
			return m_IngestAmount;
		}
		set
		{
			m_IngestAmount = value;
		}
	}

	public void Dispose()
	{
	}
}
