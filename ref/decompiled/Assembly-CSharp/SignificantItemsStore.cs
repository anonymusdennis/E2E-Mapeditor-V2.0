using System.Collections.Generic;
using UnityEngine;

public class SignificantItemsStore : MonoBehaviour
{
	private static SignificantItemsStore s_SharedInstance;

	public List<ItemData> m_ItemsForQuickMoldTutorial = new List<ItemData>();

	public static SignificantItemsStore GetInstance()
	{
		return s_SharedInstance;
	}

	private void Awake()
	{
		if (s_SharedInstance == null)
		{
			s_SharedInstance = this;
			Object.DontDestroyOnLoad(this);
		}
	}

	protected virtual void OnDestroy()
	{
		if (s_SharedInstance == this)
		{
			s_SharedInstance = null;
		}
	}

	public bool ShouldItemTriggerQuickMoldTutorial(Item theItem)
	{
		if (theItem != null)
		{
			for (int num = m_ItemsForQuickMoldTutorial.Count - 1; num >= 0; num--)
			{
				if (theItem.ItemDataID == m_ItemsForQuickMoldTutorial[num].m_ItemDataID)
				{
					return true;
				}
			}
		}
		return false;
	}
}
