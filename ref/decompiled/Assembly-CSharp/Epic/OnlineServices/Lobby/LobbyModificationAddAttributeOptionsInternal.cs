using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct LobbyModificationAddAttributeOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_Attribute;

	private LobbyAttributeVisibility m_Visibility;

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

	public AttributeDataInternal Attribute
	{
		get
		{
			return Helper.GetAllocation<AttributeDataInternal>(m_Attribute);
		}
		set
		{
			Helper.RegisterAllocation(ref m_Attribute, value);
		}
	}

	public LobbyAttributeVisibility Visibility
	{
		get
		{
			return m_Visibility;
		}
		set
		{
			m_Visibility = value;
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_Attribute);
	}
}
