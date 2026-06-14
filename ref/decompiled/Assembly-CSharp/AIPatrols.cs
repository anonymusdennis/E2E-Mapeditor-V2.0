using System;
using System.Collections.Generic;
using UnityEngine;

public class AIPatrols : T17NetworkBehaviour
{
	[Serializable]
	public class RoutinePatrol
	{
		public Routines routine = Routines.UNASSIGNED;

		public List<PatrolPath> patrol;
	}

	public List<RoutinePatrol> m_RoutinePatrols = new List<RoutinePatrol>();

	private Dictionary<Routines, List<PatrolPath>> m_RoutineToPatrols = new Dictionary<Routines, List<PatrolPath>>();

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		for (int i = 0; i < m_RoutinePatrols.Count; i++)
		{
			List<PatrolPath> value = null;
			bool flag = m_RoutineToPatrols.TryGetValue(m_RoutinePatrols[i].routine, out value);
			List<PatrolPath> patrol = m_RoutinePatrols[i].patrol;
			if (patrol != null)
			{
				if (!flag)
				{
					value = new List<PatrolPath>();
				}
				for (int j = 0; j < patrol.Count; j++)
				{
					if (patrol[j] != null)
					{
						value.Add(patrol[j]);
					}
				}
			}
			m_RoutineToPatrols[m_RoutinePatrols[i].routine] = value;
		}
		return base.StartInit();
	}

	public PatrolPath GetRandomPatrolObject(Routines routine)
	{
		List<PatrolPath> value = null;
		if (!m_RoutineToPatrols.TryGetValue(routine, out value) || value == null || value.Count == 0)
		{
			return null;
		}
		return value[UnityEngine.Random.Range(0, value.Count)];
	}

	public bool RoutineHasPatrol(Routines routine, PatrolPath path)
	{
		List<PatrolPath> value = null;
		if (!m_RoutineToPatrols.TryGetValue(routine, out value) || value == null || value.Count == 0)
		{
			return false;
		}
		return value.Contains(path);
	}
}
