using UnityEngine;

public class CraftingTooltip : BaseTooltip
{
	public T17Text m_CraftType;

	public T17Text m_CraftName;

	public InventoryItem m_RecipeItem1;

	public InventoryItem m_RecipeItem2;

	public InventoryItem m_RecipeItem3;

	public RectTransform m_Rect;

	public float m_XOffset;

	public float m_YOffset;

	public T17Slider m_HoldToCraftBar;

	public void SetPositionWithOffset(Vector3 position)
	{
		base.transform.position = position;
		Vector2 anchoredPosition = m_Rect.anchoredPosition;
		anchoredPosition.x += m_XOffset;
		anchoredPosition.y += m_YOffset;
		m_Rect.anchoredPosition = anchoredPosition;
		m_Rect.anchoredPosition = anchoredPosition;
	}
}
