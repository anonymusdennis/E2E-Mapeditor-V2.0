using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomisationConfig", menuName = "Team17/Config/Create Customisation Config")]
public class CustomisationConfig : ScriptableObject
{
	[Localization]
	[Header("Names")]
	public string m_PrefixKey = string.Empty;

	public List<string> m_NamePool = new List<string>();

	[Header("Appearance")]
	public CustomisationSet m_Appearances = new CustomisationSet();

	public CustomisationData.Outfit m_DefaultOutfit;

	[Header("Accessory Probabilities")]
	public AccessoryWeights m_AccessoryWeights = new AccessoryWeights();
}
