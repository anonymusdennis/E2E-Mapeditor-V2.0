using UnityEngine;

public class BuildingBlock_Wall : BuildingBlock_TMS
{
	public int m_FloorTileID = -1;

	public override BuildingBlockType BlockType => BuildingBlockType.Wall;

	public override void MakeVisualRepresentation(int iIndex)
	{
		base.MakeVisualRepresentation(iIndex);
		if (m_Footprint == null)
		{
			Footprint.BlockTypes blockTypes = Footprint.BlockTypes.Walls | Footprint.BlockTypes.Blocking;
			if (m_NoBlockingBelow)
			{
				blockTypes |= Footprint.BlockTypes.NoBlockingBelow;
			}
			Footprint footPrint = new Footprint(0, 0, 1, 1, blockTypes);
			AddToFootprint(footPrint);
		}
	}

	public override void MakeActualObject(int iIndex)
	{
		base.MakeActualObject(iIndex);
		DamagableTile[] componentsInChildren = m_RealObjects[iIndex].GetComponentsInChildren<DamagableTile>(includeInactive: true);
		if (componentsInChildren.Length > 1)
		{
		}
		bool flag = true;
		if (m_Processing_TMSIndex == -1 || !m_TMSEntries[m_Processing_TMSIndex].m_Damagable)
		{
			flag = false;
		}
		if (!flag)
		{
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				TrackableUIElementsReporter component = componentsInChildren[i].GetComponent<TrackableUIElementsReporter>();
				Object.DestroyImmediate(componentsInChildren[i]);
				if (component != null)
				{
					Object.DestroyImmediate(component);
				}
			}
		}
		else
		{
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].m_Materials = new Material[m_TMSEntries[m_Processing_TMSIndex].m_DamagedMaterial.Length + 1];
				componentsInChildren[j].m_Materials[m_TMSEntries[m_Processing_TMSIndex].m_DamagedMaterial.Length] = m_TMSEntries[m_Processing_TMSIndex].m_Materials[m_Processing_MatIndex];
				int num = 0;
				for (int num2 = m_TMSEntries[m_Processing_TMSIndex].m_DamagedMaterial.Length - 1; num2 >= 0; num2--)
				{
					componentsInChildren[j].m_Materials[num2] = m_TMSEntries[m_Processing_TMSIndex].m_DamagedMaterial[num++];
				}
			}
			m_RealObjects[iIndex].AddComponent<LevelSetup_DamageableWall>();
		}
		if (m_TMSEntries[m_Processing_TMSIndex].m_FacadeMaterial != null)
		{
			LevelSetup_Facades levelSetup_Facades = m_RealObjects[iIndex].AddComponent<LevelSetup_Facades>();
			if (levelSetup_Facades != null)
			{
				levelSetup_Facades.m_FacadeMaterial = m_TMSEntries[m_Processing_TMSIndex].m_FacadeMaterial;
				levelSetup_Facades.m_BlockID = m_ID;
			}
		}
		if (m_Processing_TMSIndex != -1 && m_Processing_MatIndex != -1)
		{
			ThisIsMyRenderer componentInChildren = m_RealObjects[iIndex].GetComponentInChildren<ThisIsMyRenderer>(includeInactive: true);
			if (componentInChildren != null && componentInChildren.m_Renderer != null)
			{
				componentInChildren.m_Renderer.material = m_TMSEntries[m_Processing_TMSIndex].m_Materials[m_Processing_MatIndex];
			}
			else
			{
				Renderer componentInChildren2 = m_RealObjects[iIndex].GetComponentInChildren<Renderer>(includeInactive: true);
				if (componentInChildren2 != null)
				{
					componentInChildren2.material = m_TMSEntries[m_Processing_TMSIndex].m_Materials[m_Processing_MatIndex];
				}
			}
		}
		int num3 = m_RealObjects[iIndex].GetComponentsInChildren<PhotonView>().Length;
		if (num3 != 0)
		{
			m_HasPhotonViewCount = num3;
		}
		BuildingBlockHelper.AddLayerShift(this, m_RealObjects[iIndex]);
	}

	public override CompletionState GetBlockCompletionState(ref string strProblems, bool bCreateErrorString = false)
	{
		return base.GetBlockCompletionState(ref strProblems, bCreateErrorString);
	}
}
