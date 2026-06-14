using System.Collections.Generic;
using UnityEngine;

public class SpeechManager : MonoBehaviour
{
	public class Token
	{
		public string m_Token;

		public int m_ReplacementViewID = -1;

		public string m_ReplacementString = string.Empty;

		public bool m_bIsCharacterNetViewID;

		public bool m_bIsRequireTranslating = true;

		public Token()
		{
		}

		public Token(string token)
		{
			m_Token = token;
		}

		public Token(string token, int replacementViewID, bool bIsCharacterNetviewID)
		{
			m_Token = token;
			m_ReplacementViewID = replacementViewID;
			m_ReplacementString = string.Empty;
			m_bIsCharacterNetViewID = bIsCharacterNetviewID;
		}

		public Token(string token, string replacementString, bool bIsCharacterNetviewID)
		{
			m_Token = token;
			m_ReplacementViewID = -1;
			m_ReplacementString = replacementString;
			m_bIsCharacterNetViewID = bIsCharacterNetviewID;
		}

		public Token(string token, string replacementString, bool bIsCharacterNetviewID, bool bIsRequireTranslating)
		{
			m_Token = token;
			m_ReplacementViewID = -1;
			m_ReplacementString = replacementString;
			m_bIsCharacterNetViewID = bIsCharacterNetviewID;
			m_bIsRequireTranslating = bIsRequireTranslating;
		}
	}

	private delegate void TokenReplacer(Character speaker, out int replacementViewID, out bool bIsLocalised);

	private class SpeechQueueEntry
	{
		public Character _speaker;

		public string _textID;

		public List<Token> _tokens;

		public SpeechTone _tone;

		public float _duration;

		public int _priority;

		public int _forcedVariation;

		public bool _bAllowTextRecolour;

		public SpeechDecorations _decoration;

		public SpeechQueueEntry(Character speaker, string textID, List<Token> tokens, SpeechTone tone, float duration, int priority, int forcedVariation, bool bAllowTextRecolour, SpeechDecorations decoration)
		{
			_speaker = speaker;
			_textID = textID;
			_tokens = tokens;
			_tone = tone;
			_duration = duration;
			_priority = priority;
			_forcedVariation = forcedVariation;
			_bAllowTextRecolour = bAllowTextRecolour;
			_decoration = decoration;
		}
	}

	private static SpeechManager m_Instance;

	private const float kDefaultSpeechLength = 3f;

	private Dictionary<string, TokenReplacer> m_TokenReplacerMap = new Dictionary<string, TokenReplacer>();

	private List<Character> m_AllGuards;

	private List<Character> m_AllInmates;

	private List<SpeechQueueEntry> m_SpeechQueue = new List<SpeechQueueEntry>();

	public static SpeechManager GetInstance()
	{
		return m_Instance;
	}

	private void Awake()
	{
		m_Instance = this;
		m_TokenReplacerMap["$guard"] = GetRandomGuardViewID;
		m_TokenReplacerMap["$inmate"] = GetRandomInmateViewID;
	}

	protected virtual void OnDestroy()
	{
		CleanUp();
		m_AllGuards = null;
		m_AllInmates = null;
		m_SpeechQueue = null;
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	public void CleanUp()
	{
		if (m_AllGuards != null)
		{
			int count = m_AllGuards.Count;
			for (int i = 0; i < count; i++)
			{
				m_AllGuards[i] = null;
			}
			m_AllGuards.Clear();
		}
		if (m_AllInmates != null)
		{
			int count2 = m_AllInmates.Count;
			for (int j = 0; j < count2; j++)
			{
				m_AllInmates[j] = null;
			}
			m_AllInmates.Clear();
		}
		if (m_SpeechQueue != null)
		{
			int count3 = m_SpeechQueue.Count;
			for (int k = 0; k < count3; k++)
			{
				m_SpeechQueue[k]._speaker = null;
				if (m_SpeechQueue[k]._tokens != null)
				{
					m_SpeechQueue[k]._tokens.Clear();
					m_SpeechQueue[k]._tokens = null;
				}
				m_SpeechQueue[k] = null;
			}
			m_SpeechQueue.Clear();
		}
		m_TokenReplacerMap.Clear();
	}

	private void Update()
	{
		ProcessSpeechQueue();
	}

	public void SaySomething(Character speaker, SpeechPODO speechInfo, bool bAllowTextRecolour = false)
	{
		if (speechInfo != null && speechInfo.IsSet())
		{
			string textId = speechInfo.m_TextId;
			SpeechTone speechTone = speechInfo.m_SpeechTone;
			float duration = speechInfo.m_Duration;
			int priority = speechInfo.m_Priority;
			int forcedVariation = speechInfo.m_ForcedVariation;
			bool bAllowTextRecolour2 = bAllowTextRecolour;
			SaySomething(speaker, textId, speechTone, duration, priority, forcedVariation, ignoreStatus: false, bAllowTextRecolour2);
		}
	}

	public void SaySomething(Character speaker, string textID, SpeechTone tone, float duration = -1f, int priority = 0, int forcedVariation = -1, bool ignoreStatus = false, bool bAllowTextRecolour = false)
	{
		SaySomething(speaker, textID, null, tone, duration, priority, forcedVariation, ignoreStatus, bAllowTextRecolour);
	}

	public void SaySomething(Character speaker, string textID, List<Token> tokens, SpeechTone tone, float duration = -1f, int priority = 0, int forcedVariation = -1, bool ignoreStatus = false, bool bAllowTextRecolour = false, SpeechDecorations decoration = SpeechDecorations.None)
	{
		if (!(speaker == null) && !string.IsNullOrEmpty(textID) && (ignoreStatus || (!speaker.GetIsKnockedOut() && !speaker.GetIsSleeping() && !speaker.m_bIsBound && !speaker.GetIsDisabled())) && !CutsceneManagerBase.IsACutscenePlaying())
		{
			SpeechQueueEntry item = new SpeechQueueEntry(speaker, textID, tokens, tone, duration, priority, forcedVariation, bAllowTextRecolour, decoration);
			m_SpeechQueue.Add(item);
		}
	}

	private void ProcessSpeechQueue()
	{
		if (m_SpeechQueue.Count == 0 || UpdateManager.IsHeavyCpuLocked())
		{
			return;
		}
		SpeechQueueEntry speechQueueEntry = m_SpeechQueue[0];
		m_SpeechQueue.RemoveAt(0);
		Character speaker = speechQueueEntry._speaker;
		string text = speechQueueEntry._textID;
		List<Token> list = speechQueueEntry._tokens;
		SpeechTone tone = speechQueueEntry._tone;
		float num = speechQueueEntry._duration;
		int priority = speechQueueEntry._priority;
		int forcedVariation = speechQueueEntry._forcedVariation;
		bool bAllowTextRecolour = speechQueueEntry._bAllowTextRecolour;
		SpeechDecorations decoration = speechQueueEntry._decoration;
		if (speaker.m_CharacterRole == CharacterRole.Dog)
		{
			text += ".Doggie";
		}
		string localized = string.Empty;
		if (Localization.Get(text, out localized, forcedVariation))
		{
			foreach (KeyValuePair<string, TokenReplacer> item in m_TokenReplacerMap)
			{
				if (localized.Contains(item.Key))
				{
					if (list == null)
					{
						list = new List<Token>();
					}
					if (list.Find((Token x) => x.m_Token == item.Key) == null)
					{
						Token token = new Token(item.Key);
						item.Value(speaker, out token.m_ReplacementViewID, out token.m_bIsCharacterNetViewID);
						list.Add(token);
					}
				}
			}
		}
		if (num <= 0f)
		{
			num = 3f;
		}
		if (list == null || list.Count == 0)
		{
			speaker.m_NetView.GameplayRPC("RPC_SaySomething", NetTargets.All, text, tone, num, priority, forcedVariation, bAllowTextRecolour, decoration);
		}
		else if (list.Count == 1)
		{
			if (list[0].m_bIsCharacterNetViewID)
			{
				speaker.m_NetView.GameplayRPC("RPC_SaySomethingWithReplaceNetViewID", NetTargets.All, text, list[0].m_Token, list[0].m_ReplacementViewID, tone, num, priority, forcedVariation, bAllowTextRecolour, decoration);
			}
			else if (list[0].m_bIsRequireTranslating)
			{
				speaker.m_NetView.GameplayRPC("RPC_SaySomethingWithReplaceString", NetTargets.All, text, list[0].m_Token, list[0].m_ReplacementString, tone, num, priority, forcedVariation, bAllowTextRecolour, decoration);
			}
			else
			{
				speaker.m_NetView.GameplayRPC("RPC_SaySomethingWithDirectReplaceString", NetTargets.All, text, list[0].m_Token, list[0].m_ReplacementString, tone, num, priority, forcedVariation, bAllowTextRecolour, decoration);
			}
		}
		else if (list.Count == 2)
		{
			speaker.m_NetView.GameplayRPC("RPC_SaySomethingWithReplacements", NetTargets.All, text, list[0].m_Token, (!list[0].m_bIsCharacterNetViewID) ? list[0].m_ReplacementString : ((object)list[0].m_ReplacementViewID), list[0].m_bIsCharacterNetViewID, list[1].m_Token, (!list[1].m_bIsCharacterNetViewID) ? list[1].m_ReplacementString : ((object)list[1].m_ReplacementViewID), list[1].m_bIsCharacterNetViewID, tone, num, priority, forcedVariation, bAllowTextRecolour, decoration);
		}
	}

	public void SaySomethingUsingOpinion(Character speaker, Character subject, string textID, SpeechTone tone, float duration = -1f, int priority = 0, int forcedVariation = -1)
	{
		SaySomethingUsingOpinion(speaker, subject, textID, null, tone, duration, priority, forcedVariation);
	}

	public void SaySomethingUsingOpinion(Character speaker, Character subject, string textID, List<Token> tokens, SpeechTone tone, float duration = -1f, int priority = 0, int forcedVariation = -1)
	{
		if (!(speaker == null) && !(subject == null) && !string.IsNullOrEmpty(textID))
		{
			textID = textID + "Rep" + speaker.GetOpinionOf(subject);
			SaySomething(speaker, textID, tokens, tone, duration, priority, forcedVariation);
		}
	}

	private void PopulateCharacterLists()
	{
		m_AllGuards = new List<Character>();
		m_AllInmates = new List<Character>();
		List<Character> allCharacters = Character.GetAllCharacters();
		int count = allCharacters.Count;
		for (int i = 0; i < count; i++)
		{
			if (allCharacters[i].m_CharacterRole == CharacterRole.Guard)
			{
				m_AllGuards.Add(allCharacters[i]);
			}
			else if (allCharacters[i].m_CharacterRole == CharacterRole.Inmate)
			{
				m_AllInmates.Add(allCharacters[i]);
			}
		}
	}

	private void GetRandomGuardViewID(Character speaker, out int characterViewID, out bool bIsLocalised)
	{
		if (m_AllGuards == null || m_AllInmates == null)
		{
			PopulateCharacterLists();
		}
		characterViewID = speaker.m_NetView.viewID;
		if (m_AllGuards.Count > 1)
		{
			do
			{
				int index = Random.Range(0, m_AllGuards.Count);
				characterViewID = m_AllGuards[index].m_NetView.viewID;
			}
			while (characterViewID == speaker.m_NetView.viewID);
		}
		bIsLocalised = true;
	}

	private void GetRandomInmateViewID(Character speaker, out int characterViewID, out bool bIsLocalised)
	{
		if (m_AllGuards == null || m_AllInmates == null)
		{
			PopulateCharacterLists();
		}
		characterViewID = speaker.m_NetView.viewID;
		if (m_AllInmates.Count > 1)
		{
			do
			{
				int index = Random.Range(0, m_AllInmates.Count);
				characterViewID = m_AllInmates[index].m_NetView.viewID;
			}
			while (characterViewID == speaker.m_NetView.viewID);
		}
		bIsLocalised = true;
	}
}
