using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class ObjectiveTree
{
	public delegate void OnObjectiveTreeEvent(ObjectiveTree tree);

	public struct StartNodeData
	{
		public int NodeID;

		public List<int> LinksToNodes;

		public Vector2 CanvasPosition;

		public bool WasLoaded;
	}

	public struct EndNodeData
	{
		public int NodeID;

		public Vector2 CanvasPosition;

		public bool WasLoaded;
	}

	public string m_MultiPartText = string.Empty;

	public OnObjectiveTreeEvent OnObjectiveTreeCompleted;

	public OnObjectiveTreeEvent OnObjectiveTreeFailed;

	public OnObjectiveTreeEvent OnObjectiveTreeCanceled;

	public Dictionary<string, Localization.TokenInfo> m_AvailableTokens;

	public bool m_bShowInJournal = true;

	public bool m_bIsTrackable = true;

	public bool m_bShowTrackingArrows = true;

	public bool m_bShowTrackingPins = true;

	private TreeBranch m_MainTreeBranch;

	private string m_SceneToUseIn = string.Empty;

	private ObjectiveStatus m_TreeStatus;

	public static int HIGHEST_ACTIVE_TREE_ID;

	private int m_CurrentTreeID = -999;

	private bool m_IsBeingTracked;

	public List<ObjectiveGoal> m_EditorObjectiveGoals;

	public StartNodeData m_StartNodeData;

	public EndNodeData m_EndNodeData;

	public static bool DEBUG_DONT_LOAD_TOKENS;

	public string SceneToUseIn => m_SceneToUseIn;

	public ObjectiveStatus GetObjectiveStatus => m_TreeStatus;

	public TreeBranch MainBranch => m_MainTreeBranch;

	public int ActiveTreeID => m_CurrentTreeID;

	public bool isBeingTracked
	{
		get
		{
			return m_IsBeingTracked;
		}
		set
		{
			m_IsBeingTracked = value;
		}
	}

	public ObjectiveTree()
	{
		m_AvailableTokens = new Dictionary<string, Localization.TokenInfo>();
		m_EditorObjectiveGoals = new List<ObjectiveGoal>();
		m_StartNodeData = default(StartNodeData);
		m_StartNodeData.NodeID = -1;
		m_StartNodeData.WasLoaded = false;
		m_EndNodeData = default(EndNodeData);
		m_EndNodeData.WasLoaded = false;
		m_EndNodeData.NodeID = -1;
		m_StartNodeData.LinksToNodes = new List<int>();
		m_StartNodeData.CanvasPosition = -Vector2.one;
		m_EndNodeData.CanvasPosition = -Vector2.one;
		m_CurrentTreeID = HIGHEST_ACTIVE_TREE_ID;
		HIGHEST_ACTIVE_TREE_ID++;
		if (Application.isPlaying)
		{
			m_MainTreeBranch = new TreeBranch(0, 0, this);
		}
	}

	public void SetSceneToUseIn(string scene)
	{
		m_SceneToUseIn = scene;
	}

	public void Initialize()
	{
		m_MainTreeBranch.Initialize();
		m_MainTreeBranch.PreActionBranch();
	}

	public bool EvaluateCurrentGoal()
	{
		if (m_MainTreeBranch != null)
		{
			m_TreeStatus = m_MainTreeBranch.EvaluateBranch();
			switch (m_TreeStatus)
			{
			case ObjectiveStatus.Done:
				if (OnObjectiveTreeCompleted != null)
				{
					m_MainTreeBranch.ResetObjectiveAnalytics();
					OnObjectiveTreeCompleted(this);
				}
				return true;
			case ObjectiveStatus.Failed:
				if (OnObjectiveTreeFailed != null)
				{
					OnObjectiveTreeFailed(this);
				}
				return true;
			case ObjectiveStatus.Canceled:
				if (OnObjectiveTreeCanceled != null)
				{
					OnObjectiveTreeCanceled(this);
				}
				return true;
			case ObjectiveStatus.Invalid:
				return true;
			}
		}
		return false;
	}

	public void BuildOrderList()
	{
		List<int> linksToNodes = m_StartNodeData.LinksToNodes;
		m_MainTreeBranch.BuildOrderList(linksToNodes, null);
		m_MainTreeBranch.GetQuestDescription();
	}

	public void EndTreeEarly(bool isTreeFailed)
	{
		m_MainTreeBranch.EndTreeEarly(isTreeFailed);
		m_MainTreeBranch.PostActionAndResetTree();
	}

	public void AddObjectiveStringToken(string token, Localization.TokenReplaceType replaceType, ref string outToken)
	{
		if (!string.IsNullOrEmpty(outToken) && m_AvailableTokens.ContainsKey(outToken))
		{
			outToken = string.Empty;
			return;
		}
		if (string.IsNullOrEmpty(outToken) && (replaceType == Localization.TokenReplaceType.Player || replaceType == Localization.TokenReplaceType.QuestGiver))
		{
			foreach (KeyValuePair<string, Localization.TokenInfo> availableToken in m_AvailableTokens)
			{
				if (availableToken.Value.m_ReplaceType == replaceType)
				{
					outToken = string.Empty;
					return;
				}
			}
		}
		outToken = token;
		int num = 1;
		while (m_AvailableTokens.ContainsKey(outToken))
		{
			outToken = token + num;
			num++;
		}
		Localization.TokenInfo tokenInfo = new Localization.TokenInfo();
		tokenInfo.m_Token = outToken;
		tokenInfo.m_ReplaceType = replaceType;
		m_AvailableTokens.Add(outToken, tokenInfo);
	}

	public bool GetObjectiveToken(string tokenID, out Localization.TokenInfo token)
	{
		return m_AvailableTokens.TryGetValue(tokenID, out token);
	}

	public void UpdateObjectiveToken(string tokenID, Localization.TokenInfo updatedToken)
	{
		if (m_AvailableTokens.ContainsKey(tokenID))
		{
			m_AvailableTokens[tokenID] = updatedToken;
		}
	}

	public string GetTokenizedLocalization(BaseObjective baseObj, string localizationTag, bool alreadyLocalized = false)
	{
		string localized = string.Empty;
		if (alreadyLocalized)
		{
			localized = localizationTag;
		}
		else if (!Localization.Get(localizationTag, out localized))
		{
			return "[TBT]" + localizationTag;
		}
		if (!string.IsNullOrEmpty(localized))
		{
			string[] array = Localization.SplitStringWithPunctuation(localized);
			if (array != null && array.Length > 0)
			{
				int num = array.Length;
				for (int i = 0; i < num; i++)
				{
					string stringToClear = array[i];
					stringToClear = Localization.RemovePunctuationFromToken(stringToClear);
					if (stringToClear.StartsWith("$"))
					{
						int num2 = stringToClear.IndexOfAny(new char[6] { ',', '.', '\'', '?', '!', '-' });
						if (num2 > 0)
						{
							stringToClear = stringToClear.Substring(0, num2);
						}
						string value = stringToClear;
						if (baseObj.m_AvailableTokens == null || !baseObj.m_AvailableTokens.TryGetValue(stringToClear, out value))
						{
							value = stringToClear;
						}
						if (m_AvailableTokens.TryGetValue(value, out var value2))
						{
							string text = value.Replace("$", "@");
							text += "\\b";
							localized = localized.Replace("$", "@");
							localized = Regex.Replace(localized, text, value2.m_TextID);
						}
					}
				}
			}
		}
		return localized;
	}

	public bool LoadInGameObjectiveTree(string treeObjString)
	{
		JObject jObject = JObject.Parse(treeObjString);
		if (jObject != null)
		{
			JProperty jProperty = jObject.Property("Tokens");
			if (jProperty != null && jProperty.Value.Type == JTokenType.Array)
			{
				JArray jArray = (JArray)jProperty.Value;
				if (m_AvailableTokens != null)
				{
					m_AvailableTokens.Clear();
				}
				else
				{
					m_AvailableTokens = new Dictionary<string, Localization.TokenInfo>();
				}
				for (int i = 0; i < jArray.Count; i++)
				{
					if (jArray[i] == null || jArray[i].Type != JTokenType.Object)
					{
						continue;
					}
					JObject jObject2 = (JObject)jArray[i];
					JProperty jProperty2 = jObject2.Properties().ElementAt(0);
					if (jProperty2 == null)
					{
						continue;
					}
					JObject jObject3 = (JObject)jProperty2.Value;
					if (jObject3 == null)
					{
						continue;
					}
					Localization.TokenInfo tokenInfo = new Localization.TokenInfo();
					JProperty jProperty3 = jObject3.Property("RT");
					if (jProperty3 != null)
					{
						tokenInfo.m_ReplaceType = (Localization.TokenReplaceType)(int)jProperty3.Value;
					}
					JProperty jProperty4 = jObject3.Property("TID");
					if (jProperty4 != null)
					{
						tokenInfo.m_TextID = (string)jProperty4.Value;
						if (Localization.Get(tokenInfo.m_TextID, out var localized))
						{
							tokenInfo.m_TextIDTag = tokenInfo.m_TextID;
							tokenInfo.m_TextID = localized;
						}
					}
					JProperty jProperty5 = jObject3.Property("TOK");
					if (jProperty5 != null)
					{
						tokenInfo.m_Token = (string)jProperty5.Value;
					}
					m_AvailableTokens.Add(jProperty2.Name, tokenInfo);
				}
			}
			JProperty jProperty6 = jObject.Property("STU");
			if (jProperty6 != null)
			{
				m_SceneToUseIn = (string)jProperty6.Value;
			}
			JProperty jProperty7 = jObject.Property("TS");
			if (jProperty7 != null)
			{
				m_TreeStatus = (ObjectiveStatus)(int)jProperty7.Value;
			}
			JProperty jProperty8 = jObject.Property("MPT");
			if (jProperty8 != null)
			{
				m_MultiPartText = (string)jProperty8.Value;
			}
			JProperty jProperty9 = jObject.Property("CTID");
			if (jProperty9 != null)
			{
				m_CurrentTreeID = (int)jProperty9.Value;
				if (m_CurrentTreeID >= HIGHEST_ACTIVE_TREE_ID)
				{
					HIGHEST_ACTIVE_TREE_ID++;
				}
			}
			JProperty jProperty10 = jObject.Property("MTB");
			if (jProperty10 != null && jProperty10.Value.Type == JTokenType.Object)
			{
				m_MainTreeBranch.LoadBranch((JObject)jProperty10.Value);
				m_MainTreeBranch.PreActionBranch();
				m_MainTreeBranch.RefreshQuestDescription();
			}
			JProperty jProperty11 = jObject.Property("TR");
			m_IsBeingTracked = jProperty11 != null && (bool)jProperty11.Value;
			JProperty jProperty12 = jObject.Property("STA");
			m_bShowTrackingArrows = jProperty12 != null && (bool)jProperty12.Value;
		}
		return false;
	}

	public string SaveInGameObjectiveTrees()
	{
		JObject jObject = new JObject();
		jObject.Add(new JProperty("STU", m_SceneToUseIn));
		jObject.Add(new JProperty("TS", (int)m_TreeStatus));
		jObject.Add(new JProperty("MPT", m_MultiPartText));
		jObject.Add(new JProperty("CTID", m_CurrentTreeID));
		jObject.Add(new JProperty("MTB", m_MainTreeBranch.SaveBranch()));
		jObject.Add(new JProperty("TR", m_IsBeingTracked));
		jObject.Add(new JProperty("STA", m_bShowTrackingArrows));
		if (m_AvailableTokens != null)
		{
			JProperty jProperty = new JProperty("Tokens");
			JArray jArray = new JArray();
			foreach (KeyValuePair<string, Localization.TokenInfo> availableToken in m_AvailableTokens)
			{
				JObject jObject2 = new JObject();
				jObject2.Add(new JProperty("RT", (int)availableToken.Value.m_ReplaceType));
				if (string.IsNullOrEmpty(availableToken.Value.m_TextIDTag))
				{
					jObject2.Add(new JProperty("TID", availableToken.Value.m_TextID));
				}
				else
				{
					jObject2.Add(new JProperty("TID", availableToken.Value.m_TextIDTag));
				}
				jObject2.Add(new JProperty("TOK", availableToken.Value.m_Token));
				JObject jObject3 = new JObject();
				jObject3.Add(new JProperty(availableToken.Key, jObject2));
				jArray.Add(jObject3);
			}
			jProperty.Add(jArray);
			jObject.Add(jProperty);
		}
		return jObject.ToString();
	}

	public bool LoadEditorObjectiveTree(JObject treeObj, bool bUpdateNetworkService = false)
	{
		if (treeObj == null)
		{
			return false;
		}
		JProperty jProperty = treeObj.Property("SceneToUseIn");
		if (jProperty != null)
		{
			m_SceneToUseIn = (string)jProperty.Value;
		}
		JProperty jProperty2 = treeObj.Property("StartNodeData");
		if (jProperty2 != null)
		{
			JObject jObject = (JObject)jProperty2.Value;
			if (jObject.Property("NodeID") != null)
			{
				m_StartNodeData.NodeID = (int)jObject.Property("NodeID").Value;
			}
			if (jObject.Property("LinksToNodes") != null)
			{
				JArray source = (JArray)jObject.Property("LinksToNodes").Value;
				m_StartNodeData.LinksToNodes = source.Select((JToken c) => (int)c).ToList();
			}
			if (jObject.Property("CanvasPosition") != null)
			{
				JArray source2 = (JArray)jObject.Property("CanvasPosition").Value;
				List<float> list = source2.Select((JToken c) => (float)c).ToList();
				m_StartNodeData.CanvasPosition.x = list[0];
				m_StartNodeData.CanvasPosition.y = list[1];
			}
			m_StartNodeData.WasLoaded = true;
		}
		JProperty jProperty3 = treeObj.Property("EndNodeData");
		if (jProperty3 != null)
		{
			JObject jObject2 = (JObject)jProperty3.Value;
			if (jObject2.Property("NodeID") != null)
			{
				m_EndNodeData.NodeID = (int)jObject2.Property("NodeID").Value;
			}
			if (jObject2.Property("CanvasPosition") != null)
			{
				JArray source3 = (JArray)jObject2.Property("CanvasPosition").Value;
				List<float> list2 = source3.Select((JToken c) => (float)c).ToList();
				m_EndNodeData.CanvasPosition.x = list2[0];
				m_EndNodeData.CanvasPosition.y = list2[1];
			}
			m_EndNodeData.WasLoaded = true;
		}
		if (!DEBUG_DONT_LOAD_TOKENS)
		{
			JProperty jProperty4 = treeObj.Property("Tokens");
			if (jProperty4 != null && jProperty4.Value.Type == JTokenType.Array)
			{
				JArray jArray = (JArray)jProperty4.Value;
				m_AvailableTokens.Clear();
				for (int i = 0; i < jArray.Count; i++)
				{
					if (bUpdateNetworkService)
					{
						GlobalStart.TimedNetworkService();
					}
					if (jArray[i] == null || jArray[i].Type != JTokenType.Object)
					{
						continue;
					}
					JObject jObject3 = (JObject)jArray[i];
					Localization.TokenInfo tokenInfo = new Localization.TokenInfo();
					tokenInfo.m_Token = (string)jObject3.Property("Token").Value;
					tokenInfo.m_ReplaceType = (Localization.TokenReplaceType)(int)jObject3.Property("Type").Value;
					tokenInfo.m_TextID = (string)jObject3.Property("TextID").Value;
					tokenInfo.m_SceneReference = null;
					JProperty jProperty5 = jObject3.Property("SceneRef_ID");
					if (jProperty5 != null)
					{
						string text = (string)jObject3.Property("SceneRef_Scene").Value;
						int id = (int)jProperty5.Value;
						if ((Application.isPlaying || !(text != SceneManagerHelper.ActiveSceneName)) && Application.isPlaying)
						{
							tokenInfo.m_SceneReference = ObjectiveSceneElement.FindSceneReference(id);
						}
					}
					m_AvailableTokens.Add(tokenInfo.m_Token, tokenInfo);
				}
			}
		}
		JProperty jProperty6 = treeObj.Property("ObjectiveGoals");
		if (jProperty6 == null || jProperty6.Value.Type != JTokenType.Array)
		{
			return false;
		}
		if (m_EditorObjectiveGoals != null)
		{
			m_EditorObjectiveGoals.Clear();
		}
		JArray source4 = (JArray)jProperty6.Value;
		List<JObject> list3 = source4.Select((JToken c) => (JObject)c).ToList();
		for (int j = 0; j < list3.Count; j++)
		{
			ObjectiveGoal objectiveGoal = new ObjectiveGoal();
			if (objectiveGoal.LoadGoal(list3[j], ingame: false, bUpdateNetworkService))
			{
				if (bUpdateNetworkService)
				{
					GlobalStart.TimedNetworkService();
				}
				objectiveGoal.RegisterTokens(this);
				m_EditorObjectiveGoals.Add(objectiveGoal);
			}
		}
		return true;
	}
}
