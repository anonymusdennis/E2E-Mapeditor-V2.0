using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

[CreateAssetMenu(fileName = "Repair Fence Functionality", menuName = "Team17/Items/Functionalities/Create Repair Fence Functionality")]
public class RepairFenceFunctionality : RepairFunctionality
{
	protected override bool FindTile(out FloorManager.Floor floor, out FloorManager.TileSystem_Type systemType, out int row, out int column)
	{
		if (m_Owner != null)
		{
			int targetTileRow = m_Owner.GetTargetTileRow();
			int targetTileColumn = m_Owner.GetTargetTileColumn();
			FloorManager.Floor currentFloor = m_Owner.CurrentFloor;
			FloorManager.TileSystem_Type tileSystem_Type = FloorManager.TileSystem_Type.TileSystem_Wall;
			if (targetTileRow != -1 && targetTileColumn != -1 && !currentFloor.IsUnderGround() && !currentFloor.IsVent())
			{
				DamagableTile damagableTile = FloorManager.GetInstance().GetDamagableTile(currentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, targetTileRow, targetTileColumn);
				if ((damagableTile == null || !damagableTile.HasBeenDamaged()) && FloorManager.GetInstance().CheckDamagableTileExists(currentFloor, tileSystem_Type, targetTileRow, targetTileColumn, bIncludeInactive: true, out var action, out var health) && action == DamagableTile.DamageAction.Cut && health <= m_fMinRequiredHealth)
				{
					floor = currentFloor;
					row = targetTileRow;
					column = targetTileColumn;
					systemType = tileSystem_Type;
					return true;
				}
			}
		}
		floor = null;
		row = -1;
		column = -1;
		systemType = FloorManager.TileSystem_Type.TileSystem_Wall;
		return false;
	}

	protected override Events GetRepairSoundEvent()
	{
		return Events.Play_Player_Wall_Cover;
	}
}
