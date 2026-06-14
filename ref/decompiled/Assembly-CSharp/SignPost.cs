using UnityEngine;

public class SignPost : GameMenuBehaviour
{
	public T17Text m_TitleText;

	public T17Text m_BodyText;

	public T17Image m_Image;

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

	public void SetUIElements(string title, string body, Sprite imageSprite)
	{
		if (m_TitleText != null)
		{
			m_TitleText.SetLocalisedTextCatchAll(title);
		}
		if (m_BodyText != null)
		{
			m_BodyText.SetLocalisedTextCatchAll(body);
		}
		if (m_Image != null)
		{
			m_Image.sprite = imageSprite;
		}
	}
}
