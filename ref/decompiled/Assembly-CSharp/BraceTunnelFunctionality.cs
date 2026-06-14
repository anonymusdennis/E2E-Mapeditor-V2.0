using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

[CreateAssetMenu(fileName = "Brace Tunnel Functionality", menuName = "Team17/Items/Functionalities/Create Brace Tunnel Functionality")]
public class BraceTunnelFunctionality : BaseItemFunctionality
{
	private AnimState m_Animation = AnimState.UseMed;

	private float m_UseTime = 1f;

	private float m_ElapsedTime;

	public override void Init()
	{
	}

	public override Functionality GetFunctionalityType()
	{
		return Functionality.BraceTunnel;
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
		if (base.ParentItem != null && m_Owner != null)
		{
			int targetTileRow = m_Owner.GetTargetTileRow();
			int targetTileColumn = m_Owner.GetTargetTileColumn();
			FloorManager.Floor currentFloor = m_Owner.CurrentFloor;
			FloorManager.TileSystem_Type systemType = FloorManager.TileSystem_Type.TileSystem_Wall;
			if (targetTileRow != -1 && targetTileColumn != -1)
			{
				if (!currentFloor.IsUnderGround())
				{
					return false;
				}
				if (!FloorManager.GetInstance().CheckIsInBounds(currentFloor, systemType, targetTileRow, targetTileColumn))
				{
					return false;
				}
				if (FloorManager.GetInstance().CheckTileExists(currentFloor, systemType, targetTileRow, targetTileColumn))
				{
					return false;
				}
				if (FloorManager.GetInstance().GetDamagableTile(currentFloor, systemType, targetTileRow, targetTileColumn, DamagableTile.DamageAction.Dig) == null)
				{
					return false;
				}
				if (FloorManager.GetInstance().GetRock(targetTileRow, targetTileColumn, currentFloor.m_FloorIndex) != null)
				{
					return false;
				}
				if (FloorManager.GetInstance().GetTunnelBrace(targetTileRow, targetTileColumn, currentFloor.m_FloorIndex) != null)
				{
					return false;
				}
				FloorManager.Floor floor = FloorManager.GetInstance().UpAFloor(currentFloor);
				if (floor != currentFloor)
				{
					Hole hole = FloorManager.GetInstance().GetHole(targetTileRow + -1, targetTileColumn, floor.m_FloorIndex);
					if (hole != null && hole.IsFullyDug())
					{
						return false;
					}
				}
				return true;
			}
		}
		return false;
	}

	public override bool StartUsing(AnimState useAnimation, float useTime)
	{
		base.StartUsing(useAnimation, useTime);
		m_Animation = useAnimation;
		m_UseTime = useTime;
		if (base.ParentItem != null && m_Owner != null)
		{
			m_Owner.m_CharacterAnimator.StartAnimation(m_Animation);
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Timber_Brace, m_Owner.gameObject);
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
		if (m_ElapsedTime >= m_UseTime)
		{
			if (OnEndOfUse != null)
			{
				OnEndOfUse();
			}
			FinishUsing();
			return false;
		}
		return true;
	}

	public override bool CancelUsing()
	{
		base.CancelUsing();
		if (m_Owner != null)
		{
			m_Owner.m_CharacterAnimator.StopAnimation(m_Animation);
		}
		return true;
	}

	public void FinishUsing()
	{
		if (m_Owner != null)
		{
			m_Owner.m_CharacterAnimator.StopAnimation(m_Animation);
			FloorManager.GetInstance().PlaceTunnelBrace(m_Owner.CurrentFloor, m_Owner.GetTargetTileRow(), m_Owner.GetTargetTileColumn());
			if (base.ParentItem != null)
			{
				m_Owner.RemoveItemRPC(base.ParentItem, RPC_CallContexts.Unknown, release: true);
			}
		}
	}
}
