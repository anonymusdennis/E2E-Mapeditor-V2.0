using Rotorz.Tile;
using UnityEngine;

[CreateAssetMenu(fileName = "Cut Functionality", menuName = "Team17/Items/Functionalities/Create Cut Functionality")]
public class CutFunctionality : BaseItemFunctionality
{
	public string m_PlayCutSound;

	public string m_StopCutSound;

	public string m_PlayOneshotCutSound;

	public int m_DamageDealtPerUse = 10;

	public int m_ItemDecayPerUse = 20;

	public int m_StaminaUsedPerUse = 20;

	public bool m_Reclaim = true;

	public bool m_bDoesCancelReduceHealth = true;

	private AnimState m_Animation = AnimState.IdleCut;

	private float m_CutTime = 1f;

	private float m_ElapsedCutTime;

	private ElectricFence m_ElectricFence;

	private float m_ElectrocuteTime = 0.3f;

	public override void Init()
	{
		m_ElectrocuteTime = m_CutTime * 0.3f;
	}

	public override Functionality GetFunctionalityType()
	{
		return Functionality.Cut;
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
			if (intendsOnUsingImmediately && !m_Owner.m_CharacterStats.HasEnoughEnergyForTask(m_StaminaUsedPerUse) && m_Owner.m_CharacterStats.m_bIsPlayer)
			{
				SpeechManager.GetInstance().SaySomething(m_Owner, "Text.Player.NoStamina", SpeechTone.Negative, 3f, 10);
				return false;
			}
			int targetTileRow = m_Owner.GetTargetTileRow();
			int targetTileColumn = m_Owner.GetTargetTileColumn();
			if (targetTileRow != -1 && targetTileColumn != -1)
			{
				return FloorManager.GetInstance().CanDamageTile(m_Owner.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Wall, targetTileRow, targetTileColumn, DamagableTile.DamageAction.Cut, base.ParentItem.m_ItemData.m_ItemDataID);
			}
		}
		return false;
	}

	public override bool StartUsing(AnimState useAnimation, float useTime)
	{
		base.StartUsing(useAnimation, useTime);
		if (base.ParentItem != null && base.ParentItem.m_ItemData != null && m_Owner != null)
		{
			if (!m_Owner.m_CharacterStats.HasEnoughEnergyForTask(m_StaminaUsedPerUse) && m_Owner.m_CharacterStats.m_bIsPlayer)
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
				if (m_Owner.m_CharacterStats.m_bIsPlayer && m_Reclaim)
				{
					ItemData itemReclaimed = null;
					if (FloorManager.GetInstance().WouldFullyDamageTile(currentFloor, systemType, targetTileRow, targetTileColumn, DamagableTile.DamageAction.Cut, m_ParentItem.m_ItemData.m_ItemDataID, m_DamageDealtPerUse, out itemReclaimed) && itemReclaimed != null && m_Owner.m_ItemContainer.GetFreeSpaceCount() == 0)
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
				m_CutTime = useTime;
				m_ElapsedCutTime = 0f;
				m_Animation = useAnimation;
				m_Owner.UseEquippedItemRPC(m_Owner.GetEquippedItem(), bUse: true);
				m_Owner.m_CharacterAnimator.StartAnimation(m_Animation);
				m_Owner.SetIsCutting(value: true);
				if (!string.IsNullOrEmpty(m_PlayCutSound))
				{
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_PlayCutSound, m_Owner.gameObject);
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
		m_ElapsedCutTime += UpdateManager.deltaTime;
		if (m_ElectricFence != null && m_ElapsedCutTime >= m_ElectrocuteTime && m_ElectricFence.GetEnabled() && m_Owner != null && m_ElectricFence.ShockCharacter(m_Owner))
		{
			CancelUsing();
			return false;
		}
		if (m_ElapsedCutTime >= m_CutTime)
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
		if (m_ElapsedCutTime < m_CutTime && !string.IsNullOrEmpty(m_PlayOneshotCutSound))
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_PlayOneshotCutSound, m_Owner.gameObject);
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
			m_Owner.SetIsCutting(value: false);
			if (!string.IsNullOrEmpty(m_StopCutSound))
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_StopCutSound, m_Owner.gameObject);
			}
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
			m_Owner.SetIsCutting(value: false);
			if (!string.IsNullOrEmpty(m_StopCutSound))
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_StopCutSound, m_Owner.gameObject);
			}
			int targetTileRow = m_Owner.GetTargetTileRow();
			int targetTileColumn = m_Owner.GetTargetTileColumn();
			FloorManager.GetInstance().DamageTile(m_Owner.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Wall, targetTileRow, targetTileColumn, DamagableTile.DamageAction.Cut, m_ParentItem.m_ItemData.m_ItemDataID, m_DamageDealtPerUse, m_Reclaim, m_Owner);
		}
		m_Owner.m_CharacterStats.DecreaseEnergyRPC(m_StaminaUsedPerUse);
		EffectManager.PlayEffect(EffectManager.effectType.StaminaDecrease, m_Owner.GetStatChangeEffectPosition());
		if (base.ParentItem != null)
		{
			base.ParentItem.DecreaseHealth(m_ItemDecayPerUse);
		}
		m_ElectricFence = null;
	}
}
