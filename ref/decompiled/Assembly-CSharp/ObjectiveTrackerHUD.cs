using System.Collections;
using UnityEngine;

public class ObjectiveTrackerHUD : BaseIngamePassiveUI
{
	public GameObject m_QuestUIItem;

	public T17Text m_Title;

	public T17Text m_MultiPartText;

	public Animator m_NewQuestAnimator;

	public string m_NewQuestTriggerStart = "SlideRight";

	public Sprite m_DebugActiveQuestSprite;

	private ObjectiveTree m_ObjectiveTree;

	private ObjectiveGoal m_LastKnownGoal;

	private int m_ActiveSubGoals;

	private ObjectiveSubGoalHUD[] m_SubGoalList;

	private bool m_bShowObjectiveArrow;

	private bool m_bShowObjectivePins;

	public AnimationPlayer m_FlashPlayer;

	private UIAnimatedEffect m_SlidingAnimation;

	public float m_PostAnimDisplayLength = 1f;

	private bool m_bPlayNewQuestAnim;

	protected override void Awake()
	{
		if (m_QuestUIItem != null)
		{
			if (m_SubGoalList == null)
			{
				m_SubGoalList = m_QuestUIItem.GetComponentsInChildren<ObjectiveSubGoalHUD>(includeInactive: true);
			}
			HideSubGoals();
			m_QuestUIItem.SetActive(value: false);
		}
		base.Awake();
	}

	protected override void OnDestroy()
	{
		m_QuestUIItem = null;
		m_Title = null;
		m_MultiPartText = null;
		m_NewQuestAnimator = null;
		m_DebugActiveQuestSprite = null;
		m_ObjectiveTree = null;
		m_LastKnownGoal = null;
		m_SubGoalList = null;
		m_FlashPlayer = null;
		if (m_SlidingAnimation != null)
		{
			m_SlidingAnimation.AnimationFinishedEvent -= SlidingAnimation_AnimationFinishedEvent;
		}
		m_SlidingAnimation = null;
		base.OnDestroy();
	}

	public override bool Init(Player owner)
	{
		if (base.Init(owner))
		{
			if (string.IsNullOrEmpty(m_NewQuestTriggerStart))
			{
				m_NewQuestTriggerStart = "SlideRight";
			}
			if (m_NewQuestAnimator != null)
			{
				m_SlidingAnimation = m_NewQuestAnimator.GetComponent<UIAnimatedEffect>();
				if (m_SlidingAnimation != null)
				{
					m_SlidingAnimation.AnimationFinishedEvent += SlidingAnimation_AnimationFinishedEvent;
				}
			}
			return true;
		}
		return false;
	}

	protected void Update()
	{
		if (m_LinkedPlayer == null)
		{
			return;
		}
		if (m_bPlayNewQuestAnim)
		{
			if (m_NewQuestAnimator != null)
			{
				if (m_NewQuestAnimator.isInitialized)
				{
					m_bPlayNewQuestAnim = false;
					m_NewQuestAnimator.SetTrigger(m_NewQuestTriggerStart);
					StartCoroutine(DelayedUIEnable());
				}
			}
			else
			{
				m_bPlayNewQuestAnim = false;
				MakeUIActive();
			}
		}
		if (m_ObjectiveTree != null)
		{
			if (m_ObjectiveTree.GetObjectiveStatus == ObjectiveStatus.InComplete || m_ObjectiveTree.GetObjectiveStatus == ObjectiveStatus.InActive)
			{
				ObjectiveGoal nextLogableGoal = m_ObjectiveTree.MainBranch.GetNextLogableGoal();
				if (nextLogableGoal == m_LastKnownGoal)
				{
					return;
				}
				bool flag = true;
				if (object.ReferenceEquals(m_ObjectiveTree.MainBranch.GetWhichGoalComesFirst(nextLogableGoal, m_LastKnownGoal), m_LastKnownGoal))
				{
					for (int i = 0; i < m_ActiveSubGoals; i++)
					{
						if (m_SubGoalList[i].isActiveAndEnabled && m_SubGoalList[i].ElapsedTimeAfterTick < m_PostAnimDisplayLength)
						{
							flag = false;
							break;
						}
					}
				}
				if (flag)
				{
					m_LastKnownGoal = nextLogableGoal;
					UpdateGoalInformation(m_LastKnownGoal);
				}
				return;
			}
			bool flag2 = true;
			if (m_ObjectiveTree.GetObjectiveStatus == ObjectiveStatus.Done)
			{
				for (int j = 0; j < m_ActiveSubGoals; j++)
				{
					if (m_SubGoalList[j].isActiveAndEnabled && m_SubGoalList[j].ElapsedTimeAfterTick < m_PostAnimDisplayLength)
					{
						flag2 = false;
						break;
					}
				}
			}
			if (flag2)
			{
				ObjectiveTree objectiveTree = null;
				ObjectiveManager instance = ObjectiveManager.GetInstance();
				if (instance != null)
				{
					objectiveTree = instance.GetPrisonObjectiveTree(m_LinkedPlayer);
				}
				if (objectiveTree != null && objectiveTree != m_ObjectiveTree)
				{
					SetObjectiveTreeToTrack(objectiveTree);
					return;
				}
				ReleasePin();
				MakeUIInactive();
			}
		}
		else if (m_QuestUIItem != null && m_QuestUIItem.activeInHierarchy)
		{
			MakeUIInactive();
		}
	}

	private IEnumerator DelayedUIEnable()
	{
		yield return new WaitForEndOfFrame();
		MakeUIActive();
	}

	public bool SetObjectiveTreeToTrack(ObjectiveTree objectiveTree, bool force = false)
	{
		if (!force && (m_ObjectiveTree == objectiveTree || objectiveTree == null))
		{
			ReleasePin();
			MakeUIInactive();
			return false;
		}
		if (m_LastKnownGoal != null)
		{
			m_LastKnownGoal.SetObjectivePins(on: false);
			m_LastKnownGoal.SetObjectiveArrow(on: false);
			m_LastKnownGoal.ClearHUDInfo();
		}
		if (m_ObjectiveTree != null)
		{
			m_ObjectiveTree.isBeingTracked = false;
		}
		m_ObjectiveTree = objectiveTree;
		if (m_ObjectiveTree != null)
		{
			m_LastKnownGoal = m_ObjectiveTree.MainBranch.GetNextLogableGoal();
			m_bShowObjectiveArrow = m_ObjectiveTree.m_bShowTrackingArrows;
			m_bShowObjectivePins = m_ObjectiveTree.m_bShowTrackingPins;
			m_ObjectiveTree.isBeingTracked = true;
			SetUiForObjectiveTree();
			if (m_QuestUIItem != null)
			{
				if (m_ObjectiveTree.m_bIsTrackable)
				{
					m_bPlayNewQuestAnim = true;
					m_QuestUIItem.SetActive(value: false);
				}
				else
				{
					m_QuestUIItem.SetActive(value: false);
				}
			}
			UpdateGoalInformation(m_LastKnownGoal);
			if (m_LastKnownGoal != null)
			{
				AttachPin();
			}
			else
			{
				ReleasePin();
			}
			return true;
		}
		ReleasePin();
		MakeUIInactive();
		return false;
	}

	private void SetUiForObjectiveTree()
	{
		if (m_MultiPartText != null)
		{
			if (string.IsNullOrEmpty(m_ObjectiveTree.m_MultiPartText))
			{
				m_MultiPartText.gameObject.SetActive(value: false);
			}
			else
			{
				m_MultiPartText.gameObject.SetActive(value: true);
				m_MultiPartText.m_bNeedsLocalization = false;
				m_MultiPartText.text = m_ObjectiveTree.m_MultiPartText;
			}
		}
		if (!(m_Title != null))
		{
			return;
		}
		m_Title.m_bNeedsLocalization = false;
		m_Title.text = string.Empty;
		if (!m_ObjectiveTree.m_bIsTrackable)
		{
			return;
		}
		QuestIntroObjective questDescription = m_ObjectiveTree.MainBranch.GetQuestDescription();
		if (questDescription != null)
		{
			if (questDescription.m_bUsePerObjectiveTitles && m_LastKnownGoal != null)
			{
				m_Title.text = m_LastKnownGoal.m_Objective.LocalizedObjectiveName;
			}
			else
			{
				m_Title.text = questDescription.QuestLocalizedObjectiveName;
			}
		}
	}

	public bool IsTreeCurrentlyTracked(ObjectiveTree tree)
	{
		return tree != null && tree == m_ObjectiveTree;
	}

	private void AttachPin()
	{
	}

	private void ReleasePin()
	{
		if (m_LastKnownGoal != null)
		{
			m_LastKnownGoal.SetObjectivePins(on: false);
			m_LastKnownGoal.SetObjectiveArrow(on: false);
		}
	}

	public void UpdateGoalInformation(ObjectiveGoal goal)
	{
		HideSubGoals();
		if (goal == null)
		{
			m_ActiveSubGoals = 0;
			return;
		}
		if (m_ObjectiveTree == null || m_ObjectiveTree.m_bIsTrackable)
		{
			m_ActiveSubGoals = goal.UpdateHUDInfo(ref m_SubGoalList);
		}
		else
		{
			m_ActiveSubGoals = 0;
		}
		goal.SetObjectivePins(m_bShowObjectivePins);
		goal.SetObjectiveArrow(m_bShowObjectiveArrow);
		if (m_ObjectiveTree.m_bIsTrackable)
		{
			QuestIntroObjective questDescription = m_ObjectiveTree.MainBranch.GetQuestDescription();
			if (questDescription != null && questDescription.m_bUsePerObjectiveTitles)
			{
				m_Title.text = goal.m_Objective.LocalizedObjectiveName;
			}
		}
	}

	private void MakeUIInactive()
	{
		ClearData();
		if (m_QuestUIItem != null)
		{
			m_QuestUIItem.SetActive(value: false);
		}
		if (m_MultiPartText != null)
		{
			m_MultiPartText.gameObject.SetActive(value: false);
		}
	}

	private void MakeUIActive()
	{
		if (m_QuestUIItem != null)
		{
			m_QuestUIItem.SetActive(value: true);
		}
	}

	private void ClearData()
	{
		m_ObjectiveTree = null;
		m_LastKnownGoal = null;
		HideSubGoals();
		if (m_Title != null)
		{
			m_Title.text = string.Empty;
		}
	}

	private void HideSubGoals()
	{
		m_ActiveSubGoals = 0;
		if (m_SubGoalList != null)
		{
			for (int i = 0; i < m_SubGoalList.Length; i++)
			{
				m_SubGoalList[i].Hide();
			}
		}
	}

	private void SlidingAnimation_AnimationFinishedEvent(UIAnimatedEffect sender)
	{
		if (m_FlashPlayer != null)
		{
			m_FlashPlayer.Play();
		}
	}
}
