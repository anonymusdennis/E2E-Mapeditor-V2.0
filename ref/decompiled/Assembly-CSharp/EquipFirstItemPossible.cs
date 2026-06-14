using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Action")]
public class EquipFirstItemPossible : ActionTask<AICharacter>
{
	[BlackboardOnly]
	public BBParameter<List<int>> m_Items;

	protected override string info
	{
		get
		{
			if (m_Items != null)
			{
				return "Equip first item out of collection:\n" + m_Items.name;
			}
			return "Equip first item out of a \nUNDEFINED collection";
		}
	}

	protected override void OnExecute()
	{
		if (m_Items.value == null || m_Items.value.Count == 0)
		{
			EndAction(false);
			return;
		}
		Item equippedItem = base.agent.m_Character.GetEquippedItem();
		int i = 0;
		for (int count = m_Items.value.Count; i < count; i++)
		{
			int num = m_Items.value[i];
			if (equippedItem != null && equippedItem.ItemDataID == num)
			{
				EndAction(true);
				return;
			}
			Item firstItemWithItemID = base.agent.m_Character.m_ItemContainer.GetFirstItemWithItemID(num);
			if (firstItemWithItemID != null)
			{
				base.agent.m_Character.SetEquippedItem(firstItemWithItemID);
				EndAction(true);
				return;
			}
		}
		EndAction(false);
	}
}
