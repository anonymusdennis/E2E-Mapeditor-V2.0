using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct PlayerStatInfoInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_Name;

	private int m_CurrentValue;

	private int m_ThresholdValue;

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

	public int CurrentValue
	{
		get
		{
			return m_CurrentValue;
		}
		set
		{
			m_CurrentValue = value;
		}
	}

	public int ThresholdValue
	{
		get
		{
			return m_ThresholdValue;
		}
		set
		{
			m_ThresholdValue = value;
		}
	}

	public void Dispose()
	{
	}
}
