using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class DebugMenu : MonoBehaviour
{
	private class DebugItem
	{
		public DebugMenuMenuObject m_MenuItem;

		public int m_ID;

		public bool m_bRequiresConfirm;

		public ItemToggle m_OnItemToggle;

		public bool m_bTogglePositive;

		public ItemRange m_OnItemRange;

		public ItemRangeQuery m_ItemRangeQuery;

		public int m_RangeValue;

		public ListPickSize m_OnListSize;

		public ListPick m_OnListPick;

		public int m_MinVal;

		public int m_MaxVal;

		public bool m_bAllowLooping;

		public ItemCall m_OnItemCall;

		public bool m_Called;

		public bool m_bIsFolder;

		public bool m_bFolderOpen;

		public DebugItem m_Folder;

		public bool m_bAutoCloseDebugMenu;

		public ItemTextCall m_updateTextCall;
	}

	public delegate bool ItemToggle(bool bPos, bool bJustRead);

	public class ItemData
	{
		public string m_Name;

		public bool m_bAutoCloseDebugMenu;

		public ItemData(string name)
		{
			m_Name = name;
		}

		public ItemData()
		{
		}
	}

	public class ItemDataToggle : ItemData
	{
		public ItemToggle m_OnItemToggle;

		public bool m_bRequiresConfirm;

		public ItemDataToggle(string name)
		{
			m_Name = name;
		}

		public ItemDataToggle(string name, ItemToggle it)
		{
			m_Name = name;
			m_OnItemToggle = it;
		}

		public ItemDataToggle(string name, ItemToggle it, bool bRequiresConfirm)
		{
			m_Name = name;
			m_OnItemToggle = it;
			m_bRequiresConfirm = bRequiresConfirm;
		}
	}

	public delegate void ItemCall();

	public delegate string ItemTextCall();

	public class ItemDataCall : ItemData
	{
		public ItemCall m_OnItemCall;

		public ItemDataCall(string name)
		{
			m_Name = name;
		}

		public ItemDataCall(string name, ItemCall it)
		{
			m_Name = name;
			m_OnItemCall = it;
		}
	}

	public delegate int ItemRange(int val, bool bJustRead);

	public delegate void ItemRangeQuery(ref int minVal, ref int maxVal);

	public class ItemDataIntRange : ItemData
	{
		public ItemRange m_OnItemRange;

		public ItemRangeQuery m_ItemRangeQuery;

		public int m_MinVal;

		public int m_MaxVal;

		public ItemDataIntRange(string name)
		{
			m_Name = name;
		}

		public ItemDataIntRange(string name, ItemRange ir, int min, int max)
		{
			m_Name = name;
			m_OnItemRange = ir;
			m_MinVal = min;
			m_MaxVal = max;
		}

		public ItemDataIntRange(string name, ItemRange ir, ItemRangeQuery iq, int min, int max)
		{
			m_Name = name;
			m_OnItemRange = ir;
			m_ItemRangeQuery = iq;
			m_MinVal = min;
			m_MaxVal = max;
		}
	}

	public delegate string ListPick(int val, bool bJustRead);

	public delegate int ListPickSize();

	public class ItemDataListPick : ItemData
	{
		public ListPick m_OnListPick;

		public ListPickSize m_OnListSize;

		public ItemDataListPick(string name)
		{
			m_Name = name;
		}

		public ItemDataListPick(string name, ListPick lp, ListPickSize ls)
		{
			m_Name = name;
			m_OnListPick = lp;
			m_OnListSize = ls;
		}

		public ItemDataListPick(string name, ListPick lp, ListPickSize ls, bool bCloseDebugMenu)
		{
			m_Name = name;
			m_OnListPick = lp;
			m_OnListSize = ls;
			m_bAutoCloseDebugMenu = bCloseDebugMenu;
		}
	}

	public class ItemDataUpdateableText : ItemData
	{
		public ItemTextCall m_TextUpdateMethod;

		public ItemDataUpdateableText(string name, ItemTextCall textUpdateMethod)
		{
			m_Name = name;
			m_TextUpdateMethod = textUpdateMethod;
		}
	}

	public class ItemDataFolder : ItemData
	{
		public ItemDataFolder(string folderName)
		{
			m_Name = folderName;
		}
	}

	public class ItemDataFolderEnd : ItemData
	{
	}

	public static DebugMenu m_Instance;

	public float HighlightChangeDelay = 0.1f;

	public GameObject m_ScrollContentObject;

	public GameObject m_MenuItemPrefab;

	public GameObject m_MenuItemSlider;

	public GameObject m_MenuWithChildsPrefab;

	public Color m_HighLightedColour;

	public Color m_NormalColour;

	public Color m_FolderColour = Color.yellow;

	public Color m_EdittingColour;

	public T17Text m_LocalizationHints;

	public List<global::ItemData> m_DebugItems = new List<global::ItemData>();

	[HideInInspector]
	public Player m_CurrentPlayer;

	private List<DebugItem> m_ItemTree = new List<DebugItem>();

	private GameObject m_Confirm;

	private GameObject m_ConfirmNoItem;

	private GameObject m_ConfirmYesItem;

	private GameObject m_ConfirmQues;

	private static int HIGHEST_ID;

	private bool[,] m_RewiredMaps = new bool[4, 10];

	private int m_Highlighted = -1;

	private bool m_bAxisLock;

	private bool m_bItemLock;

	private int m_ItemVal;

	private int m_ItemRangeMin;

	private int m_ItemRangeMax = 100;

	private bool m_bAllowLooping;

	private bool m_bShowingConfirm;

	private bool m_bConfirmHighNo = true;

	private int m_NumberOfClosedItems;

	private int[] m_LastValues;

	private float m_RepeatTimer;

	public float m_InitRepeatDelay = 0.5f;

	private float m_CurrentRepeatDelay = 0.5f;

	private float tNextSelectTime;

	private bool m_bAddToCoolItems = true;

	private bool m_bShowing;

	public ScrollRect m_ScrollRect;

	private readonly string[] DEBUG_COMBO = new string[2] { "DebugMenu1", "DebugMenu2" };

	private void Awake()
	{
		if (m_Instance != null)
		{
			Object.Destroy(this);
		}
		else
		{
			m_Instance = this;
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	private void Start()
	{
		HideDebugMenu(bDoInput: false);
	}

	public void HideDebugMenu(bool bDoInput)
	{
		m_bShowing = false;
		foreach (Transform item in base.transform)
		{
			item.gameObject.SetActive(value: false);
		}
		for (int i = 0; i < m_ItemTree.Count; i++)
		{
			if (m_ItemTree[i].m_OnListPick != null)
			{
				m_LastValues[i] = m_ItemTree[i].m_RangeValue;
			}
		}
		if (bDoInput)
		{
			for (int i = 0; i < ReInput.players.playerCount; i++)
			{
				ReInput.players.GetPlayer(i).controllers.maps.SetMapsEnabled(m_RewiredMaps[i, 0], "InGame");
				ReInput.players.GetPlayer(i).controllers.maps.SetMapsEnabled(m_RewiredMaps[i, 1], "UI");
				ReInput.players.GetPlayer(i).controllers.maps.SetMapsEnabled(m_RewiredMaps[i, 2], "DebugKeys");
				ReInput.players.GetPlayer(i).controllers.maps.SetMapsEnabled(m_RewiredMaps[i, 3], "Assignment");
				ReInput.players.GetPlayer(i).controllers.maps.SetMapsEnabled(m_RewiredMaps[i, 4], "Default");
				ReInput.players.GetPlayer(i).controllers.maps.SetMapsEnabled(m_RewiredMaps[i, 5], "MainMap");
			}
		}
	}
}
