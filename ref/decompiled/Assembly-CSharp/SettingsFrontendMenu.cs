using System;
using UnityEngine;

public class SettingsFrontendMenu : FrontendMenuBehaviour
{
	public T17StatsSlider m_MusicSlider;

	public T17StatsSlider m_SFXSlider;

	[Header("Tab Settings")]
	public T17TabPanel m_OptionsTabPanel;

	public GameObject m_SaveModeTextBox;

	public T17Button m_SaveModeToggleButton;

	public T17Text m_SaveModeToggleText;

	public T17Text m_SaveModeExplanationText;

	public string m_AutoSaveTitle = string.Empty;

	public string m_ManualSaveTitle = string.Empty;

	public string m_OffSaveTitle = string.Empty;

	public string m_AutoSaveBody = string.Empty;

	public string m_ManualSaveBody = string.Empty;

	public string m_OffSaveBody = string.Empty;

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		SaveManager instance = SaveManager.GetInstance();
		if (instance != null)
		{
			instance.OnSaveModeChanged = (SaveManager.OnSaveModeChangedDelegate)Delegate.Combine(instance.OnSaveModeChanged, new SaveManager.OnSaveModeChangedDelegate(OnSaveModeChanged));
		}
		bool active = IsAllowedToSaveGames();
		if ((bool)m_SaveModeToggleButton)
		{
			m_SaveModeToggleButton.gameObject.SetActive(active);
		}
		if (m_SaveModeTextBox != null)
		{
			m_SaveModeTextBox.SetActive(active);
		}
		bool flag = base.Show(currentGamer, parent, invoker, hideInvoker);
		if (flag && m_OptionsTabPanel != null)
		{
			m_OptionsTabPanel.Show(currentGamer, this, null);
			m_OptionsTabPanel.SetTabIndex(0);
		}
		UpdateSaveToggleButton();
		return flag;
	}

	protected override void SingleTimeInitialize()
	{
		base.SingleTimeInitialize();
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		SaveManager instance = SaveManager.GetInstance();
		if (instance != null)
		{
			instance.OnSaveModeChanged = (SaveManager.OnSaveModeChangedDelegate)Delegate.Remove(instance.OnSaveModeChanged, new SaveManager.OnSaveModeChangedDelegate(OnSaveModeChanged));
		}
		return true;
	}

	public void OnMusicVolumeChanged(float value)
	{
	}

	public void OnSFXVolumeChanged(float value)
	{
	}

	public void CreditsButton_OnClicked()
	{
		GlobalStart.GetInstance().ShowCreditsScreen();
	}

	public void CallCancel()
	{
		if (m_NavigateOnUICancel != null && m_NavigateOnUICancel.m_DoThisOnUICancel != null)
		{
			m_NavigateOnUICancel.m_DoThisOnUICancel.Invoke();
		}
	}

	private bool IsAllowedToSaveGames()
	{
		if (ConfigManager.GetInstance() != null && (ConfigManager.GetInstance().gameType == PrisonConfig.ConfigType.Versus || ConfigManager.GetInstance().gameType == PrisonConfig.ConfigType.Singleplayer))
		{
			return false;
		}
		if (LevelScript.GetInstance() != null && LevelScript.GetInstance().m_LevelSetup != null && LevelScript.GetInstance().m_LevelSetup.m_LevelInfo.m_PrisonEnum == LevelScript.PRISON_ENUM.CustomPrison)
		{
			return LevelScript.AreWeOriginalHost();
		}
		return true;
	}

	public void UpdateSaveToggleButton()
	{
		if (m_SaveModeToggleText != null && m_SaveModeExplanationText != null && m_SaveModeTextBox != null)
		{
			bool active = IsAllowedToSaveGames();
			bool interactable = base.CurrentGamer != null && base.CurrentGamer.m_bPrimaryLocal;
			SaveManager.SaveMode currentSaveMode = SaveManager.GetInstance().CurrentSaveMode;
			m_SaveModeToggleButton.gameObject.SetActive(active);
			m_SaveModeToggleButton.interactable = interactable;
			m_SaveModeTextBox.SetActive(active);
			switch (currentSaveMode)
			{
			case SaveManager.SaveMode.Automatic:
				m_SaveModeToggleText.SetNewLocalizationTag(m_AutoSaveTitle);
				m_SaveModeExplanationText.SetNewLocalizationTag(m_AutoSaveBody);
				break;
			case SaveManager.SaveMode.Manual:
				m_SaveModeToggleText.SetNewLocalizationTag(m_ManualSaveTitle);
				m_SaveModeExplanationText.SetNewLocalizationTag(m_ManualSaveBody);
				break;
			case SaveManager.SaveMode.Off:
				m_SaveModeToggleText.SetNewLocalizationTag(m_OffSaveTitle);
				m_SaveModeExplanationText.SetNewLocalizationTag(m_OffSaveBody);
				break;
			}
		}
	}

	public void OnCancel(FrontendMenuBehaviour menu)
	{
		if (base.CurrentRewiredPlayer.GetButtonUp("UI_Cancel"))
		{
			return;
		}
		for (int num = m_OptionsTabPanel.m_MenuBodies.Length - 1; num >= 0; num--)
		{
			if (m_OptionsTabPanel.m_MenuBodies[num].gameObject != null && m_OptionsTabPanel.m_MenuBodies[num].gameObject.activeInHierarchy)
			{
				m_OptionsTabPanel.m_MenuBodies[num].UICancel();
				return;
			}
		}
		FrontEndFlow.Instance.SwitchBackToFrontendMenu(menu);
	}

	public void OnSaveModeChanged(SaveManager.SaveMode newSaveMode)
	{
		UpdateSaveToggleButton();
	}

	public void ChangeSaveMode()
	{
		GoogleAnalyticsV3.LogCommericalAnalyticEvent("Save Mode", "Frontend Save Mode Changed", string.Empty, 0L);
		SaveManager instance = SaveManager.GetInstance();
		if (instance != null)
		{
			instance.CycleSaveMode();
		}
	}
}
