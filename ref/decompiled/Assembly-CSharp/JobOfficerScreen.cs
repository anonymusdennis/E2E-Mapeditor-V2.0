using System.Collections.Generic;
using UnityEngine;

public class JobOfficerScreen : FakeCharacter
{
	private enum JOS_STAGE
	{
		JOS_S_WAITING_FOR_CHARACTER,
		JOS_S_GET_LINES_TO_SAY,
		JOS_S_TALKING,
		JOS_S_REWARD
	}

	public InteractiveObject m_Chair;

	public float m_FirstDelayVar = 5f;

	public float m_RepeatDelayVar = 5f;

	public float m_SpeechTime = 3f;

	private JOS_STAGE m_Stage;

	private List<string> m_Lines;

	private int m_CurrentLine;

	private static List<int> m_CharactersWeHaveSpokenTo = new List<int>();

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		T17BehaviourManager.INITSTATE result = base.StartInit();
		SetNextTalkTime(bFirstTime: true);
		m_Stage = JOS_STAGE.JOS_S_WAITING_FOR_CHARACTER;
		RoutineManager.GetInstance().OnRoutineEnded += ClearCharacterList;
		return result;
	}

	public void ClearCharacterList(RoutinesData.Routine routine, bool forceEnd)
	{
		m_CharactersWeHaveSpokenTo.Clear();
	}

	public static void Cleanup()
	{
		m_CharactersWeHaveSpokenTo.Clear();
	}

	public override void ControlledUpdate()
	{
		if (!(m_Chair != null) || !(RoutineManager.GetInstance() != null) || RoutineManager.GetInstance().GetCurrentRoutine() == null)
		{
			return;
		}
		int num = -1;
		if (RoutineManager.GetInstance().GetCurrentRoutine().m_SubRoutineType == RoutineSubTypes.JobTime && m_Chair.GetLocalInteractingCharacter() != null && m_Chair.GetLocalInteractingCharacter().m_NetView.isMine)
		{
			num = m_Chair.GetLocalInteractingCharacter().m_NetView.viewID;
		}
		else if (m_Stage != 0)
		{
			m_Stage = JOS_STAGE.JOS_S_WAITING_FOR_CHARACTER;
		}
		switch (m_Stage)
		{
		case JOS_STAGE.JOS_S_WAITING_FOR_CHARACTER:
		{
			if (num <= -1)
			{
				break;
			}
			bool flag = true;
			if (AICharacter_JobOfficer.CharacterHaveJob(num))
			{
				flag = false;
			}
			else
			{
				for (int i = 0; i < m_CharactersWeHaveSpokenTo.Count; i++)
				{
					if (m_CharactersWeHaveSpokenTo[i] == num)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				m_Stage = JOS_STAGE.JOS_S_GET_LINES_TO_SAY;
			}
			break;
		}
		case JOS_STAGE.JOS_S_GET_LINES_TO_SAY:
		{
			JobConfig.SpeechLines[] speechLines = ConfigManager.GetInstance().jobConfig.m_SpeechLines;
			m_CurrentLine = 0;
			if (speechLines == null || speechLines.Length == 0)
			{
				m_Lines = new List<string> { "No Speech Lines" };
			}
			else
			{
				JobConfig.SpeechLines speechLines2 = speechLines[Random.Range(0, speechLines.Length)];
				if (speechLines2 == null || speechLines2.lines == null || speechLines2.lines.Count == 0)
				{
					m_Lines = new List<string> { "Speech Line was empty!" };
				}
				else
				{
					m_Lines = speechLines2.lines;
				}
			}
			m_Stage = JOS_STAGE.JOS_S_TALKING;
			break;
		}
		case JOS_STAGE.JOS_S_TALKING:
			if (m_CurrentLine < m_Lines.Count)
			{
				if (m_SpeechBubbleHandler != null && !m_SpeechBubbleHandler.IsProcessingSpeech())
				{
					string localized = string.Empty;
					if (Localization.Get(m_Lines[m_CurrentLine], out localized))
					{
						m_SpeechBubbleHandler.NewSpeech(localized, SpeechTone.Computer_Positive, 3f, 10, bAllowTextColourControl: false);
					}
					m_CurrentLine++;
				}
			}
			else
			{
				m_Stage = JOS_STAGE.JOS_S_REWARD;
			}
			break;
		case JOS_STAGE.JOS_S_REWARD:
		{
			Character localInteractingCharacter = m_Chair.GetLocalInteractingCharacter();
			if (localInteractingCharacter != null)
			{
				localInteractingCharacter.m_CharacterStats.IncreaseMoney(ConfigManager.GetInstance().jobConfig.m_MoneyReward);
				if (localInteractingCharacter.m_CharacterStats.m_bIsPlayer)
				{
					EffectManager.GetInstance().NewEffectInstanceRPC(EffectManager.effectType.MoneyIncreased, localInteractingCharacter.GetStatChangeEffectPosition());
				}
				m_CharactersWeHaveSpokenTo.Add(localInteractingCharacter.m_NetView.viewID);
			}
			m_Stage = JOS_STAGE.JOS_S_WAITING_FOR_CHARACTER;
			break;
		}
		}
	}

	private void SetNextTalkTime(bool bFirstTime)
	{
		if (!bFirstTime)
		{
		}
	}
}
