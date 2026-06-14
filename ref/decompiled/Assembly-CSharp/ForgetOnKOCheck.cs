using System;
using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Action")]
public class ForgetOnKOCheck : ActionTask<AICharacter>
{
	public bool m_bReturnStatus;

	public AIEvent.EventType[] m_checkEventTypes;

	private Dictionary<int, List<AIEvent.EventType>> m_TrackedCharacterIdToEventTypes = new Dictionary<int, List<AIEvent.EventType>>();

	protected override string info
	{
		get
		{
			string empty = string.Empty;
			empty += '\n';
			empty += '\n';
			for (int i = 0; i < m_checkEventTypes.Length; i++)
			{
				empty += m_checkEventTypes[i];
				if (i < m_checkEventTypes.Length - 1)
				{
					empty += ", ";
					empty += '\n';
				}
			}
			return "Forget Events on Char KO: " + empty;
		}
	}

	protected override void OnExecute()
	{
		EndAction(m_bReturnStatus);
	}

	protected override string OnInit()
	{
		for (int i = 0; i < m_checkEventTypes.Length; i++)
		{
			base.agent.ListenForEvent(AIEventReceived, m_checkEventTypes[i]);
		}
		for (int j = 0; j < m_checkEventTypes.Length; j++)
		{
			AIEvent.EventType eventType = m_checkEventTypes[j];
			List<AIEventMemory> eventMemories = base.agent.GetEventMemories(eventType);
			if (eventMemories != null)
			{
				for (int k = 0; k < eventMemories.Count; k++)
				{
					AIEventReceived(eventMemories[k]);
				}
			}
		}
		return base.OnInit();
	}

	private void AIEventReceived(AIEventMemory aiEventMemory)
	{
		if (aiEventMemory != null && aiEventMemory.m_AIEvent != null && !(aiEventMemory.m_AIEvent.m_TargetCharacter == null) && !(aiEventMemory.m_AIEvent.m_EventData == null))
		{
			Character targetCharacter = aiEventMemory.m_AIEvent.m_TargetCharacter;
			AIEvent.EventType eEventType = aiEventMemory.m_AIEvent.m_EventData.m_eEventType;
			List<AIEvent.EventType> value = null;
			int viewID = targetCharacter.m_NetView.viewID;
			m_TrackedCharacterIdToEventTypes.TryGetValue(viewID, out value);
			if (value == null)
			{
				value = new List<AIEvent.EventType>();
			}
			if (!value.Contains(eEventType))
			{
				value.Add(eEventType);
			}
			m_TrackedCharacterIdToEventTypes[viewID] = value;
			if (aiEventMemory.m_AIEvent.m_TargetCharacter.GetIsKnockedOut())
			{
				CharacterKnockedOut(aiEventMemory.m_AIEvent.m_TargetCharacter, null);
				return;
			}
			targetCharacter.OnCharacterKnockedOut = (Character.CharacterToCharacterEvent)Delegate.Remove(targetCharacter.OnCharacterKnockedOut, new Character.CharacterToCharacterEvent(CharacterKnockedOut));
			targetCharacter.OnCharacterKnockedOut = (Character.CharacterToCharacterEvent)Delegate.Combine(targetCharacter.OnCharacterKnockedOut, new Character.CharacterToCharacterEvent(CharacterKnockedOut));
		}
	}

	public void CharacterKnockedOut(Character character, Character attacker)
	{
		character.OnCharacterKnockedOut = (Character.CharacterToCharacterEvent)Delegate.Remove(character.OnCharacterKnockedOut, new Character.CharacterToCharacterEvent(CharacterKnockedOut));
		List<AIEvent.EventType> value = null;
		int viewID = character.m_NetView.viewID;
		m_TrackedCharacterIdToEventTypes.TryGetValue(viewID, out value);
		base.agent.FlagEventsToForget(character, value);
		value.Clear();
	}
}
