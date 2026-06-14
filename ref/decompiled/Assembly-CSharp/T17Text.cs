using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
[AddComponentMenu("T17_UI/T17Text", 29)]
public class T17Text : Text, IT17EventHelper
{
	[Serializable]
	public struct PlatformOverride
	{
		public Platform.PlatformOverride PlatformToOverride;

		public int OverrideFontSize;

		public int OverrideFontMinSize;

		public int OverrideFontMaxSize;
	}

	[Serializable]
	public class ImageData
	{
		[ReadOnly]
		[SerializeField]
		public string m_OriginalMarkup;

		[SerializeField]
		[Tooltip("This can be changed in editor BUT will be changed to the correct image at runtime!")]
		public Sprite IconSprite;

		[SerializeField]
		[ReadOnly]
		public Vector2 Position = Vector2.zero;

		[SerializeField]
		[ReadOnly]
		public bool OutOfBounds;

		[SerializeField]
		public Vector2 Offset = Vector2.zero;

		[SerializeField]
		public Vector2 AlignOffset = new Vector2(0f, 0.2f);

		[SerializeField]
		public int Size = 34;

		[SerializeField]
		[HideInInspector]
		public T17Image ImageObj;

		[SerializeField]
		[HideInInspector]
		public int OriginalQuadIndex;

		[SerializeField]
		[HideInInspector]
		public int PlaceInString;

		[SerializeField]
		[HideInInspector]
		public bool NeedsToBeDeleted;
	}

	private static readonly T17Regex s_InputQuadRegex = new T17Regex("<quad input name=", " />");

	private static Font s_PermanentFontOverride = null;

	public bool m_bNeedsLocalization = true;

	public string m_LocalizationTag = "XXX.YYY.ZZZ";

	public string m_PlaceholderText = string.Empty;

	public string m_NonLocalizedPCText = string.Empty;

	public string m_PCTextInputPrefix = string.Empty;

	public string m_PCTextInputSuffix = string.Empty;

	public bool m_bUseUserDefinedPrefixSuffix;

	public static string m_sPCInputTextColorHex = string.Empty;

	public Color32 m_PCInputTextOverrideColour = new Color32(0, 0, 0, byte.MaxValue);

	public bool m_bUseOverrideColour;

	private string m_PCInputTextColorHexOverride = string.Empty;

	public RootMenu.FontTypes m_FontType = RootMenu.FontTypes.NormalText;

	public bool m_bTagFound;

	public bool m_OutputColour;

	public Func<bool> m_ReleaseOnPointerClickDelegate;

	private Func<int> m_GetRewiredIdDelegate;

	private Vector2 m_PreviousSize = new Vector2(-1f, -1f);

	private bool m_bResizingText;

	private bool m_bTextHasChanged;

	[Header("Character")]
	public bool m_bForceRichText;

	private int m_EventSystemsRewiredId;

	private bool m_bHasIcons;

	private bool m_bFromOnValidate;

	private bool m_bFromOnPopulateMesh;

	private bool m_bSetVerticesDirty;

	private bool m_bOnPopulateMesh;

	private float m_CachedAlpha = -1f;

	private Font m_BackupFont;

	[SerializeField]
	public List<ImageData> m_ImagesAttached;

	public List<T17Text> m_LinkedText = new List<T17Text>();

	private string m_MarkedUpString = string.Empty;

	private static GameObject m_IconImagePrefab = null;

	private Vector3[] m_SelfWorldCorners = new Vector3[4];

	private Vector3[] m_IconWorldCorners = new Vector3[4];

	public bool m_UsePCKeySizeOverride;

	public int m_PCKeySizeOverride;

	private int m_DefaultFontSize = -1;

	private bool m_bCurrentSizePCKey;

	public bool m_bAlwaysUseControllerIcons;

	public int m_PCIconOverrideSize = -1;

	public int m_CachedfontSizeUsedForBestFit;

	[SerializeField]
	[Tooltip("Set to test an override for a different platform to what the editor is set to. If set to unknown then will use the platform the editor is set to. Only works when running in the editor")]
	public List<PlatformOverride> m_PlatformOverrides;

	public Platform.PlatformOverride m_TestPlatformOverride;

	public override string text
	{
		set
		{
			if (value != base.text)
			{
				base.text = value;
				m_bTextHasChanged = true;
			}
		}
	}

	public bool HasIcons => m_bHasIcons;

	public override Color color
	{
		get
		{
			return base.color;
		}
		set
		{
			base.color = value;
			if (!m_bHasIcons)
			{
				return;
			}
			for (int i = 0; i < m_ImagesAttached.Count; i++)
			{
				if (m_ImagesAttached[i] != null && m_ImagesAttached[i].ImageObj != null && !m_ImagesAttached[i].NeedsToBeDeleted)
				{
					m_CachedAlpha = value.a;
					Color color = m_ImagesAttached[i].ImageObj.color;
					color.a = m_CachedAlpha;
					m_ImagesAttached[i].ImageObj.color = color;
				}
			}
		}
	}

	public override void CrossFadeAlpha(float alpha, float duration, bool ignoreTimeScale)
	{
		base.CrossFadeAlpha(alpha, duration, ignoreTimeScale);
		for (int num = m_LinkedText.Count - 1; num >= 0; num--)
		{
			if (m_LinkedText[num] != null && m_LinkedText[num].m_LinkedText.Count == 0)
			{
				m_LinkedText[num].CrossFadeAlpha(alpha, duration, ignoreTimeScale);
			}
		}
	}

	public override void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha, bool useRGB)
	{
		base.CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha, useRGB);
		for (int num = m_LinkedText.Count - 1; num >= 0; num--)
		{
			if (m_LinkedText[num] != null && m_LinkedText[num].m_LinkedText.Count == 0)
			{
				m_LinkedText[num].CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha, useRGB);
			}
		}
	}

	public override void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha)
	{
		base.CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha);
		for (int num = m_LinkedText.Count - 1; num >= 0; num--)
		{
			if (m_LinkedText[num] != null && m_LinkedText[num].m_LinkedText.Count == 0)
			{
				m_LinkedText[num].CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha);
			}
		}
	}

	public void SetBackupFont(Font font)
	{
		m_BackupFont = font;
	}

	public static void SetPermanentFontOverride(Font font)
	{
		s_PermanentFontOverride = font;
	}

	protected override void OnEnable()
	{
		if (s_PermanentFontOverride != null && s_PermanentFontOverride != base.font)
		{
			base.font = s_PermanentFontOverride;
		}
		if (m_BackupFont == null)
		{
			m_BackupFont = base.font;
		}
		if (m_IconImagePrefab == null)
		{
			m_IconImagePrefab = Resources.Load("T17_UIElements/T17Image") as GameObject;
		}
		base.OnEnable();
		Localization.OnLanguageChanged = (Localization.LocalizationEvent)Delegate.Remove(Localization.OnLanguageChanged, new Localization.LocalizationEvent(LanguageChanged));
		Localization.OnLanguageChanged = (Localization.LocalizationEvent)Delegate.Combine(Localization.OnLanguageChanged, new Localization.LocalizationEvent(LanguageChanged));
		SetPlatformOverrides();
		LanguageChanged();
		if (m_bHasIcons && !T17RewiredStandaloneInputModule.m_TextToRefresh.Contains(this))
		{
			T17RewiredStandaloneInputModule.m_TextToRefresh.Add(this);
			CheckMarkup();
		}
	}

	protected override void OnDisable()
	{
		T17RewiredStandaloneInputModule.m_TextToRefresh.Remove(this);
		base.OnDisable();
		Localization.OnLanguageChanged = (Localization.LocalizationEvent)Delegate.Remove(Localization.OnLanguageChanged, new Localization.LocalizationEvent(LanguageChanged));
	}

	protected override void Awake()
	{
		if (s_PermanentFontOverride != null)
		{
			base.font = s_PermanentFontOverride;
		}
	}

	protected override void Start()
	{
		if (s_PermanentFontOverride != null && s_PermanentFontOverride != base.font)
		{
			base.font = s_PermanentFontOverride;
		}
		if (base.resizeTextForBestFit && base.font != null && !base.font.dynamic)
		{
			DoBestFit();
		}
		base.Start();
		if (m_bForceRichText)
		{
			base.supportRichText = true;
		}
		if (m_bUseOverrideColour)
		{
			m_PCInputTextColorHexOverride = ColorUtility.ToHtmlStringRGBA(m_PCInputTextOverrideColour);
		}
		SetPlatformOverrides();
		if (m_bNeedsLocalization)
		{
			Convert();
		}
		else if (!m_bTagFound && !string.IsNullOrEmpty(m_NonLocalizedPCText) && ShouldUsePCText())
		{
			CheckMarkup(m_NonLocalizedPCText);
		}
		else
		{
			CheckMarkup(text);
		}
		if (!m_bOnPopulateMesh)
		{
			SetVerticesDirty();
		}
	}

	protected override void OnDestroy()
	{
		T17RewiredStandaloneInputModule.m_TextToRefresh.Remove(this);
		m_ImagesAttached.Clear();
		m_ImagesAttached = null;
		m_LinkedText.Clear();
		base.OnDestroy();
	}

	private void Update()
	{
		if (m_bSetVerticesDirty || m_bOnPopulateMesh)
		{
			m_bSetVerticesDirty = false;
			m_bOnPopulateMesh = false;
			if (m_bHasIcons)
			{
				UpdateQuadImage();
			}
		}
		if (m_bHasIcons)
		{
			if (m_CachedAlpha != this.color.a)
			{
				m_CachedAlpha = this.color.a;
				for (int i = 0; i < m_ImagesAttached.Count; i++)
				{
					if (m_ImagesAttached[i] != null && m_ImagesAttached[i].ImageObj != null && !m_ImagesAttached[i].NeedsToBeDeleted)
					{
						Color color = m_ImagesAttached[i].ImageObj.color;
						color.a = m_CachedAlpha;
						m_ImagesAttached[i].ImageObj.color = color;
					}
				}
			}
			if (base.resizeTextForBestFit && m_CachedfontSizeUsedForBestFit != base.resizeTextMaxSize)
			{
				UpdateControllerSprites();
			}
		}
		if (m_bTextHasChanged)
		{
			if (base.resizeTextForBestFit && base.font != null && !base.font.dynamic)
			{
				DoBestFit();
			}
			m_bTextHasChanged = false;
		}
	}

	public void Convert()
	{
		if (string.IsNullOrEmpty(m_PlaceholderText))
		{
			m_PlaceholderText = text;
		}
		Localization.LoadDictionarys(reload: false);
		string localized = string.Empty;
		m_bTagFound = false;
		if (ShouldUsePCText())
		{
			m_bTagFound = Localization.Get(m_LocalizationTag + "_PC", out localized);
		}
		if (!m_bTagFound)
		{
			m_bTagFound = Localization.Get(m_LocalizationTag, out localized);
		}
		if (m_bTagFound)
		{
			CheckMarkup(localized);
			text = localized;
		}
		else
		{
			CheckMarkup(localized);
			text = localized;
		}
		if (m_bHasIcons)
		{
			CanvasUpdateRegistry.TryRegisterCanvasElementForLayoutRebuild(this);
		}
	}

	public void CheckMarkup()
	{
		if (!m_bTagFound && !string.IsNullOrEmpty(m_NonLocalizedPCText) && ShouldUsePCText())
		{
			CheckMarkup(m_NonLocalizedPCText);
		}
		else
		{
			CheckMarkup(text);
		}
	}

	private void CheckMarkup(string text)
	{
		if (this == null)
		{
			return;
		}
		m_MarkedUpString = text;
		Rewired.Player player = null;
		if (m_UsePCKeySizeOverride && Application.isPlaying && base.gameObject.scene.IsValid())
		{
			if (m_DefaultFontSize == -1)
			{
				m_DefaultFontSize = base.fontSize;
			}
			if (ReInput.isReady)
			{
				player = ReInput.players.GetPlayer(GetOwnersRewiredId());
				if (player != null && player.controllers != null && player.controllers.GetLastActiveController() != null && player.controllers.GetLastActiveController().type != ControllerType.Joystick)
				{
					if (!m_bCurrentSizePCKey)
					{
						m_bCurrentSizePCKey = true;
						base.fontSize = m_PCKeySizeOverride;
					}
				}
				else if (m_bCurrentSizePCKey)
				{
					base.fontSize = m_DefaultFontSize;
					m_bCurrentSizePCKey = false;
				}
			}
		}
		int num = 0;
		if (m_PCIconOverrideSize > 0)
		{
			num = m_PCIconOverrideSize;
		}
		else if (base.resizeTextForBestFit)
		{
			num = base.resizeTextMaxSize;
			m_CachedfontSizeUsedForBestFit = base.resizeTextMaxSize;
		}
		else
		{
			num = ((m_ImagesAttached == null || m_ImagesAttached.Count <= 0) ? base.fontSize : m_ImagesAttached[m_ImagesAttached.Count - 1].Size);
		}
		string pCInputTextColour = m_sPCInputTextColorHex;
		bool flag = false;
		if (m_bUseOverrideColour)
		{
			pCInputTextColour = m_PCInputTextColorHexOverride;
		}
		flag = m_bAlwaysUseControllerIcons;
		m_bHasIcons = Localization.CheckForMarkup(GetOwnersRewiredId(), ref m_MarkedUpString, ref m_ImagesAttached, flag, pCInputTextColour, num, m_bUseUserDefinedPrefixSuffix, m_PCTextInputPrefix, m_PCTextInputSuffix);
		if (Application.isPlaying)
		{
			if (m_IconImagePrefab == null)
			{
				m_IconImagePrefab = Resources.Load("T17_UIElements/T17Image") as GameObject;
			}
			UpdateQuadImage();
		}
		if (m_bHasIcons && !T17RewiredStandaloneInputModule.m_TextToRefresh.Contains(this))
		{
			T17RewiredStandaloneInputModule.m_TextToRefresh.Add(this);
		}
		bool flag2 = false;
		flag2 = m_bHasIcons || m_bForceRichText || m_MarkedUpString.IndexOf("<") >= 0;
		if (base.supportRichText != flag2)
		{
			base.supportRichText = flag2;
		}
	}

	public void SetLocalisedTextCatchAll(string text)
	{
		bool flag = false;
		if ((!m_bNeedsLocalization) ? (this.text != text) : (m_LocalizationTag != text))
		{
			this.text = text;
			SetNewPlaceHolder(text);
			m_bNeedsLocalization = true;
			SetNewLocalizationTag(text);
		}
	}

	public void SetNewLocalizationTag(string newTag)
	{
		if (m_bNeedsLocalization)
		{
			m_LocalizationTag = newTag;
			Convert();
		}
		else
		{
			m_LocalizationTag = string.Empty;
			CheckMarkup(newTag);
			text = newTag;
		}
	}

	public void SetNewPlaceHolder(string newPlaceholder)
	{
		m_PlaceholderText = newPlaceholder;
	}

	public void SetNonLocalizedText(string text, bool checkMarkup = false)
	{
		ResetLocalisation();
		if (checkMarkup)
		{
			CheckMarkup(text);
		}
		this.text = text;
	}

	private void ResetLocalisation()
	{
		m_bNeedsLocalization = false;
		m_LocalizationTag = string.Empty;
		m_PlaceholderText = string.Empty;
	}

	private void LanguageChanged()
	{
		if (m_bNeedsLocalization)
		{
			Convert();
		}
	}

	public override void SetVerticesDirty()
	{
		if (m_bResizingText)
		{
			return;
		}
		if (m_bHasIcons)
		{
			if (m_IconImagePrefab == null)
			{
				m_IconImagePrefab = Resources.Load("T17_UIElements/T17Image") as GameObject;
			}
			if (base.rectTransform != null)
			{
				base.rectTransform.GetWorldCorners(m_SelfWorldCorners);
			}
			for (int i = 0; i < m_ImagesAttached.Count; i++)
			{
				ImageData imageData = m_ImagesAttached[i];
				if (!(imageData.ImageObj == null))
				{
					if (imageData.ImageObj.rectTransform != null)
					{
						imageData.ImageObj.rectTransform.GetWorldCorners(m_IconWorldCorners);
					}
					imageData.OutOfBounds = false;
					imageData.ImageObj.gameObject.SetActive(value: true);
				}
			}
		}
		base.SetVerticesDirty();
		m_bSetVerticesDirty = true;
	}

	public void UpdateQuadImage()
	{
		for (int num = m_ImagesAttached.Count - 1; num >= 0; num--)
		{
			if (m_ImagesAttached[num] != null && m_ImagesAttached[num].NeedsToBeDeleted)
			{
				if (!m_bFromOnValidate && !m_bFromOnPopulateMesh && m_ImagesAttached[num].ImageObj != null)
				{
					if (Application.isPlaying)
					{
						UnityEngine.Object.Destroy(m_ImagesAttached[num].ImageObj.gameObject);
					}
					else
					{
						UnityEngine.Object.DestroyImmediate(m_ImagesAttached[num].ImageObj.gameObject);
					}
					m_ImagesAttached.RemoveAt(num);
				}
				else if (m_ImagesAttached[num].ImageObj != null)
				{
					m_ImagesAttached[num].ImageObj.gameObject.SetActive(value: false);
				}
				else
				{
					m_ImagesAttached.RemoveAt(num);
				}
			}
		}
		if (!CanvasUpdateRegistry.IsRebuildingGraphics())
		{
			int num2 = 0;
			num2 += ProcessInputIcons(num2);
		}
		m_bFromOnValidate = false;
	}

	private int ProcessInputIcons(int matches)
	{
		if (m_IconImagePrefab == null)
		{
			return 0;
		}
		int count;
		T17Regex.StringMatch[] array = s_InputQuadRegex.Matches(m_MarkedUpString, out count);
		for (int i = 0; i < count; i++)
		{
			matches++;
			string value = array[i].value;
			int num = value.IndexOf("name=") + 5;
			int num2 = value.IndexOf(" size=");
			string spriteName = value.Substring(num, num2 - num) + matches;
			ImageData imageData = m_ImagesAttached.Find((ImageData dt) => dt.m_OriginalMarkup == spriteName);
			if (imageData == null)
			{
				continue;
			}
			imageData.PlaceInString = array[i].position;
			imageData.OriginalQuadIndex = imageData.PlaceInString * 4;
			if (imageData.ImageObj == null)
			{
				if (m_bFromOnPopulateMesh)
				{
					continue;
				}
				GameObject gameObject = UnityEngine.Object.Instantiate(m_IconImagePrefab);
				gameObject.layer = base.gameObject.layer;
				RectTransform rectTransform = gameObject.transform as RectTransform;
				if ((bool)rectTransform)
				{
					rectTransform.SetParent(base.rectTransform);
					rectTransform.localPosition = Vector3.zero;
					rectTransform.localRotation = Quaternion.identity;
					rectTransform.localScale = Vector3.one;
				}
				imageData.ImageObj = gameObject.GetComponent<T17Image>();
				imageData.Size = ((m_ImagesAttached.Count <= 0) ? base.fontSize : m_ImagesAttached[m_ImagesAttached.Count - 1].Size);
			}
			imageData.ImageObj.gameObject.name = "IMG_" + ((!(imageData.IconSprite == null)) ? imageData.IconSprite.name : "Not_Loaded");
			imageData.ImageObj.gameObject.SetActive(!imageData.OutOfBounds);
			imageData.ImageObj.sprite = imageData.IconSprite;
			float num3 = ((!(imageData.IconSprite == null)) ? (imageData.IconSprite.rect.width / imageData.IconSprite.rect.height) : 1f);
			imageData.ImageObj.rectTransform.sizeDelta = new Vector2((float)imageData.Size * num3, imageData.Size);
			imageData.ImageObj.enabled = true;
			imageData.ImageObj.rectTransform.anchoredPosition = imageData.Position;
		}
		return matches;
	}

	protected override void OnPopulateMesh(VertexHelper toFill)
	{
		if (!m_bHasIcons)
		{
			base.OnPopulateMesh(toFill);
			return;
		}
		string text = m_Text;
		m_Text = m_MarkedUpString;
		base.OnPopulateMesh(toFill);
		m_Text = text;
		m_bFromOnPopulateMesh = true;
		UpdateQuadImage();
		UIVertex vertex = default(UIVertex);
		for (int i = 0; i < m_ImagesAttached.Count; i++)
		{
			ImageData imageData = m_ImagesAttached[i];
			if (imageData.ImageObj == null)
			{
				continue;
			}
			int originalQuadIndex = imageData.OriginalQuadIndex;
			RectTransform rectTransform = imageData.ImageObj.rectTransform;
			Vector2 sizeDelta = rectTransform.sizeDelta;
			if (originalQuadIndex < toFill.currentVertCount)
			{
				toFill.PopulateUIVertex(ref vertex, originalQuadIndex);
				Vector2 vector = new Vector2(sizeDelta.x * imageData.AlignOffset.x, sizeDelta.y * imageData.AlignOffset.y);
				Vector2 position = new Vector2(vertex.position.x + sizeDelta.x / 2f + imageData.Offset.x - vector.x, vertex.position.y - sizeDelta.y / 2f + imageData.Offset.y - vector.y);
				if (position.x < 10000f && position.x > -10000f && position.y < 10000f && position.y > -10000f)
				{
					imageData.Position = position;
				}
				toFill.PopulateUIVertex(ref vertex, originalQuadIndex + 3);
				Vector3 position2 = vertex.position;
				int j = originalQuadIndex;
				for (int num = originalQuadIndex + 3; j < num; j++)
				{
					toFill.PopulateUIVertex(ref vertex, originalQuadIndex);
					vertex.position = position2;
					toFill.SetUIVertex(vertex, j);
				}
			}
		}
		UpdateQuadImage();
		m_bFromOnPopulateMesh = false;
		m_bOnPopulateMesh = true;
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		if (base.isActiveAndEnabled && base.resizeTextForBestFit && base.font != null && !base.font.dynamic)
		{
			DoBestFit();
			m_bTextHasChanged = false;
		}
	}

	private bool ShouldUsePCText()
	{
		Rewired.Player player = null;
		if (ReInput.isReady)
		{
			player = ReInput.players.GetPlayer(GetOwnersRewiredId());
			return player != null && player.controllers != null && player.controllers.GetLastActiveController() != null && player.controllers.GetLastActiveController().type != ControllerType.Joystick;
		}
		return false;
	}

	protected void UpdateControllerSprites()
	{
		if (!(this == null))
		{
			if (m_bNeedsLocalization)
			{
				Convert();
			}
			CheckMarkup();
		}
	}

	public void SetGamerForEventSystem(Gamer gamer, T17EventSystem gamersEventSystem = null)
	{
		m_EventSystemsRewiredId = gamer.m_RewiredPlayer.id;
		if (!m_bTagFound && !string.IsNullOrEmpty(m_NonLocalizedPCText) && ShouldUsePCText())
		{
			CheckMarkup(m_NonLocalizedPCText);
		}
		else
		{
			CheckMarkup(text);
		}
	}

	public T17EventSystem GetDomain()
	{
		return null;
	}

	public GameObject GetGameobject()
	{
		return base.gameObject;
	}

	public bool CanReselectOnMouseDisable()
	{
		return true;
	}

	public bool ReleaseSelectionOnPointerClickOrExit()
	{
		if (m_ReleaseOnPointerClickDelegate == null)
		{
			return true;
		}
		return m_ReleaseOnPointerClickDelegate();
	}

	public int GetOwnersRewiredId()
	{
		if (m_GetRewiredIdDelegate == null)
		{
			return m_EventSystemsRewiredId;
		}
		return m_GetRewiredIdDelegate();
	}

	public void SetGetRewiredIndexDelegate(Func<int> theDelegate)
	{
		m_GetRewiredIdDelegate = theDelegate;
	}

	private void SetPlatformOverrides()
	{
		if (!Application.isPlaying || m_PlatformOverrides.Count <= 0)
		{
			return;
		}
		Platform.PlatformOverride platformOverride = Platform.PlatformOverride.Standalone;
		for (int i = 0; i < m_PlatformOverrides.Count; i++)
		{
			PlatformOverride platformOverride2 = m_PlatformOverrides[i];
			if (platformOverride2.PlatformToOverride == platformOverride)
			{
				base.fontSize = platformOverride2.OverrideFontSize;
				base.resizeTextMinSize = platformOverride2.OverrideFontMinSize;
				base.resizeTextMaxSize = platformOverride2.OverrideFontMaxSize;
				break;
			}
		}
	}

	public void ForceDoBestFit()
	{
		m_bTextHasChanged = true;
		DoBestFit();
	}

	public void DoBestFit()
	{
		if (!base.resizeTextForBestFit)
		{
			return;
		}
		Vector2 size = base.rectTransform.rect.size;
		if (size.x == m_PreviousSize.x && size.y == m_PreviousSize.y && !m_bTextHasChanged)
		{
			return;
		}
		m_PreviousSize = size;
		if (size.x <= 0f || size.y <= 0f)
		{
			return;
		}
		if (base.fontSize < base.resizeTextMinSize || base.fontSize > base.resizeTextMaxSize)
		{
			base.fontSize = base.resizeTextMaxSize;
		}
		int num = base.resizeTextMinSize;
		int num2 = base.resizeTextMaxSize;
		int num3 = base.fontSize;
		int num4 = base.fontSize;
		m_bResizingText = true;
		int num5 = base.fontSize;
		while (true)
		{
			bool flag = false;
			if (preferredWidth <= size.x)
			{
				if (preferredHeight <= size.y)
				{
					flag = true;
				}
			}
			else if (preferredHeight <= size.y)
			{
				flag = true;
			}
			if (num < num2)
			{
				if (flag)
				{
					num4 = num3;
					num = num3;
				}
				else
				{
					num2 = num3 - 1;
				}
				num3 = (base.fontSize = (num + num2 + 1) / 2);
				continue;
			}
			TextGenerationSettings generationSettings = GetGenerationSettings(size);
			generationSettings.fontSize = base.fontSize;
			generationSettings.generateOutOfBounds = false;
			base.cachedTextGenerator.Populate(text, generationSettings);
			int characterCount = base.cachedTextGenerator.characterCount;
			if (characterCount <= 0 || characterCount >= base.cachedTextGeneratorForLayout.characterCount || num3 <= 1)
			{
				break;
			}
			num3 = (base.fontSize = num3 - 1);
			num4 = base.fontSize;
		}
		if (base.fontSize != num4)
		{
			base.fontSize = num4;
		}
		m_bResizingText = false;
		if (num5 != base.fontSize)
		{
			SetVerticesDirty();
		}
		m_CachedfontSizeUsedForBestFit = base.fontSize;
	}

	public override void Rebuild(CanvasUpdate update)
	{
		base.Rebuild(update);
		if (update == CanvasUpdate.Prelayout && m_bHasIcons)
		{
			UpdateGeometry();
		}
	}
}
