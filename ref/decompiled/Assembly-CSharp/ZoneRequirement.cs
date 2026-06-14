using System;

[Serializable]
public class ZoneRequirement
{
	public enum WhoForEnum
	{
		User,
		Game,
		Both
	}

	public bool m_bValid;

	public string m_OurName = string.Empty;

	public string m_NameTextResource = string.Empty;

	public string m_TooManyError = string.Empty;

	public string m_TooFewError = string.Empty;

	public string m_BlockGroup = string.Empty;

	public WhoForEnum m_WhoFor = WhoForEnum.Both;

	private int m_BlockSetIndex = -1;

	public LimitValue m_Minimum;

	public LimitValue m_Maximum;

	public int GetBlockSetIndex()
	{
		if (m_BlockSetIndex == -1)
		{
			m_BlockSetIndex = BuildingBlockGroupManager.GetInstance().GetGroupIndexByName(m_BlockGroup);
		}
		return m_BlockSetIndex;
	}
}
