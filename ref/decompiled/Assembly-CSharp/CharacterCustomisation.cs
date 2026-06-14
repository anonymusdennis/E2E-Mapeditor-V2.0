using System;
using UnityEngine;

public class CharacterCustomisation : T17MonoBehaviour
{
	public enum Mode
	{
		Static,
		Pool,
		Blueprint
	}

	[HideInInspector]
	public Mode m_Mode = Mode.Blueprint;

	[HideInInspector]
	public int m_BlueprintIdentifier;

	[HideInInspector]
	public CustomisationConfig m_Pool;

	[HideInInspector]
	public bool m_ShowDefaultOutfit;

	[Header("Name")]
	public string m_NamePrefixKey = string.Empty;

	public string m_RealName = "- A Character has no Name! -";

	public string m_SafeName = "SAFE NAME";

	[Header("Appearance")]
	public CustomisationData.BodyType m_BodyType = CustomisationData.BodyType.NULL;

	public CustomisationData.SkinColour m_SkinColour = CustomisationData.SkinColour.NULL;

	public CustomisationData.Hair m_Hair = CustomisationData.Hair.NULL;

	public CustomisationData.Hat m_Hat = CustomisationData.Hat.NULL;

	public CustomisationData.UpperFaceAccessory m_UpperFaceAccessory = CustomisationData.UpperFaceAccessory.NULL;

	public CustomisationData.LowerFaceAccessory m_LowerFaceAccessory = CustomisationData.LowerFaceAccessory.NULL;

	public CustomisationData.Outfit m_DefaultOutfit = CustomisationData.Outfit.NULL;

	[ReadOnly]
	[Header("Debug Info")]
	public string m_DisplayName = string.Empty;

	[ReadOnly]
	public CustomisationData.Outfit m_Outfit = CustomisationData.Outfit.NULL;

	[ReadOnly]
	public CustomisationData.Hair m_HairOverride = CustomisationData.Hair.NULL;

	[ReadOnly]
	public CustomisationData.Hat m_HatOverride = CustomisationData.Hat.NULL;

	[ReadOnly]
	public CustomisationData.UpperFaceAccessory m_UpperFaceOverride = CustomisationData.UpperFaceAccessory.NULL;

	[ReadOnly]
	public CustomisationData.LowerFaceAccessory m_LowerFaceOverride = CustomisationData.LowerFaceAccessory.NULL;

	private static UnityEngine.Random.State m_PoolRandomState = default(UnityEngine.Random.State);

	private CustomisationData.BodyData m_BodyTextureData;

	private bool m_IsForceNaked;

	private Character m_Character;

	private CharacterAnimator m_CharacterAnimator;

	public static void SetRandomSeed(UnityEngine.Random.State seed)
	{
		m_PoolRandomState = seed;
	}

	protected override void Awake()
	{
		base.Awake();
		m_Character = GetComponent<Character>();
		m_CharacterAnimator = GetComponent<CharacterAnimator>();
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		bool flag = false;
		if (m_Mode == Mode.Blueprint)
		{
			Customisation nPCDetails = PrisonCustomisationManager.GetNPCDetails(m_BlueprintIdentifier);
			if (nPCDetails != null)
			{
				SetCustomisation_Internal(nPCDetails);
				flag = true;
				if (Platform.GetInstance() != null)
				{
					Platform instance = Platform.GetInstance();
					instance.OnParentalsChanged = (Platform.OnlineAreaNewUserEvent)Delegate.Combine(instance.OnParentalsChanged, new Platform.OnlineAreaNewUserEvent(OnParentalsChanged));
				}
			}
			if (m_Character != null)
			{
				LevelScript instance2 = LevelScript.GetInstance();
				if (instance2 != null)
				{
					PrisonData levelSetup = instance2.m_LevelSetup;
					if (levelSetup.m_bAddRobinsonCharacter)
					{
						ConfigManager instance3 = ConfigManager.GetInstance();
						m_Character.m_bIsRobinsonCharacter = m_BlueprintIdentifier == levelSetup.m_CustomisableRoles[0] - 1 && (instance3 == null || instance3.gameType != PrisonConfig.ConfigType.Versus);
					}
				}
			}
		}
		else if (m_Mode == Mode.Pool)
		{
			if (m_Pool != null)
			{
				if (m_Character.m_CharacterRole == CharacterRole.Crowd)
				{
				}
				UnityEngine.Random.State state = UnityEngine.Random.state;
				UnityEngine.Random.state = m_PoolRandomState;
				Customisation details = new Customisation();
				flag = CustomisationManager.RandomiseFromPool(ref details, m_Pool);
				if (flag)
				{
					SetCustomisation_Internal(details);
				}
				m_PoolRandomState = UnityEngine.Random.state;
				UnityEngine.Random.state = state;
			}
		}
		else
		{
			SetDisplayName(FormatName(m_RealName, m_NamePrefixKey));
			flag = true;
		}
		if (!flag)
		{
			if (CustomisationManager.GetInstance() != null)
			{
				SetCustomisation_Internal(CustomisationManager.GetInstance().DefaultCustomisation);
			}
			else
			{
				m_BodyType = CustomisationData.BodyType.FEMALE;
				m_SkinColour = CustomisationData.SkinColour.WHITE;
				m_Hair = CustomisationData.Hair.NONE;
				m_Hat = CustomisationData.Hat.NONE;
				m_UpperFaceAccessory = CustomisationData.UpperFaceAccessory.NONE;
				m_LowerFaceAccessory = CustomisationData.LowerFaceAccessory.NONE;
				m_DefaultOutfit = CustomisationData.Outfit.NONE;
			}
		}
		if (m_Character != null)
		{
			CharacterRole characterRole = m_Character.m_CharacterRole;
			if (characterRole == CharacterRole.Dog || characterRole == CharacterRole.Ghost)
			{
				m_CharacterAnimator.SetMaterialHandHeld(null);
				return base.StartInit();
			}
		}
		m_BodyTextureData = CustomisationData.GetInstance().GetCustomisationData(m_BodyType, m_SkinColour);
		if (m_BodyTextureData == null)
		{
		}
		m_Outfit = (m_ShowDefaultOutfit ? m_DefaultOutfit : CustomisationData.Outfit.NONE);
		UpdateCharacterAnimator();
		m_CharacterAnimator.SetMaterialHandHeld(null);
		OnParentalsChanged();
		return base.StartInit();
	}

	protected virtual void OnDestroy()
	{
		if (Platform.GetInstance() != null)
		{
			Platform instance = Platform.GetInstance();
			instance.OnParentalsChanged = (Platform.OnlineAreaNewUserEvent)Delegate.Remove(instance.OnParentalsChanged, new Platform.OnlineAreaNewUserEvent(OnParentalsChanged));
		}
		m_Character = null;
		m_CharacterAnimator = null;
	}

	public void SetCustomisation(CharacterCustomisation other)
	{
		if (null == other)
		{
			T17NetManager.LogGoogleException("SetCustomisation null CharacterCustomisation parameter");
			return;
		}
		bool flag = other.m_BodyType != m_BodyType || other.m_SkinColour != m_SkinColour;
		m_NamePrefixKey = other.m_NamePrefixKey;
		m_RealName = other.m_RealName;
		m_SafeName = other.m_SafeName;
		SetDisplayName(FormatName(m_RealName, m_NamePrefixKey));
		m_BodyType = other.m_BodyType;
		m_SkinColour = other.m_SkinColour;
		m_Hair = other.m_Hair;
		m_Hat = other.m_Hat;
		m_UpperFaceAccessory = other.m_UpperFaceAccessory;
		m_LowerFaceAccessory = other.m_LowerFaceAccessory;
		m_DefaultOutfit = other.m_DefaultOutfit;
		m_HairOverride = other.m_HairOverride;
		m_HatOverride = other.m_HatOverride;
		m_UpperFaceOverride = other.m_UpperFaceOverride;
		m_LowerFaceOverride = other.m_LowerFaceOverride;
		m_Outfit = other.m_Outfit;
		if (flag)
		{
			m_BodyTextureData = CustomisationData.GetInstance().GetCustomisationData(m_BodyType, m_SkinColour);
		}
		UpdateCharacterAnimator();
	}

	public void SetCustomisationOverrides(Customisation overrides)
	{
		if (overrides == null)
		{
			T17NetManager.LogGoogleException("SetCustomisationOverrides null customisation parameter");
			return;
		}
		m_HairOverride = overrides.hair;
		m_HatOverride = overrides.hat;
		m_UpperFaceOverride = overrides.upperFace;
		m_LowerFaceOverride = overrides.lowerFace;
		m_Outfit = overrides.defaultOutfit;
		UpdateCharacterAnimator();
	}

	public void SetFaceOverrides(CustomisationData.LowerFaceAccessory lowerFaceOverride, CustomisationData.UpperFaceAccessory upperFaceOverride)
	{
		m_LowerFaceOverride = lowerFaceOverride;
		m_UpperFaceOverride = upperFaceOverride;
		UpdateCharacterAnimator();
	}

	public void ClearOverrides()
	{
		m_HairOverride = CustomisationData.Hair.NULL;
		m_HatOverride = CustomisationData.Hat.NULL;
		m_UpperFaceOverride = CustomisationData.UpperFaceAccessory.NULL;
		m_LowerFaceOverride = CustomisationData.LowerFaceAccessory.NULL;
	}

	public void SetCustomisation(Customisation customisation)
	{
		if (customisation == null)
		{
			T17NetManager.LogGoogleException("SetCustomisation null customisation parameter");
			return;
		}
		bool flag = customisation.body != m_BodyType || customisation.skin != m_SkinColour;
		SetCustomisation_Internal(customisation);
		if (null != m_Character && null != m_Character.m_CharacterStats && m_Character.m_CharacterStats.m_bIsPlayer)
		{
			Player player = (Player)m_Character;
			if (player != null && player.m_Gamer != null && !player.m_Gamer.IsLocal())
			{
				if (customisation.bUseSafeName)
				{
					SetDisplayName(FormatName(m_SafeName, m_NamePrefixKey));
				}
				else
				{
					OnParentalsChanged();
				}
			}
		}
		if (flag)
		{
			m_BodyTextureData = CustomisationData.GetInstance().GetCustomisationData(m_BodyType, m_SkinColour);
		}
		UpdateCharacterAnimator();
	}

	private void SetCustomisation_Internal(Customisation customisation)
	{
		m_NamePrefixKey = customisation.namePrefixKey;
		m_RealName = customisation.FinalName;
		m_SafeName = customisation.safeName;
		SetDisplayName(FormatName(m_RealName, m_NamePrefixKey));
		m_BodyType = customisation.body;
		m_SkinColour = customisation.skin;
		m_Hair = customisation.hair;
		m_Hat = customisation.hat;
		m_UpperFaceAccessory = customisation.upperFace;
		m_LowerFaceAccessory = customisation.lowerFace;
		m_DefaultOutfit = customisation.defaultOutfit;
	}

	private string FormatName(string name, string formatKey)
	{
		if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(formatKey))
		{
			return name;
		}
		string localised = null;
		if (!Localization.GetWithKeySwap(formatKey, out localised, "$name", name))
		{
			localised = name;
		}
		return localised;
	}

	public void SetDisplayName(string displayName)
	{
		if (string.Compare(displayName, m_DisplayName) != 0 && !string.IsNullOrEmpty(displayName))
		{
			m_DisplayName = displayName;
			if (m_Character != null)
			{
				m_Character.OnDisplayNameChanged(displayName);
			}
		}
	}

	public void SetRealName(string realName)
	{
		if (string.Compare(realName, m_RealName) != 0 && !string.IsNullOrEmpty(realName))
		{
			m_RealName = realName;
			OnParentalsChanged();
		}
	}

	public void SetOutfit(CustomisationData.Outfit outfit)
	{
		if (outfit != m_Outfit)
		{
			m_Outfit = outfit;
			m_HatOverride = CustomisationData.Hat.NULL;
			m_HairOverride = CustomisationData.Hair.NULL;
			m_UpperFaceOverride = CustomisationData.UpperFaceAccessory.NULL;
			m_LowerFaceOverride = CustomisationData.LowerFaceAccessory.NULL;
			UpdateCharacterAnimator();
		}
	}

	public void SetOutfit(Item item)
	{
		CustomisationData.Outfit outfit = CustomisationData.Outfit.NONE;
		CustomisationData.Hair hair = CustomisationData.Hair.NULL;
		CustomisationData.Hat hat = CustomisationData.Hat.NULL;
		CustomisationData.UpperFaceAccessory upperFaceAccessory = CustomisationData.UpperFaceAccessory.NULL;
		CustomisationData.LowerFaceAccessory lowerFaceAccessory = CustomisationData.LowerFaceAccessory.NULL;
		if (item != null && item.OutfitData != null)
		{
			outfit = item.OutfitData.m_OutfitAppearance;
			hair = item.OutfitData.m_HairOverride;
			hat = item.OutfitData.m_HatOverride;
			upperFaceAccessory = item.OutfitData.m_UpperFaceOverride;
			lowerFaceAccessory = item.OutfitData.m_LowerFaceOverride;
		}
		if (outfit != m_Outfit || hair != m_HairOverride || hat != m_HatOverride || upperFaceAccessory != m_UpperFaceOverride || lowerFaceAccessory != m_LowerFaceOverride)
		{
			m_Outfit = outfit;
			m_HairOverride = hair;
			m_HatOverride = hat;
			m_UpperFaceOverride = upperFaceAccessory;
			m_LowerFaceOverride = lowerFaceAccessory;
			UpdateCharacterAnimator();
		}
	}

	public void SetForceNaked(bool naked)
	{
		if (naked != m_IsForceNaked)
		{
			m_IsForceNaked = naked;
			UpdateCharacterAnimator();
		}
	}

	private void UpdateCharacterAnimator()
	{
		SetupCharacterAnimator(m_CharacterAnimator);
	}

	public bool SetupCharacterAnimator(CharacterAnimator animator)
	{
		if (m_BodyTextureData == null)
		{
			return false;
		}
		if (null == animator)
		{
			T17NetManager.LogGoogleException("SetupCharacterAnimator - animator is null");
			return false;
		}
		CustomisationData instance = CustomisationData.GetInstance();
		if (null == instance)
		{
			T17NetManager.LogGoogleException("SetupCharacterAnimator - CustomisationData is null");
			return false;
		}
		Material material = null;
		Material material2 = null;
		Material material3 = null;
		Material material4 = null;
		Material material5 = null;
		if (m_IsForceNaked)
		{
			material = m_BodyTextureData.GetMaterialForOutfit(CustomisationData.Outfit.NONE);
			material2 = instance.GetMaterialForHair(m_Hair);
			material3 = instance.GetMaterialForHat(m_Hat);
			material4 = instance.GetMaterialForUpperFace(m_UpperFaceAccessory);
			material5 = instance.GetMaterialForLowerFace(m_LowerFaceAccessory);
		}
		else
		{
			material = m_BodyTextureData.GetMaterialForOutfit((m_Outfit != CustomisationData.Outfit.NULL) ? m_Outfit : CustomisationData.Outfit.NONE);
			material2 = instance.GetMaterialForHair((m_HairOverride == CustomisationData.Hair.NULL) ? m_Hair : m_HairOverride);
			material3 = instance.GetMaterialForHat((m_HatOverride == CustomisationData.Hat.NULL) ? m_Hat : m_HatOverride);
			material4 = instance.GetMaterialForUpperFace((m_UpperFaceOverride == CustomisationData.UpperFaceAccessory.NULL) ? m_UpperFaceAccessory : m_UpperFaceOverride);
			material5 = instance.GetMaterialForLowerFace((m_LowerFaceOverride == CustomisationData.LowerFaceAccessory.NULL) ? m_LowerFaceAccessory : m_LowerFaceOverride);
		}
		animator.SetMaterialAppearance(material, material2, material3, material4, material5);
		return true;
	}

	private void OnParentalsChanged()
	{
		if (PrisonCustomisationManager.m_bUGCBlockEnforced && !T17NetManager.IsMasterClient && m_Character != null && !m_Character.m_CharacterStats.m_bIsPlayer)
		{
			SetDisplayName(FormatName(m_SafeName, m_NamePrefixKey));
		}
		else if (Platform.GetInstance() != null)
		{
			Platform.GetInstance().IsUGCRestrictedRequest(OnUGCRestrictedRequest);
		}
	}

	private void OnUGCRestrictedRequest(bool isRestricted, Platform.OnlineAccessErrorCode returnCode, bool failureHandledPlatformside)
	{
		string text = null;
		if (!isRestricted || !(m_Character != null) || !m_Character.m_CharacterStats.m_bIsPlayer)
		{
			text = ((!isRestricted || T17NetManager.IsMasterClient) ? m_RealName : m_SafeName);
		}
		else
		{
			Player player = (Player)m_Character;
			if (player != null)
			{
				text = ((player.m_Gamer == null) ? m_SafeName : ((!player.m_Gamer.IsLocal()) ? m_SafeName : m_RealName));
			}
		}
		SetDisplayName(FormatName(text, m_NamePrefixKey));
	}
}
