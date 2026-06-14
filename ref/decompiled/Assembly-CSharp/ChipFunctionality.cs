using UnityEngine;

[CreateAssetMenu(fileName = "Chip Functionality", menuName = "Team17/Items/Functionalities/Create Chip Functionality")]
public class ChipFunctionality : BaseItemFunctionality
{
	public string m_PlayChipSound;

	public string m_StopChipSound;

	public string m_PlayOneshotChipSound;

	public int m_DamageDealtPerUse = 10;

	public int m_ItemDecayPerUse = 20;

	public int m_StaminaUsedPerUse = 20;

	public bool m_Reclaim = true;

	public bool m_bDoesCancelReduceHealth = true;

	private AnimState m_Animation = AnimState.PickaxeUse;

	private float m_ChipTime = 1f;

	private float m_ElapsedChipTime;

	public override void Init()
	{
	}

	public override Functionality GetFunctionalityType()
	{
		return Functionality.Chip;
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
			int targetTileRow = m_Owner.GetTargetTileRow();
			int targetTileColumn = m_Owner.GetTargetTileColumn();
			if (targetTileRow != -1 && targetTileColumn != -1)
			{
				return FloorManager.GetInstance().CanDamageTile(m_Owner.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Wall, targetTileRow, targetTileColumn, DamagableTile.DamageAction.Chip, base.ParentItem.m_ItemData.m_ItemDataID);
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
				SpeechManager.GetInstance().SaySomething(m_Owner, "Text.Player.NoStaminaInteract", SpeechTone.Negative, 3f, 10);
				return false;
			}
			int targetTileRow = m_Owner.GetTargetTileRow();
			int targetTileColumn = m_Owner.GetTargetTileColumn();
			FloorManager.Floor currentFloor = m_Owner.CurrentFloor;
			FloorManager.TileSystem_Type systemType = FloorManager.TileSystem_Type.TileSystem_Wall;
			if (targetTileRow != -1 && targetTileColumn != -1)
			{
				if (m_Reclaim && m_Owner.m_CharacterStats.m_bIsPlayer)
				{
					ItemData itemReclaimed = null;
					if (FloorManager.GetInstance().WouldFullyDamageTile(currentFloor, systemType, targetTileRow, targetTileColumn, DamagableTile.DamageAction.Chip, m_ParentItem.m_ItemData.m_ItemDataID, m_DamageDealtPerUse, out itemReclaimed) && itemReclaimed != null && m_Owner.m_ItemContainer.GetFreeSpaceCount() == 0)
					{
						SpeechManager.GetInstance().SaySomething(m_Owner, "Text.Player.InventoryFull", SpeechTone.Negative, 3f, 10);
						return false;
					}
				}
				m_ChipTime = useTime;
				m_ElapsedChipTime = 0f;
				m_Animation = useAnimation;
				m_Owner.UseEquippedItemRPC(m_Owner.GetEquippedItem(), bUse: true);
				m_Owner.m_CharacterAnimator.StartAnimation(m_Animation);
				m_Owner.SetIsChipping(value: true);
				if (!string.IsNullOrEmpty(m_PlayChipSound))
				{
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_PlayChipSound, m_Owner.gameObject);
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
		m_ElapsedChipTime += UpdateManager.deltaTime;
		if (m_ElapsedChipTime >= m_ChipTime)
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
		if (m_ElapsedChipTime < m_ChipTime && !string.IsNullOrEmpty(m_PlayChipSound))
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_PlayOneshotChipSound, m_Owner.gameObject);
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
			m_Owner.SetIsChipping(value: false);
			if (!string.IsNullOrEmpty(m_StopChipSound))
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_StopChipSound, m_Owner.gameObject);
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
			m_Owner.SetIsChipping(value: false);
			if (!string.IsNullOrEmpty(m_StopChipSound))
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_StopChipSound, m_Owner.gameObject);
			}
			int targetTileRow = m_Owner.GetTargetTileRow();
			int targetTileColumn = m_Owner.GetTargetTileColumn();
			FloorManager.GetInstance().DamageTile(m_Owner.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Wall, targetTileRow, targetTileColumn, DamagableTile.DamageAction.Chip, m_ParentItem.m_ItemData.m_ItemDataID, m_DamageDealtPerUse, m_Reclaim, m_Owner);
		}
		m_Owner.m_CharacterStats.DecreaseEnergyRPC(m_StaminaUsedPerUse);
		EffectManager.PlayEffect(EffectManager.effectType.StaminaDecrease, m_Owner.GetStatChangeEffectPosition());
		if (base.ParentItem != null)
		{
			base.ParentItem.DecreaseHealth(m_ItemDecayPerUse);
		}
	}
}
