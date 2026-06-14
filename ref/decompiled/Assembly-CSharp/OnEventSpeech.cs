using System;
using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine.Serialization;

[Category("★T17 Action")]
public class OnEventSpeech : ActionTask<AICharacter>
{
	[Serializable]
	public class EventTypeReaction
	{
		public enum DecorationConditions
		{
			IGNORED,
			FullAlertness
		}

		public AIEvent.EventType eventType;

		public string speech;

		public DecorationConditions decorationCondition;

		public bool isDecorationForPlayersOnly = true;

		[FormerlySerializedAs("playerFirstResponderDecoration")]
		public SpeechDecorations FirstResponderDecoration;
	}

	public bool m_bReturnStatus;

	public EventTypeReaction[] m_checkEventTypes;

	private List<SpeechManager.Token> tokens = new List<SpeechManager.Token>();

	private const int NO_NAME = -1;

	protected override string info
	{
		get
		{
			string empty = string.Empty;
			empty += '\n';
			empty += '\n';
			for (int i = 0; i < m_checkEventTypes.Length; i++)
			{
				empty += m_checkEventTypes[i].eventType;
				if (i < m_checkEventTypes.Length - 1)
				{
					empty += ", ";
					empty += '\n';
				}
			}
			return "On Event speech: " + empty;
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
			base.agent.ListenForEvent(AIEventReceived, m_checkEventTypes[i].eventType);
		}
		tokens.Add(new SpeechManager.Token("$name", -1, bIsCharacterNetviewID: true));
		return base.OnInit();
	}

	private void AIEventReceived(AIEventMemory aiEventMemory)
	{
		Character character = base.agent.m_Character;
		if (character.GetIsKnockedOut() || character.GetIsDisabled() || character.GetIsSleeping())
		{
			return;
		}
		for (int i = 0; i < m_checkEventTypes.Length; i++)
		{
			if (m_checkEventTypes[i].eventType == aiEventMemory.m_AIEvent.m_EventData.m_eEventType)
			{
				tokens[0].m_ReplacementViewID = -1;
				if (aiEventMemory.m_AIEvent.m_CharacterResponsible != null && aiEventMemory.m_AIEvent.m_CharacterResponsible.m_CharacterCustomisation != null)
				{
					tokens[0].m_ReplacementViewID = aiEventMemory.m_AIEvent.m_CharacterResponsible.m_NetView.viewID;
				}
				bool flag = false;
				if (base.agent.m_Character.m_CharacterRole == CharacterRole.Guard && aiEventMemory.m_AIEvent.m_TargetCharacter != null && aiEventMemory.m_AIEvent.m_TargetCharacter.m_CharacterStats.m_bIsPlayer)
				{
					flag = true;
				}
				SpeechDecorations speechDecorationForEvent = GetSpeechDecorationForEvent(aiEventMemory, m_checkEventTypes[i]);
				int num = 6;
				if (flag)
				{
					num += 6;
				}
				if (speechDecorationForEvent == SpeechDecorations.FivePrisonStars)
				{
					num += 5;
				}
				SpeechManager instance = SpeechManager.GetInstance();
				Character character2 = base.agent.m_Character;
				string speech = m_checkEventTypes[i].speech;
				List<SpeechManager.Token> list = tokens;
				SpeechTone tone = SpeechTone.Negative;
				int priority = num;
				bool bAllowTextRecolour = flag;
				instance.SaySomething(character2, speech, list, tone, -1f, priority, -1, ignoreStatus: false, bAllowTextRecolour, speechDecorationForEvent);
				base.agent.m_Character.PauseMovement(1f);
				base.agent.m_Character.m_IconHandler.DisplayIconRPC(CharacterIconHandler.IconType.GuardAlert, 1f);
			}
		}
	}

	private SpeechDecorations GetSpeechDecorationForEvent(AIEventMemory aiEventMemory, EventTypeReaction typeReaction)
	{
		if ((!typeReaction.isDecorationForPlayersOnly || (aiEventMemory.m_AIEvent.m_TargetCharacter != null && aiEventMemory.m_AIEvent.m_TargetCharacter.m_CharacterStats.m_bIsPlayer)) && !aiEventMemory.m_AIEvent.IsFirstResponderSpeechClaimed())
		{
			bool flag = false;
			EventTypeReaction.DecorationConditions decorationCondition = typeReaction.decorationCondition;
			if (decorationCondition == EventTypeReaction.DecorationConditions.FullAlertness)
			{
				PrisonAlertness currentAlertness = PrisonAlertnessManager.GetInstance().GetCurrentAlertness();
				flag = currentAlertness == PrisonAlertness.Lockdown || currentAlertness == PrisonAlertness.FiveStars;
			}
			if (flag)
			{
				aiEventMemory.m_AIEvent.ClaimFirstResponderSpeech();
				return typeReaction.FirstResponderDecoration;
			}
		}
		return SpeechDecorations.None;
	}
}
