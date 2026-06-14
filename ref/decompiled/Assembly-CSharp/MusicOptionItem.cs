using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class MusicOptionItem : BaseOptionItem
{
	public MusicOptionItem(MonoBehaviour theUIObject, float defaultValue)
		: base(theUIObject, defaultValue)
	{
	}

	public override void Initialise()
	{
		m_InitialValue = AudioController.GetCurrentMusicVolume();
		m_SaveKey = "Audio:MusicVol";
		base.Initialise();
	}

	public override void OnValueChanged()
	{
		T17StatsSlider t17StatsSlider = m_theUIObject as T17StatsSlider;
		if (t17StatsSlider != null && m_CurrentValue != (float)t17StatsSlider.currentValue)
		{
			m_CurrentValue = t17StatsSlider.currentValue;
			AudioController.SetParameter(Game_Parameter.Music_Volume, t17StatsSlider.currentValuePercent);
			base.OnValueChanged();
		}
	}

	protected override void SyncUIObject(bool bForce = false)
	{
		T17StatsSlider t17StatsSlider = m_theUIObject as T17StatsSlider;
		if (m_theUIObject != null && (m_CurrentValue != (float)t17StatsSlider.currentValue || bForce))
		{
			t17StatsSlider.SetValue((int)m_CurrentValue);
			AudioController.SetParameter(Game_Parameter.Music_Volume, t17StatsSlider.currentValuePercent);
			base.SyncUIObject(bForce);
		}
	}
}
