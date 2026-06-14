using UnityEngine;

public class PCControlTypeOptionSelector : MonoBehaviour
{
	public T17Text m_UIText;

	private PCControlOptionItem m_OptionItem;

	private int m_CurrentIndex;

	public T17Button m_FirstFocus;

	private static ControlOption[] m_LookupTable = new ControlOption[2]
	{
		new ControlOption(ControlSetting.KeyboardAndPad, "Text.UI.KeyboardAndPad"),
		new ControlOption(ControlSetting.Keyboard, "Text.UI.Keyboard")
	};

	public ControlSetting GetCurrentSelectedControlOption()
	{
		return m_LookupTable[m_CurrentIndex].m_EnumValue;
	}

	public void Initialise(PCControlOptionItem optionItem)
	{
		m_OptionItem = optionItem;
		m_CurrentIndex = GetCurrentIndex();
		UpdateElement();
	}

	public int GetCurrentIndex()
	{
		Gamer primaryGamer = Gamer.GetPrimaryGamer();
		if (primaryGamer != null && primaryGamer.m_RewiredPlayer != null)
		{
			if (primaryGamer.m_RewiredPlayer.controllers.joystickCount > 0)
			{
				return 0;
			}
			return 1;
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
		m_CurrentIndex = 0;
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

	public void LoadSavedValue()
	{
		GlobalSave.GetInstance().Get(m_OptionItem.GetSaveKey(), out m_CurrentIndex, 0);
		UpdateElement();
	}
}
