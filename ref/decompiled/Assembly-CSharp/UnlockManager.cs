using System;
using System.Collections.Generic;
using UnityEngine;

public class UnlockManager : T17MonoBehaviour
{
	[Serializable]
	public class TimedUnlockConditions
	{
		public int previousUnlocks;

		public int requiredMinutesInGame;
	}

	public delegate void OnCustomisationSetUnlocked(CustomisationSet set);

	[Serializable]
	private class UnlockSaveData
	{
		public int[] bodys;

		public int[] skins;

		public int[] hairs;

		public int[] hats;

		public int[] upperFaces;

		public int[] lowerFaces;
	}

	private const string UNLOCKS_SAVE_ID = "Progress:Unlocks";

	private const string NEW_UNLOCKS_SAVE_ID = "Progress:NewUnlocks";

	[Header("Initally Unlocked")]
	public CustomisationSet m_StartingCustomisations = new CustomisationSet();

	public CustomisationSet m_InfluencerCustomisations = new CustomisationSet();

	[Header("Timed Unlocks")]
	public CustomisationSet m_TimedUnlocks = new CustomisationSet();

	public List<TimedUnlockConditions> m_TimedUnlockConditions = new List<TimedUnlockConditions>();

	private CustomisationSet m_Unlocked = new CustomisationSet();

	private CustomisationSet m_NewUnlocks = new CustomisationSet();

	private float m_TimeUntilNextUnlock;

	private bool m_bShouldSaveUnlocks;

	private bool m_bShouldSaveNewUnlocks;

	private static UnlockManager m_Instance;

	public event OnCustomisationSetUnlocked onSetUnlocked;

	public static UnlockManager GetInstance()
	{
		return m_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		m_Instance = this;
		Initialise();
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	private void Initialise()
	{
		UnlockCustomisationSet(m_StartingCustomisations, markAsNew: false, saveChanges: false);
		UnlockCustomisationSet(m_InfluencerCustomisations, markAsNew: false, saveChanges: false);
		m_TimeUntilNextUnlock = GetTimeUntilNextUnlock(0);
	}

	private void Update()
	{
		UpdateTimedUnlocks();
		if (m_bShouldSaveUnlocks)
		{
			m_bShouldSaveUnlocks = false;
			SaveUnlockData("Progress:Unlocks", m_Unlocked);
		}
		if (m_bShouldSaveNewUnlocks)
		{
			m_bShouldSaveNewUnlocks = false;
			SaveUnlockData("Progress:NewUnlocks", m_NewUnlocks);
		}
	}

	private void UpdateTimedUnlocks()
	{
		if (m_TimeUntilNextUnlock <= 0f)
		{
			return;
		}
		bool flag = false;
		GlobalStart instance = GlobalStart.GetInstance();
		if (instance != null && instance.IsWithinLevel())
		{
			Gamer primaryGamer = Gamer.GetPrimaryGamer();
			if (primaryGamer != null && primaryGamer.m_eCharacterSelectionStage >= Gamer.CharacterSelectionStage.EnabledInGame && !CutsceneManagerBase.IsACutscenePlaying())
			{
				Player playerObject = primaryGamer.m_PlayerObject;
				if (playerObject != null && !playerObject.IsBrowsingPauseMenu)
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			m_TimeUntilNextUnlock -= Time.unscaledDeltaTime;
			if (m_TimeUntilNextUnlock <= 0f)
			{
				m_TimeUntilNextUnlock = 0f;
				OnNextUnlockTimeReached();
			}
		}
	}

	private void OnNextUnlockTimeReached()
	{
		int remainingUnlocks = 0;
		if (!UnlockRandomFromCustomisationSet(m_TimedUnlocks, out remainingUnlocks))
		{
			return;
		}
		if (remainingUnlocks > 0)
		{
			int previouslyUnlocked = m_TimedUnlocks.count - remainingUnlocks;
			m_TimeUntilNextUnlock = GetTimeUntilNextUnlock(previouslyUnlocked);
		}
		GlobalStart instance = GlobalStart.GetInstance();
		if (instance != null && instance.IsWithinLevel())
		{
			ChatFeedManager instance2 = ChatFeedManager.GetInstance();
			Gamer primaryGamer = Gamer.GetPrimaryGamer();
			if (instance2 != null && primaryGamer != null)
			{
				instance2.DisplayMessageToUser(primaryGamer, "Text.ChatFeed.TimedUnlock", ChatFeedManager.MessageTag.Unlock, bLocalize: true);
			}
		}
		StatSystem.GetInstance().IncStat(32, 1f, Gamer.GetPrimaryGamer(), string.Empty);
	}

	public bool UnlockRandomFromCustomisationSet(CustomisationSet set, out int remainingUnlocks, bool markAsNew = true, bool saveChanges = true)
	{
		bool flag = false;
		int num = 0;
		CustomisationSet customisationSet = new CustomisationSet();
		num += DiffEnumLists(m_Unlocked.bodyTypes, set.bodyTypes, ref customisationSet.bodyTypes);
		num += DiffEnumLists(m_Unlocked.skinColours, set.skinColours, ref customisationSet.skinColours);
		num += DiffEnumLists(m_Unlocked.hairs, set.hairs, ref customisationSet.hairs);
		num += DiffEnumLists(m_Unlocked.hats, set.hats, ref customisationSet.hats);
		num += DiffEnumLists(m_Unlocked.upperFaces, set.upperFaces, ref customisationSet.upperFaces);
		num += DiffEnumLists(m_Unlocked.lowerFaces, set.lowerFaces, ref customisationSet.lowerFaces);
		if (num > 0)
		{
			CustomisationSet customisationSet2 = new CustomisationSet();
			int num2 = UnityEngine.Random.Range(0, num);
			if (num2 >= 0)
			{
				num2 -= customisationSet.bodyTypes.Count;
				if (num2 < 0)
				{
					int index = num2 + customisationSet.bodyTypes.Count;
					customisationSet2.bodyTypes.Add(customisationSet.bodyTypes[index]);
				}
			}
			if (num2 >= 0)
			{
				num2 -= customisationSet.skinColours.Count;
				if (num2 < 0)
				{
					int index2 = num2 + customisationSet.skinColours.Count;
					customisationSet2.skinColours.Add(customisationSet.skinColours[index2]);
				}
			}
			if (num2 >= 0)
			{
				num2 -= customisationSet.hairs.Count;
				if (num2 < 0)
				{
					int index3 = num2 + customisationSet.hairs.Count;
					customisationSet2.hairs.Add(customisationSet.hairs[index3]);
				}
			}
			if (num2 >= 0)
			{
				num2 -= customisationSet.hats.Count;
				if (num2 < 0)
				{
					int index4 = num2 + customisationSet.hats.Count;
					customisationSet2.hats.Add(customisationSet.hats[index4]);
				}
			}
			if (num2 >= 0)
			{
				num2 -= customisationSet.upperFaces.Count;
				if (num2 < 0)
				{
					int index5 = num2 + customisationSet.upperFaces.Count;
					customisationSet2.upperFaces.Add(customisationSet.upperFaces[index5]);
				}
			}
			if (num2 >= 0)
			{
				num2 -= customisationSet.lowerFaces.Count;
				if (num2 < 0)
				{
					int index6 = num2 + customisationSet.lowerFaces.Count;
					customisationSet2.lowerFaces.Add(customisationSet.lowerFaces[index6]);
				}
			}
			flag = UnlockCustomisationSet(customisationSet2, markAsNew, saveChanges);
			remainingUnlocks = num;
			if (flag)
			{
				remainingUnlocks--;
			}
		}
		else
		{
			remainingUnlocks = 0;
		}
		return flag;
	}

	public bool UnlockCustomisationSet(CustomisationSet set, bool markAsNew = true, bool saveChanges = true)
	{
		if (markAsNew)
		{
			CustomisationSet customisationSet = new CustomisationSet();
			int num = 0;
			num += DiffEnumLists(m_Unlocked.bodyTypes, set.bodyTypes, ref customisationSet.bodyTypes);
			num += DiffEnumLists(m_Unlocked.skinColours, set.skinColours, ref customisationSet.skinColours);
			num += DiffEnumLists(m_Unlocked.hairs, set.hairs, ref customisationSet.hairs);
			num += DiffEnumLists(m_Unlocked.hats, set.hats, ref customisationSet.hats);
			num += DiffEnumLists(m_Unlocked.upperFaces, set.upperFaces, ref customisationSet.upperFaces);
			num += DiffEnumLists(m_Unlocked.lowerFaces, set.lowerFaces, ref customisationSet.lowerFaces);
			if (num > 0)
			{
				int num2 = 0;
				num2 += MergeEnumLists(ref m_NewUnlocks.bodyTypes, customisationSet.bodyTypes);
				num2 += MergeEnumLists(ref m_NewUnlocks.skinColours, customisationSet.skinColours);
				num2 += MergeEnumLists(ref m_NewUnlocks.hairs, customisationSet.hairs);
				num2 += MergeEnumLists(ref m_NewUnlocks.hats, customisationSet.hats);
				num2 += MergeEnumLists(ref m_NewUnlocks.upperFaces, customisationSet.upperFaces);
				num2 += MergeEnumLists(ref m_NewUnlocks.lowerFaces, customisationSet.lowerFaces);
				if (num2 > 0 && saveChanges)
				{
					m_bShouldSaveNewUnlocks = true;
				}
				if (this.onSetUnlocked != null)
				{
					this.onSetUnlocked(customisationSet);
				}
			}
		}
		int num3 = 0;
		num3 += MergeEnumLists(ref m_Unlocked.bodyTypes, set.bodyTypes);
		num3 += MergeEnumLists(ref m_Unlocked.skinColours, set.skinColours);
		num3 += MergeEnumLists(ref m_Unlocked.hairs, set.hairs);
		num3 += MergeEnumLists(ref m_Unlocked.hats, set.hats);
		num3 += MergeEnumLists(ref m_Unlocked.upperFaces, set.upperFaces);
		num3 += MergeEnumLists(ref m_Unlocked.lowerFaces, set.lowerFaces);
		bool flag = num3 > 0;
		if (flag && saveChanges)
		{
			m_bShouldSaveUnlocks = true;
		}
		return flag;
	}

	public bool MarkCustomisationSetAsSeen(CustomisationSet set, bool saveChanges = true)
	{
		int num = 0;
		num += RemoveEnumLists(ref m_NewUnlocks.bodyTypes, set.bodyTypes);
		num += RemoveEnumLists(ref m_NewUnlocks.skinColours, set.skinColours);
		num += RemoveEnumLists(ref m_NewUnlocks.hairs, set.hairs);
		num += RemoveEnumLists(ref m_NewUnlocks.hats, set.hats);
		num += RemoveEnumLists(ref m_NewUnlocks.upperFaces, set.upperFaces);
		num += RemoveEnumLists(ref m_NewUnlocks.lowerFaces, set.lowerFaces);
		bool flag = num > 0;
		if (flag && saveChanges)
		{
			m_bShouldSaveNewUnlocks = true;
		}
		return flag;
	}

	public bool CategoriseSet(CustomisationSet set, ref CustomisationCategorisedSet categorised)
	{
		if (categorised == null)
		{
			return false;
		}
		AddSetToCategory(set, ref categorised, UnlockCategories.Influencer, m_InfluencerCustomisations);
		int count = categorised.sets.Count;
		if (count > 0)
		{
			categorised.bodyTypes = new List<CustomisationData.BodyType>[count];
			for (int i = 0; i < count; i++)
			{
				categorised.bodyTypes[i] = categorised.sets[i].bodyTypes;
			}
			categorised.skinColours = new List<CustomisationData.SkinColour>[count];
			for (int j = 0; j < count; j++)
			{
				categorised.skinColours[j] = categorised.sets[j].skinColours;
			}
			categorised.hairs = new List<CustomisationData.Hair>[count];
			for (int k = 0; k < count; k++)
			{
				categorised.hairs[k] = categorised.sets[k].hairs;
			}
			categorised.hats = new List<CustomisationData.Hat>[count];
			for (int l = 0; l < count; l++)
			{
				categorised.hats[l] = categorised.sets[l].hats;
			}
			categorised.upperFaces = new List<CustomisationData.UpperFaceAccessory>[count];
			for (int m = 0; m < count; m++)
			{
				categorised.upperFaces[m] = categorised.sets[m].upperFaces;
			}
			categorised.lowerFaces = new List<CustomisationData.LowerFaceAccessory>[count];
			for (int n = 0; n < count; n++)
			{
				categorised.lowerFaces[n] = categorised.sets[n].lowerFaces;
			}
		}
		else
		{
			categorised.bodyTypes = new List<CustomisationData.BodyType>[0];
			categorised.skinColours = new List<CustomisationData.SkinColour>[0];
			categorised.hairs = new List<CustomisationData.Hair>[0];
			categorised.hats = new List<CustomisationData.Hat>[0];
			categorised.upperFaces = new List<CustomisationData.UpperFaceAccessory>[0];
			categorised.lowerFaces = new List<CustomisationData.LowerFaceAccessory>[0];
		}
		return count > 0;
	}

	private bool AddSetToCategory(CustomisationSet toCategorise, ref CustomisationCategorisedSet output, UnlockCategories category, CustomisationSet toAllow)
	{
		CustomisationSet customisationSet = new CustomisationSet();
		customisationSet.CopyData(toCategorise);
		int num = 0;
		num += FilterEnumLists(ref customisationSet.bodyTypes, toAllow.bodyTypes);
		num += FilterEnumLists(ref customisationSet.skinColours, toAllow.skinColours);
		num += FilterEnumLists(ref customisationSet.hairs, toAllow.hairs);
		num += FilterEnumLists(ref customisationSet.hats, toAllow.hats);
		num += FilterEnumLists(ref customisationSet.upperFaces, toAllow.upperFaces);
		num += FilterEnumLists(ref customisationSet.lowerFaces, toAllow.lowerFaces);
		if (num > 0)
		{
			output.categories.Add(category);
			output.sets.Add(customisationSet);
		}
		return num > 0;
	}

	private static int MergeEnumLists<T>(ref List<T> values, List<T> toAdd)
	{
		int num = 0;
		int count = toAdd.Count;
		for (int i = 0; i < count; i++)
		{
			T item = toAdd[i];
			if (!values.Contains(item))
			{
				values.Add(item);
				num++;
			}
		}
		if (num > 0)
		{
			values.Sort();
		}
		return num;
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

	private static int RemoveEnumLists<T>(ref List<T> values, List<T> toRemove)
	{
		int count = values.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			T item = values[num];
			if (toRemove.Contains(item))
			{
				values.RemoveAt(num);
			}
		}
		return count - values.Count;
	}

	private static int DiffEnumLists<T>(List<T> values, List<T> toCompare, ref List<T> differences)
	{
		differences.Clear();
		int count = toCompare.Count;
		for (int i = 0; i < count; i++)
		{
			T item = toCompare[i];
			if (!values.Contains(item))
			{
				differences.Add(item);
			}
		}
		return differences.Count;
	}

	private float GetTimeUntilNextUnlock(int previouslyUnlocked)
	{
		if (m_TimedUnlockConditions == null || m_TimedUnlockConditions.Count == 0)
		{
			return 0f;
		}
		int num = 0;
		for (int i = 1; i < m_TimedUnlockConditions.Count && previouslyUnlocked >= m_TimedUnlockConditions[i].previousUnlocks; i++)
		{
			num++;
		}
		float result = 0f;
		if (num >= 0 && num < m_TimedUnlockConditions.Count)
		{
			int requiredMinutesInGame = m_TimedUnlockConditions[num].requiredMinutesInGame;
			result = requiredMinutesInGame * 60;
		}
		return result;
	}

	public CustomisationSet GetAllUnlockedCustomisations()
	{
		return m_Unlocked;
	}

	public CustomisationSet GetNewCustomisations()
	{
		return m_NewUnlocks;
	}

	public bool LoadData()
	{
		CustomisationSet customisationSet = LoadUnlockData("Progress:NewUnlocks");
		if (customisationSet.count > 0)
		{
			UnlockCustomisationSet(customisationSet, markAsNew: true, saveChanges: false);
		}
		CustomisationSet customisationSet2 = LoadUnlockData("Progress:Unlocks");
		if (customisationSet2.count > 0)
		{
			UnlockCustomisationSet(customisationSet2, markAsNew: false, saveChanges: false);
		}
		return true;
	}

	private bool SaveUnlockData(string id, CustomisationSet toSave)
	{
		bool flag = false;
		if (!string.IsNullOrEmpty(id))
		{
			UnlockSaveData unlockSaveData = new UnlockSaveData();
			int count = toSave.bodyTypes.Count;
			unlockSaveData.bodys = SaveEnumLists(toSave.bodyTypes, 2);
			unlockSaveData.skins = SaveEnumLists(toSave.skinColours, 10);
			unlockSaveData.hairs = SaveEnumLists(toSave.hairs, 248);
			unlockSaveData.hats = SaveEnumLists(toSave.hats, 149);
			unlockSaveData.upperFaces = SaveEnumLists(toSave.upperFaces, 42);
			unlockSaveData.lowerFaces = SaveEnumLists(toSave.lowerFaces, 69);
			string value = JsonUtility.ToJson(unlockSaveData);
			if (!string.IsNullOrEmpty(value))
			{
				GlobalSave.GetInstance().Set(id, value);
				GlobalSave.GetInstance().RequestSave();
				flag = true;
			}
		}
		if (!flag)
		{
		}
		return flag;
	}

	private CustomisationSet LoadUnlockData(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return new CustomisationSet();
		}
		CustomisationSet customisationSet = null;
		string value = string.Empty;
		GlobalSave.GetInstance().Get(id, out value, string.Empty);
		if (!string.IsNullOrEmpty(value))
		{
			UnlockSaveData unlockSaveData = JsonUtility.FromJson<UnlockSaveData>(value);
			if (unlockSaveData != null)
			{
				customisationSet = new CustomisationSet();
				if (unlockSaveData.bodys != null && unlockSaveData.bodys.Length > 0)
				{
					customisationSet.bodyTypes = LoadEnumLists<CustomisationData.BodyType>(unlockSaveData.bodys, 2);
				}
				if (unlockSaveData.skins != null && unlockSaveData.skins.Length > 0)
				{
					customisationSet.skinColours = LoadEnumLists<CustomisationData.SkinColour>(unlockSaveData.skins, 10);
				}
				if (unlockSaveData.hairs != null && unlockSaveData.hairs.Length > 0)
				{
					customisationSet.hairs = LoadEnumLists<CustomisationData.Hair>(unlockSaveData.hairs, 248);
				}
				if (unlockSaveData.hats != null && unlockSaveData.hats.Length > 0)
				{
					customisationSet.hats = LoadEnumLists<CustomisationData.Hat>(unlockSaveData.hats, 149);
				}
				if (unlockSaveData.upperFaces != null && unlockSaveData.upperFaces.Length > 0)
				{
					customisationSet.upperFaces = LoadEnumLists<CustomisationData.UpperFaceAccessory>(unlockSaveData.upperFaces, 42);
				}
				if (unlockSaveData.lowerFaces != null && unlockSaveData.lowerFaces.Length > 0)
				{
					customisationSet.lowerFaces = LoadEnumLists<CustomisationData.LowerFaceAccessory>(unlockSaveData.lowerFaces, 69);
				}
				if (customisationSet.count > 0)
				{
				}
			}
		}
		else
		{
			customisationSet = new CustomisationSet();
		}
		return customisationSet;
	}

	private static int[] SaveEnumLists<T>(List<T> values, int maxValue)
	{
		int count = values.Count;
		int[] array = new int[count];
		if (count > 0)
		{
			for (int i = 0; i < count; i++)
			{
				T val = values[i];
				int num = (int)(object)val;
				if (num >= 0 && num < maxValue)
				{
					array[i] = num;
				}
				else
				{
					array[i] = -1;
				}
			}
		}
		return array;
	}

	private static List<T> LoadEnumLists<T>(int[] data, int maxValue)
	{
		List<T> list = new List<T>();
		if (data != null && data.Length > 0)
		{
			for (int i = 0; i < data.Length; i++)
			{
				int num = data[i];
				if (num >= 0 && num < maxValue)
				{
					T item = (T)(object)data[i];
					list.Add(item);
				}
			}
		}
		return list;
	}
}
