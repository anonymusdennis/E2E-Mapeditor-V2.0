using System;
using System.Collections.Generic;
using UnityEngine;

public class CraftManager : MonoBehaviour
{
	public delegate void CraftEvent(Item itemCrafted, Character crafter);

	[Serializable]
	public class Recipe
	{
		public ItemData m_Product;

		public ItemData[] m_Ingredients = new ItemData[3];

		public bool[] m_IngredientsToBeDestroyed = new bool[3] { true, true, true };

		public LevelScript.PRISON_ENUM_MASK m_PrisonMask = (LevelScript.PRISON_ENUM_MASK)Enum.ToObject(typeof(LevelScript.PRISON_ENUM_MASK), -1);

		public int m_Intellect = 10;

		public int m_CraftedCount;

		public bool m_bHidden;

		public bool m_bDiscovered;

		public int GetIngredientCount()
		{
			int num = 0;
			for (int i = 0; i < m_Ingredients.Length; i++)
			{
				if (m_Ingredients[i] != null)
				{
					num++;
				}
			}
			return num;
		}

		public int GetDestroyableIngredientCount()
		{
			int num = 0;
			for (int i = 0; i < m_Ingredients.Length; i++)
			{
				if (m_Ingredients[i] != null && m_IngredientsToBeDestroyed[i])
				{
					num++;
				}
			}
			return num;
		}
	}

	public struct CraftInfo
	{
		public CraftItemRemovalInfo[] RemovalInfo;

		public ItemContainer DestinationContainer;

		public Recipe Recipe;

		public PhotonPlayer PhotonPlayer;

		public int GamePlayerViewId;
	}

	public struct CraftItemRemovalInfo
	{
		public ItemContainer ItemContainer;

		public int[] IndicesToRemove;

		public int NumIndiciesToRemove;
	}

	public static CraftEvent OnItemCrafted;

	private const int EVERY_PRISON = -1;

	[SerializeField]
	private List<Recipe> m_Recipes;

	private List<Recipe> m_LiveRecipes;

	private int m_TotalCrafts;

	private int[] m_UniqueList = new int[8];

	private static CraftManager m_TheInstance;

	public const int DONT_HAVE_ID = -100;

	public static CraftManager GetInstance()
	{
		return m_TheInstance;
	}

	private void Awake()
	{
		if (m_TheInstance == null)
		{
			m_TheInstance = this;
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_TheInstance == this)
		{
			m_TheInstance = null;
		}
	}

	private void Start()
	{
	}

	public List<Recipe> GetCurrentRecipes()
	{
		return m_LiveRecipes;
	}

	public Recipe GetRandomAllowedRecipe()
	{
		return m_LiveRecipes[UnityEngine.Random.Range(0, m_LiveRecipes.Count)];
	}

	public Recipe GetRecipeForProduct(ItemData finalItem)
	{
		if (finalItem == null)
		{
			return null;
		}
		if (m_LiveRecipes == null)
		{
			return null;
		}
		for (int i = 0; i < m_LiveRecipes.Count; i++)
		{
			if (m_LiveRecipes[i] == null)
			{
				T17NetManager.LogGoogleException("Recipe #" + i + " is null!");
			}
			else if (m_LiveRecipes[i].m_Product == null)
			{
				T17NetManager.LogGoogleException("No final product for recipe #" + i + "!");
			}
			else if (m_LiveRecipes[i].m_Product.m_ItemDataID == finalItem.m_ItemDataID)
			{
				return m_LiveRecipes[i];
			}
		}
		return null;
	}

	public Recipe GetRecipeForProduct(int finalItemID)
	{
		if (m_LiveRecipes == null)
		{
			return null;
		}
		if (finalItemID != -1)
		{
			for (int i = 0; i < m_LiveRecipes.Count; i++)
			{
				if (m_LiveRecipes[i] == null)
				{
					T17NetManager.LogGoogleException("Recipe #" + i + " is null!");
				}
				else if (m_LiveRecipes[i].m_Product == null)
				{
					T17NetManager.LogGoogleException("No final product for recipe #" + i + "!");
				}
				else if (m_LiveRecipes[i].m_Product.m_ItemDataID == finalItemID)
				{
					return m_LiveRecipes[i];
				}
			}
		}
		return null;
	}

	public List<Recipe> GetAllRecipes()
	{
		return m_Recipes;
	}

	public void CreateLiveList()
	{
		int count = m_Recipes.Count;
		m_LiveRecipes = new List<Recipe>();
		for (int i = 0; i < count; i++)
		{
			if (LevelScript.IsPrisonEnumInMask(m_Recipes[i].m_PrisonMask, LevelScript.GetInstance().m_LevelSetup.m_LevelInfo.m_PrisonEnum))
			{
				bool flag = true;
				for (int j = 0; j < m_Recipes[i].m_Ingredients.Length; j++)
				{
					if (m_Recipes[i].m_Ingredients[j] != null && !ItemManager.GetInstance().IsItemInAllowedList(m_Recipes[i].m_Ingredients[j]))
					{
						flag = false;
					}
				}
				if (flag)
				{
					m_LiveRecipes.Add(m_Recipes[i]);
				}
			}
			else if (!(m_Recipes[i].m_Product != null))
			{
			}
		}
	}

	public void DestroyLiveList()
	{
		m_LiveRecipes = null;
	}

	public void AddNewRecipe()
	{
		m_Recipes.Add(new Recipe());
	}

	public void RemoveRecipe(int index)
	{
		if (index >= 0 && index < m_Recipes.Count)
		{
			m_Recipes.RemoveAt(index);
		}
	}

	public int GetIntellectRequiredForRecipe(int recipeID)
	{
		if (recipeID >= 0 && recipeID < m_LiveRecipes.Count)
		{
			return m_LiveRecipes[recipeID].m_Intellect;
		}
		return 0;
	}

	public bool HaveRecipeWithItem(ItemData data)
	{
		for (int i = 0; i < m_Recipes.Count; i++)
		{
			for (int j = 0; j < m_Recipes[i].m_Ingredients.Length; j++)
			{
				if (m_Recipes[i].m_Ingredients[j] != null && m_Recipes[i].m_Ingredients[j].m_ItemDataID == data.m_ItemDataID)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasItemsForRecipe(int recipeIndex, ItemContainer container, ref int[] usingItemIndices, bool failFast = true)
	{
		if (recipeIndex < 0 || recipeIndex >= m_LiveRecipes.Count)
		{
			return false;
		}
		Recipe recipeToCheck = m_LiveRecipes[recipeIndex];
		return HasItemsForRecipe(recipeToCheck, container, ref usingItemIndices, failFast);
	}

	private bool HasItemsForRecipe(Recipe recipeToCheck, ItemContainer container, ref int[] usingItemIndices, bool failFast = true)
	{
		int numIndiciesSet;
		if (recipeToCheck.m_Product != null)
		{
			return HasItemsForRecipe(recipeToCheck.m_Ingredients, recipeToCheck.m_IngredientsToBeDestroyed, container, ref usingItemIndices, out numIndiciesSet, failFast);
		}
		return false;
	}

	private bool HasItemsForRecipe(ItemData[] recipeItems, bool[] itemsToBeDestroyed, ItemContainer container, ref int[] usingItemIndices, out int numIndiciesSet, bool failFast = true, bool nullOffRecipeItems = false)
	{
		Array.Clear(m_UniqueList, 0, m_UniqueList.Length);
		int num = 0;
		int num2 = 0;
		numIndiciesSet = 0;
		bool flag = true;
		for (int i = 0; i < recipeItems.Length; i++)
		{
			if (recipeItems[i] == null)
			{
				continue;
			}
			bool flag2 = false;
			ItemData itemData = recipeItems[i];
			Character characterOwner = container.GetCharacterOwner();
			if (characterOwner != null && characterOwner.m_CharacterStats.m_bIsPlayer && characterOwner.GetEquippedItem() != null && itemData.m_ItemDataID == characterOwner.GetEquippedItem().m_ItemData.m_ItemDataID && !characterOwner.GetEquippedItem().IsInUse())
			{
				bool flag3 = false;
				for (int j = 0; j < num; j++)
				{
					if (m_UniqueList[j] == -1)
					{
						flag3 = true;
						break;
					}
				}
				if (!flag3)
				{
					flag2 = true;
					m_UniqueList[num] = -1;
					if (itemsToBeDestroyed[i])
					{
						usingItemIndices[num2] = -1;
						num2++;
						numIndiciesSet++;
					}
					num++;
					if (nullOffRecipeItems)
					{
						recipeItems[i] = null;
					}
					continue;
				}
			}
			for (int k = 0; k < container.GetItemCount(); k++)
			{
				bool flag4 = false;
				for (int j = 0; j < num; j++)
				{
					if (m_UniqueList[j] == k)
					{
						flag4 = true;
						break;
					}
				}
				if (!flag4 && itemData.m_ItemDataID == container.GetItem(k).ItemDataID)
				{
					flag2 = true;
					m_UniqueList[num] = k;
					if (itemsToBeDestroyed[i])
					{
						usingItemIndices[num2] = k;
						num2++;
						numIndiciesSet++;
					}
					num++;
					if (nullOffRecipeItems)
					{
						recipeItems[i] = null;
					}
					break;
				}
			}
			if (flag2 || !failFast)
			{
				continue;
			}
			flag = false;
			num = 0;
			break;
		}
		if (flag)
		{
			return true;
		}
		return false;
	}

	public bool HasItemsForRecipe(int recipeIndex, ItemData[] items, ref int[] usingItemIndices, bool failFast = true)
	{
		if (recipeIndex < 0 || recipeIndex >= m_LiveRecipes.Count)
		{
			return false;
		}
		Array.Clear(m_UniqueList, 0, m_UniqueList.Length);
		int num = 0;
		int num2 = 0;
		Debug.Log("  ****** FindRecipe FindRecipe FindRecipe");
		Recipe recipe = m_LiveRecipes[recipeIndex];
		if (recipe.m_Product != null)
		{
			bool flag = true;
			for (int i = 0; i < recipe.m_Ingredients.Length; i++)
			{
				if (recipe.m_Ingredients[i] == null)
				{
					continue;
				}
				bool flag2 = false;
				ItemData itemData = recipe.m_Ingredients[i];
				for (int j = 0; j < items.Length; j++)
				{
					bool flag3 = false;
					for (int k = 0; k < num; k++)
					{
						if (m_UniqueList[k] == j)
						{
							flag3 = true;
							break;
						}
					}
					if (!flag3 && items[j] != null && itemData.m_ItemDataID == items[j].m_ItemDataID)
					{
						flag2 = true;
						m_UniqueList[num] = j;
						if (recipe.m_IngredientsToBeDestroyed[i])
						{
							usingItemIndices[num2] = j;
							num2++;
						}
						num++;
						break;
					}
				}
				if (!flag2 && failFast)
				{
					flag = false;
					num = 0;
					break;
				}
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	public bool GetCraftingIndiciesForRecipe(Recipe recipe, ref List<CraftItemRemovalInfo> removalInfos, List<ItemContainer> sourceContainers)
	{
		removalInfos.Clear();
		if (recipe.m_Product == null)
		{
			return false;
		}
		ItemData[] array = new ItemData[recipe.m_Ingredients.Length];
		recipe.m_Ingredients.CopyTo(array, 0);
		int[] usingItemIndices = new int[8];
		int count = sourceContainers.Count;
		for (int i = 0; i < count; i++)
		{
			ItemContainer itemContainer = sourceContainers[i];
			if (!(itemContainer == null))
			{
				int numIndiciesSet = 0;
				HasItemsForRecipe(array, recipe.m_IngredientsToBeDestroyed, itemContainer, ref usingItemIndices, out numIndiciesSet, failFast: false, nullOffRecipeItems: true);
				CraftItemRemovalInfo item = default(CraftItemRemovalInfo);
				item.ItemContainer = itemContainer;
				item.IndicesToRemove = new int[8];
				usingItemIndices.CopyTo(item.IndicesToRemove, 0);
				item.NumIndiciesToRemove = numIndiciesSet;
				removalInfos.Add(item);
			}
		}
		bool result = true;
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j] != null)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	public bool ReplaceWithPreferredItems(ItemContainer container, int count, ref int[] indices, int[] preferred)
	{
		if (indices == null || indices.Length <= 0)
		{
			return false;
		}
		if (preferred == null || preferred.Length <= 0)
		{
			return false;
		}
		if (container == null)
		{
			return false;
		}
		Item item = null;
		Character characterOwner = container.GetCharacterOwner();
		if (characterOwner != null)
		{
			item = characterOwner.GetEquippedItem();
		}
		foreach (int num in preferred)
		{
			if (Array.IndexOf(indices, num) >= 0)
			{
				continue;
			}
			Item item2 = ((num < 0) ? item : container.GetItem(num));
			if (item2 == null)
			{
				continue;
			}
			for (int j = 0; j < count; j++)
			{
				int num2 = indices[j];
				if (Array.IndexOf(preferred, num2) < 0)
				{
					Item item3 = ((num2 < 0) ? item : container.GetItem(num2));
					if (item3 != null && item3.ItemDataID == item2.ItemDataID)
					{
						indices[j] = num;
						break;
					}
				}
			}
		}
		return true;
	}

	public bool HasItemsForAnyRecipe(ItemContainer container)
	{
		for (int i = 0; i < container.GetItemCount(); i++)
		{
			ItemData itemData = container.GetItem(i).m_ItemData;
			for (int j = i + 1; j < container.GetItemCount(); j++)
			{
				ItemData itemData2 = container.GetItem(j).m_ItemData;
				if (GetRecipeIDFromItems(itemData, itemData2, null) != -1)
				{
					return true;
				}
				for (int k = j + 1; k < container.GetItemCount(); k++)
				{
					ItemData itemData3 = container.GetItem(k).m_ItemData;
					if (GetRecipeIDFromItems(itemData, itemData2, itemData3) != -1)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public void CheckItemsAvailable(int recipeIndex, ItemContainer container, ref int[] usingItemIndices)
	{
		if (recipeIndex < 0 || recipeIndex >= m_LiveRecipes.Count)
		{
			return;
		}
		int num = 0;
		Recipe recipe = m_LiveRecipes[recipeIndex];
		if (!(recipe.m_Product != null))
		{
			return;
		}
		for (int i = 0; i < recipe.m_Ingredients.Length; i++)
		{
			if (recipe.m_Ingredients[i] == null)
			{
				continue;
			}
			usingItemIndices[i] = -100;
			ItemData itemData = recipe.m_Ingredients[i];
			Character characterOwner = container.GetCharacterOwner();
			if (characterOwner != null && characterOwner.m_CharacterStats.m_bIsPlayer && characterOwner.GetEquippedItem() != null && itemData.m_ItemDataID == characterOwner.GetEquippedItem().m_ItemData.m_ItemDataID)
			{
				bool flag = false;
				for (int j = 0; j < num; j++)
				{
					if (usingItemIndices[j] == -1)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					usingItemIndices[i] = -1;
					num++;
					continue;
				}
			}
			for (int k = 0; k < container.GetItemCount(); k++)
			{
				bool flag2 = false;
				for (int j = 0; j < num; j++)
				{
					if (usingItemIndices[j] == k)
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2 && itemData.m_ItemDataID == container.GetItem(k).ItemDataID)
				{
					usingItemIndices[i] = k;
					num++;
					break;
				}
			}
		}
	}

	public void HaveCraftedRecipe(Recipe recipe)
	{
		recipe.m_CraftedCount++;
		string key = "Craft:R:" + CreateRecipeSaveString(recipe);
		GlobalSave.GetInstance().Set(key, recipe.m_CraftedCount);
		m_TotalCrafts++;
		GlobalSave.GetInstance().Set("Craft:Total", m_TotalCrafts);
		GlobalSave.GetInstance().RequestSave();
	}

	public void DiscoverHiddenItem(Recipe recipe)
	{
		recipe.m_bDiscovered = true;
		string key = "Craft:Discovered:" + CreateRecipeSaveString(recipe);
		GlobalSave.GetInstance().Set(key, recipe.m_bDiscovered);
		GlobalSave.GetInstance().RequestSave();
	}

	public void LoadData()
	{
		if (m_Recipes == null)
		{
			return;
		}
		int count = m_Recipes.Count;
		GlobalSave.GetInstance().Get("Craft:Total", out m_TotalCrafts, 0);
		for (int i = 0; i < count; i++)
		{
			if (m_Recipes[i].m_Product != null)
			{
				string key = "Craft:R:" + m_Recipes[i].m_Product.name;
				GlobalSave.GetInstance().Get(key, out m_Recipes[i].m_CraftedCount, 0);
				string key2 = "Craft:Discovered:" + CreateRecipeSaveString(m_Recipes[i]);
				GlobalSave.GetInstance().Get(key2, out m_Recipes[i].m_bDiscovered, def: false);
			}
		}
	}

	private string CreateRecipeSaveString(Recipe recipe)
	{
		string text = recipe.m_Product.name;
		for (int i = 0; i < recipe.GetIngredientCount(); i++)
		{
			text += recipe.m_Ingredients[i].name[0];
		}
		return text;
	}

	public void SortAlphabetically()
	{
		m_Recipes.Sort(delegate(Recipe p1, Recipe p2)
		{
			if (p1.m_Product == null)
			{
				return 1;
			}
			return (p2.m_Product == null) ? (-1) : string.Compare(p1.m_Product.m_ItemLocalizationTag, p2.m_Product.m_ItemLocalizationTag);
		});
	}

	public int GetRecipeIDFromItems(ItemData ingredient1, ItemData ingredient2, ItemData ingredient3)
	{
		for (int i = 0; i < m_LiveRecipes.Count; i++)
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			for (int j = 0; j < m_LiveRecipes[i].m_Ingredients.Length; j++)
			{
				ItemData itemData = m_LiveRecipes[i].m_Ingredients[j];
				if (ingredient1 != null)
				{
					if (itemData != null && !flag && ingredient1.m_ItemDataID == itemData.m_ItemDataID)
					{
						flag = true;
						continue;
					}
				}
				else if (!flag && itemData == null)
				{
					flag = true;
					continue;
				}
				if (ingredient2 != null)
				{
					if (itemData != null && !flag2 && ingredient2.m_ItemDataID == itemData.m_ItemDataID)
					{
						flag2 = true;
						continue;
					}
				}
				else if (!flag2 && itemData == null)
				{
					flag2 = true;
					continue;
				}
				if (ingredient3 != null)
				{
					if (itemData != null && !flag3 && ingredient3.m_ItemDataID == itemData.m_ItemDataID)
					{
						flag3 = true;
					}
				}
				else if (!flag3 && itemData == null)
				{
					flag3 = true;
				}
			}
			if (flag && flag2 && flag3)
			{
				return i;
			}
		}
		return -1;
	}

	public void GetRecipiesThatUseIngredient(ItemData itemData, List<Recipe> outRecipies)
	{
		outRecipies.Clear();
		if (itemData == null)
		{
			return;
		}
		for (int i = 0; i < m_LiveRecipes.Count; i++)
		{
			bool flag = false;
			for (int j = 0; j < m_LiveRecipes[i].m_Ingredients.Length; j++)
			{
				ItemData itemData2 = m_LiveRecipes[i].m_Ingredients[j];
				if (itemData2 != null && itemData2.m_ItemDataID == itemData.m_ItemDataID)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				outRecipies.Add(m_LiveRecipes[i]);
			}
		}
	}

	public int GetRecipieIDForItem(ItemData itemData)
	{
		if (itemData != null)
		{
			for (int i = 0; i < m_LiveRecipes.Count; i++)
			{
				if (m_LiveRecipes[i] != null && m_LiveRecipes[i].m_Product.m_ItemDataID == itemData.m_ItemDataID)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public Recipe GetRecipeByID(int id)
	{
		if (id >= 0 && id < m_LiveRecipes.Count)
		{
			return m_LiveRecipes[id];
		}
		return null;
	}

	public int GetRecipeId(Recipe recipe)
	{
		for (int i = 0; i < m_LiveRecipes.Count; i++)
		{
			if (m_LiveRecipes[i] == recipe)
			{
				return i;
			}
		}
		return -1;
	}
}
