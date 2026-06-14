using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SetPresenceOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	private IntPtr m_PresenceModificationHandle;

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

	public EpicAccountId LocalUserId
	{
		get
		{
			return Helper.GetHandle<EpicAccountId>(m_LocalUserId);
		}
		set
		{
			m_LocalUserId = Helper.GetInnerHandle(value);
		}
	}

	public PresenceModification PresenceModificationHandle
	{
		get
		{
			return Helper.GetHandle<PresenceModification>(m_PresenceModificationHandle);
		}
		set
		{
			m_PresenceModificationHandle = Helper.GetInnerHandle(value);
		}
	}

	public void Dispose()
	{
	}
}
