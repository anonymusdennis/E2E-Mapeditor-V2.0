using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomisationToggleOption : MonoBehaviour
{
	public delegate void OnToggleChanged(ToggleGroup group, int index, bool toggled);

	public delegate void OnHighlightChanged(ToggleGroup group, int index, bool highlighted);

	public T17Toggle m_Toggle;

	private T17_UISelectDeselectEvents m_SelectDeselectEvents;

	public T17RawImage m_Texture;

	public T17Image m_SelectedIndicator;

	public T17Image m_NewIndicator;

	public T17Image[] m_CategoryIndicators = new T17Image[0];

	public OnToggleChanged onValueChanged;

	public OnHighlightChanged onHighlightChanged;

	private ToggleGroup m_ToggleGroup;

	private int m_ToggleIndex = -1;

	private int m_CategoryMask;

	private bool m_bIsNew;

	private bool m_bIsSelected;

	private void Awake()
	{
		if (m_Toggle != null)
		{
			m_Toggle.onValueChanged.RemoveAllListeners();
			m_Toggle.onValueChanged.AddListener(OnToggleValueChanged);
		}
		if (m_SelectDeselectEvents == null)
		{
			m_SelectDeselectEvents = GetComponent<T17_UISelectDeselectEvents>();
			if (m_SelectDeselectEvents == null)
			{
				m_SelectDeselectEvents = base.gameObject.AddComponent<T17_UISelectDeselectEvents>();
			}
		}
		if (m_SelectDeselectEvents != null)
		{
			m_SelectDeselectEvents.m_OnSelectEvent.AddListener(OnHighlighted);
			m_SelectDeselectEvents.m_OnDeselectEvent.AddListener(OnDeHighlighted);
		}
	}

	public void Reset()
	{
		if (m_Toggle != null)
		{
			m_Toggle.group = null;
		}
		m_ToggleGroup = null;
		m_ToggleIndex = -1;
		onValueChanged = null;
		if (m_Texture != null)
		{
			m_Texture.texture = null;
		}
		m_bIsNew = false;
		m_bIsSelected = false;
		UpdateStatusIcon();
		m_CategoryMask = 0;
		UpdateCategoryIcons();
	}

	public void SetImage(Texture image)
	{
		if (m_Texture != null)
		{
			m_Texture.texture = image;
		}
	}

	public void SetIsNew(bool isNew)
	{
		m_bIsNew = isNew;
		UpdateStatusIcon();
	}

	public void SetCategories(int categoryMask)
	{
		m_CategoryMask = categoryMask;
		UpdateCategoryIcons();
	}

	public void SetToggleGroup(ToggleGroup group, int index)
	{
		m_ToggleGroup = group;
		m_ToggleIndex = index;
		if (m_Toggle != null)
		{
			m_Toggle.group = m_ToggleGroup;
		}
	}

	public void ForceSelect()
	{
		if (m_Toggle != null)
		{
			m_Toggle.isOn = true;
			if (m_ToggleGroup != null)
			{
				m_ToggleGroup.NotifyToggleOn(m_Toggle);
			}
		}
	}

	private void UpdateStatusIcon()
	{
		if (m_bIsSelected)
		{
			if (m_SelectedIndicator != null)
			{
				m_SelectedIndicator.enabled = true;
			}
			if (m_NewIndicator != null)
			{
				m_NewIndicator.enabled = false;
			}
		}
		else if (m_bIsNew)
		{
			if (m_SelectedIndicator != null)
			{
				m_SelectedIndicator.enabled = false;
			}
			if (m_NewIndicator != null)
			{
				m_NewIndicator.enabled = true;
			}
		}
		else
		{
			if (m_SelectedIndicator != null)
			{
				m_SelectedIndicator.enabled = false;
			}
			if (m_NewIndicator != null)
			{
				m_NewIndicator.enabled = false;
			}
		}
	}

	private void UpdateCategoryIcons()
	{
		if (m_CategoryMask != 0)
		{
			int num = 0;
			for (int i = 0; i < m_CategoryIndicators.Length; i++)
			{
				num = 1 << i;
				if (num >= 9)
				{
					break;
				}
				bool flag = (num & m_CategoryMask) > 0;
				if (m_CategoryIndicators[i] != null)
				{
					m_CategoryIndicators[i].enabled = flag;
				}
			}
			return;
		}
		for (int j = 0; j < m_CategoryIndicators.Length; j++)
		{
			if (m_CategoryIndicators[j] != null)
			{
				m_CategoryIndicators[j].enabled = false;
			}
		}
	}

	private void OnToggleValueChanged(bool toggled)
	{
		if (onValueChanged != null)
		{
			onValueChanged(m_ToggleGroup, m_ToggleIndex, toggled);
		}
		m_bIsSelected = toggled;
		UpdateStatusIcon();
	}

	private void OnHighlighted(BaseEventData baseEventData)
	{
		if (onHighlightChanged != null)
		{
			onHighlightChanged(m_ToggleGroup, m_ToggleIndex, highlighted: true);
		}
	}

	private void OnDeHighlighted(BaseEventData baseEventData)
	{
		if (onHighlightChanged != null)
		{
			onHighlightChanged(m_ToggleGroup, m_ToggleIndex, highlighted: false);
		}
	}
}
