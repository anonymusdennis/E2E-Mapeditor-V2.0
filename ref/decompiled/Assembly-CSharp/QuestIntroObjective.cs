using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class QuestIntroObjective : BaseObjective
{
	public enum RewardType
	{
		Money = 1,
		Item = 2,
		Escape = 4
	}

	public enum RewardReceive
	{
		UponCompletion,
		TalkToQuestGiver
	}

	public string m_QTitleLocalizationTag = "Text.Quest.Title";

	public string m_QDescLocalizationTag = "Text.Quest.Description";

	public int m_ReputationGainOnSuccess = 10;

	public int m_ReputationLossOnCancel = 10;

	public int m_ReputationLossOnFailed = 5;

	public int m_CompletionTimoutInHours = 72;

	public bool m_bShouldShowQuestArrow;

	public bool m_bUsePerObjectiveTitles;

	public RewardType Reward = RewardType.Money;

	public int MoneyReward = 50;

	public ItemData ItemReward;

	public RewardReceive ReceiveType;

	public STAT_IDS CompletionStat = STAT_IDS.NoneStat;

	protected string m_QLocalizedObjectiveName = string.Empty;

	protected string m_QLocalizedDescription = string.Empty;

	public string QuestLocalizedObjectiveName
	{
		get
		{
			if (m_ParentObjectiveTree != null && string.IsNullOrEmpty(m_QLocalizedObjectiveName))
			{
				m_QLocalizedObjectiveName = m_ParentObjectiveTree.GetTokenizedLocalization(this, m_QTitleLocalizationTag);
			}
			return m_QLocalizedObjectiveName;
		}
	}

	public string QuestLocalizedDescription
	{
		get
		{
			if (m_ParentObjectiveTree != null && string.IsNullOrEmpty(m_QLocalizedDescription))
			{
				m_QLocalizedDescription = m_ParentObjectiveTree.GetTokenizedLocalization(this, m_QDescLocalizationTag);
			}
			return m_QLocalizedDescription;
		}
	}

	protected override void Child_PickAllTargets()
	{
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
		if (m_ParentObjectiveTree != null && m_ParentObjectiveTree.MainBranch != null)
		{
			m_ParentObjectiveTree.MainBranch.RefreshQuestDescription();
		}
	}

	protected override bool Child_EvaluateDependencies()
	{
		return true;
	}

	protected override bool Child_EvaluateStatus()
	{
		return true;
	}

	public override int SetHUDInfo(ref ObjectiveSubGoalHUD[] infoList)
	{
		return 0;
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
		baseObj.Add(new JProperty("QuestTitle", m_QTitleLocalizationTag));
		baseObj.Add(new JProperty("QuestDescription", m_QDescLocalizationTag));
		baseObj.Add(new JProperty("ReputationSuccess", m_ReputationGainOnSuccess));
		baseObj.Add(new JProperty("ReputationCancel", m_ReputationLossOnCancel));
		baseObj.Add(new JProperty("ReputationFailure", m_ReputationLossOnFailed));
		baseObj.Add(new JProperty("RewardType", (int)Reward));
		baseObj.Add(new JProperty("MoneyReward", MoneyReward));
		baseObj.Add(new JProperty("ItemReward", (!(ItemReward == null)) ? ItemReward.m_ItemDataID : (-1)));
		baseObj.Add(new JProperty("ReceiveTpye", (int)ReceiveType));
		baseObj.Add(new JProperty("CompletionTime", m_CompletionTimoutInHours));
		baseObj.Add(new JProperty("CompletionStat", (int)CompletionStat));
		baseObj.Add(new JProperty("ShouldShowQuestArrow", m_bShouldShowQuestArrow));
		baseObj.Add(new JProperty("UsePerObjectiveTitles", m_bUsePerObjectiveTitles));
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject json, bool ingameLoad)
	{
		JProperty jProperty = json.Property("QuestTitle");
		if (jProperty != null)
		{
			string qTitleLocalizationTag = (string)jProperty.Value;
			m_QTitleLocalizationTag = qTitleLocalizationTag;
		}
		JProperty jProperty2 = json.Property("QuestDescription");
		if (jProperty != null)
		{
			string qDescLocalizationTag = (string)jProperty2.Value;
			m_QDescLocalizationTag = qDescLocalizationTag;
		}
		if (json.Property("ReputationSuccess") != null)
		{
			m_ReputationGainOnSuccess = (int)json.Property("ReputationSuccess").Value;
		}
		if (json.Property("ReputationCancel") != null)
		{
			m_ReputationLossOnCancel = (int)json.Property("ReputationCancel").Value;
		}
		if (json.Property("ReputationFailure") != null)
		{
			m_ReputationLossOnFailed = (int)json.Property("ReputationFailure").Value;
		}
		if (json.Property("RewardType") != null)
		{
			Reward = (RewardType)(int)json.Property("RewardType").Value;
		}
		if (json.Property("MoneyReward") != null)
		{
			MoneyReward = (int)json.Property("MoneyReward").Value;
		}
		if (json.Property("ItemReward") != null)
		{
			int itemID = (int)json.Property("ItemReward").Value;
			if (itemID != -1)
			{
				ItemReward = Resources.LoadAll<ItemData>("Prefabs/Items").ToList().FirstOrDefault((ItemData id) => id.m_ItemDataID == itemID);
			}
		}
		if (json.Property("ReceiveType") != null)
		{
			ReceiveType = (RewardReceive)(int)json.Property("ReceiveType").Value;
		}
		if (json.Property("CompletionTime") != null)
		{
			m_CompletionTimoutInHours = (int)json.Property("CompletionTime").Value;
		}
		if (json.Property("CompletionStat") != null)
		{
			CompletionStat = (STAT_IDS)(int)json.Property("CompletionStat").Value;
		}
		if (json.Property("ShouldShowQuestArrow") != null)
		{
			m_bShouldShowQuestArrow = (bool)json.Property("ShouldShowQuestArrow").Value;
		}
		if (json.Property("UsePerObjectiveTitles") != null)
		{
			m_bUsePerObjectiveTitles = (bool)json.Property("UsePerObjectiveTitles").Value;
		}
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.QuestIntroObjective;
	}
}
