using System.Collections.Generic;
using UnityEngine;

public class FakeCharacterUpdateController : IUpdateController
{
	private enum FakeCharacterComponents
	{
		FakeCharacter,
		CharacterSpeechBubbleHandler,
		Count
	}

	private Dictionary<GameObject, IControlledUpdate[]> m_CharacterBehaviourMap = new Dictionary<GameObject, IControlledUpdate[]>();

	private FastList<IControlledUpdate> m_FakeCharacterList = new FastList<IControlledUpdate>();

	private FastList<IControlledUpdate> m_CharacterSpeechBubbleList = new FastList<IControlledUpdate>();

	public void Register(IControlledUpdate behaviour)
	{
		FakeCharacter fakeCharacter = behaviour as FakeCharacter;
		if (fakeCharacter == null)
		{
			return;
		}
		GameObject gameObject = fakeCharacter.gameObject;
		if (!m_CharacterBehaviourMap.ContainsKey(gameObject))
		{
			IControlledUpdate[] array = FindControlledBehaviours(gameObject);
			if (array != null)
			{
				m_CharacterBehaviourMap.Add(gameObject, array);
			}
		}
	}

	public void Unregister(IControlledUpdate behaviour)
	{
		FakeCharacter fakeCharacter = behaviour as FakeCharacter;
		if (!(fakeCharacter == null))
		{
			GameObject gameObject = fakeCharacter.gameObject;
			if (m_CharacterBehaviourMap.ContainsKey(gameObject))
			{
				m_CharacterBehaviourMap.Remove(gameObject);
			}
		}
	}

	public bool RequiresRunUpdates()
	{
		return true;
	}

	public void RunUpdates()
	{
		UpdateManager.deltaTime = UpdateManager.systemDeltaTime;
		int num = 0;
		int count = m_FakeCharacterList.Count;
		IControlledUpdate[] items = m_FakeCharacterList._items;
		GlobalStart instance = GlobalStart.GetInstance();
		if (instance != null && instance.IsWithinLevel())
		{
			for (num = 0; num < count; num++)
			{
				if (items[num] != null)
				{
					items[num].ControlledUpdate();
				}
			}
		}
		count = m_CharacterSpeechBubbleList.Count;
		items = m_CharacterSpeechBubbleList._items;
		for (num = 0; num < count; num++)
		{
			if (items[num] != null)
			{
				items[num].ControlledUpdate();
			}
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

	private IControlledUpdate[] FindControlledBehaviours(GameObject obj)
	{
		IControlledUpdate[] array = new IControlledUpdate[2];
		if (array != null)
		{
			array[0] = obj.GetComponent<FakeCharacter>();
			m_FakeCharacterList.Add(array[0]);
			array[1] = obj.GetComponent<CharacterSpeechBubbleHandler>();
			m_CharacterSpeechBubbleList.Add(array[1]);
		}
		return array;
	}

	public void UnregisterAll()
	{
		m_CharacterBehaviourMap.Clear();
	}
}
