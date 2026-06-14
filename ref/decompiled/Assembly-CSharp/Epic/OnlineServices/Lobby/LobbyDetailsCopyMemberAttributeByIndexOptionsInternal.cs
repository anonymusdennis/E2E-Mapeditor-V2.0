using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct LobbyDetailsCopyMemberAttributeByIndexOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_TargetUserId;

	private uint m_AttrIndex;

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

	public uint AttrIndex
	{
		get
		{
			return m_AttrIndex;
		}
		set
		{
			m_AttrIndex = value;
		}
	}

	public void Dispose()
	{
	}
}
