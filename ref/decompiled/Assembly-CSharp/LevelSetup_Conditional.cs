using System.Collections.Generic;
using UnityEngine;

public class LevelSetup_Conditional : BaseComponentSetup
{
	public enum Contition
	{
		MustBeAtLeastOne,
		CantBeAnyOfThem
	}

	public enum Action
	{
		Remove,
		Deactivate,
		Activate
	}

	public GameObject m_Target;

	public int m_XOffset;

	public int m_YOffset;

	public int m_Width = 1;

	public int m_Height = 1;

	public Contition m_Condition;

	public Action m_Action;

	[HideInInspector]
	public List<int> m_Blocks = new List<int>();

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_10_Last;
	}

	public override SetupReturnState Setup()
	{
		if (m_Target == null)
		{
			return FinishedAndRemove();
		}
		if (m_Width < 1 || m_Height < 1)
		{
			return FinishedAndRemove();
		}
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance == null)
		{
			return FinishedAndRemove();
		}
		int X = 0;
		int Y = 0;
		BaseLevelManager.LevelLayers layer = BaseLevelManager.LevelLayers.GroundFloor;
		if (!GetLayerAndPosition(ref X, ref Y, ref layer, FloorManager.TileSystem_Type.TileSystem_Ground))
		{
			return FinishedAndRemove();
		}
		Y = 119 - Y;
		int num = Mathf.Clamp(X + m_XOffset, 0, 119);
		int num2 = Mathf.Clamp(X + m_XOffset + m_Width, 0, 119);
		int i = Mathf.Clamp(Y + m_YOffset, 0, 119);
		int num3 = Mathf.Clamp(Y + m_YOffset + m_Height, 0, 119);
		if (num == num2 || i == num3)
		{
			return FinishedAndRemove();
		}
		BaseLevelManager.LayerDataCollection layerDataCollection = instance.m_BuildingLayers[(uint)layer];
		int num4 = i * 120 + num;
		int num5 = 120 - (num2 - num);
		for (; i < num3; i++)
		{
			for (int j = num; j < num2; j++)
			{
				bool flag = false;
				BaseLevelManager.TileProperty tileProperty = layerDataCollection.m_TileProperties[num4];
				if ((tileProperty & BaseLevelManager.TileProperty.TileMask) == BaseLevelManager.TileProperty.TileMask)
				{
					flag = AnyMatch((int)(layerDataCollection.m_TileTileIDs[num4] & BaseLevelManager.TileIDData.IDMask));
				}
				if (!flag && (tileProperty & BaseLevelManager.TileProperty.WallMask) == BaseLevelManager.TileProperty.WallMask)
				{
					flag |= AnyMatch((int)(layerDataCollection.m_WallTileIDs[num4] & BaseLevelManager.TileIDData.IDMask));
				}
				if (!flag && (tileProperty & BaseLevelManager.TileProperty.ObjDecMask) != 0)
				{
					flag |= AnyMatch((int)(layerDataCollection.m_ObjectTileIDs[num4] & BaseLevelManager.TileIDData.IDMask));
				}
				if (flag && m_Condition == Contition.CantBeAnyOfThem)
				{
					return FinishedAndRemove();
				}
				if (!flag && m_Condition == Contition.MustBeAtLeastOne)
				{
					return FinishedAndRemove();
				}
				num4++;
			}
			num4 += num5;
		}
		switch (m_Action)
		{
		case Action.Activate:
			m_Target.SetActive(value: true);
			break;
		case Action.Deactivate:
			m_Target.SetActive(value: false);
			break;
		case Action.Remove:
			Object.Destroy(m_Target);
			break;
		}
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		return Setup();
	}

	private bool AnyMatch(int iBlock)
	{
		for (int num = m_Blocks.Count - 1; num >= 0; num--)
		{
			if (m_Blocks[num] != -1 && m_Blocks[num] == iBlock)
			{
				return true;
			}
		}
		return false;
	}
}
