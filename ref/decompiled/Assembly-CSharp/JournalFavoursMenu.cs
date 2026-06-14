using System.Collections.Generic;
using UnityEngine;

public class JournalFavoursMenu : GameMenuBehaviour
{
	public GameObject m_FavourListParent;

	public GameObject m_FavourPrefabObject;

	private ObjectiveManager m_ObjectiveManager;

	private FavourListItem[] m_FavourListItems;

	public T17Text m_QuestDescriptionText;

	public T17Text m_QuestCurrentGoalText;

	public T17Text m_QuestRewardText;

	public T17Text m_MultiPartText;

	public T17Text m_QuestGiverNameText;

	private int m_CancelledFavourItemIndex = -1;

	public List<GameObject> m_GameobjectsToEnableForActiveQuests = new List<GameObject>();

	public List<GameObject> m_GameobjectsToEnableForNoQuests = new List<GameObject>();

	private const string HOTKEY_JOURNALMENU = "Hotkey_Journal";

	protected override void OnDestroy()
	{
		m_ObjectiveManager = null;
		base.OnDestroy();
	}

	protected override void SingleTimeInitialize()
	{
		base.SingleTimeInitialize();
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		return base.Hide(restoreInvokerState, isTabSwitch);
	}

	public override void SetGamePlayer(Player gamePlayer)
	{
		base.SetGamePlayer(gamePlayer);
		if (base.CurrentGamePlayer != null)
		{
			if (m_ObjectiveManager == null)
			{
				m_ObjectiveManager = ObjectiveManager.GetInstance();
			}
			if (m_FavourListItems == null)
			{
				m_FavourListItems = m_FavourListParent.GetComponentsInChildren<FavourListItem>(includeInactive: true);
			}
		}
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		bool result = base.Show(currentGamer, parent, invoker, hideInvoker);
		PopulateFavourMenuWithData();
		return result;
	}

	public void PopulateFavourMenuWithData()
	{
		SetNoneActive();
		bool flag = false;
		if (base.CurrentGamePlayer != null)
		{
			List<ObjectiveTree> activeTrees = null;
			ObjectiveManager.GetInstance().GetActiveTrees(base.CurrentGamePlayer, out activeTrees);
			if (activeTrees != null)
			{
				CameraManager.PlayerBindingID playerCameraManagerBindingID = base.CurrentGamePlayer.m_PlayerCameraManagerBindingID;
				ObjectiveTrackerHUD playerObjectiveHUD = HUDMenuFlow.Instance.GetPlayerObjectiveHUD(playerCameraManagerBindingID);
				int num = 0;
				for (int i = 0; i < activeTrees.Count && i < m_FavourListItems.Length; i++)
				{
					FavourListItem favourListItem = m_FavourListItems[num];
					ObjectiveTree loopedTree = activeTrees[i];
					if (!(favourListItem != null) || loopedTree == null || !loopedTree.m_bShowInJournal || loopedTree.MainBranch.GetBranchStatus() == ObjectiveStatus.Canceled)
					{
						continue;
					}
					QuestIntroObjective questDescription = loopedTree.MainBranch.GetQuestDescription();
					if (questDescription == null)
					{
						continue;
					}
					flag = true;
					bool flag2 = false;
					if (playerObjectiveHUD != null)
					{
						flag2 = playerObjectiveHUD.IsTreeCurrentlyTracked(loopedTree);
					}
					favourListItem.m_ActiveIndicator.enabled = flag2;
					if (favourListItem.m_Title != null)
					{
						favourListItem.gameObject.SetActive(value: true);
						favourListItem.m_Title.m_bNeedsLocalization = false;
						favourListItem.m_Title.text = loopedTree.MainBranch.GetQuestLabel("NO LABEL");
					}
					int index = num;
					if (favourListItem.m_CancelButton != null)
					{
						favourListItem.m_CancelButton.onClick.AddListener(delegate
						{
							OnCancelButtonClicked(index, loopedTree);
						});
						favourListItem.m_CancelButton.OnButtonSelectEvent.AddListener(delegate
						{
							OnButtonSelected(index, loopedTree);
						});
					}
					if (favourListItem.m_QuestButton != null)
					{
						favourListItem.m_QuestButton.onClick.AddListener(delegate
						{
							OnQuestButtonClicked(index, loopedTree);
						});
						favourListItem.m_QuestButton.OnButtonSelectEvent.AddListener(delegate
						{
							OnButtonSelected(index, loopedTree);
						});
					}
					num++;
				}
				T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(base.CurrentGamer);
				if (eventSystemForGamer != null)
				{
					int num2 = Mathf.Min(m_CancelledFavourItemIndex, num - 1);
					if (num2 < 0)
					{
						num2 = 0;
					}
					FavourListItem favourListItem2 = m_FavourListItems[num2];
					eventSystemForGamer.SetSelectedGameObject(null);
					eventSystemForGamer.SetSelectedGameObject(favourListItem2.m_QuestButton.gameObject);
				}
			}
		}
		if (flag)
		{
			SetAllGameobjectsActive(m_GameobjectsToEnableForActiveQuests, state: true);
			SetAllGameobjectsActive(m_GameobjectsToEnableForNoQuests, state: false);
		}
	}

	private void SetAllGameobjectsActive(List<GameObject> gos, bool state)
	{
		for (int num = gos.Count - 1; num >= 0; num--)
		{
			if (gos[num] != null)
			{
				gos[num].SetActive(state);
			}
		}
	}

	private void OnCancelButtonClicked(int index, ObjectiveTree objectiveTree)
	{
		if (index < 0 || index >= m_FavourListItems.Length || objectiveTree == null)
		{
			return;
		}
		objectiveTree.EndTreeEarly(isTreeFailed: false);
		m_CancelledFavourItemIndex = index;
		PopulateFavourMenuWithData();
		CameraManager.PlayerBindingID playerCameraManagerBindingID = base.CurrentGamePlayer.m_PlayerCameraManagerBindingID;
		ObjectiveTrackerHUD playerObjectiveHUD = HUDMenuFlow.Instance.GetPlayerObjectiveHUD(playerCameraManagerBindingID);
		if (playerObjectiveHUD != null)
		{
			ObjectiveManager instance = ObjectiveManager.GetInstance();
			if (instance != null)
			{
				ObjectiveTree prisonObjectiveTree = instance.GetPrisonObjectiveTree(base.CurrentGamePlayer);
				playerObjectiveHUD.SetObjectiveTreeToTrack(prisonObjectiveTree);
			}
			else
			{
				playerObjectiveHUD.SetObjectiveTreeToTrack(null);
			}
		}
	}

	private void OnQuestButtonClicked(int index, ObjectiveTree objectiveTree)
	{
		for (int i = 0; i < m_FavourListItems.Length; i++)
		{
			m_FavourListItems[i].m_ActiveIndicator.enabled = false;
		}
		CameraManager.PlayerBindingID playerCameraManagerBindingID = base.CurrentGamePlayer.m_PlayerCameraManagerBindingID;
		ObjectiveTrackerHUD playerObjectiveHUD = HUDMenuFlow.Instance.GetPlayerObjectiveHUD(playerCameraManagerBindingID);
		bool flag = false;
		if (playerObjectiveHUD != null)
		{
			flag = playerObjectiveHUD.SetObjectiveTreeToTrack(objectiveTree);
		}
		FavourListItem favourListItem = m_FavourListItems[index];
		if (favourListItem != null)
		{
			favourListItem.m_ActiveIndicator.enabled = flag;
		}
	}

	private void OnButtonSelected(int index, ObjectiveTree objectiveTree)
	{
		if (index < 0 || index >= m_FavourListItems.Length || objectiveTree == null)
		{
			return;
		}
		if (m_QuestCurrentGoalText != null)
		{
			objectiveTree.MainBranch.GetCurrentGoal().UpdateMenuJournalInfo(m_QuestCurrentGoalText);
		}
		if (m_QuestRewardText != null)
		{
			QuestIntroObjective questDescription = objectiveTree.MainBranch.GetQuestDescription();
			if (questDescription != null)
			{
				m_QuestRewardText.gameObject.SetActive(value: true);
				m_QuestRewardText.m_bNeedsLocalization = false;
				if ((questDescription.Reward & QuestIntroObjective.RewardType.Escape) != 0)
				{
					string localized = string.Empty;
					if (Localization.Get("Text.Interact.Escape", out localized))
					{
						m_QuestRewardText.text = localized;
					}
				}
				else if ((questDescription.Reward & QuestIntroObjective.RewardType.Item) != 0)
				{
					string localized2 = string.Empty;
					if (Localization.Get(questDescription.ItemReward.m_ItemLocalizationTag, out localized2))
					{
						m_QuestRewardText.text = localized2;
					}
				}
				else if ((questDescription.Reward & QuestIntroObjective.RewardType.Money) != 0)
				{
					m_QuestRewardText.text = questDescription.MoneyReward.ToString();
				}
			}
		}
		if (m_QuestDescriptionText != null)
		{
			QuestIntroObjective questDescription2 = objectiveTree.MainBranch.GetQuestDescription();
			if (questDescription2 != null)
			{
				m_QuestDescriptionText.gameObject.SetActive(value: true);
				m_QuestDescriptionText.m_bNeedsLocalization = false;
				m_QuestDescriptionText.text = objectiveTree.MainBranch.GetQuestDescriptionLabel();
			}
		}
		if (m_MultiPartText != null)
		{
			if (!string.IsNullOrEmpty(objectiveTree.m_MultiPartText))
			{
				m_MultiPartText.gameObject.SetActive(value: true);
				m_MultiPartText.m_bNeedsLocalization = false;
				m_MultiPartText.text = objectiveTree.m_MultiPartText;
			}
			else
			{
				m_MultiPartText.gameObject.SetActive(value: false);
			}
		}
		if (m_QuestGiverNameText != null)
		{
			if (!string.IsNullOrEmpty(objectiveTree.MainBranch.QuestGiver.m_CharacterCustomisation.m_DisplayName))
			{
				m_QuestGiverNameText.gameObject.SetActive(value: true);
				m_QuestGiverNameText.SetNonLocalizedText(objectiveTree.MainBranch.QuestGiver.m_CharacterCustomisation.m_DisplayName + ":");
			}
			else
			{
				m_QuestGiverNameText.gameObject.SetActive(value: false);
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (base.CurrentGamer != null && base.CurrentGamePlayer != null && base.CurrentGamer.m_RewiredPlayer.GetButtonUp("Hotkey_Journal"))
		{
			base.CurrentGamePlayer.SwallowInputAction("Hotkey_Journal");
			base.CurrentGamePlayer.RequestCloseContainer();
		}
	}

	private void SetNoneActive()
	{
		for (int i = 0; i < m_FavourListItems.Length; i++)
		{
			m_FavourListItems[i].gameObject.SetActive(value: false);
			if (m_FavourListItems[i].m_QuestButton != null)
			{
				m_FavourListItems[i].m_QuestButton.OnButtonSelectEvent.RemoveAllListeners();
				m_FavourListItems[i].m_QuestButton.onClick.RemoveAllListeners();
			}
			if (m_FavourListItems[i].m_CancelButton != null)
			{
				m_FavourListItems[i].m_CancelButton.OnButtonSelectEvent.RemoveAllListeners();
				m_FavourListItems[i].m_CancelButton.onClick.RemoveAllListeners();
			}
		}
		if (m_QuestCurrentGoalText != null)
		{
			m_QuestCurrentGoalText.gameObject.SetActive(value: false);
		}
		if (m_QuestRewardText != null)
		{
			m_QuestRewardText.gameObject.SetActive(value: false);
		}
		if (m_QuestDescriptionText.text != null)
		{
			m_QuestDescriptionText.gameObject.SetActive(value: false);
		}
		if (m_MultiPartText != null)
		{
			m_MultiPartText.gameObject.SetActive(value: false);
		}
		if (m_QuestGiverNameText != null)
		{
			m_QuestGiverNameText.m_bNeedsLocalization = true;
			m_QuestGiverNameText.SetLocalisedTextCatchAll("Text.Journal.Name");
		}
		SetAllGameobjectsActive(m_GameobjectsToEnableForActiveQuests, state: false);
		SetAllGameobjectsActive(m_GameobjectsToEnableForNoQuests, state: true);
	}
}
