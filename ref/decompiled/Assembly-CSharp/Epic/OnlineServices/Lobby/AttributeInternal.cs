using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct AttributeInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_Data;

	private LobbyAttributeVisibility m_Visbility;

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

	public AttributeDataInternal Data
	{
		get
		{
			return Helper.GetAllocation<AttributeDataInternal>(m_Data);
		}
		set
		{
			Helper.RegisterAllocation(ref m_Data, value);
		}
	}

	public LobbyAttributeVisibility Visbility
	{
		get
		{
			return m_Visbility;
		}
		set
		{
			m_Visbility = value;
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_Data);
	}
}
