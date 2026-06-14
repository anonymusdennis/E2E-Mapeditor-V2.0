using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

[CreateAssetMenu(fileName = "Sharpening Functionality", menuName = "Team17/Items/Functionalities/Create Sharpening Functionality")]
public class SharpeningFunctionality : BaseItemFunctionality
{
	private AnimState m_Animation = AnimState.PickaxeUse;

	private float m_ChipTime = 1f;

	private float m_ElapsedChipTime;

	public ItemData m_SharpenedItemData;

	private int m_SharpenedItemID;

	private ItemContainer m_Container;

	private Character m_CacheOwner;

	public override Functionality GetFunctionalityType()
	{
		return Functionality.Sharpen;
	}

	public override void Init()
	{
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
		if (base.ParentItem == null || base.ParentItem.m_ItemData == null)
		{
			return false;
		}
		if (m_SharpenedItemData == null)
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
				return FloorManager.GetInstance().CanSharpenOnTile(m_Owner.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Wall, targetTileRow, targetTileColumn);
			}
		}
		return false;
	}

	public override bool StartUsing(AnimState useAnimation, float useTime)
	{
		base.StartUsing(useAnimation, useTime);
		if (base.ParentItem != null && base.ParentItem.m_ItemData != null && m_Owner != null)
		{
			int targetTileRow = m_Owner.GetTargetTileRow();
			int targetTileColumn = m_Owner.GetTargetTileColumn();
			FloorManager.Floor currentFloor = m_Owner.CurrentFloor;
			if (targetTileRow != -1 && targetTileColumn != -1)
			{
				m_ChipTime = useTime;
				m_ElapsedChipTime = 0f;
				m_Animation = useAnimation;
				m_Owner.UseEquippedItemRPC(m_Owner.GetEquippedItem(), bUse: true);
				m_Owner.m_CharacterAnimator.StartAnimation(m_Animation);
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Chip, m_Owner.gameObject);
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
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_Player_Chip, m_Owner.gameObject);
		}
		return true;
	}

	public void FinishUsing()
	{
		if (m_Owner != null && m_ParentItem != null && m_ParentItem.m_ItemData != null)
		{
			m_Owner.UseEquippedItemRPC(m_Owner.GetEquippedItem(), bUse: false);
			m_Owner.m_CharacterAnimator.StopAnimation(m_Animation);
			m_CacheOwner = m_Owner;
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_Player_Chip, m_Owner.gameObject);
			int ownerId = m_Owner.m_NetView.ownerId;
			m_Container = m_Owner.m_ItemContainer;
			if (m_Owner.GetEquippedItem() == base.ParentItem)
			{
				m_Owner.SetEquippedItem(null);
			}
			ItemManager.GetInstance().AssignItemRPC(ownerId, m_SharpenedItemData.m_ItemDataID, OnSharpenedItemSpawn, ref m_SharpenedItemID);
		}
	}

	private void OnSharpenedItemSpawn(Item item, int eventID)
	{
		if (eventID == m_SharpenedItemID && m_Container != null)
		{
			m_Container.RemoveItemRPC(base.ParentItem);
			if (m_CacheOwner != null && m_CacheOwner.CanEquipItem(item))
			{
				m_CacheOwner.SetEquippedItem(item);
			}
			else
			{
				m_Container.LOCAL_AddItemToContainer(item);
			}
			m_Container = null;
		}
	}
}
