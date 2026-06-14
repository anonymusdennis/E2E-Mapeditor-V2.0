using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Jobs")]
[Description("Check the state of a DogBowl")]
public class CheckDogBowlState : ConditionTask<AICharacter>
{
	public BBParameter<List<InteractiveObject>> m_DogBowls;

	public BBParameter<InteractiveObject> m_TargetDogBowl;

	private List<InteractiveObject> m_DogBowls_IntObjCache;

	private List<DogBowl> m_DogBowls_Cache;

	public DogBowl.Stages m_CurrentStage;

	public bool m_RequireAll;

	protected override string info => (m_RequireAll ? "Check all DogBowls are " : "Check a DogBowl is ") + m_CurrentStage;

	protected override bool OnCheck()
	{
		if (m_DogBowls == null || m_DogBowls.value == null)
		{
			return false;
		}
		if (m_DogBowls_IntObjCache != m_DogBowls.value)
		{
			m_DogBowls_IntObjCache = m_DogBowls.value;
			m_DogBowls_Cache = null;
		}
		if (m_DogBowls_Cache == null)
		{
			m_DogBowls_Cache = new List<DogBowl>();
			for (int i = 0; i < m_DogBowls_IntObjCache.Count; i++)
			{
				DogBowl component = m_DogBowls_IntObjCache[i].GetComponent<DogBowl>();
				if (component != null)
				{
					m_DogBowls_Cache.Add(component);
				}
			}
		}
		bool result = true;
		for (int j = 0; j < m_DogBowls_Cache.Count; j++)
		{
			DogBowl dogBowl = m_DogBowls_Cache[j];
			if (dogBowl == null)
			{
				continue;
			}
			if (dogBowl.GetStage() == m_CurrentStage)
			{
				m_TargetDogBowl.value = dogBowl.GetComponent<InteractiveObject>();
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
		m_DogBowls_Cache = null;
		return base.OnInit();
	}
}
