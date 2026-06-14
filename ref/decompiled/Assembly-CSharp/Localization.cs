using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Rewired;
using Steamworks;
using UnityEngine;

public static class Localization
{
	public enum GameSupportedLanguages
	{
		Unset,
		English,
		German,
		French,
		Spanish,
		Russian,
		Italian,
		Chinese,
		Japanese,
		Korean
	}

	public delegate void LocalizationEvent();

	private class StringVariants
	{
		public short index;

		public List<string[]> strings = new List<string[]>();

		public string[] GetNext()
		{
			if (strings == null || strings.Count == 0)
			{
				return null;
			}
			index--;
			if (index < 0)
			{
				strings.Shuffle();
				index = (short)(strings.Count - 1);
			}
			return strings[index];
		}

		public string[] GetSpecific(int pos)
		{
			if (strings == null || strings.Count == 0)
			{
				return null;
			}
			int num = Mathf.Clamp(pos, 0, strings.Count - 1);
			return strings[num];
		}
	}

	public enum TokenReplaceType
	{
		QuestGiver,
		Player,
		Character,
		TextID,
		Item,
		ItemContainer
	}

	public class TokenInfo
	{
		public TokenReplaceType m_ReplaceType;

		public string m_Token = string.Empty;

		public ObjectiveSceneElement m_SceneReference;

		public string m_TextID = string.Empty;

		[HideInInspector]
		public string m_TextIDTag = string.Empty;
	}

	private static int m_EnglishHashCode = "english".GetHashCode();

	private static int m_GermanHashCode = "german".GetHashCode();

	private static int m_FrenchHashCode = "french".GetHashCode();

	private static int m_SpanishHashCode = "spanish".GetHashCode();

	private static int m_RussianHashCode = "russian".GetHashCode();

	private static int m_ItalianHashCode = "italian".GetHashCode();

	private static int m_ChineseHashCode = "schinese".GetHashCode();

	private static int m_JapaneseHashCode = "japanese".GetHashCode();

	private static int m_KoreanHashCode = "koreana".GetHashCode();

	public static LocalizationEvent OnLanguageChanged = null;

	private static string[] m_Languages = null;

	private static Dictionary<string, string[]> m_StringTable = new Dictionary<string, string[]>();

	private static Dictionary<string, StringVariants> m_StringVariantTable = new Dictionary<string, StringVariants>();

	private static SystemLanguage m_SystemLanguage = SystemLanguage.Unknown;

	private static bool m_bIsLoaded = false;

	private static bool m_bIsLoading = false;

	private static Sprite m_UnknownIconImagePrefab = null;

	private static int m_LastSizeOverride = -1;

	private static int m_LastTranslatedPlayerID = -1;

	private static string m_LastTranslatedInput = null;

	private static string m_LastTranslatedOutput = null;

	private static List<T17Text.ImageData> m_LastImageData = null;

	private static bool m_bLastUsePadIcons = false;

	private static readonly T17Regex s_InputRegex = new T17Regex("[IN=", "]");

	private static readonly T17Regex s_IconRegex = new T17Regex("[TICON=", "]");

	private const string VARIATION_MARKUP = "&";

	private const string OVERWRITE_MARKUP = "*";

	private static int m_LastRandomTextIndex = -1;

	private static GameSupportedLanguages m_LanguageOverride = GameSupportedLanguages.Unset;

	private static string m_KeyOverride = string.Empty;

	public static List<string> EDITOR_MISSING_TAGS = new List<string>();

	public static Dictionary<string, string> EDITOR_MISSING_TAGS_VERBOSE = new Dictionary<string, string>();

	public static int LastRandomTextIndex => m_LastRandomTextIndex;

	public static GameSupportedLanguages LanguageOverride
	{
		get
		{
			return m_LanguageOverride;
		}
		set
		{
			m_LanguageOverride = value;
			if (OnLanguageChanged != null)
			{
				OnLanguageChanged();
			}
		}
	}

	public static void RemoveLanguageOverride()
	{
		LanguageOverride = GameSupportedLanguages.Unset;
	}

	public static void SetKeyOverride(string keyOverride)
	{
		RemoveKeyOverride();
		if (keyOverride != null)
		{
			m_KeyOverride = keyOverride;
		}
	}

	public static void RemoveKeyOverride()
	{
		m_KeyOverride = string.Empty;
	}

	public static bool AreLanguagesLoaded()
	{
		return m_bIsLoaded;
	}

	public static void Init()
	{
		CultureInfo currentCulture = new CultureInfo("en-GB");
		Thread.CurrentThread.CurrentCulture = currentCulture;
	}

	private static int GetPlatformSupportedLanguagesBitmask()
	{
		int num = 127;
		num |= 0x80;
		return num | 0x100;
	}

	public static bool LoadDictionarys(bool reload)
	{
		if (reload)
		{
			m_bIsLoaded = false;
			m_bIsLoading = false;
			m_Languages = null;
			m_StringTable.Clear();
			m_StringVariantTable.Clear();
		}
		if (m_bIsLoaded || m_bIsLoading)
		{
			return true;
		}
		m_bIsLoading = true;
		if (m_UnknownIconImagePrefab == null)
		{
			m_UnknownIconImagePrefab = Resources.Load<Sprite>("Localization/Unknown_Icon");
		}
		AddLanguageFile("Localization/Localization");
		AddLanguageFile("Localization/LocalizationItems");
		AddLanguageFile("Localization/LocalizationSteam");
		AddLanguageFile("Localization/LocalizationSpeech");
		AddLanguageFile("Localization/LocalizationSpeech_CenterPerks");
		AddLanguageFile("Localization/LocalizationSpeech_WildWest");
		AddLanguageFile("Localization/LocalizationSpeech_Transport_Train");
		AddLanguageFile("Localization/LocalizationSpeech_Transport_Plane");
		AddLanguageFile("Localization/LocalizationSpeech_Transport_Boat");
		AddLanguageFile("Localization/LocalizationSpeech_Space");
		AddLanguageFile("Localization/LocalizationSpeech_POW");
		AddLanguageFile("Localization/LocalizationSpeech_OilRig");
		AddLanguageFile("Localization/LocalizationSpeech_Gulag");
		AddLanguageFile("Localization/LocalizationSpeech_Dictatorship");
		AddLanguageFile("Localization/LocalizationSpeech_Area17");
		AddLanguageFile("Localization/LocalizationSpeech_Tutorial");
		AddLanguageFile("Localization/LocalizationQuests");
		AddLanguageFile("Localization/LocalizationTutorial");
		AddLanguageFile("Localization/LocalizationEditor");
		AddLanguageFile("Localization/Localization_DLC02");
		AddLanguageFile("Localization/LocalizationSpeech_DLC02");
		AddLanguageFile("Localization/Localization_DLC03");
		AddLanguageFile("Localization/LocalizationSpeech_DLC03");
		AddLanguageFile("Localization/Localization_DLC04");
		AddLanguageFile("Localization/LocalizationSpeech_DLC04");
		AddLanguageFile("Localization/Localization_DLC05");
		AddLanguageFile("Localization/LocalizationSpeech_DLC05");
		AddLanguageFile("Localization/Localization_DLC06");
		AddLanguageFile("Localization/LocalizationSpeech_DLC06");
		m_bIsLoading = false;
		m_bIsLoaded = true;
		return true;
	}

	private static bool AddLanguageFile(string path, AssetBundle asset_bundle = null)
	{
		TextAsset textAsset = null;
		textAsset = ((!(asset_bundle == null)) ? asset_bundle.LoadAsset<TextAsset>(path) : (Resources.Load(path, typeof(TextAsset)) as TextAsset));
		if (textAsset == null)
		{
			return false;
		}
		LoadCSV(textAsset, GetPlatformSupportedLanguagesBitmask());
		return true;
	}

	private static bool LoadCSV(TextAsset text, int supportedLanguagesBitmask = int.MaxValue)
	{
		CVSReader cVSReader = new CVSReader(text.bytes);
		string[] array = cVSReader.ReadRow();
		if (array.Length < 2)
		{
			return false;
		}
		m_Languages = new string[array.Length - 1];
		for (int i = 0; i < m_Languages.Length; i++)
		{
			m_Languages[i] = array[i + 1];
		}
		string text2 = string.Empty;
		bool flag = true;
		string[] array2 = null;
		do
		{
			array2 = cVSReader.ReadRow();
			if (array2 == null || array2.Length <= 0 || !(array2[0] != string.Empty))
			{
				continue;
			}
			string text3 = array2[0];
			if (supportedLanguagesBitmask != int.MaxValue)
			{
				int j = 1;
				for (int num = array2.Length; j < num; j++)
				{
					if ((supportedLanguagesBitmask & (1 << j)) == 0)
					{
						array2[j] = string.Empty;
					}
				}
			}
			text3 = text3.Trim();
			if (flag)
			{
				flag = false;
				if (text3.StartsWith("OVERRIDE="))
				{
					string text4 = text3.Substring(9, text3.Length - 9);
					if (!string.IsNullOrEmpty(text4))
					{
						text2 = text4;
					}
					continue;
				}
			}
			if (text3.ToLower() != "end")
			{
				for (int k = 1; k < array2.Length; k++)
				{
					if (array2[k] != null && array2[k].Length > 2)
					{
						array2[k] = array2[k].Replace("\"", "\b");
						array2[k] = array2[k].Replace("\b\b", "\"");
						array2[k] = array2[k].Replace("\b", string.Empty);
						array2[k] = array2[k].Replace('\u00a0', ' ');
					}
				}
				bool flag2 = text3.Substring(0, "&".Length) == "&";
				if (flag2)
				{
					text3 = text3.Substring("&".Length, text3.Length - 1);
				}
				bool flag3 = text3.Substring(0, "*".Length) == "*";
				if (flag3)
				{
					text3 = text3.Substring("*".Length, text3.Length - 1);
				}
				if (!string.IsNullOrEmpty(text2))
				{
					text3 = text3 + "." + text2;
				}
				if (flag2)
				{
					if (m_StringVariantTable.ContainsKey(text3))
					{
						m_StringVariantTable[text3].strings.Add(array2);
						continue;
					}
					m_StringVariantTable.Add(text3, new StringVariants());
					m_StringVariantTable[text3].strings.Add(array2);
				}
				else if (m_StringTable.ContainsKey(text3))
				{
					if (flag3)
					{
						CheckForMarkup(ref array2);
						m_StringTable.Remove(text3);
						m_StringTable.Add(text3, array2);
					}
				}
				else
				{
					CheckForMarkup(ref array2);
					m_StringTable.Add(text3, array2);
				}
			}
			else
			{
				array2 = null;
			}
		}
		while (array2 != null);
		return true;
	}

	private static bool LookUp(string tag, out string localized, int forcedVariation, GameSupportedLanguages language, bool bUsingKeyOverride = false)
	{
		if (string.IsNullOrEmpty(tag) || language == GameSupportedLanguages.Unset)
		{
			localized = string.Empty;
			return false;
		}
		if (tag.Substring(0, "&".Length) == "&")
		{
			tag = tag.Substring("&".Length, tag.Length - 1);
		}
		if (m_StringTable.ContainsKey(tag))
		{
			string[] array = m_StringTable[tag];
			if ((int)language < array.Length)
			{
				localized = array[(int)language];
				return true;
			}
			T17NetManager.LogGoogleException("The tag: " + tag + " was not available in language: " + language);
		}
		else if (m_StringVariantTable.ContainsKey(tag))
		{
			if (forcedVariation == -1)
			{
				string[] next = m_StringVariantTable[tag].GetNext();
				if ((int)language >= next.Length)
				{
					localized = string.Empty;
					T17NetManager.LogGoogleException("The VARIATION tag: " + tag + " was not available in language: " + language);
					return false;
				}
				localized = next[(int)language];
			}
			else
			{
				string[] specific = m_StringVariantTable[tag].GetSpecific(forcedVariation);
				if ((int)language >= specific.Length)
				{
					localized = string.Empty;
					T17NetManager.LogGoogleException("The FORCED VARIATION tag: " + tag + " of variation #" + forcedVariation + " was not available in language: " + language.ToString());
					return false;
				}
				localized = specific[(int)language];
			}
			return true;
		}
		if (bUsingKeyOverride)
		{
			localized = tag;
		}
		else
		{
			localized = "M: " + tag + "  " + language;
		}
		if (!EDITOR_MISSING_TAGS.Contains(tag) && !bUsingKeyOverride)
		{
			EDITOR_MISSING_TAGS.Add(tag);
		}
		if (!EDITOR_MISSING_TAGS_VERBOSE.ContainsKey(tag))
		{
			string empty = string.Empty;
			EDITOR_MISSING_TAGS_VERBOSE.Add(tag, empty);
		}
		return false;
	}

	public static bool Get(string tag, out string localized, out List<TokenInfo> tokens, GameSupportedLanguages localOverride = GameSupportedLanguages.Unset)
	{
		tokens = null;
		localized = string.Empty;
		if (Get(tag, out localized, -1, localOverride))
		{
			string[] array = localized.Split(' ');
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].StartsWith("$"))
				{
					if (tokens == null)
					{
						tokens = new List<TokenInfo>();
					}
					TokenInfo tokenInfo = new TokenInfo();
					tokenInfo.m_Token = array[i];
					tokens.Add(tokenInfo);
				}
			}
			return true;
		}
		return false;
	}

	public static bool GetWithKeySwap<T>(string tag, out string localised, string key, T value, int forcedVariation = -1, GameSupportedLanguages localOverride = GameSupportedLanguages.Unset)
	{
		bool result = Get(tag, out localised, forcedVariation, localOverride);
		if (value != null)
		{
			localised = localised.Replace(key, value.ToString());
		}
		return result;
	}

	public static bool Get(string tag, out string localized, GameSupportedLanguages localOverride = GameSupportedLanguages.Unset)
	{
		return Get(tag, out localized, -1, localOverride);
	}

	public static bool Get(string tag, out string localized, int forcedVariation, GameSupportedLanguages localOverride = GameSupportedLanguages.Unset)
	{
		GameSupportedLanguages language = GetLanguageIndex();
		if (LanguageOverride != 0)
		{
			language = LanguageOverride;
		}
		if (localOverride != 0)
		{
			language = localOverride;
		}
		if (string.IsNullOrEmpty(m_KeyOverride) && LevelScript.GetInstance() != null)
		{
			SetKeyOverride(LevelScript.GetInstance().m_LocalizationKeyOverride);
		}
		if (!string.IsNullOrEmpty(m_KeyOverride) && LookUp(tag + "." + m_KeyOverride, out localized, forcedVariation, language, bUsingKeyOverride: true))
		{
			return true;
		}
		return LookUp(tag, out localized, forcedVariation, language);
	}

	private static void CheckForMarkup(ref string[] trans)
	{
		for (int i = 1; i < trans.Length; i++)
		{
			string text = string.Empty;
			int num = 0;
			bool flag = false;
			int num2 = -1;
			int num3 = -1;
			while (num < trans[i].Length)
			{
				num2 = trans[i].IndexOf('[', num);
				if (num2 > -1)
				{
					text += trans[i].Substring(num, num2 - num);
					num = num2 + 1;
					num3 = trans[i].IndexOf(']', num);
					if (num3 > -1)
					{
						num = num3 + 1;
					}
					continue;
				}
				if (flag)
				{
					text += trans[i].Substring(num, trans[i].Length - num);
				}
				break;
			}
			if (flag)
			{
				trans[i] = text;
			}
		}
	}

	public static bool CheckForMarkup(int playerID, ref string trans, ref List<T17Text.ImageData> imageData, bool bAlwaysUseControllerIcons = false, string PCInputTextColour = "FFFFFFFF", int? imageSizeOverride = null, bool bOverridePrefixSuffix = false, string prefixOverride = "", string suffixOverride = "")
	{
		bool foundAny = false;
		int count = 0;
		Rewired.Player player = null;
		bool flag = true;
		if (ReInput.isReady)
		{
			player = ReInput.players.GetPlayer(playerID);
			if (!bAlwaysUseControllerIcons && player != null && player.controllers != null && player.controllers.GetLastActiveController() != null && player.controllers.GetLastActiveController().type != ControllerType.Joystick)
			{
				flag = false;
			}
		}
		int num = trans.IndexOf('[');
		if (num >= 0)
		{
			if (m_LastTranslatedPlayerID != playerID || m_LastTranslatedInput != trans || m_LastImageData != imageData || (imageSizeOverride.HasValue && imageSizeOverride.Value != m_LastSizeOverride) || m_bLastUsePadIcons != flag)
			{
				m_LastTranslatedPlayerID = playerID;
				m_LastTranslatedInput = trans;
				if (imageSizeOverride.HasValue)
				{
					m_LastSizeOverride = imageSizeOverride.Value;
				}
				else
				{
					m_LastSizeOverride = -1;
				}
				m_bLastUsePadIcons = flag;
				if (imageData != null)
				{
					foreach (T17Text.ImageData imageDatum in imageData)
					{
						imageDatum.NeedsToBeDeleted = true;
					}
				}
				CheckForInputActionMarkup(playerID, ref trans, ref imageData, ref count, ref foundAny, bAlwaysUseControllerIcons, PCInputTextColour, imageSizeOverride, bOverridePrefixSuffix, prefixOverride, suffixOverride);
				CheckForNormalIconMarkup(ref trans, ref imageData, ref count, ref foundAny, imageSizeOverride);
				m_LastTranslatedOutput = trans;
				m_LastImageData = imageData;
			}
			else
			{
				trans = m_LastTranslatedOutput;
				imageData = m_LastImageData;
				foundAny = true;
			}
		}
		else if (imageData != null)
		{
			if (imageData == m_LastImageData)
			{
				m_LastImageData = null;
			}
			foreach (T17Text.ImageData imageDatum2 in imageData)
			{
				imageDatum2.NeedsToBeDeleted = true;
			}
		}
		return foundAny;
	}

	public static void CheckForInputActionMarkup(int playerID, ref string trans, ref List<T17Text.ImageData> imageData, ref int count, ref bool foundAny, bool bAlwaysUseControllerIcons, string PCInputTextColour, int? imageSizeOverride = null, bool bOverridePrefixSuffix = false, string prefixOverride = "", string suffixOverride = "")
	{
		Rewired.Player player = null;
		if (ReInput.isReady)
		{
			player = ReInput.players.GetPlayer(playerID);
		}
		Func<string, Sprite> spriteDelegate = (string inputString) => TextIconManager.GetIconDataForRewiredPlayer(playerID, inputString, imageSizeOverride)?.Sprite;
		int count2;
		T17Regex.StringMatch[] array = s_InputRegex.Matches(trans, out count2);
		for (int i = 0; i < count2; i++)
		{
			if (imageData == null)
			{
				imageData = new List<T17Text.ImageData>();
			}
			foundAny = true;
			if (m_bLastUsePadIcons)
			{
				ProcessMatchedIconString(array[i].value, ref trans, imageData, imageSizeOverride, ref count, spriteDelegate);
			}
			else if (bOverridePrefixSuffix)
			{
				ProcessMatchedKBMString(player, array[i].value, ref trans, ref count, PCInputTextColour, prefixOverride, suffixOverride);
			}
			else
			{
				ProcessMatchedKBMString(player, array[i].value, ref trans, ref count, PCInputTextColour);
			}
		}
	}

	public static void CheckForNormalIconMarkup(ref string trans, ref List<T17Text.ImageData> imageData, ref int count, ref bool foundAny, int? imageSizeOverride = null)
	{
		Func<string, Sprite> spriteDelegate = (string inputString) => T17IconManager.GetSpriteForKey(inputString);
		int count2;
		T17Regex.StringMatch[] array = s_IconRegex.Matches(trans, out count2);
		for (int i = 0; i < count2; i++)
		{
			if (imageData == null)
			{
				imageData = new List<T17Text.ImageData>();
			}
			foundAny = true;
			ProcessMatchedIconString(array[i].value, ref trans, imageData, imageSizeOverride, ref count, spriteDelegate, string.Empty, string.Empty);
		}
	}

	private static void ProcessMatchedIconString(string matchValue, ref string trans, List<T17Text.ImageData> imageData, int? imageSizeOverride, ref int count, Func<string, Sprite> spriteDelegate, string prefx = "  ", string suffix = "  ")
	{
		string text = matchValue.Substring(matchValue.IndexOf('=') + 1);
		text = text.Substring(0, text.Length - 1);
		string markup = text + (count + 1);
		T17Text.ImageData imageData2 = imageData.Find((T17Text.ImageData dt) => dt.m_OriginalMarkup == markup);
		if (imageData2 == null)
		{
			imageData2 = new T17Text.ImageData();
			imageData2.m_OriginalMarkup = markup;
			if (imageSizeOverride.HasValue)
			{
				imageData2.Size = imageSizeOverride.Value;
			}
			imageData.Add(imageData2);
		}
		if (imageSizeOverride.HasValue)
		{
			imageData2.Size = imageSizeOverride.Value;
		}
		imageData2.NeedsToBeDeleted = false;
		if (Application.isPlaying)
		{
			imageData2.IconSprite = spriteDelegate(text);
		}
		else
		{
			imageData2.IconSprite = m_UnknownIconImagePrefab;
		}
		float num = ((!(imageData2.IconSprite == null)) ? (imageData2.IconSprite.rect.width / imageData2.IconSprite.rect.height) : 1f);
		string newValue = prefx + $"<quad input name={text} size={imageData2.Size:d} width={num:f} />" + suffix;
		trans = trans.Replace(matchValue, newValue);
		count++;
	}

	private static void ProcessMatchedKBMString(Rewired.Player player, string matchValue, ref string trans, ref int count, string textColour, string prefix = " [", string suffix = "] ")
	{
		if (player == null || player.controllers == null)
		{
			return;
		}
		string text = matchValue.Substring(matchValue.IndexOf('=') + 1);
		text = text.Substring(0, text.Length - 1);
		string text2 = text;
		bool flag = false;
		Pole axisPole = Pole.Positive;
		if (text2[text2.Length - 1] == '+')
		{
			flag = true;
			axisPole = Pole.Positive;
		}
		else if (text2[text2.Length - 1] == '-')
		{
			flag = true;
			axisPole = Pole.Negative;
		}
		if (flag)
		{
			text2 = text2.Remove(text2.Length - 1);
		}
		List<string> actionInputList = new List<string>();
		IEnumerable<ActionElementMap> mappings = player.controllers.maps.ButtonMapsWithAction(ControllerType.Mouse, text2, skipDisabledMaps: false);
		GetInputsForAction(ref actionInputList, mappings, flag, axisPole);
		mappings = player.controllers.maps.AxisMapsWithAction(ControllerType.Mouse, text2, skipDisabledMaps: false);
		GetInputsForAction(ref actionInputList, mappings, flag, axisPole);
		if (actionInputList.Count == 0)
		{
			mappings = player.controllers.maps.ButtonMapsWithAction(ControllerType.Keyboard, text2, skipDisabledMaps: false);
			GetInputsForAction(ref actionInputList, mappings, flag, axisPole);
			mappings = player.controllers.maps.AxisMapsWithAction(ControllerType.Keyboard, text2, skipDisabledMaps: false);
			GetInputsForAction(ref actionInputList, mappings, flag, axisPole);
		}
		if (actionInputList.Count() > 0)
		{
			string empty = string.Empty;
			empty = empty + "<color=#" + textColour + ">";
			empty += prefix;
			int num = actionInputList.Count();
			for (int i = 0; i < num; i++)
			{
				if (i > 0)
				{
					empty += "/";
				}
				empty += GetPCInputTranslation(actionInputList[i]);
			}
			empty += suffix;
			empty += "</color>";
			trans = trans.Replace(matchValue, empty);
		}
		else
		{
			trans = trans.Replace(matchValue, string.Empty);
		}
	}

	private static void GetInputsForAction(ref List<string> actionInputList, IEnumerable<ActionElementMap> mappings, bool bIsAxisPole, Pole axisPole)
	{
		int num = mappings.Count();
		for (int i = 0; i < num; i++)
		{
			ActionElementMap actionElementMap = mappings.ElementAt(i);
			if ((!bIsAxisPole || actionElementMap.axisContribution == axisPole) && !actionInputList.Contains(actionElementMap.elementIdentifierName))
			{
				actionInputList.Add(actionElementMap.elementIdentifierName);
				break;
			}
		}
	}

	public static string GetPCInputTranslation(string inputName)
	{
		string localized = string.Empty;
		string tag = ("Text.RewiredInput." + inputName).Replace(" ", string.Empty);
		if (Get(tag, out localized))
		{
			return localized;
		}
		return inputName;
	}

	public static GameSupportedLanguages GetLanguageIndex()
	{
		GameSupportedLanguages result = GameSupportedLanguages.English;
		int platformSupportedLanguagesBitmask = GetPlatformSupportedLanguagesBitmask();
		int num = -1;
		Platform instance = Platform.GetInstance();
		if (Application.isPlaying && instance != null && instance.IsInitialized)
		{
			num = SteamApps.GetCurrentGameLanguage().GetHashCode();
		}
		if (num == -1 || num == m_EnglishHashCode)
		{
			result = GameSupportedLanguages.English;
		}
		else if (num == m_GermanHashCode)
		{
			result = GameSupportedLanguages.German;
		}
		else if (num == m_FrenchHashCode)
		{
			result = GameSupportedLanguages.French;
		}
		else if (num == m_SpanishHashCode)
		{
			result = GameSupportedLanguages.Spanish;
		}
		else if (num == m_RussianHashCode)
		{
			result = GameSupportedLanguages.Russian;
		}
		else if (num == m_ItalianHashCode)
		{
			result = GameSupportedLanguages.Italian;
		}
		else if (num == m_ChineseHashCode)
		{
			result = GameSupportedLanguages.Chinese;
		}
		else if (num == m_JapaneseHashCode)
		{
			result = GameSupportedLanguages.Japanese;
		}
		else if (num == m_KoreanHashCode && IsLanguageSupportedInLanguageMask(GameSupportedLanguages.Korean, platformSupportedLanguagesBitmask))
		{
			result = GameSupportedLanguages.Korean;
		}
		return result;
	}

	private static bool IsLanguageSupportedInLanguageMask(GameSupportedLanguages language, int languageMask)
	{
		int num = 1 << (int)language;
		return (num & languageMask) == num;
	}

	public static string[] SplitStringWithPunctuation(string stringToSplit)
	{
		return stringToSplit.Split(' ', '\u00a0', '\n', '!', ',', '?', '\ufffd', '\ufffd', '.', '(', ')', ';', ':', '-', '\'');
	}

	public static string RemovePunctuationFromToken(string stringToClear)
	{
		return Regex.Replace(stringToClear, "[^$'\\s\\w]", string.Empty);
	}
}
