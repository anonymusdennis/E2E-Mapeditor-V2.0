using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

[CreateAssetMenu(fileName = "Climb Functionality", menuName = "Team17/Items/Functionalities/Create Climb Functionality")]
public class ClimbFunctionality : BaseItemFunctionality
{
	public enum EquipAction
	{
		ClimbDown,
		ClimbUp
	}

	private struct ClimbLocation
	{
		public int m_FloorIndex;

		public int m_Row;

		public int m_Column;

		public void Set(int floorIndex, int row, int column)
		{
			m_FloorIndex = floorIndex;
			m_Row = row;
			m_Column = column;
		}
	}

	public EquipAction m_EquipAction;

	private AnimState m_Animation = AnimState.IdleRope;

	private float m_ClimbTime = 2f;

	private float m_ElapsedClimbTime;

	private ClimbLocation m_StartClimbLocation;

	private ClimbLocation m_EndClimbLocation;

	private float m_StartingZOffset;

	public override void Init()
	{
	}

	public override Functionality GetFunctionalityType()
	{
		return Functionality.Climb;
	}

	public override bool RequiresTargetting()
	{
		return true;
	}

	public override bool RequiresPositioning()
	{
		return false;
	}

	public override bool ImmobilisesOwner()
	{
		return true;
	}

	public override bool IsImmediateUse()
	{
		return false;
	}

	public override bool CanUse(bool intendsOnUsingImmediately = false)
	{
		if (base.ParentItem == null)
		{
			return false;
		}
		if (m_Owner != null)
		{
			int targetTileRow = m_Owner.GetTargetTileRow();
			int targetTileColumn = m_Owner.GetTargetTileColumn();
			if (targetTileRow != -1 && targetTileColumn != -1)
			{
				if (m_EquipAction == EquipAction.ClimbDown)
				{
					ClimbableTile.ClimbAction climbAction = ClimbableTile.ClimbAction.Invaid;
					if (FloorManager.GetInstance().CheckClimbableTileExists(m_Owner.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Wall, targetTileRow, targetTileColumn, out climbAction) && climbAction == ClimbableTile.ClimbAction.Invaid)
					{
						return false;
					}
					bool flag = false;
					int row;
					int column;
					if (climbAction != 0)
					{
						flag = climbAction == ClimbableTile.ClimbAction.Down || climbAction == ClimbableTile.ClimbAction.UpAndDown;
					}
					else if ((!FloorManager.GetInstance().CheckTileExists(m_Owner.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Wall, targetTileRow, targetTileColumn) || (m_Owner.m_CurrentLocation != null && m_Owner.m_CurrentLocation.location == RoomBlob.eLocation.RoofArea)) && FloorManager.GetInstance().GetTileGridPoint(m_Owner.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, m_Owner.transform.position, out row, out column))
					{
						Vector3 vector = new Vector3(targetTileColumn - column, row - targetTileRow, 0f);
						vector.Normalize();
						FloorManager.GetInstance().GetTileCentrePosition(m_Owner.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, targetTileRow, targetTileColumn, out var worldPosition);
						worldPosition += vector * 0.5f;
						if (targetTileColumn == column)
						{
							if (IsOnBuildingBoundary(worldPosition + new Vector3(0.5f, 0f, 0f)) && IsOnBuildingBoundary(worldPosition - new Vector3(0.5f, 0f, 0f)))
							{
								flag = true;
							}
						}
						else if (IsOnBuildingBoundary(worldPosition + new Vector3(0f, 0.5f, 0f)) && IsOnBuildingBoundary(worldPosition - new Vector3(0f, 0.5f, 0f)))
						{
							flag = true;
						}
					}
					if (flag && FloorManager.GetInstance().GetTileGridPoint(m_Owner.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, m_Owner.transform.position, out var row2, out var column2))
					{
						int num = targetTileColumn - column2;
						int num2 = targetTileRow - row2;
						int startColumn = targetTileColumn + num;
						int startRow = targetTileRow + num2;
						FloorManager.Floor groundFloor = null;
						int groundRow = -1;
						int groundColumn = -1;
						bool groundIsClear = false;
						int num3 = FloorManager.GetInstance().FindGround(m_Owner.CurrentFloor, startRow, startColumn, out groundFloor, out groundRow, out groundColumn, out groundIsClear);
						if (num3 == 2 && groundIsClear)
						{
							if (m_Owner.GetFacingDirectionEnum() == Directionx4.Up)
							{
								m_StartClimbLocation.Set(groundFloor.m_FloorIndex, groundRow, groundColumn);
							}
							else
							{
								m_StartClimbLocation.Set(m_Owner.CurrentFloor.m_FloorIndex, targetTileRow, targetTileColumn);
							}
							m_EndClimbLocation.Set(groundFloor.m_FloorIndex, groundRow, groundColumn);
							return true;
						}
					}
					return false;
				}
				if (m_EquipAction == EquipAction.ClimbUp)
				{
					FloorManager.Floor floor = FloorManager.GetInstance().UpAFloor(m_Owner.CurrentFloor);
					if (floor == m_Owner.CurrentFloor)
					{
						return false;
					}
					if (floor.IsVent())
					{
						floor = FloorManager.GetInstance().UpAFloor(floor);
						if (!floor.IsPrisonFloorOrRoof())
						{
							return false;
						}
					}
					ClimbableTile.ClimbAction climbAction2 = ClimbableTile.ClimbAction.Invaid;
					if (FloorManager.GetInstance().CheckClimbableTileExists(floor, FloorManager.TileSystem_Type.TileSystem_Wall, targetTileRow, targetTileColumn, out climbAction2) && climbAction2 == ClimbableTile.ClimbAction.Invaid)
					{
						return false;
					}
					bool flag2 = false;
					int row3;
					int column3;
					if (climbAction2 != 0)
					{
						flag2 = climbAction2 == ClimbableTile.ClimbAction.Up || climbAction2 == ClimbableTile.ClimbAction.UpAndDown;
					}
					else if (FloorManager.GetInstance().GetTileGridPoint(m_Owner.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, m_Owner.transform.position, out row3, out column3))
					{
						Vector3 vector2 = new Vector3(targetTileColumn - column3, row3 + -1 - targetTileRow, 0f);
						vector2.Normalize();
						FloorManager.GetInstance().GetTileCentrePosition(floor, FloorManager.TileSystem_Type.TileSystem_Ground, targetTileRow, targetTileColumn, out var worldPosition2);
						Vector3 vector3 = worldPosition2 - vector2 * 0.5f;
						bool flag3 = false;
						if (targetTileColumn == column3)
						{
							if (IsOnBuildingBoundary(vector3 + new Vector3(0.5f, 0f, 0f)) && IsOnBuildingBoundary(vector3 - new Vector3(0.5f, 0f, 0f)))
							{
								flag3 = true;
							}
						}
						else if (IsOnBuildingBoundary(vector3 + new Vector3(0f, 0.5f, 0f)) && IsOnBuildingBoundary(vector3 - new Vector3(0f, 0.5f, 0f)))
						{
							flag3 = true;
						}
						if (flag3)
						{
							RoomBlob roomBlob = RoomManager.GetInstance().LookUpRoom(worldPosition2 + vector2 * 0.5f);
							bool flag4 = roomBlob != null && roomBlob.location == RoomBlob.eLocation.RoofArea;
							bool flag5 = FloorManager.GetInstance().CheckTileExists(floor, FloorManager.TileSystem_Type.TileSystem_Wall, targetTileRow, targetTileColumn);
							bool flag6 = false;
							if (flag5)
							{
								flag6 = FloorManager.GetInstance().CheckTileHasTag(floor, FloorManager.TileSystem_Type.TileSystem_Wall, targetTileRow, targetTileColumn, "PartialTile");
							}
							if ((!flag5 || (flag4 && flag6)) && FloorManager.GetInstance().CheckTileExists(floor, FloorManager.TileSystem_Type.TileSystem_Ground, targetTileRow, targetTileColumn))
							{
								flag2 = true;
							}
						}
					}
					if (flag2 && FloorManager.GetInstance().GetTileGridPoint(m_Owner.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, m_Owner.transform.position, out var row4, out var column4) && !FloorManager.GetInstance().CheckTileExists(floor, FloorManager.TileSystem_Type.TileSystem_Ground, row4 + -1, column4))
					{
						int num4 = targetTileColumn - column4;
						int num5 = targetTileRow - (row4 + -1);
						int column5 = targetTileColumn + num4;
						int row5 = targetTileRow + num5;
						if (FloorManager.GetInstance().CheckTileExists(floor, FloorManager.TileSystem_Type.TileSystem_Ground, row5, column5) && FloorManager.GetInstance().IsFloorClear(floor, row5, column5))
						{
							m_StartClimbLocation.Set(floor.m_FloorIndex, row5, column5);
							m_EndClimbLocation.Set(floor.m_FloorIndex, row5, column5);
							return true;
						}
					}
					return false;
				}
			}
		}
		return false;
	}

	public override bool StartUsing(AnimState useAnimation, float useTime)
	{
		base.StartUsing(useAnimation, useTime);
		if (base.ParentItem != null && m_Owner != null && FloorManager.GetInstance().GetTileCentrePosition(m_StartClimbLocation.m_FloorIndex, FloorManager.TileSystem_Type.TileSystem_Ground, m_StartClimbLocation.m_Row, m_StartClimbLocation.m_Column, out var worldPosition))
		{
			if (m_EquipAction == EquipAction.ClimbUp)
			{
				m_Owner.Teleport(worldPosition);
			}
			else if (m_Owner.GetFacingDirectionEnum() == Directionx4.Up)
			{
				m_Owner.Teleport(worldPosition);
			}
			else
			{
				m_Owner.transform.position = worldPosition;
			}
			m_ClimbTime = useTime;
			m_ElapsedClimbTime = 0f;
			m_StartingZOffset = m_Owner.GetZOffsetForCharacter();
			m_Animation = useAnimation;
			m_Owner.m_bActionRenderersRequired = true;
			m_Owner.m_CharacterAnimator.StartAnimation(m_Animation);
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Climb, m_Owner.gameObject);
			if (OnStartOfUse != null)
			{
				OnStartOfUse();
			}
			return true;
		}
		return false;
	}

	public override bool UpdateUsing()
	{
		m_ElapsedClimbTime += UpdateManager.deltaTime;
		if (m_EquipAction == EquipAction.ClimbUp && m_Owner.GetFacingDirectionEnum() == Directionx4.Down)
		{
			if (m_ElapsedClimbTime < m_ClimbTime * 0.45f)
			{
				m_Owner.SetUseItemZ(1f);
			}
			else
			{
				m_Owner.SetUseItemZ(m_StartingZOffset);
			}
		}
		if (m_ElapsedClimbTime >= m_ClimbTime)
		{
			FinishUsing();
			if (OnEndOfUse != null)
			{
				OnEndOfUse();
			}
			return false;
		}
		return true;
	}

	public override bool CancelUsing()
	{
		if (!base.CancelUsing())
		{
			return false;
		}
		return false;
	}

	public void FinishUsing()
	{
		if (m_Owner != null)
		{
			if (m_EquipAction == EquipAction.ClimbDown && FloorManager.GetInstance().GetTileCentrePosition(m_EndClimbLocation.m_FloorIndex, FloorManager.TileSystem_Type.TileSystem_Ground, m_EndClimbLocation.m_Row, m_EndClimbLocation.m_Column, out var worldPosition))
			{
				m_Owner.Teleport(worldPosition);
			}
			m_Owner.m_CharacterAnimator.StopAnimation(m_Animation);
			m_Owner.m_bActionRenderersRequired = false;
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_Player_Climb, m_Owner.gameObject);
		}
	}

	private bool IsOnBuildingBoundary(Vector3 position)
	{
		RoomBlob roomBlob = RoomManager.GetInstance().LookUpRoom(position);
		return roomBlob != null && roomBlob.location == RoomBlob.eLocation.BuildingBoundary;
	}
}
