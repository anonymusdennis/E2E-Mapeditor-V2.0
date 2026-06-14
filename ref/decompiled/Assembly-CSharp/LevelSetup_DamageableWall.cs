using UnityEngine;

public class LevelSetup_DamageableWall : BaseComponentSetup
{
	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_10_Last;
	}

	public override SetupReturnState Setup()
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance == null)
		{
			return FinishedAndRemove();
		}
		if (!GetFloorManager())
		{
			return FinishedAndRemove();
		}
		if (BaseComponentSetup.m_FloorManager == null)
		{
			return FinishedAndRemove();
		}
		int X = 0;
		int Y = 0;
		BaseLevelManager.LevelLayers layer = BaseLevelManager.LevelLayers.FirstFloor;
		if (!GetLayerAndPosition(ref X, ref Y, ref layer))
		{
			return FinishedAndRemove();
		}
		Y = 119 - Y;
		int num = 120 * Y + X;
		bool flag = false;
		bool[] array = new bool[4];
		BaseLevelManager.LayerDataCollection layerDataCollection = instance.m_BuildingLayers[(uint)layer];
		BaseLevelManager.TileProperty tileProperty = layerDataCollection.m_TileProperties[num];
		if ((tileProperty & BaseLevelManager.TileProperty.ObjectMask) == BaseLevelManager.TileProperty.ObjectMask)
		{
			flag = true;
		}
		else
		{
			if (X > 0)
			{
				int num2 = num - 1;
				BaseLevelManager.TileProperty tileProperty2 = layerDataCollection.m_TileProperties[num2];
				if ((tileProperty2 & BaseLevelManager.TileProperty.WallMask) == BaseLevelManager.TileProperty.WallMask)
				{
					array[3] = true;
				}
			}
			if (X < 119)
			{
				int num3 = num + 1;
				BaseLevelManager.TileProperty tileProperty3 = layerDataCollection.m_TileProperties[num3];
				if ((tileProperty3 & BaseLevelManager.TileProperty.WallMask) == BaseLevelManager.TileProperty.WallMask)
				{
					array[1] = true;
				}
			}
			if (array[1] || array[3])
			{
				BaseLevelManager.TileProperty tileProperty4 = BaseLevelManager.TileProperty.EMPTY;
				if (Y > 0)
				{
					tileProperty4 |= layerDataCollection.m_TileProperties[num - 120];
				}
				if (Y < 119)
				{
					tileProperty4 |= layerDataCollection.m_TileProperties[num + 120];
				}
				if ((tileProperty4 & BaseLevelManager.TileProperty.ObjectMask) == BaseLevelManager.TileProperty.ObjectMask)
				{
					flag = true;
				}
			}
			else
			{
				if (Y > 0)
				{
					int num4 = num - 120;
					BaseLevelManager.TileProperty tileProperty5 = layerDataCollection.m_TileProperties[num4];
					if ((tileProperty5 & BaseLevelManager.TileProperty.WallMask) == BaseLevelManager.TileProperty.WallMask)
					{
						array[2] = true;
					}
				}
				if (Y < 119)
				{
					int num5 = num + 120;
					BaseLevelManager.TileProperty tileProperty6 = layerDataCollection.m_TileProperties[num5];
					if ((tileProperty6 & BaseLevelManager.TileProperty.WallMask) == BaseLevelManager.TileProperty.WallMask)
					{
						array[0] = true;
					}
				}
				if (array[0] || array[2])
				{
					BaseLevelManager.TileProperty tileProperty7 = BaseLevelManager.TileProperty.EMPTY;
					if (X > 0)
					{
						tileProperty7 |= layerDataCollection.m_TileProperties[num - 1];
					}
					if (X < 119)
					{
						tileProperty7 |= layerDataCollection.m_TileProperties[num + 1];
					}
					if ((tileProperty7 & BaseLevelManager.TileProperty.ObjectMask) == BaseLevelManager.TileProperty.ObjectMask)
					{
						flag = true;
					}
				}
			}
		}
		if (flag)
		{
			DamagableTile component = base.gameObject.GetComponent<DamagableTile>();
			if (component != null)
			{
				if (Application.isPlaying)
				{
					Object.Destroy(component);
				}
				else
				{
					Object.DestroyImmediate(component);
				}
				TrackableUIElementsReporter component2 = base.gameObject.GetComponent<TrackableUIElementsReporter>();
				if (component2 != null)
				{
					if (Application.isPlaying)
					{
						Object.Destroy(component2);
					}
					else
					{
						Object.DestroyImmediate(component2);
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
}
