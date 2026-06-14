using System;
using UnityEngine;

public class BuildingBlock_Tile : BuildingBlock_TMS
{
	public bool m_BlockingTile;

	public override BuildingBlockType BlockType => BuildingBlockType.Tile;

	public override void MakeVisualRepresentation(int iIndex)
	{
		m_BlockingTile = false;
		base.MakeVisualRepresentation(iIndex);
		if (m_Footprint == null)
		{
			Footprint.BlockTypes blockTypes = Footprint.BlockTypes.Tiles;
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
		if (componentsInChildren.Length > 0 && !flag && m_RealObjects[iIndex].GetComponentsInChildren<VentCover>(includeInactive: true).Length == 0)
		{
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				UnityEngine.Object.DestroyImmediate(componentsInChildren[i]);
			}
		}
		if (m_Processing_TMSIndex == -1 || m_Processing_MatIndex == -1)
		{
			return;
		}
		ThisIsMyRenderer componentInChildren = m_RealObjects[iIndex].GetComponentInChildren<ThisIsMyRenderer>(includeInactive: true);
		if (componentInChildren != null && componentInChildren.m_Renderer != null)
		{
			componentInChildren.m_Renderer.material = m_TMSEntries[m_Processing_TMSIndex].m_Materials[m_Processing_MatIndex];
			return;
		}
		MeshRenderer componentInChildren2 = m_RealObjects[iIndex].GetComponentInChildren<MeshRenderer>(includeInactive: true);
		if (componentInChildren2 != null)
		{
			componentInChildren2.material = m_TMSEntries[m_Processing_TMSIndex].m_Materials[m_Processing_MatIndex];
		}
	}

	protected override void ProcessComponent(GameObject masterGameObject, Component comp, Type compType, ref bool bKeep, ref bool bClear, int iVersionIndex = 0)
	{
		bool flag = false;
		if (compType == typeof(CollisionMarker) || compType.IsSubclassOf(typeof(CollisionMarker)))
		{
			m_BlockingTile = true;
			bClear = true;
			bKeep = true;
			flag = true;
		}
		if (!flag)
		{
			base.ProcessComponent(masterGameObject, comp, compType, ref bKeep, ref bClear, iVersionIndex);
		}
	}
}
