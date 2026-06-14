using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuBehaviour : BaseMenuBehaviour
{
	public struct SelectableTo
	{
		public Selectable selectAble;

		public Direction direction;
	}

	public struct GameMenuInformation
	{
		public Character m_MenuRepresentative;

		public ItemContainer m_MenuRepresentativeContainer;

		public Player m_Player;

		public BaseInventoryBehaviour m_PlayerInventoryBehaviour;

		public ItemContainer m_PlayerItemContainer;

		public PlayerInventoryMenu m_PlayerInventoryMenu;
	}

	public enum Direction
	{
		Up,
		Left,
		Right,
		Down
	}

	public InGameMenuTypes m_MenuType = InGameMenuTypes.UnSet;

	public T17MenuBody m_MenuBody;

	public string MenuName = "GIVE ME A TITLE!";

	private GameObject m_InventoryLinks;

	private Selectable[] m_LinksFromInventory;

	private List<List<SelectableTo>> m_LinksToInventory;

	protected GameMenuInformation m_GameMenuInformation;

	[Header("Player Inventory Tooltip override")]
	public InventoryItem.ContainerSetup m_PlayerInventorySetup;

	protected override void Awake()
	{
		base.Awake();
		Transform transform = base.transform.Find("InventoryLinks");
		if (transform != null)
		{
			m_InventoryLinks = transform.gameObject;
			m_InventoryLinks.SetActive(value: false);
		}
		SetupLinkData();
	}

	protected override void Start()
	{
		base.Start();
		if (m_InventoryLinks == null)
		{
			Transform transform = base.transform.Find("InventoryLinks");
			if (transform != null)
			{
				m_InventoryLinks = transform.gameObject;
				m_InventoryLinks.SetActive(value: false);
			}
		}
		SetupLinkData();
	}

	protected override void Update()
	{
		base.Update();
	}

	private void SetupLinkData()
	{
		if (m_LinksToInventory != null || !(m_InventoryLinks != null))
		{
			return;
		}
		m_LinksFromInventory = m_InventoryLinks.GetComponentsInChildren<Selectable>(includeInactive: true);
		m_LinksToInventory = new List<List<SelectableTo>>();
		for (int i = 0; i < m_LinksFromInventory.Length; i++)
		{
			m_LinksToInventory.Add(new List<SelectableTo>());
		}
		Selectable[] componentsInChildren = base.gameObject.GetComponentsInChildren<Selectable>(includeInactive: true);
		SelectableTo item = default(SelectableTo);
		SelectableTo item2 = default(SelectableTo);
		SelectableTo item3 = default(SelectableTo);
		SelectableTo item4 = default(SelectableTo);
		foreach (Selectable currentSelectable in componentsInChildren)
		{
			if (!(m_LinksFromInventory.FirstOrDefault((Selectable sl) => sl == currentSelectable) == null))
			{
				continue;
			}
			for (int k = 0; k < m_LinksFromInventory.Length; k++)
			{
				if (currentSelectable.navigation.selectOnUp == m_LinksFromInventory[k])
				{
					item.selectAble = currentSelectable;
					item.direction = Direction.Up;
					m_LinksToInventory[k].Add(item);
				}
				if (currentSelectable.navigation.selectOnLeft == m_LinksFromInventory[k])
				{
					item2.selectAble = currentSelectable;
					item2.direction = Direction.Left;
					m_LinksToInventory[k].Add(item2);
				}
				if (currentSelectable.navigation.selectOnRight == m_LinksFromInventory[k])
				{
					item3.selectAble = currentSelectable;
					item3.direction = Direction.Right;
					m_LinksToInventory[k].Add(item3);
				}
				if (currentSelectable.navigation.selectOnDown == m_LinksFromInventory[k])
				{
					item4.selectAble = currentSelectable;
					item4.direction = Direction.Down;
					m_LinksToInventory[k].Add(item4);
				}
			}
		}
	}

	private void SetupLinksToInventory()
	{
		if (m_LinksToInventory == null)
		{
			return;
		}
		for (int i = 0; i < m_LinksToInventory.Count; i++)
		{
			Selectable linkToInventoryObject = m_MenuBody.m_InventoryObject.GetLinkToInventoryObject(i);
			for (int j = 0; j < m_LinksToInventory[i].Count; j++)
			{
				SelectableTo selectableTo = m_LinksToInventory[i][j];
				Navigation navigation = selectableTo.selectAble.navigation;
				switch (selectableTo.direction)
				{
				case Direction.Up:
					navigation.selectOnUp = linkToInventoryObject;
					break;
				case Direction.Left:
					navigation.selectOnLeft = linkToInventoryObject;
					break;
				case Direction.Right:
					navigation.selectOnRight = linkToInventoryObject;
					break;
				default:
					navigation.selectOnDown = linkToInventoryObject;
					break;
				}
				selectableTo.selectAble.navigation = navigation;
			}
		}
	}

	private void SetupLinksFromInventory()
	{
		if (m_LinksFromInventory != null)
		{
			for (int i = 0; i < m_LinksFromInventory.Length; i++)
			{
				m_MenuBody.m_InventoryObject.SetLinkFromInventoryToObject(m_LinksFromInventory[i].navigation.selectOnUp, i);
			}
		}
	}

	private void UnsetLinksFromInventory()
	{
		if (m_LinksFromInventory != null)
		{
			for (int i = 0; i < m_LinksFromInventory.Length; i++)
			{
				m_MenuBody.m_InventoryObject.SetLinkFromInventoryToObject(null, i);
			}
		}
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		if (m_MenuBody != null)
		{
			SetMenuName(MenuName);
			SetupLinksFromInventory();
			SetupLinksToInventory();
		}
		if (m_PlayerInventorySetup != null && m_GameMenuInformation.m_PlayerInventoryMenu != null)
		{
			m_GameMenuInformation.m_PlayerInventoryMenu.SetInventoryItemSetups(m_PlayerInventorySetup);
		}
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		if (m_MenuBody != null)
		{
			UnsetLinksFromInventory();
		}
		return true;
	}

	public void SetMenuName(string menuName, bool localize = true)
	{
		MenuName = menuName;
		if (m_MenuBody != null && m_MenuBody.m_MenuTitle != null)
		{
			if (localize)
			{
				m_MenuBody.m_MenuTitle.m_bNeedsLocalization = true;
				m_MenuBody.m_MenuTitle.SetNewPlaceHolder(MenuName);
				m_MenuBody.m_MenuTitle.SetNewLocalizationTag(MenuName);
			}
			else
			{
				m_MenuBody.m_MenuTitle.m_bNeedsLocalization = false;
				m_MenuBody.m_MenuTitle.text = MenuName;
			}
		}
	}

	public void SetGameMenuInformation(GameMenuInformation gameMenuInformation)
	{
		m_GameMenuInformation = gameMenuInformation;
	}
}
