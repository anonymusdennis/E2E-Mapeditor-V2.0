using System.Collections.Generic;
using UnityEngine;

public class PeriodicUpdateController : IUpdateController
{
	private FastList<IControlledUpdate> m_Behaviours;

	private FastList<IControlledUpdate> m_Behaviours_ControlledFixedUpdate;

	private FastList<IControlledUpdate> m_Behaviours_ControlledUpdate;

	private FastList<IControlledUpdate> m_Behaviours_ControlledLateUpdate;

	private float m_UpdateInterval;

	private float m_Timer;

	private float m_LateTimer;

	public PeriodicUpdateController(int updateInterval)
	{
		m_Behaviours = new FastList<IControlledUpdate>();
		m_Behaviours_ControlledFixedUpdate = new FastList<IControlledUpdate>();
		m_Behaviours_ControlledUpdate = new FastList<IControlledUpdate>();
		m_Behaviours_ControlledLateUpdate = new FastList<IControlledUpdate>();
		m_UpdateInterval = (float)updateInterval / 1000f;
		m_Timer = 0f;
	}

	public void Register(IControlledUpdate behaviour)
	{
		if (m_Behaviours.Find((IControlledUpdate x) => x == behaviour) == null)
		{
			m_Behaviours.Add(behaviour);
			if (behaviour.RequiresControlledUpdate())
			{
				m_Behaviours_ControlledUpdate.Add(behaviour);
			}
			if (behaviour.RequiresControlledFixedUpdate())
			{
				m_Behaviours_ControlledFixedUpdate.Add(behaviour);
			}
			if (behaviour.RequiresControlledLateUpdate())
			{
				m_Behaviours_ControlledLateUpdate.Add(behaviour);
			}
		}
	}

	public void Unregister(IControlledUpdate behaviour)
	{
		m_Behaviours.Remove(behaviour);
		if (behaviour.RequiresControlledUpdate())
		{
			m_Behaviours_ControlledUpdate.Remove(behaviour);
		}
		if (behaviour.RequiresControlledFixedUpdate())
		{
			m_Behaviours_ControlledFixedUpdate.Remove(behaviour);
		}
		if (behaviour.RequiresControlledLateUpdate())
		{
			m_Behaviours_ControlledLateUpdate.Remove(behaviour);
		}
	}

	public bool RequiresRunUpdates()
	{
		return true;
	}

	public void RunUpdates()
	{
		int count = m_Behaviours_ControlledUpdate.Count;
		if (count == 0)
		{
			return;
		}
		m_Timer += UpdateManager.systemDeltaTime;
		if (m_Timer >= m_UpdateInterval)
		{
			UpdateManager.deltaTime = m_Timer;
			for (int i = 0; i < count; i++)
			{
				m_Behaviours_ControlledUpdate[i].ControlledUpdate();
			}
			m_Timer = 0f;
		}
	}

	public bool RequiresFixedUpdate()
	{
		return true;
	}

	public void RunFixedUpdates()
	{
		int count = m_Behaviours_ControlledFixedUpdate.Count;
		if (count != 0)
		{
			UpdateManager.fixedDeltaTime = Time.fixedDeltaTime;
			for (int i = 0; i < count; i++)
			{
				m_Behaviours_ControlledFixedUpdate[i].ControlledFixedUpdate();
			}
		}
	}

	public bool RequiresLateUpdates()
	{
		return true;
	}

	public void RunLateUpdates()
	{
		int count = m_Behaviours_ControlledLateUpdate.Count;
		if (count == 0)
		{
			return;
		}
		m_LateTimer += UpdateManager.systemDeltaTime;
		if (m_LateTimer >= m_UpdateInterval)
		{
			UpdateManager.deltaTime = m_LateTimer;
			for (int i = 0; i < count; i++)
			{
				m_Behaviours_ControlledLateUpdate[i].ControlledLateUpdate();
			}
			m_LateTimer = 0f;
		}
	}

	public bool RequiresPreFixedUpdate()
	{
		return false;
	}

	public void RunPreFixedUpdates()
	{
	}

	public bool RequiresRunPreUpdates()
	{
		return false;
	}

	public void RunPreUpdates()
	{
	}

	public void UnregisterAll()
	{
		m_Behaviours.Clear();
	}
}
