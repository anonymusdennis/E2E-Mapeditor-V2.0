using UnityEngine;

public class JobBoard : MonoBehaviour
{
	public ItemData[] m_AllJobKeys;

	private void Start()
	{
	}

	public void SwapJob(Player player)
	{
		if ((!T17NetManager.OfflineMode && (!T17NetManager.NetOnlineMode || !T17NetManager.IsMasterClient)) || player.m_ItemContainer.GetHiddenItemCount() <= 0)
		{
			return;
		}
		Item item = null;
		for (int i = 0; i < player.m_ItemContainer.GetHiddenItemCount(); i++)
		{
			Item hiddenItem = player.m_ItemContainer.GetHiddenItem(i);
			if (hiddenItem != null)
			{
				BaseItemFunctionality baseItemFunctionality = hiddenItem.HasFunctionality(BaseItemFunctionality.Functionality.Key);
				if (baseItemFunctionality != null && ((KeyFunctionality)baseItemFunctionality).IsHidden)
				{
					item = hiddenItem;
					break;
				}
			}
		}
		if (!(item != null))
		{
			return;
		}
		for (int i = 0; i < m_AllJobKeys.Length; i++)
		{
			BaseItemFunctionality baseItemFunctionality2 = m_AllJobKeys[i].HasFunctionality(BaseItemFunctionality.Functionality.Key);
			if (baseItemFunctionality2 != null && m_AllJobKeys[i].m_ItemDataID != item.ItemDataID)
			{
				player.m_ItemContainer.RemoveItemRPC(item);
				break;
			}
		}
	}
}
