using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Jobs")]
[Description("Check an Item Processor to see whether it has an item ready to collect")]
public class CheckItemProcessor : ConditionTask<AICharacter>
{
	public BBParameter<List<InteractiveObject>> m_ItemProcessors;

	public BBParameter<InteractiveObject> m_TargetItemProcessor;

	private List<InteractiveObject> m_ItemProcessors_IntObjCache;

	private List<ItemProcessorBase> m_ItemProcessors_Cache;

	public bool m_bIsIdle = true;

	protected override string info => "CheckItemProcessor" + ((!m_bIsIdle) ? " Has Item" : " Is Idle");

	protected override bool OnCheck()
	{
		if (m_ItemProcessors == null || m_ItemProcessors.value == null)
		{
			return false;
		}
		if (m_ItemProcessors_IntObjCache != m_ItemProcessors.value)
		{
			m_ItemProcessors_IntObjCache = m_ItemProcessors.value;
			m_ItemProcessors_Cache = null;
		}
		if (m_ItemProcessors_Cache == null)
		{
			m_ItemProcessors_Cache = new List<ItemProcessorBase>();
			for (int i = 0; i < m_ItemProcessors_IntObjCache.Count; i++)
			{
				ItemProcessorBase component = m_ItemProcessors_IntObjCache[i].GetComponent<ItemProcessorBase>();
				if (component != null)
				{
					m_ItemProcessors_Cache.Add(component);
				}
			}
		}
		for (int j = 0; j < m_ItemProcessors_Cache.Count; j++)
		{
			ItemProcessorBase itemProcessorBase = m_ItemProcessors_Cache[j];
			if (itemProcessorBase == null)
			{
				continue;
			}
			if (m_bIsIdle)
			{
				if (itemProcessorBase.IsIdle())
				{
					m_TargetItemProcessor.value = itemProcessorBase.GetComponent<InteractiveObject>();
					return true;
				}
			}
			else if (itemProcessorBase.IsFinishedCreatingItem())
			{
				m_TargetItemProcessor.value = itemProcessorBase.GetComponent<InteractiveObject>();
				return true;
			}
		}
		return false;
	}

	protected override string OnInit()
	{
		m_ItemProcessors_Cache = null;
		return base.OnInit();
	}
}
