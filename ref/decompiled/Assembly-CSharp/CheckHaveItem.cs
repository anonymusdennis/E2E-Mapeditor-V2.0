using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Items")]
[Description("Check if an AI Event is received and return true for one frame if it matches the required event types")]
public class CheckHaveItem : ConditionTask<AICharacter>
{
	[BlackboardOnly]
	public BBParameter<List<int>> m_Items;

	[BlackboardOnly]
	public BBParameter<int> m_Item = -1;

	public bool m_bRequireAll;

	public bool m_bCheckEquipped;

	protected override string info
	{
		get
		{
			string text = string.Empty;
			if (m_Items.value != null)
			{
				List<int> value = m_Items.value;
				for (int i = 0; i < value.Count; i++)
				{
					text = text + value[i] + ", ";
				}
			}
			else
			{
				text = text + "$" + m_Items.name;
			}
			return "Check Have Item " + '\n' + text;
		}
	}

	protected override bool OnCheck()
	{
		bool result = false;
		List<int> list = m_Items.value;
		if (list == null)
		{
			if (m_Item.value == -1)
			{
				return false;
			}
			list = new List<int>();
			list.Add(m_Item.value);
		}
		for (int i = 0; i < list.Count; i++)
		{
			int num = base.agent.m_ItemContainer.HasItem(list[i]);
			if (num == 0 && m_bCheckEquipped)
			{
				Item equippedItem = base.agent.m_Character.GetEquippedItem();
				if (equippedItem != null && equippedItem.ItemDataID == list[i])
				{
					num++;
				}
			}
			if (m_bRequireAll)
			{
				result = true;
				if (num == 0)
				{
					result = false;
					break;
				}
			}
			else if (num > 0)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
	}
}
