using UnityEngine;

[RequireComponent(typeof(BuildingInstructionManager))]
[RequireComponent(typeof(LevelDetailsManager))]
public class GameLevelEditorManagerV2 : GameLevelEditorManager
{
	public override bool IsEverythingSetUp()
	{
		if (LevelDetailsManager.c_CurrentLevelDataVersionNumber == LevelDetailsManager.LevelEditorDataVersion.V1_InitialRelease)
		{
			return base.IsEverythingSetUp();
		}
		return m_bIsEveryThingSetUp = base.IsEverythingSetUp();
	}

	protected override void PlaceObject(int X, int Y, BuildingBlock_Object obj, bool bMarkAsChanged = true)
	{
		if (LevelDetailsManager.c_CurrentLevelDataVersionNumber == LevelDetailsManager.LevelEditorDataVersion.V1_InitialRelease)
		{
			base.PlaceObject(X, Y, obj, bMarkAsChanged);
			return;
		}
		GameObject realObject = obj.GetRealObject(0);
		if (realObject == null)
		{
			return;
		}
		GameObject gameObject = null;
		gameObject = ((obj.BlockType != BaseBuildingBlock.BuildingBlockType.Object) ? Object.Instantiate(realObject, m_BuildingLayers[(uint)m_CurrentLayer].m_Decorations.transform) : Object.Instantiate(realObject, m_BuildingLayers[(uint)m_CurrentLayer].m_Objects.transform));
		float x = (float)X + m_fPositionOffsetsX[(int)obj.BlockType] + realObject.transform.localPosition.x;
		float y = (float)Y + m_fPositionOffsetsY[(int)obj.BlockType] + realObject.transform.localPosition.y;
		gameObject.transform.localPosition = new Vector3(x, y, 0f);
		if (obj.m_HasPhotonViewCount > 0)
		{
			m_ObjectsWithPhotonViews.Add(gameObject);
		}
		int num = X + obj.m_Footprint.m_iLeft;
		int num2 = Y + obj.m_Footprint.m_iBottom;
		int num3 = Mathf.Clamp(num + obj.m_Footprint.m_iW, 0, 120);
		int num4 = Mathf.Clamp(num2 + obj.m_Footprint.m_iH, 0, 120);
		num = Mathf.Clamp(num, 0, 120);
		num2 = Mathf.Clamp(num2, 0, 120);
		TileProperty tileProperty = ((obj.BlockType != BaseBuildingBlock.BuildingBlockType.Object) ? TileProperty.DecorationMask : TileProperty.ObjectMask);
		if (obj.m_Solid)
		{
			tileProperty |= TileProperty.BlockingMask;
			if (obj.m_BlockingDirection == BuildingBlock_Object.BlockingDirection.BlocksAll)
			{
				tileProperty |= TileProperty.Blocked_All_Mask;
			}
			else if (obj.m_BlockingDirection == BuildingBlock_Object.BlockingDirection.BlocksHorizontal)
			{
				tileProperty |= TileProperty.Blocked_Horizontal_Bits;
			}
			else if (obj.m_BlockingDirection == BuildingBlock_Object.BlockingDirection.BlocksVerticle)
			{
				tileProperty |= TileProperty.Blocked_Vertical_Mask;
			}
		}
		if ((obj.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.Entrance) == BuildingBlock_Object.SpecialFlagsEnum.Entrance)
		{
			tileProperty |= TileProperty.EntranceMask;
		}
		if ((obj.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.Exit) == BuildingBlock_Object.SpecialFlagsEnum.Exit)
		{
			tileProperty |= TileProperty.ExitMask;
		}
		if (obj.m_ItsADoor)
		{
			tileProperty |= TileProperty.ItsADoorMask;
		}
		TileIDData tileIDData = TileIDData.EMPTY;
		tileIDData = ((obj.m_ID != -1) ? ((TileIDData)(obj.m_ID & 0x3FFF)) : TileIDData.IDMask);
		if (m_CurrentComplexAllocation != 0)
		{
			tileIDData |= TileIDData.ComplexMask;
		}
		m_BuildingBlockManager.AdjustLimitationTotal(tileIDData, bAdd: true);
		int num5 = num2 * 120 + num;
		int num6 = 120 - obj.m_Footprint.m_iW;
		int num7 = 0;
		for (int i = num2; i < num4; i++)
		{
			for (int j = num; j < num3; j++)
			{
				if ((obj.m_Footprint.m_UsedTiles[num7++] & Footprint.BlockTypes.Objects) == Footprint.BlockTypes.Objects)
				{
					m_BuildingLayers[(uint)m_CurrentLayer].m_ObjectTileIDs[num5] = tileIDData;
					TileProperty[] tileProperties;
					int num8;
					(tileProperties = m_BuildingLayers[(uint)m_CurrentLayer].m_TileProperties)[num8 = num5] = tileProperties[num8] | tileProperty;
					if (m_CurrentComplexAllocation != 0)
					{
						BaseLevelManager.AddRoomNumberToProperty(ref m_BuildingLayers[(uint)m_CurrentLayer], num5, m_CurrentComplexAllocation);
					}
					GameObject gameObject2 = m_BuildingLayers[(uint)m_CurrentLayer].m_ObjectTileObjects[num5];
					if (gameObject2 != null)
					{
						Object.Destroy(gameObject2);
					}
					m_BuildingLayers[(uint)m_CurrentLayer].m_ObjectTileObjects[num5] = gameObject;
				}
				num5++;
			}
			num5 += num6;
		}
		if (bMarkAsChanged)
		{
			SetAreaAsChanged(num - 1, num2 - 1, num3 - num + 2, num4 - num2 + 2);
		}
		gameObject.SetActive(value: true);
	}
}
