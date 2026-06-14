using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Events")]
[Description("Conditional returns true if we have a patrol for this routine")]
public class CheckHavePatrol : ConditionTask<AICharacter>
{
	private bool m_bInited;

	public bool m_bHavePatrol;

	protected override string OnInit()
	{
		InitRoutine();
		return base.OnInit();
	}

	private void InitRoutine()
	{
		if (!m_bInited && RoutineManager.GetInstance().RoutineManagerReady())
		{
			RoutinesData.Routine currentRoutine = RoutineManager.GetInstance().GetCurrentRoutine();
			if (currentRoutine != null)
			{
				RoutineManager.GetInstance().OnRoutineChanged += RoutineChanged;
				RoutineChanged(null, currentRoutine, forceEnd: false);
				m_bInited = true;
			}
		}
	}

	protected override bool OnCheck()
	{
		if (!m_bInited)
		{
			InitRoutine();
		}
		return m_bHavePatrol;
	}

	public void RoutineChanged(RoutinesData.Routine oldRoutine, RoutinesData.Routine newRoutine, bool forceEnd)
	{
		PatrolPath randomPatrolObject = base.agent.m_AIPatrols.GetRandomPatrolObject(newRoutine.m_BaseRoutineType);
		m_bHavePatrol = randomPatrolObject != null;
	}
}
