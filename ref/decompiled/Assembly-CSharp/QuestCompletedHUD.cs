public class QuestCompletedHUD : BaseIngamePassiveUI
{
	public T17Text m_TitleLabel;

	public T17Text m_QuestNameLabel;

	public float m_DisplayTime = 3f;

	private float m_TimeUntilDisappear;

	private bool m_bRegisteredWithQuestManager;

	private void Start()
	{
		RegisterWithQuestManager();
		if (m_TimeUntilDisappear <= 0f)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public override bool Init(Player owner)
	{
		if (base.Init(owner))
		{
			RegisterWithQuestManager();
			return true;
		}
		return false;
	}

	private void RegisterWithQuestManager()
	{
		if (!m_bRegisteredWithQuestManager)
		{
			QuestManager.QuestCompletedEvent += QuestManager_QuestCompletedEvent;
			QuestManager.QuestFailedEvent += QuestManager_QuestFailedEvent;
			m_bRegisteredWithQuestManager = true;
		}
	}

	protected override void OnDestroy()
	{
		QuestManager.QuestCompletedEvent -= QuestManager_QuestCompletedEvent;
		QuestManager.QuestFailedEvent -= QuestManager_QuestFailedEvent;
		m_TitleLabel = null;
		m_QuestNameLabel = null;
		base.OnDestroy();
	}

	private void QuestManager_QuestCompletedEvent(ObjectiveTree tree, Player playerDoingQuest)
	{
		if (playerDoingQuest == m_LinkedPlayer)
		{
			base.gameObject.SetActive(value: true);
			m_TimeUntilDisappear = m_DisplayTime;
			m_TitleLabel.SetNewLocalizationTag("Text.UI.FavourCompleted");
			if (m_QuestNameLabel != null)
			{
				m_QuestNameLabel.m_bNeedsLocalization = false;
				m_QuestNameLabel.text = tree.MainBranch.GetQuestLabel("NOT FOUND");
			}
		}
	}

	private void QuestManager_QuestFailedEvent(ObjectiveTree tree, Player playerDoingQuest)
	{
		if (playerDoingQuest == m_LinkedPlayer)
		{
			base.gameObject.SetActive(value: true);
			m_TimeUntilDisappear = m_DisplayTime;
			m_TitleLabel.SetNewLocalizationTag("Text.UI.FavourFailed");
			if (m_QuestNameLabel != null)
			{
				m_QuestNameLabel.m_bNeedsLocalization = false;
				m_QuestNameLabel.text = tree.MainBranch.GetQuestLabel("NOT FOUND");
			}
		}
	}

	private void Update()
	{
		m_TimeUntilDisappear -= UpdateManager.deltaTime;
		if (m_TimeUntilDisappear <= 0f)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
