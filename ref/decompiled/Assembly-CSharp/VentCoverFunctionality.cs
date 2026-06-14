using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

[CreateAssetMenu(fileName = "Vent Cover Functionality", menuName = "Team17/Items/Functionalities/Create Vent Cover Functionality")]
public class VentCoverFunctionality : RepairFunctionality
{
	public VentCover.CoverType m_CanCoverType;

	protected override bool FindTile(out FloorManager.Floor floor, out FloorManager.TileSystem_Type systemType, out int row, out int column)
	{
		if (m_Owner != null)
		{
			int targetTileRow = m_Owner.GetTargetTileRow();
			int targetTileColumn = m_Owner.GetTargetTileColumn();
			FloorManager.Floor currentFloor = m_Owner.CurrentFloor;
			if (targetTileRow != -1 && targetTileColumn != -1 && !currentFloor.IsUnderGround())
			{
				VentCover ventCover = null;
				ventCover = FloorManager.GetInstance().GetVentCover(targetTileRow, targetTileColumn, currentFloor.m_FloorIndex);
				if (ventCover != null && ventCover.m_CoverType == m_CanCoverType && !ventCover.IsTileHoldingItem() && ventCover.GetTileHealth() <= m_fMinRequiredHealth)
				{
					floor = currentFloor;
					row = targetTileRow;
					column = targetTileColumn;
					systemType = FloorManager.TileSystem_Type.TileSystem_Ground;
					return true;
				}
				FloorManager.Floor floor2 = FloorManager.GetInstance().UpAFloor(currentFloor);
				if (floor2 != currentFloor)
				{
					if (currentFloor.IsVent())
					{
						ventCover = FloorManager.GetInstance().GetVentCover(targetTileRow + -1, targetTileColumn, floor2.m_FloorIndex);
						if (ventCover != null && ventCover.m_CoverType == m_CanCoverType && !ventCover.IsTileHoldingItem() && ventCover.GetTileHealth() <= m_fMinRequiredHealth)
						{
							floor = floor2;
							row = targetTileRow + -1;
							column = targetTileColumn;
							systemType = FloorManager.TileSystem_Type.TileSystem_Ground;
							return true;
						}
					}
					else if (m_Owner.m_bIsStandingOnDesk || !m_Owner.m_CharacterStats.m_bIsPlayer)
					{
						ventCover = FloorManager.GetInstance().GetVentCover(targetTileRow, targetTileColumn, floor2.m_FloorIndex);
						if (ventCover != null && ventCover.m_CoverType == m_CanCoverType && !ventCover.IsTileHoldingItem() && ventCover.GetTileHealth() <= m_fMinRequiredHealth)
						{
							floor = floor2;
							row = targetTileRow;
							column = targetTileColumn;
							systemType = FloorManager.TileSystem_Type.TileSystem_Ground;
							return true;
						}
					}
				}
			}
		}
		floor = null;
		row = -1;
		column = -1;
		systemType = FloorManager.TileSystem_Type.TileSystem_Ground;
		return false;
	}

	protected override Events GetRepairSoundEvent()
	{
		return Events.Play_Player_Vent_Cover;
	}
}
