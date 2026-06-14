using System.Collections.Generic;
using UnityEngine;

public class PrisonSetupMenu : FrontendMenuBehaviour, ICustomisableCharacters
{
	private class RT
	{
		public RenderTexture m_RenderTexture;

		public int m_ID;
	}

	public T17Button m_StartButton;

	[Header("PlayMode Settings")]
	public T17NetRoomGameView.GameRoomType m_CurrentPlaymode;

	public GameObject m_PlayModeContextMenu;

	public T17Text m_PlaymodeText;

	public T17Text[] m_PlaymodesText;

	public T17InputField m_PasswordField;

	private string m_RoomPassword = string.Empty;

	public T17Text m_PublicBodyTextConsole;

	public T17Text m_PublicBodyTextPC;

	private GlobalStart.GLOBALSTART_GAME_MODES m_GameMode;

	[Header("Customisation Settings")]
	public int m_CustomisationDialogIndex;

	public UI_DrawCharacterToRenderTexture m_CharacterRenderer;

	public T17ScrollView m_AvatarGrid;

	public T17Button[] m_Avatars = new T17Button[0];

	public T17Image m_CustomisationBackgroundImage;

	public T17Text m_PrisonerNamePreview;

	public NavigateOnUICancel m_BackButtonEvent;

	private Customisation[] m_Customisations = new Customisation[0];

	private RT[] m_RenderTextures;

	private int m_CurrentlySelectedIndex = -1;

	private int m_CustomisationToModifyIndex = -1;

	public FrontendPopup m_CentrePerksPopup;

	private bool m_bCentrePerksPopupShown;

	private Customisation m_TempCustomisation;

	[Header("Unlocks")]
	public GameObject m_UnlockPanel;

	public List<ProgressMilestone> m_PrisonMilestones = new List<ProgressMilestone>();

	public MilestoneDisplayObject.CriteriaDisplay[] m_CriteriaDisplayObjects = new MilestoneDisplayObject.CriteriaDisplay[0];

	protected override void Awake()
	{
		base.Awake();
		GlobalSave instance = GlobalSave.GetInstance();
		if (instance != null)
		{
			instance.Get("CentrePerksPopupShown", out m_bCentrePerksPopupShown, def: false);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		m_CustomisationBackgroundImage.sprite = null;
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		bool result = base.Show(currentGamer, parent, invoker, hideInvoker);
		m_TempCustomisation = new Customisation();
		SetupAvatarButtons();
		UpdateCustomisations();
		m_CharacterRenderer.RegisterActivationRequest(base.gameObject);
		SetupRenderTextures(m_Customisations.Length);
		DrawAllCharacters();
		UpdateSelectedNamePreview();
		SetPlaymodeUIText();
		SetUpUnlocks();
		m_PublicBodyTextConsole.gameObject.SetActive(value: false);
		m_PublicBodyTextPC.gameObject.SetActive(value: true);
		if (m_AvatarGrid != null)
		{
			m_AvatarGrid.Show(currentGamer, this, null, hideInvoker: false);
		}
		PrisonData currentSelectedPrisonData = GlobalStart.GetInstance().GetCurrentSelectedPrisonData();
		if (m_CustomisationBackgroundImage != null)
		{
			Sprite sprite = null;
			if (currentSelectedPrisonData != null && !string.IsNullOrEmpty(currentSelectedPrisonData.m_PrisonSetupImagePath))
			{
				sprite = Resources.Load<Sprite>(currentSelectedPrisonData.m_PrisonSetupImagePath);
			}
			if (sprite != null)
			{
				m_CustomisationBackgroundImage.sprite = sprite;
				m_CustomisationBackgroundImage.gameObject.SetActive(value: true);
			}
			else
			{
				m_CustomisationBackgroundImage.gameObject.SetActive(value: false);
			}
		}
		if (m_PlayModeContextMenu != null)
		{
			BaseContextMenu component = m_PlayModeContextMenu.GetComponent<BaseContextMenu>();
			if (component != null)
			{
				component.Hide();
			}
		}
		if (currentSelectedPrisonData != null && !m_bCentrePerksPopupShown)
		{
			LevelScript.PRISON_ENUM prisonEnum = currentSelectedPrisonData.m_LevelInfo.m_PrisonEnum;
			if (prisonEnum == LevelScript.PRISON_ENUM.Centre_Perks)
			{
				FrontEndFlow instance = FrontEndFlow.Instance;
				if (instance != null)
				{
					instance.OpenChildOnTopOfMenu(2);
					m_bCentrePerksPopupShown = true;
					GlobalSave instance2 = GlobalSave.GetInstance();
					if (instance2 != null)
					{
						instance2.Set("CentrePerksPopupShown", m_bCentrePerksPopupShown);
						instance2.RequestSave();
					}
				}
			}
		}
		return result;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		if (m_CurrentlySelectedIndex >= 0)
		{
			UnSelectCharacter(m_CurrentlySelectedIndex);
		}
		PrisonData currentSelectedPrisonData = GlobalStart.GetInstance().GetCurrentSelectedPrisonData();
		if (currentSelectedPrisonData != null)
		{
			CustomisationManager.GetInstance().OnPrisonCustomisationChanged(currentSelectedPrisonData);
		}
		m_CharacterRenderer.UnregisterActivationRequest(base.gameObject);
		DestroyAllRenderTextures();
		m_CurrentlySelectedIndex = -1;
		m_CustomisationToModifyIndex = -1;
		return true;
	}

	public void StartGamePressed()
	{
		PrisonData currentSelectedPrisonData = GlobalStart.GetInstance().GetCurrentSelectedPrisonData();
		if (currentSelectedPrisonData != null)
		{
			CustomisationManager.GetInstance().OnPrisonCustomisationChanged(currentSelectedPrisonData);
		}
		if (m_CurrentPlaymode == T17NetRoomGameView.GameRoomType.Private || m_CurrentPlaymode == T17NetRoomGameView.GameRoomType.Public)
		{
			Platform.GetInstance().EnterOnlineArea(bIsLeaderboard: false, null);
			NetGoOnlineHelper.GoOnline(showConnectionFailedPrompt: true, bProfanityCheck: false, delegate(bool isConnected)
			{
				if (isConnected)
				{
					m_GameMode = GlobalStart.GLOBALSTART_GAME_MODES.ONLINE;
					StartGame();
				}
				else
				{
					m_CurrentPlaymode = T17NetRoomGameView.GameRoomType.Offline;
					SetPlaymodeUIText();
					Platform.GetInstance().ExitOnlineArea();
				}
			});
		}
		else
		{
			T17NetManager.CanDisplayDisconnectionDialog = false;
			m_GameMode = GlobalStart.GLOBALSTART_GAME_MODES.SINGLE;
			StartGame();
		}
	}

	private void StartGame()
	{
		PrisonConfig.ConfigType currentSelectedConfigType = GlobalStart.GetInstance().GetCurrentSelectedConfigType();
		NetCreateRoomHelper.RequestCreateRoom(m_CurrentPlaymode, currentSelectedConfigType, delegate(bool roomSetupOk)
		{
			if (roomSetupOk)
			{
				GlobalStart.GetInstance().StartGameWithModeAndCurrentConfig(m_GameMode);
				PrisonData currentSelectedPrisonData = GlobalStart.GetInstance().GetCurrentSelectedPrisonData();
				if (currentSelectedPrisonData.m_LevelInfo != null)
				{
					if (currentSelectedPrisonData.m_LevelInfo.m_PrisonEnum != LevelScript.PRISON_ENUM.Tutorial)
					{
						GoogleAnalyticsV3.LogCommericalAnalyticEvent("Campaign Prison Starts", currentSelectedPrisonData.m_LevelInfo.m_PrisonEnum.ToString() + " Started", string.Empty, 0L);
						GoogleAnalyticsV3.LogCommericalAnalyticEvent("Co-op Game Started", m_CurrentPlaymode.ToString() + " Started", string.Empty, 0L);
					}
					else
					{
						GoogleAnalyticsV3.LogCommericalAnalyticEvent("Tutorial", "Tutorial Started", string.Empty, 0L);
					}
				}
			}
			else
			{
				T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
				dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Dialog.NetworkError", "Text.Dialog.NetworkFailedToConnect", "Text.Dialog.Prompt.Ok", string.Empty, string.Empty);
				dialog.SetSymbol(T17DialogBox.Symbols.Error);
				dialog.Show();
			}
		}, showDialogs: true, m_RoomPassword);
	}

	protected override void Update()
	{
		if (BaseMenuBehaviour.LastMenuThatCalledShow == this && !T17DialogBoxManager.HasAnyOpenDialogs())
		{
			if (m_CurrentEventSystem == null)
			{
				m_CurrentEventSystem = T17EventSystemsManager.Instance.GetEventSystemForGamer(base.CurrentGamer);
			}
			if (m_CurrentEventSystem != null && (m_CurrentEventSystem.currentSelectedGameObject == null || !m_CurrentEventSystem.currentSelectedGameObject.activeInHierarchy))
			{
				if (m_PlayModeContextMenu.gameObject.activeInHierarchy)
				{
					BaseContextMenu component = m_PlayModeContextMenu.GetComponent<BaseContextMenu>();
					if (component != null && component.m_Items.Length > 0)
					{
						m_CurrentEventSystem.SetSelectedGameObject(null);
						m_CurrentEventSystem.SetSelectedGameObject(component.m_Items[0].gameObject);
					}
				}
				else if (m_TopSelectable != null && m_bSelectTopElementOnShow)
				{
					m_CurrentEventSystem.SetSelectedGameObject(null);
					m_CurrentEventSystem.SetSelectedGameObject(m_TopSelectable.gameObject);
				}
			}
		}
		base.Update();
		if (m_CurrentlySelectedIndex >= 0)
		{
			int num = Mathf.Min(m_RenderTextures.Length, m_Customisations.Length);
			if (m_CurrentlySelectedIndex < num)
			{
				Customisation other = m_Customisations[m_CurrentlySelectedIndex];
				RenderTexture renderTexture = m_RenderTextures[m_CurrentlySelectedIndex].m_RenderTexture;
				m_CharacterRenderer.UpdateAnimation(Time.deltaTime);
				m_TempCustomisation.DuplicateCustomisation(other);
				OverrideCustomisation(ref m_TempCustomisation, m_CurrentlySelectedIndex);
				DrawCharacter(m_TempCustomisation, renderTexture, bIsHighlighted: true);
			}
		}
	}

	private void UnSelectCharacter(int index)
	{
		Customisation other = m_Customisations[index];
		RenderTexture renderTexture = m_RenderTextures[index].m_RenderTexture;
		Customisation customisation = new Customisation(other);
		OverrideCustomisation(ref customisation, index);
		DrawCharacter(customisation, renderTexture, bIsHighlighted: false);
	}

	private void DrawCharacter(Customisation customisation, RenderTexture texture, bool bIsHighlighted)
	{
		m_CharacterRenderer.SetCustomisation(customisation);
		m_CharacterRenderer.SetHighlighted(bIsHighlighted);
		m_CharacterRenderer.DrawCharacter(texture);
	}

	private void DrawAllCharacters()
	{
		int num = Mathf.Min(m_RenderTextures.Length, m_Customisations.Length);
		for (int i = 0; i < num; i++)
		{
			m_TempCustomisation.DuplicateCustomisation(m_Customisations[i]);
			OverrideCustomisation(ref m_TempCustomisation, i);
			DrawCharacter(m_TempCustomisation, m_RenderTextures[i].m_RenderTexture, bIsHighlighted: false);
		}
	}

	private void UpdateCustomisations()
	{
		PrisonData currentSelectedPrisonData = GlobalStart.GetInstance().GetCurrentSelectedPrisonData();
		if (currentSelectedPrisonData != null)
		{
			m_Customisations = CustomisationManager.GetInstance().GetCustomisableNpcsForPrison(currentSelectedPrisonData, generateIfNotFound: true, currentSelectedPrisonData.m_LevelInfo.m_PrisonEnum == LevelScript.PRISON_ENUM.CustomPrison);
		}
	}

	private void SetupRenderTextures(int requested)
	{
		DestroyAllRenderTextures();
		m_RenderTextures = new RT[m_Avatars.Length];
		for (int i = 0; i < m_Avatars.Length; i++)
		{
			m_RenderTextures[i] = new RT();
			m_Avatars[i].gameObject.SetActive(i < requested);
			if (i < requested)
			{
				T17RawImage componentInChildren = m_Avatars[i].GetComponentInChildren<T17RawImage>();
				int num = Mathf.FloorToInt(componentInChildren.rectTransform.rect.width);
				int num2 = Mathf.FloorToInt(componentInChildren.rectTransform.rect.height);
				if (num > 0 && num2 > 0)
				{
					m_RenderTextures[i].m_RenderTexture = m_CharacterRenderer.CreateRenderTexture(num, num2, ref m_RenderTextures[i].m_ID);
				}
				componentInChildren.texture = m_RenderTextures[i].m_RenderTexture;
			}
		}
	}

	private void DestroyAllRenderTextures()
	{
		if (m_RenderTextures != null && m_RenderTextures.Length > 0)
		{
			for (int i = 0; i < m_RenderTextures.Length; i++)
			{
				if (m_RenderTextures[i].m_RenderTexture != null)
				{
					m_CharacterRenderer.CleanupRenderTexture(ref m_RenderTextures[i].m_ID);
				}
			}
		}
		for (int j = 0; j < m_Avatars.Length; j++)
		{
			T17RawImage componentInChildren = m_Avatars[j].GetComponentInChildren<T17RawImage>();
			componentInChildren.texture = null;
		}
	}

	private void SetupAvatarButtons()
	{
		for (int i = 0; i < m_Avatars.Length; i++)
		{
			int buttonIndex = i;
			T17Button t17Button = m_Avatars[i];
			t17Button.onClick.RemoveAllListeners();
			t17Button.onClick.AddListener(delegate
			{
				OnAvatarClicked(buttonIndex);
			});
			T17_UISelectDeselectEvents component = t17Button.GetComponent<T17_UISelectDeselectEvents>();
			if (component != null)
			{
				component.m_OnSelectEvent.AddListener(delegate
				{
					OnAvatarSelected(buttonIndex);
				});
				component.m_OnDeselectEvent.AddListener(delegate
				{
					OnAvatarDeselected(buttonIndex);
				});
			}
			T17_UIPointerOverExitEvents t17_UIPointerOverExitEvents = t17Button.GetComponent<T17_UIPointerOverExitEvents>();
			if (t17_UIPointerOverExitEvents == null)
			{
				t17_UIPointerOverExitEvents = t17Button.gameObject.AddComponent<T17_UIPointerOverExitEvents>();
			}
			if (t17_UIPointerOverExitEvents != null)
			{
				t17_UIPointerOverExitEvents.m_OnPointerEnterEvent.AddListener(delegate
				{
					OnAvatarSelected(buttonIndex);
				});
				t17_UIPointerOverExitEvents.m_OnPointerExitEvent.AddListener(delegate
				{
					OnAvatarDeselected(buttonIndex);
				});
			}
		}
	}

	private void OnAvatarClicked(int index)
	{
		if (index >= 0 && index < m_Customisations.Length)
		{
			m_CustomisationToModifyIndex = index;
		}
		if (FrontEndFlow.Instance != null)
		{
			FrontEndFlow.Instance.OpenChildOnTopOfMenu(m_CustomisationDialogIndex);
		}
	}

	public void OnAvatarSelected(int index)
	{
		if (m_CurrentlySelectedIndex > -1)
		{
			UnSelectCharacter(m_CurrentlySelectedIndex);
		}
		m_CurrentlySelectedIndex = index;
		UpdateSelectedNamePreview();
	}

	public void OnAvatarDeselected(int index)
	{
		if (m_CurrentlySelectedIndex == index)
		{
			UnSelectCharacter(m_CurrentlySelectedIndex);
			m_CurrentlySelectedIndex = -1;
			UpdateSelectedNamePreview();
		}
	}

	private void UpdateSelectedNamePreview()
	{
		if (m_PrisonerNamePreview != null)
		{
			if (m_CurrentlySelectedIndex >= 0 && m_CurrentlySelectedIndex < m_Customisations.Length)
			{
				m_PrisonerNamePreview.text = m_Customisations[m_CurrentlySelectedIndex].name;
			}
			else
			{
				m_PrisonerNamePreview.text = string.Empty;
			}
		}
	}

	public void OnRandomise()
	{
		PrisonData currentSelectedPrisonData = GlobalStart.GetInstance().GetCurrentSelectedPrisonData();
		if (currentSelectedPrisonData != null)
		{
			bool flag = CustomisationManager.GetInstance().RandomiseCustomisations(currentSelectedPrisonData);
			CustomisationManager.GetInstance().InsertRandomInfluencers(currentSelectedPrisonData);
			if (flag)
			{
				DrawAllCharacters();
			}
		}
	}

	public int GetCurrentCustomsiationIndex()
	{
		return m_CustomisationToModifyIndex;
	}

	public Customisation GetCustomisationToModify()
	{
		Customisation result = null;
		if (m_CustomisationToModifyIndex >= 0 && m_CustomisationToModifyIndex < m_Customisations.Length)
		{
			result = m_Customisations[m_CustomisationToModifyIndex];
		}
		return result;
	}

	public CustomisationConstraint GetCustomisationConstraint()
	{
		CustomisationConstraint result = null;
		if (m_CustomisationToModifyIndex >= 0 && m_CustomisationToModifyIndex < m_Customisations.Length)
		{
			PrisonData currentSelectedPrisonData = GlobalStart.GetInstance().GetCurrentSelectedPrisonData();
			if (currentSelectedPrisonData != null)
			{
				int num = m_CustomisationToModifyIndex;
				for (int i = 0; i < currentSelectedPrisonData.m_CustomisableRoles.Length; i++)
				{
					num -= currentSelectedPrisonData.m_CustomisableRoles[i];
					if (num < 0)
					{
						result = currentSelectedPrisonData.m_CustomisationConstraints[i];
						break;
					}
				}
			}
		}
		return result;
	}

	public void OnCustomisationModified()
	{
		if (m_CustomisationToModifyIndex >= 0 && m_CustomisationToModifyIndex < m_Customisations.Length)
		{
			m_TempCustomisation.DuplicateCustomisation(m_Customisations[m_CustomisationToModifyIndex]);
			OverrideCustomisation(ref m_TempCustomisation, m_CustomisationToModifyIndex);
			DrawCharacter(m_TempCustomisation, m_RenderTextures[m_CustomisationToModifyIndex].m_RenderTexture, bIsHighlighted: false);
		}
	}

	public void OnPlaymode()
	{
		if (!(m_PlayModeContextMenu != null))
		{
			return;
		}
		BaseContextMenu component = m_PlayModeContextMenu.GetComponent<BaseContextMenu>();
		if (component != null)
		{
			m_NavigateOnUICancel = m_BackButtonEvent;
			if (component.isContextMenuOpen)
			{
				component.Hide();
				return;
			}
			component.ShowContextMenu((int)m_CurrentPlaymode);
			m_PasswordField.text = m_RoomPassword;
		}
	}

	public void OnPlaymodeContextItemClicked(int playmodeIndex)
	{
		if (m_PlayModeContextMenu != null)
		{
			BaseContextMenu component = m_PlayModeContextMenu.GetComponent<BaseContextMenu>();
			if (component != null)
			{
				component.Hide();
			}
		}
		m_NavigateOnUICancel = GetComponent<NavigateOnUICancel>();
		m_CurrentPlaymode = (T17NetRoomGameView.GameRoomType)playmodeIndex;
		SetPlaymodeUIText();
		if (m_PasswordField != null)
		{
			if (m_CurrentPlaymode != T17NetRoomGameView.GameRoomType.Public)
			{
				m_RoomPassword = string.Empty;
			}
			else
			{
				m_RoomPassword = m_PasswordField.text;
			}
		}
	}

	public void OnPlaymodeContextItemCancelled()
	{
		m_NavigateOnUICancel = GetComponent<NavigateOnUICancel>();
	}

	public void OnBackButtonPressed()
	{
		if (!(m_PlayModeContextMenu != null))
		{
			return;
		}
		BaseContextMenu component = m_PlayModeContextMenu.GetComponent<BaseContextMenu>();
		if (!(component != null) || !component.isContextMenuOpen)
		{
			return;
		}
		component.Hide();
		m_NavigateOnUICancel = GetComponent<NavigateOnUICancel>();
		if (m_PasswordField != null)
		{
			if (m_CurrentPlaymode != T17NetRoomGameView.GameRoomType.Public)
			{
				m_PasswordField.text = string.Empty;
			}
			else
			{
				m_PasswordField.text = m_RoomPassword;
			}
		}
	}

	private void SetPlaymodeUIText()
	{
		if (m_PlaymodeText != null)
		{
			int currentPlaymode = (int)m_CurrentPlaymode;
			if (m_PlaymodesText != null && currentPlaymode < m_PlaymodesText.Length)
			{
				m_PlaymodeText.SetNewLocalizationTag(m_PlaymodesText[currentPlaymode].m_LocalizationTag);
			}
		}
	}

	public void SwitchOutOnCancel(FrontendMenuBehaviour menu)
	{
		if (menu != null && m_NavigateOnUICancel != null)
		{
			m_NavigateOnUICancel.m_DoThisOnUICancel.RemoveAllListeners();
			m_NavigateOnUICancel.m_DoThisOnUICancel.AddListener(delegate
			{
				FrontEndFlow.Instance.SwitchBackToFrontendMenu(menu);
			});
		}
	}

	private void OverrideCustomisation(ref Customisation customisation, int index)
	{
		PrisonData currentSelectedPrisonData = GlobalStart.GetInstance().GetCurrentSelectedPrisonData();
		if (!(currentSelectedPrisonData != null) || index < 0 || index >= currentSelectedPrisonData.m_RoleStartingOutfitData.Length)
		{
			return;
		}
		Item_Outfit item_Outfit = null;
		if (!(currentSelectedPrisonData.m_RoleStartingOutfitData[index] != null))
		{
			return;
		}
		item_Outfit = currentSelectedPrisonData.m_RoleStartingOutfitData[index].m_OutfitData;
		List<ItemDataConfig> list = null;
		for (int i = 0; i < currentSelectedPrisonData.m_Configs.Count; i++)
		{
			if (currentSelectedPrisonData.m_Configs[i].m_ConfigType == PrisonConfig.ConfigType.Cooperative)
			{
				list = currentSelectedPrisonData.m_Configs[i].m_ItemDataOverrides;
			}
		}
		if (list != null)
		{
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j].m_ItemDataID == currentSelectedPrisonData.m_RoleStartingOutfitData[index].m_ItemDataID)
				{
					item_Outfit.m_HairOverride = list[j].m_HairOverride;
					item_Outfit.m_HatOverride = list[j].m_HatOverride;
					item_Outfit.m_LowerFaceOverride = list[j].m_LowerFaceOverride;
					item_Outfit.m_OutfitAppearance = list[j].m_OutfitAppearance;
					item_Outfit.m_UpperFaceOverride = list[j].m_UpperFaceOverride;
				}
			}
		}
		if (item_Outfit.m_HairOverride != CustomisationData.Hair.NULL)
		{
			customisation.hair = item_Outfit.m_HairOverride;
		}
		if (item_Outfit.m_HatOverride != CustomisationData.Hat.NULL)
		{
			customisation.hat = item_Outfit.m_HatOverride;
		}
		if (item_Outfit.m_LowerFaceOverride != CustomisationData.LowerFaceAccessory.NULL)
		{
			customisation.lowerFace = item_Outfit.m_LowerFaceOverride;
		}
		if (item_Outfit.m_OutfitAppearance != CustomisationData.Outfit.NULL)
		{
			customisation.defaultOutfit = item_Outfit.m_OutfitAppearance;
		}
		if (item_Outfit.m_UpperFaceOverride != CustomisationData.UpperFaceAccessory.NULL)
		{
			customisation.upperFace = item_Outfit.m_UpperFaceOverride;
		}
	}

	private void SetUpUnlocks()
	{
		PrisonData prisonData = GlobalStart.GetInstance().GetCurrentSelectedPrisonData();
		if (!(prisonData != null) || prisonData.m_LevelInfo == null)
		{
			return;
		}
		ProgressMilestone progressMilestone = m_PrisonMilestones.Find((ProgressMilestone x) => x != null && x.m_prison == prisonData.m_LevelInfo.m_PrisonEnum);
		if (progressMilestone != null)
		{
			m_UnlockPanel.SetActive(value: true);
			int num = progressMilestone.criteria.Length;
			bool[] ruleStatuses = new bool[num];
			float[] statValues = new float[num];
			float[] refValues = new float[num];
			StatSystem instance = StatSystem.GetInstance();
			if (instance != null)
			{
				instance.GetProgressDataForMilestone(progressMilestone.id, ref ruleStatuses, ref statValues, ref refValues);
			}
			for (int i = 0; i < m_CriteriaDisplayObjects.Length; i++)
			{
				MilestoneDisplayObject.CriteriaDisplay criteriaDisplay = m_CriteriaDisplayObjects[i];
				if (i >= num)
				{
					criteriaDisplay.parent.gameObject.SetActive(value: false);
					continue;
				}
				ProgressMilestone.Criteria criteria = progressMilestone.criteria[i];
				criteriaDisplay.parent.gameObject.SetActive(value: true);
				if (criteriaDisplay.description != null)
				{
					criteriaDisplay.description.SetLocalisedTextCatchAll(criteria.descriptionKey);
				}
				if (criteriaDisplay.tick != null)
				{
					criteriaDisplay.tick.enabled = ruleStatuses[i];
				}
			}
		}
		else if (m_UnlockPanel != null)
		{
			m_UnlockPanel.SetActive(value: false);
		}
	}
}
