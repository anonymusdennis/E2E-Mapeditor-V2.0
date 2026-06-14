using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CustomisationOptionsFE : CustomisationDialogTabMenu
{
	public enum Mode
	{
		Hair = 2,
		Hat,
		UpperFace,
		LowerFace
	}

	private class RT
	{
		public RenderTexture m_RenderTexture;

		public int m_RenderTextureID;
	}

	[Header("Settings")]
	public GameObject m_ToggleOptionPrefab;

	public ToggleGroup m_ToggleGroup;

	public T17ScrollView m_ScrollView;

	public Mode m_Mode = Mode.Hair;

	[Header("Icon Rendering")]
	public UI_DrawCharacterToRenderTexture m_CharacterRenderer;

	public bool m_bPoolRenderTextures = true;

	[FormerlySerializedAs("m_InvisibleOptionTexturePlaceholder")]
	public Texture m_PlaceholderIconTexture;

	private Dictionary<T17RawImage, RT> m_RenderTextures = new Dictionary<T17RawImage, RT>();

	private List<RT> m_RenderTexturePool = new List<RT>();

	private CustomisationToggleOption[] m_OptionButtons = new CustomisationToggleOption[0];

	private Vector3[] m_ViewCorners = new Vector3[4];

	private Vector3[] m_ButtonCorners = new Vector3[4];

	private bool m_bInitialiseToggleGroups;

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		if (m_bInitialiseToggleGroups)
		{
			m_bInitialiseToggleGroups = false;
			int count = 0;
			switch (m_Mode)
			{
			case Mode.Hair:
				count = base.availableAppearances.hairs.Count;
				break;
			case Mode.Hat:
				count = base.availableAppearances.hats.Count;
				break;
			case Mode.UpperFace:
				count = base.availableAppearances.upperFaces.Count;
				break;
			case Mode.LowerFace:
				count = base.availableAppearances.lowerFaces.Count;
				break;
			}
			InitialiseToggleGroup(m_ToggleGroup, count);
		}
		switch (m_Mode)
		{
		case Mode.Hair:
			UpdateOptions(m_ToggleGroup, base.availableAppearances.hairs, base.newAppearances.hairs, base.seenAppearances.hairs, base.categorisedAppearances.categories, base.categorisedAppearances.hairs);
			break;
		case Mode.Hat:
			UpdateOptions(m_ToggleGroup, base.availableAppearances.hats, base.newAppearances.hats, base.seenAppearances.hats, base.categorisedAppearances.categories, base.categorisedAppearances.hats);
			break;
		case Mode.UpperFace:
			UpdateOptions(m_ToggleGroup, base.availableAppearances.upperFaces, base.newAppearances.upperFaces, base.seenAppearances.upperFaces, base.categorisedAppearances.categories, base.categorisedAppearances.upperFaces);
			break;
		case Mode.LowerFace:
			UpdateOptions(m_ToggleGroup, base.availableAppearances.lowerFaces, base.newAppearances.lowerFaces, base.seenAppearances.lowerFaces, base.categorisedAppearances.categories, base.categorisedAppearances.lowerFaces);
			break;
		}
		if (m_CharacterRenderer != null)
		{
			m_CharacterRenderer.RegisterActivationRequest(base.gameObject);
		}
		if (m_ScrollView != null)
		{
			m_ScrollView.Show(currentGamer, null, null, hideInvoker: false);
		}
		UpdateIconTextures(bForceRedraw: true);
		ShowInitialValues();
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		if (m_CharacterRenderer != null)
		{
			m_CharacterRenderer.UnregisterActivationRequest(base.gameObject);
		}
		List<T17RawImage> list = new List<T17RawImage>(m_RenderTextures.Keys);
		for (int i = 0; i < list.Count; i++)
		{
			T17RawImage key = list[i];
			RT rT = m_RenderTextures[key];
			if (rT != null)
			{
				m_CharacterRenderer.CleanupRenderTexture(ref rT.m_RenderTextureID);
				rT.m_RenderTexture = null;
			}
		}
		m_RenderTextures.Clear();
		for (int j = 0; j < m_RenderTexturePool.Count; j++)
		{
			m_CharacterRenderer.CleanupRenderTexture(ref m_RenderTexturePool[j].m_RenderTextureID);
			m_RenderTexturePool[j].m_RenderTexture = null;
		}
		m_RenderTexturePool.Clear();
		return true;
	}

	public override bool HasAvailableOptions()
	{
		bool result = false;
		if (base.availableAppearances != null)
		{
			switch (m_Mode)
			{
			case Mode.Hair:
				result = base.availableAppearances.hairs.Count > 0;
				break;
			case Mode.Hat:
				result = base.availableAppearances.hats.Count > 0;
				break;
			case Mode.UpperFace:
				result = base.availableAppearances.upperFaces.Count > 0;
				break;
			case Mode.LowerFace:
				result = base.availableAppearances.lowerFaces.Count > 0;
				break;
			}
		}
		return result;
	}

	public override bool HasNewOptions()
	{
		bool result = false;
		if (base.newAppearances != null)
		{
			switch (m_Mode)
			{
			case Mode.Hair:
				result = base.newAppearances.hairs.Count - base.seenAppearances.hairs.Count > 0;
				break;
			case Mode.Hat:
				result = base.newAppearances.hats.Count - base.seenAppearances.hats.Count > 0;
				break;
			case Mode.UpperFace:
				result = base.newAppearances.upperFaces.Count - base.seenAppearances.upperFaces.Count > 0;
				break;
			case Mode.LowerFace:
				result = base.newAppearances.lowerFaces.Count - base.seenAppearances.lowerFaces.Count > 0;
				break;
			}
		}
		return result;
	}

	protected override void Awake()
	{
		base.Awake();
		m_bInitialiseToggleGroups = true;
	}

	protected override void Update()
	{
		base.Update();
		UpdateIconTextures();
	}

	private void InitialiseToggleGroup(ToggleGroup group, int count)
	{
		if (group == null || count < 0 || m_ToggleOptionPrefab == null)
		{
			return;
		}
		m_OptionButtons = group.GetComponentsInChildren<CustomisationToggleOption>(includeInactive: true);
		if (m_OptionButtons.Length < count && m_ToggleOptionPrefab != null)
		{
			List<CustomisationToggleOption> list = new List<CustomisationToggleOption>(m_OptionButtons);
			int num = count - m_OptionButtons.Length;
			for (int i = 0; i < num; i++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(m_ToggleOptionPrefab, m_ScrollView.m_ContentParent);
				if (gameObject != null)
				{
					list.Add(gameObject.GetComponent<CustomisationToggleOption>());
				}
			}
			m_OptionButtons = list.ToArray();
		}
		for (int j = 0; j < m_OptionButtons.Length; j++)
		{
			CustomisationToggleOption customisationToggleOption = m_OptionButtons[j];
			if (!(customisationToggleOption == null))
			{
				customisationToggleOption.Reset();
				customisationToggleOption.SetToggleGroup(group, j);
				customisationToggleOption.onValueChanged = (CustomisationToggleOption.OnToggleChanged)Delegate.Remove(customisationToggleOption.onValueChanged, new CustomisationToggleOption.OnToggleChanged(OnToggleChanged));
				customisationToggleOption.onValueChanged = (CustomisationToggleOption.OnToggleChanged)Delegate.Combine(customisationToggleOption.onValueChanged, new CustomisationToggleOption.OnToggleChanged(OnToggleChanged));
				customisationToggleOption.onHighlightChanged = (CustomisationToggleOption.OnHighlightChanged)Delegate.Remove(customisationToggleOption.onHighlightChanged, new CustomisationToggleOption.OnHighlightChanged(OnToggleHighlighted));
				customisationToggleOption.onHighlightChanged = (CustomisationToggleOption.OnHighlightChanged)Delegate.Combine(customisationToggleOption.onHighlightChanged, new CustomisationToggleOption.OnHighlightChanged(OnToggleHighlighted));
				if (customisationToggleOption.m_Texture != null)
				{
					customisationToggleOption.m_Texture.texture = m_PlaceholderIconTexture;
				}
			}
		}
	}

	private void OnToggleChanged(ToggleGroup group, int index, bool toggled)
	{
		if (base.modifiableCustomisation != null && toggled)
		{
			switch (m_Mode)
			{
			case Mode.Hair:
				base.modifiableCustomisation.hair = base.availableAppearances.hairs[index];
				break;
			case Mode.Hat:
				base.modifiableCustomisation.hat = base.availableAppearances.hats[index];
				break;
			case Mode.UpperFace:
				base.modifiableCustomisation.upperFace = base.availableAppearances.upperFaces[index];
				break;
			case Mode.LowerFace:
				base.modifiableCustomisation.lowerFace = base.availableAppearances.lowerFaces[index];
				break;
			}
		}
	}

	private void OnToggleHighlighted(ToggleGroup group, int index, bool highlighted)
	{
		UnlockManager instance = UnlockManager.GetInstance();
		if (instance == null || !highlighted)
		{
			return;
		}
		bool flag = false;
		switch (m_Mode)
		{
		case Mode.Hair:
			flag = AddIfMissing(base.availableAppearances.hairs[index], ref base.seenAppearances.hairs, base.newAppearances.hairs);
			break;
		case Mode.Hat:
			flag = AddIfMissing(base.availableAppearances.hats[index], ref base.seenAppearances.hats, base.newAppearances.hats);
			break;
		case Mode.UpperFace:
			flag = AddIfMissing(base.availableAppearances.upperFaces[index], ref base.seenAppearances.upperFaces, base.newAppearances.upperFaces);
			break;
		case Mode.LowerFace:
			flag = AddIfMissing(base.availableAppearances.lowerFaces[index], ref base.seenAppearances.lowerFaces, base.newAppearances.lowerFaces);
			break;
		}
		if (!flag)
		{
			return;
		}
		UpdateNewOptionsIcon();
		Transform transform = group.transform;
		if (index >= 0 && index < transform.childCount)
		{
			Transform child = transform.GetChild(index);
			CustomisationToggleOption component = child.GetComponent<CustomisationToggleOption>();
			if (component != null)
			{
				component.SetIsNew(isNew: false);
			}
		}
	}

	private bool AddIfMissing<T>(T value, ref List<T> values, List<T> toCheck = null)
	{
		if (toCheck != null && !toCheck.Contains(value))
		{
			return false;
		}
		if (!values.Contains(value))
		{
			values.Add(value);
			return true;
		}
		return false;
	}

	private void UpdateOptions<T>(ToggleGroup group, List<T> values, List<T> newValues, List<T> seenValues, List<UnlockCategories> categories, List<T>[] categorisedValues)
	{
		if (m_OptionButtons == null || m_OptionButtons.Length <= 0)
		{
			return;
		}
		for (int i = 0; i < m_OptionButtons.Length; i++)
		{
			CustomisationToggleOption customisationToggleOption = m_OptionButtons[i];
			if (customisationToggleOption == null)
			{
				continue;
			}
			if (i >= values.Count)
			{
				customisationToggleOption.gameObject.SetActive(value: false);
				continue;
			}
			customisationToggleOption.gameObject.SetActive(value: true);
			T item = values[i];
			bool isNew = newValues.Contains(item) && !seenValues.Contains(item);
			customisationToggleOption.SetIsNew(isNew);
			int num = 0;
			for (int j = 0; j < categories.Count && j < categorisedValues.Length; j++)
			{
				if (categorisedValues[j].Contains(item))
				{
					num |= (int)categories[j];
				}
			}
			customisationToggleOption.SetCategories(num);
		}
	}

	private void UpdateIconTextures(bool bForceRedraw = false)
	{
		if (m_CharacterRenderer == null)
		{
			return;
		}
		if (m_ScrollView != null && m_ScrollView.m_ViewPort != null)
		{
			m_ScrollView.m_ViewPort.GetWorldCorners(m_ViewCorners);
		}
		Customisation customisation = new Customisation(base.modifiableCustomisation);
		customisation.hair = CustomisationData.Hair.NULL;
		customisation.hat = CustomisationData.Hat.NULL;
		customisation.upperFace = CustomisationData.UpperFaceAccessory.NULL;
		customisation.lowerFace = CustomisationData.LowerFaceAccessory.NULL;
		for (int i = 0; i < m_OptionButtons.Length; i++)
		{
			CustomisationToggleOption customisationToggleOption = m_OptionButtons[i];
			if (!customisationToggleOption.gameObject.activeSelf)
			{
				break;
			}
			T17RawImage texture = customisationToggleOption.m_Texture;
			if (texture == null)
			{
				continue;
			}
			RT rT = null;
			if (m_RenderTextures.ContainsKey(texture))
			{
				rT = m_RenderTextures[texture];
			}
			bool flag = true;
			if (m_ScrollView != null && m_ScrollView.m_ViewPort != null)
			{
				texture.rectTransform.GetWorldCorners(m_ButtonCorners);
				if ((m_ButtonCorners[2].y < m_ViewCorners[0].y && m_ButtonCorners[3].y < m_ViewCorners[0].y && m_ButtonCorners[2].y < m_ViewCorners[1].y && m_ButtonCorners[3].y < m_ViewCorners[1].y) || (m_ButtonCorners[0].y > m_ViewCorners[2].y && m_ButtonCorners[1].y > m_ViewCorners[2].y && m_ButtonCorners[0].y > m_ViewCorners[3].y && m_ButtonCorners[1].y > m_ViewCorners[3].y))
				{
					flag = false;
				}
			}
			if (flag && (bForceRedraw || rT == null || rT.m_RenderTexture == null))
			{
				if (rT == null)
				{
					if (m_RenderTexturePool.Count > 0)
					{
						int index = m_RenderTexturePool.Count - 1;
						rT = m_RenderTexturePool[index];
						m_RenderTexturePool.RemoveAt(index);
					}
					else
					{
						rT = new RT();
					}
					m_RenderTextures.Add(texture, rT);
				}
				if (rT.m_RenderTexture == null)
				{
					int num = Mathf.FloorToInt(texture.rectTransform.rect.width);
					int num2 = Mathf.FloorToInt(texture.rectTransform.rect.height);
					if (num > 0 && num2 > 0)
					{
						rT.m_RenderTexture = m_CharacterRenderer.CreateRenderTexture(num, num2, ref rT.m_RenderTextureID);
					}
				}
				if (rT.m_RenderTexture != null)
				{
					switch (m_Mode)
					{
					case Mode.Hair:
						customisation.hair = base.availableAppearances.hairs[i];
						break;
					case Mode.Hat:
						customisation.hat = base.availableAppearances.hats[i];
						break;
					case Mode.UpperFace:
						customisation.upperFace = base.availableAppearances.upperFaces[i];
						break;
					case Mode.LowerFace:
						customisation.lowerFace = base.availableAppearances.lowerFaces[i];
						break;
					}
					m_CharacterRenderer.SetCustomisation(customisation);
					m_CharacterRenderer.DrawCharacter(rT.m_RenderTexture);
					customisationToggleOption.SetImage(rT.m_RenderTexture);
				}
			}
			if (!flag && rT != null)
			{
				m_RenderTextures.Remove(texture);
				if (m_bPoolRenderTextures)
				{
					m_RenderTexturePool.Add(rT);
				}
				else
				{
					m_CharacterRenderer.CleanupRenderTexture(ref rT.m_RenderTextureID);
					rT.m_RenderTexture = null;
				}
				customisationToggleOption.SetImage(m_PlaceholderIconTexture);
			}
		}
	}

	private void ShowInitialValues()
	{
		if (base.modifiableCustomisation != null)
		{
			int num = 0;
			switch (m_Mode)
			{
			case Mode.Hair:
				num = base.availableAppearances.hairs.IndexOf(base.modifiableCustomisation.hair);
				break;
			case Mode.Hat:
				num = base.availableAppearances.hats.IndexOf(base.modifiableCustomisation.hat);
				break;
			case Mode.UpperFace:
				num = base.availableAppearances.upperFaces.IndexOf(base.modifiableCustomisation.upperFace);
				break;
			case Mode.LowerFace:
				num = base.availableAppearances.lowerFaces.IndexOf(base.modifiableCustomisation.lowerFace);
				break;
			}
			if (num < 0)
			{
				num = 0;
			}
			SelectToggle(m_ToggleGroup, num);
		}
	}

	private void SelectToggle(ToggleGroup group, int index)
	{
		Transform transform = group.transform;
		if (index >= 0 && index < transform.childCount)
		{
			Transform child = transform.GetChild(index);
			CustomisationToggleOption component = child.GetComponent<CustomisationToggleOption>();
			if (component != null)
			{
				component.ForceSelect();
			}
		}
	}
}
