using UnityEngine;

[CreateAssetMenu(fileName = "Cover Tile Functionality", menuName = "Team17/Items/Functionalities/Create Cover Tile Functionality")]
public class CoverTileFunctionality : BaseItemFunctionality
{
	public string m_UseSound;

	public int m_ItemDecayPerUse = 20;

	public DamagableTile.DamageAction m_TargetDamageAction = DamagableTile.DamageAction.Chip;

	private AnimState m_Animation = AnimState.UseLow;

	private float m_CoverTime = 1f;

	private float m_ElapsedCoverTime;

	public override void Init()
	{
	}

	public override Functionality GetFunctionalityType()
	{
		return Functionality.CoverTile;
	}

	public override bool RequiresTargetting()
	{
		return true;
	}

	public override bool RequiresPositioning()
	{
		return true;
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
		FloorManager.TileSystem_Type systemType;
		int row;
		int column;
		return FindTile(out floor, out systemType, out row, out column) && !CheckForCharactersAtTilePosition(row, column, floor);
	}

	public override bool StartUsing(AnimState useAnimation, float useTime)
	{
		base.StartUsing(useAnimation, useTime);
		m_Animation = useAnimation;
		m_CoverTime = useTime;
		if (base.ParentItem != null && m_Owner != null)
		{
			m_Owner.m_bActionRenderersRequired = true;
			m_Owner.m_CharacterAnimator.StartAnimation(m_Animation);
			m_ElapsedCoverTime = 0f;
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
		if (CheckForCharactersAtTilePosition(m_Owner.GetTargetTileRow(), m_Owner.GetTargetTileRow(), m_Owner.CurrentFloor))
		{
			CancelUsing();
			if (OnEndOfUse != null)
			{
				OnEndOfUse();
			}
			return false;
		}
		m_ElapsedCoverTime += UpdateManager.deltaTime;
		if (m_ElapsedCoverTime >= m_CoverTime)
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
		if (!string.IsNullOrEmpty(m_UseSound))
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_UseSound, m_Owner.gameObject);
		}
		if (FindTile(out var floor, out var systemType, out var row, out var column))
		{
			if (m_Owner.m_CharacterStats.m_bIsPlayer)
			{
			}
			FloorManager.GetInstance().GiveTileItem(floor, systemType, row, column, base.ParentItem, m_Owner);
		}
	}

	public void OnPickUpCover()
	{
		if (base.ParentItem != null)
		{
			base.ParentItem.DecreaseHealth(m_ItemDecayPerUse);
		}
	}

	private bool FindTile(out FloorManager.Floor floor, out FloorManager.TileSystem_Type systemType, out int row, out int column)
	{
		FloorManager.TileSystem_Type targetTileSystem = GetTargetTileSystem();
		if (m_Owner != null && m_ParentItem != null && m_ParentItem.m_ItemData != null)
		{
			int num = m_Owner.GetTargetTileRow();
			int targetTileColumn = m_Owner.GetTargetTileColumn();
			FloorManager.Floor floor2 = m_Owner.CurrentFloor;
			if (num != -1 && targetTileColumn != -1)
			{
				bool flag = false;
				if (targetTileSystem == FloorManager.TileSystem_Type.TileSystem_Wall && m_TargetDamageAction != 0)
				{
					if (FloorManager.GetInstance().CanCoverTile(floor2, targetTileSystem, num, targetTileColumn, m_ParentItem.m_ItemData.m_ItemDataID, m_TargetDamageAction))
					{
						flag = !FloorManager.GetInstance().CheckDamagableTileExists(floor2, FloorManager.TileSystem_Type.TileSystem_Ground, num, targetTileColumn, bIncludeInactive: true, out var _, out var _);
					}
				}
				else if (targetTileSystem == FloorManager.TileSystem_Type.TileSystem_Ground)
				{
					if (FloorManager.GetInstance().CanCoverTile(floor2, targetTileSystem, num, targetTileColumn, m_ParentItem.m_ItemData.m_ItemDataID, m_TargetDamageAction))
					{
						flag = !FloorManager.GetInstance().CheckTileExists(floor2, FloorManager.TileSystem_Type.TileSystem_Wall, num, targetTileColumn, bIncludeInactive: true);
					}
					else
					{
						FloorManager.Floor floor3 = FloorManager.GetInstance().UpAFloor(floor2);
						if (floor3 != floor2)
						{
							if (floor2.IsVent())
							{
								VentCover ventCover = FloorManager.GetInstance().GetVentCover(num + -1, targetTileColumn, floor3.m_FloorIndex);
								if (ventCover != null)
								{
									floor2 = floor3;
									num += -1;
									flag = ventCover.GetTileHealth() <= 0f;
								}
							}
							else if (m_Owner.m_bIsStandingOnDesk || !m_Owner.m_CharacterStats.m_bIsPlayer)
							{
								VentCover ventCover2 = FloorManager.GetInstance().GetVentCover(num, targetTileColumn, floor3.m_FloorIndex);
								if (ventCover2 != null)
								{
									floor2 = floor3;
									flag = ventCover2.GetTileHealth() <= 0f;
								}
							}
						}
					}
				}
				if (flag)
				{
					floor = floor2;
					row = num;
					column = targetTileColumn;
					systemType = targetTileSystem;
					return true;
				}
			}
		}
		floor = null;
		row = -1;
		column = -1;
		systemType = targetTileSystem;
		return false;
	}

	private FloorManager.TileSystem_Type GetTargetTileSystem()
	{
		if (m_TargetDamageAction == DamagableTile.DamageAction.Unscrew)
		{
			return FloorManager.TileSystem_Type.TileSystem_Ground;
		}
		return FloorManager.TileSystem_Type.TileSystem_Wall;
	}
}
