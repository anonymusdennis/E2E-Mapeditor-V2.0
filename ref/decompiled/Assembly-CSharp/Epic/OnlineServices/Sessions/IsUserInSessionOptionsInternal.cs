using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct IsUserInSessionOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_SessionName;

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
