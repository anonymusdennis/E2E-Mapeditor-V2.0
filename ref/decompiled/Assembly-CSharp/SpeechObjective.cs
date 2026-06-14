using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class SpeechObjective : BaseObjective
{
	public enum SpeechObjectiveTalker
	{
		QuestGiver,
		Player,
		Guard,
		Inmate
	}

	public enum SpeechObjectiveReplaceType
	{
		QuestGiver,
		Player,
		Character,
		TextID
	}

	public class TokenInfo
	{
		public SpeechObjectiveReplaceType m_ReplaceType;

		public string m_Token = string.Empty;

		public ObjectiveSceneElement m_SceneReference;

		public string m_TextID = string.Empty;
	}

	public string m_SpeechBubbleText = "Text.Speech.Message";

	public int m_Variation = -1;

	public float m_ShowTime = 5f;

	public SpeechObjectiveTalker m_WhoTalks;

	public SpeechTone m_Tone;

	public int m_Priority = 10;

	public List<TokenInfo> m_ReplaceTokens = new List<TokenInfo>();

	private Character m_Speaker;

	private bool m_bHasSpoken;

	protected override void Child_PickAllTargets()
	{
		m_Speaker = GetSpeakingCharacter();
	}

	protected override void Child_RegisterTokens(ref ObjectiveTree objectiveTree)
	{
	}

	protected override void Child_Reset()
	{
	}

	protected override void Child_Initialize()
	{
	}

	protected override void Child_PreAction()
	{
		if (!(m_Speaker != null))
		{
			return;
		}
		if (m_ReplaceTokens == null || m_ReplaceTokens.Count == 0)
		{
			SpeechManager.GetInstance().SaySomething(m_Speaker, m_SpeechBubbleText, m_Tone, m_ShowTime, m_Priority, m_Variation);
			return;
		}
		List<SpeechManager.Token> list = new List<SpeechManager.Token>();
		for (int i = 0; i < m_ReplaceTokens.Count; i++)
		{
			switch (m_ReplaceTokens[i].m_ReplaceType)
			{
			case SpeechObjectiveReplaceType.QuestGiver:
			{
				SpeechManager.Token token3 = new SpeechManager.Token(m_ReplaceTokens[i].m_Token);
				token3.m_ReplacementViewID = m_QuestGiver.m_NetView.viewID;
				token3.m_bIsCharacterNetViewID = true;
				list.Add(token3);
				break;
			}
			case SpeechObjectiveReplaceType.Player:
			{
				SpeechManager.Token token4 = new SpeechManager.Token(m_ReplaceTokens[i].m_Token);
				token4.m_ReplacementViewID = m_PlayerOwner.m_NetView.viewID;
				token4.m_bIsCharacterNetViewID = true;
				list.Add(token4);
				break;
			}
			case SpeechObjectiveReplaceType.Character:
				if (m_ReplaceTokens[i].m_SceneReference != null && m_ReplaceTokens[i].m_SceneReference.m_LinksTo == ObjectiveSceneElement.ObjectiveSceneElementType.Character)
				{
					Character component = m_ReplaceTokens[i].m_SceneReference.GetComponent<Character>();
					if (component != null)
					{
						SpeechManager.Token token2 = new SpeechManager.Token(m_ReplaceTokens[i].m_Token);
						token2.m_ReplacementViewID = component.m_NetView.viewID;
						token2.m_bIsCharacterNetViewID = true;
						list.Add(token2);
					}
				}
				break;
			case SpeechObjectiveReplaceType.TextID:
			{
				SpeechManager.Token token = new SpeechManager.Token(m_ReplaceTokens[i].m_Token);
				token.m_ReplacementString = m_ReplaceTokens[i].m_TextID;
				token.m_bIsCharacterNetViewID = false;
				list.Add(token);
				break;
			}
			}
		}
		SpeechManager.GetInstance().SaySomething(m_Speaker, m_SpeechBubbleText, list, m_Tone, m_ShowTime, m_Priority, m_Variation);
	}

	protected override bool Child_EvaluateDependencies()
	{
		return true;
	}

	protected override bool Child_EvaluateStatus()
	{
		if (m_Speaker == null || !m_Speaker.IsSpeaking())
		{
			m_bHasSpoken = true;
		}
		return m_bHasSpoken;
	}

	protected override void Child_SetHUDPins(bool on)
	{
	}

	protected override void Child_SetHUDArrow(bool on)
	{
	}

	protected override void Child_PostAction()
	{
	}

	protected override string Child_Save(JObject baseObj, bool ingameSave)
	{
		if (ingameSave)
		{
			if (m_Speaker != null)
			{
				baseObj.Add(new JProperty("Speaker", m_Speaker.m_NetView.viewID));
			}
			baseObj.Add(new JProperty("HasSpoken", m_bHasSpoken));
		}
		baseObj.Add(new JProperty("SpeechText", m_SpeechBubbleText));
		baseObj.Add(new JProperty("ShowTime", m_ShowTime));
		baseObj.Add(new JProperty("Talker", (int)m_WhoTalks));
		baseObj.Add(new JProperty("Variation", m_Variation));
		baseObj.Add(new JProperty("Priority", m_Priority));
		baseObj.Add(new JProperty("Tone", (int)m_Tone));
		JProperty jProperty = new JProperty("Tokens");
		JArray jArray = new JArray();
		for (int i = 0; i < m_ReplaceTokens.Count; i++)
		{
			JObject jObject = new JObject();
			jObject.Add(new JProperty("Token", m_ReplaceTokens[i].m_Token));
			jObject.Add(new JProperty("Type", (int)m_ReplaceTokens[i].m_ReplaceType));
			jObject.Add(new JProperty("TextID", m_ReplaceTokens[i].m_TextID));
			if (m_ReplaceTokens[i].m_SceneReference != null)
			{
				jObject.Add(new JProperty("SceneRef_ID", m_ReplaceTokens[i].m_SceneReference.m_ObjectiveElementID));
				jObject.Add(new JProperty("SceneRef_Scene", m_ReplaceTokens[i].m_SceneReference.m_UsedInScene));
			}
			jArray.Add(jObject);
		}
		jProperty.Add(jArray);
		baseObj.Add(jProperty);
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject json, bool ingameLoad)
	{
		if (ingameLoad)
		{
			JProperty jProperty = json.Property("Speaker");
			if (jProperty != null)
			{
				int viewID = (int)jProperty.Value;
				m_Speaker = PhotonView.Find(viewID).GetComponent<Character>();
			}
			JProperty jProperty2 = json.Property("HasSpoken");
			if (jProperty2 != null)
			{
				m_bHasSpoken = (bool)jProperty2.Value;
			}
		}
		m_SpeechBubbleText = (string)json.Property("SpeechText").Value;
		m_ShowTime = (float)json.Property("ShowTime").Value;
		m_WhoTalks = (SpeechObjectiveTalker)(int)json.Property("Talker").Value;
		m_Variation = (int)json.Property("Variation").Value;
		m_Priority = (int)json.Property("Priority").Value;
		m_Tone = (SpeechTone)(int)json.Property("Tone").Value;
		JProperty jProperty3 = json.Property("Tokens");
		if (jProperty3 == null || jProperty3.Value.Type != JTokenType.Array)
		{
			return;
		}
		JArray jArray = (JArray)jProperty3.Value;
		m_ReplaceTokens.Clear();
		for (int i = 0; i < jArray.Count; i++)
		{
			if (jArray[i] == null || jArray[i].Type != JTokenType.Object)
			{
				continue;
			}
			JObject jObject = (JObject)jArray[i];
			TokenInfo tokenInfo = new TokenInfo();
			tokenInfo.m_Token = (string)jObject.Property("Token").Value;
			tokenInfo.m_ReplaceType = (SpeechObjectiveReplaceType)(int)jObject.Property("Type").Value;
			tokenInfo.m_TextID = (string)jObject.Property("TextID").Value;
			tokenInfo.m_SceneReference = null;
			JProperty jProperty4 = jObject.Property("SceneRef_ID");
			if (jProperty4 != null)
			{
				string text = (string)jObject.Property("SceneRef_Scene").Value;
				int id = (int)jProperty4.Value;
				if ((Application.isPlaying || !(text != SceneManagerHelper.ActiveSceneName)) && Application.isPlaying)
				{
					tokenInfo.m_SceneReference = ObjectiveSceneElement.FindSceneReference(id);
				}
			}
			m_ReplaceTokens.Add(tokenInfo);
		}
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.SpeechObjective;
	}

	private Character GetSpeakingCharacter()
	{
		Character result = null;
		switch (m_WhoTalks)
		{
		case SpeechObjectiveTalker.QuestGiver:
			result = m_QuestGiver;
			break;
		case SpeechObjectiveTalker.Player:
			result = m_PlayerOwner;
			break;
		case SpeechObjectiveTalker.Guard:
			result = QuestManager.GetInstance().GetRandomGuard();
			break;
		case SpeechObjectiveTalker.Inmate:
			result = QuestManager.GetInstance().GetRandomInmate(m_QuestGiver);
			break;
		}
		return result;
	}
}
