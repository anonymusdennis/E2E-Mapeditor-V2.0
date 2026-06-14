using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RootMenu : BaseMenuBehaviour
{
	public enum RootMenuType
	{
		FrontEnd,
		InGame,
		HUD,
		Results,
		LevelEditor
	}

	public enum FontTypes
	{
		Title,
		SubTitle,
		Header,
		SubHeader,
		NormalText,
		Numbers
	}

	[Serializable]
	public class EditorHack_BaseMenuBehaviour
	{
		public int m_DefaultTab;

		public bool m_bIsExpanded;

		public List<BaseMenuBehaviour> menus;
	}

	[Serializable]
	public class MenuList_Container
	{
		public int m_DefaultTab;

		public List<BaseMenuBehaviour> m_Menus;
	}

	public T17TabPanel m_MainTabPanel;

	public RootMenuType m_RootMenuType;

	public LegendButtonsManager m_LegendButtonsManager;

	[HideInInspector]
	public EditorHack_BaseMenuBehaviour[] m_EditorTabAbleMenuTypes;

	[SerializeField]
	[HideInInspector]
	public TextFontTypes m_FontTypesForRootMenu;

	private BaseMenuBehaviour[] m_AllBaseMenuBehaviours;

	private IT17EventHelper[] m_EventHelperInterfaces;

	protected PlayerInventoryMenu m_PlayerInventoryOnThisRoot;

	protected bool m_bIsDataInitialized;

	protected override void Awake()
	{
		base.Awake();
		InitializeData();
	}

	protected override void OnDestroy()
	{
		m_MainTabPanel = null;
		m_LegendButtonsManager = null;
		m_EditorTabAbleMenuTypes = null;
		m_FontTypesForRootMenu = null;
		m_AllBaseMenuBehaviours = null;
		m_EventHelperInterfaces = null;
		m_PlayerInventoryOnThisRoot = null;
		base.OnDestroy();
	}

	public virtual void InitializeData()
	{
		if (m_bIsDataInitialized)
		{
			return;
		}
		m_bIsDataInitialized = true;
		m_AllBaseMenuBehaviours = GetComponentsInChildren<BaseMenuBehaviour>(includeInactive: true);
		m_EventHelperInterfaces = GetComponentsInChildren<IT17EventHelper>(includeInactive: true);
		m_PlayerInventoryOnThisRoot = GetComponentInChildren<PlayerInventoryMenu>(includeInactive: true);
		if (m_FontTypesForRootMenu != null)
		{
			T17Text[] componentsInChildren = GetComponentsInChildren<T17Text>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (!(componentsInChildren[i] != null))
				{
					continue;
				}
				int fontType = (int)componentsInChildren[i].m_FontType;
				if (fontType >= 0 && fontType < m_FontTypesForRootMenu.m_SerializedFonts.Length)
				{
					Font font = m_FontTypesForRootMenu.m_SerializedFonts[fontType];
					if (font != null)
					{
						componentsInChildren[i].font = m_FontTypesForRootMenu.m_SerializedFonts[fontType];
					}
				}
			}
		}
		if (m_AllBaseMenuBehaviours == null)
		{
			return;
		}
		for (int j = 0; j < m_AllBaseMenuBehaviours.Length; j++)
		{
			if (m_AllBaseMenuBehaviours[j] != this)
			{
				m_AllBaseMenuBehaviours[j].DoSingleTimeInitialize();
			}
		}
	}

	protected override void Start()
	{
		base.Start();
	}

	public virtual BaseMenuBehaviour GetMenuOFType<T>(RootMenuType typeofMenus)
	{
		return null;
	}

	public virtual int GetTabNumberOfType<T>()
	{
		return -1;
	}

	public virtual BaseMenuBehaviour GetCurrentOpenMenu()
	{
		return null;
	}

	public virtual List<BaseMenuBehaviour> GetCurrentMenuSet()
	{
		return null;
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		if (m_EventHelperInterfaces != null)
		{
			T17EventSystem gamersEventSystem = null;
			if (currentGamer != null)
			{
				gamersEventSystem = T17EventSystemsManager.Instance.GetEventSystemForGamer(currentGamer);
			}
			for (int i = 0; i < m_EventHelperInterfaces.Length; i++)
			{
				if (m_EventHelperInterfaces[i] != null && currentGamer != null)
				{
					m_EventHelperInterfaces[i].SetGamerForEventSystem(currentGamer, gamersEventSystem);
				}
			}
		}
		if (m_PlayerInventoryOnThisRoot != null)
		{
			m_PlayerInventoryOnThisRoot.Show(currentGamer, this, null);
		}
		if (m_LegendButtonsManager != null)
		{
			m_LegendButtonsManager.RequestButtonUptate();
		}
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		if (m_MainTabPanel != null)
		{
		}
		return true;
	}

	public virtual void SetGamePlayerForMenus(Player gamePlayer)
	{
		if (m_PlayerInventoryOnThisRoot != null)
		{
			m_PlayerInventoryOnThisRoot.SetGamePlayer(gamePlayer);
		}
		if (m_AllBaseMenuBehaviours == null)
		{
			return;
		}
		for (int i = 0; i < m_AllBaseMenuBehaviours.Length; i++)
		{
			if (m_AllBaseMenuBehaviours[i] != this)
			{
				m_AllBaseMenuBehaviours[i].SetGamePlayer(gamePlayer);
			}
		}
	}

	public void OnUICancel()
	{
		BaseMenuBehaviour currentOpenMenu = GetCurrentOpenMenu();
		if (currentOpenMenu != null)
		{
			currentOpenMenu.UICancel();
		}
	}
}
