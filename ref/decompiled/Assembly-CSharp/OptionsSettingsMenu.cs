using System;
using UnityEngine;
using UnityEngine.UI;

public class OptionsSettingsMenu : BaseMenuBehaviour
{
	public enum DebugTestPlatform
	{
		Desktop,
		Console
	}

	[Header("Child Control References")]
	public T17StatsSlider m_MusicSlider_Console;

	public T17StatsSlider m_SFXSlider_Console;

	public T17StatsSlider m_MusicSlider_Desktop;

	public T17StatsSlider m_SFXSlider_Desktop;

	public T17Toggle m_VibrationToggle;

	public PhotonRegionOptionSelector m_RegionOption_Console;

	public PhotonRegionOptionSelector m_RegionOption_Desktop;

	public T17Toggle m_InfluencersToggle_Console;

	public T17Toggle m_InfluencersToggle_Desktop;

	[Header("Desktop Specific")]
	public T17Toggle m_VSyncToggle;

	public ResolutionSelector m_ResolutionSelector;

	public ShadowDetailOptionSelector m_ShadowQualitySelector;

	public T17Toggle m_BlurToggle;

	public T17Toggle m_BackgroundVideoToggle;

	public T17Toggle m_ProfanityFilterToggle;

	[Header("Platform Panels")]
	[Tooltip("The game object containing the settings panel to use when running on console platforms")]
	public GameObject m_ConsolePanel;

	public Selectable m_ConsoleTopSelectable;

	[Tooltip("The game object containing the setting panel to use when running on desktop platforms")]
	public GameObject m_DesktopPanel;

	public Selectable m_DesktopTopSelectable;

	public DebugTestPlatform m_TestPlatform;

	[Range(0f, 100f)]
	[Header("Default values")]
	public int m_DefaultMusicLevel = 100;

	[Range(0f, 100f)]
	public int m_DefaultSFXLevel = 100;

	public bool m_DefaultVibrationOn = true;

	private const int kNumSettingItems = 11;

	private BaseOptionItem[] m_Options;

	public T17Button m_DefaultButton;

	public T17Button m_ApplyButton;

	public T17Button m_TopSelectable_Console;

	public T17Button m_TopSelectable_Desktop;

	protected override void SingleTimeInitialize()
	{
		base.SingleTimeInitialize();
		T17StatsSlider musicSlider = null;
		T17StatsSlider SFXSlider = null;
		PhotonRegionOptionSelector regionOptions = null;
		T17Toggle influencersToggle = null;
		ShowDesktopPanel(out musicSlider, out SFXSlider, out regionOptions, out influencersToggle);
		m_TopSelectable = m_DesktopTopSelectable;
		m_Options = new BaseOptionItem[11]
		{
			new MusicOptionItem(musicSlider, m_DefaultMusicLevel),
			new SFXOptionItem(SFXSlider, m_DefaultSFXLevel),
			new VibrationOptionItem(m_VibrationToggle, (!m_DefaultVibrationOn) ? 0f : 1f),
			new PhotonRegionOptionItem(regionOptions, 1f),
			new ResolutionOptionItem(m_ResolutionSelector),
			new ShadowDetailOptionItem(m_ShadowQualitySelector),
			new VSyncOptionItem(m_VSyncToggle, 0f),
			new BlurOptionItem(m_BlurToggle, 0f),
			new BackgroundVideoOptionItem(m_BackgroundVideoToggle, 1f),
			new InfluencersOptionItem(influencersToggle, 1f),
			new ProfanityFilterOptionItem(m_ProfanityFilterToggle, 1f)
		};
	}

	private void ShowConsolePanel(out T17StatsSlider musicSlider, out T17StatsSlider SFXSlider, out PhotonRegionOptionSelector regionOptions, out T17Toggle influencersToggle)
	{
		musicSlider = m_MusicSlider_Console;
		SFXSlider = m_SFXSlider_Console;
		regionOptions = m_RegionOption_Console;
		influencersToggle = m_InfluencersToggle_Console;
		if (m_ConsolePanel != null)
		{
			m_ConsolePanel.SetActive(value: true);
			if (m_ApplyButton != null && m_RegionOption_Console != null)
			{
				Navigation navigation = m_ApplyButton.navigation;
				navigation.selectOnUp = m_RegionOption_Console.m_RightButton;
				m_ApplyButton.navigation = navigation;
			}
			if (m_DefaultButton != null && m_RegionOption_Console != null)
			{
				Navigation navigation2 = m_DefaultButton.navigation;
				navigation2.selectOnUp = m_RegionOption_Console.m_LeftButton;
				m_DefaultButton.navigation = navigation2;
			}
			m_TopSelectable = m_TopSelectable_Console;
		}
		if (m_DesktopPanel != null)
		{
			m_DesktopPanel.SetActive(value: false);
		}
	}

	private void ShowDesktopPanel(out T17StatsSlider musicSlider, out T17StatsSlider SFXSlider, out PhotonRegionOptionSelector regionOptions, out T17Toggle influencersToggle)
	{
		musicSlider = m_MusicSlider_Desktop;
		SFXSlider = m_SFXSlider_Desktop;
		regionOptions = m_RegionOption_Desktop;
		influencersToggle = m_InfluencersToggle_Desktop;
		if (m_ConsolePanel != null)
		{
			m_ConsolePanel.SetActive(value: false);
		}
		if (m_DesktopPanel != null)
		{
			m_DesktopPanel.SetActive(value: true);
			if (m_ApplyButton != null && m_RegionOption_Desktop != null)
			{
				Navigation navigation = m_ApplyButton.navigation;
				navigation.selectOnUp = m_RegionOption_Desktop.m_RightButton;
				m_ApplyButton.navigation = navigation;
			}
			if (m_DefaultButton != null && m_RegionOption_Desktop != null)
			{
				Navigation navigation2 = m_DefaultButton.navigation;
				navigation2.selectOnUp = m_RegionOption_Desktop.m_LeftButton;
				m_DefaultButton.navigation = navigation2;
			}
			m_TopSelectable = m_TopSelectable_Desktop;
		}
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		if (m_Options != null)
		{
			for (int i = 0; i < 11; i++)
			{
				m_Options[i].Initialise();
			}
		}
		T17ScrollView t17ScrollView = null;
		if (m_DesktopPanel != null && m_DesktopPanel.activeSelf)
		{
			t17ScrollView = m_DesktopPanel.GetComponentInChildren<T17ScrollView>(includeInactive: true);
		}
		if (m_ConsolePanel != null && m_ConsolePanel.activeSelf)
		{
			t17ScrollView = m_ConsolePanel.GetComponentInChildren<T17ScrollView>(includeInactive: true);
		}
		if (t17ScrollView != null)
		{
			t17ScrollView.Show(currentGamer, this, invoker, hideInvoker);
		}
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		return true;
	}

	public void OnApplyClicked()
	{
		if (m_Options != null)
		{
			for (int i = 0; i < 11; i++)
			{
				m_Options[i].OnApply();
			}
		}
		GlobalSave.GetInstance().RequestSave();
	}

	public void OnDefaultsClicked()
	{
		if (m_Options != null)
		{
			for (int i = 0; i < 11; i++)
			{
				m_Options[i].ResetToDefault();
			}
		}
	}

	public void OnValueChanged(GameObject theObject)
	{
		if (m_Options == null)
		{
			return;
		}
		for (int i = 0; i < 11; i++)
		{
			if (m_Options[i].IsUIObject(theObject))
			{
				m_Options[i].OnValueChanged();
			}
		}
	}

	public void ResetToInitialValues()
	{
		if (m_Options != null)
		{
			for (int i = 0; i < 11; i++)
			{
				m_Options[i].ResetToInitialValue();
			}
		}
	}

	private bool IsDirty()
	{
		if (m_Options != null)
		{
			for (int i = 0; i < 11; i++)
			{
				if (m_Options[i].isDirty)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void OnCancel(FrontendMenuBehaviour menu)
	{
		if (IsDirty())
		{
			T17DialogBox dialog2 = T17DialogBoxManager.GetDialog(forSingleUser: false);
			if (dialog2 != null)
			{
				dialog2.Initialize(hasConfirm: true, hasDecline: true, hasCancel: false, "Text.Menu.SaveChangesTitle", "Text.Menu.SaveChangesBody", "Text.Yes", "Text.No", string.Empty);
				dialog2.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog2.OnConfirm, (T17DialogBox.DialogEvent)delegate
				{
					OnApplyClicked();
					if (!m_ResolutionSelector.HasPendingChanges())
					{
						FrontEndFlow.Instance.SwitchBackToFrontendMenu(menu);
					}
					else
					{
						m_ResolutionSelector.SetBackingOut(menu);
					}
				});
				dialog2.OnDecline = (T17DialogBox.DialogEvent)Delegate.Combine(dialog2.OnDecline, (T17DialogBox.DialogEvent)delegate
				{
					ResetToInitialValues();
					FrontEndFlow.Instance.SwitchBackToFrontendMenu(menu);
				});
				dialog2.Show();
				return;
			}
		}
		FrontEndFlow.Instance.SwitchBackToFrontendMenu(menu);
	}

	public override void ConfirmChangeFocus(ConfirmFocusCallback confirmCallback)
	{
		if (IsDirty())
		{
			T17DialogBox dialog2 = T17DialogBoxManager.GetDialog(forSingleUser: false);
			if (dialog2 != null)
			{
				dialog2.Initialize(hasConfirm: true, hasDecline: true, hasCancel: false, "Text.Menu.SaveChangesTitle", "Text.Menu.SaveChangesBody", "Text.Yes", "Text.No", string.Empty);
				dialog2.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog2.OnConfirm, (T17DialogBox.DialogEvent)delegate
				{
					OnApplyClicked();
					confirmCallback(canChangeFocus: true);
				});
				dialog2.OnDecline = (T17DialogBox.DialogEvent)Delegate.Combine(dialog2.OnDecline, (T17DialogBox.DialogEvent)delegate
				{
					ResetToInitialValues();
					confirmCallback(canChangeFocus: true);
				});
				dialog2.Show();
			}
		}
		else
		{
			confirmCallback(canChangeFocus: true);
		}
	}
}
