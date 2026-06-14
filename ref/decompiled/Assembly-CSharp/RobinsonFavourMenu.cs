using System.Collections;
using UnityEngine;

public class RobinsonFavourMenu : FavourMenu
{
	private Player m_CurrentPlayer;

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		if (currentGamer != null)
		{
			m_CurrentPlayer = currentGamer.m_PlayerObject;
		}
		return true;
	}

	public override void Cancel()
	{
		base.Cancel();
		ChangeRobinsonQuest(QuestManager.GetInstance().m_RobinsonDefaultQuest, -2, 0, bAccept: false);
	}

	public void StartQuestDigging()
	{
		ChangeRobinsonQuest(QuestManager.RobinsonQuests.Digging);
	}

	public void StartQuestChipping()
	{
		ChangeRobinsonQuest(QuestManager.RobinsonQuests.Chipping);
	}

	public void StartQuestCutting()
	{
		ChangeRobinsonQuest(QuestManager.RobinsonQuests.Cutting);
	}

	private void ChangeRobinsonQuest(QuestManager.RobinsonQuests robinsonQuest, bool bAccept = true)
	{
		if (robinsonQuest >= QuestManager.RobinsonQuests.Count)
		{
			return;
		}
		QuestManager instance = QuestManager.GetInstance();
		if (instance != null)
		{
			QuestManager.QuestMapping questMapping = instance.m_RobinsonQuests[(int)robinsonQuest];
			if (questMapping != null)
			{
				ChangeRobinsonQuest(questMapping.m_Quest, -3, (int)robinsonQuest, bAccept);
			}
		}
	}

	private void ChangeRobinsonQuest(QuestManager.QuestList newQuestList, int questType, int questIndex, bool bAccept = true)
	{
		QuestManager instance = QuestManager.GetInstance();
		if (!(instance != null))
		{
			return;
		}
		QuestManager.QuestGiver questGiver = instance.GetQuestGiver(m_GameMenuInformation.m_MenuRepresentative);
		if (questGiver != null)
		{
			questGiver.SetQuest(newQuestList, questType, questIndex);
			questGiver.PrepareMenu(this, m_CurrentPlayer, bReloadQuestTrees: true);
			if (bAccept)
			{
				questGiver.OnQuestAccepted();
				questGiver.m_QuestGiver.StartCoroutine(OnRobinsonQuestAccepted());
			}
		}
	}

	public IEnumerator OnRobinsonQuestAccepted()
	{
		QuestManager qMan = QuestManager.GetInstance();
		if (qMan == null)
		{
			yield break;
		}
		QuestManager.QuestGiver robinson = qMan.GetQuestGiver(m_GameMenuInformation.m_MenuRepresentative);
		if (robinson == null)
		{
			yield break;
		}
		ObjectiveTree questTree = robinson.GetActiveObjectiveTree();
		if (questTree == null)
		{
			yield break;
		}
		QuestIntroObjective questDataNode = questTree.MainBranch.GetQuestDescription();
		if (questDataNode == null)
		{
			yield break;
		}
		robinson.m_QuestGiverMessage = questDataNode.m_DescriptionLocalizationTag;
		if (!string.IsNullOrEmpty(robinson.m_QuestGiverMessage))
		{
			while (m_CurrentPlayer.IsInteracting())
			{
				yield return null;
			}
			NetObjectLock questGiverNOL = robinson.m_QuestGiver.GetNetObjectLock();
			TalkInteraction talkInteraction = questGiverNOL.GetFirstInteractionOfType<TalkInteraction>() as TalkInteraction;
			if (!(talkInteraction == null) && talkInteraction.AllowedToInteract(m_CurrentPlayer))
			{
				talkInteraction.Interact(m_CurrentPlayer);
			}
		}
	}
}
