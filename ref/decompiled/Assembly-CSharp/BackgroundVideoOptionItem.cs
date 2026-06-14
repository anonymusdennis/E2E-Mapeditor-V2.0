using UnityEngine;

public class BackgroundVideoOptionItem : ToggleOptionItem
{
	public BackgroundVideoOptionItem(MonoBehaviour theUIObject, float defaultValue)
		: base(theUIObject, defaultValue)
	{
	}

	public override void Initialise()
	{
		m_SaveKey = null;
		bool flag = true;
		flag = PlayerPrefs.GetInt("Settings:BackgroundVideoEnabled", 1) == 1;
		m_InitialValue = ((!flag) ? 0f : 1f);
		base.Initialise();
	}

	public override void OnApply()
	{
		base.OnApply();
		bool flag = m_CurrentValue == 1f;
		PlayerPrefs.SetInt("Settings:BackgroundVideoEnabled", flag ? 1 : 0);
		if (FrontEndFlow.Instance != null)
		{
			FrontEndFlow.Instance.ToggleBackgroundVideo(flag);
		}
	}
}
