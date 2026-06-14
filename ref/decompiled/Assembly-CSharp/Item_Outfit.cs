using UnityEngine;

[CreateAssetMenu(fileName = "TYPE_ItemOutfit", menuName = "Team17/Items/Create New Outfit Config")]
public class Item_Outfit : ScriptableObject
{
	public enum OutFitType
	{
		None,
		Guard,
		Inmate,
		Medic,
		Warden,
		Civilian
	}

	[Header("Disguise Category")]
	public OutFitType m_Type = OutFitType.Inmate;

	[Header("Appearance")]
	public CustomisationData.Outfit m_OutfitAppearance;

	public CustomisationData.Hair m_HairOverride = CustomisationData.Hair.NULL;

	public CustomisationData.Hat m_HatOverride = CustomisationData.Hat.NULL;

	public CustomisationData.UpperFaceAccessory m_UpperFaceOverride = CustomisationData.UpperFaceAccessory.NULL;

	public CustomisationData.LowerFaceAccessory m_LowerFaceOverride = CustomisationData.LowerFaceAccessory.NULL;

	[Header("Combat Data")]
	public ArmourConfig m_ArmourConfig;
}
