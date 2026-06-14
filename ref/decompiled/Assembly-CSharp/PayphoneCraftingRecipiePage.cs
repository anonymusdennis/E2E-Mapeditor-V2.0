using UnityEngine;

public class PayphoneCraftingRecipiePage : MonoBehaviour
{
	public GameObject m_2PartRecipiePage;

	public T17Image m_2PartImage1;

	public T17Image m_2PartImage2;

	public GameObject m_3PartRecipiePage;

	public T17Image m_3PartImage1;

	public T17Image m_3PartImage2;

	public T17Image m_3PartImage3;

	public T17Text m_RecipieName;

	public T17Button m_BackButton;

	public void DisplayCraftRecipie(ItemData recipieResult)
	{
		CraftManager instance = CraftManager.GetInstance();
		if (!(instance != null) || !(recipieResult != null))
		{
			return;
		}
		CraftManager.Recipe recipeForProduct = instance.GetRecipeForProduct(recipieResult);
		if (recipeForProduct == null)
		{
			return;
		}
		if (recipeForProduct.m_Ingredients.Length > 2 && recipeForProduct.m_Ingredients[2] != null)
		{
			if (m_2PartRecipiePage != null && m_3PartRecipiePage != null)
			{
				m_2PartRecipiePage.SetActive(value: false);
				m_3PartRecipiePage.SetActive(value: true);
				if (m_3PartImage1 != null && recipeForProduct.m_Ingredients[0] != null)
				{
					m_3PartImage1.sprite = recipeForProduct.m_Ingredients[0].m_ItemUIImage;
				}
				if (m_3PartImage2 != null && recipeForProduct.m_Ingredients[1] != null)
				{
					m_3PartImage2.sprite = recipeForProduct.m_Ingredients[1].m_ItemUIImage;
				}
				if (m_3PartImage3 != null && recipeForProduct.m_Ingredients[2] != null)
				{
					m_3PartImage3.sprite = recipeForProduct.m_Ingredients[2].m_ItemUIImage;
				}
				if (m_RecipieName != null)
				{
					m_RecipieName.SetNewLocalizationTag(recipieResult.m_ItemLocalizationTag);
				}
			}
		}
		else if ((recipeForProduct.m_Ingredients.Length <= 2 || recipeForProduct.m_Ingredients[2] == null) && m_2PartRecipiePage != null && m_3PartRecipiePage != null)
		{
			m_2PartRecipiePage.SetActive(value: true);
			m_3PartRecipiePage.SetActive(value: false);
			if (m_2PartImage1 != null && recipeForProduct.m_Ingredients[0] != null)
			{
				m_2PartImage1.sprite = recipeForProduct.m_Ingredients[0].m_ItemUIImage;
			}
			if (m_2PartImage2 != null && recipeForProduct.m_Ingredients[1] != null)
			{
				m_2PartImage2.sprite = recipeForProduct.m_Ingredients[1].m_ItemUIImage;
			}
			if (m_RecipieName != null)
			{
				m_RecipieName.SetNewLocalizationTag(recipieResult.m_ItemLocalizationTag);
			}
		}
	}
}
