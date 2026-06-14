using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Items")]
[Description("Check if an ItemID is tranferable using any of the TransferItemsInteractables specified")]
public class CheckItemTransferable : ConditionTask<AICharacter>
{
	public BBParameter<List<InteractiveObject>> m_ItemTransferers;

	public BBParameter<InteractiveObject> m_ItemTransferer;

	private List<InteractiveObject> m_ItemTransferers_IntObjCache;

	private List<TransferItemsInteraction> m_ItemTransferers_Cache;

	public BBParameter<List<int>> m_ItemIDs;

	public BBParameter<int> m_ItemID;

	protected override string info => "Check Item Is Transferable" + '\n' + m_ItemIDs;

	protected override bool OnCheck()
	{
		if (m_ItemTransferers == null || m_ItemTransferers.value == null)
		{
			return false;
		}
		if (m_ItemTransferers_IntObjCache != m_ItemTransferers.value)
		{
			m_ItemTransferers_IntObjCache = m_ItemTransferers.value;
			m_ItemTransferers_Cache = null;
		}
		if (m_ItemTransferers_IntObjCache == null)
		{
			return false;
		}
		if (m_ItemTransferers_Cache == null)
		{
			m_ItemTransferers_Cache = new List<TransferItemsInteraction>();
			for (int i = 0; i < m_ItemTransferers_IntObjCache.Count; i++)
			{
				TransferItemsInteraction component = m_ItemTransferers_IntObjCache[i].GetComponent<TransferItemsInteraction>();
				if (component != null)
				{
					m_ItemTransferers_Cache.Add(component);
				}
			}
		}
		for (int j = 0; j < m_ItemTransferers_Cache.Count; j++)
		{
			TransferItemsInteraction transferItemsInteraction = m_ItemTransferers_Cache[j];
			for (int k = 0; k < m_ItemIDs.value.Count; k++)
			{
				int num = m_ItemIDs.value[k];
				Item item = base.agent.m_Character.m_ItemContainer.GetFirstItemWithItemID(num);
				if (item == null)
				{
					item = base.agent.m_Character.GetEquippedItem();
					if (item == null || item.ItemDataID != num)
					{
						continue;
					}
				}
				if (transferItemsInteraction != null && transferItemsInteraction.IsItemTransferable(item))
				{
					m_ItemTransferer.value = transferItemsInteraction;
					m_ItemID.value = item.ItemDataID;
					return true;
				}
			}
		}
		return false;
	}
}
