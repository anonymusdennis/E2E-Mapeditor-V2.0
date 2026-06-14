using UnityEngine;

public class ShadowDetailOptionSelector : MonoBehaviour
{
	public T17Text m_UIText;

	private ShadowDetailOptionItem m_OptionItem;

	private int m_CurrentIndex;

	private static ShadowOption[] m_LookupTable = new ShadowOption[4]
	{
		new ShadowOption(ShadowLevel.Off, "Text.UI.ShadowsOff"),
		new ShadowOption(ShadowLevel.Low, "Text.UI.ShadowsLow"),
		new ShadowOption(ShadowLevel.Medium, "Text.UI.ShadowsMed"),
		new ShadowOption(ShadowLevel.High, "Text.UI.ShadowsHigh")
	};

	public ShadowLevel GetCurrentSelectedLevel()
	{
		return m_LookupTable[m_CurrentIndex].m_EnumValue;
	}

	public void Initialise(ShadowDetailOptionItem optionItem)
	{
		m_OptionItem = optionItem;
		m_CurrentIndex = CalculateCurrentIndex();
		UpdateElement();
	}

	private int CalculateCurrentIndex()
	{
		if (QualitySettings.shadows != 0)
		{
			switch (QualitySettings.shadowResolution)
			{
			case ShadowResolution.Low:
				return 1;
			case ShadowResolution.Medium:
				return 2;
			case ShadowResolution.High:
				return 3;
			}
		}
		return 0;
	}

	private void UpdateElement()
	{
		if (m_OptionItem != null)
		{
			m_OptionItem.SetValue(m_CurrentIndex);
		}
		if (!(m_UIText == null))
		{
			m_UIText.SetLocalisedTextCatchAll(m_LookupTable[m_CurrentIndex].m_LocalisationTag);
		}
	}

	public void ResetToDefault()
	{
		m_CurrentIndex = 3;
		UpdateElement();
	}

	public void SelectPrevious()
	{
		m_CurrentIndex--;
		if (m_CurrentIndex < 0)
		{
			m_CurrentIndex = m_LookupTable.Length - 1;
		}
		UpdateElement();
	}

	public void SelectNext()
	{
		m_CurrentIndex++;
		if (m_CurrentIndex >= m_LookupTable.Length)
		{
			m_CurrentIndex = 0;
		}
		UpdateElement();
	}
}
