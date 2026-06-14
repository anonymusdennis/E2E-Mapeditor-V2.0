using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct LobbyDetailsCopyMemberAttributeByKeyOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_TargetUserId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_AttrKey;

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

	public ProductUserId TargetUserId
	{
		get
		{
			return Helper.GetHandle<ProductUserId>(m_TargetUserId);
		}
		set
		{
			m_TargetUserId = Helper.GetInnerHandle(value);
		}
	}

	public string AttrKey
	{
		get
		{
			return m_AttrKey;
		}
		set
		{
			m_AttrKey = value;
		}
	}

	public void Dispose()
	{
	}
}
