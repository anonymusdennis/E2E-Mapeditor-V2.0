using Rewired;
using UnityEngine;

public class LevelEditor_PrisonSettingsDialog : MonoBehaviour
{
	public T17RawImage m_PreviewImage;

	public T17InputField m_TitleInputField;

	public T17InputField m_DescriptionInputField;

	public T17Button m_TitleInputButton;

	public T17Button m_DescriptionInputButton;

	public T17Text m_TitleInputText;

	public T17Text m_DescriptionInputText;

	public T17TabPanel m_SettingsTabPanel;

	public T17Text m_DifficultyTitle;

	public T17Text m_DifficultyDescription;

	public T17Text[] m_SpawnGroupTitles = new T17Text[5];

	public T17Text m_MusicDescription;

	public T17Text m_OutfitDescription;

	public LevelEditor_Settings m_EditorSettings;

	private Rewired.Player m_Player;

	private LevelDetailsManager m_LevelDetailsMan;

	private LevelEditor_Controller m_LevelEditorController;

	private void Awake()
	{
		if (m_TitleInputField != null)
		{
			if (m_TitleInputButton == null)
			{
				m_TitleInputButton = m_TitleInputField.GetComponentInParent<T17Button>();
			}
			if (m_TitleInputText == null)
			{
				m_TitleInputText = m_TitleInputField.GetComponent<T17Text>();
			}
			m_TitleInputField.onEndEdit.AddListener(OnPrisonTitleChanged);
		}
		if (m_DescriptionInputField != null)
		{
			if (m_DescriptionInputButton == null)
			{
				m_DescriptionInputButton = m_DescriptionInputField.GetComponentInParent<T17Button>();
			}
			if (m_DescriptionInputText == null)
			{
				m_DescriptionInputText = m_DescriptionInputField.GetComponent<T17Text>();
			}
			m_DescriptionInputField.onEndEdit.AddListener(OnPrisionDescriptionChanged);
		}
		if (m_TitleInputButton != null)
		{
			m_TitleInputButton.m_CanUIReselectDelegate = m_TitleInputField.CanReselectOnMouseDisable;
			m_TitleInputButton.m_ReleaseOnPointerClickDelegate = m_TitleInputField.ReleaseSelectionOnPointerClickOrExit;
		}
		if (m_TitleInputText != null)
		{
			m_TitleInputText.m_ReleaseOnPointerClickDelegate = m_TitleInputField.ReleaseSelectionOnPointerClickOrExit;
		}
		if (m_DescriptionInputButton != null)
		{
			m_DescriptionInputButton.m_CanUIReselectDelegate = m_DescriptionInputField.CanReselectOnMouseDisable;
			m_DescriptionInputButton.m_ReleaseOnPointerClickDelegate = m_DescriptionInputField.ReleaseSelectionOnPointerClickOrExit;
		}
		if (m_DescriptionInputText != null)
		{
			m_DescriptionInputText.m_ReleaseOnPointerClickDelegate = m_DescriptionInputField.ReleaseSelectionOnPointerClickOrExit;
		}
		m_LevelEditorController = LevelEditor_Controller.GetInstance();
		if (m_LevelEditorController != null)
		{
			m_LevelEditorController.RegisterOnSnapshotDelegate(UpdatePreviewImage);
		}
	}

	private void Update()
	{
		if (m_LevelDetailsMan == null)
		{
			m_LevelDetailsMan = LevelDetailsManager.GetInstance();
		}
		if (m_Player != null && m_Player.GetButtonUp("UI_Close"))
		{
			Hide();
		}
	}

	public void OnPrisonTitleChanged(string strTitle)
	{
		if (m_LevelDetailsMan != null)
		{
			string text = m_LevelDetailsMan.SetLevelName(strTitle);
			m_TitleInputText.SetNonLocalizedText(text);
			m_TitleInputField.text = text;
			if (LevelEditor_UIController.GetInstance() != null)
			{
				LevelEditor_UIController.GetInstance().UpdateWorldTextItems();
			}
		}
	}

	public void OnPrisionDescriptionChanged(string strDescription)
	{
		if (m_LevelDetailsMan != null)
		{
			string text = m_LevelDetailsMan.SetLevelDecription(strDescription);
			m_DescriptionInputText.SetNonLocalizedText(text);
			m_DescriptionInputField.text = text;
			if (LevelEditor_UIController.GetInstance() != null)
			{
				LevelEditor_UIController.GetInstance().UpdateWorldTextItems();
			}
		}
	}

	public void OnPreviewImageCaptureClicked()
	{
		if (m_LevelEditorController != null)
		{
			m_LevelEditorController.TriggerSnapshot();
		}
	}

	public void OnDifficultyButtonClicked(int iDirection)
	{
		if (m_LevelDetailsMan != null)
		{
			LevelDetailsManager.DiffecultyLevel levelDifficulty = m_LevelDetailsMan.GetLevelDifficulty();
			levelDifficulty = (LevelDetailsManager.DiffecultyLevel)((int)levelDifficulty + iDirection);
			if (levelDifficulty < LevelDetailsManager.DiffecultyLevel.Easy)
			{
				levelDifficulty = LevelDetailsManager.DiffecultyLevel.Hard;
			}
			else if (levelDifficulty > LevelDetailsManager.DiffecultyLevel.Hard)
			{
				levelDifficulty = LevelDetailsManager.DiffecultyLevel.Easy;
			}
			m_LevelDetailsMan.SetLevelDifficulty(levelDifficulty);
			if (m_DifficultyTitle != null && m_EditorSettings != null)
			{
				m_DifficultyTitle.SetLocalisedTextCatchAll(m_EditorSettings.m_DifficultySettings[(int)levelDifficulty].Name);
			}
			if (m_DifficultyDescription != null && m_EditorSettings != null)
			{
				m_DifficultyDescription.SetLocalisedTextCatchAll(m_EditorSettings.m_DifficultySettings[(int)levelDifficulty].Description);
			}
		}
	}

	public void OnSpawnGroupButtonLeftClicked(int iButtonIndex)
	{
		if (iButtonIndex >= 0 && iButtonIndex < 5 && m_LevelDetailsMan != null && m_EditorSettings != null)
		{
			int randomItemGroup = m_LevelDetailsMan.GetRandomItemGroup(iButtonIndex);
			randomItemGroup--;
			if (randomItemGroup < 0)
			{
				randomItemGroup = m_EditorSettings.m_ItemGroupSettingsList.Count - 1;
			}
			m_LevelDetailsMan.SetRandomItemGroup(iButtonIndex, randomItemGroup);
			if (m_SpawnGroupTitles[iButtonIndex] != null)
			{
				m_SpawnGroupTitles[iButtonIndex].SetLocalisedTextCatchAll(m_EditorSettings.m_ItemGroupSettingsList[randomItemGroup].Name);
			}
		}
	}

	public void OnSpawnGroupButtonRightClicked(int iButtonIndex)
	{
		if (iButtonIndex >= 0 && iButtonIndex < 5 && m_LevelDetailsMan != null && m_EditorSettings != null)
		{
			int randomItemGroup = m_LevelDetailsMan.GetRandomItemGroup(iButtonIndex);
			randomItemGroup++;
			if (randomItemGroup >= m_EditorSettings.m_ItemGroupSettingsList.Count)
			{
				randomItemGroup = 0;
			}
			m_LevelDetailsMan.SetRandomItemGroup(iButtonIndex, randomItemGroup);
			if (m_SpawnGroupTitles[iButtonIndex] != null)
			{
				m_SpawnGroupTitles[iButtonIndex].SetLocalisedTextCatchAll(m_EditorSettings.m_ItemGroupSettingsList[randomItemGroup].Name);
			}
		}
	}

	public void Show(int iTabIndex)
	{
		base.gameObject.SetActive(value: true);
		if (m_LevelDetailsMan == null)
		{
			m_LevelDetailsMan = LevelDetailsManager.GetInstance();
		}
		if (m_Player == null)
		{
			Gamer primaryGamer = Gamer.GetPrimaryGamer();
			if (primaryGamer != null)
			{
				m_Player = primaryGamer.m_RewiredPlayer;
			}
		}
		if (m_Player != null)
		{
			T17EventSystem.InputCateogryStates stateForRewiredPlayer = T17EventSystem.GetStateForRewiredPlayer(m_Player);
			if (stateForRewiredPlayer != T17EventSystem.InputCateogryStates.Dialogbox)
			{
				T17EventSystem.ApplyCategories(m_Player, T17EventSystem.InputCateogryStates.Dialogbox);
			}
		}
		if (m_LevelDetailsMan != null)
		{
			if (m_TitleInputField != null)
			{
				m_TitleInputField.text = m_LevelDetailsMan.GetLevelName();
			}
			if (m_DescriptionInputField != null)
			{
				m_DescriptionInputField.text = m_LevelDetailsMan.GetLevelDecription();
			}
			if (m_EditorSettings != null)
			{
				LevelDetailsManager.DiffecultyLevel levelDifficulty = m_LevelDetailsMan.GetLevelDifficulty();
				if (m_DifficultyTitle != null)
				{
					m_DifficultyTitle.SetLocalisedTextCatchAll(m_EditorSettings.m_DifficultySettings[(int)levelDifficulty].Name);
				}
				if (m_DifficultyDescription != null && m_EditorSettings != null)
				{
					m_DifficultyDescription.SetLocalisedTextCatchAll(m_EditorSettings.m_DifficultySettings[(int)levelDifficulty].Description);
				}
				for (int i = 0; i < 5; i++)
				{
					if (m_SpawnGroupTitles[i] != null)
					{
						int randomItemGroup = m_LevelDetailsMan.GetRandomItemGroup(i);
						m_SpawnGroupTitles[i].SetLocalisedTextCatchAll(m_EditorSettings.m_ItemGroupSettingsList[randomItemGroup].Name);
					}
				}
			}
		}
		if (m_SettingsTabPanel != null)
		{
			m_SettingsTabPanel.Show(Gamer.GetPrimaryGamer(), null, null, hideInvoker: false);
			m_SettingsTabPanel.SetTabIndex(iTabIndex);
		}
		if (m_LevelEditorController != null && m_PreviewImage != null)
		{
			m_PreviewImage.texture = m_LevelEditorController.GetPreviewSnapShot();
		}
		UpdateMusic();
		UpdateOutfit();
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
		if (m_Player != null)
		{
			T17EventSystem.ApplyLastRequestedStateSinceDialogboxWasUp(m_Player);
			T17EventSystem.InputCateogryStates stateForRewiredPlayer = T17EventSystem.GetStateForRewiredPlayer(m_Player);
			if (stateForRewiredPlayer == T17EventSystem.InputCateogryStates.InputField)
			{
				T17EventSystem.ApplyCategories(m_Player, T17EventSystem.InputCateogryStates.LevelEditor);
			}
		}
	}

	public void UpdatePreviewImage(Texture2D previewTexture)
	{
		if (previewTexture != null && m_PreviewImage != null)
		{
			m_PreviewImage.texture = previewTexture;
		}
	}

	public void OnMusicLeft()
	{
		if (m_LevelDetailsMan != null || (m_LevelDetailsMan = LevelDetailsManager.GetInstance()) != null)
		{
			if (m_LevelDetailsMan.GetMusicType() == LevelScript.PRISON_ENUM.Centre_Perks)
			{
				m_LevelDetailsMan.SetMusicType(LevelScript.PRISON_ENUM.OldWestFort);
			}
			else
			{
				m_LevelDetailsMan.SetMusicType(LevelScript.PRISON_ENUM.Centre_Perks);
			}
			UpdateMusic();
		}
	}

	public void OnMusicRight()
	{
		if (m_LevelDetailsMan != null || (m_LevelDetailsMan = LevelDetailsManager.GetInstance()) != null)
		{
			if (m_LevelDetailsMan.GetMusicType() == LevelScript.PRISON_ENUM.Centre_Perks)
			{
				m_LevelDetailsMan.SetMusicType(LevelScript.PRISON_ENUM.OldWestFort);
			}
			else
			{
				m_LevelDetailsMan.SetMusicType(LevelScript.PRISON_ENUM.Centre_Perks);
			}
			UpdateMusic();
		}
	}

	public void UpdateMusic()
	{
		if (m_MusicDescription != null && (m_LevelDetailsMan != null || (m_LevelDetailsMan = LevelDetailsManager.GetInstance()) != null))
		{
			string prisonString = m_LevelDetailsMan.GetPrisonString(m_LevelDetailsMan.GetMusicType());
			m_MusicDescription.SetLocalisedTextCatchAll(prisonString);
		}
	}

	public void OnOutFitLeft()
	{
		if (m_LevelDetailsMan != null || (m_LevelDetailsMan = LevelDetailsManager.GetInstance()) != null)
		{
			if (m_LevelDetailsMan.GetOutfitType() == LevelScript.PRISON_ENUM.Centre_Perks)
			{
				m_LevelDetailsMan.SetOutfitType(LevelScript.PRISON_ENUM.OldWestFort);
			}
			else
			{
				m_LevelDetailsMan.SetOutfitType(LevelScript.PRISON_ENUM.Centre_Perks);
			}
			UpdateOutfit();
		}
	}

	public void OnOutfitRight()
	{
		if (m_LevelDetailsMan != null || (m_LevelDetailsMan = LevelDetailsManager.GetInstance()) != null)
		{
			if (m_LevelDetailsMan.GetOutfitType() == LevelScript.PRISON_ENUM.Centre_Perks)
			{
				m_LevelDetailsMan.SetOutfitType(LevelScript.PRISON_ENUM.OldWestFort);
			}
			else
			{
				m_LevelDetailsMan.SetOutfitType(LevelScript.PRISON_ENUM.Centre_Perks);
			}
			UpdateOutfit();
		}
	}

	public void UpdateOutfit()
	{
		if (m_OutfitDescription != null && (m_LevelDetailsMan != null || (m_LevelDetailsMan = LevelDetailsManager.GetInstance()) != null))
		{
			string prisonString = m_LevelDetailsMan.GetPrisonString(m_LevelDetailsMan.GetOutfitType());
			m_OutfitDescription.SetLocalisedTextCatchAll(prisonString);
		}
	}
}
