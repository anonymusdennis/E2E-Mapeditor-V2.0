public class SaveData
{
	private Saveable m_SavingFor;

	private int m_PrimaryID = -1;

	private int m_SecondaryID = -1;

	public SaveData(Saveable Saving, int iID, out string strSerializedSnatshop, bool bIsMajorManagerComponent, int secondaryId = -1)
	{
		m_PrimaryID = iID;
		m_SecondaryID = secondaryId;
		m_SavingFor = Saving;
		strSerializedSnatshop = PrisonSnapshotIO.RegisterSaveData(this, iID, bIsMajorManagerComponent, secondaryId);
	}

	public int GetPrimaryID()
	{
		return m_PrimaryID;
	}

	public int GetSecondaryID()
	{
		return m_SecondaryID;
	}

	public Saveable GetSaveable()
	{
		return m_SavingFor;
	}
}
