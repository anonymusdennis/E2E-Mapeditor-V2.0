using UnityEngine;

public class VibrationOptionItem : ToggleOptionItem
{
	public VibrationOptionItem(MonoBehaviour theUIObject, float defaultValue)
		: base(theUIObject, defaultValue)
	{
	}

	public override void Initialise()
	{
		m_InitialValue = ((!Platform.controllerVibrationEnabled) ? 0f : 1f);
		m_SaveKey = "Settings:Vibration";
		base.Initialise();
	}

	public override void OnValueChanged()
	{
		base.OnValueChanged();
		Platform.controllerVibrationEnabled = !(m_CurrentValue < 0.5f);
	}

	protected override void SyncUIObject(bool bForce = false)
	{
		base.SyncUIObject(bForce);
		Platform.controllerVibrationEnabled = !(m_CurrentValue < 0.5f);
	}
}
