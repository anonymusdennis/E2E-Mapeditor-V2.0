using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct DefinitionInternal : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_AchievementId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_DisplayName;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_Description;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_LockedDisplayName;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_LockedDescription;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_HiddenDescription;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_CompletionDescription;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_UnlockedIconId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_LockedIconId;

	private int m_IsHidden;

	private int m_StatThresholdsCount;

	private IntPtr m_StatThresholds;

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

	public string AchievementId
	{
		get
		{
			return m_AchievementId;
		}
		set
		{
			m_AchievementId = value;
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

	public string Description
	{
		get
		{
			return m_Description;
		}
		set
		{
			m_Description = value;
		}
	}

	public string LockedDisplayName
	{
		get
		{
			return m_LockedDisplayName;
		}
		set
		{
			m_LockedDisplayName = value;
		}
	}

	public string LockedDescription
	{
		get
		{
			return m_LockedDescription;
		}
		set
		{
			m_LockedDescription = value;
		}
	}

	public string HiddenDescription
	{
		get
		{
			return m_HiddenDescription;
		}
		set
		{
			m_HiddenDescription = value;
		}
	}

	public string CompletionDescription
	{
		get
		{
			return m_CompletionDescription;
		}
		set
		{
			m_CompletionDescription = value;
		}
	}

	public string UnlockedIconId
	{
		get
		{
			return m_UnlockedIconId;
		}
		set
		{
			m_UnlockedIconId = value;
		}
	}

	public string LockedIconId
	{
		get
		{
			return m_LockedIconId;
		}
		set
		{
			m_LockedIconId = value;
		}
	}

	public bool IsHidden
	{
		get
		{
			return Helper.GetBoolFromInt(m_IsHidden);
		}
		set
		{
			m_IsHidden = Helper.GetIntFromBool(value);
		}
	}

	public StatThresholdsInternal[] StatThresholds
	{
		get
		{
			return Helper.GetAllocation<StatThresholdsInternal[]>(m_StatThresholds, m_StatThresholdsCount);
		}
		set
		{
			Helper.RegisterArrayAllocation(ref m_StatThresholds, value, out m_StatThresholdsCount);
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_StatThresholds);
	}
}
