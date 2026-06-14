using System.Collections.Generic;

public class ItemUpdateController : IUpdateController
{
	private LinkedList<IControlledUpdate> m_UpdatableItems = new LinkedList<IControlledUpdate>();

	public void Register(IControlledUpdate behaviour)
	{
		if (behaviour != null)
		{
			Item item = behaviour as Item;
			if (!(item == null) && !item.IsImmediateUse() && !m_UpdatableItems.Contains(behaviour))
			{
				m_UpdatableItems.AddLast(behaviour);
			}
		}
	}

	public void Unregister(IControlledUpdate behaviour)
	{
		m_UpdatableItems.Remove(behaviour);
	}

	public bool RequiresRunUpdates()
	{
		return true;
	}

	public void RunUpdates()
	{
		UpdateManager.deltaTime = UpdateManager.systemDeltaTime;
		for (LinkedListNode<IControlledUpdate> linkedListNode = m_UpdatableItems.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			linkedListNode.Value.ControlledUpdate();
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
