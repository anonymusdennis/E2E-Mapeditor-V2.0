using System.Collections.Generic;

public class FilteredCarryableIntoConsumerJob : CarriedInputJob
{
	protected List<CarryableObjectConsumer> m_Consumers = new List<CarryableObjectConsumer>();

	protected override void PreSetupDispenser()
	{
		m_Consumers.Clear();
		for (int num = base.RoomData.m_BespokeJobObjects.Count - 1; num >= 0; num--)
		{
			if (!(base.RoomData.m_BespokeJobObjects[num] == null))
			{
				CarryableObjectConsumer component = base.RoomData.m_BespokeJobObjects[num].GetComponent<CarryableObjectConsumer>();
				if (component != null)
				{
					component.InputDroppedOnUsEvent += CarryableConsumer_AcceptedInputEvent;
					m_Consumers.Add(component);
					if (component.m_AcceptedTags.Count == 0)
					{
					}
					if (component.m_ProcessingTags.Count != 0)
					{
					}
				}
			}
		}
		if (m_Consumers.Count != 0)
		{
		}
	}

	protected override List<uint> GetPossibleSpawnTags()
	{
		return CarryableObjectConsumer.GetAllPossibleSpawnTags(m_Consumers);
	}

	protected override void OnDestroy()
	{
		for (int num = m_Consumers.Count - 1; num >= 0; num--)
		{
			m_Consumers[num].InputDroppedOnUsEvent -= CarryableConsumer_AcceptedInputEvent;
		}
		base.OnDestroy();
	}

	private void CarryableConsumer_AcceptedInputEvent(CarryableObjectConsumer consumer, CarryObjectInteraction theObject)
	{
		if (T17NetManager.IsMasterClient && consumer.m_ProcessingTags.Contains(theObject.m_Tag))
		{
			IncrementQuotaAchieved();
		}
		for (int i = 0; i < m_CarriedObjectDispenser.Length; i++)
		{
			m_CarriedObjectDispenser[i].AddObjectBackToSpawnPool(theObject);
		}
	}
}
