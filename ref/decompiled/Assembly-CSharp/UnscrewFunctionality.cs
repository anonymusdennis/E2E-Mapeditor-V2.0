using UnityEngine;

[CreateAssetMenu(fileName = "Unscrew Functionality", menuName = "Team17/Items/Functionalities/Create Unscrew Functionality")]
public class UnscrewFunctionality : BaseItemFunctionality
{
	public enum Mode
	{
		High,
		Low,
		Both
	}

	public string m_PlayUnscrewSound;

	public string m_StopUnscrewSound;

	public int m_DamageDealtPerUse = 10;

	public int m_ItemDecayPerUse = 20;

	public bool m_Reclaim = true;

	public int m_StaminaUsedPerUse = 5;

	public bool m_bDoesCancelReduceHealth = true;

	public Mode m_UnscrewMode = Mode.Both;

	private AnimState m_Animation = AnimState.Hammer;

	private float m_UnscrewTime = 1f;

	private float m_ElapsedUnscrewTime;

	public override void Init()
	{
	}

	public override Functionality GetFunctionalityType()
	{
		return Functionality.Unscrew;
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
		if (base.ParentItem == null)
		{
			return false;
		}
		if (base.ParentItem.Health <= 0)
		{
			return false;
		}
		if (intendsOnUsingImmediately && m_Owner.m_CharacterStats.m_bIsPlayer && !m_Owner.m_CharacterStats.HasEnoughEnergyForTask(m_StaminaUsedPerUse))
		{
			SpeechManager.GetInstance().SaySomething(m_Owner, "Text.Player.NoStamina", SpeechTone.Negative, 3f, 10);
			return false;
		}
		FloorManager.Floor floor;
		int row;
		int column;
		return FindTile(out floor, out row, out column);
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
			FloorManager.TileSystem_Type systemType = FloorManager.TileSystem_Type.TileSystem_Ground;
			if (targetTileRow != -1 && targetTileColumn != -1)
			{
				if (m_Owner.m_CharacterStats.m_bIsPlayer && m_Reclaim)
				{
					ItemData itemReclaimed = null;
					if (FloorManager.GetInstance().WouldFullyDamageTile(currentFloor, systemType, targetTileRow, targetTileColumn, DamagableTile.DamageAction.Unscrew, m_ParentItem.m_ItemData.m_ItemDataID, m_DamageDealtPerUse, out itemReclaimed) && itemReclaimed != null && m_Owner.m_ItemContainer.GetFreeSpaceCount() == 0)
					{
						SpeechManager.GetInstance().SaySomething(m_Owner, "Text.Player.InventoryFull", SpeechTone.Negative, 3f, 10);
						return false;
					}
				}
				m_UnscrewTime = useTime;
				m_ElapsedUnscrewTime = 0f;
				m_Animation = useAnimation;
				m_Owner.UseEquippedItemRPC(m_Owner.GetEquippedItem(), bUse: true);
				m_Owner.m_CharacterAnimator.StartAnimation(m_Animation);
				m_Owner.SetIsCutting(value: true);
				if (!string.IsNullOrEmpty(m_PlayUnscrewSound))
				{
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_PlayUnscrewSound, m_Owner.gameObject);
				}
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
		m_ElapsedUnscrewTime += UpdateManager.deltaTime;
		if (m_ElapsedUnscrewTime >= m_UnscrewTime)
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
			m_Owner.SetIsCutting(value: false);
			if (!string.IsNullOrEmpty(m_StopUnscrewSound))
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_StopUnscrewSound, m_Owner.gameObject);
			}
		}
		if (base.ParentItem != null && m_bDoesCancelReduceHealth)
		{
			base.ParentItem.DecreaseHealth(m_ItemDecayPerUse);
		}
		return true;
	}

	public void FinishUsing()
	{
		if (m_Owner != null && m_ParentItem != null && m_ParentItem.m_ItemData != null)
		{
			m_Owner.UseEquippedItemRPC(m_Owner.GetEquippedItem(), bUse: false);
			m_Owner.m_CharacterAnimator.StopAnimation(m_Animation);
			m_Owner.SetIsCutting(value: false);
			if (!string.IsNullOrEmpty(m_StopUnscrewSound))
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_StopUnscrewSound, m_Owner.gameObject);
			}
			if (FindTile(out var floor, out var row, out var column))
			{
				FloorManager.GetInstance().DamageTile(floor, FloorManager.TileSystem_Type.TileSystem_Ground, row, column, DamagableTile.DamageAction.Unscrew, m_ParentItem.m_ItemData.m_ItemDataID, m_DamageDealtPerUse, m_Reclaim, m_Owner);
			}
			m_Owner.m_CharacterStats.DecreaseEnergyRPC(m_StaminaUsedPerUse);
			EffectManager.PlayEffect(EffectManager.effectType.StaminaDecrease, m_Owner.GetStatChangeEffectPosition());
		}
		if (base.ParentItem != null)
		{
			base.ParentItem.DecreaseHealth(m_ItemDecayPerUse);
		}
	}

	private bool FindTile(out FloorManager.Floor floor, out int row, out int column)
	{
		if (m_Owner != null && m_ParentItem != null && m_ParentItem.m_ItemData != null)
		{
			int targetTileRow = m_Owner.GetTargetTileRow();
			int targetTileColumn = m_Owner.GetTargetTileColumn();
			FloorManager.Floor currentFloor = m_Owner.CurrentFloor;
			FloorManager.TileSystem_Type systemType = FloorManager.TileSystem_Type.TileSystem_Ground;
			if (targetTileRow != -1 && targetTileColumn != -1 && !currentFloor.IsUnderGround())
			{
				if ((m_UnscrewMode == Mode.Low || m_UnscrewMode == Mode.Both) && FloorManager.GetInstance().CanDamageTile(currentFloor, systemType, targetTileRow, targetTileColumn, DamagableTile.DamageAction.Unscrew, m_ParentItem.m_ItemData.m_ItemDataID))
				{
					floor = currentFloor;
					row = targetTileRow;
					column = targetTileColumn;
					return true;
				}
				FloorManager.Floor floor2 = FloorManager.GetInstance().UpAFloor(currentFloor);
				if (floor2 != currentFloor)
				{
					if (currentFloor.IsVent())
					{
						if ((m_UnscrewMode == Mode.High || m_UnscrewMode == Mode.Both) && FloorManager.GetInstance().CanDamageTile(floor2, systemType, targetTileRow + -1, targetTileColumn, DamagableTile.DamageAction.Unscrew, m_ParentItem.m_ItemData.m_ItemDataID))
						{
							floor = floor2;
							row = targetTileRow + -1;
							column = targetTileColumn;
							return true;
						}
					}
					else if ((m_UnscrewMode == Mode.High || m_UnscrewMode == Mode.Both) && (!m_Owner.IsPlayer() || m_Owner.m_bIsStandingOnDesk) && FloorManager.GetInstance().CanDamageTile(floor2, systemType, targetTileRow, targetTileColumn, DamagableTile.DamageAction.Unscrew, m_ParentItem.m_ItemData.m_ItemDataID))
					{
						floor = floor2;
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
