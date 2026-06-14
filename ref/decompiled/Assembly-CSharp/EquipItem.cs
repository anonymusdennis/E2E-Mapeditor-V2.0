using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Action")]
public class EquipItem : ActionTask<AICharacter>
{
	[BlackboardOnly]
	public BBParameter<int> m_ItemDataID;

	protected override void OnExecute()
	{
		Item equippedItem = base.agent.m_Character.GetEquippedItem();
		if (equippedItem == null || equippedItem.ItemDataID != m_ItemDataID.value)
		{
			Item firstItemWithItemID = base.agent.m_Character.m_ItemContainer.GetFirstItemWithItemID(m_ItemDataID.value);
			if (firstItemWithItemID != null)
			{
				base.agent.m_Character.SetEquippedItem(firstItemWithItemID);
			}
		}
		EndAction(true);
	}
}
