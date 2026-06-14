using System.Collections.Generic;

public class CarriedInputJob : BaseJob
{
	public HazardousCarryableObjectInteraction m_ObjectPrefabToDispense;

	public bool m_bCanDispensersBeUsedOutsideJobTime;

	public int m_NumberEquipmentItemsSpawned = 8;

	protected CarriedObjectDispenser[] m_CarriedObjectDispenser;

	private TransferItemsInteraction[] m_EquipmentDispensers;

	private int m_DispensedItemIndex;

	protected override void Awake()
	{
		base.Awake();
		if (JobsManager.GetJobCategory(m_Type) != JobCategory.CarriedInputJob)
		{
		}
		if (m_ObjectPrefabToDispense == null)
		{
			LogDesignerProblemToGoogle("We have no prefab to spawn. ObjectPrefabToDispense has not been set up");
		}
	}

	public override void Init(RoomBlob jobRoom)
	{
		base.Init(jobRoom);
		m_ObjectPrefabToDispense.m_Decoration = CarryObjectInteraction.AI_Decorations.Job;
		PreSetupDispenser();
		SetupDispensers();
	}

	protected virtual void PreSetupDispenser()
	{
	}

	protected virtual void SetupDispensers()
	{
		List<uint> possibleSpawnTags = GetPossibleSpawnTags();
		List<CarriedObjectDispenser> list = new List<CarriedObjectDispenser>();
		List<TransferItemsInteraction> list2 = new List<TransferItemsInteraction>();
		ItemData[] transferItemTypes = m_ObjectPrefabToDispense.m_RequiredItems.ToArray();
		for (int num = base.RoomData.m_Dispensers.Count - 1; num >= 0; num--)
		{
			if (!(base.RoomData.m_Dispensers[num] == null))
			{
				CarriedObjectDispenser carriedObjectDispenser = base.RoomData.m_Dispensers[num] as CarriedObjectDispenser;
				TransferItemsInteraction transferItemsInteraction = base.RoomData.m_Dispensers[num] as TransferItemsInteraction;
				if (!(carriedObjectDispenser == null) || !(transferItemsInteraction == null))
				{
					if (transferItemsInteraction != null)
					{
						list2.Add(transferItemsInteraction);
						transferItemsInteraction.SetTransferDirection(TransferItemsInteraction.TransferDirection.ToCharacter);
						transferItemsInteraction.SetTransferItemTypes(transferItemTypes);
						transferItemsInteraction.GetItemContainer().m_MaxSize = m_NumberEquipmentItemsSpawned;
						transferItemsInteraction.SetCanBeUsedOutsideJobTime(m_bCanDispensersBeUsedOutsideJobTime);
					}
					else if (carriedObjectDispenser != null)
					{
						list.Add(carriedObjectDispenser);
						carriedObjectDispenser.SetCanBeUsedOutsideJobTime(m_bCanDispensersBeUsedOutsideJobTime);
						carriedObjectDispenser.SetDispensedObject(m_ObjectPrefabToDispense.gameObject, m_ObjectPrefabToDispense.m_RequiredItems, m_ObjectPrefabToDispense.m_NoEquipmentSpeech, possibleSpawnTags);
						carriedObjectDispenser.JobManager_Init();
					}
				}
			}
		}
		m_CarriedObjectDispenser = list.ToArray();
		m_EquipmentDispensers = list2.ToArray();
		if (m_CarriedObjectDispenser.Length == 0)
		{
			LogDesignerProblemToGoogle("No valid dispensers found, job will not function");
		}
		if (m_EquipmentDispensers.Length == 0 && m_ObjectPrefabToDispense.m_RequiredItems.Count > 0)
		{
			LogDesignerProblemToGoogle("No transfer item interaction in the dispenser list set up to dispense the required items needed for this carried object");
		}
		if (!PrisonSnapshotIO.IsThereSaveData())
		{
			FillDispensers();
		}
	}

	public override void OnJobTimeStarted(bool isSaveRestore)
	{
		base.OnJobTimeStarted(isSaveRestore);
		if (!isSaveRestore)
		{
			FillDispensers();
			for (int i = 0; i < m_CarriedObjectDispenser.Length; i++)
			{
				m_CarriedObjectDispenser[i].ReleaseAllActiveObjectsWithEffect();
			}
		}
	}

	protected virtual List<uint> GetPossibleSpawnTags()
	{
		return null;
	}

	private void CarryableConsumer_AcceptedInputEvent(CarryableObjectConsumer consumer, CarryObjectInteraction theObject)
	{
		for (int i = 0; i < m_CarriedObjectDispenser.Length; i++)
		{
			m_CarriedObjectDispenser[i].AddObjectBackToSpawnPool(theObject);
		}
		base.RequiresSerialization = true;
	}

	private void FillDispensers()
	{
		if ((!T17NetManager.OfflineMode && (!T17NetManager.NetOnlineMode || !T17NetManager.IsMasterClient)) || m_ObjectPrefabToDispense.m_RequiredItems.Count == 0 || m_EquipmentDispensers.Length <= 0)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < m_EquipmentDispensers.Length; i++)
		{
			num += m_EquipmentDispensers[i].GetItemContainer().GetFreeSpaceCount();
		}
		m_ItemMgrResponseIDs.Clear();
		for (int j = 0; j < num; j++)
		{
			ItemData itemData = null;
			for (int k = 0; k < m_ObjectPrefabToDispense.m_RequiredItems.Count; k++)
			{
				if (!(itemData == null))
				{
					break;
				}
				itemData = m_ObjectPrefabToDispense.m_RequiredItems[m_DispensedItemIndex];
				if (++m_DispensedItemIndex >= m_ObjectPrefabToDispense.m_RequiredItems.Count)
				{
					m_DispensedItemIndex = 0;
				}
			}
			if (itemData == null)
			{
				break;
			}
			m_ItemMgrResponseIDs.Add(ItemManager.GetInstance().AssignItemRPC(0, itemData.m_ItemDataID, OnItemMgrResponseAddToDispenser, ref m_ImmediateItemMgrResponseID));
		}
	}

	private void OnItemMgrResponseAddToDispenser(Item item, int eventID)
	{
		if (!(item != null) || (eventID != m_ImmediateItemMgrResponseID && !m_ItemMgrResponseIDs.Contains(eventID)))
		{
			return;
		}
		ItemContainer itemContainer = null;
		for (int i = 0; i < m_EquipmentDispensers.Length; i++)
		{
			if (m_EquipmentDispensers[i].GetItemContainer().GetFreeSpaceCount() > 0)
			{
				itemContainer = m_EquipmentDispensers[i].GetItemContainer();
				break;
			}
		}
		if (itemContainer != null)
		{
			if (!itemContainer.AddItemRPC(item))
			{
				ItemManager.GetInstance().RequestReleaseItem(item);
			}
		}
		else
		{
			ItemManager.GetInstance().RequestReleaseItem(item);
		}
	}

	protected override List<ItemData> OneTimeCalculateJobRelatedItems()
	{
		if (m_ObjectPrefabToDispense != null)
		{
			return m_ObjectPrefabToDispense.m_RequiredItems;
		}
		return null;
	}
}
