using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VendorConfig", menuName = "Team17/Config/Create Vendor Config")]
public class VendorConfig : ScriptableObject
{
	[Header("Settings")]
	[Range(0f, 2f)]
	public float m_ItemCostModifier = 1f;

	[Range(0f, 100f)]
	public int m_RequiredOpinion;

	[Range(0f, 12f)]
	public int m_MinItems;

	[Range(0f, 12f)]
	public int m_MaxItems;

	[Header("Time Settings (in Game-Minutes)")]
	public int m_MinVendorDuration;

	public int m_MaxVendorDuration;

	[Header("Vendor Limit")]
	public int m_MaxVendors = 6;

	[Header("Items")]
	public List<VendorManager.WeightedItemGroup> m_PossibleItemSets = new List<VendorManager.WeightedItemGroup>();
}
