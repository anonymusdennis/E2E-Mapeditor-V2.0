using UnityEngine;

public class InformationBoard : GameMenuBehaviour
{
	public T17Text m_TitleLabel;

	public T17Text m_BodyLabel;

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

	public void SetBoardLocalisationTags(string titleTag, string bodyTag)
	{
		if (m_TitleLabel != null)
		{
			m_TitleLabel.m_bNeedsLocalization = true;
			m_TitleLabel.SetLocalisedTextCatchAll(titleTag);
		}
		if (m_BodyLabel != null)
		{
			m_BodyLabel.m_bNeedsLocalization = true;
			m_BodyLabel.SetLocalisedTextCatchAll(bodyTag);
		}
	}
}
