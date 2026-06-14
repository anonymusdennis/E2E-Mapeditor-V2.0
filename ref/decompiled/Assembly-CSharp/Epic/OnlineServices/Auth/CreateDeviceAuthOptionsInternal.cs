using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CreateDeviceAuthOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	private IntPtr m_DeviceInfo;

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

	public DeviceInfoInternal DeviceInfo
	{
		get
		{
			return Helper.GetAllocation<DeviceInfoInternal>(m_DeviceInfo);
		}
		set
		{
			Helper.RegisterAllocation(ref m_DeviceInfo, value);
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_DeviceInfo);
	}
}
