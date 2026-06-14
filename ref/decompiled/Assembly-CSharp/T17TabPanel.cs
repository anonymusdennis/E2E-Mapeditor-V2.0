using System;
using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class T17TabPanel : BaseMenuBehaviour
{
	public enum RelativePosition
	{
		Top,
		Left,
		Right,
		Bottom
	}

	[HideInInspector]
	public T17Button[] m_Buttons;

	[HideInInspector]
	public BaseMenuBehaviour[] m_MenuBodies;

	[HideInInspector]
	public UnityEvent[] m_MenuDelegates;

	[HideInInspector]
	public bool m_bHasEventsEnabled;

	[HideInInspector]
	public bool m_bKeepSelectedTabHighlighted = true;

	[HideInInspector]
	public bool m_bTabEntriesSetExternally;

	[HideInInspector]
	public Sprite[] m_TabSplitterSprites;

	[HideInInspector]
	public string[] m_TabTitleTags;

	public RelativePosition m_RelativePositionToBodies;

	[HideInInspector]
	public bool m_bAllowDirectNavigation;

	[HideInInspector]
	public bool m_bAllowIndirectNavigation = true;

	[HideInInspector]
	public string m_PreviousTabInputAction = "CycleLeft";

	[HideInInspector]
	public string m_NextTabInputAction = "CycleRight";

	private const string m_ButtonAnimatorHoldBool = "HoldSelected";

	public Color m_TabDisabledColour = Color.black;

	public T17Text m_TabTitle;

	public T17Image m_TabSplitter;

	public bool m_bPlayAudio = true;

	private int m_OldTabIndex;

	private int m_CurrentTabIndex;

	private int m_MaxTabIndex;

	private ColorBlock m_CurrentButtonNormalColors;

	private ColorBlock m_CurrentButtonHighlightColors;

	private SpriteState m_CurrentButtonSpriteState;

	private Sprite m_CurrentButtonNormalSprite;

	private T17Image[] m_ButtonChildImages;

	public int CurrentTabIndex => m_CurrentTabIndex;

	public int PreviousTabIndex => m_OldTabIndex;

	protected override void Awake()
	{
		base.Awake();
		T17Button[] componentsInChildren = GetComponentsInChildren<T17Button>(includeInactive: true);
		int num = componentsInChildren.Length;
		m_Buttons = new T17Button[num];
		int num2 = 0;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].GetComponent<DontCountThisButton>() == null)
			{
				m_Buttons[num2++] = componentsInChildren[i];
			}
		}
		if (num2 != num)
		{
			Array.Resize(ref m_Buttons, num2);
		}
		m_ButtonChildImages = new T17Image[m_Buttons.Length];
		for (int j = 0; j < m_Buttons.Length; j++)
		{
			m_Buttons[j].onClick.RemoveAllListeners();
			T17Button buttonToClick = m_Buttons[j];
			m_Buttons[j].onClick.AddListener(delegate
			{
				OnTabButtonClicked(buttonToClick);
			});
			if (m_Buttons[j].transform.childCount > 0)
			{
				m_ButtonChildImages[j] = m_Buttons[j].transform.GetChild(0).GetComponent<T17Image>();
			}
		}
		if (m_bTabEntriesSetExternally)
		{
			m_MenuBodies = new BaseMenuBehaviour[m_Buttons.Length];
		}
		else
		{
			m_MaxTabIndex = m_Buttons.Length - 1;
		}
		m_CurrentButtonNormalColors = m_Buttons[m_CurrentTabIndex].colors;
		m_CurrentButtonHighlightColors = m_Buttons[m_CurrentTabIndex].colors;
		m_CurrentButtonSpriteState = m_Buttons[m_CurrentTabIndex].spriteState;
		m_CurrentButtonNormalSprite = ((T17Image)m_Buttons[m_CurrentTabIndex].targetGraphic).sprite;
	}

	protected override void SingleTimeInitialize()
	{
		base.SingleTimeInitialize();
		for (int i = 0; i < m_MenuBodies.Length; i++)
		{
			if (m_MenuBodies[i] != null)
			{
				m_MenuBodies[i].Hide();
			}
		}
		if (base.CurrentGamer != null)
		{
			SetTabIndex(m_CurrentTabIndex);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!m_bAllowIndirectNavigation || base.CurrentRewiredPlayer == null || T17DialogBoxManager.HasDialogsForGamer(base.CurrentGamer))
		{
			return;
		}
		if (base.CurrentRewiredPlayer.GetButtonUp(m_PreviousTabInputAction))
		{
			int num = m_CurrentTabIndex;
			int num2 = m_MenuBodies.Length;
			do
			{
				num = ((num > 0) ? (num - 1) : m_MaxTabIndex);
				num2--;
			}
			while (!CheckMenuEnabled(num) && num2 > 0);
			if (num != m_CurrentTabIndex)
			{
				SetTabIndex(num, delegate(bool bSuccess)
				{
					if (bSuccess)
					{
						SetNavigationLinking(selectFirstItemOnBody: true);
					}
				});
			}
		}
		if (!base.CurrentRewiredPlayer.GetButtonUp(m_NextTabInputAction))
		{
			return;
		}
		int num3 = m_CurrentTabIndex;
		int num4 = m_MenuBodies.Length;
		do
		{
			num3 = ((num3 < m_MaxTabIndex) ? (num3 + 1) : 0);
			num4--;
		}
		while (!CheckMenuEnabled(num3) && num4 > 0);
		if (num3 == m_CurrentTabIndex)
		{
			return;
		}
		SetTabIndex(num3, delegate(bool bSuccess)
		{
			if (bSuccess)
			{
				SetNavigationLinking(selectFirstItemOnBody: true);
			}
		});
	}

	public void SetMenuBodies(List<BaseMenuBehaviour> menus)
	{
		if (menus == null || menus.Count <= 0 || menus.Count > m_MenuBodies.Length)
		{
			return;
		}
		for (int i = 0; i < m_Buttons.Length; i++)
		{
			if (m_Buttons[i] != null)
			{
				m_Buttons[i].gameObject.SetActive(value: false);
				Animator component = m_Buttons[i].GetComponent<Animator>();
				if (component != null)
				{
					component.SetBool("HoldSelected", value: false);
				}
			}
		}
		for (int j = 0; j < m_MenuBodies.Length; j++)
		{
			if (m_MenuBodies[j] != null)
			{
				m_MenuBodies[j].Hide(restoreInvokerState: true, isTabSwitch: true);
			}
		}
		for (int k = 0; k < menus.Count; k++)
		{
			m_MenuBodies[k] = menus[k];
			if (!(m_Buttons[k] != null))
			{
				continue;
			}
			m_Buttons[k].gameObject.SetActive(value: true);
			m_Buttons[k].interactable = m_MenuBodies[k].m_bMenuIsEnabled;
			if (m_ButtonChildImages[k] != null)
			{
				if (m_MenuBodies[k].m_bMenuIsEnabled)
				{
					m_ButtonChildImages[k].sprite = m_MenuBodies[k].m_TabIcon;
				}
				else
				{
					m_ButtonChildImages[k].sprite = m_MenuBodies[k].m_TabIconDisabled;
				}
			}
		}
		m_MaxTabIndex = menus.Count - 1;
	}

	public void SetTabIndex(int index, Action<bool> onCompleted = null, bool bPlayAudio = true)
	{
		bool bPlayAudio2 = m_bPlayAudio;
		m_bPlayAudio &= bPlayAudio;
		if (index >= 0 && index < m_Buttons.Length)
		{
			OnTabButtonClicked(m_Buttons[index], onCompleted);
		}
		m_bPlayAudio = bPlayAudio2;
	}

	public bool CheckMenuEnabled(int index)
	{
		if (m_MenuBodies != null && index >= 0 && index < m_MenuBodies.Length && m_MenuBodies[index] != null)
		{
			return m_MenuBodies[index].m_bMenuIsEnabled;
		}
		return false;
	}

	public void OnTabButtonClicked(T17Button button, Action<bool> onCompleted = null)
	{
		if (m_MenuBodies == null || m_CurrentTabIndex >= m_MenuBodies.Length || m_MenuBodies[m_CurrentTabIndex] == null)
		{
			_OnTabButtonClicked(button);
			if (onCompleted != null)
			{
				onCompleted(obj: true);
			}
			return;
		}
		m_MenuBodies[m_CurrentTabIndex].ConfirmChangeFocus(delegate(bool canChangeFocus)
		{
			if (canChangeFocus)
			{
				_OnTabButtonClicked(button);
				if (onCompleted != null)
				{
					onCompleted(obj: true);
				}
			}
			else if (onCompleted != null)
			{
				onCompleted(obj: false);
			}
		});
	}

	private void _OnTabButtonClicked(T17Button button)
	{
		int i;
		for (i = 0; i < m_Buttons.Length && !(m_Buttons[i] == button); i++)
		{
		}
		if (m_Buttons[m_CurrentTabIndex].animator != null)
		{
			m_Buttons[m_CurrentTabIndex].animator.SetBool("HoldSelected", value: false);
		}
		if (m_bHasEventsEnabled && i < m_MenuDelegates.Length && m_MenuDelegates[i] != null)
		{
			m_MenuDelegates[i].Invoke();
		}
		SwitchToTab(i);
		if (m_bKeepSelectedTabHighlighted)
		{
			if (m_Buttons[m_OldTabIndex].transition == Selectable.Transition.ColorTint)
			{
				m_Buttons[m_OldTabIndex].colors = m_CurrentButtonNormalColors;
			}
			else if (m_Buttons[m_OldTabIndex].transition == Selectable.Transition.SpriteSwap)
			{
				((T17Image)m_Buttons[m_OldTabIndex].targetGraphic).sprite = m_CurrentButtonNormalSprite;
			}
			if (button.transition == Selectable.Transition.ColorTint)
			{
				m_CurrentButtonNormalColors = button.colors;
				m_CurrentButtonHighlightColors = button.colors;
				m_CurrentButtonHighlightColors.normalColor = m_CurrentButtonHighlightColors.highlightedColor;
				m_CurrentButtonHighlightColors.highlightedColor = Color.cyan;
				m_Buttons[m_CurrentTabIndex].colors = m_CurrentButtonHighlightColors;
			}
			else if (button.transition == Selectable.Transition.SpriteSwap)
			{
				m_CurrentButtonSpriteState = button.spriteState;
				m_CurrentButtonNormalSprite = ((T17Image)button.targetGraphic).sprite;
				((T17Image)button.targetGraphic).sprite = m_CurrentButtonSpriteState.pressedSprite;
			}
			else if (button.transition == Selectable.Transition.Animation)
			{
				button.animator.SetBool("HoldSelected", value: true);
			}
		}
	}

	private void SwitchToTab(int tabIndex)
	{
		if (m_MenuBodies[m_CurrentTabIndex] != null)
		{
			m_MenuBodies[m_CurrentTabIndex].Hide(restoreInvokerState: true, isTabSwitch: true);
		}
		if (m_bPlayAudio)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Tab, AudioController.UI_Audio_GO);
		}
		if (tabIndex < m_MenuBodies.Length && m_MenuBodies[tabIndex] != null)
		{
			m_OldTabIndex = m_CurrentTabIndex;
			m_CurrentTabIndex = tabIndex;
			if (m_MenuBodies[m_CurrentTabIndex] != null)
			{
				m_MenuBodies[m_CurrentTabIndex].Show(base.CurrentGamer, this, null);
				m_MenuBodies[m_CurrentTabIndex].SetGamePlayer(base.CurrentGamePlayer);
				SetNavigationLinking(selectFirstItemOnBody: false);
			}
		}
		else
		{
			m_OldTabIndex = m_CurrentTabIndex;
			m_CurrentTabIndex = tabIndex;
		}
		if (m_TabSplitterSprites != null && m_TabSplitter != null && m_CurrentTabIndex < m_TabSplitterSprites.Length)
		{
			m_TabSplitter.sprite = m_TabSplitterSprites[m_CurrentTabIndex];
		}
		if (m_TabTitleTags != null && m_TabTitle != null && m_CurrentTabIndex < m_TabTitleTags.Length)
		{
			m_TabTitle.m_PlaceholderText = m_TabTitleTags[m_CurrentTabIndex];
			m_TabTitle.SetNewLocalizationTag(m_TabTitleTags[m_CurrentTabIndex]);
		}
		GetComponentInParent<IMenuEventDelegate>()?.ChildMenuChanged();
	}

	private void SetNavigationLinking(bool selectFirstItemOnBody)
	{
		if (m_MenuBodies == null || m_CurrentTabIndex >= m_MenuBodies.Length || m_MenuBodies[m_CurrentTabIndex] == null)
		{
			return;
		}
		GameObject selectedGameObject = null;
		switch (m_RelativePositionToBodies)
		{
		case RelativePosition.Left:
			if (m_MenuBodies[m_CurrentTabIndex].m_LeftSelectable != null)
			{
				if (m_bAllowDirectNavigation)
				{
					Navigation navigation = m_MenuBodies[m_CurrentTabIndex].m_LeftSelectable.navigation;
					navigation.selectOnLeft = m_Buttons[m_CurrentTabIndex];
					m_MenuBodies[m_CurrentTabIndex].m_LeftSelectable.navigation = navigation;
					navigation = m_Buttons[m_CurrentTabIndex].navigation;
					navigation.selectOnRight = m_MenuBodies[m_CurrentTabIndex].m_LeftSelectable;
					m_Buttons[m_CurrentTabIndex].navigation = navigation;
				}
				selectedGameObject = m_MenuBodies[m_CurrentTabIndex].m_LeftSelectable.gameObject;
			}
			break;
		case RelativePosition.Right:
			if (m_MenuBodies[m_CurrentTabIndex].m_RightSelectable != null)
			{
				if (m_bAllowDirectNavigation)
				{
					Navigation navigation = m_MenuBodies[m_CurrentTabIndex].m_RightSelectable.navigation;
					navigation.selectOnRight = m_Buttons[m_CurrentTabIndex];
					m_MenuBodies[m_CurrentTabIndex].m_RightSelectable.navigation = navigation;
					navigation = m_Buttons[m_CurrentTabIndex].navigation;
					navigation.selectOnLeft = m_MenuBodies[m_CurrentTabIndex].m_RightSelectable;
					m_Buttons[m_CurrentTabIndex].navigation = navigation;
				}
				selectedGameObject = m_MenuBodies[m_CurrentTabIndex].m_RightSelectable.gameObject;
			}
			break;
		case RelativePosition.Bottom:
			if (m_MenuBodies[m_CurrentTabIndex].m_BottomSelectable != null)
			{
				if (m_bAllowDirectNavigation)
				{
					Navigation navigation = m_MenuBodies[m_CurrentTabIndex].m_BottomSelectable.navigation;
					navigation.selectOnDown = m_Buttons[m_CurrentTabIndex];
					m_MenuBodies[m_CurrentTabIndex].m_BottomSelectable.navigation = navigation;
					navigation = m_Buttons[m_CurrentTabIndex].navigation;
					navigation.selectOnUp = m_MenuBodies[m_CurrentTabIndex].m_BottomSelectable;
					m_Buttons[m_CurrentTabIndex].navigation = navigation;
				}
				selectedGameObject = m_MenuBodies[m_CurrentTabIndex].m_BottomSelectable.gameObject;
			}
			break;
		default:
			if (m_MenuBodies[m_CurrentTabIndex].m_TopSelectable != null)
			{
				if (m_bAllowDirectNavigation)
				{
					Navigation navigation = m_MenuBodies[m_CurrentTabIndex].m_TopSelectable.navigation;
					navigation.selectOnUp = m_Buttons[m_CurrentTabIndex];
					m_MenuBodies[m_CurrentTabIndex].m_TopSelectable.navigation = navigation;
					navigation = m_Buttons[m_CurrentTabIndex].navigation;
					navigation.selectOnDown = m_MenuBodies[m_CurrentTabIndex].m_TopSelectable;
					m_Buttons[m_CurrentTabIndex].navigation = navigation;
				}
				selectedGameObject = m_MenuBodies[m_CurrentTabIndex].m_TopSelectable.gameObject;
			}
			break;
		}
		if ((selectFirstItemOnBody || !m_bAllowDirectNavigation) && EventSystem.current != null && m_Buttons[m_CurrentTabIndex] != null)
		{
			T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(base.CurrentGamer);
			eventSystemForGamer.SetSelectedGameObject(null);
			eventSystemForGamer.SetSelectedGameObject(selectedGameObject);
		}
	}

	private void OnEnable()
	{
		if (EventSystem.current != null && m_Buttons[m_CurrentTabIndex] != null && m_Buttons[m_CurrentTabIndex].transition == Selectable.Transition.Animation)
		{
			if (m_bKeepSelectedTabHighlighted)
			{
				m_Buttons[m_CurrentTabIndex].animator.SetBool("HoldSelected", value: true);
			}
			m_Buttons[m_CurrentTabIndex].animator.SetTrigger(m_Buttons[m_CurrentTabIndex].animationTriggers.highlightedTrigger);
		}
	}

	public BaseMenuBehaviour GetCurrentPage()
	{
		if (m_MenuBodies == null || m_CurrentTabIndex >= m_MenuBodies.Length || m_MenuBodies[m_CurrentTabIndex] == null)
		{
			return null;
		}
		return m_MenuBodies[m_CurrentTabIndex];
	}

	public void AttemptToSetTabIndex(int index, Action<bool> onCompleted = null)
	{
		int num = index;
		if (!CheckMenuEnabled(num))
		{
			int num2 = m_MenuBodies.Length;
			do
			{
				num = ((num > 0) ? (num - 1) : m_MaxTabIndex);
				num2--;
			}
			while (!CheckMenuEnabled(num) && num2 > 0);
		}
		if (CheckMenuEnabled(num))
		{
			SetTabIndex(num, onCompleted);
		}
	}

	public void SelectPreviousTab()
	{
		int num = m_CurrentTabIndex - 1;
		if (num < 0)
		{
			num = 0;
		}
		if (num != m_CurrentTabIndex)
		{
			SetTabIndex(num);
		}
	}

	public void SelectNextTab()
	{
		int num = m_CurrentTabIndex + 1;
		if (num > m_MaxTabIndex)
		{
			num = m_MaxTabIndex;
		}
		if (num != m_CurrentTabIndex)
		{
			SetTabIndex(num);
		}
	}
}
