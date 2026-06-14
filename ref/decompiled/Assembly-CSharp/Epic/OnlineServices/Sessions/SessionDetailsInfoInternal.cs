using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SessionDetailsInfoInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_SessionId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_HostAddress;

	private uint m_NumOpenPublicConnections;

	private IntPtr m_Settings;

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

	public string SessionId
	{
		get
		{
			return m_SessionId;
		}
		set
		{
			m_SessionId = value;
		}
	}

	public string HostAddress
	{
		get
		{
			return m_HostAddress;
		}
		set
		{
			m_HostAddress = value;
		}
	}

	public uint NumOpenPublicConnections
	{
		get
		{
			return m_NumOpenPublicConnections;
		}
		set
		{
			m_NumOpenPublicConnections = value;
		}
	}

	public SessionDetailsSettingsInternal Settings
	{
		get
		{
			return Helper.GetAllocation<SessionDetailsSettingsInternal>(m_Settings);
		}
		set
		{
			Helper.RegisterAllocation(ref m_Settings, value);
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_Settings);
	}
}
