using UnityEngine;

public class LevelSetup_Generator : BaseComponentSetup
{
	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_10_Last;
	}

	public override SetupReturnState Setup()
	{
		Generator component = GetComponent<Generator>();
		if (component != null)
		{
			PrisonPowerManager classInstance = GetClassInstance<PrisonPowerManager>();
			if (RoomManager.GetInstance() != null && BaseLevelManager.GetInstance() != null && classInstance != null)
			{
				int X = 0;
				int Y = 0;
				BaseLevelManager.LevelLayers layer = BaseLevelManager.LevelLayers.TOTAL;
				if (GetLayerAndPosition(ref X, ref Y, ref layer, FloorManager.TileSystem_Type.TileSystem_ObjectPlops))
				{
					RoomBlob roomBlob = RoomManager.GetInstance().LookUpRoom(base.transform.position);
					if (roomBlob != null)
					{
						int complexAllocationFromRoomNumber = BaseLevelManager.GetInstance().GetComplexAllocationFromRoomNumber(roomBlob.m_ID);
						BaseLevelManager.RoomObjectCollectionType<GeneratorInteraction> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<GeneratorInteraction>();
						BaseLevelManager.GetInstance().GetObjectsInRoom(complexAllocationFromRoomNumber, layer, roomObjectCollectionType);
						if (roomObjectCollectionType.m_Contents.Count > 0)
						{
							component.m_Switch = roomObjectCollectionType.m_Contents[0];
							PrisonPowerManager.GeneratorData generatorData = new PrisonPowerManager.GeneratorData();
							generatorData.m_GeneratorColour = Color.HSVToRGB(Random.value, 1f, 1f);
							generatorData.m_Generator = component;
							classInstance.m_Generators.Add(generatorData);
						}
					}
				}
			}
		}
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		LevelEditor_ZoneManager instance = LevelEditor_ZoneManager.GetInstance();
		if (instance == null)
		{
			return FinishedAndRemove();
		}
		Generator component = GetComponent<Generator>();
		if (component != null)
		{
			PrisonPowerManager classInstance = GetClassInstance<PrisonPowerManager>();
			if (RoomManager.GetInstance() != null && BaseLevelManager.GetInstance() != null && classInstance != null)
			{
				int iIndex = 0;
				BaseLevelManager.LevelLayers layer = BaseLevelManager.LevelLayers.TOTAL;
				if (GetLayerAndZoneMapIndex(ref iIndex, ref layer, FloorManager.TileSystem_Type.TileSystem_ObjectPlops))
				{
					LevelEditor_ZoneManager.ZoneMap zoneMap = instance.GetZoneMap(layer);
					if (zoneMap != null)
					{
						int num = zoneMap.m_Map[iIndex];
						if (num != -1)
						{
							LevelEditor_ZoneManager.Zone zone = instance.GetZone(num);
							if (zone != null && zone.IsGameObjectInZone(base.gameObject))
							{
								BaseLevelManager.RoomObjectCollectionType<GeneratorInteraction> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<GeneratorInteraction>();
								BaseLevelManager.GetInstance().GetObjectsInZone(ref zone, roomObjectCollectionType);
								if (roomObjectCollectionType.m_Contents.Count > 0)
								{
									component.m_Switch = roomObjectCollectionType.m_Contents[0];
									PrisonPowerManager.GeneratorData generatorData = new PrisonPowerManager.GeneratorData();
									generatorData.m_GeneratorColour = Color.HSVToRGB(Random.value, 1f, 1f);
									generatorData.m_Generator = component;
									classInstance.m_Generators.Add(generatorData);
								}
							}
						}
					}
				}
			}
		}
		return FinishedAndRemove();
	}
}
