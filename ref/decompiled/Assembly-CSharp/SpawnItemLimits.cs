using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnItemLimits", menuName = "Team17/Items/Create Spawn Item Limits")]
public class SpawnItemLimits : ScriptableObject
{
	[Serializable]
	public class ItemLimit
	{
		public ItemData m_Item;

		[Range(1f, 20f)]
		public int m_Min = 1;

		[Range(2f, 20f)]
		public int m_Max = 2;
	}

	public List<ItemLimit> m_ItemLimits;
}
