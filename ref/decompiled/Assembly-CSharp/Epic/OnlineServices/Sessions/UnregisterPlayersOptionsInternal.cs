using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct UnregisterPlayersOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_SessionName;

	private IntPtr m_PlayersToUnregister;

	private uint m_PlayersToUnregisterCount;

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

	public string SessionName
	{
		get
		{
			return m_SessionName;
		}
		set
		{
			m_SessionName = value;
		}
	}

	public ProductUserId[] PlayersToUnregister
	{
		get
		{
			return Helper.GetHandleArrayAllocation<ProductUserId>(m_PlayersToUnregister, (int)m_PlayersToUnregisterCount);
		}
		set
		{
			Helper.RegisterHandleArrayAllocation(ref m_PlayersToUnregister, value, out m_PlayersToUnregisterCount);
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_PlayersToUnregister);
	}
}
