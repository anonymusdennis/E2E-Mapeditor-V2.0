using UnityEngine;

public class PCControlOptionItem : BaseOptionItem
{
	private PCControlTypeOptionSelector m_OptionSelector;

	public PCControlOptionItem(MonoBehaviour theUIObject)
		: base(theUIObject, 0f)
	{
	}

	public override void Initialise()
	{
		m_SaveKey = "Settings:ControlOption";
		m_OptionSelector = m_theUIObject as PCControlTypeOptionSelector;
		if (m_OptionSelector != null)
		{
			m_OptionSelector.Initialise(this);
			m_OptionSelector.LoadSavedValue();
		}
		m_InitialValue = m_CurrentValue;
		m_bDirty = false;
		base.Initialise();
	}

	public void SetValue(int index)
	{
		m_CurrentValue = index;
		OnValueChanged();
	}

	public override void OnApply()
	{
		base.OnApply();
		if (m_OptionSelector != null)
		{
			T17RewiredStandaloneInputModule.SetPCKeyboardMode((ControlSetting)m_CurrentValue);
		}
	}

	public override void ResetToDefault()
	{
		if (m_OptionSelector != null)
		{
			m_OptionSelector.ResetToDefault();
		}
	}
}
