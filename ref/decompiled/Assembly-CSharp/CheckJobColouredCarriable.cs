using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("★T17 Jobs")]
[Description("Check a Carryable Object Consumer to see whether it has an item ready to collect")]
public class CheckJobColouredCarriable : ConditionTask<AICharacter>
{
	public BBParameter<List<GameObject>> m_Consumers;

	public BBParameter<GameObject> m_Consumer;

	private List<GameObject> m_Consumers_ObjCache;

	private List<CarryableObjectConsumer> m_Consumers_Cache;

	protected override string info => "CheckJobColouredCarriable matches";

	protected override bool OnCheck()
	{
		if (m_Consumers == null || m_Consumers.value == null)
		{
			return false;
		}
		if (m_Consumers_ObjCache != m_Consumers.value)
		{
			m_Consumers_ObjCache = m_Consumers.value;
			m_Consumers_Cache = null;
		}
		if (m_Consumers_ObjCache == null)
		{
			return false;
		}
		if (m_Consumers_Cache == null)
		{
			m_Consumers_Cache = new List<CarryableObjectConsumer>();
			for (int i = 0; i < m_Consumers_ObjCache.Count; i++)
			{
				CarryableObjectConsumer component = m_Consumers_ObjCache[i].GetComponent<CarryableObjectConsumer>();
				if (component != null)
				{
					m_Consumers_Cache.Add(component);
				}
			}
		}
		if (base.agent.m_Character.GetInteractiveObject() == null)
		{
			return false;
		}
		uint tag = base.agent.m_Character.GetInteractiveObject().m_Tag;
		for (int j = 0; j < m_Consumers_Cache.Count; j++)
		{
			CarryableObjectConsumer carryableObjectConsumer = m_Consumers_Cache[j];
			if (carryableObjectConsumer != null && carryableObjectConsumer.m_ProcessingTags.Contains(tag))
			{
				m_Consumer.value = carryableObjectConsumer.gameObject;
				return true;
			}
		}
		return false;
	}
}
