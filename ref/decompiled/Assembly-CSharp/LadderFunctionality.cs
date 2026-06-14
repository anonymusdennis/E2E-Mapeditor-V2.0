using UnityEngine;

[CreateAssetMenu(fileName = "Ladder Functionality", menuName = "Team17/Items/Functionalities/Ladder Functionality")]
public class LadderFunctionality : BaseItemFunctionality
{
	private AnimState m_Animation = AnimState.UseLow;

	private float m_AnimTime = 0.5f;

	private float m_ElapsedAnimTime;

	public override void Init()
	{
	}

	public override Functionality GetFunctionalityType()
	{
		return Functionality.Ladder;
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
		if (base.ParentItem == null || m_Owner == null)
		{
			return false;
		}
		FloorManager.Floor floor;
		int row;
		int column;
		return FindTile(out floor, out row, out column) && !CheckForCharactersAtTilePosition(row, column, floor);
	}

	public override bool StartUsing(AnimState useAnimation, float useTime)
	{
		base.StartUsing(useAnimation, useTime);
		m_Animation = useAnimation;
		m_AnimTime = useTime;
		if (base.ParentItem != null && m_Owner != null)
		{
			m_Owner.m_bActionRenderersRequired = true;
			m_Owner.m_CharacterAnimator.StartAnimation(m_Animation);
			m_ElapsedAnimTime = 0f;
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
		if (CheckForCharactersAtTilePosition(m_Owner.GetTargetTileRow(), m_Owner.GetTargetTileColumn(), m_Owner.CurrentFloor))
		{
			CancelUsing();
			if (OnEndOfUse != null)
			{
				OnEndOfUse();
			}
			return false;
		}
		m_ElapsedAnimTime += UpdateManager.deltaTime;
		if (m_ElapsedAnimTime >= m_AnimTime)
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
		if (m_Owner != null)
		{
			m_Owner.UseEquippedItemRPC(m_Owner.GetEquippedItem(), bUse: false);
			m_Owner.m_CharacterAnimator.StopAnimation(m_Animation);
			m_Owner.m_bActionRenderersRequired = false;
		}
		return true;
	}

	public void FinishUsing()
	{
		if (!(m_Owner != null))
		{
			return;
		}
		m_Owner.m_CharacterAnimator.StopAnimation(m_Animation);
		m_Owner.m_bActionRenderersRequired = false;
		if (FindTile(out var floor, out var row, out var column))
		{
			if (m_Owner.m_CharacterStats.m_bIsPlayer)
			{
			}
			if (!FloorManager.GetInstance().GetTileCentrePosition(floor, FloorManager.TileSystem_Type.TileSystem_Ground, row, column, out var worldPosition))
			{
				worldPosition = m_Owner.transform.position;
			}
			m_Owner.DropEquipedItem(worldPosition);
		}
	}

	private bool FindTile(out FloorManager.Floor floor, out int row, out int column)
	{
		if (m_Owner != null)
		{
			int targetTileRow = m_Owner.GetTargetTileRow();
			int targetTileColumn = m_Owner.GetTargetTileColumn();
			FloorManager.Floor currentFloor = m_Owner.CurrentFloor;
			if (targetTileRow != -1 && targetTileColumn != -1 && !currentFloor.IsUnderGround() && !currentFloor.IsVent() && !FloorManager.GetInstance().CheckDamagableTileExists(currentFloor, FloorManager.TileSystem_Type.TileSystem_Wall, targetTileRow, targetTileColumn, bIncludeInactive: true, out var _, out var _))
			{
				DamagableTile damagableTile = FloorManager.GetInstance().GetDamagableTile(currentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, targetTileRow, targetTileColumn);
				if (damagableTile == null || !damagableTile.HasBeenDamaged())
				{
					if (!FloorManager.GetInstance().GetTileCentrePosition(currentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, targetTileRow, targetTileColumn, out var worldPosition))
					{
						worldPosition = m_Owner.transform.position;
					}
					if (m_Owner.DropItemCheck(m_ParentItem, worldPosition, silent: true))
					{
						floor = currentFloor;
						row = targetTileRow;
						column = targetTileColumn;
						return true;
					}
				}
			}
		}
		floor = null;
		row = -1;
		column = -1;
		return false;
	}
}
