using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CatalogReleaseInternal : IDisposable
{
	private int m_ApiVersion;

	private uint m_CompatibleAppIdCount;

	private IntPtr m_CompatibleAppIds;

	private uint m_CompatiblePlatformCount;

	private IntPtr m_CompatiblePlatforms;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_ReleaseNote;

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

	public string[] CompatibleAppIds
	{
		get
		{
			return Helper.GetAllocation<string[]>(m_CompatibleAppIds, (int)m_CompatibleAppIdCount);
		}
		set
		{
			Helper.RegisterArrayAllocation(ref m_CompatibleAppIds, value, out m_CompatibleAppIdCount);
		}
	}

	public string[] CompatiblePlatforms
	{
		get
		{
			return Helper.GetAllocation<string[]>(m_CompatiblePlatforms, (int)m_CompatiblePlatformCount);
		}
		set
		{
			Helper.RegisterArrayAllocation(ref m_CompatiblePlatforms, value, out m_CompatiblePlatformCount);
		}
	}

	public string ReleaseNote
	{
		get
		{
			return m_ReleaseNote;
		}
		set
		{
			m_ReleaseNote = value;
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_CompatibleAppIds);
		Helper.ReleaseAllocation(ref m_CompatiblePlatforms);
	}
}
