using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class TreeBranch
{
	public List<ObjectiveGoal> m_ObjectiveGoals;

	public Dictionary<int, List<TreeBranch>> m_SubBranches;

	private ObjectiveTree m_RootParentTree;

	private Player m_PlayerOwner;

	private Character m_QuestGiver;

	private int m_BranchID = -1;

	private int m_BranchIndex = -1;

	private int m_CurrentGoalIndex;

	private int m_ItemRewardResponseID = -1;

	private ObjectiveStatus m_BranchStatus;

	private QuestIntroObjective m_QuestIntroObjective;

	private ObjectiveGoal m_BranchEndGoal;

	public bool IsBranchFinished => m_BranchStatus == ObjectiveStatus.Done;

	public bool IsMainBranch => m_RootParentTree == null || m_RootParentTree.MainBranch == this;

	public int BranchID => m_BranchID;

	public int BranchIndex => m_BranchIndex;

	public ObjectiveGoal BranchEndGoal => m_BranchEndGoal;

	public Character QuestGiver => m_QuestGiver;

	public Player PlayerOwner => m_PlayerOwner;

	public TreeBranch(int branchID, int branchIndex, ObjectiveTree rootparent)
	{
		m_BranchID = branchID;
		m_BranchIndex = branchIndex;
		m_RootParentTree = rootparent;
	}

	public ObjectiveStatus EvaluateBranch(bool bIsBranch = false)
	{
		if ((!bIsBranch && m_BranchStatus == ObjectiveStatus.Done) || m_BranchStatus == ObjectiveStatus.Failed || m_BranchStatus == ObjectiveStatus.Canceled)
		{
			return m_BranchStatus;
		}
		if (m_CurrentGoalIndex < 0 || m_CurrentGoalIndex >= m_ObjectiveGoals.Count)
		{
			m_BranchStatus = ObjectiveStatus.Invalid;
			return m_BranchStatus;
		}
		ObjectiveGoal objectiveGoal = m_ObjectiveGoals[m_CurrentGoalIndex];
		if (objectiveGoal != null)
		{
			int verifyPreviousObjectiveIndex = objectiveGoal.m_Objective.VerifyPreviousObjectiveIndex;
			if (verifyPreviousObjectiveIndex != -1)
			{
				ObjectiveGoal objectiveGoal2 = m_ObjectiveGoals[verifyPreviousObjectiveIndex];
				if (objectiveGoal2 != null && !objectiveGoal2.EvaluateObjectiveComplete())
				{
					ResetToSpecifiedGoal(verifyPreviousObjectiveIndex);
					m_BranchStatus = ObjectiveStatus.InComplete;
					objectiveGoal = m_ObjectiveGoals[m_CurrentGoalIndex];
				}
			}
		}
		if (objectiveGoal != null)
		{
			switch (objectiveGoal.EvaluateObjectives())
			{
			case ObjectiveStatus.InActive:
				m_BranchStatus = ObjectiveStatus.InActive;
				break;
			case ObjectiveStatus.InComplete:
				m_BranchStatus = ObjectiveStatus.InComplete;
				break;
			case ObjectiveStatus.Done:
				if (m_BranchStatus != ObjectiveStatus.Done)
				{
					objectiveGoal.Analytics_ObjectiveEnded();
				}
				if (!GotoNextGoal())
				{
					if (!bIsBranch)
					{
						objectiveGoal.PostAction();
					}
					else
					{
						m_CurrentGoalIndex--;
					}
					m_BranchStatus = ObjectiveStatus.Done;
				}
				else
				{
					objectiveGoal.PostAction();
					m_BranchStatus = ObjectiveStatus.InComplete;
				}
				break;
			case ObjectiveStatus.Failed:
				objectiveGoal.PostAction();
				m_BranchStatus = ObjectiveStatus.Failed;
				ProcessFailure();
				break;
			case ObjectiveStatus.Canceled:
				objectiveGoal.PostAction();
				m_BranchStatus = ObjectiveStatus.Canceled;
				ProcessCancellation();
				break;
			case ObjectiveStatus.Invalid:
				objectiveGoal.PostAction();
				m_BranchStatus = ObjectiveStatus.Invalid;
				break;
			case ObjectiveStatus.Reset:
				ResetToSpecifiedGoal(objectiveGoal.m_Objective.ResetToIndex);
				m_BranchStatus = ObjectiveStatus.InComplete;
				break;
			default:
				m_BranchStatus = ObjectiveStatus.InComplete;
				break;
			}
		}
		else if (!m_SubBranches.ContainsKey(m_CurrentGoalIndex))
		{
			m_BranchStatus = ObjectiveStatus.Invalid;
		}
		else
		{
			List<TreeBranch> list = m_SubBranches[m_CurrentGoalIndex];
			if (list != null)
			{
				bool flag = true;
				for (int i = 0; i < list.Count; i++)
				{
					ObjectiveStatus objectiveStatus = list[i].EvaluateBranch(list.Count > 1);
					if (objectiveStatus != ObjectiveStatus.Done)
					{
						flag = false;
					}
					if (objectiveStatus == ObjectiveStatus.Canceled || objectiveStatus == ObjectiveStatus.Failed || objectiveStatus == ObjectiveStatus.Invalid)
					{
						flag = false;
						m_BranchStatus = objectiveStatus;
						break;
					}
				}
				if (flag)
				{
					if (list.Count > 1)
					{
						for (int j = 0; j < list.Count; j++)
						{
							list[j].PostActionLastGoal();
						}
					}
					if (!GotoNextGoal())
					{
						m_BranchStatus = ObjectiveStatus.Done;
					}
					else
					{
						m_BranchStatus = ObjectiveStatus.InComplete;
					}
				}
				else if (m_BranchStatus != ObjectiveStatus.Canceled && m_BranchStatus != ObjectiveStatus.Failed && m_BranchStatus != ObjectiveStatus.Invalid && list.Count > 1)
				{
					for (int k = 0; k < list.Count; k++)
					{
						list[k].SetLastGoalToIncomplete();
					}
				}
			}
		}
		if (m_BranchStatus == ObjectiveStatus.Done && IsMainBranch)
		{
			ProcessCompletion();
		}
		return m_BranchStatus;
	}

	public ObjectiveStatus GetBranchStatus()
	{
		return m_BranchStatus;
	}

	public int FindSortIndexOfGoalByNodeID(int nodeID)
	{
		int num = FindExtention.FindIndex(m_ObjectiveGoals, (ObjectiveGoal s) => s.NodeID == nodeID);
		if (num == -1)
		{
			foreach (KeyValuePair<int, List<TreeBranch>> subBranch in m_SubBranches)
			{
				for (int i = 0; i < subBranch.Value.Count; i++)
				{
					int num2 = subBranch.Value[i].FindSortIndexOfGoalByNodeID(nodeID);
					if (num2 != -1)
					{
						return subBranch.Key;
					}
				}
			}
		}
		return num;
	}

	public ObjectiveGoal FindGoalByNodeID(int nodeID)
	{
		ObjectiveGoal objectiveGoal = m_ObjectiveGoals.FirstOrDefault((ObjectiveGoal g) => g.NodeID == nodeID);
		if (objectiveGoal == null)
		{
			foreach (KeyValuePair<int, List<TreeBranch>> subBranch in m_SubBranches)
			{
				for (int i = 0; i < subBranch.Value.Count; i++)
				{
					objectiveGoal = subBranch.Value[i].FindGoalByNodeID(nodeID);
					if (objectiveGoal != null)
					{
						return objectiveGoal;
					}
				}
			}
		}
		return objectiveGoal;
	}

	private bool GotoNextGoal()
	{
		m_CurrentGoalIndex++;
		if (m_CurrentGoalIndex >= m_ObjectiveGoals.Count)
		{
			return false;
		}
		PreActionCurrent();
		return true;
	}

	private bool ResetToSpecifiedGoal(int resetToIndex)
	{
		if (resetToIndex >= m_ObjectiveGoals.Count || resetToIndex >= m_CurrentGoalIndex)
		{
			return false;
		}
		for (int num = m_CurrentGoalIndex; num >= resetToIndex; num--)
		{
			if (m_ObjectiveGoals[num] != null)
			{
				m_ObjectiveGoals[num].Reset();
				m_ObjectiveGoals[num].Initialize();
			}
			else
			{
				List<TreeBranch> list = m_SubBranches[num];
				if (list != null)
				{
					int i = 0;
					for (int count = list.Count; i < count; i++)
					{
						list[i].ResetToSpecifiedGoal(0);
					}
				}
			}
		}
		m_CurrentGoalIndex = resetToIndex;
		PreActionCurrent();
		return true;
	}

	public void SetBaseInfo(Player owner, Character questGiver)
	{
		m_PlayerOwner = owner;
		m_QuestGiver = questGiver;
		for (int i = 0; i < m_ObjectiveGoals.Count; i++)
		{
			if (m_ObjectiveGoals[i] != null)
			{
				m_ObjectiveGoals[i].SetPlayerOwner(owner);
				m_ObjectiveGoals[i].SetQuestGiver(questGiver);
			}
		}
		if (m_SubBranches == null)
		{
			return;
		}
		foreach (KeyValuePair<int, List<TreeBranch>> subBranch in m_SubBranches)
		{
			for (int j = 0; j < subBranch.Value.Count; j++)
			{
				if (subBranch.Value[j] != null)
				{
					subBranch.Value[j].SetBaseInfo(owner, questGiver);
				}
			}
		}
	}

	public void PickAllRandomTargets()
	{
		for (int i = 0; i < m_ObjectiveGoals.Count; i++)
		{
			if (m_ObjectiveGoals[i] != null)
			{
				m_ObjectiveGoals[i].PickAllRandomTargets();
			}
		}
		if (m_SubBranches == null)
		{
			return;
		}
		foreach (KeyValuePair<int, List<TreeBranch>> subBranch in m_SubBranches)
		{
			for (int j = 0; j < subBranch.Value.Count; j++)
			{
				if (subBranch.Value[j] != null)
				{
					subBranch.Value[j].PickAllRandomTargets();
				}
			}
		}
	}

	public void Initialize()
	{
		for (int i = 0; i < m_ObjectiveGoals.Count; i++)
		{
			if (m_ObjectiveGoals[i] != null)
			{
				m_ObjectiveGoals[i].Initialize();
			}
		}
		if (m_SubBranches == null)
		{
			return;
		}
		foreach (KeyValuePair<int, List<TreeBranch>> subBranch in m_SubBranches)
		{
			for (int j = 0; j < subBranch.Value.Count; j++)
			{
				if (subBranch.Value[j] != null)
				{
					subBranch.Value[j].Initialize();
				}
			}
		}
	}

	public QuestIntroObjective GetQuestDescription()
	{
		if (m_QuestIntroObjective != null)
		{
			return m_QuestIntroObjective;
		}
		for (int i = 0; i < m_ObjectiveGoals.Count; i++)
		{
			if (m_ObjectiveGoals[i] != null && m_ObjectiveGoals[i].m_Objective.GetObjectiveType() == BaseObjective.ObjectiveType.QuestIntroObjective)
			{
				m_QuestIntroObjective = (QuestIntroObjective)m_ObjectiveGoals[i].m_Objective;
				return m_QuestIntroObjective;
			}
		}
		return null;
	}

	public string GetQuestLabel(string strDefault)
	{
		QuestIntroObjective questDescription = GetQuestDescription();
		if (questDescription != null)
		{
			if (questDescription.m_bUsePerObjectiveTitles && m_CurrentGoalIndex != -1)
			{
				ObjectiveGoal objectiveGoal = null;
				if ((objectiveGoal = GetCurrentGoal()) != null || (objectiveGoal = GetLastObjective()) != null)
				{
					return objectiveGoal.m_Objective.LocalizedObjectiveName;
				}
			}
			return questDescription.QuestLocalizedObjectiveName;
		}
		return strDefault;
	}

	public string GetQuestDescriptionLabel()
	{
		string empty = string.Empty;
		QuestIntroObjective questDescription = GetQuestDescription();
		if (questDescription != null)
		{
			empty = questDescription.QuestLocalizedDescription;
			if (!questDescription.m_bUsePerObjectiveTitles || m_CurrentGoalIndex == -1)
			{
				return empty;
			}
			if (m_CurrentGoalIndex < m_ObjectiveGoals.Count)
			{
				for (int num = m_CurrentGoalIndex - 1; num >= 0; num--)
				{
					if (m_ObjectiveGoals[num] != null)
					{
						if (m_ObjectiveGoals[num].m_Objective.GetObjectiveType() == BaseObjective.ObjectiveType.QuestIntroObjective)
						{
							if (m_ObjectiveGoals[num].m_Objective is QuestIntroObjective questIntroObjective && !string.IsNullOrEmpty(questIntroObjective.QuestLocalizedDescription))
							{
								return questIntroObjective.QuestLocalizedDescription;
							}
						}
						else if (m_ObjectiveGoals[num].m_Objective.GetObjectiveType() == BaseObjective.ObjectiveType.InteractObjective && m_ObjectiveGoals[num].m_Objective is InteractObjective interactObjective && !string.IsNullOrEmpty(interactObjective.m_QuestGiverMessage))
						{
							Localization.Get(interactObjective.m_QuestGiverMessage, out empty);
							return empty;
						}
					}
				}
			}
			return empty;
		}
		return empty;
	}

	public void RefreshQuestDescription()
	{
		if (m_CurrentGoalIndex >= m_ObjectiveGoals.Count)
		{
			return;
		}
		for (int num = m_CurrentGoalIndex; num >= 0; num--)
		{
			if (m_ObjectiveGoals[num] != null && m_ObjectiveGoals[num].m_Objective.GetObjectiveType() == BaseObjective.ObjectiveType.QuestIntroObjective)
			{
				m_QuestIntroObjective = (QuestIntroObjective)m_ObjectiveGoals[num].m_Objective;
				break;
			}
		}
	}

	public void PreActionBranch()
	{
		PreActionCurrent();
	}

	private void PreActionCurrent()
	{
		if (m_ObjectiveGoals[m_CurrentGoalIndex] != null)
		{
			m_ObjectiveGoals[m_CurrentGoalIndex].PreAction();
			return;
		}
		List<TreeBranch> list = m_SubBranches[m_CurrentGoalIndex];
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				list[i].PreActionBranch();
			}
		}
	}

	public void PostActionAndResetTree()
	{
		if (m_SubBranches != null)
		{
			foreach (KeyValuePair<int, List<TreeBranch>> subBranch in m_SubBranches)
			{
				for (int i = 0; i < subBranch.Value.Count; i++)
				{
					subBranch.Value[i].PostActionAndResetTree();
				}
			}
		}
		for (int j = 0; j < m_ObjectiveGoals.Count; j++)
		{
			if (m_ObjectiveGoals[j] != null)
			{
				m_ObjectiveGoals[j].PostAction();
				m_ObjectiveGoals[j].Reset();
			}
		}
	}

	public void PostActionLastGoal()
	{
		int num = m_ObjectiveGoals.Count - 1;
		if (num >= 0 && m_ObjectiveGoals[num] != null)
		{
			m_ObjectiveGoals[num].PostAction();
		}
	}

	public void SetLastGoalToIncomplete()
	{
		int num = m_ObjectiveGoals.Count - 1;
		if (num >= 0 && m_ObjectiveGoals[num] != null)
		{
			m_ObjectiveGoals[num].SetToIncomplete();
		}
	}

	private void ProcessCompletion()
	{
		if (m_QuestIntroObjective == null)
		{
			return;
		}
		if (m_QuestIntroObjective.Reward != 0)
		{
			if ((m_QuestIntroObjective.Reward & QuestIntroObjective.RewardType.Money) != 0 && m_PlayerOwner != null)
			{
				m_PlayerOwner.m_CharacterStats.IncreaseMoney(m_QuestIntroObjective.MoneyReward);
				EffectManager.GetInstance().NewEffectInstanceRPC(EffectManager.effectType.MoneyIncreased, m_PlayerOwner.GetStatChangeEffectPosition());
			}
			if ((m_QuestIntroObjective.Reward & QuestIntroObjective.RewardType.Item) != 0 && m_PlayerOwner != null && m_PlayerOwner.m_ItemContainer != null && m_QuestIntroObjective.ItemReward != null)
			{
				ItemManager.GetInstance().AssignItemRPC(m_PlayerOwner.photonView.ownerId, m_QuestIntroObjective.ItemReward.m_ItemDataID, OnItemMgrResponse, ref m_ItemRewardResponseID);
			}
		}
		if (m_QuestIntroObjective.m_ReputationGainOnSuccess != 0 && m_PlayerOwner != null && m_QuestGiver != null && m_QuestGiver.m_CharacterOpinions != null)
		{
			m_QuestGiver.m_CharacterOpinions.IncreaseOpinionOf(m_PlayerOwner, m_QuestIntroObjective.m_ReputationGainOnSuccess);
			EffectManager.PlayEffect(EffectManager.effectType.OpinionIncrease, m_QuestGiver.GetStatChangeEffectPosition());
		}
		if (m_PlayerOwner != null && m_PlayerOwner.m_Gamer != null && m_PlayerOwner.m_Gamer.IsLocal() && m_PlayerOwner.m_Gamer == Gamer.GetPrimaryGamer() && StatSystem.GetInstance() != null && m_QuestIntroObjective.CompletionStat != STAT_IDS.NoneStat)
		{
			StatSystem.GetInstance().IncStat((int)m_QuestIntroObjective.CompletionStat, 1f, Gamer.GetPrimaryGamer(), string.Empty);
		}
	}

	private void ProcessCancellation()
	{
		if (m_QuestIntroObjective != null && m_QuestGiver != null && m_PlayerOwner != null && m_QuestGiver.m_CharacterOpinions != null)
		{
			m_QuestGiver.m_CharacterOpinions.DecreaseOpinionOf(m_PlayerOwner, m_QuestIntroObjective.m_ReputationLossOnCancel);
			EffectManager.PlayEffect(EffectManager.effectType.OpinionDecrease, m_QuestGiver.GetStatChangeEffectPosition());
		}
	}

	private void ProcessFailure()
	{
		if (m_QuestIntroObjective != null && m_QuestGiver != null && m_QuestGiver.m_CharacterOpinions != null && m_PlayerOwner != null)
		{
			m_QuestGiver.m_CharacterOpinions.DecreaseOpinionOf(m_PlayerOwner, m_QuestIntroObjective.m_ReputationLossOnFailed);
			EffectManager.PlayEffect(EffectManager.effectType.OpinionDecrease, m_QuestGiver.GetStatChangeEffectPosition());
		}
	}

	private void OnItemMgrResponse(Item newItem, int eventID)
	{
		if (newItem != null && eventID == m_ItemRewardResponseID && !m_PlayerOwner.m_ItemContainer.AddItemRPC(newItem))
		{
			newItem.DropItemInLevel(m_PlayerOwner, m_PlayerOwner.transform.position);
		}
	}

	public ObjectiveGoal GetCurrentGoal()
	{
		if (m_CurrentGoalIndex == -1 || m_CurrentGoalIndex >= m_ObjectiveGoals.Count)
		{
			return null;
		}
		if (m_ObjectiveGoals[m_CurrentGoalIndex] != null)
		{
			return m_ObjectiveGoals[m_CurrentGoalIndex];
		}
		List<TreeBranch> list = m_SubBranches[m_CurrentGoalIndex];
		if (list != null)
		{
			TreeBranch treeBranch = null;
			for (int i = 0; i < list.Count; i++)
			{
				if (!list[i].IsBranchFinished)
				{
					treeBranch = list[i];
					break;
				}
			}
			if (treeBranch != null)
			{
				return treeBranch.GetCurrentGoal();
			}
		}
		return null;
	}

	public ObjectiveGoal GetLastObjective()
	{
		int count = m_ObjectiveGoals.Count;
		if (count == 0 || m_CurrentGoalIndex == -1)
		{
			return null;
		}
		if (m_ObjectiveGoals[count - 1] != null)
		{
			return m_ObjectiveGoals[count - 1];
		}
		List<TreeBranch> list = m_SubBranches[count - 1];
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].IsBranchFinished)
				{
					return list[i].GetLastObjective();
				}
			}
		}
		return null;
	}

	public ObjectiveGoal GetNextLogableGoal()
	{
		for (int i = m_CurrentGoalIndex; i < m_ObjectiveGoals.Count; i++)
		{
			if (m_ObjectiveGoals[i] != null)
			{
				if (m_ObjectiveGoals[i].IsLogable())
				{
					return m_ObjectiveGoals[i];
				}
				return null;
			}
			List<TreeBranch> list = m_SubBranches[i];
			if (list == null)
			{
				continue;
			}
			TreeBranch treeBranch = null;
			for (int j = 0; j < list.Count; j++)
			{
				if (!list[j].IsBranchFinished)
				{
					treeBranch = list[j];
					break;
				}
			}
			if (treeBranch != null)
			{
				return treeBranch.GetNextLogableGoal();
			}
		}
		return null;
	}

	public ObjectiveGoal GetWhichGoalComesFirst(ObjectiveGoal firstGoal, ObjectiveGoal secondGoal)
	{
		for (int i = 0; i < m_ObjectiveGoals.Count; i++)
		{
			if (m_ObjectiveGoals[i] != null)
			{
				if (object.ReferenceEquals(m_ObjectiveGoals[i], firstGoal) || object.ReferenceEquals(m_ObjectiveGoals[i], secondGoal))
				{
					return m_ObjectiveGoals[i];
				}
				continue;
			}
			List<TreeBranch> list = m_SubBranches[i];
			if (list == null)
			{
				continue;
			}
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j] != null)
				{
					ObjectiveGoal whichGoalComesFirst = list[j].GetWhichGoalComesFirst(firstGoal, secondGoal);
					if (whichGoalComesFirst != null)
					{
						return whichGoalComesFirst;
					}
					break;
				}
			}
		}
		return null;
	}

	public void EndTreeEarly(bool isTreeFailed)
	{
		if (m_BranchStatus == ObjectiveStatus.Done || m_BranchStatus == ObjectiveStatus.Failed || m_BranchStatus == ObjectiveStatus.Canceled)
		{
			return;
		}
		if (m_SubBranches != null)
		{
			foreach (KeyValuePair<int, List<TreeBranch>> subBranch in m_SubBranches)
			{
				for (int i = 0; i < subBranch.Value.Count; i++)
				{
					subBranch.Value[i].EndTreeEarly(isTreeFailed);
				}
			}
		}
		if (isTreeFailed)
		{
			m_BranchStatus = ObjectiveStatus.Failed;
			ProcessFailure();
		}
		else
		{
			m_BranchStatus = ObjectiveStatus.Canceled;
			ProcessCancellation();
		}
	}

	public void ResetObjectiveAnalytics()
	{
		if (m_SubBranches != null)
		{
			foreach (KeyValuePair<int, List<TreeBranch>> subBranch in m_SubBranches)
			{
				for (int i = 0; i < subBranch.Value.Count; i++)
				{
					subBranch.Value[i].ResetObjectiveAnalytics();
				}
			}
		}
		if (m_ObjectiveGoals == null)
		{
			return;
		}
		int j = 0;
		for (int count = m_ObjectiveGoals.Count; j < count; j++)
		{
			if (m_ObjectiveGoals[j] != null)
			{
				m_ObjectiveGoals[j].ResetAnalytics();
			}
		}
	}

	public void AddFirstGoal(ObjectiveGoal goal)
	{
		if (m_ObjectiveGoals == null)
		{
			m_ObjectiveGoals = new List<ObjectiveGoal>();
		}
		m_ObjectiveGoals.Clear();
		m_ObjectiveGoals.Add(goal);
		BuildOrderList(m_ObjectiveGoals[0].LinksTo, m_ObjectiveGoals[0]);
	}

	public void AddObjectiveGoal(ObjectiveGoal goal)
	{
		m_ObjectiveGoals.Add(goal);
	}

	public void BuildOrderList(List<int> linksToNodes, ObjectiveGoal currentGoal)
	{
		List<int> validFlowLinks = new List<int>();
		for (int j = 0; j < linksToNodes.Count; j++)
		{
			NodeHelp.DecodeNode(linksToNodes[j], out var nodeID, out var ourDirection, out var otherDirection);
			if (NodeHelp.GetDirectionFromFlag(ourDirection) == 3 && NodeHelp.GetDirectionFromFlag(otherDirection) == 2)
			{
				validFlowLinks.Add(nodeID);
			}
			if (currentGoal != null && NodeHelp.GetDirectionFromFlag(ourDirection) == 0 && NodeHelp.GetDirectionFromFlag(otherDirection) == 0)
			{
				currentGoal.AddDependencyNode(nodeID);
			}
		}
		if (validFlowLinks.Count == 0)
		{
			return;
		}
		if (validFlowLinks.Count == 1)
		{
			ObjectiveGoal objectiveGoal = m_RootParentTree.m_EditorObjectiveGoals.FirstOrDefault((ObjectiveGoal g) => g.NodeID == validFlowLinks[0]);
			if (objectiveGoal == null)
			{
				return;
			}
			bool flag = false;
			if (currentGoal != null)
			{
				for (int k = 0; k < m_RootParentTree.m_EditorObjectiveGoals.Count; k++)
				{
					for (int l = 0; l < m_RootParentTree.m_EditorObjectiveGoals[k].LinksTo.Count; l++)
					{
						if (m_RootParentTree.m_EditorObjectiveGoals[k].NodeID != currentGoal.NodeID)
						{
							NodeHelp.DecodeNode(m_RootParentTree.m_EditorObjectiveGoals[k].LinksTo[l], out var nodeID2, out var ourDirection2, out var otherDirection2);
							if (NodeHelp.GetDirectionFromFlag(ourDirection2) == 3 && NodeHelp.GetDirectionFromFlag(otherDirection2) == 2 && nodeID2 == objectiveGoal.NodeID)
							{
								flag = true;
								break;
							}
						}
					}
					if (flag)
					{
						break;
					}
				}
			}
			if (flag)
			{
				m_BranchEndGoal = objectiveGoal;
				return;
			}
			if (m_ObjectiveGoals == null)
			{
				AddFirstGoal(objectiveGoal);
				return;
			}
			m_ObjectiveGoals.Add(objectiveGoal);
			BuildOrderList(objectiveGoal.LinksTo, objectiveGoal);
			return;
		}
		m_ObjectiveGoals.Add(null);
		int num = m_ObjectiveGoals.Count - 1;
		if (m_SubBranches == null)
		{
			m_SubBranches = new Dictionary<int, List<TreeBranch>>();
		}
		m_SubBranches[num] = new List<TreeBranch>();
		Dictionary<ObjectiveGoal, int> dictionary = new Dictionary<ObjectiveGoal, int>();
		for (int i = 0; i < validFlowLinks.Count; i++)
		{
			ObjectiveGoal objectiveGoal2 = m_RootParentTree.m_EditorObjectiveGoals.FirstOrDefault((ObjectiveGoal g) => g.NodeID == validFlowLinks[i]);
			if (objectiveGoal2 == null)
			{
				continue;
			}
			m_SubBranches[num].Add(new TreeBranch(num, i, m_RootParentTree));
			m_SubBranches[num][i].AddFirstGoal(objectiveGoal2);
			if (m_SubBranches[num][i].BranchEndGoal != null)
			{
				if (dictionary.ContainsKey(m_SubBranches[num][i].BranchEndGoal))
				{
					dictionary[m_SubBranches[num][i].BranchEndGoal]++;
				}
				else
				{
					dictionary.Add(m_SubBranches[num][i].BranchEndGoal, 1);
				}
			}
		}
		if (dictionary.Count <= 1)
		{
			ObjectiveGoal key = dictionary.First().Key;
			m_ObjectiveGoals.Add(key);
			BuildOrderList(key.LinksTo, key);
		}
	}

	public List<TreeBranch> GetBranches(int branchID)
	{
		if (m_SubBranches.ContainsKey(branchID))
		{
			return m_SubBranches[branchID];
		}
		return null;
	}

	public bool LoadBranch(JObject objBranch)
	{
		if (objBranch != null)
		{
			if (m_ObjectiveGoals == null)
			{
				m_ObjectiveGoals = new List<ObjectiveGoal>();
			}
			if (Application.isPlaying)
			{
				JProperty jProperty = objBranch.Property("CurrentGoalIndex");
				if (jProperty != null)
				{
					m_CurrentGoalIndex = (int)jProperty.Value;
				}
				JProperty jProperty2 = objBranch.Property("BranchStatus");
				if (jProperty2 != null)
				{
					m_BranchStatus = (ObjectiveStatus)(int)jProperty2.Value;
				}
				JProperty jProperty3 = objBranch.Property("PlayerOwner");
				if (jProperty3 != null)
				{
					m_PlayerOwner = PhotonView.Find((int)jProperty3.Value).GetComponent<Player>();
				}
				JProperty jProperty4 = objBranch.Property("QuestGiver");
				if (jProperty4 != null)
				{
					m_QuestGiver = PhotonView.Find((int)jProperty4.Value).GetComponent<Character>();
				}
				JProperty jProperty5 = objBranch.Property("ObjectiveGoals");
				if (jProperty5 != null && jProperty5.Value.Type == JTokenType.Array)
				{
					JArray jArray = (JArray)jProperty5.Value;
					for (int i = 0; i < jArray.Count; i++)
					{
						if (jArray[i] != null && jArray[i].Type == JTokenType.Object)
						{
							JObject goalObj = (JObject)jArray[i];
							ObjectiveGoal objectiveGoal = new ObjectiveGoal();
							objectiveGoal.LoadGoal(goalObj, ingame: true);
							objectiveGoal.RegisterTokens(m_RootParentTree);
							m_ObjectiveGoals.Add(objectiveGoal);
						}
						else
						{
							m_ObjectiveGoals.Add(null);
						}
					}
				}
				JProperty jProperty6 = objBranch.Property("SubBranches");
				if (jProperty6 != null)
				{
					m_SubBranches = new Dictionary<int, List<TreeBranch>>();
					if (jProperty6.Value.Type == JTokenType.Array)
					{
						JArray jArray2 = (JArray)jProperty6.Value;
						for (int j = 0; j < jArray2.Count; j++)
						{
							if (jArray2[j] != null && jArray2[j].Type == JTokenType.Array)
							{
								JArray jArray3 = (JArray)jArray2[j];
								if (jArray3 == null)
								{
									continue;
								}
								for (int k = 0; k < jArray3.Count; k++)
								{
									if (jArray3[j] != null)
									{
										JObject jObject = (JObject)jArray3[j].Values().ElementAt(0);
										JProperty jProperty7 = jObject.Properties().ElementAt(0);
										int result = -1;
										int.TryParse(jProperty7.Name, out result);
										TreeBranch treeBranch = new TreeBranch(0, 0, m_RootParentTree);
										treeBranch.LoadBranch((JObject)jProperty7.Value);
										if (!m_SubBranches.ContainsKey(result))
										{
											m_SubBranches.Add(result, new List<TreeBranch>(1) { treeBranch });
										}
										else
										{
											m_SubBranches[result].Add(treeBranch);
										}
									}
								}
							}
							else
							{
								m_ObjectiveGoals.Add(null);
							}
						}
					}
				}
			}
		}
		return true;
	}

	public JObject SaveBranch()
	{
		JObject jObject = new JObject();
		if (Application.isPlaying)
		{
			jObject.Add(new JProperty("CurrentGoalIndex", m_CurrentGoalIndex));
			jObject.Add(new JProperty("BranchStatus", (int)m_BranchStatus));
			if (m_PlayerOwner != null)
			{
				jObject.Add(new JProperty("PlayerOwner", m_PlayerOwner.m_NetView.viewID));
			}
			if (m_QuestGiver != null)
			{
				jObject.Add(new JProperty("QuestGiver", m_QuestGiver.m_NetView.viewID));
			}
		}
		JProperty jProperty = new JProperty("ObjectiveGoals");
		JArray jArray = new JArray();
		for (int i = 0; i < m_ObjectiveGoals.Count; i++)
		{
			if (m_ObjectiveGoals[i] != null)
			{
				jArray.Add(m_ObjectiveGoals[i].SaveGoal(ingamesave: true));
			}
			else
			{
				jArray.Add("Branch");
			}
		}
		jProperty.Add(jArray);
		jObject.Add(jProperty);
		if (m_SubBranches != null)
		{
			JProperty jProperty2 = new JProperty("SubBranches");
			JArray jArray2 = new JArray();
			foreach (KeyValuePair<int, List<TreeBranch>> subBranch in m_SubBranches)
			{
				JArray jArray3 = new JArray();
				for (int j = 0; j < subBranch.Value.Count; j++)
				{
					JObject jObject2 = new JObject();
					jObject2.Add(new JProperty(subBranch.Key.ToString(), subBranch.Value[j].SaveBranch()));
					jArray3.Add(jObject2);
				}
				jArray2.Add(jArray3);
			}
			jProperty2.Add(jArray2);
			jObject.Add(jProperty2);
		}
		return jObject;
	}
}
