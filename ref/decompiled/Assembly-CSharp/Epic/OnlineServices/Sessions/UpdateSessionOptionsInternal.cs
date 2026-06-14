using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct UpdateSessionOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_SessionModificationHandle;

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

	public SessionModification SessionModificationHandle
	{
		get
		{
			return Helper.GetHandle<SessionModification>(m_SessionModificationHandle);
		}
		set
		{
			m_SessionModificationHandle = Helper.GetInnerHandle(value);
		}
	}

	public void Dispose()
	{
	}
}
