using System;

public class SaveDataRegister : IDisposable
{
	public enum SAVE_DATA_ID
	{
		kPlayerDataManager = 19090,
		kMapItemTracker,
		kCullingBuckets
	}

	private bool m_bActive;

	private string m_strSerializedSnatshop = string.Empty;

	private int m_ID = -1;

	private int m_SecondaryId = -1;

	public SaveDataRegister(Saveable Saving, int iID, bool bIsMajorManagerComponent, int secondaryId = -1)
	{
		new SaveData(Saving, iID, out m_strSerializedSnatshop, bIsMajorManagerComponent, secondaryId);
		m_ID = iID;
		m_SecondaryId = secondaryId;
		m_bActive = true;
	}

	~SaveDataRegister()
	{
		CleanUp();
	}

	public void Dispose()
	{
		if (m_bActive)
		{
			m_bActive = false;
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			CleanUp();
		}
	}

	public string GetSaveData()
	{
		return m_strSerializedSnatshop;
	}

	private void CleanUp()
	{
		if (m_ID != -1)
		{
			PrisonSnapshotIO.DeRegisterSaveData(m_ID, m_SecondaryId);
			m_ID = -1;
			m_SecondaryId = -1;
			m_strSerializedSnatshop = null;
		}
	}
}
