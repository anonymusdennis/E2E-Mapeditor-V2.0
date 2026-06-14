using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "NEWITEM_ItemData", menuName = "Team17/Items/Create New Item")]
public class ItemData : ScriptableObject
{
	[Serializable]
	public class FunctionalityData
	{
		public BaseItemFunctionality m_Functionality;

		public AnimState m_UseAnimation = AnimState.Idle;

		public float m_UseTime;

		public Vector2 m_UseOffset = new Vector2(0f, 0f);
	}

	[Serializable]
	public class GiftValues
	{
		public int Inmate;

		public int Guard;
	}

	[Serializable]
	public enum ITEM_ANIMATION_TYPE
	{
		IAT_NOT_SET,
		IAT_SINGLE,
		IAT_DOUBLE,
		IAT_MOP,
		IAT_HAMMER,
		IAT_ROPE,
		IAT_OTHER
	}

	[Serializable]
	public enum MATERIAL_TYPE
	{
		MAT_GENERIC,
		MAT_METAL,
		MAT_WOOD
	}

	[ReadOnly]
	[Header("Item Info")]
	public int m_ItemDataID;

	[Localization]
	public string m_ItemLocalizationTag = "Text.Item.ItemName";

	public int m_ItemHealth = 100;

	private int m_BaseHealth = 100;

	public int m_ItemValue;

	public bool m_bIsContraband;

	public int m_AlertnessIncreaseWhenFound;

	public bool m_bCanSwitchInUse = true;

	[Header("Gifting")]
	public GiftValues m_GiftOpinionValues = new GiftValues();

	public bool m_bCanBeGiftedToGuards = true;

	[Header("Usability")]
	public bool m_CanBeEquiped = true;

	[SerializeField]
	[Tooltip("Functionalities should be ordered in priority! Highest priority First!")]
	public List<FunctionalityData> m_ItemFunctionalities = new List<FunctionalityData>();

	private bool bIsWorkingCopySoClearFunctionalitiesData;

	[Header("Item Visual")]
	public Sprite m_ItemUIImage;

	public Sprite m_ItemUIMapImage;

	public Vector2 m_UIMapWorldPositionOffset = Vector2.zero;

	public Texture m_ItemWorldImage;

	public Material m_ItemWorldMaterial;

	public Texture m_ItemHeldImage;

	public Material m_ItemHeldMaterial;

	public ITEM_ANIMATION_TYPE m_ItemHeldType;

	public Texture m_ItemUseImage;

	public Material m_ItemUseMaterial;

	public ITEM_ANIMATION_TYPE m_ItemUseType;

	public Material m_ItemBoundMaterial;

	public Material m_ItemTileCoverVerticalMaterial;

	public Material m_ItemTileCoverHorizontalMaterial;

	[Header("Combat")]
	public Item_Combat m_CombatData;

	public Item_Outfit m_OutfitData;

	public MATERIAL_TYPE m_MaterialType;

	[EnumFlag("Allowed Prisons")]
	public LevelScript.PRISON_ENUM_MASK m_PrisonMask = (LevelScript.PRISON_ENUM_MASK)Enum.ToObject(typeof(LevelScript.PRISON_ENUM_MASK), -1);

	[SerializeField]
	private byte m_Flags;

	private byte CONTRABAND_FLAG = 1;

	private byte TOOL_FLAG = 4;

	private byte COMPONENT_FLAG = 8;

	private byte WEAPON_FLAG = 16;

	private byte OUTFIT_FLAG = 32;

	private byte CONSUMABLE_FLAG = 64;

	public byte GetFlags => m_Flags;

	public ItemData(ItemData data)
	{
		CopyData(data);
	}

	public ItemData()
	{
		InitFunctionalities();
	}

	protected virtual void OnDestroy()
	{
		if (bIsWorkingCopySoClearFunctionalitiesData)
		{
			for (int num = m_ItemFunctionalities.Count - 1; num >= 0; num--)
			{
				if (m_ItemFunctionalities[num] != null)
				{
					UnityEngine.Object.Destroy(m_ItemFunctionalities[num].m_Functionality);
					m_ItemFunctionalities[num].m_Functionality = null;
					m_ItemFunctionalities.RemoveAt(num);
				}
			}
			m_ItemFunctionalities = null;
			bIsWorkingCopySoClearFunctionalitiesData = false;
		}
		if (m_ItemWorldMaterial != null)
		{
			m_ItemWorldMaterial.mainTexture = null;
			m_ItemWorldMaterial = null;
		}
		if (m_ItemHeldMaterial != null)
		{
			m_ItemHeldMaterial.mainTexture = null;
			m_ItemHeldMaterial = null;
		}
		if (m_ItemUseMaterial != null)
		{
			m_ItemUseMaterial.mainTexture = null;
			m_ItemUseMaterial = null;
		}
		if (m_ItemBoundMaterial != null)
		{
			m_ItemBoundMaterial.mainTexture = null;
			m_ItemBoundMaterial = null;
		}
		if (m_ItemTileCoverVerticalMaterial != null)
		{
			m_ItemTileCoverVerticalMaterial.mainTexture = null;
			m_ItemTileCoverVerticalMaterial = null;
		}
		if (m_ItemTileCoverHorizontalMaterial != null)
		{
			m_ItemTileCoverHorizontalMaterial.mainTexture = null;
			m_ItemTileCoverHorizontalMaterial = null;
		}
	}

	public BaseItemFunctionality HasFunctionality(BaseItemFunctionality.Functionality function)
	{
		if (m_ItemFunctionalities == null)
		{
			return null;
		}
		for (int i = 0; i < m_ItemFunctionalities.Count; i++)
		{
			if (m_ItemFunctionalities[i] != null && m_ItemFunctionalities[i].m_Functionality != null && m_ItemFunctionalities[i].m_Functionality.GetFunctionalityType() == function)
			{
				return m_ItemFunctionalities[i].m_Functionality;
			}
		}
		return null;
	}

	public bool IsContraband()
	{
		if ((m_Flags & CONTRABAND_FLAG) != 0)
		{
			return true;
		}
		return false;
	}

	public bool IsTool()
	{
		if ((m_Flags & TOOL_FLAG) != 0)
		{
			return true;
		}
		if (m_ItemFunctionalities == null)
		{
			return false;
		}
		for (int i = 0; i < m_ItemFunctionalities.Count; i++)
		{
			if (m_ItemFunctionalities[i] == null)
			{
				continue;
			}
			switch (m_ItemFunctionalities[i].m_Functionality.GetFunctionalityType())
			{
			case BaseItemFunctionality.Functionality.Dig:
			case BaseItemFunctionality.Functionality.Chip:
			case BaseItemFunctionality.Functionality.Cut:
			case BaseItemFunctionality.Functionality.Unscrew:
			case BaseItemFunctionality.Functionality.HideContraband:
			case BaseItemFunctionality.Functionality.CoverTile:
				m_Flags |= TOOL_FLAG;
				return true;
			case BaseItemFunctionality.Functionality.Key:
			{
				KeyFunctionality keyFunctionality = (KeyFunctionality)m_ItemFunctionalities[i].m_Functionality;
				if (!keyFunctionality.IsDurable)
				{
					m_Flags |= TOOL_FLAG;
					return true;
				}
				break;
			}
			}
		}
		return false;
	}

	public bool IsWeapon()
	{
		if ((m_Flags & WEAPON_FLAG) != 0)
		{
			return true;
		}
		if (m_CombatData != null)
		{
			m_Flags |= WEAPON_FLAG;
			return true;
		}
		return false;
	}

	public bool IsOutfit()
	{
		if ((m_Flags & OUTFIT_FLAG) != 0)
		{
			return true;
		}
		if (m_OutfitData != null)
		{
			m_Flags |= OUTFIT_FLAG;
			return true;
		}
		return false;
	}

	public bool IsComponent()
	{
		if ((m_Flags & COMPONENT_FLAG) != 0)
		{
			return true;
		}
		return false;
	}

	public bool IsConsumable()
	{
		if ((m_Flags & CONSUMABLE_FLAG) != 0)
		{
			return true;
		}
		if (m_ItemFunctionalities == null)
		{
			return false;
		}
		for (int i = 0; i < m_ItemFunctionalities.Count; i++)
		{
			if (m_ItemFunctionalities[i] != null && m_ItemFunctionalities[i].m_Functionality.GetFunctionalityType() == BaseItemFunctionality.Functionality.StatChange)
			{
				m_Flags |= CONSUMABLE_FLAG;
				return true;
			}
		}
		return false;
	}

	public void SetComponent()
	{
		m_Flags |= COMPONENT_FLAG;
	}

	public void Init()
	{
		if (m_ItemUseType == ITEM_ANIMATION_TYPE.IAT_NOT_SET)
		{
			m_ItemUseType = m_ItemHeldType;
		}
		InitFunctionalities();
		if (m_bIsContraband)
		{
			m_Flags |= CONTRABAND_FLAG;
		}
	}

	private void InitFunctionalities()
	{
		if (m_ItemFunctionalities == null)
		{
			return;
		}
		for (int i = 0; i < m_ItemFunctionalities.Count; i++)
		{
			if (m_ItemFunctionalities[i] != null && m_ItemFunctionalities[i].m_Functionality != null)
			{
				m_ItemFunctionalities[i].m_Functionality.Init();
			}
		}
	}

	public void CopyData(ItemData data)
	{
		m_ItemLocalizationTag = data.m_ItemLocalizationTag;
		m_ItemHealth = data.m_ItemHealth;
		m_BaseHealth = m_ItemHealth;
		m_ItemValue = data.m_ItemValue;
		m_ItemDataID = data.m_ItemDataID;
		m_CanBeEquiped = data.m_CanBeEquiped;
		m_GiftOpinionValues.Inmate = data.m_GiftOpinionValues.Inmate;
		m_GiftOpinionValues.Guard = data.m_GiftOpinionValues.Guard;
		m_ItemFunctionalities = new List<FunctionalityData>();
		for (int i = 0; i < data.m_ItemFunctionalities.Count; i++)
		{
			if (data.m_ItemFunctionalities[i] != null && data.m_ItemFunctionalities[i].m_Functionality != null)
			{
				bIsWorkingCopySoClearFunctionalitiesData = true;
				FunctionalityData functionalityData = new FunctionalityData();
				functionalityData.m_Functionality = UnityEngine.Object.Instantiate(data.m_ItemFunctionalities[i].m_Functionality);
				functionalityData.m_UseAnimation = data.m_ItemFunctionalities[i].m_UseAnimation;
				functionalityData.m_UseTime = data.m_ItemFunctionalities[i].m_UseTime;
				functionalityData.m_UseOffset = data.m_ItemFunctionalities[i].m_UseOffset;
				m_ItemFunctionalities.Add(functionalityData);
			}
		}
		InitFunctionalities();
		m_ItemUIImage = data.m_ItemUIImage;
		m_ItemUIMapImage = data.m_ItemUIMapImage;
		m_UIMapWorldPositionOffset = data.m_UIMapWorldPositionOffset;
		m_ItemWorldImage = data.m_ItemWorldImage;
		m_ItemWorldMaterial = data.m_ItemWorldMaterial;
		m_ItemHeldImage = data.m_ItemHeldImage;
		m_ItemHeldMaterial = data.m_ItemHeldMaterial;
		m_ItemHeldType = data.m_ItemHeldType;
		m_ItemUseImage = data.m_ItemUseImage;
		m_ItemUseMaterial = data.m_ItemUseMaterial;
		m_ItemUseType = data.m_ItemUseType;
		m_CombatData = data.m_CombatData;
		m_OutfitData = data.m_OutfitData;
		m_Flags = data.m_Flags;
		m_MaterialType = data.m_MaterialType;
		m_ItemTileCoverHorizontalMaterial = data.m_ItemTileCoverHorizontalMaterial;
		m_ItemTileCoverVerticalMaterial = data.m_ItemTileCoverVerticalMaterial;
		m_ItemBoundMaterial = data.m_ItemBoundMaterial;
		m_bCanBeGiftedToGuards = data.m_bCanBeGiftedToGuards;
		m_AlertnessIncreaseWhenFound = data.m_AlertnessIncreaseWhenFound;
		m_bIsContraband = data.m_bIsContraband;
		m_bCanSwitchInUse = data.m_bCanSwitchInUse;
	}

	public void ApplyConfigData(ItemDataConfig config)
	{
		if (config.m_OverrideBaseData)
		{
			m_ItemHealth = config.m_ItemHealth;
			m_BaseHealth = m_ItemHealth;
			m_ItemValue = config.m_ItemValue;
			m_GiftOpinionValues.Inmate = config.m_GiftOpinionValues.Inmate;
			m_GiftOpinionValues.Guard = config.m_GiftOpinionValues.Guard;
			m_bIsContraband = config.m_bIsContraband;
			m_CombatData = config.m_CombatData;
		}
		if (config.m_OverrideOutfitData)
		{
			m_OutfitData = UnityEngine.Object.Instantiate(m_OutfitData);
			if (config.m_OverrideDisguise)
			{
				m_OutfitData.m_Type = config.m_OutfitType;
			}
			if (config.m_OverrideArmour)
			{
				m_OutfitData.m_ArmourConfig = config.m_ArmourConfig;
			}
			if (config.m_OverrideOutfit)
			{
				m_OutfitData.m_OutfitAppearance = config.m_OutfitAppearance;
				m_OutfitData.m_HairOverride = config.m_HairOverride;
				m_OutfitData.m_HatOverride = config.m_HatOverride;
				m_OutfitData.m_UpperFaceOverride = config.m_UpperFaceOverride;
				m_OutfitData.m_LowerFaceOverride = config.m_LowerFaceOverride;
			}
		}
		if (config.m_OverrideAppearance)
		{
			m_ItemUIImage = config.m_ItemUIImage;
			m_ItemWorldImage = config.m_ItemWorldImage;
			m_ItemWorldMaterial = config.m_ItemWorldMaterial;
			m_ItemHeldImage = config.m_ItemHeldImage;
			m_ItemHeldMaterial = config.m_ItemHeldMaterial;
			m_ItemBoundMaterial = config.m_ItemBoundMaterial;
		}
	}

	public void SetParentItem(Item parent)
	{
		for (int i = 0; i < m_ItemFunctionalities.Count; i++)
		{
			if (m_ItemFunctionalities[i] != null && m_ItemFunctionalities[i].m_Functionality != null)
			{
				m_ItemFunctionalities[i].m_Functionality.ParentItem = parent;
			}
		}
	}

	public void ResetHealth()
	{
		m_ItemHealth = m_BaseHealth;
	}
}
