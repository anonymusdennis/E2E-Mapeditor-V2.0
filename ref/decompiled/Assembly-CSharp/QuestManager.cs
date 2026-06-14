using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using AUTOGEN_T17Wwise_Enums;
using NetworkLoadable;
using SaveHelpers;
using UnityEngine;

public class QuestManager : MonoBehaviour, IDeserializable, Saveable, INetworkLoadable
{
	public delegate void QuestHandler(ObjectiveTree tree, Player playerDoingQuest);

	[Serializable]
	public class QuestMapping
	{
		[SerializeField]
		public QuestList m_Quest;

		[SerializeField]
		public Character m_QuestGiver;

		[SerializeField]
		public bool m_AutoCreate;
	}

	[Serializable]
	public class QuestType
	{
		[SerializeField]
		public int m_AvailablePercentage = 100;

		[SerializeField]
		public string m_QuestType = "Quest Type";

		[SerializeField]
		public List<QuestList> m_QuestList = new List<QuestList>();

		[Tooltip("How often should a character refresh it's available quest")]
		[SerializeField]
		public int m_QuestRefreshTimeInHours = 48;

		[SerializeField]
		public bool m_bIsMultiFavour;
	}

	[Serializable]
	public class QuestList
	{
		[SerializeField]
		public string m_QuestName = "Quest Name";

		[SerializeField]
		public List<TextAsset> m_Quests = new List<TextAsset>();

		private int m_CurrentQuest;

		public TextAsset CurrentQuest => m_Quests[m_CurrentQuest];

		public int CurrentQuestIndex => m_CurrentQuest;

		public bool GotoNextQuest()
		{
			return GoToSpecificQuest(m_CurrentQuest + 1, cacheQuest: false);
		}

		public bool GoToSpecificQuest(int index, bool cacheQuest)
		{
			m_CurrentQuest = index;
			if (m_CurrentQuest >= m_Quests.Count)
			{
				m_CurrentQuest = 0;
				return false;
			}
			if (cacheQuest)
			{
				GetInstance().CacheQuestTextData(m_Quests[m_CurrentQuest]);
			}
			return true;
		}
	}

	public enum RobinsonQuests
	{
		Digging,
		Chipping,
		Cutting,
		Count
	}

	private class PlayerQuestItemGroups
	{
		public Dictionary<int, List<int>> groups = new Dictionary<int, List<int>>();

		public int maxGroupIndex = -1;
	}

	[Serializable]
	public class NetQuestManagerSaveData
	{
		public List<ulong> m_QuestGiverPool = new List<ulong>();

		public string m_QuestItems = string.Empty;
	}

	[Serializable]
	public class QuestGiver
	{
		public Character m_QuestGiver;

		public Player m_PlayerDoingQuest;

		public QuestList m_AvailableQuests;

		private List<ObjectiveTree> m_QuestTrees;

		public const int QUEST_TYPE_SPECIFIC_GIVER = -2;

		public const int QUEST_TYPE_ROBINSON = -3;

		private int m_QuestTypeIndex = -1;

		private int m_QuestListIndex = -1;

		public RoutineManager.CallbackInGameTimer m_ExpireCallbackTimer;

		public string m_QuestGiverMessage = string.Empty;

		public T17NetView m_NetView;

		public int m_PinID = -1;

		private int m_QuestGiverID = -1;

		private bool m_bIsExpired;

		private bool m_bIsAccepted;

		private bool m_bTalkingToQuestgiver;

		private Player m_LastKnownPlayer;

		public int QuestGiverID => m_QuestGiverID;

		public bool IsExpired => m_bIsExpired;

		public bool IsAccepted => m_bIsAccepted;

		public QuestGiver(int index)
		{
			m_QuestGiverID = index;
			ResetInfo();
		}

		~QuestGiver()
		{
			m_NetView = null;
		}

		public int GetTypeIndex()
		{
			return m_QuestTypeIndex;
		}

		public void SetAccepted()
		{
			m_bIsAccepted = true;
		}

		public void SetQuest(QuestList questList, int typeIndex, int randomIndex)
		{
			m_QuestTypeIndex = typeIndex;
			m_QuestListIndex = randomIndex;
			m_AvailableQuests = new QuestList();
			m_AvailableQuests.m_Quests = new List<TextAsset>(questList.m_Quests);
			m_AvailableQuests.m_QuestName = questList.m_QuestName;
			m_QuestGiverMessage = string.Empty;
		}

		public void ResetInfo()
		{
			if (m_QuestTrees != null && m_QuestTrees.Count > 0 && m_QuestTrees[0] != null && m_QuestTrees[0].GetObjectiveStatus != ObjectiveStatus.Done)
			{
				m_QuestTrees[0].EndTreeEarly(isTreeFailed: false);
			}
			m_PlayerDoingQuest = null;
			m_AvailableQuests = null;
			if (m_QuestTypeIndex != -2)
			{
				m_QuestTypeIndex = -1;
			}
			m_QuestListIndex = -1;
			m_bIsExpired = false;
			m_bIsAccepted = false;
			ShowQuestAvailable(available: false);
			if (m_ExpireCallbackTimer != null)
			{
				RoutineManager.GetInstance().RemoveCallbackTimer(m_ExpireCallbackTimer);
			}
			m_ExpireCallbackTimer = null;
			m_LastKnownPlayer = null;
		}

		public void ShowQuestAvailable(bool available)
		{
			if (m_QuestGiver != null && m_QuestGiver.m_TrackableElementReporter != null)
			{
				if (available)
				{
					m_QuestGiver.m_IconHandler.DisplayIcon(CharacterIconHandler.IconType.Quest);
				}
				else
				{
					m_QuestGiver.m_IconHandler.RemoveIcon(CharacterIconHandler.IconType.Quest);
				}
				m_QuestGiver.ResetPinImage(PinManager.Pin.PinFilterType.Favours);
			}
			if (available)
			{
				QuestManager instance = GetInstance();
				if (instance != null)
				{
					m_QuestGiver.SetPinImage(instance.GetMapQuestSprite(m_QuestGiver), PinManager.Pin.PinFilterType.Favours);
				}
			}
		}

		public void PrepareMenu(FavourMenu favourMenu, Player player, bool bReloadQuestTrees = false)
		{
			m_bTalkingToQuestgiver = true;
			m_PlayerDoingQuest = player;
			if (bReloadQuestTrees || m_QuestTrees == null || m_LastKnownPlayer != m_PlayerDoingQuest)
			{
				m_LastKnownPlayer = m_PlayerDoingQuest;
				if (m_AvailableQuests != null)
				{
					m_QuestTrees = GetInstance().GetCachedObjectiveTreeData(m_AvailableQuests.CurrentQuest);
					if (m_QuestTrees != null)
					{
						_PrepareMenu(favourMenu);
					}
					else
					{
						m_QuestTrees = new List<ObjectiveTree>();
						if (ObjectiveManager.GetInstance().LoadObjectiveTreeData(m_AvailableQuests.CurrentQuest, ref m_QuestTrees))
						{
							_PrepareMenu(favourMenu);
						}
					}
				}
			}
			else if (m_QuestTrees.Count == 1)
			{
				m_QuestTrees[0].MainBranch.SetBaseInfo(m_PlayerDoingQuest, m_QuestGiver);
				QuestIntroObjective rewardInfo = m_QuestTrees[0].MainBranch.GetQuestDescription();
				if (rewardInfo != null)
				{
					m_QuestTrees[0].m_bShowTrackingArrows = rewardInfo.m_bShouldShowQuestArrow;
					if (rewardInfo.m_bUsePerObjectiveTitles)
					{
						ObjectiveGoal nextLogableGoal = m_QuestTrees[0].MainBranch.GetNextLogableGoal();
						if (nextLogableGoal != null)
						{
							favourMenu.SetTitleText(nextLogableGoal.m_Objective.LocalizedObjectiveName);
						}
					}
					else
					{
						favourMenu.SetTitleText(rewardInfo.QuestLocalizedObjectiveName);
					}
					if (string.IsNullOrEmpty(m_QuestGiverMessage))
					{
						favourMenu.SetDescriptionText(rewardInfo.QuestLocalizedDescription);
					}
					else
					{
						string localized = string.Empty;
						Localization.Get(m_QuestGiverMessage, out localized);
						favourMenu.SetDescriptionText(localized);
					}
					favourMenu.SetQuestGiverNameText(m_QuestGiver.m_CharacterCustomisation.m_DisplayName);
					favourMenu.SetQuestRewardText(ref rewardInfo);
					favourMenu.SetQuestGiver(this);
				}
				if (m_AvailableQuests.m_Quests.Count > 1)
				{
					m_QuestTrees[0].m_MultiPartText = favourMenu.SetMultipart(m_AvailableQuests.CurrentQuestIndex + 1, m_AvailableQuests.m_Quests.Count);
				}
				else
				{
					m_QuestTrees[0].m_MultiPartText = string.Empty;
					favourMenu.DisableMultipart();
				}
			}
			favourMenu.OnConfirm = OnQuestAccepted;
			favourMenu.OnDecline = OnQuestDeclined;
			favourMenu.OnCancel = OnQuestCanceled;
			m_QuestGiverMessage = string.Empty;
		}

		private void _PrepareMenu(FavourMenu favourMenu)
		{
			if (m_QuestTrees.Count != 1)
			{
				return;
			}
			m_QuestTrees[0].BuildOrderList();
			m_QuestTrees[0].MainBranch.SetBaseInfo(m_PlayerDoingQuest, m_QuestGiver);
			m_QuestTrees[0].MainBranch.PickAllRandomTargets();
			m_QuestTrees[0].m_bShowTrackingArrows = false;
			QuestIntroObjective rewardInfo = m_QuestTrees[0].MainBranch.GetQuestDescription();
			if (rewardInfo != null)
			{
				m_QuestTrees[0].m_bShowTrackingArrows = rewardInfo.m_bShouldShowQuestArrow;
				favourMenu.SetTitleText(rewardInfo.QuestLocalizedObjectiveName);
				if (string.IsNullOrEmpty(m_QuestGiverMessage))
				{
					favourMenu.SetDescriptionText(rewardInfo.QuestLocalizedDescription);
				}
				else
				{
					string localized = string.Empty;
					Localization.Get(m_QuestGiverMessage, out localized);
					favourMenu.SetDescriptionText(localized);
				}
				favourMenu.SetQuestGiverNameText(m_QuestGiver.m_CharacterCustomisation.m_DisplayName);
				favourMenu.SetQuestRewardText(ref rewardInfo);
				favourMenu.SetQuestGiver(this);
			}
			if (m_AvailableQuests.m_Quests.Count > 1)
			{
				m_QuestTrees[0].m_MultiPartText = favourMenu.SetMultipart(m_AvailableQuests.CurrentQuestIndex + 1, m_AvailableQuests.m_Quests.Count);
				return;
			}
			m_QuestTrees[0].m_MultiPartText = string.Empty;
			favourMenu.DisableMultipart();
		}

		public void ConfirmMenuClose()
		{
			m_bTalkingToQuestgiver = false;
			if (!m_bIsAccepted)
			{
				m_PlayerDoingQuest = null;
				m_QuestTrees = null;
			}
		}

		public void HideMenu()
		{
			if (m_PlayerDoingQuest != null)
			{
				m_PlayerDoingQuest.RequestStopInteraction();
			}
			if (m_PlayerDoingQuest != null)
			{
				m_PlayerDoingQuest.RequestCloseContainer();
			}
		}

		public void OnQuestAccepted()
		{
			m_bIsAccepted = true;
			if (m_QuestTrees == null)
			{
			}
			if (m_AvailableQuests != null && m_QuestTrees != null && m_QuestTrees.Count > 0)
			{
				if (TutorialManager.GetInstance() != null)
				{
					TutorialManager.GetInstance().StartTutorialRPC(m_PlayerDoingQuest, TutorialSubject.QuestLog);
				}
				m_QuestGiver.SetHasQuest(value: false);
				for (int i = 0; i < m_QuestTrees.Count; i++)
				{
					m_QuestTrees[i].Initialize();
				}
				ObjectiveManager.GetInstance().AddActiveTrees(m_PlayerDoingQuest, m_QuestTrees);
				CameraManager.PlayerBindingID playerCameraManagerBindingID = m_PlayerDoingQuest.m_PlayerCameraManagerBindingID;
				ObjectiveTrackerHUD playerObjectiveHUD = HUDMenuFlow.Instance.GetPlayerObjectiveHUD(playerCameraManagerBindingID);
				if (playerObjectiveHUD != null)
				{
					playerObjectiveHUD.SetObjectiveTreeToTrack(m_QuestTrees[0], force: true);
				}
				m_PlayerDoingQuest.IncreaseActiveQuests();
				GoogleAnalyticsV3.LogCommericalAnalyticEvent("Quests Completed", LevelScript.GetCurrentLevelInfo().m_PrisonEnum.ToString() + " Completed", m_QuestTrees[0].MainBranch.GetQuestDescription().m_QTitleLocalizationTag + " Started", 0L);
			}
			if (m_ExpireCallbackTimer != null)
			{
				RoutineManager.GetInstance().RemoveCallbackTimer(m_ExpireCallbackTimer);
			}
			if (m_QuestTrees != null && m_QuestTrees.Count > 0)
			{
				ObjectiveTree objectiveTree = m_QuestTrees[0];
				objectiveTree.OnObjectiveTreeCompleted = (ObjectiveTree.OnObjectiveTreeEvent)Delegate.Remove(objectiveTree.OnObjectiveTreeCompleted, new ObjectiveTree.OnObjectiveTreeEvent(OnObjectiveTreeCompleted));
				ObjectiveTree objectiveTree2 = m_QuestTrees[0];
				objectiveTree2.OnObjectiveTreeCompleted = (ObjectiveTree.OnObjectiveTreeEvent)Delegate.Combine(objectiveTree2.OnObjectiveTreeCompleted, new ObjectiveTree.OnObjectiveTreeEvent(OnObjectiveTreeCompleted));
				ObjectiveTree objectiveTree3 = m_QuestTrees[0];
				objectiveTree3.OnObjectiveTreeCanceled = (ObjectiveTree.OnObjectiveTreeEvent)Delegate.Remove(objectiveTree3.OnObjectiveTreeCanceled, new ObjectiveTree.OnObjectiveTreeEvent(OnObjectiveTreeCanceled));
				ObjectiveTree objectiveTree4 = m_QuestTrees[0];
				objectiveTree4.OnObjectiveTreeCanceled = (ObjectiveTree.OnObjectiveTreeEvent)Delegate.Combine(objectiveTree4.OnObjectiveTreeCanceled, new ObjectiveTree.OnObjectiveTreeEvent(OnObjectiveTreeCanceled));
				ObjectiveTree objectiveTree5 = m_QuestTrees[0];
				objectiveTree5.OnObjectiveTreeFailed = (ObjectiveTree.OnObjectiveTreeEvent)Delegate.Remove(objectiveTree5.OnObjectiveTreeFailed, new ObjectiveTree.OnObjectiveTreeEvent(OnObjectiveTreeFailed));
				ObjectiveTree objectiveTree6 = m_QuestTrees[0];
				objectiveTree6.OnObjectiveTreeFailed = (ObjectiveTree.OnObjectiveTreeEvent)Delegate.Combine(objectiveTree6.OnObjectiveTreeFailed, new ObjectiveTree.OnObjectiveTreeEvent(OnObjectiveTreeFailed));
			}
			ShowQuestAvailable(available: false);
			HideMenu();
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Quest_Accept, m_QuestGiver.gameObject);
			if (m_NetView != null)
			{
				m_NetView.RPC("RPC_ALL_RemoveQuestGiver", NetTargets.All, m_QuestGiverID, m_PlayerDoingQuest.m_NetView.viewID);
			}
		}

		private void OnQuestDeclined()
		{
			if (m_ExpireCallbackTimer != null)
			{
				RoutineManager.GetInstance().RemoveCallbackTimer(m_ExpireCallbackTimer);
			}
			HideMenu();
			m_QuestGiver.SetHasQuest(value: false);
			ShowQuestAvailable(available: false);
			if (!(m_NetView != null))
			{
				return;
			}
			if (m_QuestTypeIndex == -2)
			{
				m_NetView.RPC("RPC_ALL_RemoveQuestGiver", NetTargets.All, m_QuestGiverID, -1);
				GetInstance().CreateSpecificCharacterQuest(m_QuestGiver, bForce: true);
				return;
			}
			m_NetView.RPC("RPC_ALL_RemoveQuestGiver", NetTargets.All, m_QuestGiverID, -1);
			if (m_QuestTrees != null && m_QuestTrees.Count > 0 && m_QuestTrees[0] != null && m_QuestTrees[0].GetObjectiveStatus != ObjectiveStatus.Done)
			{
				m_QuestTrees[0].EndTreeEarly(isTreeFailed: false);
				OnObjectiveTreeCanceled(m_QuestTrees[0]);
			}
		}

		private void OnQuestCanceled()
		{
			HideMenu();
			m_PlayerDoingQuest = null;
		}

		public void OnQuestExpired()
		{
			m_bIsExpired = true;
		}

		private void OnObjectiveTreeCompleted(ObjectiveTree tree)
		{
			tree.OnObjectiveTreeCompleted = (ObjectiveTree.OnObjectiveTreeEvent)Delegate.Remove(tree.OnObjectiveTreeCompleted, new ObjectiveTree.OnObjectiveTreeEvent(OnObjectiveTreeCompleted));
			tree.OnObjectiveTreeCanceled = (ObjectiveTree.OnObjectiveTreeEvent)Delegate.Remove(tree.OnObjectiveTreeCanceled, new ObjectiveTree.OnObjectiveTreeEvent(OnObjectiveTreeCanceled));
			tree.OnObjectiveTreeFailed = (ObjectiveTree.OnObjectiveTreeEvent)Delegate.Remove(tree.OnObjectiveTreeFailed, new ObjectiveTree.OnObjectiveTreeEvent(OnObjectiveTreeFailed));
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Quest_Passed, GetInstance().gameObject);
			if (QuestManager.QuestCompletedEvent != null)
			{
				QuestManager.QuestCompletedEvent(tree, m_PlayerDoingQuest);
			}
			if (m_PlayerDoingQuest.m_Gamer != null && m_PlayerDoingQuest.m_Gamer.IsLocal())
			{
				StatSystem.GetInstance().IncStat(14, 1f, m_PlayerDoingQuest.m_Gamer, string.Empty);
				ScoreManager.EventRPC(ScoreManager.Events.FavourCompleted, m_PlayerDoingQuest);
				GoogleAnalyticsV3.LogCommericalAnalyticEvent("Quests Completed", LevelScript.GetCurrentLevelInfo().m_PrisonEnum.ToString() + " Completed", tree.MainBranch.GetQuestDescription().m_QTitleLocalizationTag + " Completed", 0L);
			}
			if (m_PlayerDoingQuest != null)
			{
				m_PlayerDoingQuest.DecreaseActiveQuests();
			}
			if (!m_AvailableQuests.GotoNextQuest())
			{
				m_NetView.RPC("RPC_ALL_FreeAcceptedQuest", NetTargets.All, m_QuestGiverID);
				return;
			}
			m_QuestGiver.SetHasQuest(value: true);
			ShowQuestAvailable(available: true);
			m_QuestTrees.Clear();
			m_QuestTrees = null;
			m_bIsAccepted = false;
		}

		private void OnObjectiveTreeFailed(ObjectiveTree tree)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Quest_Failed, GetInstance().gameObject);
			tree.OnObjectiveTreeCompleted = (ObjectiveTree.OnObjectiveTreeEvent)Delegate.Remove(tree.OnObjectiveTreeCompleted, new ObjectiveTree.OnObjectiveTreeEvent(OnObjectiveTreeCompleted));
			tree.OnObjectiveTreeCanceled = (ObjectiveTree.OnObjectiveTreeEvent)Delegate.Remove(tree.OnObjectiveTreeCanceled, new ObjectiveTree.OnObjectiveTreeEvent(OnObjectiveTreeCanceled));
			tree.OnObjectiveTreeFailed = (ObjectiveTree.OnObjectiveTreeEvent)Delegate.Remove(tree.OnObjectiveTreeFailed, new ObjectiveTree.OnObjectiveTreeEvent(OnObjectiveTreeFailed));
			if (QuestManager.QuestFailedEvent != null)
			{
				QuestManager.QuestFailedEvent(tree, m_PlayerDoingQuest);
			}
			if (m_PlayerDoingQuest != null)
			{
				m_PlayerDoingQuest.DecreaseActiveQuests();
			}
			if (m_QuestTypeIndex == -2 || m_QuestTypeIndex == -3)
			{
				m_NetView.RPC("RPC_ALL_FreeAcceptedQuest", NetTargets.All, m_QuestGiverID);
				GetInstance().CreateSpecificCharacterQuest(m_QuestGiver, bForce: true);
			}
			else
			{
				m_NetView.RPC("RPC_ALL_FreeAcceptedQuest", NetTargets.All, m_QuestGiverID);
			}
		}

		private void OnObjectiveTreeCanceled(ObjectiveTree tree)
		{
			tree.OnObjectiveTreeCompleted = (ObjectiveTree.OnObjectiveTreeEvent)Delegate.Remove(tree.OnObjectiveTreeCompleted, new ObjectiveTree.OnObjectiveTreeEvent(OnObjectiveTreeCompleted));
			tree.OnObjectiveTreeCanceled = (ObjectiveTree.OnObjectiveTreeEvent)Delegate.Remove(tree.OnObjectiveTreeCanceled, new ObjectiveTree.OnObjectiveTreeEvent(OnObjectiveTreeCanceled));
			tree.OnObjectiveTreeFailed = (ObjectiveTree.OnObjectiveTreeEvent)Delegate.Remove(tree.OnObjectiveTreeFailed, new ObjectiveTree.OnObjectiveTreeEvent(OnObjectiveTreeFailed));
			if (m_PlayerDoingQuest != null)
			{
				m_PlayerDoingQuest.DecreaseActiveQuests();
			}
			if (m_QuestTypeIndex == -2 || m_QuestTypeIndex == -3)
			{
				m_NetView.RPC("RPC_ALL_FreeAcceptedQuest", NetTargets.All, m_QuestGiverID);
				GetInstance().CreateSpecificCharacterQuest(m_QuestGiver, bForce: true);
			}
			else
			{
				m_NetView.RPC("RPC_ALL_FreeAcceptedQuest", NetTargets.All, m_QuestGiverID);
			}
		}

		public ulong Serialize()
		{
			BitField bitField = new BitField();
			int uValue = 0;
			if (m_ExpireCallbackTimer != null)
			{
				uValue = (int)(m_ExpireCallbackTimer.TimeLeft / 60f / 60f);
				uValue = Mathf.Clamp(uValue, 0, 127);
			}
			int uValue2 = 0;
			if (m_QuestGiver != null && m_QuestGiver.m_NetView != null)
			{
				uValue2 = m_QuestGiver.m_NetView.viewID;
			}
			bitField.Set(12, (uint)uValue2);
			bitField.Set(12, (m_PlayerDoingQuest == null) ? 4095u : ((!m_bTalkingToQuestgiver) ? ((uint)m_PlayerDoingQuest.m_NetView.viewID) : 4095u));
			bitField.Set(5, (uint)m_QuestGiverID);
			bitField.Set(7, (uint)uValue);
			bitField.Set(3, m_QuestTypeIndex);
			bitField.Set(5, m_QuestListIndex);
			int num = ((m_QuestTrees == null) ? (-1) : GetActiveObjectiveTree().ActiveTreeID);
			if (num > 127)
			{
				bitField.Set(7, 0x7F & num);
			}
			else
			{
				bitField.Set(7, num);
			}
			Debug.Log(" *******  before quest index " + bitField.ToString());
			if (m_AvailableQuests != null)
			{
				Debug.Log(" *******             quest index " + m_AvailableQuests.CurrentQuestIndex);
			}
			bitField.Set(6, (m_AvailableQuests != null) ? ((uint)m_AvailableQuests.CurrentQuestIndex) : 0u);
			Debug.Log(" *******  after quest index " + bitField.ToString());
			if (num > 127)
			{
				num >>= 7;
				bitField.Set(4, (uint)num);
			}
			return (ulong)bitField;
		}

		public ObjectiveTree GetActiveObjectiveTree()
		{
			if (m_QuestTrees != null && m_QuestTrees.Count > 0 && m_QuestTrees[0] != null)
			{
				return m_QuestTrees[0];
			}
			return null;
		}

		public void OnQuestDeserialized(int activeTreeID)
		{
			if (activeTreeID >= 0 && m_QuestTrees == null)
			{
				List<ObjectiveTree> activeTrees = new List<ObjectiveTree>();
				ObjectiveManager.GetInstance().GetActiveTrees(m_PlayerDoingQuest, out activeTrees);
				for (int i = 0; i < activeTrees.Count; i++)
				{
					if (activeTreeID == activeTrees[i].ActiveTreeID)
					{
						if (m_QuestTrees == null)
						{
							m_QuestTrees = new List<ObjectiveTree>(1);
						}
						m_QuestTrees.Add(activeTrees[i]);
						break;
					}
				}
			}
			if (m_QuestTrees != null && m_QuestTrees.Count > 0)
			{
				m_PlayerDoingQuest.IncreaseActiveQuests();
				ObjectiveTree activeObjectiveTree = GetActiveObjectiveTree();
				activeObjectiveTree.OnObjectiveTreeCompleted = (ObjectiveTree.OnObjectiveTreeEvent)Delegate.Combine(activeObjectiveTree.OnObjectiveTreeCompleted, new ObjectiveTree.OnObjectiveTreeEvent(OnObjectiveTreeCompleted));
				ObjectiveTree activeObjectiveTree2 = GetActiveObjectiveTree();
				activeObjectiveTree2.OnObjectiveTreeCanceled = (ObjectiveTree.OnObjectiveTreeEvent)Delegate.Combine(activeObjectiveTree2.OnObjectiveTreeCanceled, new ObjectiveTree.OnObjectiveTreeEvent(OnObjectiveTreeCanceled));
				ObjectiveTree activeObjectiveTree3 = GetActiveObjectiveTree();
				activeObjectiveTree3.OnObjectiveTreeFailed = (ObjectiveTree.OnObjectiveTreeEvent)Delegate.Combine(activeObjectiveTree3.OnObjectiveTreeFailed, new ObjectiveTree.OnObjectiveTreeEvent(OnObjectiveTreeFailed));
			}
		}
	}

	private static QuestManager s_Instance;

	public List<QuestType> m_AvailableQuests = new List<QuestType>();

	public List<QuestMapping> m_SpecificQuests = new List<QuestMapping>();

	public QuestList m_RobinsonDefaultQuest = new QuestList();

	public QuestMapping[] m_RobinsonQuests = new QuestMapping[3];

	public Sprite m_RobinsonMapQuestIcon;

	[Tooltip("How much of the inmates can offer quests at the same time")]
	[Range(1f, 100f)]
	public int m_MaxPercentageQuestGivers = 25;

	[Tooltip("How long the game will wait with  giving inmatess quests, since the level started")]
	public int m_TimeInHoursBeforeInmatesHaveQuests = 12;

	public Sprite m_MapQuestIcon;

	public string m_MapToolTipTag = string.Empty;

	private const int MAX_QUESTS = 24;

	private QuestGiver[] m_QuestGiverPool = new QuestGiver[24];

	private List<QuestGiver> m_OfferingQuestGivers = new List<QuestGiver>();

	private QuestGiver[] m_AcceptedQuests = new QuestGiver[24];

	private List<Character> m_GuardCharacters = new List<Character>();

	private List<Character> m_InmateCharacters = new List<Character>();

	private Dictionary<int, int> m_QuestItems = new Dictionary<int, int>();

	private Dictionary<int, PlayerQuestItemGroups> m_PlayerQuestItemGroups = new Dictionary<int, PlayerQuestItemGroups>();

	private bool m_bStartGivingQuests;

	public const int NO_PLAYER = 4095;

	private Dictionary<TextAsset, List<ObjectiveTree>> m_CachedObjectiveTrees = new Dictionary<TextAsset, List<ObjectiveTree>>();

	private int m_MaxQuestGivers;

	private float m_ElapsedGivingQuestTime;

	private T17NetView m_NetView;

	private NetQuestManagerSaveData m_NetQMSaveData = new NetQuestManagerSaveData();

	private SaveDataRegister m_SaveData;

	public Character m_GDCQuestGiver;

	public const int m_QuestIDLowerBits = 7;

	public const int m_QuestIDLowerValueMax = 127;

	private const int MAX_CONCURRENT_GROUPS = 6;

	private string m_LoadError = string.Empty;

	private LOADSTATE m_LoadState;

	private static int m_DebugQuestCount = -1;

	public static event QuestHandler QuestCompletedEvent;

	public static event QuestHandler QuestFailedEvent;

	public static QuestManager GetInstance()
	{
		return s_Instance;
	}

	private void Awake()
	{
		if (s_Instance != null)
		{
			UnityEngine.Object.Destroy(this);
		}
		else
		{
			s_Instance = this;
		}
	}

	private void Start()
	{
		m_NetView = GetComponent<T17NetView>();
		m_SaveData = new SaveDataRegister(this, m_NetView.viewID, bIsMajorManagerComponent: true, 11);
		NetLoadManagerSync.m_AllNetworkLoadables.Add(this);
	}

	protected virtual void OnDestroy()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
		}
		if (m_SaveData != null)
		{
			m_SaveData.Dispose();
		}
		Gamer.OnDeleteImminent -= OnGamerDeleteImminent;
		NetLoadManagerSync.m_AllNetworkLoadables.Remove(this);
		T17NetManager.OnBecameMasterClient -= OnBecameMasterClient;
		m_CachedObjectiveTrees.Clear();
		QuestManager.QuestCompletedEvent = null;
		QuestManager.QuestFailedEvent = null;
		m_NetView = null;
	}

	public void Initialise()
	{
		Gamer.OnDeleteImminent -= OnGamerDeleteImminent;
		Gamer.OnDeleteImminent += OnGamerDeleteImminent;
		T17NetManager.OnBecameMasterClient += OnBecameMasterClient;
		if (ConfigManager.GetInstance() != null && ConfigManager.GetInstance().questConfig != null)
		{
			ApplyConfigData(ConfigManager.GetInstance().questConfig);
		}
	}

	private void ApplyConfigData(QuestConfig config)
	{
		m_AvailableQuests = config.m_OverrideQuests;
		m_MaxPercentageQuestGivers = config.m_MaxPercentageQuestGivers;
		m_TimeInHoursBeforeInmatesHaveQuests = config.m_TimeInHoursBeforeInmatesHaveQuests;
		if (!config.m_bAllowSpecificQuests)
		{
			m_SpecificQuests.Clear();
		}
	}

	public void Begin()
	{
		if (RoutineManager.GetInstance() != null)
		{
			RoutineManager.GetInstance().CreateCallbackTimer(m_TimeInHoursBeforeInmatesHaveQuests / 24, m_TimeInHoursBeforeInmatesHaveQuests % 24, 0, StartHandingOutQuests);
		}
		m_MaxQuestGivers = Mathf.CeilToInt((float)(m_InmateCharacters.Count * m_MaxPercentageQuestGivers) / 100f);
		for (int num = m_AvailableQuests.Count - 1; num >= 0; num--)
		{
			GlobalStart.TimedNetworkService();
			if (m_AvailableQuests[num] != null)
			{
				for (int num2 = m_AvailableQuests[num].m_QuestList.Count - 1; num2 >= 0; num2--)
				{
					GlobalStart.TimedNetworkService();
					for (int num3 = m_AvailableQuests[num].m_QuestList[num2].m_Quests.Count - 1; num3 >= 0; num3--)
					{
						GlobalStart.TimedNetworkService();
						if (m_AvailableQuests[num].m_QuestList[num2].m_Quests[num3] == null)
						{
							m_AvailableQuests[num].m_QuestList[num2].m_Quests.RemoveAt(num3);
						}
						else
						{
							TextAsset assetToSwap = m_AvailableQuests[num].m_QuestList[num2].m_Quests[num3];
							if (AssetManager.instance.SwapAssetForOneInBundle("objectivefiles", ref assetToSwap))
							{
								m_AvailableQuests[num].m_QuestList[num2].m_Quests[num3] = assetToSwap;
							}
						}
					}
					if (m_AvailableQuests[num].m_QuestList[num2].m_Quests.Count == 0)
					{
						m_AvailableQuests[num].m_QuestList.RemoveAt(num2);
					}
				}
				if (m_AvailableQuests[num].m_QuestList.Count == 0)
				{
					m_AvailableQuests.RemoveAt(num);
				}
			}
		}
		CacheQuestList(m_AvailableQuests, bUpdateNetworkService: true);
		CacheQuestList(m_SpecificQuests, bUpdateNetworkService: true);
		CacheQuestList(m_RobinsonDefaultQuest, bUpdateNetworkService: true);
		CacheQuestList(m_RobinsonQuests, bUpdateNetworkService: true);
		GC.Collect();
		GlobalStart.TimedNetworkService();
	}

	private void CacheQuestList(QuestList quest, bool bUpdateNetworkService = false)
	{
		if (quest != null)
		{
			int i = 0;
			for (int count = quest.m_Quests.Count; i < count; i++)
			{
				CacheQuestTextData(quest.m_Quests[i], bUpdateNetworkService);
			}
		}
	}

	private void CacheQuestList(QuestMapping[] quests, bool bUpdateNetworkService = false)
	{
		if (quests == null)
		{
			return;
		}
		int i = 0;
		for (int num = quests.Length; i < num; i++)
		{
			if (quests[i] != null && quests[i].m_Quest != null)
			{
				int j = 0;
				for (int count = quests[i].m_Quest.m_Quests.Count; j < count; j++)
				{
					CacheQuestTextData(quests[i].m_Quest.m_Quests[j], bUpdateNetworkService);
				}
			}
		}
	}

	private void CacheQuestList(List<QuestMapping> quests, bool bUpdateNetworkService = false)
	{
		int i = 0;
		for (int count = quests.Count; i < count; i++)
		{
			if (quests[i] != null && quests[i].m_Quest != null)
			{
				int j = 0;
				for (int count2 = quests[i].m_Quest.m_Quests.Count; j < count2; j++)
				{
					CacheQuestTextData(quests[i].m_Quest.m_Quests[j], bUpdateNetworkService);
				}
			}
		}
	}

	private void CacheQuestList(List<QuestType> quests, bool bUpdateNetworkService = false)
	{
		int i = 0;
		for (int count = quests.Count; i < count; i++)
		{
			if (quests[i] == null)
			{
				continue;
			}
			int j = 0;
			for (int count2 = quests[i].m_QuestList.Count; j < count2; j++)
			{
				QuestList questList = quests[i].m_QuestList[j];
				int k = 0;
				for (int count3 = questList.m_Quests.Count; k < count3; k++)
				{
					if (bUpdateNetworkService)
					{
						GlobalStart.TimedNetworkService();
					}
					CacheQuestTextData(questList.m_Quests[k], bUpdateNetworkService);
				}
			}
		}
	}

	private void CacheQuestTextData(TextAsset objectiveData, bool bUpdateNetworkService = false)
	{
		if (!m_CachedObjectiveTrees.ContainsKey(objectiveData))
		{
			List<ObjectiveTree> list = new List<ObjectiveTree>();
			if (ObjectiveManager.GetInstance().LoadObjectiveTreeData(objectiveData, ref list, bUpdateNetworkService) && list.Count == 1)
			{
				m_CachedObjectiveTrees.Add(objectiveData, list);
			}
		}
	}

	private List<ObjectiveTree> GetCachedObjectiveTreeData(TextAsset objectiveData)
	{
		if (objectiveData != null)
		{
			if (m_CachedObjectiveTrees.ContainsKey(objectiveData))
			{
				List<ObjectiveTree> result = m_CachedObjectiveTrees[objectiveData];
				m_CachedObjectiveTrees.Remove(objectiveData);
				return result;
			}
			CacheQuestList(m_AvailableQuests);
			if (m_CachedObjectiveTrees.ContainsKey(objectiveData))
			{
				List<ObjectiveTree> result2 = m_CachedObjectiveTrees[objectiveData];
				m_CachedObjectiveTrees.Remove(objectiveData);
				return result2;
			}
		}
		return null;
	}

	public void RemoveQuestsForPlayer(Player player, bool bSpecificQuestsOnly = false)
	{
		for (int i = 0; i < 24; i++)
		{
			if (m_AcceptedQuests[i] == null || !(m_AcceptedQuests[i].m_PlayerDoingQuest == player))
			{
				continue;
			}
			bool flag = false;
			Character character = null;
			if (m_AcceptedQuests[i].GetTypeIndex() == -2 || m_AcceptedQuests[i].GetTypeIndex() == -3)
			{
				flag = true;
				character = m_AcceptedQuests[i].m_QuestGiver;
			}
			else if (bSpecificQuestsOnly)
			{
				continue;
			}
			m_NetView.RPC("RPC_ALL_FreeAcceptedQuest", NetTargets.All, m_AcceptedQuests[i].QuestGiverID);
			if (flag && character != null)
			{
				GetInstance().CreateSpecificCharacterQuest(character, bForce: true);
			}
			List<Item> list = new List<Item>();
			foreach (KeyValuePair<int, int> questItem in m_QuestItems)
			{
				if (questItem.Value == player.m_NetView.viewID)
				{
					Item itemFromUsedListByNetView = ItemManager.GetInstance().GetItemFromUsedListByNetView(questItem.Key);
					if (itemFromUsedListByNetView != null)
					{
						list.Add(itemFromUsedListByNetView);
					}
				}
			}
			for (int num = list.Count - 1; num >= 0; num--)
			{
				PhotonView photonView = PhotonView.Find(list[num].m_ContainerViewID);
				if (photonView != null)
				{
					ItemContainer component = photonView.GetComponent<ItemContainer>();
					if (component != null)
					{
						component.RemoveItemRPC(list[num], releaseToManager: true);
					}
				}
			}
		}
	}

	public void OnBecameMasterClient()
	{
		List<Player> allPlayers = Player.GetAllPlayers();
		int count = allPlayers.Count;
		for (int i = 0; i < count; i++)
		{
			Player player = allPlayers[i];
			if (null != player && player.m_Gamer == null)
			{
				RemoveQuestsForPlayer(player);
			}
		}
	}

	public void OnGamerDeleteImminent(Gamer gamer)
	{
		if (T17NetManager.IsMasterClient && gamer != null && gamer.m_PlayerObject != null)
		{
			RemoveQuestsForPlayer(gamer.m_PlayerObject);
		}
	}

	private void Update()
	{
		if ((!T17NetManager.OfflineMode && (!T17NetManager.NetOnlineMode || !T17NetManager.IsMasterClient)) || !m_bStartGivingQuests || m_AvailableQuests.Count <= 0)
		{
			return;
		}
		m_ElapsedGivingQuestTime += UpdateManager.deltaTime;
		if (m_ElapsedGivingQuestTime > 3f)
		{
			m_ElapsedGivingQuestTime = 0f;
			if (m_OfferingQuestGivers.Count < m_MaxQuestGivers)
			{
				int availableInmateForQuest = GetAvailableInmateForQuest();
				if (availableInmateForQuest != -1)
				{
					int num = UnityEngine.Random.Range(0, 100);
					int num2 = 0;
					int num3 = 0;
					for (int i = 0; i < m_AvailableQuests.Count; i++)
					{
						num3 += m_AvailableQuests[i].m_AvailablePercentage;
						if (m_AvailableQuests[i].m_QuestList.Count <= 0)
						{
							continue;
						}
						if (num >= num2 && num < num3)
						{
							if (m_NetView != null)
							{
								int num4 = UnityEngine.Random.Range(0, m_AvailableQuests[i].m_QuestList.Count);
								m_NetView.RPC("RPC_CreateQuestGiver", NetTargets.All, availableInmateForQuest, i, num4);
							}
							break;
						}
						num2 = num3;
					}
				}
			}
		}
		for (int num5 = m_OfferingQuestGivers.Count - 1; num5 >= 0; num5--)
		{
			if (m_OfferingQuestGivers[num5].IsExpired)
			{
				m_NetView.RPC("RPC_ALL_RemoveQuestGiver", NetTargets.All, m_OfferingQuestGivers[num5].QuestGiverID, -1);
			}
		}
	}

	[PunRPC]
	private void RPC_CreateSpecificQuestGiver(int poolIndex, int questIndex, PhotonMessageInfo info)
	{
		if (poolIndex < 0 || poolIndex >= 24 || m_QuestGiverPool[poolIndex] == null || m_QuestGiverPool[poolIndex].m_AvailableQuests != null || !(m_QuestGiverPool[poolIndex].m_PlayerDoingQuest == null))
		{
			return;
		}
		QuestGiver questGiver = m_QuestGiverPool[poolIndex];
		if (questIndex >= m_SpecificQuests.Count)
		{
			if (questGiver.m_QuestGiver.m_bIsRobinsonCharacter && T17NetManager.IsConnectedOnline() && !T17NetManager.IsMasterClient)
			{
				SetUpRobinsonQuests(questGiver.m_QuestGiver);
			}
			if (questIndex >= m_SpecificQuests.Count)
			{
				return;
			}
		}
		questGiver.m_QuestGiver.SetHasQuest(value: true);
		questGiver.m_QuestGiver.m_IconHandler.DisplayIcon(CharacterIconHandler.IconType.Quest);
		questGiver.m_QuestGiver.SetPinImage(GetMapQuestSprite(questGiver.m_QuestGiver), PinManager.Pin.PinFilterType.Favours);
		questGiver.SetQuest(m_SpecificQuests[questIndex].m_Quest, -2, questIndex);
		questGiver.m_NetView = m_NetView;
		questGiver.m_ExpireCallbackTimer = null;
		m_OfferingQuestGivers.Add(questGiver);
	}

	[PunRPC]
	private void RPC_CreateQuestGiver(int poolIndex, int questTypeIndex, int randomIndex, PhotonMessageInfo info)
	{
		if (poolIndex >= 0 && poolIndex < 24 && m_QuestGiverPool[poolIndex] != null && m_QuestGiverPool[poolIndex].m_AvailableQuests == null && m_QuestGiverPool[poolIndex].m_PlayerDoingQuest == null)
		{
			QuestGiver questGiver = m_QuestGiverPool[poolIndex];
			questGiver.m_QuestGiver.SetHasQuest(value: true);
			questGiver.m_QuestGiver.m_IconHandler.DisplayIcon(CharacterIconHandler.IconType.Quest);
			questGiver.m_QuestGiver.SetPinImage(GetMapQuestSprite(questGiver.m_QuestGiver), PinManager.Pin.PinFilterType.Favours);
			questGiver.SetQuest(m_AvailableQuests[questTypeIndex].m_QuestList[randomIndex], questTypeIndex, randomIndex);
			questGiver.m_NetView = m_NetView;
			if (T17NetManager.IsMasterClient)
			{
				questGiver.m_ExpireCallbackTimer = RoutineManager.GetInstance().CreateCallbackTimer(m_AvailableQuests[questTypeIndex].m_QuestRefreshTimeInHours / 24, m_AvailableQuests[questTypeIndex].m_QuestRefreshTimeInHours % 24, 0, questGiver.OnQuestExpired, relativeToStart: false);
			}
			m_OfferingQuestGivers.Add(questGiver);
		}
	}

	[PunRPC]
	private void RPC_ALL_RemoveQuestGiver(int questGiverID, int playerViewID, PhotonMessageInfo info)
	{
		if (questGiverID == -1)
		{
			return;
		}
		for (int num = m_OfferingQuestGivers.Count - 1; num >= 0; num--)
		{
			if (m_OfferingQuestGivers[num].QuestGiverID == questGiverID)
			{
				m_OfferingQuestGivers[num].m_QuestGiver.SetHasQuest(value: false);
				m_OfferingQuestGivers[num].ShowQuestAvailable(available: false);
				if (playerViewID != -1 && m_OfferingQuestGivers[num] != null && m_AcceptedQuests[questGiverID] == null)
				{
					m_AcceptedQuests[questGiverID] = m_OfferingQuestGivers[num];
					if (m_AcceptedQuests[questGiverID].m_PlayerDoingQuest == null)
					{
						m_AcceptedQuests[questGiverID].m_PlayerDoingQuest = PhotonView.Find(playerViewID).GetComponent<Player>();
					}
				}
				m_OfferingQuestGivers.RemoveAt(num);
				break;
			}
		}
		if (playerViewID == -1)
		{
			m_QuestGiverPool[questGiverID].HideMenu();
			m_QuestGiverPool[questGiverID].ResetInfo();
		}
	}

	[PunRPC]
	private void RPC_ALL_FreeAcceptedQuest(int questGiverID, PhotonMessageInfo info)
	{
		if (questGiverID != -1 && m_AcceptedQuests[questGiverID] != null && m_AcceptedQuests[questGiverID].QuestGiverID == questGiverID)
		{
			m_QuestGiverPool[questGiverID].ResetInfo();
			m_AcceptedQuests[questGiverID] = null;
		}
	}

	public void PauseQuestOfferTimer(Character character, bool paused)
	{
		int num = -1;
		for (int i = 0; i < m_OfferingQuestGivers.Count; i++)
		{
			if (m_OfferingQuestGivers[i].m_QuestGiver == character)
			{
				num = i;
				break;
			}
		}
		if (num >= 0 && m_NetView != null)
		{
			m_NetView.RPC("MASTER_ONLY_PauseQuestOfferTimer", NetTargets.MasterClient, character.m_NetView.viewID, paused);
		}
	}

	[PunRPC]
	private void MASTER_ONLY_PauseQuestOfferTimer(int giverIndex, bool paused)
	{
		if (giverIndex >= 0 && giverIndex < m_OfferingQuestGivers.Count)
		{
			QuestGiver questGiver = m_OfferingQuestGivers[giverIndex];
			if (questGiver != null)
			{
				PauseCallbackTimer(questGiver.m_ExpireCallbackTimer, paused);
			}
		}
	}

	private void PauseCallbackTimer(RoutineManager.CallbackInGameTimer callbackTimer, bool pauseState)
	{
		if (callbackTimer != null)
		{
			if (pauseState)
			{
				callbackTimer.Pause();
			}
			else
			{
				callbackTimer.Resume();
			}
		}
	}

	private int GetNumActiveQuests()
	{
		int num = 0;
		for (int i = 0; i < 24; i++)
		{
			if (m_AcceptedQuests[i] != null)
			{
				num++;
			}
		}
		return num;
	}

	private void StartHandingOutQuests()
	{
		m_bStartGivingQuests = true;
		if (T17NetManager.OfflineMode || (T17NetManager.NetOnlineMode && T17NetManager.IsMasterClient))
		{
			if (!PrisonSnapshotIO.IsThereSaveData())
			{
				AutoCreateSpecificQuests();
			}
			CreateRobinsonSpecificQuestGiver();
		}
	}

	private void AutoCreateSpecificQuests()
	{
		for (int i = 0; i < m_SpecificQuests.Count; i++)
		{
			if (m_SpecificQuests[i] != null && m_SpecificQuests[i].m_QuestGiver != null && m_SpecificQuests[i].m_Quest != null && m_SpecificQuests[i].m_AutoCreate)
			{
				int num = FindQuestableInmate(m_SpecificQuests[i].m_QuestGiver);
				if (num != -1 && m_QuestGiverPool[num].m_AvailableQuests == null && m_QuestGiverPool[num].m_PlayerDoingQuest == null && !m_QuestGiverPool[num].m_QuestGiver.m_bIsVendor)
				{
					m_NetView.RPC("RPC_CreateSpecificQuestGiver", NetTargets.All, num, i);
				}
			}
		}
	}

	public void CreateSpecificCharacterQuest(Character character, bool bForce = false)
	{
		if (character != null)
		{
			m_NetView.RPC("RPC_CreateSpecificCharacterQuest", NetTargets.MasterClient, character.m_NetView.viewID, bForce);
		}
	}

	[PunRPC]
	public void RPC_CreateSpecificCharacterQuest(int characterID, bool bForce)
	{
		Character character = T17NetView.Find<Character>(characterID);
		if (!(character != null))
		{
			return;
		}
		int num = -1;
		for (int i = 0; i < m_SpecificQuests.Count; i++)
		{
			if (m_SpecificQuests[i] != null && m_SpecificQuests[i].m_QuestGiver == character && m_SpecificQuests[i].m_Quest != null && (bForce || !m_SpecificQuests[i].m_AutoCreate))
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			int num2 = FindQuestableInmate(character);
			if (num2 != -1 && m_QuestGiverPool[num2].m_AvailableQuests == null && m_QuestGiverPool[num2].m_PlayerDoingQuest == null && !m_QuestGiverPool[num2].m_QuestGiver.m_bIsVendor)
			{
				m_NetView.RPC("RPC_CreateSpecificQuestGiver", NetTargets.All, num2, num);
			}
		}
	}

	public bool AddQuestItem(Item item, int questOwner, ref int groupID)
	{
		int viewID = item.m_NetView.viewID;
		if (m_QuestItems.ContainsKey(viewID))
		{
			return false;
		}
		m_QuestItems.Add(viewID, questOwner);
		PlayerQuestItemGroups value = null;
		if (!m_PlayerQuestItemGroups.TryGetValue(questOwner, out value))
		{
			value = new PlayerQuestItemGroups();
			m_PlayerQuestItemGroups.Add(questOwner, value);
		}
		List<int> value2 = null;
		if (!value.groups.TryGetValue(groupID, out value2))
		{
			if (groupID >= 0)
			{
				if (groupID > value.maxGroupIndex)
				{
					value.maxGroupIndex = groupID;
				}
			}
			else
			{
				groupID = -1;
				for (int i = 0; i <= value.maxGroupIndex; i++)
				{
					if (!value.groups.ContainsKey(i))
					{
						groupID = i;
						break;
					}
				}
				if (groupID < 0)
				{
					groupID = ++value.maxGroupIndex;
				}
			}
			value2 = new List<int>();
			value.groups.Add(groupID, value2);
		}
		if (!value2.Contains(viewID))
		{
			value2.Add(viewID);
		}
		return true;
	}

	public void RemoveQuestItem(Item item)
	{
		int viewID = item.m_NetView.viewID;
		if (m_QuestItems.ContainsKey(viewID))
		{
			m_QuestItems.Remove(viewID);
		}
		foreach (PlayerQuestItemGroups value2 in m_PlayerQuestItemGroups.Values)
		{
			for (int i = 0; i <= value2.maxGroupIndex; i++)
			{
				List<int> value = null;
				if (value2.groups.TryGetValue(i, out value))
				{
					value.Remove(viewID);
					if (value.Count <= 0)
					{
						value2.groups.Remove(i);
					}
				}
			}
		}
	}

	public bool DoesPlayerOwnQuestItem(int itemViewID, int playerViewID)
	{
		int value = 0;
		return m_QuestItems.TryGetValue(itemViewID, out value) && value == playerViewID;
	}

	public List<int> GetItemIDsForGroup(int ownerID, int groupID)
	{
		PlayerQuestItemGroups value = null;
		if (m_PlayerQuestItemGroups.TryGetValue(ownerID, out value) && value.groups.ContainsKey(groupID))
		{
			return value.groups[groupID];
		}
		return null;
	}

	public bool IsCharacterSafeToUse(Character character)
	{
		for (int i = 0; i < 24; i++)
		{
			if (m_AcceptedQuests[i] != null && m_AcceptedQuests[i].m_QuestGiver == character)
			{
				return false;
			}
		}
		return !character.m_bHasQuestAvailable || !character.m_bIsRobinsonCharacter;
	}

	public Character GetRandomInmate(Character questGiver)
	{
		List<Character> list = new List<Character>();
		for (int i = 0; i < m_InmateCharacters.Count; i++)
		{
			if (m_InmateCharacters[i] != questGiver && !m_InmateCharacters[i].m_bHasQuestAvailable && !IsSpecificQuestGiver(m_InmateCharacters[i]))
			{
				list.Add(m_InmateCharacters[i]);
			}
		}
		int index = UnityEngine.Random.Range(0, list.Count);
		return list[index];
	}

	private bool IsSpecificQuestGiver(Character character)
	{
		if (character != null)
		{
			for (int i = 0; i < m_SpecificQuests.Count; i++)
			{
				if (m_SpecificQuests[i] != null && m_SpecificQuests[i].m_QuestGiver == character)
				{
					return true;
				}
			}
		}
		return false;
	}

	private int FindQuestableInmate(Character character)
	{
		int result = -1;
		if (character != null)
		{
			for (int i = 0; i < 24; i++)
			{
				if (m_QuestGiverPool[i] != null && m_QuestGiverPool[i].m_QuestGiver == character)
				{
					result = m_QuestGiverPool[i].QuestGiverID;
					break;
				}
			}
		}
		return result;
	}

	public int GetAvailableInmateForQuest()
	{
		List<QuestGiver> list = new List<QuestGiver>();
		for (int i = 0; i < 24; i++)
		{
			if (m_QuestGiverPool[i] != null && m_QuestGiverPool[i].m_AvailableQuests == null && m_QuestGiverPool[i].m_PlayerDoingQuest == null && !m_QuestGiverPool[i].m_QuestGiver.m_bIsVendor && !IsSpecificQuestGiver(m_QuestGiverPool[i].m_QuestGiver))
			{
				list.Add(m_QuestGiverPool[i]);
			}
		}
		if (list.Count > 0)
		{
			int index = UnityEngine.Random.Range(0, list.Count);
			return list[index].QuestGiverID;
		}
		return -1;
	}

	public Character GetRandomGuard()
	{
		int index = UnityEngine.Random.Range(0, m_GuardCharacters.Count);
		return m_GuardCharacters[index];
	}

	public List<Character> GetGuards()
	{
		return m_GuardCharacters;
	}

	public void RegisterQuestableGuard(Character guard)
	{
		m_GuardCharacters.Add(guard);
	}

	public void RegisterQuestableInmate(Character inmate)
	{
		if (inmate.m_CharacterStats.m_bIsPlayer)
		{
			return;
		}
		if (T17NetManager.OfflineMode || (T17NetManager.NetOnlineMode && T17NetManager.IsMasterClient))
		{
			for (int i = 0; i < 24; i++)
			{
				if (m_QuestGiverPool[i] == null)
				{
					m_QuestGiverPool[i] = new QuestGiver(i);
					m_QuestGiverPool[i].m_QuestGiver = inmate;
					m_NetView.RPC("RPC_OTHERS_SetupQuestGiverPool", NetTargets.Others, i, inmate.m_NetView.viewID);
					break;
				}
			}
		}
		m_InmateCharacters.Add(inmate);
	}

	public void RegisterSpecificQuestableCharacter(Character character)
	{
		if (!(character != null) || (character.m_CharacterRole != CharacterRole.Generic && character.m_CharacterRole != CharacterRole.Dolphin && character.m_CharacterRole != CharacterRole.SpecificQuestGiver))
		{
			return;
		}
		for (int i = 0; i < m_SpecificQuests.Count; i++)
		{
			if (m_SpecificQuests[i] != null && m_SpecificQuests[i].m_QuestGiver == character)
			{
				RegisterQuestableInmate(character);
				break;
			}
		}
	}

	[PunRPC]
	private void RPC_OTHERS_SetupQuestGiverPool(int poolIndex, int inmateNetViewID)
	{
		if (inmateNetViewID != -1 && poolIndex >= 0 && poolIndex < 24)
		{
			m_QuestGiverPool[poolIndex] = new QuestGiver(poolIndex);
			Character component = PhotonView.Find(inmateNetViewID).GetComponent<Character>();
			m_QuestGiverPool[poolIndex].m_QuestGiver = component;
		}
	}

	public void InteractWithQuestGiver(Character questGiver, Player interactingPlayer, FavourMenu favourMenu)
	{
		if (!(questGiver != null) || !(interactingPlayer != null))
		{
			return;
		}
		for (int i = 0; i < 24; i++)
		{
			if (m_AcceptedQuests[i] != null && m_AcceptedQuests[i].m_QuestGiver == questGiver)
			{
				m_AcceptedQuests[i].PrepareMenu(favourMenu, interactingPlayer);
				return;
			}
		}
		for (int j = 0; j < m_OfferingQuestGivers.Count; j++)
		{
			if (m_OfferingQuestGivers[j].m_QuestGiver == questGiver)
			{
				m_OfferingQuestGivers[j].PrepareMenu(favourMenu, interactingPlayer);
				break;
			}
		}
	}

	public Sprite GetMapQuestSprite(Character character)
	{
		if (character.m_bIsRobinsonCharacter && m_RobinsonMapQuestIcon != null)
		{
			return m_RobinsonMapQuestIcon;
		}
		return m_MapQuestIcon;
	}

	public void ResetLoadState()
	{
		if (T17NetManager.IsMasterClient)
		{
			m_LoadState = LOADSTATE.Finished_OK;
			m_LoadError = string.Empty;
		}
		else
		{
			m_LoadState = LOADSTATE.NotStarted;
			m_LoadError = string.Empty;
		}
	}

	public LOADSTATE GetLoadState()
	{
		return m_LoadState;
	}

	public string GetLoadError()
	{
		return m_LoadError;
	}

	public void SendLoadDataToClientRPC(PhotonPlayer player)
	{
		if (!T17NetManager.IsMasterClient || player.IsLocal)
		{
			return;
		}
		if (m_LoadState == LOADSTATE.Finished_OK)
		{
			Serialize(isForLocalSave: false);
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using MemoryStream memoryStream = new MemoryStream();
			binaryFormatter.Serialize(memoryStream, m_NetQMSaveData);
			m_NetView.RPC("RPC_RequestStateResponce_Yes_QuestManager", player, memoryStream.ToArray());
			return;
		}
		m_NetView.RPC("RPC_RequestStateResponce_No_QuestManager", player);
	}

	[PunRPC]
	private void RPC_RequestStateResponce_Yes_QuestManager(byte[] questData, PhotonMessageInfo info)
	{
		string error = string.Empty;
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using (MemoryStream serializationStream = new MemoryStream(questData))
		{
			m_NetQMSaveData = (NetQuestManagerSaveData)binaryFormatter.Deserialize(serializationStream);
		}
		if (DeserializeBinary(ref m_NetQMSaveData, ref error))
		{
			m_LoadState = LOADSTATE.Finished_OK;
			return;
		}
		m_LoadState = LOADSTATE.Finished_Error;
		m_LoadError += error;
	}

	[PunRPC]
	private void RPC_RequestStateResponce_No_QuestManager(PhotonMessageInfo info)
	{
		m_LoadError = "QuestManager RPC_RequestStateResponce_No - Error on MasterClient";
		m_LoadState = LOADSTATE.Finished_Error;
	}

	private void Serialize(bool isForLocalSave)
	{
		GlobalStart instance = GlobalStart.GetInstance();
		if (!(instance != null) || !instance.IsWithinLevel())
		{
			return;
		}
		if (m_NetQMSaveData == null)
		{
			m_NetQMSaveData = new NetQuestManagerSaveData();
		}
		if (m_NetQMSaveData == null)
		{
			return;
		}
		if (m_NetQMSaveData.m_QuestGiverPool == null)
		{
			m_NetQMSaveData.m_QuestGiverPool = new List<ulong>();
		}
		if (m_NetQMSaveData.m_QuestGiverPool == null)
		{
			return;
		}
		m_NetQMSaveData.m_QuestGiverPool.Clear();
		for (int i = 0; i < 24; i++)
		{
			if (m_QuestGiverPool[i] != null)
			{
				m_NetQMSaveData.m_QuestGiverPool.Add(m_QuestGiverPool[i].Serialize());
			}
		}
		int num = -1;
		if (isForLocalSave)
		{
			Player playerObject = Gamer.GetPrimaryGamer().m_PlayerObject;
			if (playerObject != null)
			{
				num = playerObject.m_NetView.viewID;
			}
		}
		int viewID = Gamer.GetPrimaryGamer().m_PlayerObject.m_NetView.viewID;
		string text = string.Empty;
		uint num2 = 0u;
		foreach (KeyValuePair<int, int> questItem in m_QuestItems)
		{
			if (num == -1 || questItem.Value == num)
			{
				num2 = (uint)questItem.Key;
				text = text + (num2 | (uint)(questItem.Value << 12)) + ",";
			}
		}
		m_NetQMSaveData.m_QuestItems = text;
	}

	public string CreateSnapshot()
	{
		Serialize(isForLocalSave: true);
		return JsonUtility.ToJson(m_NetQMSaveData);
	}

	public void StartedFromSnapshot()
	{
	}

	public string GetSerializationData()
	{
		return m_SaveData.GetSaveData();
	}

	public bool DeserializeBinary(ref NetQuestManagerSaveData data, ref string error)
	{
		StartCoroutine(WaitForObjectiveManagerSnapshot(data));
		return true;
	}

	private IEnumerator WaitForObjectiveManagerSnapshot(NetQuestManagerSaveData data)
	{
		while (!ObjectiveManager.SnapshotIsReady)
		{
			yield return new WaitForEndOfFrame();
		}
		string error = string.Empty;
		Debug.Log(" ******   WaitForObjectiveManagerSnapshot");
		for (int i = 0; i < data.m_QuestGiverPool.Count; i++)
		{
			ulong num = data.m_QuestGiverPool[i];
			Debug.Log("   **     Quest Giver  " + data.m_QuestGiverPool[i]);
			if (num == 0)
			{
				continue;
			}
			BitField bitField = new BitField(num);
			if (bitField == null)
			{
				error = "QuestManager out of memory allocating BitField";
				break;
			}
			int uInt = (int)bitField.GetUInt(12);
			int uInt2 = (int)bitField.GetUInt(12);
			int uInt3 = (int)bitField.GetUInt(5);
			uint uInt4 = bitField.GetUInt(7);
			int @int = bitField.GetInt(3);
			int int2 = bitField.GetInt(5);
			int int3 = bitField.GetInt(7);
			int uInt5 = (int)bitField.GetUInt(6);
			uint uInt6 = bitField.GetUInt(4);
			uInt6 <<= 7;
			int3 |= (int)uInt6;
			Debug.Log(" *******  Read back  the stageIndex " + uInt5);
			Character character = T17NetView.Find<Character>(uInt);
			if (null == character)
			{
			}
			m_QuestGiverPool[uInt3] = new QuestGiver(uInt3);
			if (m_QuestGiverPool[uInt3] == null)
			{
				error = "QuestManager out of memory allocating QuestGiver";
				break;
			}
			m_QuestGiverPool[uInt3].m_QuestGiver = character;
			if (uInt2 == 4095)
			{
				if (@int != -1 && int2 != -1)
				{
					RoutineManager instance = RoutineManager.GetInstance();
					PinManager instance2 = PinManager.GetInstance();
					QuestGiver questGiver = m_QuestGiverPool[uInt3];
					Character questGiver2 = questGiver.m_QuestGiver;
					if (questGiver == null || !(null != questGiver2) || !(null != questGiver2.m_IconHandler) || !(null != instance2) || !(null != instance))
					{
						error = "QuestManager handled unexpected null references: qGiver" + (null != questGiver) + " qGiverCharacter: " + (null != questGiver2) + " PinManager: " + (null != instance2) + " RoutineManager: " + (null != instance);
						break;
					}
					Debug.Log(" ******   Quest Giver that is not  accepted ??   " + questGiver2.name);
					questGiver2.SetHasQuest(value: true);
					questGiver.m_QuestGiver.m_IconHandler.DisplayIcon(CharacterIconHandler.IconType.Quest);
					questGiver2.SetPinImage(GetMapQuestSprite(questGiver2), PinManager.Pin.PinFilterType.Favours);
					if (!DeSerializeQuestDataIntoQuestGiver(@int, int2, uInt5, questGiver, out error, bSetAccepted: false))
					{
						break;
					}
					questGiver.m_NetView = m_NetView;
					if (@int != -2 && T17NetManager.IsMasterClient)
					{
						questGiver.m_ExpireCallbackTimer = instance.CreateCallbackTimer((int)(uInt4 / 24), (int)(uInt4 % 24), 0, questGiver.OnQuestExpired, relativeToStart: false);
					}
					m_OfferingQuestGivers.Add(questGiver);
				}
				continue;
			}
			Debug.Log(" ******   Quest Giver that is accepted ??   ");
			Player player = T17NetView.Find<Player>(uInt2);
			if (player != null && uInt3 >= 0 && uInt3 < m_QuestGiverPool.Length)
			{
				QuestGiver questGiver3 = m_QuestGiverPool[uInt3];
				Debug.Log(" ******   Quest Giver that is accepted   qGiver " + questGiver3.m_QuestGiver.name);
				if (questGiver3 != null)
				{
					questGiver3.m_PlayerDoingQuest = player;
					if (!DeSerializeQuestDataIntoQuestGiver(@int, int2, uInt5, questGiver3, out error, bSetAccepted: true))
					{
						break;
					}
					questGiver3.m_NetView = m_NetView;
					if (uInt3 >= 0 && uInt3 < m_AcceptedQuests.Length && m_AcceptedQuests[uInt3] == null)
					{
						m_AcceptedQuests[uInt3] = m_QuestGiverPool[uInt3];
					}
					questGiver3.OnQuestDeserialized(int3);
					if (int3 == -1 && !questGiver3.IsAccepted)
					{
						questGiver3.m_QuestGiver.SetHasQuest(value: true);
						questGiver3.m_QuestGiver.m_IconHandler.DisplayIcon(CharacterIconHandler.IconType.Quest);
						questGiver3.m_QuestGiver.SetPinImage(GetMapQuestSprite(questGiver3.m_QuestGiver), PinManager.Pin.PinFilterType.Favours);
					}
				}
			}
			else
			{
				Debug.Log(" ******   Quest Giver that is accepted   ERROR ");
			}
		}
		DeSerializeQuestItems(data.m_QuestItems);
		if (!string.IsNullOrEmpty(error))
		{
		}
		Debug.Log(" ******   WaitForObjectiveManagerSnapshot");
	}

	private void DeSerializeQuestItems(string items)
	{
		if (m_QuestItems != null)
		{
			m_QuestItems.Clear();
		}
		else
		{
			m_QuestItems = new Dictionary<int, int>();
		}
		if (m_QuestItems == null || string.IsNullOrEmpty(items))
		{
			return;
		}
		string[] array = items.Split(',');
		if (array == null)
		{
			return;
		}
		int num = array.Length;
		for (int i = 0; i < num; i++)
		{
			if (string.IsNullOrEmpty(array[i]))
			{
				continue;
			}
			uint result = 0u;
			if (uint.TryParse(array[i], out result))
			{
				int num2 = (int)(result & 0xFFF);
				int value = (int)(result >> 12);
				if (num2 != 0)
				{
					m_QuestItems.Add(num2, value);
				}
			}
		}
	}

	private bool DeSerializeQuestDataIntoQuestGiver(int questTypeIndex, int questListIndex, int stageIndex, QuestGiver qGiver, out string error, bool bSetAccepted)
	{
		error = string.Empty;
		if (questTypeIndex == -2 || questTypeIndex == -3)
		{
			if (questTypeIndex == -3 || qGiver.m_QuestGiver.m_bIsRobinsonCharacter)
			{
				if (!qGiver.m_QuestGiver.m_bIsRobinsonCharacter)
				{
					error = "A character other than robinson had a robinson quest!";
					return false;
				}
				SetUpRobinsonQuests(qGiver.m_QuestGiver);
				switch (questTypeIndex)
				{
				case -2:
					qGiver.SetQuest(m_RobinsonDefaultQuest, -2, 0);
					break;
				case -3:
					if (questListIndex >= 0 && questListIndex < m_RobinsonQuests.Length)
					{
						List<ObjectiveTree> activeTrees = new List<ObjectiveTree>();
						ObjectiveManager.GetInstance().GetActiveTrees(qGiver.m_PlayerDoingQuest, out activeTrees);
						qGiver.SetQuest(m_RobinsonQuests[questListIndex].m_Quest, -3, questListIndex);
						qGiver.m_AvailableQuests.GoToSpecificQuest(stageIndex, cacheQuest: true);
						int i = 0;
						for (int count = activeTrees.Count; i < count; i++)
						{
							ObjectiveGoal currentGoal = activeTrees[i].MainBranch.GetCurrentGoal();
							if (currentGoal != null && currentGoal.m_Objective != null)
							{
								if (bSetAccepted)
								{
									qGiver.SetAccepted();
								}
								if (currentGoal.m_Objective is InteractObjective interactObjective)
								{
									qGiver.m_QuestGiverMessage = interactObjective.m_QuestGiverMessage;
								}
								break;
							}
						}
						return true;
					}
					error = "QuestManager specific questListIndex out of bounds questListIndex: " + questListIndex + " m_SpecificQuestsSize: " + m_SpecificQuests.Count;
					return false;
				}
				return true;
			}
			if (questListIndex >= 0 && questListIndex < m_SpecificQuests.Count)
			{
				qGiver.SetQuest(m_SpecificQuests[questListIndex].m_Quest, -2, questListIndex);
				qGiver.m_AvailableQuests.GoToSpecificQuest(stageIndex, cacheQuest: true);
				return true;
			}
			error = "QuestManager specific questListIndex out of bounds questListIndex: " + questListIndex + " m_SpecificQuestsSize: " + m_SpecificQuests.Count;
			return false;
		}
		if (questTypeIndex >= 0 && questTypeIndex < m_AvailableQuests.Count)
		{
			QuestType questType = m_AvailableQuests[questTypeIndex];
			if (questType != null && questListIndex >= 0 && questListIndex < questType.m_QuestList.Count)
			{
				qGiver.SetQuest(questType.m_QuestList[questListIndex], questTypeIndex, questListIndex);
				qGiver.m_AvailableQuests.GoToSpecificQuest(stageIndex, cacheQuest: true);
				return true;
			}
			error = "QuestManager questListIndex out of bounds questListIndex: " + questListIndex + " AvailableQuestsSize: " + questType.m_QuestList.Count;
			return false;
		}
		error = "QuestManager questTypeIndex out of bounds questTypeIndex: " + questTypeIndex + " AvailableQuestsSize: " + m_AvailableQuests.Count;
		return false;
	}

	public bool Deserialize(string data, ref string error)
	{
		if (data == null)
		{
			return true;
		}
		m_NetQMSaveData = JsonUtility.FromJson<NetQuestManagerSaveData>(data);
		if (m_NetQMSaveData == null || m_NetQMSaveData.m_QuestGiverPool == null)
		{
			return true;
		}
		return DeserializeBinary(ref m_NetQMSaveData, ref error);
	}

	public QuestGiver GetQuestGiver(Character character)
	{
		for (int i = 0; i < m_QuestGiverPool.Length; i++)
		{
			if (m_QuestGiverPool[i].m_QuestGiver == character)
			{
				return m_QuestGiverPool[i];
			}
		}
		return null;
	}

	public void CleanUpQuestGiver(Character character)
	{
		if (!(character != null))
		{
			return;
		}
		if (m_QuestGiverPool != null)
		{
			for (int num = m_QuestGiverPool.Length - 1; num >= 0; num--)
			{
				QuestGiver questGiver = m_QuestGiverPool[num];
				if (questGiver != null && questGiver.m_QuestGiver == character)
				{
					questGiver.ResetInfo();
					m_QuestGiverPool[num] = null;
					return;
				}
			}
		}
		if (m_AcceptedQuests != null)
		{
			for (int num2 = m_AcceptedQuests.Length - 1; num2 >= 0; num2--)
			{
				QuestGiver questGiver2 = m_AcceptedQuests[num2];
				if (questGiver2 != null && questGiver2.m_QuestGiver == character)
				{
					questGiver2.ResetInfo();
					m_AcceptedQuests[num2] = null;
					return;
				}
			}
		}
		if (m_OfferingQuestGivers == null)
		{
			return;
		}
		for (int num3 = m_OfferingQuestGivers.Count - 1; num3 >= 0; num3--)
		{
			QuestGiver questGiver3 = m_OfferingQuestGivers[num3];
			if (questGiver3 != null && questGiver3.m_QuestGiver == character)
			{
				questGiver3.ResetInfo();
				m_OfferingQuestGivers.RemoveAt(num3);
				break;
			}
		}
	}

	private void AddRobinsonSpecificQuest(Character robinson)
	{
		if (robinson.m_bIsRobinsonCharacter && m_SpecificQuests.Find((QuestMapping x) => x != null && x.m_Quest == m_RobinsonDefaultQuest) == null)
		{
			QuestMapping questMapping = new QuestMapping();
			questMapping.m_AutoCreate = true;
			questMapping.m_Quest = m_RobinsonDefaultQuest;
			questMapping.m_QuestGiver = robinson;
			m_SpecificQuests.Add(questMapping);
		}
	}

	private void CreateRobinsonSpecificQuestGiver()
	{
		PrisonData levelSetup = LevelScript.GetInstance().m_LevelSetup;
		ConfigManager instance = ConfigManager.GetInstance();
		if (!levelSetup.m_bAddRobinsonCharacter || instance == null || instance.gameType == PrisonConfig.ConfigType.Versus)
		{
			return;
		}
		int num = m_SpecificQuests.FindIndex((QuestMapping x) => object.ReferenceEquals(x.m_Quest, m_RobinsonDefaultQuest));
		if (num == -1)
		{
			int num2 = m_QuestGiverPool.FindIndex((QuestGiver x) => x != null && x.m_QuestGiver != null && x.m_QuestGiver.m_bIsRobinsonCharacter);
			if (num2 != -1)
			{
				QuestGiver questGiver = m_QuestGiverPool[num2];
				if (questGiver == null)
				{
					Debug.LogError("Robinson could not be made a quest giver! (Was null)");
					return;
				}
				if (questGiver.m_QuestGiver.m_bIsVendor)
				{
					bool success = false;
					VendorManager instance2 = VendorManager.GetInstance();
					instance2.UnregisterPotentialVendor(questGiver.m_QuestGiver);
					Vendor vendorForCharacter = instance2.GetVendorForCharacter(questGiver.m_QuestGiver, out success);
					if (success && vendorForCharacter != null)
					{
						vendorForCharacter.RequestUnassignCharacter();
						VendorManager.GetInstance().OnVendorExpired(vendorForCharacter);
					}
				}
				if (questGiver.m_AvailableQuests != null || questGiver.m_PlayerDoingQuest != null)
				{
					questGiver.ResetInfo();
				}
				Character questGiver2 = questGiver.m_QuestGiver;
				SetUpRobinsonQuests(questGiver2);
			}
			num = m_SpecificQuests.FindIndex((QuestMapping x) => object.ReferenceEquals(x.m_Quest, m_RobinsonDefaultQuest));
		}
		if (num != -1)
		{
			int num3 = FindQuestableInmate(m_SpecificQuests[num].m_QuestGiver);
			if (num3 != -1 && m_QuestGiverPool[num3].m_AvailableQuests == null && m_QuestGiverPool[num3].m_PlayerDoingQuest == null && !m_QuestGiverPool[num3].m_QuestGiver.m_bIsVendor)
			{
				m_NetView.RPC("RPC_CreateSpecificQuestGiver", NetTargets.All, num3, num);
			}
		}
	}

	private void SetUpRobinsonQuests(Character robinsonCharacter)
	{
		if (!robinsonCharacter.m_bIsRobinsonCharacter)
		{
			return;
		}
		AddRobinsonSpecificQuest(robinsonCharacter);
		int i = 0;
		for (int num = m_RobinsonQuests.Length; i < num; i++)
		{
			if (m_RobinsonQuests[i] != null)
			{
				m_RobinsonQuests[i].m_QuestGiver = robinsonCharacter;
			}
		}
	}

	public bool CheckCharacterHasPrisonQuest(Character character)
	{
		for (int i = 0; i < m_SpecificQuests.Count; i++)
		{
			if (m_SpecificQuests[i].m_QuestGiver == character)
			{
				return true;
			}
		}
		return false;
	}

	public static int DebugMenuQuestCount()
	{
		if (s_Instance != null && m_DebugQuestCount == -1)
		{
			List<QuestType> availableQuests = s_Instance.m_AvailableQuests;
			m_DebugQuestCount = 0;
			int count = availableQuests.Count;
			for (int i = 0; i < count; i++)
			{
				int count2 = availableQuests[i].m_QuestList.Count;
				m_DebugQuestCount += count2;
			}
		}
		return m_DebugQuestCount;
	}
}
