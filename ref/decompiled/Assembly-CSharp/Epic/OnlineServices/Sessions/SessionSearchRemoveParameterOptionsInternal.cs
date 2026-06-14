using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SessionSearchRemoveParameterOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_Key;

	private ComparisonOp m_ComparisonOp;

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

	public string Key
	{
		get
		{
			return m_Key;
		}
		set
		{
			m_Key = value;
		}
	}

	public ComparisonOp ComparisonOp
	{
		get
		{
			return m_ComparisonOp;
		}
		set
		{
			m_ComparisonOp = value;
		}
	}

	public void Dispose()
	{
	}
}
