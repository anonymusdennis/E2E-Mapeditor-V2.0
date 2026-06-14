using System.Collections.Generic;
using NodeCanvas.BehaviourTrees;
using UnityEngine;

public class AICharacter_JobOfficer : AICharacter
{
	private static List<int> m_CharactersWeHasSpokenTo = new List<int>();

	protected override void OnAwake()
	{
		AssetBundleBehaviourOverride();
	}

	protected override void OnStart()
	{
		if (m_CharactersWeHasSpokenTo == null)
		{
			m_CharactersWeHasSpokenTo = new List<int>();
		}
		RoutineManager.GetInstance().OnRoutineEnded += ClearCharacterList;
	}

	private void AssetBundleBehaviourOverride()
	{
		BehaviourTreeOwner component = GetComponent<BehaviourTreeOwner>();
		if (!(component == null))
		{
			string text = ((Object)component.graph).name;
			text = text.Replace("(Clone)", string.Empty);
			object assetFromBundle = AssetManager.instance.GetAssetFromBundle("aibehaviours", text);
			if (assetFromBundle != null && assetFromBundle is BehaviourTree)
			{
				BehaviourTree newGraph = assetFromBundle as BehaviourTree;
				component.SwitchBehaviour(newGraph);
			}
		}
	}

	public void ClearCharacterList(RoutinesData.Routine routine, bool forceEnd)
	{
		if (m_CharactersWeHasSpokenTo.Count > 0)
		{
			m_CharactersWeHasSpokenTo.Clear();
		}
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		if (LevelScript.GetInstance() != null && LevelScript.GetInstance().m_LevelSetup != null && LevelScript.GetInstance().m_LevelSetup.m_LevelInfo.m_PrisonEnum == LevelScript.PRISON_ENUM.CustomPrison && m_AIMovement != null)
		{
			m_AIMovement.ClearAllowedDoors();
		}
		return base.StartInit();
	}

	public static void Cleanup()
	{
		if (m_CharactersWeHasSpokenTo != null)
		{
			m_CharactersWeHasSpokenTo.Clear();
			m_CharactersWeHasSpokenTo = null;
		}
	}

	public void GiveReward(int m_TargetCharacterID)
	{
		Character character = T17NetView.Find<Character>(m_TargetCharacterID);
		if (!(character == null))
		{
			character.m_CharacterStats.IncreaseMoney(ConfigManager.GetInstance().jobConfig.m_MoneyReward);
			if (character.m_CharacterStats.m_bIsPlayer)
			{
				EffectManager.GetInstance().NewEffectInstanceRPC(EffectManager.effectType.MoneyIncreased, character.GetStatChangeEffectPosition());
			}
		}
	}

	public bool CanSpeekToCharacter(int targetCharacterID)
	{
		if (!CharacterOnTime())
		{
			return false;
		}
		for (int i = 0; i < m_CharactersWeHasSpokenTo.Count; i++)
		{
			if (m_CharactersWeHasSpokenTo[i] == targetCharacterID)
			{
				return false;
			}
		}
		if (CharacterHaveJob(targetCharacterID))
		{
			return false;
		}
		return true;
	}

	public void CharacterSpokenTo(int character)
	{
		m_CharactersWeHasSpokenTo.Add(character);
	}

	public static bool CharacterOnTime()
	{
		return RoutineManager.GetInstance().GetCurrentRoutineSubType() == RoutineSubTypes.JobTime;
	}

	public static bool CharacterHaveJob(int targetCharacterID)
	{
		Character character = T17NetView.Find<Character>(targetCharacterID);
		if (character == null)
		{
			return false;
		}
		BaseJob charactersJob = JobsManager.GetInstance().GetCharactersJob(character);
		if (charactersJob == null || (charactersJob != null && charactersJob.m_Type == JobType.Invalid))
		{
			return false;
		}
		return true;
	}

	public List<string> GetSpeechLines()
	{
		JobConfig.SpeechLines[] speechLines = ConfigManager.GetInstance().jobConfig.m_SpeechLines;
		if (speechLines == null || speechLines.Length == 0)
		{
			List<string> list = new List<string>();
			list.Add("No Speech Lines");
			return list;
		}
		JobConfig.SpeechLines speechLines2 = speechLines[Random.Range(0, speechLines.Length)];
		if (speechLines2 == null || speechLines2.lines == null || speechLines2.lines.Count == 0)
		{
			List<string> list = new List<string>();
			list.Add("Speech Line was empty!");
			return list;
		}
		return speechLines2.lines;
	}
}
