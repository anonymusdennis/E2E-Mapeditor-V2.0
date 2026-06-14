using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

public class ObjectiveGoal
{
	public BaseObjective m_Objective;

	public string m_ObjectiveGoalLocaTag = "Objective Goal";

	public bool ResetToPreviousDependency;

	public bool m_AnalyticOnStart;

	public bool m_AnalyticOnEnd;

	private string m_AnalyticsStartCategory = string.Empty;

	private string m_AnalyticsStartAction = string.Empty;

	private string m_AnalyticsStartLabel = string.Empty;

	private string m_AnalyticsEndCategory = string.Empty;

	private string m_AnalyticsEndAction = string.Empty;

	private string m_AnalyticsEndLabel = string.Empty;

	private bool m_bAnalyticsStartedCalled;

	private bool m_bAnalyticsEndedCalled;

	private List<int> m_LinksToNodes = new List<int>();

	private int m_DependencyNode = -1;

	private int m_NodeID = -1;

	private bool m_bIsCurrentlyActive;

	private bool m_bIsADependency;

	public List<int> LinksTo => m_LinksToNodes;

	public int DependencyNode => m_DependencyNode;

	public int NodeID => m_NodeID;

	public bool CurrentlyActive => m_bIsCurrentlyActive;

	public bool IsADependency => m_bIsADependency;

	public bool IsLogable()
	{
		return m_Objective.m_bIsLogable;
	}

	public void SetObjectivePins(bool on)
	{
		m_Objective.SetHUDPins(on);
	}

	public void SetObjectiveArrow(bool on)
	{
		m_Objective.SetHUDArrow(on);
	}

	public void ClearHUDInfo()
	{
		m_Objective.ClearHUDInfo();
	}

	public int UpdateHUDInfo(ref ObjectiveSubGoalHUD[] infoList)
	{
		return m_Objective.SetHUDInfo(ref infoList);
	}

	public void UpdateMenuJournalInfo(T17Text textObj)
	{
		m_Objective.SetJournalMenuInfo(textObj);
	}

	public void RegisterTokens(ObjectiveTree objectiveTree)
	{
		if (m_Objective != null)
		{
			m_Objective.RegisterTokens(ref objectiveTree);
		}
	}

	public ObjectiveStatus EvaluateObjectives()
	{
		if (!(m_Objective.PlayerOwner != null) || m_Objective.PlayerOwner.m_Gamer == null || m_Objective.EvaluateStatus())
		{
		}
		return m_Objective.GetObjectiveStatus();
	}

	public bool EvaluateObjectiveComplete()
	{
		if (m_Objective != null && m_Objective.PlayerOwner != null && m_Objective.PlayerOwner.m_Gamer != null)
		{
			return m_Objective.EvaluateIsComplete();
		}
		return false;
	}

	public void SetAsDependency()
	{
		m_bIsADependency = true;
		m_Objective.SetDependencyStatus(isADependency: true);
	}

	public void Initialize()
	{
		m_Objective.Initialize();
	}

	public void PreAction()
	{
		m_bIsCurrentlyActive = true;
		m_Objective.PreAction();
		Analytics_ObjectiveStarted();
	}

	public void PostAction()
	{
		m_bIsCurrentlyActive = false;
		m_Objective.PostAction();
	}

	public void SetToIncomplete()
	{
		m_Objective.SetToIncomplete();
	}

	public void Reset()
	{
		ResetAnalytics();
		m_Objective.Reset();
	}

	public void ResetAnalytics()
	{
		m_bAnalyticsStartedCalled = false;
		m_bAnalyticsEndedCalled = false;
	}

	public void SetPlayerOwner(Player owner)
	{
		m_Objective.SetPlayerOwner(owner);
	}

	public void SetQuestGiver(Character questGiver)
	{
		m_Objective.SetQuestGiver(questGiver);
	}

	public void PickAllRandomTargets()
	{
		m_Objective.PickAllTargets();
	}

	public void AddDependencyNode(int nodeID)
	{
		if (m_DependencyNode == -1)
		{
			m_DependencyNode = nodeID;
		}
	}

	public void Analytics_ObjectiveStarted()
	{
		if (m_AnalyticOnStart && !m_bAnalyticsStartedCalled)
		{
			m_bAnalyticsStartedCalled = true;
			GoogleAnalyticsV3.LogCommericalAnalyticEvent(m_AnalyticsStartCategory, m_AnalyticsStartAction, m_AnalyticsStartLabel, 0L);
		}
	}

	public void Analytics_ObjectiveEnded()
	{
		if (m_AnalyticOnEnd && !m_bAnalyticsEndedCalled)
		{
			m_bAnalyticsEndedCalled = true;
			GoogleAnalyticsV3.LogCommericalAnalyticEvent(m_AnalyticsEndCategory, m_AnalyticsEndAction, m_AnalyticsEndLabel, 0L);
		}
	}

	public JObject SaveGoal(bool ingamesave)
	{
		JObject jObject = new JObject();
		jObject.Add(new JProperty("NodeID", m_NodeID));
		jObject.Add(new JProperty("LinksToNodes", new JArray(m_LinksToNodes.Select((int c) => new JValue(c)))));
		jObject.Add(new JProperty("GoalName", m_ObjectiveGoalLocaTag));
		jObject.Add(new JProperty("ResetOnFail", ResetToPreviousDependency));
		JProperty jProperty = new JProperty("Objectives");
		JArray jArray = new JArray();
		if (m_Objective != null)
		{
			jArray.Add(new JValue(m_Objective.Save(ingamesave)));
		}
		jProperty.Add(jArray);
		jObject.Add(jProperty);
		jObject.Add(new JProperty("AnalyticsOnStart", m_AnalyticOnStart));
		jObject.Add(new JProperty("AnalyticsOnEnd", m_AnalyticOnEnd));
		jObject.Add(new JProperty("AnalyticsStartCategory", m_AnalyticsStartCategory));
		jObject.Add(new JProperty("AnalyticsStartAction", m_AnalyticsStartAction));
		jObject.Add(new JProperty("AnalyticsStartLabel", m_AnalyticsStartLabel));
		jObject.Add(new JProperty("AnalyticsEndCategory", m_AnalyticsEndCategory));
		jObject.Add(new JProperty("AnalyticsEndAction", m_AnalyticsEndAction));
		jObject.Add(new JProperty("AnalyticsEndLabel", m_AnalyticsEndLabel));
		jObject.Add(new JProperty("AnalyticsStartedCalled", m_bAnalyticsStartedCalled));
		jObject.Add(new JProperty("AnalyticsEndedCalled", m_bAnalyticsEndedCalled));
		return jObject;
	}

	public bool LoadGoal(JObject goalObj, bool ingame, bool bUpdateNetworkService = false)
	{
		if (goalObj == null)
		{
			return false;
		}
		if (goalObj.Property("NodeID") != null)
		{
			m_NodeID = (int)goalObj.Property("NodeID").Value;
		}
		if (goalObj.Property("LinksToNodes") != null)
		{
			if (goalObj.Property("LinksToNodes").Value.Type != JTokenType.Array)
			{
				m_LinksToNodes.Add((int)goalObj.Property("LinksToNodes").Value);
			}
			else
			{
				JArray source = (JArray)goalObj.Property("LinksToNodes").Value;
				m_LinksToNodes = source.Select((JToken c) => (int)c).ToList();
			}
		}
		m_ObjectiveGoalLocaTag = (string)goalObj.Property("GoalName").Value;
		if (goalObj.Property("ResetOnFail") != null)
		{
			ResetToPreviousDependency = (bool)goalObj.Property("ResetOnFail").Value;
		}
		JProperty jProperty = goalObj.Property("Objectives");
		if (jProperty == null || jProperty.Value.Type != JTokenType.Array)
		{
			return false;
		}
		m_Objective = null;
		JArray source2 = (JArray)jProperty.Value;
		List<string> list = source2.Select((JToken c) => (string)c).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			if (bUpdateNetworkService)
			{
				GlobalStart.TimedNetworkService();
			}
			int num = list[i].IndexOf("_");
			string typeName = list[i].Substring(0, num);
			string json = list[i].Substring(num + 1);
			Type type = Type.GetType(typeName);
			if (type == typeof(ItemObjective))
			{
				ItemObjective itemObjective = new ItemObjective();
				itemObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = itemObjective;
			}
			if (type == typeof(CraftObjective))
			{
				CraftObjective craftObjective = new CraftObjective();
				craftObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = craftObjective;
			}
			if (type == typeof(DialogObjective))
			{
				DialogObjective dialogObjective = new DialogObjective();
				dialogObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = dialogObjective;
			}
			if (type == typeof(InventoryObjective))
			{
				InventoryObjective inventoryObjective = new InventoryObjective();
				inventoryObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = inventoryObjective;
			}
			if (type == typeof(TriggerObjective))
			{
				TriggerObjective triggerObjective = new TriggerObjective();
				triggerObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = triggerObjective;
			}
			if (type == typeof(InteractObjective))
			{
				InteractObjective interactObjective = new InteractObjective();
				interactObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = interactObjective;
			}
			if (type == typeof(SpeechObjective))
			{
				SpeechObjective speechObjective = new SpeechObjective();
				speechObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = speechObjective;
			}
			if (type == typeof(PassiveDialogObjective))
			{
				PassiveDialogObjective passiveDialogObjective = new PassiveDialogObjective();
				passiveDialogObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = passiveDialogObjective;
			}
			if (type == typeof(CombatObjective))
			{
				CombatObjective combatObjective = new CombatObjective();
				combatObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = combatObjective;
			}
			if (type == typeof(OutfitObjective))
			{
				OutfitObjective outfitObjective = new OutfitObjective();
				outfitObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = outfitObjective;
			}
			if (type == typeof(DestroyItemObjective))
			{
				DestroyItemObjective destroyItemObjective = new DestroyItemObjective();
				destroyItemObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = destroyItemObjective;
			}
			if (type == typeof(QuestIntroObjective))
			{
				QuestIntroObjective questIntroObjective = new QuestIntroObjective();
				questIntroObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = questIntroObjective;
			}
			if (type == typeof(FloodToiletObjective))
			{
				FloodToiletObjective floodToiletObjective = new FloodToiletObjective();
				floodToiletObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = floodToiletObjective;
			}
			if (type == typeof(JobDisruptionObjective))
			{
				JobDisruptionObjective jobDisruptionObjective = new JobDisruptionObjective();
				jobDisruptionObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = jobDisruptionObjective;
			}
			if (type == typeof(TutorialCompleteObjective))
			{
				TutorialCompleteObjective tutorialCompleteObjective = new TutorialCompleteObjective();
				tutorialCompleteObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = tutorialCompleteObjective;
			}
			if (type == typeof(SetObjectiveArrowObjective))
			{
				SetObjectiveArrowObjective setObjectiveArrowObjective = new SetObjectiveArrowObjective();
				setObjectiveArrowObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = setObjectiveArrowObjective;
			}
			if (type == typeof(SpawnVendorObjective))
			{
				SpawnVendorObjective spawnVendorObjective = new SpawnVendorObjective();
				spawnVendorObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = spawnVendorObjective;
			}
			if (type == typeof(UseItemObjective))
			{
				UseItemObjective useItemObjective = new UseItemObjective();
				useItemObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = useItemObjective;
			}
			if (type == typeof(WaitUntilTimeObjective))
			{
				WaitUntilTimeObjective waitUntilTimeObjective = new WaitUntilTimeObjective();
				waitUntilTimeObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = waitUntilTimeObjective;
			}
			if (type == typeof(PlayEscapeCutsceneObjective))
			{
				PlayEscapeCutsceneObjective playEscapeCutsceneObjective = new PlayEscapeCutsceneObjective();
				playEscapeCutsceneObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = playEscapeCutsceneObjective;
			}
			if (type == typeof(EnableMultistageInteractionObjective))
			{
				EnableMultistageInteractionObjective enableMultistageInteractionObjective = new EnableMultistageInteractionObjective();
				enableMultistageInteractionObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = enableMultistageInteractionObjective;
			}
			if (type == typeof(SetRoutineObjective))
			{
				SetRoutineObjective setRoutineObjective = new SetRoutineObjective();
				setRoutineObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = setRoutineObjective;
			}
			if (type == typeof(EnableInputObjective))
			{
				EnableInputObjective enableInputObjective = new EnableInputObjective();
				enableInputObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = enableInputObjective;
			}
			if (type == typeof(DamageTileObjective))
			{
				DamageTileObjective damageTileObjective = new DamageTileObjective();
				damageTileObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = damageTileObjective;
			}
			if (type == typeof(SwapBehaviourObjective))
			{
				SwapBehaviourObjective swapBehaviourObjective = new SwapBehaviourObjective();
				swapBehaviourObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = swapBehaviourObjective;
			}
			if (type == typeof(MoveDeskObjective))
			{
				MoveDeskObjective moveDeskObjective = new MoveDeskObjective();
				moveDeskObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = moveDeskObjective;
			}
			if (type == typeof(TutorialSpeechObjective))
			{
				TutorialSpeechObjective tutorialSpeechObjective = new TutorialSpeechObjective();
				tutorialSpeechObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = tutorialSpeechObjective;
			}
			if (type == typeof(TutorialGuidedUIObjective))
			{
				TutorialGuidedUIObjective tutorialGuidedUIObjective = new TutorialGuidedUIObjective();
				tutorialGuidedUIObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = tutorialGuidedUIObjective;
			}
			if (type == typeof(EnableInteractionObjective))
			{
				EnableInteractionObjective enableInteractionObjective = new EnableInteractionObjective();
				enableInteractionObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = enableInteractionObjective;
			}
			if (type == typeof(EnableInGameMenuObjective))
			{
				EnableInGameMenuObjective enableInGameMenuObjective = new EnableInGameMenuObjective();
				enableInGameMenuObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = enableInGameMenuObjective;
			}
			if (type == typeof(SetUsableItemObjective))
			{
				SetUsableItemObjective setUsableItemObjective = new SetUsableItemObjective();
				setUsableItemObjective.Load(JObject.Parse(json), ingame, bUpdateNetworkService);
				m_Objective = setUsableItemObjective;
			}
		}
		if (goalObj.Property("AnalyticsOnStart") != null)
		{
			m_AnalyticOnStart = (bool)goalObj.Property("AnalyticsOnStart").Value;
		}
		if (goalObj.Property("AnalyticsOnEnd") != null)
		{
			m_AnalyticOnEnd = (bool)goalObj.Property("AnalyticsOnEnd").Value;
		}
		if (goalObj.Property("AnalyticsStartCategory") != null)
		{
			m_AnalyticsStartCategory = (string)goalObj.Property("AnalyticsStartCategory").Value;
		}
		if (goalObj.Property("AnalyticsStartAction") != null)
		{
			m_AnalyticsStartAction = (string)goalObj.Property("AnalyticsStartAction").Value;
		}
		if (goalObj.Property("AnalyticsStartLabel") != null)
		{
			m_AnalyticsStartLabel = (string)goalObj.Property("AnalyticsStartLabel").Value;
		}
		if (goalObj.Property("AnalyticsEndCategory") != null)
		{
			m_AnalyticsEndCategory = (string)goalObj.Property("AnalyticsEndCategory").Value;
		}
		if (goalObj.Property("AnalyticsEndAction") != null)
		{
			m_AnalyticsEndAction = (string)goalObj.Property("AnalyticsEndAction").Value;
		}
		if (goalObj.Property("AnalyticsEndLabel") != null)
		{
			m_AnalyticsEndLabel = (string)goalObj.Property("AnalyticsEndLabel").Value;
		}
		if (goalObj.Property("AnalyticsStartedCalled") != null)
		{
			m_bAnalyticsStartedCalled = (bool)goalObj.Property("AnalyticsStartedCalled").Value;
		}
		if (goalObj.Property("AnalyticsEndedCalled") != null)
		{
			m_bAnalyticsEndedCalled = (bool)goalObj.Property("AnalyticsEndedCalled").Value;
		}
		return true;
	}
}
