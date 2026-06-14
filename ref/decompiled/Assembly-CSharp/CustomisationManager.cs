using System.Collections.Generic;
using UnityEngine;

public class CustomisationManager : T17MonoBehaviour
{
	private const string PLAYER_SAVE_ID = "Customisations:PlayerPresets";

	private const string PRISON_SAVE_PREFIX = "Customisations:PrisonSetup";

	private const int FLUSH_BLUEPRINT_CUSTOM = 1;

	[Header("Presets")]
	public Customisation DefaultCustomisation = new Customisation();

	public List<Customisation> DefaultPlayerPresets = new List<Customisation>();

	[Header("Influencers")]
	public List<Customisation> InfluencerPresets = new List<Customisation>();

	private Customisation[] m_PlayerPresets;

	private Dictionary<string, Customisation[]> m_PrisonCustomisations = new Dictionary<string, Customisation[]>();

	private bool[] m_PlayerPresetsModified;

	private int m_LastChosenPresetIndex;

	private bool m_bShouldSavePlayerPresets;

	private bool m_bShouldSaveChosenPreset;

	private bool m_bShouldSavePrison;

	private PrisonData m_PrisonToSave;

	private static CustomisationManager m_Instance;

	public static CustomisationManager GetInstance()
	{
		return m_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		m_Instance = this;
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	private void Update()
	{
		if (m_bShouldSavePlayerPresets)
		{
			m_bShouldSavePlayerPresets = false;
			SavePlayerPresets();
			SavePlayerPresetModified();
		}
		if (m_bShouldSavePrison)
		{
			m_bShouldSavePrison = false;
			if (SavePrisonCustomisation(m_PrisonToSave))
			{
				m_PrisonToSave = null;
			}
		}
		if (m_bShouldSaveChosenPreset)
		{
			m_bShouldSaveChosenPreset = false;
			SavePlayerChosenPreset();
		}
	}

	public Customisation[] GetPlayerPresets()
	{
		return m_PlayerPresets;
	}

	public int GetPlayerLastChosenPreset()
	{
		return m_LastChosenPresetIndex;
	}

	public bool GetHasPresetBeenModified(int slotIndex)
	{
		if (m_PlayerPresetsModified == null || slotIndex > m_PlayerPresetsModified.Length || slotIndex < 0)
		{
			return false;
		}
		return m_PlayerPresetsModified[slotIndex];
	}

	public Customisation[] GenerateDefaultCustomisations(PrisonData prison)
	{
		List<Customisation> list = new List<Customisation>();
		if (prison.m_CustomisableRoles != null && prison.m_CustomisableRoles.Length > 0)
		{
			int num = 0;
			for (int i = 0; i < prison.m_CustomisableRoles.Length; i++)
			{
				num += prison.m_CustomisableRoles[i];
			}
			for (int j = 0; j < num; j++)
			{
				list.Add(new Customisation(DefaultCustomisation, takeFilteredName: false));
			}
		}
		return list.ToArray();
	}

	public bool RandomiseCustomisations(PrisonData prison)
	{
		if (prison == null || prison.m_CustomisableRoles == null || prison.m_CustomisableRoles.Length <= 0)
		{
			return false;
		}
		Customisation[] customisableNpcsForPrison = GetCustomisableNpcsForPrison(prison);
		if (customisableNpcsForPrison == null || customisableNpcsForPrison.Length <= 0)
		{
			return false;
		}
		int num = 0;
		for (int i = 0; i < prison.m_CustomisableRoles.Length; i++)
		{
			int num2 = customisableNpcsForPrison.Length - num;
			if (num2 <= 0)
			{
				break;
			}
			CustomisationConfig customisationConfig = prison.m_CustomisationPools[i];
			int num3 = Mathf.Min(prison.m_CustomisableRoles[i], num2);
			if (customisationConfig == null || num3 <= 0)
			{
				num += num3;
				continue;
			}
			CustomisationConstraint customisationConstraint = prison.m_CustomisationConstraints[i];
			List<string> list = new List<string>(customisationConfig.m_NamePool);
			for (int j = 0; j < num3; j++)
			{
				Customisation details = customisableNpcsForPrison[num];
				if (!RandomiseFromPool(ref details, customisationConfig))
				{
					continue;
				}
				if (customisationConstraint != null)
				{
					ApplyConstraint(ref details, customisationConstraint);
				}
				if (customisationConfig.m_NamePool.Count > 0)
				{
					if (list.Count <= 0)
					{
						list.Clear();
						list.AddRange(customisationConfig.m_NamePool);
					}
					int index = Random.Range(0, list.Count);
					details.name = list[index];
					details.safeName = details.name;
					list.RemoveAt(index);
				}
				num++;
			}
		}
		return true;
	}

	public bool InsertRandomInfluencers(PrisonData prison)
	{
		if (prison == null || prison.m_InfluencerWeights == null || prison.m_InfluencerWeights.Length <= 0)
		{
			return false;
		}
		if (InfluencerPresets == null || InfluencerPresets.Count <= 0)
		{
			return false;
		}
		Customisation[] customisableNpcsForPrison = GetCustomisableNpcsForPrison(prison);
		if (customisableNpcsForPrison == null || customisableNpcsForPrison.Length <= 0)
		{
			return false;
		}
		GlobalSave.GetInstance().Get("Settings:Influencers", out var value, 1f);
		if (value < 0.5f)
		{
			return false;
		}
		List<Customisation> list = new List<Customisation>(InfluencerPresets);
		int num = 0;
		list.Shuffle();
		int num2 = 0;
		for (int i = 0; i < prison.m_CustomisableRoles.Length; i++)
		{
			int num3 = list.Count - num;
			if (num3 <= 0)
			{
				break;
			}
			int num4 = customisableNpcsForPrison.Length - num2;
			if (num4 <= 0)
			{
				break;
			}
			int num5 = Mathf.Min(prison.m_CustomisableRoles[i], num4);
			if (num5 <= 0)
			{
				num2 += num5;
				continue;
			}
			InfluencerWeights influencerWeights = prison.m_InfluencerWeights[i];
			if (influencerWeights != null)
			{
				int num6 = Mathf.Min(Random.Range(influencerWeights.min, influencerWeights.max), num3, num5);
				if (num6 > 0)
				{
					int num7 = num2 + num5;
					if (prison.m_bAddRobinsonCharacter && i == 0)
					{
						num7--;
					}
					List<int> list2 = new List<int>(num6);
					for (int j = 0; j < num6; j++)
					{
						int num8 = Random.Range(num2, num7);
						for (int k = 0; k < list2.Count; k++)
						{
							if (!list2.Contains(num8))
							{
								break;
							}
							num8++;
							if (num8 >= num7)
							{
								num8 = num2;
							}
						}
						if (num8 >= 0)
						{
							Customisation customisation = list[num];
							Customisation customisation2 = customisableNpcsForPrison[num8];
							if (customisation != null && customisation2 != null)
							{
								customisation2.name = customisation.name;
								customisation2.safeName = customisation.safeName;
								if (customisation.body != CustomisationData.BodyType.NULL)
								{
									customisation2.body = customisation.body;
								}
								if (customisation.skin != CustomisationData.SkinColour.NULL)
								{
									customisation2.skin = customisation.skin;
								}
								if (customisation.hair != CustomisationData.Hair.NULL)
								{
									customisation2.hair = customisation.hair;
								}
								if (customisation.hat != CustomisationData.Hat.NULL)
								{
									customisation2.hat = customisation.hat;
								}
								if (customisation.upperFace != CustomisationData.UpperFaceAccessory.NULL)
								{
									customisation2.upperFace = customisation.upperFace;
								}
								if (customisation.lowerFace != CustomisationData.LowerFaceAccessory.NULL)
								{
									customisation2.lowerFace = customisation.lowerFace;
								}
							}
							num++;
						}
						list2.Add(num8);
					}
				}
			}
			num2 += num5;
		}
		return true;
	}

	public static bool RandomiseFromPool(ref Customisation details, CustomisationConfig pool)
	{
		details.namePrefixKey = pool.m_PrefixKey;
		if (pool.m_NamePool.Count > 0)
		{
			details.name = pool.m_NamePool[Random.Range(0, pool.m_NamePool.Count)];
			details.safeName = details.name;
			if (details.name == "Dye")
			{
				details.namePrefixKey = "Text.NamePrefix.Doctor";
			}
		}
		if (pool.m_Appearances.bodyTypes.Count > 0)
		{
			details.body = pool.m_Appearances.bodyTypes[Random.Range(0, pool.m_Appearances.bodyTypes.Count)];
		}
		if (pool.m_Appearances.skinColours.Count > 0)
		{
			details.skin = pool.m_Appearances.skinColours[Random.Range(0, pool.m_Appearances.skinColours.Count)];
		}
		if (pool.m_Appearances.hairs.Count > 0)
		{
			details.hair = pool.m_Appearances.hairs[Random.Range(0, pool.m_Appearances.hairs.Count)];
		}
		if (pool.m_Appearances.hats.Count > 0)
		{
			details.hat = RandomiseWithWeightedDefault(pool.m_Appearances.hats, pool.m_AccessoryWeights.defaultHat, pool.m_AccessoryWeights.hats);
		}
		if (pool.m_Appearances.upperFaces.Count > 0)
		{
			details.upperFace = RandomiseWithWeightedDefault(pool.m_Appearances.upperFaces, pool.m_AccessoryWeights.defaultUpperFace, pool.m_AccessoryWeights.upperFaces);
		}
		if (pool.m_Appearances.lowerFaces.Count > 0)
		{
			details.lowerFace = RandomiseWithWeightedDefault(pool.m_Appearances.lowerFaces, pool.m_AccessoryWeights.defaultLowerFace, pool.m_AccessoryWeights.lowerFaces);
		}
		if (pool.m_DefaultOutfit > CustomisationData.Outfit.NULL)
		{
			details.defaultOutfit = pool.m_DefaultOutfit;
		}
		return true;
	}

	private static T RandomiseWithWeightedDefault<T>(List<T> options, T defaultValue, float nonDefaultChance)
	{
		T val = (T)(object)0;
		int num = options.IndexOf(defaultValue);
		if (num > -1)
		{
			float num2 = 1f - nonDefaultChance;
			if (options.Count != 1 && !(Random.value < num2))
			{
				int num3 = Random.Range(0, options.Count - 1);
				if (num3 >= num)
				{
					return options[num3 + 1];
				}
				return options[num3];
			}
			return defaultValue;
		}
		return options[Random.Range(0, options.Count)];
	}

	public static bool ApplyConstraint(ref CustomisationSet set, CustomisationConstraint constraint)
	{
		if (set == null || constraint == null)
		{
			return false;
		}
		if (constraint.allowed.bodyTypes.Count > 0)
		{
			FilterEnumLists(ref set.bodyTypes, constraint.allowed.bodyTypes);
		}
		if (constraint.allowed.skinColours.Count > 0)
		{
			FilterEnumLists(ref set.skinColours, constraint.allowed.skinColours);
		}
		if (constraint.allowed.hairs.Count > 0)
		{
			FilterEnumLists(ref set.hairs, constraint.allowed.hairs);
		}
		if (constraint.allowed.hats.Count > 0)
		{
			FilterEnumLists(ref set.hats, constraint.allowed.hats);
		}
		if (constraint.allowed.upperFaces.Count > 0)
		{
			FilterEnumLists(ref set.upperFaces, constraint.allowed.upperFaces);
		}
		if (constraint.allowed.lowerFaces.Count > 0)
		{
			FilterEnumLists(ref set.lowerFaces, constraint.allowed.lowerFaces);
		}
		return set.count > 0;
	}

	private static int FilterEnumLists<T>(ref List<T> values, List<T> toAllow)
	{
		int count = values.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			T item = values[num];
			if (!toAllow.Contains(item))
			{
				values.RemoveAt(num);
			}
		}
		return count - values.Count;
	}

	public static bool ApplyConstraint(ref Customisation details, CustomisationConstraint constraint)
	{
		if (details == null || constraint == null)
		{
			return false;
		}
		bool flag = true;
		if (constraint.allowed.bodyTypes.Count > 0)
		{
			flag &= FilterAppearanceEnum(ref details.body, constraint.allowed.bodyTypes, constraint.fallback.body, 2);
		}
		if (constraint.allowed.skinColours.Count > 0)
		{
			flag &= FilterAppearanceEnum(ref details.skin, constraint.allowed.skinColours, constraint.fallback.skin, 10);
		}
		if (constraint.allowed.hairs.Count > 0)
		{
			flag &= FilterAppearanceEnum(ref details.hair, constraint.allowed.hairs, constraint.fallback.hair, 248);
		}
		if (constraint.allowed.hats.Count > 0)
		{
			flag &= FilterAppearanceEnum(ref details.hat, constraint.allowed.hats, constraint.fallback.hat, 149);
		}
		if (constraint.allowed.upperFaces.Count > 0)
		{
			flag &= FilterAppearanceEnum(ref details.upperFace, constraint.allowed.upperFaces, constraint.fallback.upperFace, 42);
		}
		if (constraint.allowed.lowerFaces.Count > 0)
		{
			flag &= FilterAppearanceEnum(ref details.lowerFace, constraint.allowed.lowerFaces, constraint.fallback.lowerFace, 69);
		}
		return flag;
	}

	public static bool FilterAppearance(ref Customisation details, CustomisationSet allowed, Customisation fallback = null)
	{
		if (details == null || allowed == null)
		{
			return false;
		}
		bool flag = true;
		if (fallback != null)
		{
			flag &= FilterAppearanceEnum(ref details.body, allowed.bodyTypes, fallback.body, 2);
			flag &= FilterAppearanceEnum(ref details.skin, allowed.skinColours, fallback.skin, 10);
			flag &= FilterAppearanceEnum(ref details.hair, allowed.hairs, fallback.hair, 248);
			flag &= FilterAppearanceEnum(ref details.hat, allowed.hats, fallback.hat, 149);
			flag &= FilterAppearanceEnum(ref details.upperFace, allowed.upperFaces, fallback.upperFace, 42);
			return flag & FilterAppearanceEnum(ref details.lowerFace, allowed.lowerFaces, fallback.lowerFace, 69);
		}
		flag &= FilterAppearanceEnum(ref details.body, allowed.bodyTypes, 2);
		flag &= FilterAppearanceEnum(ref details.skin, allowed.skinColours, 10);
		flag &= FilterAppearanceEnum(ref details.hair, allowed.hairs, 248);
		flag &= FilterAppearanceEnum(ref details.hat, allowed.hats, 149);
		flag &= FilterAppearanceEnum(ref details.upperFace, allowed.upperFaces, 42);
		return flag & FilterAppearanceEnum(ref details.lowerFace, allowed.lowerFaces, 69);
	}

	public static bool FilterAppearanceEnum<T>(ref T value, List<T> allowed, int maxValue)
	{
		if (!allowed.Contains(value))
		{
			if (allowed.Count <= 0)
			{
				return false;
			}
			value = allowed[0];
		}
		return true;
	}

	public static bool FilterAppearanceEnum<T>(ref T value, List<T> allowed, T fallback, int maxValue)
	{
		if (!allowed.Contains(value))
		{
			int num = (int)(object)fallback;
			if (num >= 0 && num < maxValue)
			{
				value = fallback;
			}
			else
			{
				if (allowed.Count <= 0)
				{
					return false;
				}
				value = allowed[0];
			}
		}
		return true;
	}

	public static void SetRobinsonCustimations(Customisation customisation)
	{
		if (customisation != null)
		{
			customisation.name = "Robinson";
			customisation.safeName = "Robinson";
			customisation.hair = CustomisationData.Hair.DEBBIE_BROWN;
			customisation.body = CustomisationData.BodyType.MALE;
			customisation.hat = CustomisationData.Hat.NONE;
			customisation.lowerFace = CustomisationData.LowerFaceAccessory.BEARD_BROWN;
			customisation.upperFace = CustomisationData.UpperFaceAccessory.NONE;
			customisation.skin = CustomisationData.SkinColour.WHITE;
		}
	}

	public static void EnforceRobinson(PrisonData prison, Customisation[] customisations)
	{
		if (T17NetManager.IsMasterClient && prison.m_bAddRobinsonCharacter)
		{
			int num = prison.m_CustomisableRoles[0];
			SetRobinsonCustimations(customisations[num - 1]);
		}
	}

	public Customisation[] GetCustomisableNpcsForPrison(PrisonData prison, bool generateIfNotFound = false, bool forceGeneration = false)
	{
		Customisation[] value = null;
		if (prison == null || prison.m_LevelInfo == null)
		{
			return value;
		}
		string associatedFile = prison.m_LevelInfo.m_AssociatedFile;
		if (forceGeneration)
		{
			m_PrisonCustomisations.Remove(associatedFile);
			value = GenerateDefaultCustomisations(prison);
			m_PrisonCustomisations.Add(associatedFile, value);
			RandomiseCustomisations(prison);
			InsertRandomInfluencers(prison);
		}
		else if (!m_PrisonCustomisations.TryGetValue(associatedFile, out value) && generateIfNotFound)
		{
			value = GenerateDefaultCustomisations(prison);
			m_PrisonCustomisations.Add(associatedFile, value);
			RandomiseCustomisations(prison);
			InsertRandomInfluencers(prison);
		}
		return value;
	}

	public void OnPlayerPresetChanged(int slotIndex)
	{
		m_bShouldSavePlayerPresets = true;
		if (slotIndex < m_PlayerPresetsModified.Length || slotIndex > 0)
		{
			m_PlayerPresetsModified[slotIndex] = true;
		}
	}

	public void OnPrisonCustomisationChanged(PrisonData prison)
	{
		m_bShouldSavePrison = true;
		m_PrisonToSave = prison;
	}

	public void OnPlayerChosenSlotChanged(int index)
	{
		m_bShouldSaveChosenPreset = true;
		m_LastChosenPresetIndex = index;
	}

	public bool LoadData()
	{
		m_PlayerPresets = LoadPlayerPresets();
		if (m_PlayerPresets == null || m_PlayerPresets.Length <= 0)
		{
			int num = Mathf.Max(DefaultPlayerPresets.Count, 0);
			m_PlayerPresets = new Customisation[num];
			for (int i = 0; i < num; i++)
			{
				m_PlayerPresets[i] = new Customisation(DefaultPlayerPresets[i]);
			}
		}
		m_PlayerPresetsModified = LoadPlayerPresetModified();
		if (m_PlayerPresetsModified == null || m_PlayerPresetsModified.Length <= 0)
		{
			int num2 = Mathf.Max(DefaultPlayerPresets.Count, 0);
			m_PlayerPresetsModified = new bool[num2];
			for (int j = 0; j < num2; j++)
			{
				m_PlayerPresetsModified[j] = false;
			}
		}
		m_LastChosenPresetIndex = LoadPlayerChosenPreset();
		if (m_LastChosenPresetIndex < 0 || m_LastChosenPresetIndex >= m_PlayerPresets.Length)
		{
			m_LastChosenPresetIndex = 0;
		}
		PrisonData[] t17LevelData = LevelDataManager.GetInstance().GetT17LevelData();
		int value = 0;
		GlobalSave.GetInstance().Get("FLUSH_BLUEPRINT_CUSTOM", out value, 0);
		foreach (PrisonData prisonData in t17LevelData)
		{
			int num3 = 0;
			for (int l = 0; l < prisonData.m_CustomisableRoles.Length; l++)
			{
				num3 += prisonData.m_CustomisableRoles[l];
			}
			Customisation[] array = ((value >= 1) ? LoadPrisonCustomisation(prisonData) : null);
			if (array == null || array.Length <= 0)
			{
				continue;
			}
			if (array.Length == num3)
			{
				string associatedFile = prisonData.m_LevelInfo.m_AssociatedFile;
				if (!m_PrisonCustomisations.ContainsKey(associatedFile))
				{
					m_PrisonCustomisations.Add(associatedFile, array);
				}
				continue;
			}
			Customisation[] array2 = new Customisation[num3];
			for (int m = 0; m < array2.Length; m++)
			{
				if (m >= 0 && m < array.Length)
				{
					array2[m] = array[m];
					continue;
				}
				array2[m] = DefaultCustomisation;
				int num4 = 0;
				int num5 = m;
				for (int n = 0; n < prisonData.m_CustomisableRoles.Length; n++)
				{
					num5 -= prisonData.m_CustomisableRoles[n] - 1;
					if (num5 <= 0)
					{
						num4 = n;
						break;
					}
				}
				CustomisationConfig customisationConfig = prisonData.m_CustomisationPools[num4];
				if (!RandomiseFromPool(ref array2[m], customisationConfig))
				{
					continue;
				}
				if (prisonData.m_CustomisationConstraints.Length > num4 && prisonData.m_CustomisationConstraints[num4] != null)
				{
					ApplyConstraint(ref array2[m], prisonData.m_CustomisationConstraints[num4]);
				}
				List<string> list = new List<string>(customisationConfig.m_NamePool);
				if (customisationConfig.m_NamePool.Count > 0)
				{
					if (list.Count <= 0)
					{
						list.Clear();
						list.AddRange(customisationConfig.m_NamePool);
					}
					int index = Random.Range(0, list.Count);
					array2[m].name = list[index];
					array2[m].safeName = array2[m].name;
					list.RemoveAt(index);
				}
			}
		}
		GlobalSave.GetInstance().Set("FLUSH_BLUEPRINT_CUSTOM", 1);
		return true;
	}

	private bool SavePlayerPresets()
	{
		bool flag = false;
		if (!string.IsNullOrEmpty("Customisations:PlayerPresets"))
		{
			GlobalSave.GetInstance().Set("Customisations:PlayerPresets.Count", m_PlayerPresets.Length);
			string value = CustomisationSerialiser.SerialiseCustomisations_ToJSON(m_PlayerPresets);
			if (!string.IsNullOrEmpty(value))
			{
				GlobalSave.GetInstance().Set("Customisations:PlayerPresets", value);
				GlobalSave.GetInstance().RequestSave();
				flag = true;
			}
		}
		if (!flag)
		{
		}
		return flag;
	}

	private bool SavePlayerPresetModified()
	{
		bool flag = false;
		if (!string.IsNullOrEmpty("Customisations:PlayerPresets"))
		{
			CustomisationModifiedNetData customisationModifiedNetData = new CustomisationModifiedNetData();
			customisationModifiedNetData.isModified = m_PlayerPresetsModified;
			string value = JsonUtility.ToJson(customisationModifiedNetData);
			if (!string.IsNullOrEmpty(value))
			{
				GlobalSave.GetInstance().Set("Customisations:PlayerPresets.Modified", value);
				GlobalSave.GetInstance().RequestSave();
				flag = true;
			}
		}
		if (!flag)
		{
		}
		return flag;
	}

	private bool SavePlayerChosenPreset()
	{
		bool flag = false;
		if (!string.IsNullOrEmpty("Customisations:PlayerPresets"))
		{
			GlobalSave.GetInstance().Set("Customisations:PlayerPresets.LastChosen", m_LastChosenPresetIndex);
			GlobalSave.GetInstance().RequestSave();
			flag = true;
		}
		if (!flag)
		{
		}
		return flag;
	}

	private bool SavePrisonCustomisation(PrisonData prison)
	{
		bool result = false;
		if (prison != null)
		{
			Customisation[] customisableNpcsForPrison = GetCustomisableNpcsForPrison(prison);
			if (prison.m_bAddRobinsonCharacter)
			{
				GlobalStart instance = GlobalStart.GetInstance();
				if (!instance.m_CustomLevel)
				{
					int num = prison.m_CustomisableRoles[0];
					SetRobinsonCustimations(customisableNpcsForPrison[num - 1]);
				}
			}
			string associatedFile = prison.m_LevelInfo.m_AssociatedFile;
			string value = CustomisationSerialiser.SerialiseCustomisations_ToJSON(customisableNpcsForPrison);
			if (!string.IsNullOrEmpty(associatedFile))
			{
				string key = string.Format("{0}:{1}", "Customisations:PrisonSetup", associatedFile);
				GlobalSave.GetInstance().Set(key, value);
				GlobalSave.GetInstance().RequestSave();
				result = true;
			}
		}
		return result;
	}

	private Customisation[] LoadPlayerPresets()
	{
		Customisation[] array = null;
		string value = string.Empty;
		GlobalSave.GetInstance().Get("Customisations:PlayerPresets", out value, string.Empty);
		if (!string.IsNullOrEmpty(value))
		{
			return CustomisationSerialiser.DeserialiseCustomisations_FromJSON(value);
		}
		return new Customisation[0];
	}

	private bool[] LoadPlayerPresetModified()
	{
		bool[] array = null;
		string value = string.Empty;
		GlobalSave.GetInstance().Get("Customisations:PlayerPresets.Modified", out value, string.Empty);
		if (!string.IsNullOrEmpty(value))
		{
			CustomisationModifiedNetData customisationModifiedNetData = JsonUtility.FromJson<CustomisationModifiedNetData>(value);
			return customisationModifiedNetData.isModified;
		}
		return new bool[0];
	}

	private int LoadPlayerChosenPreset()
	{
		int value = -1;
		GlobalSave.GetInstance().Get("Customisations:PlayerPresets.LastChosen", out value, 0);
		return value;
	}

	private Customisation[] LoadPrisonCustomisation(PrisonData prison)
	{
		Customisation[] result = null;
		string associatedFile = prison.m_LevelInfo.m_AssociatedFile;
		string key = string.Format("{0}:{1}", "Customisations:PrisonSetup", associatedFile);
		string value = string.Empty;
		if (GlobalSave.GetInstance().Get(key, out value, string.Empty))
		{
			result = CustomisationSerialiser.DeserialiseCustomisations_FromJSON(value);
		}
		return result;
	}
}
