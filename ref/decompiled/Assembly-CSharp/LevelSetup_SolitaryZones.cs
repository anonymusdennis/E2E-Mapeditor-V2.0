using Rotorz.Tile;
using UnityEngine;

public class LevelSetup_SolitaryZones : BaseComponentSetup
{
	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_9;
	}

	public override SetupReturnState Setup()
	{
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		LevelEditor_ZoneManager instance = LevelEditor_ZoneManager.GetInstance();
		BaseLevelManager instance2 = BaseLevelManager.GetInstance();
		if (instance == null || instance2 == null)
		{
			if ((bool)instance)
			{
			}
			if (!instance2)
			{
			}
		}
		else
		{
			int totalZones = instance.GetTotalZones();
			BaseLevelManager.LayerDataCollection[] buildingLayers = instance2.m_BuildingLayers;
			for (int i = 0; i < totalZones; i++)
			{
				LevelEditor_ZoneManager.Zone zone = instance.GetZone(i);
				if (zone == null || zone.m_ZoneType != ZoneDetailsManager.ZoneTypes.Solitary)
				{
					continue;
				}
				int num = zone.m_Bottom * 120 + zone.m_Left;
				int num2 = 0;
				int num3 = 0;
				int num4 = 120 - zone.m_Width;
				for (int j = 0; j < zone.m_Height; j++)
				{
					for (int k = 0; k < zone.m_Width; k++)
					{
						byte b = (byte)(1 << num3);
						if ((zone.m_ZonePrint[num2] & b) != 0)
						{
							int num5 = k + zone.m_Left;
							int num6 = 119 - (j + zone.m_Bottom);
							int num7 = num6 + 1;
							if (zone.m_Layer == BaseLevelManager.LevelLayers.GroundFloor && num7 < 120)
							{
								TileData tile = buildingLayers[0].m_Tiles_TileSystem.GetTile(num7, num5);
								TileData tile2 = buildingLayers[0].m_Walls_TileSystem.GetTile(num7, num5);
								if (tile != null && tile.gameObject != null)
								{
									DamagableTile component = tile.gameObject.GetComponent<DamagableTile>();
									if (component != null)
									{
										Object.Destroy(component);
									}
									if (tile.gameObject.GetComponent<IndestructibleTile>() == null)
									{
										tile.gameObject.AddComponent<IndestructibleTile>();
									}
								}
								if (tile2 != null && tile2.gameObject != null)
								{
									DamagableTile component2 = tile2.gameObject.GetComponent<DamagableTile>();
									if (component2 != null)
									{
										Object.Destroy(component2);
									}
									if (tile2.gameObject.GetComponent<IndestructibleTile>() == null)
									{
										tile2.gameObject.AddComponent<IndestructibleTile>();
									}
								}
							}
							GameObject gameObject = buildingLayers[(uint)zone.m_Layer].m_TileTileObjects[num];
							if (gameObject != null)
							{
								DamagableTile component3 = gameObject.GetComponent<DamagableTile>();
								if (component3 != null)
								{
									Object.Destroy(component3);
								}
								if (gameObject.GetComponent<IndestructibleTile>() == null)
								{
									gameObject.AddComponent<IndestructibleTile>();
								}
							}
							GameObject gameObject2 = null;
							int num8 = 0;
							int num9 = 0;
							int num10 = num;
							for (int l = 0; l < 8; l++)
							{
								num10 = num;
								switch (l)
								{
								case 0:
									num10 -= 120;
									num9 = num6 + 1;
									num8 = num5;
									break;
								case 1:
									num10 -= 119;
									num9 = num6 + 1;
									num8 = num5 + 1;
									break;
								case 2:
									num10 -= 121;
									num9 = num6 + 1;
									num8 = num5 - 1;
									break;
								case 3:
									num10 += 120;
									num9 = num6 - 1;
									num8 = num5;
									break;
								case 4:
									num10 += 121;
									num9 = num6 - 1;
									num8 = num5 + 1;
									break;
								case 5:
									num10 += 119;
									num9 = num6 - 1;
									num8 = num5 - 1;
									break;
								case 6:
									num10++;
									num9 = num6;
									num8 = num5 + 1;
									break;
								case 7:
									num10--;
									num9 = num6;
									num8 = num5 - 1;
									break;
								}
								if (num9 < 0 || num9 >= 120 || num8 < 0 || num8 >= 120)
								{
									continue;
								}
								gameObject2 = buildingLayers[(uint)zone.m_Layer].m_WallTileObjects[num10];
								if (gameObject2 != null)
								{
									DamagableTile component4 = gameObject2.GetComponent<DamagableTile>();
									if (component4 != null)
									{
										Object.Destroy(component4);
									}
									if (gameObject2.GetComponent<IndestructibleTile>() == null)
									{
										gameObject2.AddComponent<IndestructibleTile>();
									}
								}
							}
						}
						if (num3 == 7)
						{
							num3 = 0;
							num2++;
						}
						else
						{
							num3++;
						}
						num++;
					}
					num += num4;
				}
			}
		}
		return FinishedAndRemove();
	}
}
