using System.Collections.Generic;

public class TrackedElementReporterUpdateController : IUpdateController
{
	private static TrackedElementReporterUpdateController m_Instance;

	private LinkedList<TimeTrackedControlledUpdate> m_UpdatableItems = new LinkedList<TimeTrackedControlledUpdate>();

	public TrackedElementReporterUpdateController()
	{
		m_Instance = this;
	}

	public static TrackedElementReporterUpdateController GetInstance()
	{
		return m_Instance;
	}

	public void Register(IControlledUpdate behaviour)
	{
	}

	public void Unregister(IControlledUpdate behaviour)
	{
		RemoveFromUpdateList(behaviour);
	}

	~TrackedElementReporterUpdateController()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	public void AddToUpdateList(IControlledUpdate behaviour)
	{
		m_UpdatableItems.AddLast(new TimeTrackedControlledUpdate(behaviour));
	}

	public void RemoveFromUpdateList(IControlledUpdate behaviour)
	{
		foreach (TimeTrackedControlledUpdate updatableItem in m_UpdatableItems)
		{
			if (updatableItem.m_Behaviour == behaviour)
			{
				m_UpdatableItems.Remove(updatableItem);
				break;
			}
		}
	}

	public bool RequiresRunUpdates()
	{
		return true;
	}

	public void RunUpdates()
	{
		for (LinkedListNode<TimeTrackedControlledUpdate> linkedListNode = m_UpdatableItems.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			TimeTrackedControlledUpdate value = linkedListNode.Value;
			float deltaTime = UpdateManager.deltaTimeSinceStart - value.m_fPreviousDeltaTime;
			value.m_fPreviousDeltaTime = UpdateManager.deltaTimeSinceStart;
			UpdateManager.deltaTime = deltaTime;
			linkedListNode.Value.m_Behaviour.ControlledUpdate();
		}
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
		m_UpdatableItems.Clear();
	}
}
