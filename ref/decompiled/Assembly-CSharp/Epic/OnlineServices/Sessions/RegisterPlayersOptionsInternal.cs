using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct RegisterPlayersOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_SessionName;

	private IntPtr m_PlayersToRegister;

	private uint m_PlayersToRegisterCount;

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

	public ProductUserId[] PlayersToRegister
	{
		get
		{
			return Helper.GetHandleArrayAllocation<ProductUserId>(m_PlayersToRegister, (int)m_PlayersToRegisterCount);
		}
		set
		{
			Helper.RegisterHandleArrayAllocation(ref m_PlayersToRegister, value, out m_PlayersToRegisterCount);
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_PlayersToRegister);
	}
}
