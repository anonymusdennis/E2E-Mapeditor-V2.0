using UnityEngine;

public class OutfitTypeChanger : BaseVisualObjectKeeper
{
	public GameObject m_CenterPercs;

	public GameObject m_OldWildFort;

	private LevelScript.PRISON_ENUM m_CurrentPrison = LevelScript.PRISON_ENUM.AITest;

	private LevelDetailsManager m_LevelDetailsManager;

	public void Update()
	{
		if ((m_LevelDetailsManager != null || (m_LevelDetailsManager = LevelDetailsManager.GetInstance()) != null) && m_CurrentPrison != m_LevelDetailsManager.GetOutfitType())
		{
			UpdateLook(bIgnoreCheck: true);
		}
	}

	private void UpdateLook(bool bIgnoreCheck)
	{
		if (bIgnoreCheck || m_LevelDetailsManager != null || (m_LevelDetailsManager = LevelDetailsManager.GetInstance()) != null)
		{
			m_CurrentPrison = m_LevelDetailsManager.GetOutfitType();
			bool flag = m_CurrentPrison == LevelScript.PRISON_ENUM.Centre_Perks;
			m_CenterPercs.SetActive(flag);
			m_OldWildFort.SetActive(!flag);
		}
	}
}
