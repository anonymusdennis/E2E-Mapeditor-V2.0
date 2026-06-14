using UnityEngine.Events;

public class SlotSelectionInGame : SlotSelectionMenu
{
	public UnityEvent m_CloseMethod;

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		bool flag = base.Hide(restoreInvokerState, isTabSwitch);
		if (flag && SaveManager.GetInstance().GetUIMode() == SaveManager.SaveUIMode.GuestSave && !SaveManager.GetInstance().IsSlotValid())
		{
			SaveManager.GetInstance().ResetUIMode();
		}
		return flag;
	}

	public override void Close()
	{
		if (m_CloseMethod != null)
		{
			m_CloseMethod.Invoke();
		}
	}
}
