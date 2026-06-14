using UnityEngine;

public class LevelSetup_AudioBank : BaseComponentSetup
{
	public GameObject m_CenterPerksBank;

	public GameObject m_OldWildWestPerksBank;

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_1;
	}

	public override SetupReturnState Setup()
	{
		LevelDetailsManager levelDetailsManager = null;
		if ((levelDetailsManager = LevelDetailsManager.GetInstance()) != null && m_CenterPerksBank != null && m_OldWildWestPerksBank != null)
		{
			LevelScript.PRISON_ENUM musicType = levelDetailsManager.GetMusicType();
			if (musicType == LevelScript.PRISON_ENUM.OldWestFort)
			{
				m_OldWildWestPerksBank.SetActive(value: true);
				Object.Destroy(m_CenterPerksBank);
			}
			else
			{
				m_CenterPerksBank.SetActive(value: true);
				Object.Destroy(m_OldWildWestPerksBank);
			}
		}
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		return Setup();
	}
}
