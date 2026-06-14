using System;
using System.Collections.Generic;
using UnityEngine;

public class CrowdCharacterUpdateController : IUpdateController
{
	private enum CharacterComponents
	{
		Character,
		AICharacter,
		AIMovement,
		CharacterAnimator,
		Count
	}

	private Dictionary<GameObject, IControlledUpdate[]> m_CharacterBehaviourMap = new Dictionary<GameObject, IControlledUpdate[]>();

	private FastList<IControlledUpdate> m_CharacterList = new FastList<IControlledUpdate>();

	private FastList<IControlledUpdate> m_AICharactersList = new FastList<IControlledUpdate>();

	private FastList<IControlledUpdate> m_AIMovementList = new FastList<IControlledUpdate>();

	private FastList<IControlledUpdate> m_CharacterAnimatorList = new FastList<IControlledUpdate>();

	private FastList<Character> m_Characters = new FastList<Character>();

	private int m_iPTE_UpdateIndex;

	public void Register(IControlledUpdate behaviour)
	{
		Character character = behaviour as Character;
		if (character == null)
		{
			return;
		}
		GameObject gameObject = character.gameObject;
		if (!m_CharacterBehaviourMap.ContainsKey(gameObject))
		{
			IControlledUpdate[] array = FindControlledBehaviours(gameObject);
			if (array != null)
			{
				m_CharacterBehaviourMap.Add(gameObject, array);
			}
			m_Characters.Add(character);
		}
	}

	public void Unregister(IControlledUpdate behaviour)
	{
		Character character = behaviour as Character;
		if (character == null)
		{
			return;
		}
		GameObject gameObject = character.gameObject;
		if (!m_CharacterBehaviourMap.ContainsKey(gameObject))
		{
			return;
		}
		IControlledUpdate[] array = m_CharacterBehaviourMap[gameObject];
		if (array != null)
		{
			foreach (IControlledUpdate controlledUpdate in array)
			{
				if (controlledUpdate as Character != null)
				{
					TryRemoveBehaviour(ref m_CharacterList, controlledUpdate);
				}
				else if (controlledUpdate as AICharacter != null)
				{
					TryRemoveBehaviour(ref m_AICharactersList, controlledUpdate);
				}
				else if (controlledUpdate as AIMovement != null)
				{
					TryRemoveBehaviour(ref m_AIMovementList, controlledUpdate);
				}
				else if (controlledUpdate as CharacterAnimator != null)
				{
					TryRemoveBehaviour(ref m_CharacterAnimatorList, controlledUpdate);
				}
			}
		}
		m_CharacterBehaviourMap.Remove(gameObject);
		m_Characters.Remove(character);
	}

	private bool TryRemoveBehaviour(ref FastList<IControlledUpdate> list, IControlledUpdate update)
	{
		if (list.Contains(update))
		{
			list.Remove(update);
			return true;
		}
		return false;
	}

	private Player GetPTEUpdatePlayer(Gamer[] gamers, int numGamers)
	{
		return (m_iPTE_UpdateIndex >= numGamers || gamers[m_iPTE_UpdateIndex] == null || !gamers[m_iPTE_UpdateIndex].IsLocal()) ? null : gamers[m_iPTE_UpdateIndex].m_PlayerObject;
	}

	public bool RequiresRunUpdates()
	{
		return true;
	}

	public void RunUpdates()
	{
		UpdateManager.deltaTime = UpdateManager.systemDeltaTime;
		int count = m_CharacterList.Count;
		IControlledUpdate[] items = m_CharacterList._items;
		GlobalStart instance = GlobalStart.GetInstance();
		if (instance != null && instance.IsWithinLevel())
		{
			for (int i = 0; i < count; i++)
			{
				if (items[i] != null)
				{
					items[i].ControlledUpdate();
				}
			}
		}
		count = m_AICharactersList.Count;
		items = m_AICharactersList._items;
		for (int i = 0; i < count; i++)
		{
			if (items[i] != null)
			{
				items[i].ControlledUpdate();
			}
		}
		count = m_CharacterAnimatorList.Count;
		items = m_CharacterAnimatorList._items;
		for (int i = 0; i < count; i++)
		{
			if (items[i] != null)
			{
				items[i].ControlledUpdate();
			}
		}
	}

	public bool RequiresFixedUpdate()
	{
		return true;
	}

	public void RunFixedUpdates()
	{
		UpdateManager.fixedDeltaTime = Time.fixedDeltaTime;
		int num = 0;
		int count = m_CharacterList.Count;
		IControlledUpdate[] items = m_CharacterList._items;
		int count2 = m_Characters.Count;
		Character[] items2 = m_Characters._items;
		GlobalStart instance = GlobalStart.GetInstance();
		if (T17NetRoomManager.IsInRoom() && instance != null && instance.IsWithinLevel() && T17NetManager.IsConnectedToGameServerAndReady)
		{
			for (num = 0; num < count2; num++)
			{
				if (items2[num] != null && !items2[num].m_NetView.isMine)
				{
					items2[num].ApplyNetworkPrediciton();
				}
			}
		}
		count = m_AIMovementList.Count;
		items = m_AIMovementList._items;
		for (num = 0; num < count; num++)
		{
			if (items[num] != null && items[num].RequiresControlledFixedUpdate())
			{
				items[num].ControlledFixedUpdate();
			}
		}
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

	private IControlledUpdate[] FindControlledBehaviours(GameObject obj)
	{
		IControlledUpdate[] behaviours = new IControlledUpdate[4];
		if (behaviours != null)
		{
			Addbehaviour(ref obj, ref behaviours, ref m_CharacterList, typeof(Character), CharacterComponents.Character);
			Addbehaviour(ref obj, ref behaviours, ref m_AICharactersList, typeof(AICharacter), CharacterComponents.AICharacter);
			Addbehaviour(ref obj, ref behaviours, ref m_AIMovementList, typeof(AIMovement), CharacterComponents.AIMovement);
			Addbehaviour(ref obj, ref behaviours, ref m_CharacterAnimatorList, typeof(CharacterAnimator), CharacterComponents.CharacterAnimator);
		}
		return behaviours;
	}

	private void Addbehaviour(ref GameObject obj, ref IControlledUpdate[] behaviours, ref FastList<IControlledUpdate> targetList, Type componentType, CharacterComponents type)
	{
		if (obj.GetComponent(componentType) is IControlledUpdate controlledUpdate)
		{
			behaviours[(int)type] = controlledUpdate;
			targetList.Add(controlledUpdate);
		}
	}

	public void UnregisterAll()
	{
		m_CharacterBehaviourMap.Clear();
		m_CharacterList.Clear();
		m_AICharactersList.Clear();
		m_AIMovementList.Clear();
		m_CharacterAnimatorList.Clear();
	}
}
