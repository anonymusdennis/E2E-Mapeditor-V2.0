using UnityEngine;

public class LevelSetup_CullComponent : BaseComponentSetup
{
	public string m_strGroup = string.Empty;

	public Component m_Component;

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_6;
	}

	public override SetupReturnState Setup()
	{
		if (m_Component != null && !string.IsNullOrEmpty(m_strGroup))
		{
			BuildingBlockManager instance = BuildingBlockManager.GetInstance();
			if (instance != null && instance.GetLimitationTotal(m_strGroup) == 0)
			{
				Object.Destroy(m_Component);
			}
		}
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		return Setup();
	}
}
