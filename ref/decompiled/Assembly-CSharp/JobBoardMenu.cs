using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;
using UnityEngine.EventSystems;

public class JobBoardMenu : MonoBehaviour
{
	private Player m_Player;

	public T17Text m_JobBoardTitle;

	public GameObject m_JobListObj;

	private List<T17Text> m_JobListText = new List<T17Text>();

	private List<T17Button> m_JobListButtons = new List<T17Button>();

	private List<JobType> m_JobListTypes = new List<JobType>();

	public GameObject m_JobDetails;

	public T17Button m_QuitApplyButton;

	public T17Text m_QuitApplyText;

	public T17Button m_BackButton;

	public T17Text m_BackText;

	public T17Text m_JobDescription;

	public T17Text m_JobStatus;

	public T17Text m_StrengthValue;

	public T17Text m_IntellectValue;

	public T17Text m_PayValue;

	public GameObject m_JobConfirm;

	public T17Button m_JobConfirmYesButton;

	public T17Text m_JobQuitText;

	public GameObject m_JobGranted;

	public T17Button m_JobGrantedOk;

	public Color m_JobAvailableColour = Color.green;

	public Color m_JobNotAvailableColour = Color.red;

	public string m_EmployeeNameHighlightColour = "A1BAD6FF";

	private JobType m_SelectedJob;

	private IT17EventHelper[] m_EventHelperInterfaces;

	private int m_LastSelectedIndex = -1;

	public string m_RequirementNotMetTag = string.Empty;

	public string m_JobFilledTag = string.Empty;

	public string m_JobFilledEmployeeKey = "$JobEmployee";

	public string m_CanAccessJobTag = string.Empty;

	public string m_ApplyTag = string.Empty;

	public string m_QuitTag = string.Empty;

	public string m_QuitForNewJobTag = string.Empty;

	private void Awake()
	{
		m_EventHelperInterfaces = GetComponentsInChildren<IT17EventHelper>(includeInactive: true);
		m_JobListButtons.AddRange(m_JobListObj.GetComponentsInChildren<T17Button>(includeInactive: true));
		m_JobListText.AddRange(m_JobListObj.GetComponentsInChildren<T17Text>(includeInactive: true));
		m_JobStatus.supportRichText = true;
		m_JobStatus.m_bNeedsLocalization = false;
	}

	public void ShowJobBoard(Player player, CameraManager.PlayerBindingID bindingID)
	{
		if (!base.gameObject.GetActive())
		{
			base.gameObject.SetActive(value: true);
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_JobBoard_Highlight, base.gameObject);
			m_Player = player;
		}
		UpdateJobs();
		if (m_EventHelperInterfaces != null)
		{
			T17EventSystem gamersEventSystem = null;
			if (m_Player != null && m_Player.m_Gamer != null)
			{
				gamersEventSystem = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Player.m_Gamer);
			}
			for (int i = 0; i < m_EventHelperInterfaces.Length; i++)
			{
				if (m_EventHelperInterfaces[i] != null && m_Player.m_Gamer != null)
				{
					m_EventHelperInterfaces[i].SetGamerForEventSystem(m_Player.m_Gamer, gamersEventSystem);
				}
			}
		}
		if (EventSystem.current != null && m_JobListButtons[0] != null && m_Player.m_Gamer != null)
		{
			T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Player.m_Gamer);
			eventSystemForGamer.SetSelectedGameObject(null);
			eventSystemForGamer.SetSelectedGameObject(m_JobListButtons[0].gameObject);
		}
	}

	public void Hide()
	{
		ShowJobList(selectList: false);
		if (base.gameObject.activeSelf)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_JobBoard_Out, base.gameObject);
			base.gameObject.SetActive(value: false);
		}
		if (AreAllJobsTakenByOtherInmates())
		{
			TutorialManager instance = TutorialManager.GetInstance();
			if (instance != null)
			{
				instance.StartTutorialRPC(m_Player, TutorialSubject.Sabotage);
			}
		}
		m_Player = null;
	}

	private void Update()
	{
		if (m_Player != null && m_Player.m_Gamer != null && m_Player.m_Gamer.m_RewiredPlayer != null)
		{
			if (m_Player.m_Gamer.m_RewiredPlayer.GetButtonUp("UI_Cancel"))
			{
				if (m_JobDetails.gameObject.GetActive())
				{
					ShowJobList();
				}
				else if (m_Player != null)
				{
					m_Player.RequestStopInteraction();
				}
			}
		}
		else if (!(m_Player == null) && m_Player.m_Gamer != null && m_Player.m_Gamer.m_RewiredPlayer != null)
		{
		}
	}

	private void UpdateJobs()
	{
		m_JobListTypes.Clear();
		JobsManager instance = JobsManager.GetInstance();
		if (!(instance != null))
		{
			return;
		}
		for (int i = 0; i < m_JobListButtons.Count; i++)
		{
			JobType jobType = instance.GetJobType(i);
			if (jobType == JobType.Invalid)
			{
				m_JobListButtons[i].gameObject.SetActive(value: false);
				continue;
			}
			m_JobListButtons[i].gameObject.SetActive(value: true);
			m_JobListTypes.Add(jobType);
			if (i >= m_JobListText.Count)
			{
				continue;
			}
			JobInfo jobInfo = instance.GetJobInfo(jobType);
			if (jobInfo != null)
			{
				m_JobListText[i].SetNewLocalizationTag(jobInfo.m_NameTextID);
			}
			if (!instance.DoesCharacterMeetJobRequirements(m_Player, jobType))
			{
				m_JobListText[i].color = m_JobNotAvailableColour;
				continue;
			}
			Character jobEmployee = instance.GetJobEmployee(jobType);
			if (jobEmployee != null && jobEmployee != m_Player)
			{
				m_JobListText[i].color = m_JobNotAvailableColour;
			}
			else
			{
				m_JobListText[i].color = m_JobAvailableColour;
			}
		}
	}

	public void JobSelected(int index)
	{
		if (index >= 0 && index < m_JobListTypes.Count)
		{
			m_LastSelectedIndex = index;
			m_JobListObj.SetActive(value: false);
			m_JobDetails.SetActive(value: true);
			m_SelectedJob = m_JobListTypes[index];
			JobInfo jobInfo = JobsManager.GetInstance().GetJobInfo(m_SelectedJob);
			m_JobDescription.SetNewLocalizationTag(jobInfo.m_DescTextID);
			if (m_StrengthValue != null)
			{
				m_StrengthValue.m_bNeedsLocalization = false;
				m_StrengthValue.text = jobInfo.m_StrengthRequired.ToString();
			}
			if (m_IntellectValue != null)
			{
				m_IntellectValue.m_bNeedsLocalization = false;
				m_IntellectValue.text = jobInfo.m_IntellectRequired.ToString();
			}
			if (m_PayValue != null)
			{
				m_PayValue.m_bNeedsLocalization = false;
				m_PayValue.text = jobInfo.m_MoneyEarned.ToString();
			}
			GameObject gameObject = null;
			if (JobsManager.GetInstance().GetCharactersJobType(m_Player) != m_SelectedJob)
			{
				m_QuitApplyText.SetNewLocalizationTag(m_ApplyTag);
				Character jobEmployee = JobsManager.GetInstance().GetJobEmployee(m_SelectedJob);
				bool flag = JobsManager.GetInstance().DoesCharacterMeetJobRequirements(m_Player, m_SelectedJob);
				bool flag2 = jobEmployee != null && jobEmployee != m_Player;
				if (!flag || flag2)
				{
					m_JobStatus.text = string.Empty;
					if (flag2)
					{
						AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Unavailable, base.gameObject);
						string displayName = JobsManager.GetInstance().GetJobEmployee(m_SelectedJob).m_CharacterCustomisation.m_DisplayName;
						string localized = string.Empty;
						string value = "<color=#" + m_EmployeeNameHighlightColour + ">" + displayName + "</color>";
						Localization.Get(m_JobFilledTag, out localized);
						Localization.GetWithKeySwap(m_JobFilledTag, out localized, m_JobFilledEmployeeKey, value);
						m_JobStatus.text += localized;
					}
					else if (!flag)
					{
						AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Unavailable, base.gameObject);
						string localized2 = string.Empty;
						Localization.Get(m_RequirementNotMetTag, out localized2);
						m_JobStatus.text += localized2;
					}
					m_QuitApplyButton.gameObject.SetActive(value: false);
					gameObject = m_BackButton.gameObject;
				}
				else
				{
					string localized3 = string.Empty;
					Localization.Get(m_CanAccessJobTag, out localized3);
					m_JobStatus.text = localized3;
					m_QuitApplyButton.gameObject.SetActive(value: true);
					gameObject = m_QuitApplyButton.gameObject;
				}
			}
			else
			{
				m_JobStatus.text = string.Empty;
				m_QuitApplyText.SetNewLocalizationTag(m_QuitTag);
				m_QuitApplyButton.gameObject.SetActive(value: true);
				gameObject = m_QuitApplyButton.gameObject;
			}
			if (EventSystem.current != null && m_QuitApplyText.gameObject != null && gameObject != null)
			{
				T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Player.m_Gamer);
				eventSystemForGamer.SetSelectedGameObject(null);
				eventSystemForGamer.SetSelectedGameObject(gameObject);
			}
		}
		else if (index < m_JobListText.Count && index >= 0 && m_JobListTypes.Count >= m_JobListText.Count)
		{
		}
	}

	public void OnQuitApplyClicked()
	{
		if (!(m_Player != null))
		{
			return;
		}
		if (JobsManager.GetInstance().GetJobEmployee(m_SelectedJob) != m_Player)
		{
			if (JobsManager.GetInstance().GetCharactersJobType(m_Player) != 0)
			{
				ShowJobConfirm();
				m_JobQuitText.SetNewLocalizationTag(m_QuitForNewJobTag);
			}
			else
			{
				JobsManager.GetInstance().AssignCharacterToJob(m_Player, m_SelectedJob);
				ShowJobGranted();
			}
		}
		else
		{
			ShowJobConfirm();
			m_JobQuitText.SetNewLocalizationTag(m_QuitTag);
		}
	}

	public void ShowJobList(bool selectList = true)
	{
		m_SelectedJob = JobType.Invalid;
		m_JobListObj.SetActive(value: true);
		if (null != m_JobDetails)
		{
			m_JobDetails.SetActive(value: false);
		}
		if (null != m_JobConfirm)
		{
			m_JobConfirm.SetActive(value: false);
		}
		if (null != m_JobGranted)
		{
			m_JobGranted.SetActive(value: false);
		}
		if (selectList)
		{
			int num = 0;
			if (m_LastSelectedIndex != -1)
			{
				num = m_LastSelectedIndex;
			}
			if (EventSystem.current != null && m_JobListButtons.Count > num && m_JobListButtons[num] != null && m_Player != null && m_Player.m_Gamer != null)
			{
				T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Player.m_Gamer);
				eventSystemForGamer.SetSelectedGameObject(null);
				eventSystemForGamer.SetSelectedGameObject(m_JobListButtons[num].gameObject);
			}
		}
	}

	public void ShowJobConfirm()
	{
		m_JobListObj.SetActive(value: false);
		if (null != m_JobDetails)
		{
			m_JobDetails.SetActive(value: false);
		}
		if (null != m_JobConfirm)
		{
			m_JobConfirm.SetActive(value: true);
		}
		if (null != m_JobGranted)
		{
			m_JobGranted.SetActive(value: false);
		}
		if (EventSystem.current != null && m_JobDetails != null && m_Player != null && m_Player.m_Gamer != null)
		{
			T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Player.m_Gamer);
			eventSystemForGamer.SetSelectedGameObject(null);
			eventSystemForGamer.SetSelectedGameObject(m_JobConfirmYesButton.gameObject);
		}
	}

	public void ShowJobDetails()
	{
		m_JobListObj.SetActive(value: false);
		if (null != m_JobDetails)
		{
			m_JobDetails.SetActive(value: true);
		}
		if (null != m_JobConfirm)
		{
			m_JobConfirm.SetActive(value: false);
		}
		if (null != m_JobGranted)
		{
			m_JobGranted.SetActive(value: false);
		}
		if (EventSystem.current != null && m_JobConfirm != null && m_Player != null && m_Player.m_Gamer != null)
		{
			T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Player.m_Gamer);
			eventSystemForGamer.SetSelectedGameObject(null);
			eventSystemForGamer.SetSelectedGameObject(m_QuitApplyText.gameObject);
		}
	}

	public void ShowJobGranted()
	{
		m_SelectedJob = JobType.Invalid;
		m_JobListObj.SetActive(value: false);
		if (null != m_JobDetails)
		{
			m_JobDetails.SetActive(value: false);
		}
		if (null != m_JobConfirm)
		{
			m_JobConfirm.SetActive(value: false);
		}
		if (null != m_JobGranted)
		{
			m_JobGranted.SetActive(value: true);
		}
		if (EventSystem.current != null && m_JobConfirm != null && m_Player != null && m_Player.m_Gamer != null)
		{
			T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Player.m_Gamer);
			eventSystemForGamer.SetSelectedGameObject(null);
			eventSystemForGamer.SetSelectedGameObject(m_JobGrantedOk.gameObject);
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_JobBoard_Accept, base.gameObject);
		}
	}

	public void JobQuitConfirm()
	{
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Unavailable, base.gameObject);
		if (m_SelectedJob != JobsManager.GetInstance().GetCharactersJobType(m_Player))
		{
			JobsManager.GetInstance().AssignCharacterToJob(m_Player, m_SelectedJob);
			ShowJobGranted();
		}
		else
		{
			JobsManager.GetInstance().RemoveCharacterFromJob(m_Player);
			ShowJobList();
		}
	}

	public void JobQuitDecline()
	{
		ShowJobList();
	}

	public void JobGrantedOk()
	{
		ShowJobList();
		if (m_Player != null)
		{
			m_Player.RequestStopInteraction();
		}
	}

	private bool AnyJobAvailable()
	{
		JobsManager instance = JobsManager.GetInstance();
		if (instance != null)
		{
			for (int i = 0; i < m_JobListButtons.Count; i++)
			{
				JobType jobType = instance.GetJobType(i);
				if (instance.DoesCharacterMeetJobRequirements(m_Player, jobType))
				{
					Character jobEmployee = instance.GetJobEmployee(jobType);
					if (jobEmployee == null || jobEmployee == m_Player)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool AreAllJobsTakenByOtherInmates()
	{
		JobsManager instance = JobsManager.GetInstance();
		if (instance != null)
		{
			for (int i = 0; i < m_JobListButtons.Count; i++)
			{
				JobType jobType = instance.GetJobType(i);
				Character jobEmployee = instance.GetJobEmployee(jobType);
				if (jobEmployee == null || jobEmployee == m_Player)
				{
					return false;
				}
			}
		}
		return true;
	}
}
