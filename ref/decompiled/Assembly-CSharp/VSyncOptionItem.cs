using UnityEngine;

public class VSyncOptionItem : ToggleOptionItem
{
	public VSyncOptionItem(MonoBehaviour theUIObject, float defaultValue)
		: base(theUIObject, defaultValue)
	{
	}

	public override void Initialise()
	{
		m_SaveKey = "Settings:VSync";
		m_InitialValue = GetValueFromGlobalSave((QualitySettings.vSyncCount == 0) ? 0f : 1f);
		base.Initialise();
	}

	public override void OnApply()
	{
		base.OnApply();
		QualityManager.SetVsyncCount((!(m_CurrentValue < 0.5f)) ? 1 : 0);
		RenderTargetManager.CheckForLostRTs();
		CameraManager instance = CameraManager.GetInstance();
		if (instance != null)
		{
			instance.VSyncOptionChanged();
		}
	}
}
