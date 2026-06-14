using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.UserInfo;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct UserInfoDataInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_UserId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_Country;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_DisplayName;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_PreferredLanguage;

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

	public EpicAccountId UserId
	{
		get
		{
			return Helper.GetHandle<EpicAccountId>(m_UserId);
		}
		set
		{
			m_UserId = Helper.GetInnerHandle(value);
		}
	}

	public string Country
	{
		get
		{
			return m_Country;
		}
		set
		{
			m_Country = value;
		}
	}

	public string DisplayName
	{
		get
		{
			return m_DisplayName;
		}
		set
		{
			m_DisplayName = value;
		}
	}

	public string PreferredLanguage
	{
		get
		{
			return m_PreferredLanguage;
		}
		set
		{
			m_PreferredLanguage = value;
		}
	}

	public void Dispose()
	{
	}
}
