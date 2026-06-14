using System.Collections.Generic;
using SaveHelpers;
using UnityEngine;

public class RelightTorchesJob : BaseJob
{
	public ItemData m_FuelItem;

	public ItemData m_LighterItem;

	public int m_NumLighterItemsProduced = 3;

	public int m_NumFuelItemsProduced = 3;

	public bool m_bAreDispensersUsableOutsideJobtime;

	private List<WallTorch> m_WallTorches = new List<WallTorch>();

	private List<TransferItemsInteraction> m_Dispensers = new List<TransferItemsInteraction>();

	private int m_DispenserImmediateResponseId;

	private List<int> m_DispenserResponseIds = new List<int>();

	private const int NUM_BITS_PER_ENTRY = 16;

	private const int NUM_BITS_HEADER = 5;

	private const int NUM_BITS_FOR_DATA = 59;

	private const int MAX_ENTRIES_PER_LONG = 3;

	public override void Init(RoomBlob jobRoom)
	{
		base.Init(jobRoom);
		InitTorches();
		InitDispensers();
		RoutineManager.GetInstance().OnRoutineChanged += RelightTorchJob_OnRoutineChanged;
	}

	protected override void OnDestroy()
	{
		for (int num = m_WallTorches.Count - 1; num >= 0; num--)
		{
			m_WallTorches[num].OnWallTorchLitEvent -= OnWallTorchLitEvent;
		}
		RoutineManager instance = RoutineManager.GetInstance();
		if (instance != null)
		{
			instance.OnRoutineChanged -= RelightTorchJob_OnRoutineChanged;
		}
		base.OnDestroy();
	}

	private void InitDispensers()
	{
		ItemData[] itemTypesToTransfer = new ItemData[2] { m_FuelItem, m_LighterItem };
		for (int num = base.RoomData.m_Dispensers.Count - 1; num >= 0; num--)
		{
			InteractiveObject interactiveObject = base.RoomData.m_Dispensers[num];
			if (interactiveObject != null)
			{
				TransferItemsInteraction component = interactiveObject.GetComponent<TransferItemsInteraction>();
				if (component != null)
				{
					m_Dispensers.Add(component);
					SetupDispenser(component, itemTypesToTransfer);
				}
			}
		}
		if (m_Dispensers.Count == 0)
		{
			LogDesignerProblemToGoogle("No fuel + lighter dispensers were found");
		}
	}

	private void SetupDispenser(TransferItemsInteraction tfi, ItemData[] itemTypesToTransfer)
	{
		tfi.SetTransferDirection(TransferItemsInteraction.TransferDirection.ToCharacter);
		tfi.SetTransferItemTypes(itemTypesToTransfer);
		tfi.SetCanBeUsedOutsideJobTime(m_bAreDispensersUsableOutsideJobtime);
		ItemContainer itemContainer = tfi.GetItemContainer();
		itemContainer.m_MaxHiddenSize = 0;
		itemContainer.m_MaxSize = m_NumLighterItemsProduced + m_NumFuelItemsProduced;
	}

	private void InitTorches()
	{
		for (int num = base.RoomData.m_Collectors.Count - 1; num >= 0; num--)
		{
			InteractiveObject interactiveObject = base.RoomData.m_Collectors[num];
			if (interactiveObject != null)
			{
				WallTorch component = interactiveObject.GetComponent<WallTorch>();
				if (component != null)
				{
					m_WallTorches.Add(component);
					component.m_LighterItem = m_LighterItem;
					component.m_FuelItem = m_FuelItem;
					component.SetCanBeUsedOutsideJobTime(canBeUsed: false);
					component.OnWallTorchLitEvent += OnWallTorchLitEvent;
				}
			}
		}
		if (m_QuotaTarget > m_WallTorches.Count)
		{
			LogDesignerProblemToGoogle("Not enough torches (" + m_WallTorches.Count + ") to meet quota of " + m_QuotaTarget);
		}
	}

	private void RelightTorchJob_OnRoutineChanged(RoutinesData.Routine oldRoutine, RoutinesData.Routine newRoutine, bool forceEnd)
	{
		if (newRoutine.m_BaseRoutineType == Routines.LightsOut)
		{
			for (int num = m_WallTorches.Count - 1; num >= 0; num--)
			{
				m_WallTorches[num].RPC_SetTorchState(WallTorch.TorchState.UnfueledTorch);
			}
		}
	}

	public override void OnJobTimeStarted(bool isSaveRestore)
	{
		base.OnJobTimeStarted(isSaveRestore);
		if (!isSaveRestore && T17NetManager.IsMasterClient)
		{
			ResetAll();
			AddItemsToDispensers();
		}
	}

	private void ResetAll()
	{
		for (int num = m_Dispensers.Count - 1; num >= 0; num--)
		{
			m_Dispensers[num].GetItemContainer().RemoveAllItems(releaseToManager: true, exemptQuestItems: false);
		}
		for (int num2 = m_WallTorches.Count - 1; num2 >= 0; num2--)
		{
			m_WallTorches[num2].SetStateRPC(WallTorch.TorchState.UnfueledTorch);
		}
	}

	public override void OnJobTimeEnded()
	{
		base.OnJobTimeEnded();
	}

	private void AddItemsToDispensers()
	{
		if (m_Dispensers.Count != 0)
		{
			RequestItemCreation(m_NumFuelItemsProduced, m_FuelItem, m_DispenserResponseIds, ref m_DispenserImmediateResponseId);
			RequestItemCreation(m_NumLighterItemsProduced, m_LighterItem, m_DispenserResponseIds, ref m_DispenserImmediateResponseId);
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
		if (eventID == m_DispenserImmediateResponseId || m_DispenserResponseIds.Contains(eventID))
		{
			itemContainer = GetUnfilledDispenser(m_Dispensers);
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

	private void OnWallTorchLitEvent(WallTorch sender)
	{
		if (T17NetManager.IsMasterClient)
		{
			IncrementQuotaAchieved();
		}
		StartDelayedRecalculteRoutineInformation();
	}

	public List<TransferItemsInteraction> GetItemDispensers()
	{
		return m_Dispensers;
	}

	public List<WallTorch> GetWallTorches()
	{
		return m_WallTorches;
	}

	protected override List<ItemData> OneTimeCalculateJobRelatedItems()
	{
		List<ItemData> list = new List<ItemData>();
		list.Add(m_LighterItem);
		list.Add(m_FuelItem);
		return list;
	}

	public override void SetRoutineInformationForCharacter(Character character)
	{
		if (character == null)
		{
			return;
		}
		List<WallTorch> list = m_WallTorches.FindAll((WallTorch x) => x.GetTorchState() != WallTorch.TorchState.LitTorch);
		if (list.Count == 0)
		{
			if (character.m_CharacterStats.m_bIsPlayer)
			{
				Player player = (Player)character;
				player.SetRoutineArrowTarget((RoomBlob)null);
			}
			return;
		}
		if (!character.HasItemOnPerson(m_FuelItem) || !character.HasItemOnPerson(m_LighterItem))
		{
			ItemContainer itemContainer = m_Dispensers[Random.Range(0, m_Dispensers.Count)].GetItemContainer();
			if (character.m_CharacterStats.m_bIsPlayer)
			{
				Player player2 = (Player)character;
				player2.SetRoutineArrowTarget(itemContainer.NetView);
				return;
			}
		}
		WallTorch wallTorch = FindClosestInList(character, list);
		if (character.m_CharacterStats.m_bIsPlayer)
		{
			Player player3 = (Player)character;
			player3.SetRoutineArrowTarget(wallTorch.m_NetViewID);
		}
	}

	private WallTorch FindClosestInList(Character character, List<WallTorch> torches)
	{
		WallTorch wallTorch = torches[torches.Count - 1];
		float num = GetMovementCostToInteraction(wallTorch, character);
		for (int num2 = torches.Count - 2; num2 >= 0; num2--)
		{
			WallTorch wallTorch2 = torches[num2];
			float movementCostToInteraction = GetMovementCostToInteraction(wallTorch2, character);
			if (movementCostToInteraction < num)
			{
				num = movementCostToInteraction;
				wallTorch = wallTorch2;
			}
		}
		return wallTorch;
	}

	private float GetMovementCostToInteraction(WallTorch interaction, Character character)
	{
		float sqrMagnitude = (interaction.transform.position - character.transform.position).sqrMagnitude;
		float num = interaction.transform.position.z - character.transform.position.z;
		return sqrMagnitude * (num / (float)FloorManager.GetInstance().m_FloorOffset);
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
				WallTorch.TorchState uInt2 = (WallTorch.TorchState)bitField.GetUInt(4);
				WallTorch wallTorch = m_WallTorches.Find((WallTorch x) => x.m_NetViewID.viewID == netViewId);
				if (wallTorch != null)
				{
					wallTorch.RPC_SetTorchState(uInt2);
				}
			}
		}
	}

	public override List<ulong> Serialize()
	{
		List<ulong> list = base.Serialize();
		List<WallTorch> list2 = m_WallTorches.FindAll((WallTorch x) => x.GetTorchState() != WallTorch.TorchState.UnfueledTorch);
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
				bitField.Set(4, (uint)list2[i].GetTorchState());
				num--;
			}
			list.Add((ulong)bitField);
		}
		return list;
	}
}
