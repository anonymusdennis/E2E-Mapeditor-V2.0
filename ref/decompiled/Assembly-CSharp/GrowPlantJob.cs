using System.Collections.Generic;
using UnityEngine;

public class GrowPlantJob : MultistageItemConversionJob
{
	private List<PlantPatch> m_PlantPatches = new List<PlantPatch>();

	public ItemData m_Trowel;

	public ItemData m_Seeds;

	public ItemData m_PottedPlant;

	public int m_NumSeedsPerTrowel = 4;

	public override void Init(RoomBlob jobRoom)
	{
		base.Init(jobRoom);
		for (int num = base.RoomData.m_Processors.Count - 1; num >= 0; num--)
		{
			if (!(base.RoomData.m_Processors[num] == null))
			{
				PlantPatch component = base.RoomData.m_Processors[num].GetComponent<PlantPatch>();
				if (component != null)
				{
					component.SeedGrowingEvent += PlantPatch_SeedGrowingEvent;
					component.GrownPlantTakenEvent += PlantPatch_GrownPlantTakenEvent;
					m_PlantPatches.Add(component);
				}
			}
		}
	}

	protected override List<ItemData> GetDispenserTransactionItems()
	{
		List<ItemData> list = new List<ItemData>(base.GetDispenserTransactionItems());
		list.Add(m_Trowel);
		return list;
	}

	public override List<ItemData> GetDispenserContentsForFilling()
	{
		List<ItemData> list = new List<ItemData>();
		list.Add(m_Trowel);
		for (int i = 0; i < m_NumSeedsPerTrowel; i++)
		{
			list.AddRange(base.GetDispenserContentsForFilling());
		}
		return list;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		for (int num = m_PlantPatches.Count - 1; num >= 0; num--)
		{
			m_PlantPatches[num].SeedGrowingEvent -= PlantPatch_SeedGrowingEvent;
		}
	}

	private void PlantPatch_GrownPlantTakenEvent(PlantPatch sender)
	{
		MasterPlantQuotaIncrease();
	}

	private void PlantPatch_SeedGrowingEvent(PlantPatch sender)
	{
		MasterPlantQuotaIncrease();
	}

	private void MasterPlantQuotaIncrease()
	{
		if (T17NetManager.IsMasterClient)
		{
			IncrementQuotaAchieved();
			SetRoutineInformationForCharacter(base.Employee);
		}
	}

	public override void SetRoutineInformationForCharacter(Character character)
	{
		if (character == null)
		{
			return;
		}
		List<PlantPatch> list = m_PlantPatches.FindAll((PlantPatch x) => x.GetPlantState() == PlantPatch.State.FullyGrown);
		if (list.Count > 0)
		{
			PlantPatch plantPatch = FindClosestInList(character, list);
			if (character.m_CharacterStats.m_bIsPlayer)
			{
				Player player = (Player)character;
				player.SetRoutineArrowTarget(plantPatch.m_NetView);
			}
			return;
		}
		List<ItemData> list2 = new List<ItemData>();
		list2.Add(m_Trowel);
		List<PlantPatch> list3 = m_PlantPatches.FindAll((PlantPatch x) => x.GetPlantState() == PlantPatch.State.DugPatch || x.GetPlantState() == PlantPatch.State.EmptyPatch);
		if (list3.Count > 0)
		{
			list2.Add(m_Seeds);
		}
		if (!character.HasItemsOnPerson(list2))
		{
			ItemContainer itemContainer = m_DispenserContainers[Random.Range(0, m_DispenserContainers.Count)];
			if (character.m_CharacterStats.m_bIsPlayer)
			{
				Player player2 = (Player)character;
				player2.SetRoutineArrowTarget(itemContainer.NetView);
			}
			return;
		}
		List<PlantPatch> list4 = m_PlantPatches.FindAll((PlantPatch x) => x.GetPlantState() != PlantPatch.State.SeedsGrowing);
		if (list4.Count > 0)
		{
			PlantPatch plantPatch2 = FindClosestInList(character, list4);
			if (character.m_CharacterStats.m_bIsPlayer)
			{
				Player player3 = (Player)character;
				player3.SetRoutineArrowTarget(plantPatch2.m_NetView);
			}
		}
		else if (character.m_CharacterStats.m_bIsPlayer)
		{
			Player player4 = (Player)character;
			player4.SetRoutineArrowTarget((RoomBlob)null);
		}
	}

	private PlantPatch FindClosestInList(Character character, List<PlantPatch> patches)
	{
		PlantPatch plantPatch = patches[patches.Count - 1];
		float num = GetMovementCostToInteraction(plantPatch, character);
		for (int num2 = patches.Count - 2; num2 >= 0; num2--)
		{
			PlantPatch plantPatch2 = patches[num2];
			float movementCostToInteraction = GetMovementCostToInteraction(plantPatch2, character);
			if (movementCostToInteraction < num)
			{
				num = movementCostToInteraction;
				plantPatch = plantPatch2;
			}
		}
		return plantPatch;
	}

	private float GetMovementCostToInteraction(PlantPatch interaction, Character character)
	{
		float sqrMagnitude = (interaction.transform.position - character.transform.position).sqrMagnitude;
		float num = interaction.transform.position.z - character.transform.position.z;
		return sqrMagnitude * (num / (float)FloorManager.GetInstance().m_FloorOffset);
	}

	protected override List<ItemData> OneTimeCalculateJobRelatedItems()
	{
		List<ItemData> list = base.OneTimeCalculateJobRelatedItems();
		list.Add(m_Trowel);
		return list;
	}
}
