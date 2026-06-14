using UnityEngine;

public class ProfanityFilterOptionItem : ToggleOptionItem
{
	public ProfanityFilterOptionItem(MonoBehaviour theUIObject, float defaultValue)
		: base(theUIObject, defaultValue)
	{
	}

	public override void Initialise()
	{
		m_SaveKey = "Settings:ProfanityFilter";
		m_InitialValue = GetValueFromGlobalSave(1f);
		base.Initialise();
	}
}
