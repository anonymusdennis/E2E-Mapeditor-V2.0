using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Item")]
public class CraftItem : ActionTask<AICharacter>
{
	[BlackboardOnly]
	public BBParameter<int> m_ItemToCraft;

	private int m_ProductID;

	private int m_RecipeID = -1;

	private CraftManager.Recipe m_CraftRecipe;

	private int[] m_DestroyItemIndices = new int[8];

	private List<int> m_CraftResponsesToBeHandled = new List<int>();

	protected override string info => "Craft Item " + '\n' + "$" + m_ItemToCraft.name;

	protected override void OnExecute()
	{
		if (m_ItemToCraft == null)
		{
			EndAction(true);
		}
		else if (!Setup(m_ItemToCraft.value))
		{
			EndAction(false);
		}
		else if (CraftManager.GetInstance().HasItemsForRecipe(m_RecipeID, base.agent.m_ItemContainer, ref m_DestroyItemIndices))
		{
			m_CraftResponsesToBeHandled.Add(ItemManager.GetInstance().GetNextRequestID());
			int requestID = -1;
			ItemManager.GetInstance().AssignItemRPC(0, m_ItemToCraft.value, OnItemMgrResponse, ref requestID);
		}
		else
		{
			EndAction(false);
		}
	}

	private void OnItemMgrResponse(Item item, int eventID)
	{
		if (!m_CraftResponsesToBeHandled.Contains(eventID))
		{
			return;
		}
		m_CraftResponsesToBeHandled.Remove(eventID);
		if (item == null)
		{
			EndAction(false);
			return;
		}
		base.agent.m_ItemContainer.RemoveItems(m_CraftRecipe.GetDestroyableIngredientCount(), ref m_DestroyItemIndices, releaseToManager: true);
		if (!base.agent.m_ItemContainer.AddItemRPC(item))
		{
			ItemManager.GetInstance().RequestReleaseItem(item);
		}
		EndAction(true);
	}

	protected override void OnStop()
	{
		if (m_CraftResponsesToBeHandled == null || m_CraftResponsesToBeHandled.Count > 0)
		{
		}
		base.OnStop();
	}

	private bool Setup(int productID)
	{
		if (m_RecipeID == -1 || productID != m_ProductID)
		{
			m_ProductID = productID;
			m_CraftRecipe = CraftManager.GetInstance().GetRecipeForProduct(productID);
			if (m_CraftRecipe == null)
			{
				return false;
			}
			m_RecipeID = CraftManager.GetInstance().GetRecipeIDFromItems(m_CraftRecipe.m_Ingredients[0], m_CraftRecipe.m_Ingredients[1], m_CraftRecipe.m_Ingredients[2]);
			if (m_RecipeID == -1)
			{
				return false;
			}
		}
		return true;
	}
}
