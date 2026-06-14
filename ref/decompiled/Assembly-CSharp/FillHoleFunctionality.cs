using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

[CreateAssetMenu(fileName = "Fill Hole Functionality", menuName = "Team17/Items/Functionalities/Create Fill Hole Functionality")]
public class FillHoleFunctionality : BaseItemFunctionality
{
	public bool m_FakeCoverHole = true;

	public float m_HealthRestore = 20f;

	public bool m_bDestroyOnUse = true;

	private AnimState m_Animation = AnimState.UseLow;

	private float m_FillTime = 1f;

	private float m_ElapsedTime;

	public override void Init()
	{
	}

	public override Functionality GetFunctionalityType()
	{
		return Functionality.FillHole;
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
		return FindHole() != null;
	}

	public override bool StartUsing(AnimState useAnimation, float useTime)
	{
		base.StartUsing(useAnimation, useTime);
		m_Animation = useAnimation;
		m_FillTime = useTime;
		if (base.ParentItem != null && m_Owner != null)
		{
			m_Owner.m_bActionRenderersRequired = true;
			m_Owner.UseEquippedItemRPC(m_Owner.GetEquippedItem(), bUse: true);
			m_Owner.m_CharacterAnimator.StartAnimation(m_Animation);
			m_ElapsedTime = 0f;
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
		m_ElapsedTime += UpdateManager.deltaTime;
		if (m_ElapsedTime >= m_FillTime)
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
		Hole hole = FindHole();
		if (hole != null)
		{
			FloorManager.GetInstance().FillHole(hole, m_HealthRestore, m_FakeCoverHole, m_Owner);
			if (m_Owner.m_CharacterStats.m_bIsPlayer)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Hole_Cover, m_Owner.gameObject);
			}
			if (base.ParentItem != null && m_bDestroyOnUse)
			{
				m_Owner.RemoveItemRPC(base.ParentItem, RPC_CallContexts.Unknown, release: true);
			}
		}
	}

	private Hole FindHole()
	{
		if (m_Owner != null)
		{
			int targetTileRow = m_Owner.GetTargetTileRow();
			int targetTileColumn = m_Owner.GetTargetTileColumn();
			int floorIndex = m_Owner.CurrentFloor.m_FloorIndex;
			if (targetTileRow != -1 && targetTileColumn != -1)
			{
				bool floorCheck = true;
				if (!m_Owner.IsPlayer())
				{
					floorCheck = false;
				}
				Hole hole = FloorManager.GetInstance().GetHole(targetTileRow, targetTileColumn, floorIndex, floorCheck);
				if (hole != null && (!m_FakeCoverHole || !hole.IsCoveredUp))
				{
					return hole;
				}
			}
		}
		return null;
	}
}
