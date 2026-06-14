using System;
using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class BaseMenuBehaviour : T17MonoBehaviour, IMenuEventDelegate
{
	private delegate void BaseMenuBehaviourEvent(BaseMenuBehaviour menu);

	[Serializable]
	public class LegendIconData
	{
		public int m_Size;

		public Vector2 m_Offset;

		public LegendIconData(int size, float offx, float offy)
		{
			m_Size = size;
			m_Offset.Set(offx, offy);
		}
	}

	public enum InGameMenuTypes
	{
		UnSet = -1,
		SelfCharacterInfo = 1,
		CraftingMenu = 2,
		JournalFavours = 4,
		JournalTips = 8,
		ControlsMenu = 16,
		SettingsMenu = 32,
		InmateCharacterInfo = 64,
		InmateGifting = 128,
		InmateShop = 256,
		InmateFavour = 512,
		InmateLooting = 1024,
		DeskInventory = 2048,
		ToiletInventory = 4096,
		SwagBagInventory = 8192,
		CutlreyInventory = 16384,
		MAX = 16385
	}

	public delegate void ConfirmFocusCallback(bool canChangeFocus);

	private Player m_CurrentGamePlayer;

	private T17EventSystem m_CachedEventSystem;

	private Gamer m_CurrentGamer;

	private BaseMenuBehaviourEvent OnHide;

	[HideInInspector]
	public Selectable m_TopSelectable;

	[HideInInspector]
	public Selectable m_BottomSelectable;

	[HideInInspector]
	public Selectable m_LeftSelectable;

	[HideInInspector]
	public Selectable m_RightSelectable;

	[HideInInspector]
	public Sprite m_TabIcon;

	[HideInInspector]
	public Sprite m_TabIconDisabled;

	protected T17TabPanel m_TabPanelController;

	private bool m_bDidSingleTimeInitialize;

	protected GameObject m_ObjectThatInvokedShow;

	protected bool m_bWasInvokerActive = true;

	protected List<BaseMenuBehaviour> m_ChildMenus;

	protected NavigateOnUICancel m_NavigateOnUICancel;

	[HideInInspector]
	public bool m_bShouldBlockParentCancelHandler = true;

	private bool m_bAllowCancelHandeling;

	public Animator m_TransitionAnimator;

	public string m_BackTransition = "TransitionBack";

	public string m_ForwardTransition = "TransitionForward";

	private bool m_bTransitionInProgress;

	private float m_TransitionStartedTimestamp;

	private const float SAFETY_TRANSITION_TIMEOUT = 0.3f;

	public static int AllowedCancelHandlers;

	protected BaseMenuBehaviour m_Parent;

	public static BaseMenuBehaviour LastMenuThatCalledShow;

	public bool m_bMenuIsEnabled = true;

	public bool m_bClearPreviousSelectablesOnShow;

	public bool m_bLogAnalyticOnEnter;

	[HideInInspector]
	public T17Text m_LegendTextItem;

	[HideInInspector]
	public string m_LegendLocalisationTag;

	[HideInInspector]
	[SerializeField]
	public List<LegendIconData> m_LegendIconData;

	public Player CurrentGamePlayer => m_CurrentGamePlayer;

	public T17EventSystem CachedEventSystem
	{
		get
		{
			if (CurrentGamer == null)
			{
				return null;
			}
			return m_CachedEventSystem;
		}
	}

	public Gamer CurrentGamer
	{
		get
		{
			if (m_CurrentGamePlayer != null)
			{
				return m_CurrentGamePlayer.m_Gamer;
			}
			return m_CurrentGamer;
		}
	}

	public Rewired.Player CurrentRewiredPlayer
	{
		get
		{
			if (m_CurrentGamePlayer != null && m_CurrentGamePlayer.m_Gamer != null)
			{
				return m_CurrentGamePlayer.m_Gamer.m_RewiredPlayer;
			}
			if (m_CurrentGamer != null)
			{
				return m_CurrentGamer.m_RewiredPlayer;
			}
			return null;
		}
	}

	public event MenuChangedHandler MenuChangedEvent;

	protected override void Awake()
	{
		base.Awake();
	}

	protected virtual void OnDestroy()
	{
		m_CurrentGamePlayer = null;
		m_CachedEventSystem = null;
		m_CurrentGamer = null;
		OnHide = null;
		m_TopSelectable = null;
		m_BottomSelectable = null;
		m_LeftSelectable = null;
		m_RightSelectable = null;
		m_TabIcon = null;
		m_TabIconDisabled = null;
		m_TabPanelController = null;
		m_ObjectThatInvokedShow = null;
		if (m_ChildMenus != null)
		{
			m_ChildMenus.Clear();
		}
		m_ChildMenus = null;
		m_NavigateOnUICancel = null;
		m_LegendIconData.Clear();
		m_TransitionAnimator = null;
		m_Parent = null;
		this.MenuChangedEvent = null;
		LastMenuThatCalledShow = null;
		m_LegendTextItem = null;
		if (LastMenuThatCalledShow == this)
		{
			LastMenuThatCalledShow = null;
		}
	}

	protected virtual void Start()
	{
		if (!m_bDidSingleTimeInitialize)
		{
			SingleTimeInitialize();
		}
	}

	protected virtual void Update()
	{
		if (m_bTransitionInProgress && Time.unscaledTime - m_TransitionStartedTimestamp > 0.3f)
		{
			m_bTransitionInProgress = false;
		}
		if (CurrentRewiredPlayer != null && CurrentRewiredPlayer.GetButtonUp("UI_Cancel"))
		{
			UICancel();
		}
	}

	public void UICancel()
	{
		if (!FriendsContextMenu.IsContextMenuOpen && m_bAllowCancelHandeling && !(m_NavigateOnUICancel == null) && !T17DialogBoxManager.HasDialogsForGamer(CurrentGamer))
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Reject, AudioController.UI_Audio_GO);
			m_NavigateOnUICancel.m_DoThisOnUICancel.Invoke();
		}
	}

	public virtual void SetGamePlayer(Player gamePlayer)
	{
		if (gamePlayer != null)
		{
			m_CurrentGamePlayer = gamePlayer;
		}
	}

	public void DoSingleTimeInitialize()
	{
		if (!m_bDidSingleTimeInitialize)
		{
			SingleTimeInitialize();
		}
	}

	protected virtual void SingleTimeInitialize()
	{
		m_bDidSingleTimeInitialize = true;
		m_NavigateOnUICancel = GetComponent<NavigateOnUICancel>();
		if (m_TransitionAnimator == null)
		{
			m_TransitionAnimator = GetComponent<Animator>();
		}
	}

	private void DEBUG_ExceptionInBaseMenu(string text)
	{
		string text2 = "DEBUGGING NullReferenceException in BaseMenuBehaviiour: " + text + "\n\nOn" + base.transform.name + " under " + DEBUG_PrintTransformHeirarchy(base.transform) + "\nIn global start state " + GlobalStart.GetInstance().CurrentGlobalStartMode;
		text2 += "PlayerDump:\n";
		List<Player> allPlayers = Player.GetAllPlayers();
		for (int i = 0; i < allPlayers.Count; i++)
		{
			text2 = text2 + i + "index ";
			if (allPlayers[i] == null)
			{
				text2 += " is null";
			}
			else
			{
				text2 += " is not null";
				text2 = ((allPlayers[i].m_Gamer != null) ? (text2 + " and their gamer isn't null") : (text2 + " and their gamer is null"));
			}
			text2 += "\n\n";
		}
		T17NetManager.LogGoogleException(text2);
	}

	private string DEBUG_PrintTransformHeirarchy(Transform theTransform)
	{
		string text = string.Empty;
		Transform transform = theTransform;
		while (transform.parent != null)
		{
			text = text + transform.parent.name + "/";
			transform = transform.parent;
		}
		return text;
	}

	public virtual bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		m_Parent = parent;
		SetGamer(currentGamer);
		m_ObjectThatInvokedShow = invoker;
		if (!m_bDidSingleTimeInitialize)
		{
			SingleTimeInitialize();
		}
		if (m_bShouldBlockParentCancelHandler && m_NavigateOnUICancel != null)
		{
			m_bAllowCancelHandeling = true;
			AllowedCancelHandlers++;
		}
		if (m_ObjectThatInvokedShow != null)
		{
			m_bWasInvokerActive = m_ObjectThatInvokedShow.activeSelf;
		}
		if (parent != null)
		{
			parent.AddChildMenu(this);
			if (m_bShouldBlockParentCancelHandler && parent.m_NavigateOnUICancel != null)
			{
				parent.m_bAllowCancelHandeling = false;
				AllowedCancelHandlers--;
			}
		}
		UpdateLegendText();
		if (m_bClearPreviousSelectablesOnShow)
		{
			ClearPreviousSelectables();
		}
		if (!base.gameObject.activeInHierarchy)
		{
			if (hideInvoker && m_ObjectThatInvokedShow != null)
			{
				m_ObjectThatInvokedShow.SetActive(value: false);
			}
			base.gameObject.SetActive(value: true);
			if (this.MenuChangedEvent != null)
			{
				this.MenuChangedEvent();
			}
			if (m_Parent != null)
			{
				m_Parent.ChildMenuChanged(this, this);
			}
			LastMenuThatCalledShow = this;
			BaseMenuBehaviour[] componentsInChildren = base.gameObject.GetComponentsInChildren<BaseMenuBehaviour>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].SetGamer(currentGamer);
			}
			return true;
		}
		return false;
	}

	public void SetGamer(Gamer currentGamer)
	{
		m_CurrentGamer = currentGamer;
		if (m_CurrentGamer == null)
		{
			DEBUG_ExceptionInBaseMenu("Current gamer is null");
			m_CachedEventSystem = null;
		}
		else
		{
			m_CachedEventSystem = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_CurrentGamer);
		}
		if (null != m_CurrentGamer.m_PlayerObject)
		{
			m_CurrentGamePlayer = m_CurrentGamer.m_PlayerObject;
		}
	}

	private void UpdateLegendText()
	{
		if (!(m_LegendTextItem != null))
		{
			return;
		}
		m_LegendTextItem.SetNewPlaceHolder(m_LegendLocalisationTag);
		m_LegendTextItem.SetNewLocalizationTag(m_LegendLocalisationTag);
		if (m_LegendIconData.Count > 0)
		{
			m_LegendTextItem.m_PCIconOverrideSize = m_LegendIconData[m_LegendIconData.Count - 1].m_Size;
		}
		for (int i = 0; i < m_LegendTextItem.m_ImagesAttached.Count; i++)
		{
			if (i < m_LegendIconData.Count)
			{
				m_LegendTextItem.m_ImagesAttached[i].Size = m_LegendIconData[i].m_Size;
			}
			else if (m_LegendIconData.Count > 0)
			{
				m_LegendTextItem.m_ImagesAttached[i].Size = m_LegendIconData[m_LegendIconData.Count - 1].m_Size;
			}
		}
		m_LegendTextItem.Convert();
		m_LegendTextItem.SetVerticesDirty();
	}

	public virtual bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (base.gameObject.activeSelf || isTabSwitch)
		{
			m_bTransitionInProgress = false;
			if (m_NavigateOnUICancel != null)
			{
				m_bAllowCancelHandeling = false;
				AllowedCancelHandlers--;
			}
			if (m_ObjectThatInvokedShow != null && restoreInvokerState)
			{
				m_ObjectThatInvokedShow.SetActive(m_bWasInvokerActive);
			}
			base.gameObject.SetActive(value: false);
			if (m_ChildMenus != null)
			{
				for (int num = m_ChildMenus.Count - 1; num >= 0; num--)
				{
					m_ChildMenus[num].Hide(restoreInvokerState, isTabSwitch);
				}
			}
			if (m_LegendTextItem != null)
			{
				m_LegendTextItem.SetLocalisedTextCatchAll(string.Empty);
				m_LegendTextItem.m_PCIconOverrideSize = -1;
			}
			if (OnHide != null)
			{
				OnHide(this);
			}
			m_CurrentGamer = null;
			m_CachedEventSystem = null;
			m_CurrentGamePlayer = null;
			if (this.MenuChangedEvent != null)
			{
				this.MenuChangedEvent();
			}
			if (m_Parent != null)
			{
				m_Parent.ChildMenuChanged(this, this);
			}
			return true;
		}
		return false;
	}

	protected virtual void AddChildMenu(BaseMenuBehaviour childMenu)
	{
		if (m_ChildMenus == null)
		{
			m_ChildMenus = new List<BaseMenuBehaviour>();
		}
		if (!m_ChildMenus.Contains(childMenu))
		{
			childMenu.OnHide = (BaseMenuBehaviourEvent)Delegate.Combine(childMenu.OnHide, new BaseMenuBehaviourEvent(OnChildHide));
			m_ChildMenus.Add(childMenu);
		}
	}

	private void OnChildHide(BaseMenuBehaviour childMenu)
	{
		if (m_ChildMenus.Contains(childMenu))
		{
			childMenu.OnHide = null;
			m_ChildMenus.Remove(childMenu);
			if (childMenu.m_bShouldBlockParentCancelHandler && m_NavigateOnUICancel != null)
			{
				m_bAllowCancelHandeling = true;
				AllowedCancelHandlers++;
			}
			LastMenuThatCalledShow = this;
			UpdateLegendText();
		}
	}

	public void GetChildMenus(out List<BaseMenuBehaviour> childList)
	{
		childList = m_ChildMenus;
	}

	protected void RaiseMenuChangedEvent()
	{
		if (this.MenuChangedEvent != null)
		{
			this.MenuChangedEvent();
		}
		if (m_Parent != null)
		{
			m_Parent.ChildMenuChanged(this);
		}
	}

	public void ChildMenuChanged(IMenuEventDelegate sender = null, IMenuEventDelegate changedItem = null)
	{
		RaiseMenuChangedEvent();
		if (m_Parent != null)
		{
			m_Parent.ChildMenuChanged(this, changedItem);
		}
	}

	protected void DisableSmokesOnInventoryItems(InventoryItem[] items)
	{
		if (items != null)
		{
			for (int i = 0; i < items.Length; i++)
			{
				items[i].StopSmoke();
			}
		}
	}

	public void PlayForwardTransition()
	{
		PlayTransition(m_ForwardTransition);
	}

	public void PlayBackTransition()
	{
		PlayTransition(m_BackTransition);
	}

	private void PlayTransition(string trigger)
	{
		if (m_TransitionAnimator != null)
		{
			m_TransitionAnimator.SetTrigger(trigger);
			m_bTransitionInProgress = true;
		}
	}

	public virtual void ConfirmChangeFocus(ConfirmFocusCallback confirmCallback)
	{
		confirmCallback?.Invoke(canChangeFocus: true);
	}

	public void SetNewLegendTag(string newTag, bool updatePlaceholderText = true)
	{
		m_LegendLocalisationTag = newTag;
		if (!(m_LegendTextItem != null))
		{
			return;
		}
		if (updatePlaceholderText)
		{
			m_LegendTextItem.SetNewPlaceHolder(newTag);
		}
		m_LegendTextItem.SetNewLocalizationTag(newTag);
		m_LegendTextItem.SetVerticesDirty();
		m_LegendTextItem.UpdateQuadImage();
		if (!m_LegendTextItem.HasIcons)
		{
			return;
		}
		if (m_LegendIconData == null)
		{
			m_LegendIconData = new List<LegendIconData>(m_LegendTextItem.m_ImagesAttached.Count);
		}
		int num = 0;
		for (int i = 0; i < m_LegendTextItem.m_ImagesAttached.Count; i++)
		{
			T17Text.ImageData imageData = m_LegendTextItem.m_ImagesAttached[i];
			if (!imageData.NeedsToBeDeleted)
			{
				if (num >= m_LegendIconData.Count)
				{
					m_LegendIconData.Add(new LegendIconData(imageData.Size, imageData.Offset.x, imageData.Offset.y));
				}
				else
				{
					m_LegendTextItem.m_ImagesAttached[i].Size = m_LegendIconData[num].m_Size;
				}
				num++;
			}
		}
		if (m_LegendIconData.Count > 0 && num < m_LegendIconData.Count)
		{
			m_LegendIconData.RemoveRange(num, m_LegendIconData.Count - num);
		}
		m_LegendTextItem.Convert();
		m_LegendTextItem.SetVerticesDirty();
	}

	public void SetLegendIconSize(int index, int size)
	{
		if (index < m_LegendIconData.Count)
		{
			m_LegendIconData[index].m_Size = size;
			if (m_LegendTextItem != null && index < m_LegendTextItem.m_ImagesAttached.Count)
			{
				m_LegendTextItem.m_ImagesAttached[index].Size = size;
				m_LegendTextItem.Convert();
				m_LegendTextItem.SetVerticesDirty();
			}
		}
	}

	public void SetLegendIconOffset(int index, Vector2 offset)
	{
		if (index < m_LegendIconData.Count)
		{
			m_LegendIconData[index].m_Offset = offset;
			if (m_LegendTextItem != null && index < m_LegendTextItem.m_ImagesAttached.Count)
			{
				m_LegendTextItem.m_ImagesAttached[index].Offset = offset;
				m_LegendTextItem.Convert();
				m_LegendTextItem.SetVerticesDirty();
			}
		}
	}

	protected void ClearPreviousSelectables()
	{
		if (CurrentGamer == null)
		{
			return;
		}
		T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(CurrentGamer);
		if (eventSystemForGamer != null)
		{
			T17RewiredStandaloneInputModule t17RewiredStandaloneInputModule = (T17RewiredStandaloneInputModule)eventSystemForGamer.m_RewiredInputModule;
			if (t17RewiredStandaloneInputModule != null)
			{
				t17RewiredStandaloneInputModule.ClearPreviousSelectables();
			}
		}
	}

	public bool HasPerformedFirstTimeInitialise()
	{
		return m_bDidSingleTimeInitialize;
	}

	private void TransitionAnimationStarted()
	{
		ForceSetTransitionInProgress(inProgress: true);
	}

	private void TransitionAnimationFinished()
	{
		ForceSetTransitionInProgress(inProgress: false);
	}

	public void ForceSetTransitionInProgress(bool inProgress)
	{
		m_bTransitionInProgress = inProgress;
		if (inProgress)
		{
			m_TransitionStartedTimestamp = Time.unscaledTime;
		}
	}

	public bool IsTransitionInProgress()
	{
		return m_bTransitionInProgress;
	}
}
