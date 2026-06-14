using UnityEngine;

public class ShadowDetailOptionItem : BaseOptionItem
{
	private ShadowDetailOptionSelector m_OptionSelector;

	public ShadowDetailOptionItem(MonoBehaviour theUIObject)
		: base(theUIObject, 0f)
	{
	}

	public override void Initialise()
	{
		m_OptionSelector = m_theUIObject as ShadowDetailOptionSelector;
		if (m_OptionSelector != null)
		{
			m_OptionSelector.Initialise(this);
		}
		m_SaveKey = "Settings:ShadowQuality";
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
		ShadowLevel shadowLevel = ShadowLevel.Off;
		if (m_OptionSelector != null)
		{
			shadowLevel = m_OptionSelector.GetCurrentSelectedLevel();
		}
		if (shadowLevel == ShadowLevel.Off)
		{
			QualitySettings.shadows = ShadowQuality.Disable;
			return;
		}
		QualitySettings.shadowResolution = MapShadowLevelToShadowResolution(shadowLevel);
		QualitySettings.shadows = ShadowQuality.All;
	}

	public override void ResetToDefault()
	{
		if (m_OptionSelector != null)
		{
			m_OptionSelector.ResetToDefault();
		}
	}

	public static ShadowResolution MapShadowLevelToShadowResolution(ShadowLevel level)
	{
		return level switch
		{
			ShadowLevel.Low => ShadowResolution.Low, 
			ShadowLevel.Medium => ShadowResolution.Medium, 
			ShadowLevel.High => ShadowResolution.High, 
			_ => ShadowResolution.Low, 
		};
	}
}
