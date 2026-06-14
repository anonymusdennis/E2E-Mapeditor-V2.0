using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Platform;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct OptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_Reserved;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_ProductId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_SandboxId;

	private ClientCredentialsInternal m_ClientCredentials;

	private int m_IsServer;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_EncryptionKey;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_OverrideCountryCode;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_OverrideLocaleCode;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_DeploymentId;

	private PlatformFlags m_Flags;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_CacheDirectory;

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

	public IntPtr Reserved
	{
		get
		{
			return m_Reserved;
		}
		set
		{
			m_Reserved = value;
		}
	}

	public string ProductId
	{
		get
		{
			return m_ProductId;
		}
		set
		{
			m_ProductId = value;
		}
	}

	public string SandboxId
	{
		get
		{
			return m_SandboxId;
		}
		set
		{
			m_SandboxId = value;
		}
	}

	public ClientCredentialsInternal ClientCredentials
	{
		get
		{
			return m_ClientCredentials;
		}
		set
		{
			m_ClientCredentials = value;
		}
	}

	public bool IsServer
	{
		get
		{
			return Helper.GetBoolFromInt(m_IsServer);
		}
		set
		{
			m_IsServer = Helper.GetIntFromBool(value);
		}
	}

	public string EncryptionKey
	{
		get
		{
			return m_EncryptionKey;
		}
		set
		{
			m_EncryptionKey = value;
		}
	}

	public string OverrideCountryCode
	{
		get
		{
			return m_OverrideCountryCode;
		}
		set
		{
			m_OverrideCountryCode = value;
		}
	}

	public string OverrideLocaleCode
	{
		get
		{
			return m_OverrideLocaleCode;
		}
		set
		{
			m_OverrideLocaleCode = value;
		}
	}

	public string DeploymentId
	{
		get
		{
			return m_DeploymentId;
		}
		set
		{
			m_DeploymentId = value;
		}
	}

	public PlatformFlags Flags
	{
		get
		{
			return m_Flags;
		}
		set
		{
			m_Flags = value;
		}
	}

	public string CacheDirectory
	{
		get
		{
			return m_CacheDirectory;
		}
		set
		{
			m_CacheDirectory = value;
		}
	}

	public void Dispose()
	{
	}
}
