using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct PresenceModificationDeleteDataOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private int m_RecordsCount;

	private IntPtr m_Records;

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

	public PresenceModificationDataRecordIdInternal[] Records
	{
		get
		{
			return Helper.GetAllocation<PresenceModificationDataRecordIdInternal[]>(m_Records, m_RecordsCount);
		}
		set
		{
			Helper.RegisterArrayAllocation(ref m_Records, value, out m_RecordsCount);
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_Records);
	}
}
