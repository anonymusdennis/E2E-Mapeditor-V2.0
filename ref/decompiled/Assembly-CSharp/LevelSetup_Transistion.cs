using UnityEngine;

public class LevelSetup_Transistion : BaseComponentSetup
{
	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_9;
	}

	public override SetupReturnState Setup()
	{
		int num = 0;
		int num2 = 14400;
		for (int i = 1; i < 6; i++)
		{
			BaseLevelManager.LayerDataCollection data = BaseLevelManager.GetInstance().m_BuildingLayers[i];
			for (int j = 0; j < num2; j++)
			{
				BaseLevelManager.TileProperty tileProperty = data.m_TileProperties[j];
				num = BaseLevelManager.GetRoomNumberFromProperty(ref data, j);
				if (num == 0 || (tileProperty & BaseLevelManager.TileProperty.EntranceMask) != BaseLevelManager.TileProperty.EntranceMask)
				{
					continue;
				}
				BuildingBlock_Object buildingBlock_Object = BuildingBlockManager.GetBlock(data.m_ObjectTileIDs[j]) as BuildingBlock_Object;
				if (!(buildingBlock_Object != null) || (buildingBlock_Object.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.Entrance) != BuildingBlock_Object.SpecialFlagsEnum.Entrance || !(data.m_ObjectTileObjects[j] != null))
				{
					continue;
				}
				GameObject gameObject = data.m_ObjectTileObjects[j];
				TransitionPoint component = gameObject.GetComponent<TransitionPoint>();
				if (!(component != null) || !(component.m_Partner == null))
				{
					continue;
				}
				int iFoundInLayer = -1;
				int num3 = FindExit(i, num, ref iFoundInLayer);
				if (num3 != -1)
				{
					GameObject gameObject2 = (component.m_Partner = BaseLevelManager.GetInstance().m_BuildingLayers[iFoundInLayer].m_ObjectTileObjects[num3]);
					if (component.m_AINodeLink != null)
					{
						component.m_AINodeLink.end = gameObject2.transform;
					}
				}
			}
		}
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		return Setup();
	}

	public static int FindExit(int iLayerIgnore, int roomNumber, ref int iFoundInLayer)
	{
		int num = 0;
		int num2 = 14400;
		for (int i = 1; i < 6; i++)
		{
			if (i == iLayerIgnore)
			{
				continue;
			}
			BaseLevelManager.LayerDataCollection data = BaseLevelManager.GetInstance().m_BuildingLayers[i];
			for (int j = 0; j < num2; j++)
			{
				BaseLevelManager.TileProperty tileProperty = data.m_TileProperties[j];
				num = BaseLevelManager.GetRoomNumberFromProperty(ref data, j);
				if (num == roomNumber && (tileProperty & BaseLevelManager.TileProperty.ExitMask) == BaseLevelManager.TileProperty.ExitMask)
				{
					BuildingBlock_Object buildingBlock_Object = BuildingBlockManager.GetBlock(data.m_ObjectTileIDs[j]) as BuildingBlock_Object;
					if (buildingBlock_Object != null && (buildingBlock_Object.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.Exit) == BuildingBlock_Object.SpecialFlagsEnum.Exit && data.m_ObjectTileObjects[j] != null)
					{
						iFoundInLayer = i;
						return j;
					}
				}
			}
		}
		return -1;
	}
}
