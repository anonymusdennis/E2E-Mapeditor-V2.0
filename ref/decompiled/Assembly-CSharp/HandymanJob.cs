using System.Collections.Generic;
using SaveHelpers;
using UnityEngine;

[RequireComponent(typeof(T17NetView))]
public class HandymanJob : BaseJob
{
	[Header("Minigame Requirements")]
	public List<ItemData> m_MinigameRequirements;

	public ItemData m_MinigameRequiredEquippedItem;

	[Header("Item Setups")]
	public List<ItemData> m_CraftableItemRequirements;

	public ItemData[] m_DispensedStartingItems;

	[Header("Dispenser setup")]
	public int m_NumItemsPerDispenser = 8;

	public bool m_bDispensersUsableOutsideJobTime;

	[Header("Minigame setup")]
	public int m_NumRepsForMinigameComplete = 2;

	public int m_TimeForAutoCompletion = 5;

	private T17NetView m_NetView;

	protected List<HandymanInteraction> m_PossibleInteractions = new List<HandymanInteraction>();

	protected List<HandymanInteraction> m_ActiveInteractions = new List<HandymanInteraction>();

	private List<ItemContainer> m_DispenserContainers = new List<ItemContainer>();

	private List<HandymanInteraction> m_FixedInteractions = new List<HandymanInteraction>();

	private int m_DispensedItemIndex;

	private const int NUM_BITS_PER_ENTRY = 12;

	private const int NUM_BITS_HEADER = 5;

	private const int NUM_BITS_FOR_DATA = 59;

	private const int MAX_ENTRIES_PER_LONG = 4;

	protected override void Awake()
	{
		base.Awake();
		if (JobsManager.GetJobCategory(m_Type) != JobCategory.Repair)
		{
		}
		m_NetView = GetComponent<T17NetView>();
		m_NetView.viewID = JobsManager.GetInstance().TakeAReservedIdForJobs();
	}

	public override void Init(RoomBlob jobRoom)
	{
		base.Init(jobRoom);
		SetupDispensers();
		GlobalStart.TimedNetworkService();
		InitInteractions();
		GlobalStart.TimedNetworkService();
	}

	protected override void OnDestroy()
	{
		for (int num = m_PossibleInteractions.Count - 1; num >= 0; num--)
		{
			m_PossibleInteractions[num].FixedEvent -= Local_OnInteraction_FixedEvent;
		}
		m_NetView = null;
		base.OnDestroy();
	}

	private void SetupDispensers()
	{
		for (int i = 0; i < base.RoomData.m_Dispensers.Count; i++)
		{
			if (!(base.RoomData.m_Dispensers[i] == null))
			{
				ItemContainer component = base.RoomData.m_Dispensers[i].GetComponent<ItemContainer>();
				if (component != null)
				{
					component.m_MaxSize = m_NumItemsPerDispenser;
					component.m_MaxHiddenSize = 0;
					m_DispenserContainers.Add(component);
				}
				TransferItemsInteraction component2 = base.RoomData.m_Dispensers[i].GetComponent<TransferItemsInteraction>();
				if (component2 != null)
				{
					component2.SetTransferDirection(TransferItemsInteraction.TransferDirection.ToCharacter);
					component2.SetTransferItemTypes(m_DispensedStartingItems);
					component2.SetCanBeUsedOutsideJobTime(m_bDispensersUsableOutsideJobTime);
				}
			}
		}
	}

	public void SetupProcessors()
	{
		base.RoomData.m_Processors.Clear();
		for (int i = 0; i < m_ActiveInteractions.Count; i++)
		{
			if (m_ActiveInteractions[i] != null)
			{
				base.RoomData.m_Processors.Add(m_ActiveInteractions[i]);
			}
		}
	}

	private void InitInteractions()
	{
		m_PossibleInteractions.Clear();
		m_PossibleInteractions = SearchForAllPossibleInteractions();
		for (int num = m_PossibleInteractions.Count - 1; num >= 0; num--)
		{
			InitInteraction(m_PossibleInteractions[num]);
		}
		if (m_PossibleInteractions.Count == 0 || m_QuotaTarget == 0)
		{
			LogDesignerProblemToGoogle("Job has no things to interact with in level");
		}
	}

	protected virtual void InitInteraction(HandymanInteraction interaction)
	{
		interaction.m_NumRepsForCompletion = m_NumRepsForMinigameComplete;
		interaction.m_RequiredItemsForInteractions = m_MinigameRequirements;
		interaction.m_EquippedItemRequirement = m_MinigameRequiredEquippedItem;
		interaction.m_TimeForAutoCompletion = m_TimeForAutoCompletion;
		interaction.FixedEvent += Local_OnInteraction_FixedEvent;
	}

	protected virtual List<HandymanInteraction> SearchForAllPossibleInteractions()
	{
		HandymanInteraction[] array = Object.FindObjectsOfType<HandymanInteraction>();
		List<HandymanInteraction> list = new List<HandymanInteraction>();
		foreach (HandymanInteraction handymanInteraction in array)
		{
			if (handymanInteraction.m_JobType == m_Type)
			{
				list.Add(handymanInteraction);
			}
		}
		return list;
	}

	public override void OnJobTimeStarted(bool isSaveRestore)
	{
		base.OnJobTimeStarted(isSaveRestore);
		if (!isSaveRestore)
		{
			FillDispensers();
			m_FixedInteractions.Clear();
			if (T17NetManager.IsMasterClient)
			{
				Master_PickInteractionsAndNotifyRPC();
			}
		}
		else
		{
			SetAllInteractionsForJobTimeActive(m_ActiveInteractions, state: true, false);
		}
		SetRoutineInformationForCharacter(base.Employee);
		SetupProcessors();
	}

	private void Master_PickInteractionsAndNotifyRPC()
	{
		if (m_QuotaTarget > m_PossibleInteractions.Count)
		{
			LogDesignerProblemToGoogle("There aren't enough possible interactions for " + m_Type.ToString() + " to meet a quota of " + m_QuotaTarget);
		}
		int num = Mathf.Min(m_QuotaTarget, m_PossibleInteractions.Count);
		int[] array = new int[num];
		List<HandymanInteraction> list = new List<HandymanInteraction>(m_PossibleInteractions);
		for (int i = 0; i < num; i++)
		{
			HandymanInteraction handymanInteraction = list[Random.Range(0, list.Count)];
			array[i] = handymanInteraction.m_NetViewID.viewID;
			list.Remove(handymanInteraction);
		}
		m_NetView.PostLevelLoadRPC("RPCAll_InteractionsChosen", NetTargets.All, array);
	}

	protected virtual List<HandymanInteraction> CopyPossibleInteractionsForNewJobTime()
	{
		return new List<HandymanInteraction>(m_PossibleInteractions);
	}

	[PunRPC]
	public void RPCAll_InteractionsChosen(int[] netViewIds)
	{
		if (netViewIds.Length == 0)
		{
		}
		m_ActiveInteractions.Clear();
		for (int i = 0; i < netViewIds.Length; i++)
		{
			HandymanInteraction handymanInteraction = T17NetView.Find<HandymanInteraction>(netViewIds[i]);
			if (handymanInteraction != null)
			{
				m_ActiveInteractions.Add(handymanInteraction);
				handymanInteraction.Local_ResetReps();
				SetInteractionForJobTimeActive(handymanInteraction, state: true, false);
			}
		}
		if (m_ActiveInteractions.Count != 0)
		{
		}
	}

	public override void OnJobTimeEnded()
	{
		base.OnJobTimeEnded();
		for (int num = m_ActiveInteractions.Count - 1; num >= 0; num--)
		{
			Character localInteractingCharacter = m_ActiveInteractions[num].GetLocalInteractingCharacter();
			if (localInteractingCharacter != null)
			{
				m_ActiveInteractions[num].RequestStopInteraction(localInteractingCharacter);
			}
		}
		SetAllInteractionsForJobTimeActive(m_ActiveInteractions, state: false, null);
		m_ActiveInteractions.Clear();
	}

	protected void SetAllInteractionsForJobTimeActive(List<HandymanInteraction> interactions, bool state, bool? gotFixed)
	{
		for (int num = interactions.Count - 1; num >= 0; num--)
		{
			SetInteractionForJobTimeActive(interactions[num], state, gotFixed);
		}
	}

	protected virtual void SetInteractionForJobTimeActive(HandymanInteraction interaction, bool state, bool? gotFixed)
	{
		interaction.Local_SetForJobTimeActive(state, gotFixed);
		base.RequiresSerialization = true;
	}

	private void Local_OnInteraction_FixedEvent(HandymanInteraction sender, Character interactingCharacter)
	{
		IncrementQuotaAchieved();
		int num = -1;
		int num2 = -1;
		if (sender == null || sender.m_NetViewID == null)
		{
			if (!(sender != null))
			{
			}
		}
		else
		{
			num = sender.m_NetViewID.viewID;
		}
		if (interactingCharacter == null || interactingCharacter.m_NetView == null)
		{
			if (!(interactingCharacter != null))
			{
			}
		}
		else
		{
			num2 = interactingCharacter.m_NetView.viewID;
		}
		m_NetView.PostLevelLoadRPC("RPC_InteractionFixed", NetTargets.All, num, num2);
	}

	[PunRPC]
	protected void RPC_InteractionFixed(int interactionViewId, int fixerCharacterId)
	{
		HandymanInteraction handymanInteraction = T17NetView.Find<HandymanInteraction>(interactionViewId);
		Character character = T17NetView.Find<Character>(fixerCharacterId);
		if (handymanInteraction == null)
		{
		}
		if (character == null)
		{
		}
		All_OnInteractionFixed(handymanInteraction, character);
	}

	protected virtual void All_OnInteractionFixed(HandymanInteraction fixedInteraction, Character fixer)
	{
		if (fixedInteraction != null)
		{
			m_ActiveInteractions.Remove(fixedInteraction);
			if (fixer != null)
			{
				SetRoutineInformationForCharacter(base.Employee);
			}
			m_FixedInteractions.Add(fixedInteraction);
		}
		base.RequiresSerialization = true;
	}

	private void FillDispensers()
	{
		if ((!T17NetManager.OfflineMode && (!T17NetManager.NetOnlineMode || !T17NetManager.IsMasterClient)) || m_DispensedStartingItems == null || m_DispensedStartingItems.Length <= 0 || m_DispenserContainers.Count <= 0)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < m_DispenserContainers.Count; i++)
		{
			num += m_DispenserContainers[i].GetFreeSpaceCount();
		}
		m_ItemMgrResponseIDs.Clear();
		for (int j = 0; j < num; j++)
		{
			ItemData itemData = null;
			for (int k = 0; k < m_DispensedStartingItems.Length; k++)
			{
				if (!(itemData == null))
				{
					break;
				}
				itemData = m_DispensedStartingItems[m_DispensedItemIndex];
				if (++m_DispensedItemIndex >= m_DispensedStartingItems.Length)
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
		for (int i = 0; i < m_DispenserContainers.Count; i++)
		{
			if (m_DispenserContainers[i].GetFreeSpaceCount() > 0)
			{
				itemContainer = m_DispenserContainers[i];
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

	public override void SetRoutineInformationForCharacter(Character character)
	{
		if (character == null)
		{
			return;
		}
		if (m_ActiveInteractions.Count > 0)
		{
			if (!character.HasItemsOnPerson(m_MinigameRequirements))
			{
				ItemContainer itemContainer = m_DispenserContainers[Random.Range(0, m_DispenserContainers.Count)];
				if (character.m_CharacterStats.m_bIsPlayer)
				{
					Player player = (Player)character;
					player.SetRoutineArrowTarget(itemContainer.NetView);
				}
				return;
			}
			HandymanInteraction handymanInteraction = m_ActiveInteractions[m_ActiveInteractions.Count - 1];
			float num = GetMovementCostToInteraction(handymanInteraction, character);
			for (int num2 = m_ActiveInteractions.Count - 2; num2 >= 0; num2--)
			{
				HandymanInteraction handymanInteraction2 = m_ActiveInteractions[num2];
				float movementCostToInteraction = GetMovementCostToInteraction(handymanInteraction2, character);
				if (movementCostToInteraction < num)
				{
					num = movementCostToInteraction;
					handymanInteraction = handymanInteraction2;
				}
			}
			if (character.m_CharacterStats.m_bIsPlayer)
			{
				Player player2 = (Player)character;
				player2.SetRoutineArrowTarget(handymanInteraction.m_NetViewID);
			}
		}
		else if (character.m_CharacterStats.m_bIsPlayer)
		{
			Player player3 = (Player)character;
			player3.SetRoutineArrowTarget((RoomBlob)null);
		}
	}

	private float GetMovementCostToInteraction(HandymanInteraction interaction, Character character)
	{
		float sqrMagnitude = (interaction.transform.position - character.transform.position).sqrMagnitude;
		float num = interaction.transform.position.z - character.transform.position.z;
		return sqrMagnitude * (num / (float)FloorManager.GetInstance().m_FloorOffset);
	}

	protected override List<ItemData> OneTimeCalculateJobRelatedItems()
	{
		List<ItemData> list = new List<ItemData>(m_MinigameRequirements);
		list.AddRange(m_CraftableItemRequirements);
		list.AddRange(m_DispensedStartingItems);
		return list;
	}

	public override bool DoesEmployeeHaveToReportToJobRoom()
	{
		return !base.Employee.HasItemsOnPerson(m_MinigameRequirements);
	}

	public override void Deserialize(ulong[] jobData)
	{
		switch (m_DeserialiseDataVersion)
		{
		case 0:
			Deserialize_Version_0(jobData);
			break;
		case 1:
			Deserialise_Version_1(jobData);
			break;
		}
	}

	private void Deserialize_Version_0(ulong[] jobData)
	{
		base.Deserialize(jobData);
		for (int i = 1; i < jobData.Length; i++)
		{
			BitField bitField = new BitField(jobData[i]);
			int uInt = (int)bitField.GetUInt(5);
			for (int j = 0; j < uInt; j++)
			{
				int uInt2 = (int)bitField.GetUInt(12);
				HandymanInteraction handymanInteraction = T17NetView.Find<HandymanInteraction>(uInt2);
				if (handymanInteraction != null)
				{
					m_ActiveInteractions.Add(handymanInteraction);
					SetInteractionForJobTimeActive(handymanInteraction, state: true, null);
				}
			}
		}
	}

	private void Deserialise_Version_1(ulong[] jobData)
	{
		base.Deserialize(jobData);
		int num = 1;
		BitField bitField = new BitField(jobData[num++]);
		int uInt = (int)bitField.GetUInt(5);
		int uInt2 = (int)bitField.GetUInt(5);
		for (int i = 0; i < uInt; i++)
		{
			BitField bitField2 = new BitField(jobData[num++]);
			int uInt3 = (int)bitField2.GetUInt(5);
			for (int j = 0; j < uInt3; j++)
			{
				int uInt4 = (int)bitField2.GetUInt(12);
				HandymanInteraction handymanInteraction = T17NetView.Find<HandymanInteraction>(uInt4);
				if (handymanInteraction != null)
				{
					m_ActiveInteractions.Add(handymanInteraction);
					SetInteractionForJobTimeActive(handymanInteraction, state: true, false);
				}
			}
		}
		for (int k = 0; k < uInt2; k++)
		{
			BitField bitField3 = new BitField(jobData[num++]);
			int uInt5 = (int)bitField3.GetUInt(5);
			for (int l = 0; l < uInt5; l++)
			{
				int uInt6 = (int)bitField3.GetUInt(12);
				HandymanInteraction handymanInteraction2 = T17NetView.Find<HandymanInteraction>(uInt6);
				if (handymanInteraction2 != null)
				{
					m_FixedInteractions.Add(handymanInteraction2);
					handymanInteraction2.Local_SetGotFixed(gotFixed: true);
				}
			}
		}
		if (uInt2 > 0)
		{
			base.RequiresSerialization = true;
		}
	}

	public override List<ulong> Serialize()
	{
		List<ulong> saveDataSoFar = base.Serialize();
		BitField bitField = new BitField();
		int count = saveDataSoFar.Count;
		int uValue = SerialiseHandymanList(ref saveDataSoFar, m_ActiveInteractions);
		bitField.Set(5, (uint)uValue);
		uValue = SerialiseHandymanList(ref saveDataSoFar, m_FixedInteractions);
		bitField.Set(5, (uint)uValue);
		saveDataSoFar.Insert(count, (ulong)bitField);
		return saveDataSoFar;
	}

	private int SerialiseHandymanList(ref List<ulong> saveDataSoFar, List<HandymanInteraction> listToSerialise)
	{
		int num = listToSerialise.Count;
		int num2 = 0;
		if (num == 0)
		{
		}
		while (num > 0)
		{
			BitField bitField = new BitField();
			num2++;
			int num3 = Mathf.Min(4, num);
			bitField.Set(5, (uint)num3);
			for (int i = 0; i < num3; i++)
			{
				bitField.Set(12, (uint)listToSerialise[i].m_NetViewID.viewID);
				num--;
			}
			saveDataSoFar.Add((ulong)bitField);
		}
		return num2;
	}

	public override int GetSaveDataVersion()
	{
		return 1;
	}
}
