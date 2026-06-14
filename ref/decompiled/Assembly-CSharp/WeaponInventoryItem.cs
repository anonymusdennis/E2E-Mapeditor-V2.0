public class WeaponInventoryItem : InventoryItem
{
	public T17Text m_DamageLabel;

	public T17Text m_SpeedLabel;

	public T17Text m_RangeLabel;

	public override void SetItem(Item item)
	{
		base.SetItem(item);
		if (item == null)
		{
			if (m_DamageLabel != null)
			{
				m_DamageLabel.SetLocalisedTextCatchAll("Text.EquipmentStat.N/A");
			}
			if (m_SpeedLabel != null)
			{
				m_SpeedLabel.SetLocalisedTextCatchAll("Text.EquipmentStat.N/A");
			}
			if (m_RangeLabel != null)
			{
				m_RangeLabel.SetLocalisedTextCatchAll("Text.EquipmentStat.N/A");
			}
		}
		else if (item.m_ItemData.IsWeapon())
		{
			if (m_DamageLabel != null)
			{
				m_DamageLabel.SetLocalisedTextCatchAll("Text.EquipmentStat.Damage." + item.m_ItemData.m_CombatData.m_CombatConfig.m_DamageSummary);
			}
			if (m_SpeedLabel != null)
			{
				m_SpeedLabel.SetLocalisedTextCatchAll("Text.EquipmentStat.AttackSpeed." + item.m_ItemData.m_CombatData.GetRecoveryTimeSummary());
			}
			if (m_RangeLabel != null)
			{
				m_RangeLabel.SetLocalisedTextCatchAll("Text.EquipmentStat.Range." + item.m_ItemData.m_CombatData.GetRangeSummary());
			}
		}
		else
		{
			if (m_DamageLabel != null)
			{
				m_DamageLabel.SetLocalisedTextCatchAll("Text.EquipmentStat.N/A");
			}
			if (m_SpeedLabel != null)
			{
				m_SpeedLabel.SetLocalisedTextCatchAll("Text.EquipmentStat.N/A");
			}
			if (m_RangeLabel != null)
			{
				m_RangeLabel.SetLocalisedTextCatchAll("Text.EquipmentStat.N/A");
			}
		}
	}
}
