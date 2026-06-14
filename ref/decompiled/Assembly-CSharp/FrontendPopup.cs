using UnityEngine;

public class FrontendPopup : FrontendMenuBehaviour
{
	public T17Button m_OkButton;

	private GameObject m_PreviouslySelected;

	private Gamer m_Gamer;

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		if (currentGamer != null)
		{
			T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(currentGamer);
			if (eventSystemForGamer != null)
			{
				m_PreviouslySelected = eventSystemForGamer.currentSelectedGameObject;
				if (m_OkButton != null)
				{
					eventSystemForGamer.SetSelectedGameObject(m_OkButton.gameObject);
				}
				m_Gamer = currentGamer;
				base.gameObject.SetActive(value: true);
			}
		}
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		Gamer currentGamer = base.CurrentGamer;
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		if (m_Gamer != null)
		{
			T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Gamer);
			if (eventSystemForGamer != null)
			{
				eventSystemForGamer.SetSelectedGameObject(m_PreviouslySelected);
				m_PreviouslySelected = null;
				m_Gamer = null;
			}
		}
		base.gameObject.SetActive(value: false);
		return false;
	}

	public void OnOKClicked()
	{
		Hide(restoreInvokerState: false);
	}
}
