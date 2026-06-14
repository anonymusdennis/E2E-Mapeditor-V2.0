using System;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;
using UnityEngine.EventSystems;

public class PayphoneMenu : MonoBehaviour
{
	public T17Text m_HintVagueDescription;

	private HintButton[] m_HintButtons;

	private IT17EventHelper[] m_EventHelperInterfaces;

	public GameObject m_HintList;

	public GameObject m_HintPage;

	public T17Text m_HintFullDescription;

	public T17Button m_HintBackButton;

	public PayphoneCraftingRecipiePage m_CraftHintPage;

	private int m_SelectedHintIndex = -1;

	private Player m_Player;

	private void Awake()
	{
		if (m_HintList != null)
		{
			m_HintButtons = m_HintList.GetComponentsInChildren<HintButton>(includeInactive: true);
		}
		m_EventHelperInterfaces = GetComponentsInChildren<IT17EventHelper>(includeInactive: true);
		for (int i = 0; i < m_HintButtons.Length; i++)
		{
			T17Button button = m_HintButtons[i].m_Button;
			button.OnButtonSelect = (T17Button.T17ButtonDelegate)Delegate.Remove(button.OnButtonSelect, new T17Button.T17ButtonDelegate(ShowHintDescription));
			T17Button button2 = m_HintButtons[i].m_Button;
			button2.OnButtonSelect = (T17Button.T17ButtonDelegate)Delegate.Combine(button2.OnButtonSelect, new T17Button.T17ButtonDelegate(ShowHintDescription));
			T17Button button3 = m_HintButtons[i].m_Button;
			button3.OnButtonPointerEnter = (T17Button.T17ButtonDelegate)Delegate.Remove(button3.OnButtonPointerEnter, new T17Button.T17ButtonDelegate(ShowHintDescription));
			T17Button button4 = m_HintButtons[i].m_Button;
			button4.OnButtonPointerEnter = (T17Button.T17ButtonDelegate)Delegate.Combine(button4.OnButtonPointerEnter, new T17Button.T17ButtonDelegate(ShowHintDescription));
		}
	}

	private void OnEnable()
	{
		ResetButtons();
		ShowHintDescription(null);
	}

	public void ShowHintDescription(T17Button sender)
	{
		if (!(EventSystem.current != null) || m_HintButtons == null)
		{
			return;
		}
		T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Player.m_Gamer);
		for (int i = 0; i < m_HintButtons.Length; i++)
		{
			if (!(eventSystemForGamer.currentSelectedGameObject == m_HintButtons[i].gameObject) && !(eventSystemForGamer.GetCurrentPointerOverGameobject() == m_HintButtons[i].gameObject))
			{
				continue;
			}
			if (m_HintButtons[i].IsRegularHint)
			{
				m_HintVagueDescription.SetNewLocalizationTag(m_HintButtons[i].HintData.m_VagueHint);
				m_HintVagueDescription.SetNewPlaceHolder(m_HintButtons[i].HintData.m_VagueHint);
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Payphone_Highlight, base.gameObject);
				break;
			}
			if (m_HintButtons[i].IsCraftHint)
			{
				if (m_HintButtons[i].CraftHintData.m_ItemToCraft == null)
				{
					m_HintVagueDescription.SetNewLocalizationTag(string.Empty);
					m_HintVagueDescription.SetNewPlaceHolder("The item could not be found for this crafting hint. It is probably not set up correctly in the Hint config.");
				}
				m_HintVagueDescription.SetNewLocalizationTag(m_HintButtons[i].CraftHintData.m_ItemToCraft.m_ItemLocalizationTag);
				m_HintVagueDescription.SetNewPlaceHolder(m_HintButtons[i].CraftHintData.m_ItemToCraft.m_ItemLocalizationTag);
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Payphone_Highlight, base.gameObject);
				break;
			}
		}
	}

	public void HintClicked(int index)
	{
		if (m_HintButtons[index].IsRegularHint)
		{
			if (!m_Player.IsHintFound(index))
			{
				if (!(m_Player.m_CharacterStats.Money >= (float)m_HintButtons[index].HintData.m_HintCost))
				{
					SpeechManager.GetInstance().SaySomething(m_Player, "Text.Player.NotEnoughMoney", SpeechTone.Positive, 3f, 5);
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Unavailable, base.gameObject);
					return;
				}
				m_Player.m_CharacterStats.DecreaseMoney(m_HintButtons[index].HintData.m_HintCost);
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Payphone_Purchase, base.gameObject);
				GoogleAnalyticsV3.LogCommericalAnalyticEvent("Payphone Interaction", LevelScript.GetCurrentLevelInfo().m_PrisonEnum.ToString() + " Payphone Hint Purchased", m_HintButtons[index].HintData.m_FullHint + " Read", 0L);
			}
			m_Player.SetHintIndexAsFound(m_HintButtons[index].HintIndex);
			m_HintFullDescription.SetNewLocalizationTag(m_HintButtons[index].HintData.m_FullHint);
			m_SelectedHintIndex = index;
			ShowHintPage();
		}
		else
		{
			if (!m_HintButtons[index].IsCraftHint)
			{
				return;
			}
			if (!m_Player.IsHintFound(index))
			{
				if (!(m_Player.m_CharacterStats.Money >= (float)m_HintButtons[index].CraftHintData.m_HintCost))
				{
					SpeechManager.GetInstance().SaySomething(m_Player, "Text.Player.NotEnoughMoney", SpeechTone.Positive, 3f, 5);
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Unavailable, base.gameObject);
					return;
				}
				m_Player.m_CharacterStats.DecreaseMoney(m_HintButtons[index].CraftHintData.m_HintCost);
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Payphone_Purchase, base.gameObject);
				GoogleAnalyticsV3.LogCommericalAnalyticEvent("Payphone Interaction", LevelScript.GetCurrentLevelInfo().m_PrisonEnum.ToString() + " Payphone Hint Purchased", string.Concat(m_HintButtons[index].CraftHintData.m_ItemToCraft, " Read"), 0L);
			}
			CraftManager instance = CraftManager.GetInstance();
			if (instance != null)
			{
				CraftManager.Recipe recipeForProduct = instance.GetRecipeForProduct(m_HintButtons[index].CraftHintData.m_ItemToCraft);
				if (recipeForProduct != null)
				{
					instance.DiscoverHiddenItem(recipeForProduct);
				}
			}
			m_Player.SetHintIndexAsFound(m_HintButtons[index].HintIndex);
			m_SelectedHintIndex = index;
			ShowCraftPage();
			m_CraftHintPage.DisplayCraftRecipie(m_HintButtons[index].CraftHintData.m_ItemToCraft);
		}
	}

	public void ShowPayphone(Player player, CameraManager.PlayerBindingID bindingID)
	{
		if (!base.gameObject.GetActive())
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Payphone_Open, base.gameObject);
		}
		m_Player = player;
		base.gameObject.SetActive(value: true);
		T17EventSystem gamersEventSystem = null;
		if (m_Player != null && m_Player.m_Gamer != null)
		{
			gamersEventSystem = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Player.m_Gamer);
		}
		if (m_EventHelperInterfaces != null)
		{
			for (int i = 0; i < m_EventHelperInterfaces.Length; i++)
			{
				if (m_EventHelperInterfaces[i] != null && m_Player.m_Gamer != null)
				{
					m_EventHelperInterfaces[i].SetGamerForEventSystem(m_Player.m_Gamer, gamersEventSystem);
				}
			}
		}
		if (EventSystem.current != null && m_HintButtons[0] != null)
		{
			T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Player.m_Gamer);
			eventSystemForGamer.SetSelectedGameObject(null);
			eventSystemForGamer.SetSelectedGameObject(m_HintButtons[0].gameObject, forceSet: true);
		}
		ShowHintList();
	}

	public void Hide()
	{
		if (base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: false);
		}
		m_Player = null;
		m_SelectedHintIndex = -1;
	}

	private void Update()
	{
		if (m_Player.m_Gamer.m_RewiredPlayer.GetButtonUp("UI_Cancel"))
		{
			if (m_HintPage.gameObject.activeSelf || m_CraftHintPage.gameObject.activeSelf)
			{
				ShowHintList();
			}
			else if (m_Player != null)
			{
				m_Player.RequestStopInteraction();
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Payphone_Open, base.gameObject);
			}
		}
	}

	private void ResetButtons()
	{
		for (int i = 0; i < m_HintButtons.Length; i++)
		{
			if (!(m_Player != null))
			{
				continue;
			}
			if (!m_Player.IsHintFound(i))
			{
				m_HintButtons[i].HintIndex = i;
				m_HintButtons[i].HintData = GlobalHintManager.GetInstance().GetHintData(LevelScript.GetCurrentLevelInfo().m_PrisonEnum, i);
				if (!m_HintButtons[i].IsRegularHint)
				{
					m_HintButtons[i].CraftHintData = GlobalHintManager.GetInstance().GetCraftHintData(LevelScript.GetCurrentLevelInfo().m_PrisonEnum, i);
				}
				if ((m_HintButtons[i].IsRegularHint || m_HintButtons[i].IsCraftHint) && m_HintButtons[i].m_Text != null)
				{
					if (m_HintButtons[i].IsRegularHint)
					{
						m_HintButtons[i].m_Text.m_bNeedsLocalization = false;
						m_HintButtons[i].m_Text.text = "[TICON=Coin] " + m_HintButtons[i].HintData.m_HintCost;
					}
					else if (m_HintButtons[i].IsCraftHint)
					{
						m_HintButtons[i].m_Text.m_bNeedsLocalization = false;
						m_HintButtons[i].m_Text.text = "[TICON=Coin] " + m_HintButtons[i].CraftHintData.m_HintCost;
					}
				}
			}
			else if (m_HintButtons[i].m_Text != null)
			{
				m_HintButtons[i].m_Text.m_bNeedsLocalization = true;
				m_HintButtons[i].m_Text.SetLocalisedTextCatchAll("Text.Payphone.ReviewHint");
				m_HintButtons[i].HintIndex = i;
				m_HintButtons[i].HintData = GlobalHintManager.GetInstance().GetHintData(LevelScript.GetCurrentLevelInfo().m_PrisonEnum, i);
				if (!m_HintButtons[i].IsRegularHint)
				{
					m_HintButtons[i].CraftHintData = GlobalHintManager.GetInstance().GetCraftHintData(LevelScript.GetCurrentLevelInfo().m_PrisonEnum, i);
				}
			}
		}
	}

	private void CheckMoneyForHints()
	{
		for (int i = 0; i < m_HintButtons.Length; i++)
		{
			if ((float)m_HintButtons[i].HintData.m_HintCost > m_Player.m_CharacterStats.Money && !m_Player.IsHintFound(i))
			{
				m_HintButtons[i].m_Button.enabled = false;
				continue;
			}
			m_HintButtons[i].m_Button.enabled = true;
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Payphone_Open, base.gameObject);
		}
	}

	public void ShowHintList()
	{
		m_HintList.SetActive(value: true);
		m_CraftHintPage.gameObject.SetActive(value: false);
		m_HintPage.SetActive(value: false);
		int num = -1;
		num = ((m_SelectedHintIndex >= 0 && m_SelectedHintIndex < m_HintButtons.Length) ? m_SelectedHintIndex : 0);
		if (EventSystem.current != null && m_HintButtons[num].m_Button != null)
		{
			T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Player.m_Gamer);
			eventSystemForGamer.SetSelectedGameObject(null);
			eventSystemForGamer.SetSelectedGameObject(m_HintButtons[num].m_Button.gameObject);
		}
		ResetButtons();
		ShowHintDescription(null);
	}

	public void ShowHintPage()
	{
		m_HintList.SetActive(value: false);
		m_CraftHintPage.gameObject.SetActive(value: false);
		m_HintPage.SetActive(value: true);
		if (EventSystem.current != null && m_HintBackButton != null)
		{
			T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Player.m_Gamer);
			eventSystemForGamer.SetSelectedGameObject(null);
			eventSystemForGamer.SetSelectedGameObject(m_HintBackButton.gameObject);
		}
	}

	public void ShowCraftPage()
	{
		m_HintList.SetActive(value: false);
		m_CraftHintPage.gameObject.SetActive(value: true);
		m_HintPage.SetActive(value: false);
		if (EventSystem.current != null && m_CraftHintPage.m_BackButton != null)
		{
			T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Player.m_Gamer);
			eventSystemForGamer.SetSelectedGameObject(null);
			eventSystemForGamer.SetSelectedGameObject(m_CraftHintPage.m_BackButton.gameObject);
		}
	}
}
