using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public abstract class BaseObjective
{
	public delegate void ObjectiveEvent();

	public enum ObjectiveType
	{
		Invalid,
		ItemObjective,
		CraftObjective,
		InteractObjective,
		DialogObjective,
		TriggerObjective,
		SpeechObjective,
		InventoryObjective,
		PassiveDialogObjective,
		CombatObjective,
		OutfitObjective,
		DestroyItemObjective,
		QuestIntroObjective,
		ToiletFloodOjbective,
		JobDistruptionObjective,
		TutorialCompleteObjective,
		SetObjectiveArrowObjective,
		SpawnVendorObjective,
		UseItemObjective,
		WaitUntilTimeObjective,
		PlayEscapeCutsceneObjective,
		EnableMultistageInteractionObjective,
		SetRoutineObjective,
		EnableInputObjective,
		DamageTileObjective,
		SwapBehaviourObjective,
		MoveDeskObjective,
		TutorialSpeechObjective,
		TutorialGuidedUIObjective,
		EnableInteractionObjective,
		EnableInGameMenuObjective,
		SetUsableItemObjective
	}

	public ObjectiveEvent OnObjectiveComplete;

	public ObjectiveEvent OnObjectiveFailed;

	public ObjectiveEvent OnObjectiveCanceled;

	public string m_NameLocalizationTag = "Text.Objective.Name";

	public string m_DescriptionLocalizationTag = "Text.Objective.Name.Desc";

	public bool m_bIsLogable = true;

	public bool m_bResetWhenRetriggered;

	public Sprite m_ObjectiveIcon;

	public Dictionary<string, string> m_AvailableTokens;

	protected ObjectiveTree m_ParentObjectiveTree;

	protected Character m_QuestGiver;

	protected Player m_PlayerOwner;

	protected ObjectiveStatus m_ObjectiveStatus;

	protected bool m_bIsADependency;

	protected bool m_bHasRandomInformation;

	protected bool m_bIsInitialized;

	protected bool m_bInPostAction;

	protected string m_LocalizedObjectiveName = string.Empty;

	protected string m_LocalizedDescription = string.Empty;

	protected bool m_bPinsOn;

	protected bool m_bArrowOn;

	private int m_ResetToIndex;

	private int m_VerifyPreviousObjectiveIndex = -1;

	private const string PLAYERTOKEN = "$Player";

	private const string QUESTGIVERTOKEN = "$QuestGiver";

	private static bool LoadedObjectiveIconsFromResources;

	private static Sprite[] LoadedObjectiveIcons;

	public string LocalizedObjectiveName
	{
		get
		{
			if (m_ParentObjectiveTree != null && string.IsNullOrEmpty(m_LocalizedObjectiveName))
			{
				m_LocalizedObjectiveName = m_ParentObjectiveTree.GetTokenizedLocalization(this, m_NameLocalizationTag);
			}
			return m_LocalizedObjectiveName;
		}
	}

	public string LocalizedDescription
	{
		get
		{
			if (m_ParentObjectiveTree != null && string.IsNullOrEmpty(m_LocalizedDescription))
			{
				m_LocalizedDescription = m_ParentObjectiveTree.GetTokenizedLocalization(this, m_DescriptionLocalizationTag);
			}
			return m_LocalizedDescription;
		}
	}

	public int ResetToIndex => m_ResetToIndex;

	public int VerifyPreviousObjectiveIndex => m_VerifyPreviousObjectiveIndex;

	public Player PlayerOwner => m_PlayerOwner;

	protected abstract void Child_PickAllTargets();

	protected abstract void Child_Reset();

	protected abstract void Child_Initialize();

	protected abstract void Child_PreAction();

	protected abstract bool Child_EvaluateDependencies();

	protected abstract bool Child_EvaluateStatus();

	protected abstract void Child_PostAction();

	protected abstract void Child_Load(JObject baseObj, bool ingameLoad);

	protected abstract string Child_Save(JObject baseObj, bool ingameSave);

	protected abstract void Child_SetHUDPins(bool on);

	protected abstract void Child_SetHUDArrow(bool on);

	protected abstract void Child_RegisterTokens(ref ObjectiveTree objectiveTree);

	protected virtual int Child_EvaluateResetCondition()
	{
		return -1;
	}

	public abstract ObjectiveType GetObjectiveType();

	public void SetPlayerOwner(Player player)
	{
		m_PlayerOwner = player;
		if (!(m_PlayerOwner == null))
		{
		}
	}

	public void SetQuestGiver(Character questGiver)
	{
		m_QuestGiver = questGiver;
		if (!(m_QuestGiver == null))
		{
		}
	}

	public void SetDependencyStatus(bool isADependency)
	{
		m_bIsADependency = isADependency;
	}

	public void RegisterTokens(ref ObjectiveTree objectiveTree)
	{
		m_ParentObjectiveTree = objectiveTree;
		if (m_AvailableTokens == null)
		{
			m_AvailableTokens = new Dictionary<string, string>();
		}
		AddTokenInternal("$Player", Localization.TokenReplaceType.Player);
		AddTokenInternal("$QuestGiver", Localization.TokenReplaceType.QuestGiver);
		Child_RegisterTokens(ref objectiveTree);
	}

	public ObjectiveStatus GetObjectiveStatus()
	{
		return m_ObjectiveStatus;
	}

	private bool IsBaseInfoSet()
	{
		return m_PlayerOwner != null && m_QuestGiver != null;
	}

	public void PickAllTargets()
	{
		m_bHasRandomInformation = false;
		if (IsBaseInfoSet())
		{
			InternalTokenUpdate("$Player", m_PlayerOwner.m_CharacterCustomisation.m_DisplayName, string.Empty);
			InternalTokenUpdate("$QuestGiver", m_QuestGiver.m_CharacterCustomisation.m_DisplayName, string.Empty);
			Child_PickAllTargets();
		}
	}

	protected void AddTokenInternal(string token, Localization.TokenReplaceType replaceType)
	{
		if (m_ParentObjectiveTree != null)
		{
			if (m_AvailableTokens == null)
			{
				m_AvailableTokens = new Dictionary<string, string>();
			}
			string value = ((!m_AvailableTokens.TryGetValue(token, out value)) ? string.Empty : value);
			m_ParentObjectiveTree.AddObjectiveStringToken(token, replaceType, ref value);
			if (!string.IsNullOrEmpty(value))
			{
				m_AvailableTokens.Add(token, value);
			}
		}
	}

	protected void InternalTokenUpdate(string tokenKey, string tokenReplacement, string tokenReplacementTag = "")
	{
		if (m_ParentObjectiveTree == null || !m_AvailableTokens.TryGetValue(tokenKey, out var value))
		{
			return;
		}
		Localization.TokenInfo token = null;
		if (m_ParentObjectiveTree.GetObjectiveToken(value, out token))
		{
			if (token.m_ReplaceType != Localization.TokenReplaceType.TextID && token.m_SceneReference == null)
			{
				token.m_TextIDTag = tokenReplacementTag;
				token.m_TextID = tokenReplacement;
			}
			else if (token.m_ReplaceType == Localization.TokenReplaceType.TextID)
			{
				if (string.IsNullOrEmpty(token.m_TextID))
				{
					token.m_TextID = tokenReplacement;
				}
				if (Localization.Get(token.m_TextID, out var localized))
				{
					token.m_TextIDTag = token.m_TextID;
					token.m_TextID = localized;
				}
				else
				{
					token.m_TextID = "[TBT]" + token.m_TextID;
				}
			}
			else if (token.m_SceneReference != null)
			{
				switch (token.m_SceneReference.m_LinksTo)
				{
				case ObjectiveSceneElement.ObjectiveSceneElementType.ItemContainer:
					if (token.m_SceneReference.GetComponent<ItemContainer>().GetCharacterOwner() != null)
					{
						token.m_TextID = token.m_SceneReference.GetComponent<ItemContainer>().GetCharacterOwner().m_CharacterCustomisation.m_DisplayName;
					}
					else
					{
						token.m_TextID = "TBR: Not Owned Container";
					}
					break;
				case ObjectiveSceneElement.ObjectiveSceneElementType.Character:
					token.m_TextID = token.m_SceneReference.GetComponent<Character>().m_CharacterCustomisation.m_DisplayName;
					break;
				case ObjectiveSceneElement.ObjectiveSceneElementType.GameObject:
					token.m_TextID = "Unsupported GameObject " + token.m_SceneReference.gameObject.name;
					break;
				case ObjectiveSceneElement.ObjectiveSceneElementType.Collider:
					token.m_TextID = "Unsupported Collider " + token.m_SceneReference.GetComponent<Collider>().name;
					break;
				case ObjectiveSceneElement.ObjectiveSceneElementType.InteractiveObject:
					token.m_TextID = token.m_SceneReference.GetComponent<InteractiveObject>().m_NetObjectLock.m_InteractActionNameTag;
					break;
				}
			}
		}
		m_ParentObjectiveTree.UpdateObjectiveToken(value, token);
	}

	public void Initialize()
	{
		m_bIsInitialized = false;
		if (IsBaseInfoSet())
		{
			Child_Reset();
			Child_Initialize();
			m_bIsInitialized = true;
		}
	}

	public void Reset()
	{
		if (IsBaseInfoSet())
		{
			m_bInPostAction = false;
			m_bPinsOn = false;
			m_bArrowOn = false;
			Child_SetHUDPins(on: false);
			Child_SetHUDArrow(on: false);
			Child_Reset();
		}
	}

	public bool EvaluateStatus()
	{
		if (!IsBaseInfoSet())
		{
			return false;
		}
		if (m_ObjectiveStatus != ObjectiveStatus.Done)
		{
			bool flag = Child_EvaluateStatus();
			if (flag)
			{
				m_ObjectiveStatus = ObjectiveStatus.Done;
				if (OnObjectiveComplete != null)
				{
					OnObjectiveComplete();
					OnObjectiveComplete = null;
				}
			}
			else if (m_ObjectiveStatus == ObjectiveStatus.InComplete || m_ObjectiveStatus == ObjectiveStatus.Reset)
			{
				m_ResetToIndex = Child_EvaluateResetCondition();
				if (m_ResetToIndex != -1)
				{
					m_ObjectiveStatus = ObjectiveStatus.Reset;
					return false;
				}
			}
			else if (m_ObjectiveStatus == ObjectiveStatus.Canceled)
			{
				if (OnObjectiveCanceled != null)
				{
					OnObjectiveCanceled();
				}
			}
			else if (m_ObjectiveStatus == ObjectiveStatus.Failed && OnObjectiveFailed != null)
			{
				OnObjectiveFailed();
			}
			return flag;
		}
		return false;
	}

	public void SetToIncomplete()
	{
		m_ObjectiveStatus = ObjectiveStatus.InComplete;
	}

	public bool EvaluateIsComplete()
	{
		if (!IsBaseInfoSet())
		{
			return false;
		}
		return Child_EvaluateStatus();
	}

	public virtual void ClearHUDInfo()
	{
	}

	public virtual int SetHUDInfo(ref ObjectiveSubGoalHUD[] infoList)
	{
		if (infoList.Length > 0 && infoList[0] != null)
		{
			infoList[0].Show(m_PlayerOwner);
			infoList[0].SetObjective(this);
		}
		return 1;
	}

	public virtual void SetJournalMenuInfo(T17Text textObject)
	{
		textObject.gameObject.SetActive(value: true);
		textObject.m_bNeedsLocalization = false;
		textObject.text = (string.IsNullOrEmpty(m_LocalizedDescription) ? string.Empty : m_LocalizedDescription);
	}

	public void SetHUDPins(bool on)
	{
		if (IsBaseInfoSet() && on != m_bPinsOn)
		{
			m_bPinsOn = on;
			Child_SetHUDPins(on);
		}
	}

	public void SetHUDArrow(bool on)
	{
		if (IsBaseInfoSet() && on != m_bArrowOn)
		{
			m_bArrowOn = on;
			Child_SetHUDArrow(on);
		}
	}

	public void PreAction()
	{
		if (IsBaseInfoSet())
		{
			if (m_ObjectiveStatus == ObjectiveStatus.Done && m_bResetWhenRetriggered)
			{
				Reset();
			}
			m_ObjectiveStatus = ObjectiveStatus.InComplete;
			Child_PreAction();
		}
	}

	public void PostAction()
	{
		if (IsBaseInfoSet())
		{
			m_bInPostAction = true;
			Child_SetHUDPins(on: false);
			Child_SetHUDArrow(on: false);
			Child_PostAction();
		}
	}

	private void BaseLoad(JObject baseObj, bool ingameLoad)
	{
		if (baseObj == null)
		{
			return;
		}
		m_NameLocalizationTag = (string)baseObj.Property("NameLocaTag").Value;
		m_DescriptionLocalizationTag = (string)baseObj.Property("DescLocaTag").Value;
		if (!LoadedObjectiveIconsFromResources)
		{
			LoadedObjectiveIconsFromResources = true;
			LoadedObjectiveIcons = Resources.LoadAll<Sprite>("ObjectiveFiles/UISprites");
		}
		if (LoadedObjectiveIconsFromResources && LoadedObjectiveIcons != null)
		{
			for (int i = 0; i < LoadedObjectiveIcons.Length; i++)
			{
				if (LoadedObjectiveIcons[i].name == (string)baseObj.Property("IconName").Value)
				{
					m_ObjectiveIcon = LoadedObjectiveIcons[i];
					break;
				}
			}
		}
		if (baseObj.Property("ObjectiveStatus") != null)
		{
			m_ObjectiveStatus = (ObjectiveStatus)(int)baseObj.Property("ObjectiveStatus").Value;
		}
		if (baseObj.Property("ResetWhenRetriggered") != null)
		{
			m_bResetWhenRetriggered = (bool)baseObj.Property("ResetWhenRetriggered").Value;
		}
		if (baseObj.Property("ShowInTracker") != null)
		{
			m_bIsLogable = (bool)baseObj.Property("ShowInTracker").Value;
		}
		if (baseObj.Property("VerifyPreviousObjectiveIndex") != null)
		{
			m_VerifyPreviousObjectiveIndex = (int)baseObj.Property("VerifyPreviousObjectiveIndex").Value;
		}
		if (!ObjectiveTree.DEBUG_DONT_LOAD_TOKENS)
		{
			JProperty jProperty = baseObj.Property("Tokens");
			if (jProperty != null && jProperty.Value.Type == JTokenType.Array)
			{
				JArray jArray = (JArray)jProperty.Value;
				if (m_AvailableTokens != null)
				{
					m_AvailableTokens.Clear();
				}
				else
				{
					m_AvailableTokens = new Dictionary<string, string>();
				}
				for (int j = 0; j < jArray.Count; j++)
				{
					if (jArray[j] != null && jArray[j].Type == JTokenType.Object)
					{
						JObject jObject = (JObject)jArray[j];
						JProperty jProperty2 = jObject.Properties().ElementAt(0);
						m_AvailableTokens.Add(jProperty2.Name, (string)jProperty2.Value);
					}
				}
			}
		}
		if (ingameLoad)
		{
			JProperty jProperty3 = baseObj.Property("QuestGiver");
			if (jProperty3 != null)
			{
				int viewID = (int)jProperty3.Value;
				m_QuestGiver = PhotonView.Find(viewID).GetComponent<Character>();
			}
			JProperty jProperty4 = baseObj.Property("PlayerOwner");
			if (jProperty4 != null)
			{
				int viewID2 = (int)jProperty4.Value;
				m_PlayerOwner = PhotonView.Find(viewID2).GetComponent<Player>();
			}
			InternalTokenUpdate("$Player", m_PlayerOwner.m_CharacterCustomisation.m_DisplayName, string.Empty);
			InternalTokenUpdate("$QuestGiver", m_QuestGiver.m_CharacterCustomisation.m_DisplayName, string.Empty);
		}
	}

	private JObject BaseSave(bool ingameSave)
	{
		JObject jObject = new JObject(new JProperty("NameLocaTag", m_NameLocalizationTag), new JProperty("DescLocaTag", m_DescriptionLocalizationTag), new JProperty("IconName", (!(m_ObjectiveIcon != null)) ? "Empty" : m_ObjectiveIcon.name), new JProperty("ObjectiveStatus", (int)m_ObjectiveStatus), new JProperty("ResetWhenRetriggered", m_bResetWhenRetriggered), new JProperty("ShowInTracker", m_bIsLogable), new JProperty("VerifyPreviousObjectiveIndex", m_VerifyPreviousObjectiveIndex));
		if (m_AvailableTokens != null)
		{
			JProperty jProperty = new JProperty("Tokens");
			JArray jArray = new JArray();
			foreach (KeyValuePair<string, string> availableToken in m_AvailableTokens)
			{
				JObject jObject2 = new JObject();
				jObject2.Add(new JProperty(availableToken.Key, availableToken.Value));
				jArray.Add(jObject2);
			}
			jProperty.Add(jArray);
			jObject.Add(jProperty);
		}
		if (ingameSave)
		{
			if (m_QuestGiver != null)
			{
				jObject.Add(new JProperty("QuestGiver", m_QuestGiver.m_NetView.viewID));
			}
			if (m_PlayerOwner != null)
			{
				jObject.Add(new JProperty("PlayerOwner", m_PlayerOwner.m_NetView.viewID));
			}
		}
		return jObject;
	}

	public string Save(bool ingameSave)
	{
		return Child_Save(BaseSave(ingameSave), ingameSave);
	}

	public void Load(JObject json, bool ingameLoad, bool bUpdateNetworkService = false)
	{
		Type type = GetType();
		string text = type.ToString();
		if (bUpdateNetworkService)
		{
			GlobalStart.TimedNetworkService();
		}
		BaseLoad(json, ingameLoad);
		if (bUpdateNetworkService)
		{
			GlobalStart.TimedNetworkService();
		}
		Child_Load(json, ingameLoad);
		if (bUpdateNetworkService)
		{
			GlobalStart.TimedNetworkService();
		}
	}
}
