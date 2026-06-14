using UnityEngine;

public class LevelSetup_RemoveUnusedInmates : BaseComponentSetup
{
	public AICharacter_Inmate[] m_Inmates;

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_1;
	}

	public override SetupReturnState Setup()
	{
		int num = BuildingBlockManager.GetInstance().GetLimitationTotal(BuildingBlockManager.DefaultLimitationGroups.InmateCell.ToString()) - 4;
		int num2 = m_Inmates.Length;
		for (int i = 0; i < num2; i++)
		{
			if (m_Inmates[i] != null && i >= num)
			{
				Object.Destroy(m_Inmates[i].gameObject);
				m_Inmates[i] = null;
			}
		}
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		return Setup();
	}
}
