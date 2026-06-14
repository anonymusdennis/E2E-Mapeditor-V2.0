using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Description("Check the stage of a MultiStageTransferInteraction")]
[Category("★T17 Jobs")]
public class CheckMultiStageTransferStage : ConditionTask<AICharacter>
{
	public BBParameter<List<InteractiveObject>> m_MultiStageItemTransferers;

	public BBParameter<InteractiveObject> m_TargetMultiStageItemTransferer;

	private List<InteractiveObject> m_MultiStageItemTransferers_IntObjCache;

	private List<MultistageItemConverter> m_MultiStageItemTransferers_Cache;

	public int m_CurrentStage;

	public bool m_RequireAll;

	protected override string info => (m_RequireAll ? "Check all MultiStageTransferInteractions are " : "Check a MultiStageTransferInteraction is ") + m_CurrentStage;

	protected override bool OnCheck()
	{
		if (m_MultiStageItemTransferers == null || m_MultiStageItemTransferers.value == null)
		{
			return false;
		}
		if (m_MultiStageItemTransferers_IntObjCache != m_MultiStageItemTransferers.value)
		{
			m_MultiStageItemTransferers_IntObjCache = m_MultiStageItemTransferers.value;
			m_MultiStageItemTransferers_Cache = null;
		}
		if (m_MultiStageItemTransferers_Cache == null)
		{
			m_MultiStageItemTransferers_Cache = new List<MultistageItemConverter>();
			for (int i = 0; i < m_MultiStageItemTransferers_IntObjCache.Count; i++)
			{
				MultistageItemConverter component = m_MultiStageItemTransferers_IntObjCache[i].GetComponent<MultistageItemConverter>();
				if (component != null)
				{
					m_MultiStageItemTransferers_Cache.Add(component);
				}
			}
		}
		bool result = true;
		for (int j = 0; j < m_MultiStageItemTransferers_Cache.Count; j++)
		{
			MultistageItemConverter multistageItemConverter = m_MultiStageItemTransferers_Cache[j];
			if (multistageItemConverter == null)
			{
				continue;
			}
			if (multistageItemConverter.GetCurrentStage() == m_CurrentStage)
			{
				m_TargetMultiStageItemTransferer.value = multistageItemConverter.GetComponent<InteractiveObject>();
				if (!m_RequireAll)
				{
					result = true;
					break;
				}
			}
			else
			{
				result = false;
			}
		}
		return result;
	}

	protected override string OnInit()
	{
		m_MultiStageItemTransferers_Cache = null;
		return base.OnInit();
	}
}
