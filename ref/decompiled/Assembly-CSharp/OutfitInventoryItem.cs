public class OutfitInventoryItem : InventoryItem
{
	public T17Text m_TypeLabel;

	public T17Text m_DefenseLabel;

	public override void SetItem(Item item)
	{
		base.SetItem(item);
		if (item == null)
		{
			if (m_TypeLabel != null)
			{
				m_TypeLabel.SetLocalisedTextCatchAll("Text.EquipmentStat.N/A");
			}
			if (m_DefenseLabel != null)
			{
				m_DefenseLabel.SetLocalisedTextCatchAll("Text.EquipmentStat.N/A");
			}
		}
		else
		{
			if (!item.m_ItemData.IsOutfit())
			{
				return;
			}
			if (m_TypeLabel != null)
			{
				m_TypeLabel.SetLocalisedTextCatchAll("Text.EquipmentStat.OutfitType." + item.m_ItemData.m_OutfitData.m_Type);
			}
			if (m_DefenseLabel != null)
			{
				if (item.m_ItemData == null || item.m_ItemData.m_OutfitData == null || item.m_ItemData.m_OutfitData.m_ArmourConfig == null)
				{
					m_DefenseLabel.SetLocalisedTextCatchAll("Text.EquipmentStat.N/A");
				}
				else
				{
					m_DefenseLabel.SetLocalisedTextCatchAll("Text.EquipmentStat.DamageReduction." + item.m_ItemData.m_OutfitData.m_ArmourConfig.m_ReductionSummary);
				}
			}
		}
	}
}
