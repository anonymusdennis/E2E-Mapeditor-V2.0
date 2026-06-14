using System.Collections.Generic;
using SaveHelpers;
using UnityEngine;

public class DogFoodJob : BaseJob
{
	public ItemData m_FoodReceptacleItem;

	public ItemData m_DogFoodItem;

	public int m_NumSpareItemsProduced = 5;

	public bool m_bAreDispensersUsableOutsideJobtime;

	private List<DogBowl> m_DogBowls = new List<DogBowl>();

	private List<TransferItemsInteraction> m_ReceptacleDispensers = new List<TransferItemsInteraction>();

	private List<TransferItemsInteraction> m_FoodDispensers = new List<TransferItemsInteraction>();

	private int m_ReceptacleImmediateResponseId;

	private int m_FoodImmediateResponseId;

	private List<int> m_ReceptacleResponseIds = new List<int>();

	private List<int> m_FoodResponseIds = new List<int>();

	public int m_DogApprovalOnCompletion = 10;

	private const int NUM_BITS_PER_ENTRY = 16;

	private const int NUM_BITS_HEADER = 5;

	private const int NUM_BITS_FOR_DATA = 59;

	private const int MAX_ENTRIES_PER_LONG = 3;

	public override void Init(RoomBlob jobRoom)
	{
		base.Init(jobRoom);
		InitDogBowls();
		InitDispensers();
		RoutineManager.GetInstance().OnRoutineChanged += DogFoodJob_OnRoutineChanged;
	}

	protected override void OnDestroy()
	{
		for (int num = m_DogBowls.Count - 1; num >= 0; num--)
		{
			m_DogBowls[num].OnDogBowlFedEvent -= OnDogBowlFedEvent;
		}
		RoutineManager instance = RoutineManager.GetInstance();
		if (instance != null)
		{
			instance.OnRoutineChanged -= DogFoodJob_OnRoutineChanged;
		}
		base.OnDestroy();
	}

	private void InitDispensers()
	{
		ItemData[] itemTypesToTransfer = new ItemData[1] { m_FoodReceptacleItem };
		for (int num = base.RoomData.m_Dispensers.Count - 1; num >= 0; num--)
		{
			InteractiveObject interactiveObject = base.RoomData.m_Dispensers[num];
			if (interactiveObject != null)
			{
				TransferItemsInteraction component = interactiveObject.GetComponent<TransferItemsInteraction>();
				if (component != null)
				{
					m_ReceptacleDispensers.Add(component);
					SetupDispenser(component, itemTypesToTransfer);
				}
			}
		}
		ItemData[] itemTypesToTransfer2 = new ItemData[1] { m_DogFoodItem };
		for (int num2 = base.RoomData.m_BespokeJobObjects.Count - 1; num2 >= 0; num2--)
		{
			if (base.RoomData.m_BespokeJobObjects[num2] != null)
			{
				TransferItemsInteraction component2 = base.RoomData.m_BespokeJobObjects[num2].GetComponent<TransferItemsInteraction>();
				if (component2 != null)
				{
					m_FoodDispensers.Add(component2);
					SetupDispenser(component2, itemTypesToTransfer2);
				}
			}
		}
		if (m_FoodDispensers.Count == 0)
		{
			LogDesignerProblemToGoogle("No food dispensers were found");
		}
		if (m_ReceptacleDispensers.Count == 0)
		{
			LogDesignerProblemToGoogle("No food dispensers were found");
		}
	}

	private void SetupDispenser(TransferItemsInteraction tfi, ItemData[] itemTypesToTransfer)
	{
		tfi.SetTransferDirection(TransferItemsInteraction.TransferDirection.ToCharacter);
		tfi.SetTransferItemTypes(itemTypesToTransfer);
		tfi.SetCanBeUsedOutsideJobTime(m_bAreDispensersUsableOutsideJobtime);
		ItemContainer itemContainer = tfi.GetItemContainer();
		itemContainer.m_MaxHiddenSize = 0;
		itemContainer.m_MaxSize = GetNumItemsToSpawn();
	}

	private void InitDogBowls()
	{
		for (int num = base.RoomData.m_Collectors.Count - 1; num >= 0; num--)
		{
			InteractiveObject interactiveObject = base.RoomData.m_Collectors[num];
			if (interactiveObject != null)
			{
				DogBowl component = interactiveObject.GetComponent<DogBowl>();
				if (component != null)
				{
					m_DogBowls.Add(component);
					component.m_DogFood = m_DogFoodItem;
					component.m_FoodReceptacleItem = m_FoodReceptacleItem;
					ItemInteraction itemInteraction = component.GetItemInteraction();
					itemInteraction.SetTransferDirection(TransferItemsInteraction.TransferDirection.FromCharacter);
					itemInteraction.m_bTransferEquippedItemsOnly = true;
					component.SetCanBeUsedOutsideJobTime(canBeUsed: false);
					component.OnDogBowlFedEvent += OnDogBowlFedEvent;
				}
			}
		}
		if (m_QuotaTarget > m_DogBowls.Count)
		{
			LogDesignerProblemToGoogle("Not enough dog bowls (" + m_DogBowls.Count + ") to meet quota of " + m_QuotaTarget);
		}
	}

	private void DogFoodJob_OnRoutineChanged(RoutinesData.Routine oldRoutine, RoutinesData.Routine newRoutine, bool forceEnd)
	{
		if (newRoutine.m_BaseRoutineType == Routines.LightsOut)
		{
			for (int num = m_DogBowls.Count - 1; num >= 0; num--)
			{
				m_DogBowls[num].RPC_SetState(DogBowl.Stages.NoBowl, playEffects: false);
			}
			base.RequiresSerialization = true;
		}
	}

	public override void OnJobTimeStarted(bool isSaveRestore)
	{
		base.OnJobTimeStarted(isSaveRestore);
		if (!isSaveRestore && T17NetManager.IsMasterClient)
		{
			ResetItemContainers();
			AddItemsToDispensers();
		}
	}

	private void ResetItemContainers()
	{
		for (int num = m_FoodDispensers.Count - 1; num >= 0; num--)
		{
			m_FoodDispensers[num].GetItemContainer().RemoveAllItems(releaseToManager: true, exemptQuestItems: false);
		}
		for (int num2 = m_ReceptacleDispensers.Count - 1; num2 >= 0; num2--)
		{
			m_ReceptacleDispensers[num2].GetItemContainer().RemoveAllItems(releaseToManager: true, exemptQuestItems: false);
		}
		for (int num3 = m_DogBowls.Count - 1; num3 >= 0; num3--)
		{
			m_DogBowls[num3].RemoveAllItems();
		}
	}

	public override void OnJobTimeEnded()
	{
		base.OnJobTimeEnded();
	}

	private void AddItemsToDispensers()
	{
		if (m_FoodDispensers.Count != 0)
		{
			RequestItemCreation(GetNumItemsToSpawn(), m_DogFoodItem, m_FoodResponseIds, ref m_FoodImmediateResponseId);
		}
		if (m_ReceptacleDispensers.Count != 0)
		{
			RequestItemCreation(GetNumItemsToSpawn(), m_FoodReceptacleItem, m_ReceptacleResponseIds, ref m_ReceptacleImmediateResponseId);
		}
	}

	private void RequestItemCreation(int numItems, ItemData item, List<int> responseIds, ref int immediateResponseId)
	{
		for (int i = 0; i < numItems; i++)
		{
			responseIds.Add(ItemManager.GetInstance().AssignItemRPC(0, item.m_ItemDataID, OnItemMgrResponseAddToDispenser, ref immediateResponseId));
		}
	}

	private void OnItemMgrResponseAddToDispenser(Item item, int eventID)
	{
		if (item == null)
		{
			return;
		}
		ItemContainer itemContainer = null;
		if (eventID == m_FoodImmediateResponseId || m_FoodResponseIds.Contains(eventID))
		{
			itemContainer = GetUnfilledDispenser(m_FoodDispensers);
		}
		else if (eventID == m_ReceptacleImmediateResponseId || m_ReceptacleResponseIds.Contains(eventID))
		{
			itemContainer = GetUnfilledDispenser(m_ReceptacleDispensers);
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

	private ItemContainer GetUnfilledDispenser(List<TransferItemsInteraction> tfis)
	{
		ItemContainer result = null;
		int count = tfis.Count;
		for (int i = 0; i < tfis.Count; i++)
		{
			ItemContainer itemContainer = tfis[i].GetItemContainer();
			if (itemContainer.GetFreeSpaceCount() > 0)
			{
				result = itemContainer;
				break;
			}
		}
		return result;
	}

	private void OnDogBowlFedEvent(DogBowl sender)
	{
		if (!T17NetManager.IsMasterClient)
		{
			return;
		}
		if (base.QuotaAchieved + 1 == base.QuotaTarget && base.Employee != null && base.Employee.m_CharacterStats.m_bIsPlayer && m_Type == JobType.CanineCarer && NPCManager.GetInstance() != null && NPCManager.GetInstance().m_Doggies != null)
		{
			List<AICharacter> doggies = NPCManager.GetInstance().m_Doggies;
			for (int i = 0; i < doggies.Count; i++)
			{
				AICharacter aICharacter = doggies[i];
				if (!(aICharacter == null) && !(aICharacter.m_Character == null) && !(aICharacter.m_Character.m_CharacterOpinions == null))
				{
					aICharacter.m_Character.m_CharacterOpinions.IncreaseOpinionOf(base.Employee, m_DogApprovalOnCompletion);
				}
			}
		}
		IncrementQuotaAchieved();
	}

	private int GetNumItemsToSpawn()
	{
		return m_DogBowls.Count + m_NumSpareItemsProduced;
	}

	public List<TransferItemsInteraction> GetFoodDispensers()
	{
		return m_FoodDispensers;
	}

	protected override List<ItemData> OneTimeCalculateJobRelatedItems()
	{
		List<ItemData> list = new List<ItemData>();
		list.Add(m_DogFoodItem);
		list.Add(m_FoodReceptacleItem);
		return list;
	}

	public override void Deserialize(ulong[] jobData)
	{
		base.Deserialize(jobData);
		for (int i = 1; i < jobData.Length; i++)
		{
			BitField bitField = new BitField(jobData[i]);
			int uInt = (int)bitField.GetUInt(5);
			for (int j = 0; j < uInt; j++)
			{
				int netViewId = (int)bitField.GetUInt(12);
				DogBowl.Stages uInt2 = (DogBowl.Stages)bitField.GetUInt(4);
				DogBowl dogBowl = m_DogBowls.Find((DogBowl x) => x.m_NetViewID.viewID == netViewId);
				if (dogBowl != null)
				{
					dogBowl.RPC_SetState(uInt2, playEffects: false);
				}
			}
		}
	}

	public override List<ulong> Serialize()
	{
		List<ulong> list = base.Serialize();
		List<DogBowl> list2 = m_DogBowls.FindAll((DogBowl x) => x.GetStage() != DogBowl.Stages.NoBowl);
		int num = list2.Count;
		if (num == 0)
		{
			return list;
		}
		while (num > 0)
		{
			BitField bitField = new BitField();
			int num2 = Mathf.Min(3, num);
			bitField.Set(5, (uint)num2);
			for (int i = 0; i < num2; i++)
			{
				bitField.Set(12, (uint)list2[i].m_NetViewID.viewID);
				bitField.Set(4, (uint)list2[i].GetStage());
				num--;
			}
			list.Add((ulong)bitField);
		}
		return list;
	}
}
