using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SessionSearchSetTargetUserIdOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_TargetUserId;

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

	public void Dispose()
	{
	}
}
