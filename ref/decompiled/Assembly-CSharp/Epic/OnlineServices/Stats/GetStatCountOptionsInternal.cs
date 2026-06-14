using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Stats;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct GetStatCountOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_UserId;

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

	public ProductUserId UserId
	{
		get
		{
			return Helper.GetHandle<ProductUserId>(m_UserId);
		}
		set
		{
			m_UserId = Helper.GetInnerHandle(value);
		}
	}

	public void Dispose()
	{
	}
}
