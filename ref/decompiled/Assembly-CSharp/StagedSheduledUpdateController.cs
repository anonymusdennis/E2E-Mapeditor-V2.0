using System.Collections.Generic;
using System.Diagnostics;

public class StagedSheduledUpdateController : IUpdateController
{
	private FastList<TimeTrackedControlledUpdate> m_Behaviours;

	private float m_FrameTimeBudgetMs;

	private Stopwatch m_StopWatch = new Stopwatch();

	private int m_CurrentIndex;

	private string m_Name;

	public StagedSheduledUpdateController(float frameTimeBudgetMs, string name)
	{
		m_Behaviours = new FastList<TimeTrackedControlledUpdate>();
		m_Name = name;
		m_FrameTimeBudgetMs = frameTimeBudgetMs;
	}

	public void Register(IControlledUpdate behaviour)
	{
		if (m_Behaviours.Find((TimeTrackedControlledUpdate x) => x.m_Behaviour == behaviour) == null)
		{
			m_Behaviours.Add(new TimeTrackedControlledUpdate(behaviour));
		}
	}

	public void Unregister(IControlledUpdate behaviour)
	{
		TimeTrackedControlledUpdate timeTrackedControlledUpdate = m_Behaviours.Find((TimeTrackedControlledUpdate x) => x.m_Behaviour == behaviour);
		if (timeTrackedControlledUpdate != null)
		{
			m_Behaviours.Remove(timeTrackedControlledUpdate);
		}
	}

	public bool RequiresRunUpdates()
	{
		return true;
	}

	public void RunUpdates()
	{
		int count = m_Behaviours.Count;
		if (count == 0)
		{
			return;
		}
		m_StopWatch.Reset();
		m_StopWatch.Start();
		int num = 0;
		do
		{
			if (m_CurrentIndex >= count)
			{
				m_CurrentIndex = 0;
			}
			TimeTrackedControlledUpdate timeTrackedControlledUpdate = m_Behaviours[m_CurrentIndex];
			float deltaTime = UpdateManager.deltaTimeSinceStart - timeTrackedControlledUpdate.m_fPreviousDeltaTime;
			timeTrackedControlledUpdate.m_fPreviousDeltaTime = UpdateManager.deltaTimeSinceStart;
			UpdateManager.deltaTime = deltaTime;
			timeTrackedControlledUpdate.m_Behaviour.ControlledUpdate();
			m_CurrentIndex++;
			num++;
		}
		while (num < count && m_StopWatch.Elapsed.TotalMilliseconds < (double)m_FrameTimeBudgetMs);
	}

	public bool RequiresFixedUpdate()
	{
		return false;
	}

	public void RunFixedUpdates()
	{
	}

	public bool RequiresLateUpdates()
	{
		return false;
	}

	public void RunLateUpdates()
	{
	}

	public bool RequiresRunPreUpdates()
	{
		return false;
	}

	public void RunPreUpdates()
	{
	}

	public bool RequiresPreFixedUpdate()
	{
		return false;
	}

	public void RunPreFixedUpdates()
	{
	}

	public void UnregisterAll()
	{
		m_Behaviours.Clear();
	}
}
