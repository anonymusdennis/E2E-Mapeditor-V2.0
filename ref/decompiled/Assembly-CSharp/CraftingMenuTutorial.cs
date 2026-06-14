using System.Collections.Generic;
using UnityEngine;

public class CraftingMenuTutorial : IGMTutorialArrowHandler
{
	private CraftingMenu m_CraftingMenu;

	private ItemData m_ItemToCraft;

	private CraftManager m_CraftManager;

	public InGameRootMenu m_RootMenu;

	private T17Button m_TargetButton;

	public Sprite m_CraftTabSprite;

	public Vector2 m_ArrowPositionOffsetMenuTab = Vector2.zero;

	[Range(0f, 360f)]
	public float m_ArrowRotationMenuTab;

	public Vector2 m_ArrowPositionOffsetCraftTab = Vector2.zero;

	[Range(0f, 360f)]
	public float m_ArrowRotationCraftTab;

	public Vector2 m_ArrowPositionOffsetCraftItem = Vector2.zero;

	[Range(0f, 360f)]
	public float m_ArrowRotationCraftItem;

	public Vector2 m_ArrowPositionOffsetCraftButton = Vector2.zero;

	[Range(0f, 360f)]
	public float m_ArrowRotationCraftButton;

	protected virtual void OnDestroy()
	{
		m_CraftManager = null;
	}

	public override void ClearData()
	{
		m_ItemToCraft = null;
	}

	public override IGMTutorialArrowController.IGMTutorial GetTutorialType()
	{
		return IGMTutorialArrowController.IGMTutorial.CraftingMenu;
	}

	public override Transform GetTutorialTargetTransform()
	{
		if (m_RootMenu != null && m_RootMenu.gameObject.activeSelf && m_RootMenu.m_CurrentInGameMenuType == InGameRootMenu.InGameMenuTypeToOpen.MainSelf)
		{
			GameMenuBehaviour gameMenuBehaviour = (GameMenuBehaviour)m_RootMenu.GetCurrentOpenMenu();
			if (gameMenuBehaviour != null && gameMenuBehaviour.m_MenuType != BaseMenuBehaviour.InGameMenuTypes.CraftingMenu && m_TargetButton != null)
			{
				return m_TargetButton.transform;
			}
		}
		if (m_ItemToCraft != null)
		{
			bool flag = CheckItemsInCraftingMenu();
			bool flag2 = CheckPlayerHasCraftItems();
			int recipieIDForItem = CraftManager.GetInstance().GetRecipieIDForItem(m_ItemToCraft);
			bool flag3 = m_CraftingMenu.CheckPlayerAndMenuHasItems(recipieIDForItem);
			if ((flag || flag2 || flag3) && base.gameObject.activeInHierarchy)
			{
				if (flag)
				{
					if (m_CraftingMenu.m_CraftButton != null)
					{
						return m_CraftingMenu.m_CraftButton.transform;
					}
				}
				else if (m_CraftingMenu.CurrentOpenPage == m_CraftingMenu.GetCraftPageNumberForItem(m_ItemToCraft))
				{
					if (flag3 || flag2)
					{
						InventoryItem recipeSlotForItem = m_CraftingMenu.GetRecipeSlotForItem(m_ItemToCraft);
						if (recipeSlotForItem != null)
						{
							return recipeSlotForItem.transform;
						}
					}
				}
				else
				{
					int craftPageNumberForItem = m_CraftingMenu.GetCraftPageNumberForItem(m_ItemToCraft);
					T17Button buttonForCraftPage = m_CraftingMenu.GetButtonForCraftPage(craftPageNumberForItem);
					if (buttonForCraftPage != null)
					{
						return buttonForCraftPage.transform;
					}
				}
			}
		}
		return null;
	}

	public override Sprite GetOverrideSprite()
	{
		if (base.gameObject.activeInHierarchy && m_ItemToCraft != null && m_RootMenu != null && m_RootMenu.gameObject.activeSelf && m_RootMenu.m_CurrentInGameMenuType == InGameRootMenu.InGameMenuTypeToOpen.MainSelf)
		{
			GameMenuBehaviour gameMenuBehaviour = (GameMenuBehaviour)m_RootMenu.GetCurrentOpenMenu();
			if (gameMenuBehaviour != null && gameMenuBehaviour.m_MenuType == BaseMenuBehaviour.InGameMenuTypes.CraftingMenu && !CheckItemsInCraftingMenu() && m_CraftingMenu.CurrentOpenPage != m_CraftingMenu.GetCraftPageNumberForItem(m_ItemToCraft))
			{
				return m_CraftTabSprite;
			}
		}
		return null;
	}

	public override bool IsActive()
	{
		if (base.gameObject.activeInHierarchy)
		{
			return true;
		}
		if (m_RootMenu != null && m_RootMenu.gameObject.activeSelf && m_RootMenu.m_CurrentInGameMenuType == InGameRootMenu.InGameMenuTypeToOpen.MainSelf)
		{
			GameMenuBehaviour gameMenuBehaviour = (GameMenuBehaviour)m_RootMenu.GetCurrentOpenMenu();
			if (gameMenuBehaviour != null && gameMenuBehaviour.m_MenuType != BaseMenuBehaviour.InGameMenuTypes.CraftingMenu)
			{
				return true;
			}
		}
		return false;
	}

	public override void TutorialInit()
	{
		m_CraftingMenu = GetComponent<CraftingMenu>();
		m_CraftManager = CraftManager.GetInstance();
		if (!(m_RootMenu != null) || !m_RootMenu.m_InGameTabableMenuTypes.ContainsKey(InGameRootMenu.InGameMenuTypeToOpen.MainSelf) || !m_RootMenu.m_InGameTabableMenuTypes.ContainsKey(InGameRootMenu.InGameMenuTypeToOpen.MainSelf))
		{
			return;
		}
		List<BaseMenuBehaviour> menus = m_RootMenu.m_InGameTabableMenuTypes[InGameRootMenu.InGameMenuTypeToOpen.MainSelf].m_Menus;
		for (int i = 0; i < menus.Count; i++)
		{
			GameMenuBehaviour gameMenuBehaviour = (GameMenuBehaviour)menus[i];
			if (gameMenuBehaviour != null && gameMenuBehaviour.m_MenuType == BaseMenuBehaviour.InGameMenuTypes.CraftingMenu && i >= 0 && i < m_RootMenu.m_MainTabPanel.m_Buttons.Length)
			{
				m_TargetButton = m_RootMenu.m_MainTabPanel.m_Buttons[i];
			}
		}
	}

	public bool CheckItemsInCraftingMenu()
	{
		if (m_CraftingMenu != null && m_CraftingMenu.gameObject.activeSelf && m_CraftManager != null)
		{
			ItemData[] itemsCurrentlyInSlots = m_CraftingMenu.GetItemsCurrentlyInSlots();
			int[] usingItemIndices = new int[8];
			int recipieIDForItem = m_CraftManager.GetRecipieIDForItem(m_ItemToCraft);
			if (m_CraftManager.HasItemsForRecipe(recipieIDForItem, itemsCurrentlyInSlots, ref usingItemIndices))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckPlayerHasCraftItems()
	{
		if (m_CraftManager != null && m_CraftingMenu != null && m_CraftingMenu.CurrentGamePlayer != null)
		{
			int recipieIDForItem = m_CraftManager.GetRecipieIDForItem(m_ItemToCraft);
			int[] usingItemIndices = new int[3];
			return m_CraftManager.HasItemsForRecipe(recipieIDForItem, m_CraftingMenu.CurrentGamePlayer.m_ItemContainer, ref usingItemIndices);
		}
		return false;
	}

	public override void SetTutorialTarget(List<ItemData> targets)
	{
		if (targets == null || targets.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			if (targets[i] != null)
			{
				m_ItemToCraft = targets[i];
				break;
			}
		}
	}

	private Vector3 GetTargetPositionAsViewport(Vector2 position)
	{
		int width = Screen.width;
		int height = Screen.height;
		Vector2 vector = position;
		vector.x /= width;
		vector.y /= height;
		return vector;
	}
}
