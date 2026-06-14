using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SessionModificationSetJoinInProgressAllowedOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private int m_AllowJoinInProgress;

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

	public bool AllowJoinInProgress
	{
		get
		{
			return Helper.GetBoolFromInt(m_AllowJoinInProgress);
		}
		set
		{
			m_AllowJoinInProgress = Helper.GetIntFromBool(value);
		}
	}

	public void Dispose()
	{
	}
}
