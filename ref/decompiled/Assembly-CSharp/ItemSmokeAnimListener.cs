using UnityEngine;

public class ItemSmokeAnimListener : MonoBehaviour
{
	private InventoryItem m_CallbackItem;

	public static void SetupListenerToObject(GameObject smokePlayingObject, InventoryItem callbackItem)
	{
		ItemSmokeAnimListener itemSmokeAnimListener = smokePlayingObject.GetComponent<ItemSmokeAnimListener>();
		if (itemSmokeAnimListener == null)
		{
			itemSmokeAnimListener = smokePlayingObject.AddComponent<ItemSmokeAnimListener>();
		}
		itemSmokeAnimListener.m_CallbackItem = callbackItem;
	}

	private void SmokeCriticalPoint()
	{
		if (m_CallbackItem != null)
		{
			m_CallbackItem.SmokeCriticalPointCallback();
		}
	}

	private void SmokeComplete()
	{
		if (m_CallbackItem != null)
		{
			m_CallbackItem.SmokeCompleteCallback();
		}
	}
}
