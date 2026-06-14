using System.Collections.Generic;
using UnityEngine;

public class ExtendedProcessItemJob : ProcessItemJob
{
	[Header("Processor 2 Settings")]
	public ItemData[] m_Processor_BInputItems;

	public ItemData[] m_Processor_BOutputItems;

	protected override void SetProcessorInputItem(ItemProcessorBase itemProcessor, ItemData[] inputItems, ItemData[] outputItems)
	{
		if (itemProcessor.m_bSecondaryProcessor)
		{
			base.SetProcessorInputItem(itemProcessor, m_Processor_BInputItems, m_Processor_BOutputItems);
		}
		else
		{
			base.SetProcessorInputItem(itemProcessor, inputItems, outputItems);
		}
	}

	protected override List<ItemData> OneTimeCalculateJobRelatedItems()
	{
		List<ItemData> list = base.OneTimeCalculateJobRelatedItems();
		list.AddRange(m_Processor_BInputItems);
		list.AddRange(m_Processor_BOutputItems);
		return list;
	}
}
