public class ImmediateItemProcessor : ItemProcessorBase
{
	private Character m_RecepiantOfConvertedItem;

	public void ConvertItemForCharacter(Item inputItem, Character localCharacter)
	{
		if (inputItem == null)
		{
		}
		ConvertItemForCharacter(inputItem.ItemDataID, localCharacter);
	}

	public void ConvertItemForCharacter(int inputItemDataId, Character localCharacter)
	{
		m_RecepiantOfConvertedItem = localCharacter;
		ItemData outputItem = GetOutputItem(inputItemDataId);
		if (outputItem != null)
		{
			RequestItemCreation(0, outputItem.m_ItemDataID);
		}
	}

	protected override void OnItemManagerCreatedItemForUs(Item item, int eventId)
	{
		bool flag = false;
		if (m_RecepiantOfConvertedItem != null)
		{
			flag = m_RecepiantOfConvertedItem.m_ItemContainer.AddItemRPC(item);
			if (!flag && m_RecepiantOfConvertedItem.GetEquippedItem() == null)
			{
				flag = m_RecepiantOfConvertedItem.SetEquippedItem(item);
			}
			m_RecepiantOfConvertedItem = null;
		}
		if (!flag)
		{
			ItemManager.GetInstance().RequestReleaseItem(item);
		}
	}

	public override bool IsFinishedCreatingItem()
	{
		return false;
	}

	public override bool IsIdle()
	{
		return m_RecepiantOfConvertedItem == null;
	}
}
