using AUTOGEN_T17Wwise_Enums;
using Rotorz.Tile;
using UnityEngine;

[CreateAssetMenu(fileName = "Dig Functionality", menuName = "Team17/Items/Functionalities/Create Dig Functionality")]
public class DigFunctionality : BaseItemFunctionality
{
	public int m_DamageDealtPerUse = 10;

	public int m_ItemDecayPerUse = 20;

	public int m_StaminaUsedPerUse = 20;

	public bool m_Reclaim = true;

	public bool m_bDoesCancelReduceHealth = true;

	private AnimState m_Animation = AnimState.IdleDig;

	private float m_DigTime = 1f;

	private float m_ElapsedDigTime;

	private float m_fEffectTime = 0.75f;

	private float m_fEffectTimer = 0.5f;

	private bool m_bDigUp;

	private Vector3 m_vDigPosition;

	private bool m_bShowEffect;

	private ElectricFence m_ElectricFence;

	private float m_ElectrocuteTime = 0.3f;

	private int m_WallLayerIndex = -1;

	public override void Init()
	{
		m_ElectrocuteTime = m_DigTime * 0.3f;
		m_WallLayerIndex = LayerMask.NameToLayer("Wall");
	}

	public override Functionality GetFunctionalityType()
	{
		return Functionality.Dig;
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
		if (base.ParentItem == null || base.ParentItem.m_ItemData == null)
		{
			return false;
		}
		if (base.ParentItem.Health <= 0)
		{
			return false;
		}
		if (m_Owner != null)
		{
			if (intendsOnUsingImmediately && m_Owner.m_CharacterStats.m_bIsPlayer && !m_Owner.m_CharacterStats.HasEnoughEnergyForTask(m_StaminaUsedPerUse))
			{
				SpeechManager.GetInstance().SaySomething(m_Owner, "Text.Player.NoStamina", SpeechTone.Negative, 3f, 10);
				return false;
			}
			if (FloorManager.GetInstance().GetTileGridPoint(m_Owner.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, m_Owner.transform.position, out var row, out var column))
			{
				int num = m_Owner.GetTargetTileRow();
				int targetTileColumn = m_Owner.GetTargetTileColumn();
				if (num != -1 && targetTileColumn != -1)
				{
					if (m_Owner.CurrentFloor.IsUnderGround())
					{
						if (FloorManager.GetInstance().GetRock(num, targetTileColumn, m_Owner.CurrentFloor.m_FloorIndex) != null)
						{
							return false;
						}
						if (FloorManager.GetInstance().CheckTileExists(m_Owner.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Wall, num, targetTileColumn))
						{
							if (FloorManager.GetInstance().GetTileComponent<PreventTileDamage>(m_Owner.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Wall, num, targetTileColumn) != null)
							{
								return false;
							}
							if (!FloorManager.GetInstance().CanDamageTile(m_Owner.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Wall, num, targetTileColumn, DamagableTile.DamageAction.Dig, base.ParentItem.m_ItemData.m_ItemDataID))
							{
								return false;
							}
						}
						else
						{
							FloorManager.Floor floor = FloorManager.GetInstance().UpAFloor(m_Owner.CurrentFloor);
							if (floor == m_Owner.CurrentFloor)
							{
								return false;
							}
							if (!FloorManager.GetInstance().IsFloorClear(m_Owner.CurrentFloor, num, targetTileColumn))
							{
								return false;
							}
							if (m_Owner.IsPlayer())
							{
								num += -1;
							}
							Hole hole = FloorManager.GetInstance().GetHole(num, targetTileColumn, floor.m_FloorIndex);
							if (hole != null && hole.IsFullyDug())
							{
								return false;
							}
							if (!FloorManager.GetInstance().IsFloorClear(floor, num, targetTileColumn))
							{
								return false;
							}
						}
						return true;
					}
					if (m_Owner.m_bIsStandingOnDesk && m_Owner.IsPlayer())
					{
						return false;
					}
					if (m_Owner.CurrentFloor.IsVent())
					{
						return false;
					}
					if (FloorManager.GetInstance().CheckTileExists(m_Owner.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Wall, num, targetTileColumn))
					{
						return false;
					}
					Hole hole2 = FloorManager.GetInstance().GetHole(num, targetTileColumn, m_Owner.CurrentFloor.m_FloorIndex);
					if (hole2 != null && hole2.IsFullyDug())
					{
						return false;
					}
					int deltaRow = num - row;
					int deltaColumn = targetTileColumn - column;
					if (m_Owner.IsPlayer() && !FloorManager.GetInstance().IsFloorAheadClear(m_Owner.CurrentFloor, row, column, deltaRow, deltaColumn))
					{
						return false;
					}
					FloorManager.Floor floor2 = FloorManager.GetInstance().DownAFloor(m_Owner.CurrentFloor);
					if (floor2 == m_Owner.CurrentFloor)
					{
						return false;
					}
					num++;
					if (!floor2.IsUnderGround())
					{
						return false;
					}
					if (FloorManager.GetInstance().CheckTileExists(floor2, FloorManager.TileSystem_Type.TileSystem_Wall, num, targetTileColumn))
					{
						if (!FloorManager.GetInstance().CanDamageTile(floor2, FloorManager.TileSystem_Type.TileSystem_Wall, num, targetTileColumn, DamagableTile.DamageAction.Dig, base.ParentItem.m_ItemData.m_ItemDataID))
						{
							return false;
						}
						if (FloorManager.GetInstance().GetTileComponent<PreventTileDamage>(floor2, FloorManager.TileSystem_Type.TileSystem_Wall, num, targetTileColumn) != null)
						{
							return false;
						}
						if (!FloorManager.GetInstance().IsFloorClear(floor2, num, targetTileColumn, out var hitList, out var hitCount))
						{
							for (int i = 0; i < hitCount; i++)
							{
								if (hitList[i].collider != null && hitList[i].collider.gameObject != null && hitList[i].collider.gameObject.layer != m_WallLayerIndex)
								{
									return false;
								}
							}
						}
					}
					else if (FloorManager.GetInstance().GetRock(num, targetTileColumn, floor2.m_FloorIndex) == null)
					{
						if (!FloorManager.GetInstance().CheckTileExists(floor2, FloorManager.TileSystem_Type.TileSystem_Ground, num, targetTileColumn))
						{
							return false;
						}
						if (!FloorManager.GetInstance().IsFloorClear(floor2, num, targetTileColumn))
						{
							return false;
						}
					}
					return true;
				}
			}
		}
		return false;
	}

	public override bool StartUsing(AnimState useAnimation, float useTime)
	{
		base.StartUsing(useAnimation, useTime);
		if (base.ParentItem != null && base.ParentItem.m_ItemData != null && m_Owner != null)
		{
			if (m_Owner.m_CharacterStats.m_bIsPlayer && !m_Owner.m_CharacterStats.HasEnoughEnergyForTask(m_StaminaUsedPerUse))
			{
				SpeechManager.GetInstance().SaySomething(m_Owner, "Text.Player.NoStamina", SpeechTone.Negative, 3f, 10);
				return false;
			}
			int targetTileRow = m_Owner.GetTargetTileRow();
			int targetTileColumn = m_Owner.GetTargetTileColumn();
			FloorManager.Floor currentFloor = m_Owner.CurrentFloor;
			FloorManager.TileSystem_Type systemType = FloorManager.TileSystem_Type.TileSystem_Wall;
			if (targetTileRow != -1 && targetTileColumn != -1)
			{
				if (m_Owner.m_CharacterStats.m_bIsPlayer && currentFloor.IsUnderGround() && FloorManager.GetInstance().CheckTileExists(currentFloor, systemType, targetTileRow, targetTileColumn) && !FloorManager.GetInstance().CheckForBracedOrPremadeTunelWithinNTiles(m_Owner.CurrentFloor, targetTileRow, targetTileColumn, 3))
				{
					SpeechManager.GetInstance().SaySomething(m_Owner, "Text.Player.TunnelSupport", SpeechTone.Negative, 3f, 10);
					return false;
				}
				if (m_Reclaim)
				{
					bool flag = false;
					if (currentFloor.IsUnderGround())
					{
						if (FloorManager.GetInstance().CheckTileExists(currentFloor, systemType, targetTileRow, targetTileColumn))
						{
							ItemData itemReclaimed = null;
							if (FloorManager.GetInstance().WouldFullyDamageTile(currentFloor, systemType, targetTileRow, targetTileColumn, DamagableTile.DamageAction.Dig, m_ParentItem.m_ItemData.m_ItemDataID, m_DamageDealtPerUse, out itemReclaimed))
							{
								flag = itemReclaimed != null;
							}
						}
						else
						{
							int floorIndex = FloorManager.GetInstance().UpAFloor(currentFloor).m_FloorIndex;
							flag = FloorManager.GetInstance().WouldFullyDigHole(targetTileRow + -1, targetTileColumn, floorIndex, m_DamageDealtPerUse);
						}
					}
					else
					{
						flag = FloorManager.GetInstance().WouldFullyDigHole(targetTileRow, targetTileColumn, currentFloor.m_FloorIndex, m_DamageDealtPerUse);
					}
					if (m_Owner.m_CharacterStats.m_bIsPlayer && flag && m_Owner.m_ItemContainer.GetFreeSpaceCount() == 0)
					{
						SpeechManager.GetInstance().SaySomething(m_Owner, "Text.Player.InventoryFull", SpeechTone.Negative, 3f, 10);
						return false;
					}
				}
				m_ElectricFence = null;
				TileData tile = FloorManager.GetInstance().GetTile(currentFloor, systemType, targetTileRow, targetTileColumn);
				if (tile != null && tile.gameObject != null)
				{
					m_ElectricFence = tile.gameObject.GetComponentInChildren<ElectricFence>();
				}
				m_DigTime = useTime;
				m_ElapsedDigTime = 0f;
				m_fEffectTimer = m_fEffectTime * 0.75f;
				m_bShowEffect = FloorManager.GetInstance().GetTileCentrePosition(currentFloor, systemType, targetTileRow, targetTileColumn, out m_vDigPosition);
				if (currentFloor.IsUnderGround() && !FloorManager.GetInstance().CheckTileExists(currentFloor, systemType, targetTileRow, targetTileColumn))
				{
					useAnimation = AnimState.IdleDigAbove;
					m_bDigUp = true;
				}
				else
				{
					m_bDigUp = false;
				}
				m_Animation = useAnimation;
				m_Owner.UseEquippedItemRPC(m_Owner.GetEquippedItem(), bUse: true);
				m_Owner.m_CharacterAnimator.StartAnimation(m_Animation);
				m_Owner.SetIsDigging(value: true);
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Dig, m_Owner.gameObject);
				if (OnStartOfUse != null)
				{
					OnStartOfUse();
				}
				return true;
			}
		}
		return false;
	}

	public override bool UpdateUsing()
	{
		base.UpdateUsing();
		m_ElapsedDigTime += UpdateManager.deltaTime;
		if (m_ElectricFence != null && m_ElapsedDigTime >= m_ElectrocuteTime && m_ElectricFence.GetEnabled() && m_Owner != null && m_ElectricFence.ShockCharacter(m_Owner))
		{
			CancelUsing();
			return false;
		}
		m_fEffectTimer += UpdateManager.deltaTime;
		if (m_fEffectTimer > m_fEffectTime)
		{
			m_fEffectTimer = 0f;
			if (m_bShowEffect)
			{
				EffectManager.PlayEffect((!m_bDigUp) ? EffectManager.effectType.DiggingDown : EffectManager.effectType.DiggingUp, m_vDigPosition);
			}
		}
		if (m_ElapsedDigTime >= m_DigTime)
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

	protected override void AnimationSinglePlayDone()
	{
		if (m_ElapsedDigTime < m_DigTime)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Dig, m_Owner.gameObject);
		}
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
			m_Owner.SetIsDigging(value: false);
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_Player_Dig, m_Owner.gameObject);
		}
		if (base.ParentItem != null && m_bDoesCancelReduceHealth)
		{
			base.ParentItem.DecreaseHealth(m_ItemDecayPerUse);
		}
		m_ElectricFence = null;
		return true;
	}

	public void FinishUsing()
	{
		if (m_Owner != null && m_ParentItem != null && m_ParentItem.m_ItemData != null)
		{
			m_Owner.UseEquippedItemRPC(m_Owner.GetEquippedItem(), bUse: false);
			m_Owner.m_CharacterAnimator.StopAnimation(m_Animation);
			m_Owner.SetIsDigging(value: false);
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_Player_Dig, m_Owner.gameObject);
			int num = m_Owner.GetTargetTileRow();
			int targetTileColumn = m_Owner.GetTargetTileColumn();
			if (num != -1 && targetTileColumn != -1)
			{
				if (m_Owner.CurrentFloor.IsUnderGround())
				{
					if (FloorManager.GetInstance().CheckTileExists(m_Owner.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Wall, num, targetTileColumn))
					{
						FloorManager.GetInstance().DamageTile(m_Owner.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Wall, num, targetTileColumn, DamagableTile.DamageAction.Dig, m_ParentItem.m_ItemData.m_ItemDataID, m_DamageDealtPerUse, m_Reclaim, m_Owner);
					}
					else
					{
						int floorIndex = FloorManager.GetInstance().UpAFloor(m_Owner.CurrentFloor).m_FloorIndex;
						int toFloor = -1;
						if (m_Owner.IsPlayer())
						{
							num += -1;
						}
						FloorManager.GetInstance().DigHole(num, targetTileColumn, floorIndex, toFloor, m_ParentItem.m_ItemData.m_ItemDataID, m_DamageDealtPerUse, m_Reclaim, m_Owner);
					}
				}
				else
				{
					FloorManager.Floor currentFloor = m_Owner.CurrentFloor;
					FloorManager.Floor floor = FloorManager.GetInstance().DownAFloor(currentFloor);
					if (floor.IsVent())
					{
						floor = FloorManager.GetInstance().DownAFloor(floor);
					}
					FloorManager.GetInstance().DigHole(num, targetTileColumn, currentFloor.m_FloorIndex, floor.m_FloorIndex, m_ParentItem.m_ItemData.m_ItemDataID, m_DamageDealtPerUse, m_Reclaim, m_Owner);
				}
				m_Owner.m_CharacterStats.DecreaseEnergyRPC(m_StaminaUsedPerUse);
				EffectManager.PlayEffect(EffectManager.effectType.StaminaDecrease, m_Owner.GetStatChangeEffectPosition());
				if (base.ParentItem != null)
				{
					base.ParentItem.DecreaseHealth(m_ItemDecayPerUse);
				}
			}
		}
		m_ElectricFence = null;
	}
}
