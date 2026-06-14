using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterUpdateController : IUpdateController
{
	private enum CharacterComponents
	{
		Character,
		AICharacter,
		AIMovement,
		CharacterAnimator,
		CharacterStats,
		CharacterSpeechBubbleHandler,
		CharacterIconHandler,
		Count
	}

	private Dictionary<GameObject, IControlledUpdate[]> m_CharacterBehaviourMap = new Dictionary<GameObject, IControlledUpdate[]>();

	private FastList<IControlledUpdate> m_CharacterList = new FastList<IControlledUpdate>();

	private FastList<IControlledUpdate> m_AICharactersList = new FastList<IControlledUpdate>();

	private FastList<IControlledUpdate> m_AIMovementList = new FastList<IControlledUpdate>();

	private FastList<IControlledUpdate> m_CharacterAnimatorList = new FastList<IControlledUpdate>();

	private FastList<IControlledUpdate> m_CharacterStatsList = new FastList<IControlledUpdate>();

	private FastList<IControlledUpdate> m_CharacterSpeechBubbleList = new FastList<IControlledUpdate>();

	private FastList<IControlledUpdate> m_CharacterIconHandlerList = new FastList<IControlledUpdate>();

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
				else if (controlledUpdate as CharacterStats != null)
				{
					TryRemoveBehaviour(ref m_CharacterStatsList, controlledUpdate);
				}
				else if (controlledUpdate as CharacterSpeechBubbleHandler != null)
				{
					TryRemoveBehaviour(ref m_CharacterSpeechBubbleList, controlledUpdate);
				}
				else
				{
					TryRemoveBehaviour(ref m_CharacterIconHandlerList, controlledUpdate);
				}
			}
		}
		m_CharacterBehaviourMap.Remove(gameObject);
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

	private void ScheduleProximityTrackedElemUpdate()
	{
		Gamer[] allGamers = Gamer.GetAllGamers();
		int num = allGamers.Length;
		Player pTEUpdatePlayer = GetPTEUpdatePlayer(allGamers, num);
		if ((bool)pTEUpdatePlayer && pTEUpdatePlayer.m_PTE_State == Player.PTE_State.Idle)
		{
			pTEUpdatePlayer.m_PTE_State = Player.PTE_State.Waiting;
		}
		if (!(pTEUpdatePlayer == null) && pTEUpdatePlayer.m_PTE_State != Player.PTE_State.Updated)
		{
			return;
		}
		if ((bool)pTEUpdatePlayer)
		{
			pTEUpdatePlayer.m_PTE_State = Player.PTE_State.Idle;
			pTEUpdatePlayer = null;
		}
		int num2 = 0;
		do
		{
			if (++m_iPTE_UpdateIndex >= num)
			{
				m_iPTE_UpdateIndex = 0;
			}
			num2++;
		}
		while (num2 < num && (allGamers[m_iPTE_UpdateIndex] == null || !allGamers[m_iPTE_UpdateIndex].IsLocal()));
		pTEUpdatePlayer = GetPTEUpdatePlayer(allGamers, num);
		if ((bool)pTEUpdatePlayer)
		{
			pTEUpdatePlayer.m_PTE_State = Player.PTE_State.Waiting;
		}
	}

	public bool RequiresRunUpdates()
	{
		return true;
	}

	public void RunUpdates()
	{
		UpdateManager.deltaTime = UpdateManager.systemDeltaTime;
		ScheduleProximityTrackedElemUpdate();
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
		count = m_CharacterStatsList.Count;
		items = m_CharacterStatsList._items;
		for (int i = 0; i < count; i++)
		{
			if (items[i] != null)
			{
				items[i].ControlledUpdate();
			}
		}
		count = m_CharacterSpeechBubbleList.Count;
		items = m_CharacterSpeechBubbleList._items;
		for (int i = 0; i < count; i++)
		{
			if (items[i] != null)
			{
				items[i].ControlledUpdate();
			}
		}
		count = m_CharacterIconHandlerList.Count;
		items = m_CharacterIconHandlerList._items;
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
		GlobalStart instance = GlobalStart.GetInstance();
		if (T17NetRoomManager.IsInRoom() && instance != null && instance.IsWithinLevel() && T17NetManager.IsConnectedToGameServerAndReady)
		{
			for (num = 0; num < count; num++)
			{
				if (items[num] != null)
				{
					items[num].ControlledFixedUpdate();
				}
			}
		}
		count = m_AIMovementList.Count;
		items = m_AIMovementList._items;
		for (num = 0; num < count; num++)
		{
			if (items[num] != null)
			{
				items[num].ControlledFixedUpdate();
			}
		}
	}

	public bool RequiresLateUpdates()
	{
		return true;
	}

	public void RunLateUpdates()
	{
		int num = 0;
		int count = m_CharacterList.Count;
		IControlledUpdate[] items = m_CharacterList._items;
		GlobalStart instance = GlobalStart.GetInstance();
		if (!(instance != null) || !instance.IsWithinLevel())
		{
			return;
		}
		for (num = 0; num < count; num++)
		{
			if (items[num] != null)
			{
				items[num].ControlledLateUpdate();
			}
		}
	}

	public bool RequiresRunPreUpdates()
	{
		return true;
	}

	public void RunPreUpdates()
	{
		int num = 0;
		int count = m_CharacterList.Count;
		IControlledUpdate[] items = m_CharacterList._items;
		GlobalStart instance = GlobalStart.GetInstance();
		if (!(instance != null) || !instance.IsWithinLevel())
		{
			return;
		}
		for (num = 0; num < count; num++)
		{
			if (items[num] != null)
			{
				items[num].ControlledPreUpdate();
			}
		}
	}

	public bool RequiresPreFixedUpdate()
	{
		return true;
	}

	public void RunPreFixedUpdates()
	{
		int num = 0;
		int count = m_CharacterList.Count;
		IControlledUpdate[] items = m_CharacterList._items;
		GlobalStart instance = GlobalStart.GetInstance();
		if (!(instance != null) || !instance.IsWithinLevel())
		{
			return;
		}
		for (num = 0; num < count; num++)
		{
			if (items[num] != null)
			{
				items[num].ControlledPreFixedUpdate();
			}
		}
	}

	private IControlledUpdate[] FindControlledBehaviours(GameObject obj)
	{
		IControlledUpdate[] behaviours = new IControlledUpdate[7];
		if (behaviours != null)
		{
			Addbehaviour(ref obj, ref behaviours, ref m_CharacterList, typeof(Character), CharacterComponents.Character);
			Addbehaviour(ref obj, ref behaviours, ref m_AICharactersList, typeof(AICharacter), CharacterComponents.AICharacter);
			Addbehaviour(ref obj, ref behaviours, ref m_AIMovementList, typeof(AIMovement), CharacterComponents.AIMovement);
			Addbehaviour(ref obj, ref behaviours, ref m_CharacterAnimatorList, typeof(CharacterAnimator), CharacterComponents.CharacterAnimator);
			Addbehaviour(ref obj, ref behaviours, ref m_CharacterStatsList, typeof(CharacterStats), CharacterComponents.CharacterStats);
			Addbehaviour(ref obj, ref behaviours, ref m_CharacterSpeechBubbleList, typeof(CharacterSpeechBubbleHandler), CharacterComponents.CharacterSpeechBubbleHandler);
			Addbehaviour(ref obj, ref behaviours, ref m_CharacterIconHandlerList, typeof(CharacterIconHandler), CharacterComponents.CharacterIconHandler);
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
		m_CharacterStatsList.Clear();
		m_CharacterSpeechBubbleList.Clear();
		m_CharacterIconHandlerList.Clear();
	}
}
