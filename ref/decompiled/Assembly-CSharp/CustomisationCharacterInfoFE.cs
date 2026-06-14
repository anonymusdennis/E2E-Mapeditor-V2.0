using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomisationCharacterInfoFE : CustomisationDialogTabMenu
{
	[Header("Settings")]
	public GameObject m_ToggleOptionPrefab;

	public T17Text m_PrefixLabel;

	public T17InputField m_NameField;

	public ToggleGroup m_BodyGroup;

	public ToggleGroup m_SkinGroup;

	public T17NavigableGrid m_BodyGrid;

	public T17NavigableGrid m_SkinGrid;

	[Header("Icons")]
	public Texture[] m_BodyIcons = new Texture[0];

	public Texture[] m_SkinIcons = new Texture[0];

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
			InitialiseToggleGroup(m_BodyGroup);
			InitialiseToggleGroup(m_SkinGroup);
		}
		UpdateOptions(m_BodyGroup, base.availableAppearances.bodyTypes, base.newAppearances.bodyTypes, base.seenAppearances.bodyTypes, base.categorisedAppearances.categories, base.categorisedAppearances.bodyTypes);
		UpdateOptions(m_SkinGroup, base.availableAppearances.skinColours, base.newAppearances.skinColours, base.seenAppearances.skinColours, base.categorisedAppearances.categories, base.categorisedAppearances.skinColours);
		UpdateIcons(m_BodyGroup, base.availableAppearances.bodyTypes, m_BodyIcons);
		UpdateIcons(m_SkinGroup, base.availableAppearances.skinColours, m_SkinIcons);
		if (m_BodyGrid != null)
		{
			m_BodyGrid.Show(currentGamer, null, null, hideInvoker: false);
		}
		if (m_SkinGrid != null)
		{
			m_SkinGrid.Show(currentGamer, null, null, hideInvoker: false);
		}
		ShowInitialValues();
		if (m_NameField != null)
		{
			T17Button t17Button = m_NameField.GetComponent<T17Button>();
			if (t17Button == null)
			{
				t17Button = m_NameField.GetComponentInParent<T17Button>();
			}
			if (t17Button != null)
			{
				t17Button.m_CanUIReselectDelegate = m_NameField.CanReselectOnMouseDisable;
				t17Button.m_ReleaseOnPointerClickDelegate = m_NameField.ReleaseSelectionOnPointerClickOrExit;
			}
			T17Text t17Text = m_NameField.GetComponent<T17Text>();
			if (t17Text == null)
			{
				t17Text = m_NameField.GetComponentInParent<T17Text>();
			}
			if (t17Text != null)
			{
				t17Text.m_ReleaseOnPointerClickDelegate = m_NameField.ReleaseSelectionOnPointerClickOrExit;
			}
		}
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		if (m_NameField != null)
		{
			T17Button t17Button = m_NameField.GetComponent<T17Button>();
			if (t17Button == null)
			{
				t17Button = m_NameField.GetComponentInParent<T17Button>();
			}
			if (t17Button != null)
			{
				t17Button.m_CanUIReselectDelegate = null;
				t17Button.m_ReleaseOnPointerClickDelegate = null;
			}
			T17Text t17Text = m_NameField.GetComponent<T17Text>();
			if (t17Text == null)
			{
				t17Text = m_NameField.GetComponentInParent<T17Text>();
			}
			if (t17Text != null)
			{
				t17Text.m_ReleaseOnPointerClickDelegate = null;
			}
		}
		return true;
	}

	public override bool HasAvailableOptions()
	{
		bool result = true;
		if (base.availableAppearances != null)
		{
			result = base.availableAppearances.bodyTypes.Count > 0 || base.availableAppearances.skinColours.Count > 0;
		}
		return result;
	}

	public override bool HasNewOptions()
	{
		bool result = false;
		if (base.newAppearances != null)
		{
			int num = base.newAppearances.bodyTypes.Count - base.seenAppearances.bodyTypes.Count;
			int num2 = base.newAppearances.skinColours.Count - base.seenAppearances.skinColours.Count;
			result = num > 0 || num2 > 0;
		}
		return result;
	}

	protected override void Awake()
	{
		base.Awake();
		m_bInitialiseToggleGroups = true;
		m_NameField.onValueChanged.AddListener(OnNameChanged);
	}

	private void InitialiseToggleGroup(ToggleGroup group)
	{
		if (group == null || m_ToggleOptionPrefab == null)
		{
			return;
		}
		CustomisationToggleOption[] componentsInChildren = group.GetComponentsInChildren<CustomisationToggleOption>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			CustomisationToggleOption customisationToggleOption = componentsInChildren[i];
			if (!(customisationToggleOption == null))
			{
				customisationToggleOption.Reset();
				customisationToggleOption.SetToggleGroup(group, i);
				customisationToggleOption.onValueChanged = (CustomisationToggleOption.OnToggleChanged)Delegate.Remove(customisationToggleOption.onValueChanged, new CustomisationToggleOption.OnToggleChanged(OnToggleChanged));
				customisationToggleOption.onValueChanged = (CustomisationToggleOption.OnToggleChanged)Delegate.Combine(customisationToggleOption.onValueChanged, new CustomisationToggleOption.OnToggleChanged(OnToggleChanged));
				customisationToggleOption.onHighlightChanged = (CustomisationToggleOption.OnHighlightChanged)Delegate.Remove(customisationToggleOption.onHighlightChanged, new CustomisationToggleOption.OnHighlightChanged(OnToggleHighlighted));
				customisationToggleOption.onHighlightChanged = (CustomisationToggleOption.OnHighlightChanged)Delegate.Combine(customisationToggleOption.onHighlightChanged, new CustomisationToggleOption.OnHighlightChanged(OnToggleHighlighted));
			}
		}
	}

	private void OnToggleChanged(ToggleGroup group, int index, bool toggled)
	{
		if (base.modifiableCustomisation != null && toggled)
		{
			if (group == m_BodyGroup)
			{
				base.modifiableCustomisation.body = base.availableAppearances.bodyTypes[index];
			}
			if (group == m_SkinGroup)
			{
				base.modifiableCustomisation.skin = base.availableAppearances.skinColours[index];
			}
		}
	}

	public void OnNameChanged(string text)
	{
		base.modifiableCustomisation.name = m_NameField.text;
	}

	private void OnToggleHighlighted(ToggleGroup group, int index, bool highlighted)
	{
		UnlockManager instance = UnlockManager.GetInstance();
		if (instance == null || !highlighted)
		{
			return;
		}
		bool flag = false;
		if (group == m_BodyGroup)
		{
			flag = AddIfMissing(base.availableAppearances.bodyTypes[index], ref base.seenAppearances.bodyTypes, base.newAppearances.bodyTypes);
		}
		if (group == m_SkinGroup)
		{
			flag = AddIfMissing(base.availableAppearances.skinColours[index], ref base.seenAppearances.skinColours, base.newAppearances.skinColours);
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
		CustomisationToggleOption[] componentsInChildren = group.GetComponentsInChildren<CustomisationToggleOption>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			CustomisationToggleOption customisationToggleOption = componentsInChildren[i];
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

	private void UpdateIcons<T>(ToggleGroup group, List<T> values, Texture[] icons)
	{
		CustomisationToggleOption[] componentsInChildren = group.GetComponentsInChildren<CustomisationToggleOption>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length && i < values.Count && i < icons.Length; i++)
		{
			CustomisationToggleOption customisationToggleOption = componentsInChildren[i];
			if (!(customisationToggleOption == null))
			{
				int num = (int)(object)values[i];
				if (num >= 0 && num < icons.Length)
				{
					customisationToggleOption.SetImage(icons[num]);
				}
				else
				{
					customisationToggleOption.SetImage(null);
				}
			}
		}
	}

	private void ShowInitialValues()
	{
		if (base.modifiableCustomisation != null)
		{
			string localised = string.Empty;
			Localization.GetWithKeySwap(base.modifiableCustomisation.namePrefixKey, out localised, "$name", string.Empty);
			localised = localised.Trim();
			if (m_PrefixLabel != null)
			{
				m_PrefixLabel.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(localised));
				m_PrefixLabel.text = localised;
			}
			if (m_NameField != null)
			{
				m_NameField.text = base.modifiableCustomisation.name;
			}
			int num = base.availableAppearances.bodyTypes.IndexOf(base.modifiableCustomisation.body);
			if (num < 0)
			{
				num = 0;
			}
			int num2 = base.availableAppearances.skinColours.IndexOf(base.modifiableCustomisation.skin);
			if (num2 < 0)
			{
				num2 = 0;
			}
			SelectOption(m_BodyGroup, num);
			SelectOption(m_SkinGroup, num2);
		}
	}

	private void SelectOption(ToggleGroup group, int index)
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
