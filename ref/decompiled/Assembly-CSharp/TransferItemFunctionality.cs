using UnityEngine;

[CreateAssetMenu(fileName = "Item Transfer Functionality", menuName = "Team17/Items/Functionalities/Create Item Transfer Functionality")]
public class TransferItemFunctionality : BaseItemFunctionality
{
	private AnimState m_Animation = AnimState.UseMed;

	private float m_AnimTime = 0.3f;

	private float m_AnimTimer;

	private Character owner;

	private static int m_ItemTransferLayerMask = -1;

	private const string STATIC_MAP_OBJECT = "StaticMapObject";

	private const string FENCE = "Fence";

	private ItemInteraction m_ItemInteraction;

	private int m_LastResolvedTileRow = -1;

	private int m_LastResolvedTileColumn = -1;

	public TransferItemsInteraction.TransferDirection m_TransferDirection = TransferItemsInteraction.TransferDirection.FromCharacter;

	public override void Init()
	{
		if (m_ItemTransferLayerMask == -1)
		{
			m_ItemTransferLayerMask = LayerMask.GetMask("StaticMapObject", "Fence");
		}
		m_ItemInteraction = null;
	}

	public override Functionality GetFunctionalityType()
	{
		return Functionality.ItemTransfer;
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
		if (base.ParentItem == null)
		{
			return false;
		}
		if (m_Owner != null)
		{
			int targetTileRow = m_Owner.GetTargetTileRow();
			int targetTileColumn = m_Owner.GetTargetTileColumn();
			if (m_LastResolvedTileRow != targetTileRow || m_LastResolvedTileColumn != targetTileColumn)
			{
				m_ItemInteraction = null;
			}
			if (targetTileRow != -1 && targetTileColumn != -1)
			{
				m_LastResolvedTileRow = targetTileRow;
				m_LastResolvedTileColumn = targetTileColumn;
				Vector3 worldPosition = Vector3.zero;
				if (FloorManager.GetInstance().GetTileCentrePosition(m_Owner.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, targetTileRow, targetTileColumn, out worldPosition))
				{
					int num = EscapistsRaycast.OverlapSphereNonAlloc(worldPosition, 0.4f, m_ItemTransferLayerMask);
					if (num > 0)
					{
						for (int i = 0; i < num; i++)
						{
							if (!(m_ItemInteraction == null))
							{
								break;
							}
							Collider collider = EscapistsRaycast.ColliderOverlapList[i];
							m_ItemInteraction = collider.GetComponent<ItemInteraction>();
						}
						if (m_ItemInteraction != null)
						{
							return m_ItemInteraction.IsEnabled() && m_ItemInteraction.IsItemTransferable(base.ParentItem, m_TransferDirection);
						}
					}
					else
					{
						m_ItemInteraction = null;
					}
				}
			}
		}
		return false;
	}

	public override bool StartUsing(AnimState useAnimation, float useTime)
	{
		base.StartUsing(useAnimation, useTime);
		if (base.ParentItem != null && m_Owner != null)
		{
			m_AnimTime = useTime;
			m_AnimTimer = 0f;
			m_Animation = useAnimation;
			owner = m_Owner;
			owner.UseEquippedItemRPC(owner.GetEquippedItem(), bUse: true);
			owner.m_CharacterAnimator.StartAnimation(m_Animation);
			owner.PauseMovement(useTime);
			if (OnStartOfUse != null)
			{
				OnStartOfUse();
			}
			if (OnEndOfUse != null)
			{
				OnEndOfUse();
			}
			TransferItem();
			return true;
		}
		return false;
	}

	public override bool UpdateUsing()
	{
		m_AnimTimer += UpdateManager.deltaTime;
		if (m_AnimTimer >= m_AnimTime)
		{
			if (owner != null)
			{
				owner.UseEquippedItemRPC(owner.GetEquippedItem(), bUse: false);
				owner.m_CharacterAnimator.StopAnimation(m_Animation);
			}
			owner = null;
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
		m_ItemInteraction = null;
		if (owner != null)
		{
			owner.UseEquippedItemRPC(owner.GetEquippedItem(), bUse: false);
			owner.m_CharacterAnimator.StopAnimation(m_Animation);
			owner = null;
		}
		return true;
	}

	public void TransferItem()
	{
		if (owner != null)
		{
			if (m_ItemInteraction != null)
			{
				m_ItemInteraction.TransferEquippedItem(owner);
			}
			m_ItemInteraction = null;
		}
	}

	public bool IsItemInteractionMyTarget(ItemInteraction itemInteraction)
	{
		if (m_ItemInteraction == null)
		{
			return false;
		}
		return object.ReferenceEquals(m_ItemInteraction, itemInteraction);
	}
}
