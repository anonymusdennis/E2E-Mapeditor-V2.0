using AUTOGEN_T17Wwise_Enums;

public abstract class RepairFunctionality : BaseItemFunctionality
{
	public float m_HealthRestore = -1f;

	public bool m_bDestroyOnUse = true;

	public float m_fMinRequiredHealth;

	private AnimState m_Animation = AnimState.UseLow;

	private float m_RepairTime = 1f;

	private float m_ElapsedRepairTime;

	public override void Init()
	{
	}

	public override Functionality GetFunctionalityType()
	{
		return Functionality.Repair;
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
		bool flag = FindTile(out floor, out systemType, out row, out column);
		bool flag2 = false;
		if (m_Owner.IsPlayer())
		{
			flag2 = CheckForCharactersAtTilePosition(row, column, floor);
		}
		return flag && !flag2;
	}

	public override bool StartUsing(AnimState useAnimation, float useTime)
	{
		base.StartUsing(useAnimation, useTime);
		m_Animation = useAnimation;
		m_RepairTime = useTime;
		if (base.ParentItem != null && m_Owner != null)
		{
			m_Owner.m_bActionRenderersRequired = true;
			m_Owner.UseEquippedItemRPC(m_Owner.GetEquippedItem(), bUse: true);
			m_Owner.m_CharacterAnimator.StartAnimation(m_Animation);
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Craft, m_Owner.gameObject);
			m_ElapsedRepairTime = 0f;
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
		m_ElapsedRepairTime += UpdateManager.deltaTime;
		if (m_ElapsedRepairTime >= m_RepairTime)
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
		m_Owner.UseEquippedItemRPC(m_Owner.GetEquippedItem(), bUse: false);
		m_Owner.m_CharacterAnimator.StopAnimation(m_Animation);
		m_Owner.m_bActionRenderersRequired = false;
		if (FindTile(out var floor, out var systemType, out var row, out var column))
		{
			Character character = CharacterAtTilePosition(row, column, floor);
			if (character != null && character.IsPlayer())
			{
				character.ForceStopInteraction();
				character.Teleport(m_Owner.GetCachedCurrentPosition());
			}
			FloorManager.GetInstance().RepairTile(floor, systemType, row, column, m_HealthRestore, m_Owner);
			if (m_Owner.m_CharacterStats.m_bIsPlayer)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, GetRepairSoundEvent(), m_Owner.gameObject);
			}
			if (base.ParentItem != null && m_bDestroyOnUse)
			{
				m_Owner.RemoveItemRPC(base.ParentItem, RPC_CallContexts.Unknown, release: true);
			}
		}
	}

	protected abstract bool FindTile(out FloorManager.Floor floor, out FloorManager.TileSystem_Type systemType, out int row, out int column);

	protected abstract Events GetRepairSoundEvent();
}
