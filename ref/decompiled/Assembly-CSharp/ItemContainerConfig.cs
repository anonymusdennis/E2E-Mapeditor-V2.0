using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemContainerConfig", menuName = "Team17/Config/Create Desk Config")]
public class ItemContainerConfig : ScriptableObject
{
	[Header("StartingItems")]
	public List<ItemData> m_StartingItems = new List<ItemData>();

	public bool m_KeepOldStartingItems;

	[Space]
	public List<ItemData> m_TrackedItems = new List<ItemData>();

	public bool m_KeepOldTrackedItems;

	[HideInInspector]
	public bool m_ReplaceRandomGroups = true;

	[HideInInspector]
	public List<RandomItemGroup> m_RandomGroups = new List<RandomItemGroup>();

	[HideInInspector]
	public int[] m_RandomPercentages = new int[0];

	[HideInInspector]
	public int m_NumberFromGroups = 1;

	[HideInInspector]
	public bool m_UniqueFromGroups = true;

	[HideInInspector]
	public bool m_AllowRefresh;
}
