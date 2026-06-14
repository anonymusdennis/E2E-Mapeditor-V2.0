using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Description("Check a Carryable Object Consumer to see whether it has an item ready to collect")]
[Category("★T17 Jobs")]
public class CheckObjectConsumer : ConditionTask<AICharacter>
{
	public BBParameter<List<InteractiveObject>> m_ItemProcessors;

	public BBParameter<InteractiveObject> m_TargetItemProcessor;

	private List<InteractiveObject> m_ItemProcessors_IntObjCache;

	private List<CarryableObjectConsumer> m_ItemProcessors_Cache;

	public bool m_bIsIdle = true;

	protected override string info => "CheckObjectConsumer" + ((!m_bIsIdle) ? " Has Item" : " Is Idle");

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
		if (m_ItemProcessors_IntObjCache == null)
		{
			return false;
		}
		if (m_ItemProcessors_Cache == null)
		{
			m_ItemProcessors_Cache = new List<CarryableObjectConsumer>();
			for (int i = 0; i < m_ItemProcessors_IntObjCache.Count; i++)
			{
				CarryableObjectConsumer component = m_ItemProcessors_IntObjCache[i].GetComponent<CarryableObjectConsumer>();
				if (component != null)
				{
					m_ItemProcessors_Cache.Add(component);
				}
			}
		}
		for (int j = 0; j < m_ItemProcessors_Cache.Count; j++)
		{
			CarryableObjectConsumer carryableObjectConsumer = m_ItemProcessors_Cache[j];
			if (carryableObjectConsumer == null)
			{
				continue;
			}
			if (m_bIsIdle)
			{
				if (!carryableObjectConsumer.IsProcessing())
				{
					m_TargetItemProcessor.value = carryableObjectConsumer.GetComponent<InteractiveObject>();
					return true;
				}
			}
			else if (carryableObjectConsumer.HasItems())
			{
				m_TargetItemProcessor.value = carryableObjectConsumer.GetComponent<InteractiveObject>();
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
