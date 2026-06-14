using UnityEngine;

public class JobTutorialMenu : BaseIngameMenu
{
	private BaseJob m_Job;

	private int m_CurrentPageIndex;

	public T17Text m_TitleLabel;

	public UIJobTutorialStep[] m_StepElements;

	public T17Button m_NextPageButton;

	public T17Button m_PrevPageButton;

	public T17Text m_PageNumberLabel;

	public string m_PageNumberPrefix = "Text.Legend.Page";

	private string m_LocalisedPageNumberPrefix;

	protected override void Awake()
	{
		base.Awake();
		if (m_NextPageButton == null || m_PrevPageButton == null)
		{
		}
		if (m_StepElements.Length == 0)
		{
		}
		Localization.Get(m_PageNumberPrefix, out m_LocalisedPageNumberPrefix);
	}

	public void SetupWithJob(BaseJob job)
	{
		m_Job = job;
		if (m_TitleLabel != null)
		{
			m_TitleLabel.SetLocalisedTextCatchAll(job.m_Info.m_NameTextID);
		}
	}

	public override void Show(Player player)
	{
		base.Show(player);
		if (!(m_Job == null))
		{
			ShowPage(0);
		}
	}

	protected void ShowPage(int index)
	{
		int count = m_Job.m_TutorialSteps.Count;
		int num = count / m_StepElements.Length;
		if (count <= m_StepElements.Length)
		{
			num = 1;
		}
		else if (num % m_StepElements.Length != 0)
		{
			num++;
		}
		if ((index < num || num <= 0) && index >= 0)
		{
			m_CurrentPageIndex = index;
			int num2 = m_CurrentPageIndex * m_StepElements.Length;
			int num3 = Mathf.Min(count - 1, num2 + m_StepElements.Length - 1);
			int i = 0;
			for (int j = num2; j <= num3; j++)
			{
				m_StepElements[i].gameObject.SetActive(value: true);
				string prefix = i + 1 + ". ";
				m_StepElements[i].SetupWithJobStep(m_Job.m_TutorialSteps[j], prefix);
				i++;
			}
			for (; i < m_StepElements.Length; i++)
			{
				m_StepElements[i].gameObject.SetActive(value: false);
			}
			m_PrevPageButton.gameObject.SetActive(m_CurrentPageIndex > 0);
			m_NextPageButton.gameObject.SetActive(m_CurrentPageIndex < num - 1);
			T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Player.m_Gamer);
			eventSystemForGamer.SetSelectedGameObject(null);
			if (m_NextPageButton.gameObject.activeSelf)
			{
				eventSystemForGamer.SetSelectedGameObject(m_NextPageButton.gameObject);
			}
			else if (m_PrevPageButton.gameObject.activeSelf)
			{
				eventSystemForGamer.SetSelectedGameObject(m_PrevPageButton.gameObject);
			}
			if (m_PageNumberLabel != null)
			{
				string newPlaceHolder = m_LocalisedPageNumberPrefix + " " + (m_CurrentPageIndex + 1) + "/" + num;
				m_PageNumberLabel.m_bNeedsLocalization = false;
				m_PageNumberLabel.SetNewPlaceHolder(newPlaceHolder);
				m_PageNumberLabel.text = m_LocalisedPageNumberPrefix + " " + (m_CurrentPageIndex + 1) + "/" + num;
			}
		}
	}

	public void NextPage()
	{
		ShowPage(m_CurrentPageIndex + 1);
	}

	public void PreviousPage()
	{
		ShowPage(m_CurrentPageIndex - 1);
	}

	protected override void HideMenu()
	{
		if (m_Player != null)
		{
			m_Player.RequestStopInteraction();
		}
	}
}
