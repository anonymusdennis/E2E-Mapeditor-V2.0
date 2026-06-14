using UnityEngine;

public class InfluencersOptionItem : ToggleOptionItem
{
	public InfluencersOptionItem(MonoBehaviour theUIObject, float defaultValue)
		: base(theUIObject, defaultValue)
	{
	}

	public override void Initialise()
	{
		m_SaveKey = "Settings:Influencers";
		m_InitialValue = GetValueFromGlobalSave(1f);
		base.Initialise();
	}
}
