using System.Collections.Generic;
using UnityEngine;

public class LevelSetup_GuardTowerSpotlight : BaseComponentSetup
{
	private const int m_PathAreaSize = 20;

	private const int m_PathStep = 4;

	private const int m_PathAreaSizeHalf = 10;

	private Vector2 m_IgnoreBaseX = new Vector2(-2f, 2f);

	private Vector2 m_IgnoreBaseY = new Vector2(-2f, 4f);

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_2;
	}

	public override SetupReturnState Setup()
	{
		LevelDetailsManager instance = LevelDetailsManager.GetInstance();
		if (instance == null)
		{
			Debug.LogError("LevelSetup_GuardTowerSpotlight - Failed to find details manager.");
			return FinishedAndRemove();
		}
		BaseLevelManager instance2 = BaseLevelManager.GetInstance();
		if (instance2 == null)
		{
			return FinishedAndRemove();
		}
		GuardTowerSpotlight component = GetComponent<GuardTowerSpotlight>();
		if (component == null)
		{
			Debug.LogError("LevelSetup_GuardTowerSpotlight - Failed to find the spotlight!");
			return FinishedAndRemove();
		}
		PatrolPath component2 = GetComponent<PatrolPath>();
		if (component2 == null)
		{
			Debug.LogError("LevelSetup_GuardTowerSpotlight - Failed to find the patrol path!");
			return FinishedAndRemove();
		}
		Vector3 localPosition = base.transform.localPosition;
		float z = localPosition.z;
		localPosition.z = 0f;
		base.transform.localPosition = localPosition;
		int X = 0;
		int Y = 0;
		BaseLevelManager.LevelLayers layer = BaseLevelManager.LevelLayers.FirstFloor;
		if (!GetLayerAndPosition(ref X, ref Y, ref layer, FloorManager.TileSystem_Type.TileSystem_Ground))
		{
			return FinishedAndRemove();
		}
		localPosition.z = z;
		base.transform.localPosition = localPosition;
		Y = 119 - Y;
		BaseLevelManager.LayerDataCollection layerDataCollection = instance2.m_BuildingLayers[(uint)layer];
		component2.m_vPathNodes = null;
		List<PatrolPath.PathNode> list = new List<PatrolPath.PathNode>();
		for (int i = -10; i < 10; i += 4)
		{
			for (int j = -10; j < 10; j += 4)
			{
				if (!((float)j < m_IgnoreBaseX.x) && !((float)j > m_IgnoreBaseX.y) && !((float)i < m_IgnoreBaseY.x) && !((float)i > m_IgnoreBaseY.y))
				{
					continue;
				}
				int num = X + j;
				int num2 = Y + i;
				if (num > 0 && num < 119 && num2 > 0 && num2 < 119)
				{
					int num3 = 120 * num2 + num;
					BaseLevelManager.TileProperty tileProperty = layerDataCollection.m_TileProperties[num3];
					if ((tileProperty & BaseLevelManager.TileProperty.TileExistsMask) == BaseLevelManager.TileProperty.TileMask)
					{
						PatrolPath.PathNode pathNode = new PatrolPath.PathNode();
						pathNode.m_vNodePos = new Vector3(j, i, 0f);
						pathNode._m_fWaitTimer = 1f;
						pathNode._m_fWaitVariance = 0.5f;
						list.Add(pathNode);
					}
				}
			}
		}
		list.Shuffle();
		component2.m_vPathNodes = list.ToArray();
		list.Clear();
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		return Setup();
	}
}
