using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Events")]
[Description("Check current routine")]
public class CheckCurrentRoutine : ConditionTask<AICharacter>
{
	public BBParameter<Routines> m_Routine;

	public BBParameter<Routines[]> m_Routines;

	private bool m_bInited;

	public bool m_bIsRoutine;

	protected override string info => "Current Routine: " + m_Routine;

	protected override string OnInit()
	{
		InitRoutine();
		return base.OnInit();
	}

	protected override bool OnCheck()
	{
		if (!m_bInited)
		{
			InitRoutine();
		}
		return m_bIsRoutine;
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

	public void RoutineChanged(RoutinesData.Routine oldRoutine, RoutinesData.Routine newRoutine, bool forceEnd)
	{
		m_bIsRoutine = false;
		if (newRoutine == null)
		{
			return;
		}
		if (m_Routines.value != null && m_Routines.value.Length > 0)
		{
			for (int i = 0; i < m_Routines.value.Length; i++)
			{
				Routines routines = m_Routines.value[i];
				if (newRoutine.m_BaseRoutineType == routines)
				{
					m_bIsRoutine = true;
					break;
				}
			}
		}
		else
		{
			m_bIsRoutine = newRoutine.m_BaseRoutineType == m_Routine.value;
		}
	}
}
