using System;
using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectMenu : MonoBehaviour
{
	public ScrollRect m_PlayerScrollRect;

	public LoopingDragScroll m_PlayerDragScroll;

	public PlayerSelectSlot m_CharacterSlotPrefab;

	public UI_DrawCharacterToRenderTexture m_CharacterRenderer;

	public T17Text m_CharacterNameDisplay;

	public RectTransform m_CentreSlotCollider;

	public T17Text m_SelectionCountdown;

	public float m_SelectionCountdownVisibleDuration = 10f;

	public float m_SelectionCountdownDuration = 20f;

	public T17Text m_AddMorePlayersText;

	public GameObject m_VoiceChatParent;

	public Sprite m_TalkingSprite;

	public VoiceChatFeedHUD.VoiceAndGamerObject[] m_GamerVoiceFeeds = new VoiceChatFeedHUD.VoiceAndGamerObject[4];

	public List<Platform.VoiceChatGamer> m_VoiceChatGamers = new List<Platform.VoiceChatGamer>();

	public GameObject m_PlayerProgressWarning;

	private IT17EventHelper[] m_EventHelperInterfaces;

	private Gamer m_Gamer;

	private CameraManager.PlayerBindingID m_CameraBindingID;

	private PlayerSelectSlot m_CentrePlayerSlot;

	public float m_ControllerScrollSpeed = 300f;

	public float m_FadeToWhiteTime = 0.25f;

	private T17EventSystem m_EventSystem;

	private int m_ToSelect = -1;

	private float m_TimeOfShow;

	private int m_RenderCounter;

	private List<PlayerSelectSlot> m_PlayerSelectSlots = new List<PlayerSelectSlot>();

	private void Awake()
	{
		m_EventHelperInterfaces = GetComponentsInChildren<IT17EventHelper>(includeInactive: true);
		m_VoiceChatGamers.Clear();
		for (int i = 0; i < 4; i++)
		{
			m_VoiceChatGamers.Add(new Platform.VoiceChatGamer());
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_Gamer != null)
		{
			m_Gamer.m_bIsInPlayerSelectMenu = false;
		}
		RenderTargetManager.CheckForLostRTEvents -= RenderTargetManager_OnRtRecreatedEvent;
	}

	public void ShowPlayerSelect(Gamer gamer, CameraManager.PlayerBindingID bindingID)
	{
		RenderTargetManager.CheckForLostRTEvents += RenderTargetManager_OnRtRecreatedEvent;
		base.gameObject.SetActive(value: true);
		if (m_VoiceChatParent != null)
		{
			m_VoiceChatParent.SetActive(value: false);
		}
		if (null != m_SelectionCountdown)
		{
			m_SelectionCountdown.m_bNeedsLocalization = false;
		}
		m_TimeOfShow = T17NetManager.RealTime;
		Platform.GetInstance().IsUGCRestrictedRequest(OnUGCRestrictedRequest);
		bool flag = true;
		Gamer[] allGamers = Gamer.GetAllGamers();
		for (int i = 0; i < allGamers.Length; i++)
		{
			if (allGamers[i] != null && (allGamers[i].m_eCharacterSelectionStage == Gamer.CharacterSelectionStage.InGame || allGamers[i].m_eCharacterSelectionStage == Gamer.CharacterSelectionStage.EnabledInGame))
			{
				flag = false;
			}
		}
		if (flag && !T17NetManager.NetOnlineMode)
		{
			RoutineManager instance = RoutineManager.GetInstance();
			if (instance != null)
			{
				instance.SetTimeFrozenRPC(bFrozen: true);
			}
		}
		m_Gamer = gamer;
		m_Gamer.m_bIsInPlayerSelectMenu = true;
		m_CameraBindingID = bindingID;
		m_EventSystem = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Gamer);
		if (m_Gamer != null && m_Gamer.m_RewiredPlayer != null)
		{
			m_Gamer.m_eCharacterSelectionStage = Gamer.CharacterSelectionStage.InMenu;
			T17EventSystem.ApplyCategories(m_Gamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.PlayerSelect);
			T17EventSystem gamersEventSystem = null;
			if (m_Gamer != null)
			{
				gamersEventSystem = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Gamer);
			}
			if (m_EventHelperInterfaces != null)
			{
				for (int j = 0; j < m_EventHelperInterfaces.Length; j++)
				{
					if (m_EventHelperInterfaces[j] != null)
					{
						m_EventHelperInterfaces[j].SetGamerForEventSystem(m_Gamer, gamersEventSystem);
					}
				}
			}
			if (m_Gamer != null && m_Gamer.m_PlayerObject != null)
			{
				m_Gamer.m_PlayerObject.bDisplayTutorials = false;
			}
		}
		Customisation[] array = null;
		if (CustomisationManager.GetInstance() != null)
		{
			array = CustomisationManager.GetInstance().GetPlayerPresets();
		}
		if (array == null || array.Length <= 0)
		{
			if (m_CharacterNameDisplay != null)
			{
				m_CharacterNameDisplay.text = "Player apperances cannot be found!";
			}
			return;
		}
		if (m_CharacterRenderer != null)
		{
			m_CharacterRenderer.RegisterActivationRequest(base.gameObject);
		}
		if (m_PlayerScrollRect.content.childCount > 0)
		{
			PlayerSelectSlot[] componentsInChildren = m_PlayerScrollRect.content.GetComponentsInChildren<PlayerSelectSlot>();
			for (int k = 0; k < componentsInChildren.Length; k++)
			{
				UnityEngine.Object.DestroyImmediate(componentsInChildren[k].gameObject);
				componentsInChildren[k] = null;
			}
			m_PlayerDragScroll.ResetForNewItems();
		}
		CustomisationConstraint customisationConstraint = null;
		if (LevelScript.GetInstance() != null && LevelScript.GetInstance().m_LevelSetup != null)
		{
			customisationConstraint = LevelScript.GetInstance().m_LevelSetup.m_DefaultPlayerConstraint;
			if (customisationConstraint != null)
			{
				Customisation[] array2 = array;
				array = new Customisation[array2.Length];
				for (int l = 0; l < array.Length; l++)
				{
					array[l] = new Customisation(array2[l]);
				}
			}
		}
		m_PlayerSelectSlots.Clear();
		for (int m = 0; m < array.Length; m++)
		{
			PlayerSelectSlot playerSelectSlot = UnityEngine.Object.Instantiate(m_CharacterSlotPrefab, m_PlayerScrollRect.content);
			m_PlayerSelectSlots.Add(playerSelectSlot);
			playerSelectSlot.OnPressed = (PlayerSelectSlot.ButtonPressed)Delegate.Remove(playerSelectSlot.OnPressed, new PlayerSelectSlot.ButtonPressed(OnAppearanceClicked));
			playerSelectSlot.OnPressed = (PlayerSelectSlot.ButtonPressed)Delegate.Combine(playerSelectSlot.OnPressed, new PlayerSelectSlot.ButtonPressed(OnAppearanceClicked));
			if (customisationConstraint != null)
			{
				CustomisationManager.ApplyConstraint(ref array[m], customisationConstraint);
			}
			array[m].defaultOutfit = CustomisationData.Outfit.INMATE_01;
			playerSelectSlot.customisation = new Customisation(array[m]);
			if (LevelScript.GetInstance() != null && ConfigManager.GetInstance() != null)
			{
				PrisonConfig prisonConfig = LevelScript.GetInstance().m_LevelSetup.m_Configs.Find((PrisonConfig x) => x.m_ConfigType == ConfigManager.GetInstance().gameType);
				if (prisonConfig != null && prisonConfig.m_ItemDataOverrides.Count >= 2 && prisonConfig.m_ItemDataOverrides[1].m_OverrideOutfit)
				{
					playerSelectSlot.customisation.defaultOutfit = prisonConfig.m_ItemDataOverrides[1].m_OutfitAppearance;
				}
			}
			int width = Mathf.FloorToInt(playerSelectSlot.m_RawImage.rectTransform.rect.width);
			int height = Mathf.FloorToInt(playerSelectSlot.m_RawImage.rectTransform.rect.height);
			RenderTexture renderTexture = m_CharacterRenderer.CreateRenderTexture(width, height, ref playerSelectSlot.m_RenderTextureID);
			playerSelectSlot.m_RawImage.texture = renderTexture;
			playerSelectSlot.RenderTex = renderTexture;
			DrawPlayerSelectSlotWithHighlight(playerSelectSlot, isHighlighted: false);
			playerSelectSlot.index = m;
		}
		m_PlayerDragScroll.Init();
		CustomisationManager instance2 = CustomisationManager.GetInstance();
		m_ToSelect = ((instance2 != null) ? instance2.GetPlayerLastChosenPreset() : 0);
		if (m_PlayerProgressWarning != null)
		{
			m_PlayerProgressWarning.SetActive(value: true);
		}
		RenderTargetManager.CheckRtEndOfFrame(15);
	}

	private void RenderTargetManager_OnRtRecreatedEvent()
	{
		if (m_PlayerSelectSlots == null || !(m_CharacterRenderer != null))
		{
			return;
		}
		for (int num = m_PlayerSelectSlots.Count - 1; num >= 0; num--)
		{
			PlayerSelectSlot playerSelectSlot = m_PlayerSelectSlots[num];
			if (playerSelectSlot != null)
			{
				DrawPlayerSelectSlotWithHighlight(playerSelectSlot, isHighlighted: false);
			}
		}
	}

	private void DrawPlayerSelectSlotWithHighlight(PlayerSelectSlot newPlayerSlot, bool isHighlighted)
	{
		m_CharacterRenderer.SetCustomisation(newPlayerSlot.customisation);
		m_CharacterRenderer.SetHighlighted(isHighlighted);
		m_CharacterRenderer.DrawCharacter(newPlayerSlot.RenderTex, m_RenderCounter > 0);
	}

	private void OnUGCRestrictedRequest(bool isRestricted, Platform.OnlineAccessErrorCode returnCode, bool failureHandledPlatformside)
	{
		SetCharacterDisplayName();
	}

	private void SetCharacterDisplayName()
	{
		if (m_CharacterNameDisplay != null && m_CentrePlayerSlot != null && m_CentrePlayerSlot.customisation != null)
		{
			m_CharacterNameDisplay.text = m_CentrePlayerSlot.customisation.FinalName;
		}
	}

	public void Hide()
	{
		RenderTargetManager.CheckForLostRTEvents -= RenderTargetManager_OnRtRecreatedEvent;
		if (m_Gamer != null && m_Gamer.m_PlayerObject != null)
		{
			m_Gamer.m_PlayerObject.bDisplayTutorials = true;
			TutorialManager instance = TutorialManager.GetInstance();
			if (instance != null && m_Gamer != null && m_Gamer.m_PlayerObject != null)
			{
				PrisonData.LevelInfo currentLevelInfo = LevelScript.GetCurrentLevelInfo();
				if (currentLevelInfo.m_PrisonType == LevelScript.PRISON_TYPE.Transport && instance.CheckTutorialNeeded(m_Gamer.m_PlayerObject, TutorialSubject.TransportPrisons))
				{
					instance.StartTutorialRPC(m_Gamer.m_PlayerObject, TutorialSubject.TransportPrisons);
				}
				else if (!m_Gamer.m_bPrimaryLocal || !T17NetManager.IsMasterClient)
				{
					instance.StartTutorialRPC(m_Gamer.m_PlayerObject, TutorialSubject.Emote);
					if (T17NetManager.IsMasterClient)
					{
						Player playerObject = Gamer.GetPrimaryGamer().m_PlayerObject;
						if (playerObject != null)
						{
							instance.StartTutorialRPC(Gamer.GetPrimaryGamer().m_PlayerObject, TutorialSubject.Emote);
						}
					}
					else
					{
						List<Player> allPlayers = Player.GetAllPlayers();
						for (int i = 0; i < allPlayers.Count; i++)
						{
							if (allPlayers[i] != null && allPlayers[i].m_Gamer != null)
							{
								instance.StartTutorialRPC(allPlayers[i], TutorialSubject.Emote);
							}
						}
					}
				}
			}
		}
		for (int j = 0; j < m_PlayerScrollRect.content.childCount; j++)
		{
			Transform child = m_PlayerScrollRect.content.GetChild(j);
			if (child != null)
			{
				PlayerSelectSlot component = child.GetComponent<PlayerSelectSlot>();
				if (component != null)
				{
					component.m_RawImage.texture = null;
					m_CharacterRenderer.CleanupRenderTexture(ref component.m_RenderTextureID);
					component.RenderTex = null;
				}
			}
		}
		m_PlayerSelectSlots.Clear();
		if (m_Gamer != null)
		{
			m_Gamer.m_bIsInPlayerSelectMenu = false;
		}
		if (m_CharacterRenderer != null)
		{
			m_CharacterRenderer.UnregisterActivationRequest(base.gameObject);
		}
		base.gameObject.SetActive(value: false);
		m_Gamer = null;
		m_CameraBindingID = CameraManager.PlayerBindingID.CM_PBID_UNSET;
		m_TimeOfShow = 0f;
		RoutineManager instance2 = RoutineManager.GetInstance();
		if (instance2 != null)
		{
			instance2.SetTimeFrozenRPC(bFrozen: false);
		}
		if (m_VoiceChatParent != null)
		{
			m_VoiceChatParent.SetActive(value: false);
		}
	}

	private void ClickCurrentSlot()
	{
		if (!(m_PlayerScrollRect != null) || !(m_PlayerScrollRect.content != null) || !(m_EventSystem != null))
		{
			return;
		}
		GameObject gameObject = FindCentreCharacterSlot();
		if (gameObject != null)
		{
			PlayerSelectSlot component = gameObject.GetComponent<PlayerSelectSlot>();
			if (component != null)
			{
				OnAppearanceClicked(component);
			}
		}
	}

	private void UpdateAutoSelectTimeout()
	{
		if (!(null != m_SelectionCountdown))
		{
			return;
		}
		string text = string.Empty;
		if (T17NetManager.IsConnectedOnline() && T17NetRoomManager.IsInRoom() && m_TimeOfShow > 0f)
		{
			float num = m_TimeOfShow + m_SelectionCountdownDuration - T17NetManager.RealTime;
			if (num <= m_SelectionCountdownVisibleDuration)
			{
				text = ((int)num + 1).ToString();
			}
			if (num <= 0f)
			{
				ClickCurrentSlot();
			}
		}
		m_SelectionCountdown.text = text;
	}

	private void Update()
	{
		if (m_Gamer == null || m_Gamer.m_RewiredPlayer == null || m_Gamer.m_NetViewID == -1 || (T17EventSystemsManager.Instance == null && m_EventSystem == null))
		{
			return;
		}
		if (m_EventSystem == null)
		{
			m_EventSystem = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Gamer);
		}
		if (m_Gamer != null && m_Gamer.m_RewiredPlayer != null)
		{
			if (m_Gamer.m_RewiredPlayer.GetButtonDown("PS_SelectPlayer"))
			{
				ClickCurrentSlot();
			}
			else if (m_Gamer != null && m_Gamer.m_RewiredPlayer != null && m_Gamer.m_RewiredPlayer.GetButton("PS_NextPlayer"))
			{
				if (m_PlayerDragScroll != null)
				{
					m_PlayerDragScroll.GridSnap.ButtonScroll(scrollLeft: true, m_Gamer.m_RewiredPlayer.GetButtonDown("PS_NextPlayer"));
				}
			}
			else if (m_Gamer != null && m_Gamer.m_RewiredPlayer != null && m_Gamer.m_RewiredPlayer.GetButton("PS_PreviousPlayer") && m_PlayerDragScroll != null)
			{
				m_PlayerDragScroll.GridSnap.ButtonScroll(scrollLeft: false, m_Gamer.m_RewiredPlayer.GetButton("PS_PreviousPlayer"));
			}
			if (m_PlayerScrollRect != null && m_PlayerScrollRect.content != null && (m_CentrePlayerSlot == null || !CheckCentreCharacterSlot(m_CentrePlayerSlot.transform)))
			{
				GameObject gameObject = FindCentreCharacterSlot();
				if (gameObject != null && (m_CentrePlayerSlot == null || gameObject != m_CentrePlayerSlot.gameObject))
				{
					PlayerSelectSlot component = gameObject.GetComponent<PlayerSelectSlot>();
					if (m_CharacterRenderer != null && m_CentrePlayerSlot != null && m_CentrePlayerSlot.RenderTex != null)
					{
						m_CharacterRenderer.SetCustomisation(m_CentrePlayerSlot.customisation);
						m_CharacterRenderer.DrawCharacter(m_CentrePlayerSlot.RenderTex, m_RenderCounter > 0);
					}
					m_CentrePlayerSlot = component;
					if (m_CentrePlayerSlot != null && m_CentrePlayerSlot.m_Button != null && m_EventSystem != null)
					{
						m_EventSystem.SetSelectedGameObject(m_CentrePlayerSlot.m_Button.gameObject);
						AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Player_Select, AudioController.UI_Audio_GO);
						SetCharacterDisplayName();
					}
				}
			}
		}
		if (m_CentrePlayerSlot != null && m_CharacterRenderer != null)
		{
			m_CharacterRenderer.UpdateAnimation(Time.deltaTime);
			m_CharacterRenderer.SetCustomisation(m_CentrePlayerSlot.customisation);
			m_CharacterRenderer.DrawCharacter(m_CentrePlayerSlot.RenderTex, m_RenderCounter > 0);
		}
		if (m_ToSelect != -1)
		{
			SnapTo(m_ToSelect);
			m_ToSelect = -1;
		}
		UpdateAutoSelectTimeout();
		m_RenderCounter++;
	}

	public void OnAppearanceSelected()
	{
		if (!m_Gamer.m_bPrimaryLocal || !T17NetManager.IsMasterClient)
		{
			OpinionManager instance = OpinionManager.GetInstance();
			if (instance != null)
			{
				instance.ResetOpinionsOfPlayerRPC(m_Gamer.m_PlayerObject);
			}
		}
		if (m_Gamer.m_bPrimaryLocal)
		{
			m_Gamer.m_PlayerObject.m_CharacterStats.LoadStatsFromPreviousGame();
		}
		m_Gamer.m_eCharacterSelectionStage = Gamer.CharacterSelectionStage.InGame;
		if (m_Gamer != null && null != m_Gamer.m_PlayerObject && m_Gamer.m_PlayerObject.m_NetView.isMine)
		{
			m_Gamer.m_PlayerObject.EnableInLevel();
		}
		Gamer gamer = m_Gamer;
		InGameMenuFlow.Instance.HidePlayerSelect(m_Gamer, m_CameraBindingID);
		bool flag = false;
		CutsceneManagerBase instance2 = CutsceneManagerBase.GetInstance();
		if (instance2 != null)
		{
			flag = instance2.ConsiderPlayingIntroCutscene(m_FadeToWhiteTime, UIAnimatedEffectController.Effects.FadeToOpaqueWhite, UIAnimatedEffectController.Effects.FadeToOpaque, gamer.m_PlayerObject);
			if (flag)
			{
				CutsceneManagerBase.CutsceneFinishedEvent += gamer.m_PlayerObject.SetPlayerCameraForCursceneEnd;
			}
		}
		if (!flag)
		{
			gamer.m_PlayerObject.SetCameraTargetToPlayer();
		}
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_CharacterSelect, AudioController.UI_Audio_GO);
		LevelScript.PRISON_ENUM prisonEnum = LevelScript.GetCurrentLevelInfo().m_PrisonEnum;
		bool flag2 = LevelDataManager.GetInstance().IsDLCLevel(prisonEnum);
		bool flag3 = LevelScript.GetCurrentLevelInfo().m_PrisonType == LevelScript.PRISON_TYPE.Transport;
		if (T17NetRoomManager.CurrentGameRoomType == T17NetRoomGameView.GameRoomType.Offline || !Platform.GetInstance().OnlineCheck())
		{
			return;
		}
		Gamer[] allGamers = Gamer.GetAllGamers();
		for (int i = 0; i < allGamers.Length; i++)
		{
			if (allGamers[i] == null || !allGamers[i].IsLocal())
			{
				continue;
			}
			PrisonConfig.ConfigType gameType = ConfigManager.GetInstance().gameType;
			if (!flag2)
			{
				if (!flag3 && gameType == PrisonConfig.ConfigType.Cooperative)
				{
					StatSystem.GetInstance().IncStat(17, 1f, allGamers[i], string.Empty);
				}
				if (gameType == PrisonConfig.ConfigType.Versus)
				{
					StatSystem.GetInstance().IncStat(16, 1f, allGamers[i], string.Empty);
				}
			}
		}
	}

	public void OnAppearanceClicked(PlayerSelectSlot selectedSlot)
	{
		if (CutsceneManagerBase.GetState() != CutsceneManagerBase.States.Idle || !(selectedSlot != null))
		{
			return;
		}
		GameObject gameObject = FindCentreCharacterSlot();
		if (gameObject != selectedSlot.gameObject)
		{
			for (int i = 0; i < m_PlayerScrollRect.content.childCount; i++)
			{
				Transform child = m_PlayerScrollRect.content.GetChild(i);
				if (child == selectedSlot.transform)
				{
					float targetElement = gameObject.transform.localPosition.x - selectedSlot.transform.localPosition.x;
					m_PlayerDragScroll.GridSnap.SetTargetElement(targetElement);
					return;
				}
			}
		}
		if (PlayerDataManager.GetInstance() != null && m_Gamer != null && m_Gamer.m_PlayerObject != null)
		{
			PlayerDataManager.GetInstance().RequestSetPlayerAppearanceRPC(m_Gamer.m_PlayerObject, selectedSlot.customisation);
			OnAppearanceSelected();
			if (CustomisationManager.GetInstance().GetHasPresetBeenModified(selectedSlot.index))
			{
				StatSystem.GetInstance().IncStat(25, 1f, Gamer.GetPrimaryGamer(), string.Empty);
			}
			CustomisationManager instance = CustomisationManager.GetInstance();
			if (instance != null)
			{
				instance.OnPlayerChosenSlotChanged(selectedSlot.index);
			}
			GoogleAnalyticsV3.LogCommericalAnalyticEvent("Customisation", "BodyType", selectedSlot.customisation.body.ToString(), 0L);
			GoogleAnalyticsV3.LogCommericalAnalyticEvent("Customisation", "SkinColour", selectedSlot.customisation.skin.ToString(), 0L);
			GoogleAnalyticsV3.LogCommericalAnalyticEvent("Customisation", "Hair", selectedSlot.customisation.hair.ToString(), 0L);
			GoogleAnalyticsV3.LogCommericalAnalyticEvent("Customisation", "Hat", selectedSlot.customisation.hat.ToString(), 0L);
			GoogleAnalyticsV3.LogCommericalAnalyticEvent("Customisation", "UpperFaceAccessory", selectedSlot.customisation.upperFace.ToString(), 0L);
			GoogleAnalyticsV3.LogCommericalAnalyticEvent("Customisation", "LowerFaceAccessory", selectedSlot.customisation.lowerFace.ToString(), 0L);
		}
	}

	private GameObject FindCentreCharacterSlot()
	{
		for (int i = 0; i < m_PlayerScrollRect.content.childCount; i++)
		{
			Transform child = m_PlayerScrollRect.content.GetChild(i);
			if (m_CentreSlotCollider != null && m_CentreSlotCollider.rect.Contains(m_CentreSlotCollider.InverseTransformPoint(child.position)))
			{
				return child.gameObject;
			}
		}
		return null;
	}

	private int FindCentreCharacterIndex()
	{
		for (int i = 0; i < m_PlayerScrollRect.content.childCount; i++)
		{
			Transform child = m_PlayerScrollRect.content.GetChild(i);
			if (m_CentreSlotCollider != null && m_CentreSlotCollider.rect.Contains(m_CentreSlotCollider.InverseTransformPoint(child.position)))
			{
				return i;
			}
		}
		return -1;
	}

	private bool CheckCentreCharacterSlot(Transform target)
	{
		if (m_CentreSlotCollider != null && m_CentreSlotCollider.rect.Contains(m_CentreSlotCollider.InverseTransformPoint(target.position)))
		{
			return true;
		}
		return false;
	}

	public void SnapTo(int itemIndex)
	{
		if (itemIndex < m_PlayerScrollRect.content.childCount && m_PlayerDragScroll != null && m_PlayerDragScroll.GridSnap != null)
		{
			RectTransform component = m_PlayerScrollRect.content.GetComponent<RectTransform>();
			Vector3 localPosition = component.localPosition;
			float cellWidth = m_PlayerDragScroll.GridSnap.GetCellWidth();
			localPosition.x = (float)(m_PlayerScrollRect.content.childCount / 2 - 1 - itemIndex) * cellWidth;
			localPosition.y = 0f;
			localPosition.x -= m_PlayerDragScroll.GridSnap.GetCellWidth() / 2f;
			m_PlayerScrollRect.StopMovement();
			m_PlayerDragScroll.GridSnap.ForceTargetPosition(localPosition);
			component.localPosition = localPosition;
		}
	}
}
