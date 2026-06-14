using UnityEngine;

public class ToggleOptionItem : BaseOptionItem
{
	public ToggleOptionItem(MonoBehaviour theUIObject, float defaultValue)
		: base(theUIObject, defaultValue)
	{
	}

	public override void OnValueChanged()
	{
		T17Toggle t17Toggle = m_theUIObject as T17Toggle;
		if (t17Toggle != null)
		{
			float num = ((!t17Toggle.isOn) ? 0f : 1f);
			if (m_CurrentValue != num)
			{
				m_CurrentValue = num;
				base.OnValueChanged();
			}
		}
	}

	protected override void SyncUIObject(bool bForce = false)
	{
		T17Toggle t17Toggle = m_theUIObject as T17Toggle;
		if (t17Toggle != null)
		{
			t17Toggle.isOn = m_CurrentValue != 0f;
			base.SyncUIObject(bForce);
		}
	}
}
