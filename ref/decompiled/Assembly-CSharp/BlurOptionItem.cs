using UnityEngine;

public class BlurOptionItem : ToggleOptionItem
{
	public BlurOptionItem(MonoBehaviour theUIObject, float defaultValue)
		: base(theUIObject, defaultValue)
	{
	}

	public override void Initialise()
	{
		bool flag = false;
		if (CameraManager.GetInstance() != null)
		{
			flag = true;
			if (CameraManager.GetInstance().GetBlurEffectEnabled())
			{
				m_InitialValue = 1f;
			}
			else
			{
				m_InitialValue = 0f;
			}
		}
		m_SaveKey = "Settings:Blur";
		if (!flag)
		{
			m_InitialValue = GetValueFromGlobalSave(1f);
		}
		base.Initialise();
	}

	public override void OnApply()
	{
		base.OnApply();
		if (CameraManager.GetInstance() != null)
		{
			if (m_CurrentValue < 0.5f)
			{
				CameraManager.GetInstance().SetBlurEffectEnabled(bEnable: false);
			}
			else
			{
				CameraManager.GetInstance().SetBlurEffectEnabled(bEnable: true);
			}
		}
	}
}
