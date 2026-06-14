using UnityEngine;

public class LevelSetup_PatrolPathFixer : BaseComponentSetup
{
	[Tooltip("Do you which the Z part of the node position to be adjusted. (default is false")]
	public bool m_AdjustZ;

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_5;
	}

	public override SetupReturnState Setup()
	{
		PatrolPath[] componentsInChildren = GetComponentsInChildren<PatrolPath>(includeInactive: true);
		float num = 60f;
		float min = 0f - num;
		float num2 = 60f;
		float min2 = 0f - num2;
		for (int num3 = componentsInChildren.Length - 1; num3 >= 0; num3--)
		{
			for (int num4 = componentsInChildren[num3].m_vPathNodes.Length - 1; num4 >= 0; num4--)
			{
				Vector3 vNodePos = componentsInChildren[num3].m_vPathNodes[num4].m_vNodePos + base.transform.position;
				if (!m_AdjustZ)
				{
					vNodePos.z = componentsInChildren[num3].m_vPathNodes[num4].m_vNodePos.z;
				}
				vNodePos.x = Mathf.Clamp(vNodePos.x, min, num);
				vNodePos.y = Mathf.Clamp(vNodePos.y, min2, num2);
				componentsInChildren[num3].m_vPathNodes[num4].m_vNodePos = vNodePos;
			}
		}
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		return Setup();
	}
}
