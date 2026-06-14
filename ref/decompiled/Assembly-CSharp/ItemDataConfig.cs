using UnityEngine;

[CreateAssetMenu(fileName = "ItemConfig", menuName = "Team17/Config/Create Item Override Config")]
public class ItemDataConfig : ScriptableObject
{
	public int m_ItemDataID = -1;

	public bool m_OverrideBaseData;

	public int m_ItemHealth = 100;

	public int m_ItemValue;

	public ItemData.GiftValues m_GiftOpinionValues = new ItemData.GiftValues();

	public bool m_bIsContraband;

	public Item_Combat m_CombatData;

	public bool m_OverrideOutfitData;

	public bool m_OverrideDisguise;

	public Item_Outfit.OutFitType m_OutfitType = Item_Outfit.OutFitType.Inmate;

	public bool m_OverrideArmour;

	public ArmourConfig m_ArmourConfig;

	public bool m_OverrideOutfit;

	public CustomisationData.Outfit m_OutfitAppearance;

	public CustomisationData.Hair m_HairOverride = CustomisationData.Hair.NULL;

	public CustomisationData.Hat m_HatOverride = CustomisationData.Hat.NULL;

	public CustomisationData.UpperFaceAccessory m_UpperFaceOverride = CustomisationData.UpperFaceAccessory.NULL;

	public CustomisationData.LowerFaceAccessory m_LowerFaceOverride = CustomisationData.LowerFaceAccessory.NULL;

	public bool m_OverrideAppearance;

	public Sprite m_ItemUIImage;

	public Texture m_ItemWorldImage;

	public Material m_ItemWorldMaterial;

	public Texture m_ItemHeldImage;

	public Material m_ItemHeldMaterial;

	public Material m_ItemBoundMaterial;
}
