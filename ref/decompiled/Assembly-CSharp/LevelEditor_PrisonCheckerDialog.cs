using System.Collections.Generic;
using UnityEngine;

public class LevelEditor_PrisonCheckerDialog : MonoBehaviour
{
	public EditorPublishMenu m_PrisonPublishDialog;

	public Transform m_PrisonCheckingMenu;

	public Transform m_PrisonValidationMenu;

	public T17Button m_PublishButton;

	public T17Button m_PreviewButton;

	public LevelEditor_ErrorList m_ErrorList;

	private List<LevelDetailsManager.ErrorData> m_strErrors = new List<LevelDetailsManager.ErrorData>();

	private float m_fShowTimer;

	private float m_fShowDelay = 1f;

	public void ShowPrisonCheckerDialog()
	{
		base.gameObject.SetActive(value: true);
		if (m_PrisonCheckingMenu != null)
		{
			m_PrisonCheckingMenu.gameObject.SetActive(value: true);
		}
		if (m_PrisonValidationMenu != null)
		{
			m_PrisonValidationMenu.gameObject.SetActive(value: false);
		}
		Gamer primaryGamer = Gamer.GetPrimaryGamer();
		if (primaryGamer != null && primaryGamer.m_RewiredPlayer != null)
		{
			T17EventSystem.InputCateogryStates stateForRewiredPlayer = T17EventSystem.GetStateForRewiredPlayer(primaryGamer.m_RewiredPlayer);
			if (stateForRewiredPlayer != T17EventSystem.InputCateogryStates.Dialogbox)
			{
				T17EventSystem.ApplyCategories(primaryGamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.Dialogbox);
			}
		}
		LevelDetailsManager instance = LevelDetailsManager.GetInstance();
		if (!(instance != null))
		{
			return;
		}
		m_strErrors.Clear();
		if (LevelDetailsManager.c_CurrentLevelDataVersionNumber == LevelDetailsManager.LevelEditorDataVersion.V2_AddedZoneEditing)
		{
			LevelEditor_ZoneManager instance2 = LevelEditor_ZoneManager.GetInstance();
			if (instance2 != null)
			{
				instance2.GetZoneValidationErrors(ref m_strErrors);
			}
		}
		instance.ValidateEverythingIsReachable(ref m_strErrors);
		instance.GetLevelDataValidationErrors(ref m_strErrors);
		instance.ValidateWalkableAreas(ref m_strErrors);
		if (m_ErrorList != null)
		{
			m_ErrorList.CreateErrors(m_strErrors);
		}
		m_fShowTimer = Time.realtimeSinceStartup + m_fShowDelay;
	}

	private void Update()
	{
		if (m_fShowTimer != -1f && Time.realtimeSinceStartup > m_fShowTimer)
		{
			ShowValidationMenu();
		}
	}

	private void ShowValidationMenu()
	{
		m_fShowTimer = -1f;
		if (m_PrisonCheckingMenu != null)
		{
			m_PrisonCheckingMenu.gameObject.SetActive(value: false);
		}
		if (m_PrisonValidationMenu != null)
		{
			m_PrisonValidationMenu.gameObject.SetActive(value: true);
		}
		bool flag = false;
		for (int num = m_strErrors.Count - 1; num >= 0; num--)
		{
			if (m_strErrors[num].m_Severity == LevelDetailsManager.ErrorData.Severity.Error)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			if (m_PublishButton != null)
			{
				m_PublishButton.interactable = false;
			}
			if (m_PreviewButton != null)
			{
				m_PreviewButton.interactable = false;
			}
		}
		else
		{
			if (m_PublishButton != null)
			{
				m_PublishButton.interactable = true;
			}
			if (m_PreviewButton != null)
			{
				m_PreviewButton.interactable = true;
			}
		}
	}

	public void HidePrisonCheckerDialog()
	{
		m_strErrors.Clear();
		m_fShowTimer = -1f;
		if (m_ErrorList != null)
		{
			m_ErrorList.RemoveAllErrors();
		}
		Gamer primaryGamer = Gamer.GetPrimaryGamer();
		if (primaryGamer != null && primaryGamer.m_RewiredPlayer != null)
		{
			T17EventSystem.ApplyLastRequestedStateSinceDialogboxWasUp(primaryGamer.m_RewiredPlayer);
			T17EventSystem.InputCateogryStates stateForRewiredPlayer = T17EventSystem.GetStateForRewiredPlayer(primaryGamer.m_RewiredPlayer);
			if (stateForRewiredPlayer != T17EventSystem.InputCateogryStates.LevelEditor)
			{
				T17EventSystem.ApplyCategories(primaryGamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.LevelEditor);
			}
		}
		base.gameObject.SetActive(value: false);
	}

	public void OnCancelButtonClicked()
	{
		HidePrisonCheckerDialog();
	}

	public void OnPreviewButtonClicked()
	{
		if ((bool)LevelEditor_Controller.GetInstance())
		{
			LevelEditor_Controller.GetInstance().PreviewLevel();
		}
	}

	public void OnPublishButtonClicked()
	{
		HidePrisonCheckerDialog();
		if (m_PrisonPublishDialog != null)
		{
			m_PrisonPublishDialog.ShowPublishDialog();
		}
	}
}
