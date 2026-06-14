using UnityEngine;

public class GuardBoard : GameMenuBehaviour
{
	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			if (currentGamer != null)
			{
				T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(currentGamer);
				if (eventSystemForGamer != null && m_TopSelectable != null)
				{
					eventSystemForGamer.SetSelectedGameObject(m_TopSelectable.gameObject);
				}
			}
			return true;
		}
		return false;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (base.CurrentGamer != null)
		{
			T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(base.CurrentGamer);
			if (eventSystemForGamer != null)
			{
				eventSystemForGamer.SetSelectedGameObject(null);
			}
		}
		return base.Hide(restoreInvokerState, isTabSwitch);
	}

	public void Close()
	{
		if (base.CurrentGamePlayer != null)
		{
			base.CurrentGamePlayer.RequestStopInteraction();
		}
	}
}
