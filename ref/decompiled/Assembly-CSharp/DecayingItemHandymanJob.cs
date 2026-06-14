public class DecayingItemHandymanJob : HandymanJob
{
	[Tooltip("The item the required items get replaced with, after their health has been decayed")]
	public ItemData m_RequiredItemsUsedReplacement;

	public int m_ItemHealthDecayPerUse = 50;

	private Character m_LastFixer;

	protected override void Awake()
	{
		base.Awake();
		if (!(m_RequiredItemsUsedReplacement == null))
		{
		}
	}

	protected override void All_OnInteractionFixed(HandymanInteraction fixedInteraction, Character interactingCharacter)
	{
		base.All_OnInteractionFixed(fixedInteraction, interactingCharacter);
		if (interactingCharacter == null || !T17NetManager.IsMasterClient)
		{
			return;
		}
		m_LastFixer = interactingCharacter;
		bool flag = false;
		for (int num = m_MinigameRequirements.Count - 1; num >= 0; num--)
		{
			ItemData itemData = m_MinigameRequirements[num];
			Item item = interactingCharacter.m_ItemContainer.GetFirstItemWithItemID(itemData.m_ItemDataID);
			if (item == null)
			{
				Item equippedItem = interactingCharacter.GetEquippedItem();
				Item outFit = interactingCharacter.GetOutFit();
				if (equippedItem != null && equippedItem.ItemDataID == itemData.m_ItemDataID)
				{
					item = equippedItem;
				}
				else if (outFit != null && outFit.ItemDataID == itemData.m_ItemDataID)
				{
					item = outFit;
				}
			}
			if (item != null)
			{
				int num2 = item.Health - m_ItemHealthDecayPerUse;
				item.DecreaseHealth(m_ItemHealthDecayPerUse);
				if (num2 <= 0)
				{
					flag = true;
				}
			}
		}
		if (flag && m_RequiredItemsUsedReplacement != null)
		{
			ItemManager.GetInstance().AssignItemRPC(0, m_RequiredItemsUsedReplacement.m_ItemDataID, OnItemMgrResponseAddToCharacter, ref m_ImmediateItemMgrResponseID);
		}
	}

	private void OnItemMgrResponseAddToCharacter(Item item, int eventID)
	{
		if (!(item != null) || eventID != m_ImmediateItemMgrResponseID)
		{
			return;
		}
		bool flag = false;
		if (m_LastFixer != null)
		{
			if (m_LastFixer.m_ItemContainer != null && m_LastFixer.m_ItemContainer.GetFreeSpaceCount() > 0)
			{
				if (m_LastFixer.m_ItemContainer.AddItemRPC(item))
				{
					flag = true;
				}
			}
			else if (m_LastFixer.GetEquippedItem() == null && m_LastFixer.CanEquipItem(item))
			{
				m_LastFixer.SetEquippedItem(item);
				flag = true;
			}
		}
		if (!flag)
		{
			ItemManager.GetInstance().RequestReleaseItem(item, RPC_CallContexts.Master);
		}
	}
}
