using System;
using System.Collections.Generic;
using T17.UI.Carousel;
using UnityEngine;

public class EditorSubscribedMenu : FrontendMenuBehaviour
{
	public PlaylistDataCarousel m_PlaylistCarousel;

	public EditorRootMenu m_RootMenu;

	public SaveSlotController m_ContinueButton;

	public SaveSlotController m_NewGameButton;

	public SaveSlotController m_LoadGameButton;

	public Texture2D m_DefaultLevelImage;

	public ItemData m_DefaultInmateOutfit;

	public ItemData m_DefaultGuardOutfit;

	public ItemData m_DefaultRiotOutfit;

	public T17Button m_UnsubscribeButton;

	protected override void Awake()
	{
		base.Awake();
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		FrontendUserPath.RecordFrontendPath(FrontEndFlow.MenuType.LevelEditor, 3, 0);
		FrontEndFlow.Instance.EditorSetPrisonSetupMenuOnCancel(bMyPrisonMenu: false);
		if (Gamer.GetPrimaryGamer() != null && Gamer.GetPrimaryGamer().m_RewiredPlayer != null)
		{
			T17EventSystem.ApplyCategories(Gamer.GetPrimaryGamer().m_RewiredPlayer, T17EventSystem.InputCateogryStates.MainFrontend);
		}
		GenerateCustomPrisonData();
		m_PlaylistCarousel.IndexSelectedEvent += PlaylistCarousel_IndexSelectedEvent;
		if (m_PlaylistCarousel != null && m_PlaylistCarousel.GetNumOptions() != 0)
		{
			GlobalStart.GetInstance().SetSelectedPlaylist(m_PlaylistCarousel.GetSelectedItem());
		}
		UpdateButtonStatus();
		Platform.GetInstance().RegisterOnUGCChange(OnUGCUpdated);
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		if (m_PlaylistCarousel != null)
		{
			m_PlaylistCarousel.IndexSelectedEvent -= PlaylistCarousel_IndexSelectedEvent;
		}
		Platform.GetInstance().UnRegisterOnUGCChange(OnUGCUpdated);
		return true;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (m_PlaylistCarousel != null)
		{
			m_PlaylistCarousel.IndexSelectedEvent -= PlaylistCarousel_IndexSelectedEvent;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (base.CurrentGamer != null && base.CurrentGamer.m_RewiredPlayer != null && !FrontEndFlow.Instance.m_MainMenu.IsChildMenuOpen() && !T17DialogBoxManager.HasAnyOpenDialogs())
		{
			if (base.CurrentGamer.m_RewiredPlayer.GetButtonDown("UI_CycleLeft"))
			{
				m_PlaylistCarousel.SelectPrevious();
			}
			if (base.CurrentGamer.m_RewiredPlayer.GetButtonDown("UI_CycleRight"))
			{
				m_PlaylistCarousel.SelectNext();
			}
		}
	}

	public void GenerateCustomPrisonData()
	{
		LevelDataManager instance = LevelDataManager.GetInstance();
		SaveManager instance2 = SaveManager.GetInstance();
		if (instance != null && instance2 != null && m_PlaylistCarousel != null)
		{
			instance.ClearCustomLevelPlaylists();
			m_PlaylistCarousel.m_Options.Clear();
			List<SaveManager.PrisonsSaveInformation.PrisonData> customPrisons = new List<SaveManager.PrisonsSaveInformation.PrisonData>();
			instance2.EnumerateCustomPrisons(ref customPrisons, bUGC: true);
			List<Platform.UGCItem> ugcList = new List<Platform.UGCItem>();
			Platform.GetInstance().EnumerateUGCItems(ref ugcList, Platform.UGCType.eCustomLevel);
			EC2AnalyticsHelper.UpdateNumberOfSubscribedPrisons(ugcList);
			int count = customPrisons.Count;
			for (int i = 0; i < count; i++)
			{
				SaveManager.PrisonsSaveInformation.PrisonData prisonData = customPrisons[i];
				Platform.UGCItem uGCItem = null;
				ulong ugcID = ulong.Parse(prisonData.m_strPrisonName);
				uGCItem = ugcList.Find((Platform.UGCItem x) => x.m_ID == ugcID);
				if (uGCItem != null)
				{
					PrisonData prisonDataForPrison = instance.GetPrisonDataForPrison(LevelScript.PRISON_ENUM.Centre_Perks);
					PrisonData prisonDataForPrison2 = instance.GetPrisonDataForPrison(prisonData.m_OutfitType);
					PrisonData prisonData2 = UnityEngine.Object.Instantiate(prisonDataForPrison);
					prisonData2.m_Configs.Clear();
					for (int j = 0; j < instance.m_CustomPrisonConfigs.Length; j++)
					{
						prisonData2.m_Configs.Add(UnityEngine.Object.Instantiate(instance.m_CustomPrisonConfigs[j]));
					}
					for (int k = 0; k < prisonData2.m_Configs.Count; k++)
					{
						prisonData2.m_Configs[k].m_ItemDataOverrides.Clear();
						prisonData2.m_Configs[k].m_ItemDataOverrides.AddRange(prisonDataForPrison2.m_Configs[0].m_ItemDataOverrides);
					}
					prisonData2.m_NameLocalizationKey = prisonData.m_strPrisonTitle;
					prisonData2.m_DescriptionLocalizationKey = prisonData.m_strPrisonDescription;
					prisonData2.m_ImagePath = uGCItem.m_AbsolutePath + PlatformIO.GetInstance().GetSeperationCharacter() + "Level_snap.png";
					prisonData2.m_LevelInfo.m_PrisonType = LevelScript.PRISON_TYPE.Normal;
					prisonData2.m_LevelInfo.m_PrisonEnum = LevelScript.PRISON_ENUM.CustomPrison;
					prisonData2.m_LevelInfo.m_AssociatedFile = prisonData.m_strPrisonFileName;
					prisonData2.m_CustomisableRoles = new int[2];
					prisonData2.m_CustomisableRoles[0] = prisonData.m_NumPrisonRoles[0];
					prisonData2.m_CustomisableRoles[1] = prisonData.m_NumPrisonRoles[1];
					for (int l = 0; l < prisonData2.m_Configs.Count; l++)
					{
						prisonData2.m_Configs[l].m_VendorConfig.m_MaxVendors = prisonData2.m_CustomisableRoles[0] / 2;
					}
					prisonData2.m_RoleStartingOutfitData = new ItemData[prisonData.m_NumPrisonRoles[0] + prisonData.m_NumPrisonRoles[1]];
					int num = 0;
					for (num = 0; num < prisonData.m_NumPrisonRoles[0]; num++)
					{
						prisonData2.m_RoleStartingOutfitData[num] = m_DefaultInmateOutfit;
					}
					int num2;
					for (num2 = prisonData.m_NumPrisonRoles[0] + (prisonData.m_NumPrisonRoles[1] - 2); num < num2; num++)
					{
						prisonData2.m_RoleStartingOutfitData[num] = m_DefaultGuardOutfit;
					}
					for (; num < num2 + 2; num++)
					{
						prisonData2.m_RoleStartingOutfitData[num] = m_DefaultRiotOutfit;
					}
					for (int m = 0; m < prisonData2.m_InfluencerWeights.Length; m++)
					{
						prisonData2.m_InfluencerWeights[m].min = 0;
						prisonData2.m_InfluencerWeights[m].max = 0;
					}
					PlaylistData playlistData = ScriptableObject.CreateInstance(typeof(PlaylistData)) as PlaylistData;
					playlistData.m_DescriptionLocalisationKey = prisonData.m_strPrisonDescription;
					playlistData.m_NameLocalisationKey = prisonData.m_strPrisonTitle;
					playlistData.m_ImagePath = prisonData2.m_ImagePath;
					playlistData.m_Prisons.Add(new PlaylistData.PrisonSetup(prisonData2, (int)prisonData.m_ePrisonDifficultyLevel));
					playlistData.m_GUID = "T17_User_Level_test";
					instance.AddCustomLevelPlaylist(playlistData);
					m_PlaylistCarousel.m_Options.Add(playlistData);
				}
			}
		}
		GlobalSave.GetInstance().Get("CFM:LastSelectedSubscribedPrisonsIndex", out var value, 0);
		value = Mathf.Min(m_PlaylistCarousel.m_Options.Count - 1, value);
		if (value < m_PlaylistCarousel.m_Options.Count && value >= 0)
		{
			m_PlaylistCarousel.SelectIndex(value);
			GlobalStart.GetInstance().SetSelectedPlaylist(m_PlaylistCarousel.GetSelectedItem());
		}
	}

	private void PlaylistCarousel_IndexSelectedEvent(int index, SelectionDirections directionTravelledIn)
	{
		GlobalStart.GetInstance().SetSelectedPlaylist(m_PlaylistCarousel.GetSelectedItem());
		GlobalSave.GetInstance().Set("CFM:LastSelectedSubscribedPrisonsIndex", index);
	}

	private void UpdateButtonStatus()
	{
		if (m_ContinueButton == null || m_NewGameButton == null || m_LoadGameButton == null || m_UnsubscribeButton == null)
		{
			return;
		}
		bool flag = m_PlaylistCarousel.GetNumOptions() == 0;
		m_ContinueButton.m_bLockedForCustomLevel = flag;
		m_NewGameButton.m_bLockedForCustomLevel = flag;
		m_LoadGameButton.m_bLockedForCustomLevel = flag;
		m_UnsubscribeButton.interactable = !flag;
		if (flag && m_PlaylistCarousel != null && m_PlaylistCarousel.m_PreviewImage != null && m_PlaylistCarousel.m_PrisonNameLabel != null && m_PlaylistCarousel.m_PrisonDescriptionLabel != null && m_DefaultLevelImage != null)
		{
			Sprite sprite = Sprite.Create(m_DefaultLevelImage, new Rect(0f, 0f, m_DefaultLevelImage.width, m_DefaultLevelImage.height), Vector2.zero);
			if (sprite != null)
			{
				m_PlaylistCarousel.m_PreviewImage.sprite = sprite;
			}
			m_PlaylistCarousel.m_PrisonDescriptionLabel.SetLocalisedTextCatchAll("Text.Editor.NoPrisonsSubscribed");
			m_PlaylistCarousel.m_PrisonNameLabel.SetNonLocalizedText(" ");
		}
	}

	private void OnUGCUpdated()
	{
		bool flag = m_PlaylistCarousel.GetNumOptions() == 0;
		GenerateCustomPrisonData();
		if (m_PlaylistCarousel.GetSelectedIndex() >= m_PlaylistCarousel.GetNumOptions())
		{
			m_PlaylistCarousel.SelectIndex(0);
		}
		if (flag && m_PlaylistCarousel.GetNumOptions() != 0)
		{
			m_PlaylistCarousel.SelectIndex(0);
			GlobalStart.GetInstance().SetSelectedPlaylist(m_PlaylistCarousel.GetSelectedItem());
		}
		UpdateButtonStatus();
		m_PlaylistCarousel.UpdateUI();
	}

	private void DoUnsubscribe(T17DialogBox dialog)
	{
		List<SaveManager.PrisonsSaveInformation.PrisonData> customPrisons = new List<SaveManager.PrisonsSaveInformation.PrisonData>();
		SaveManager.GetInstance().EnumerateCustomPrisons(ref customPrisons, bUGC: true);
		List<Platform.UGCItem> ugcList = new List<Platform.UGCItem>();
		Platform.GetInstance().EnumerateUGCItems(ref ugcList, Platform.UGCType.eCustomLevel);
		PlaylistData currentPlaylistData = GlobalStart.GetInstance().GetCurrentPlaylistData();
		if (!(currentPlaylistData != null))
		{
			return;
		}
		for (int i = 0; i < customPrisons.Count; i++)
		{
			if (customPrisons[i] != null && customPrisons[i].m_strPrisonFileName == currentPlaylistData.m_Prisons[0].m_PrisonData.m_LevelInfo.m_AssociatedFile)
			{
				ulong ugcID = ulong.Parse(customPrisons[i].m_strPrisonName);
				Platform.UGCItem uGCItem = null;
				uGCItem = ugcList.Find((Platform.UGCItem x) => x.m_ID == ugcID);
				if (uGCItem != null)
				{
					Platform.GetInstance().UnsubscribedFromContent(uGCItem);
				}
			}
		}
	}

	public void OnUnsubscribedClicked()
	{
		T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
		if (dialog != null)
		{
			dialog.Initialize(hasConfirm: true, hasDecline: true, hasCancel: false, "Text.Editor.Unsubscribe", "Text.Editor.UnsubscribeBody", "Text.Editor.Unsubscribe", "Text.Dialog.Prompt.Cancel", string.Empty);
			dialog.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnConfirm, new T17DialogBox.DialogEvent(DoUnsubscribe));
			dialog.Show();
		}
	}
}
