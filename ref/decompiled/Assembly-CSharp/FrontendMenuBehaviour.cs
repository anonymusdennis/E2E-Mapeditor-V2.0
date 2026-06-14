using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class FrontendMenuBehaviour : BaseMenuBehaviour
{
	public bool m_bSelectTopElementOnShow = true;

	public T17MenuBody m_MenuBody;

	public string MenuName = "GIVE ME A TITLE!";

	protected GameObject m_ObjectSelectedBeforeShow;

	protected GameObject m_ObjectSelectedBeforeHide;

	protected T17EventSystem m_CurrentEventSystem;

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		m_CurrentEventSystem = T17EventSystemsManager.Instance.GetEventSystemForGamer(currentGamer);
		if (m_CurrentEventSystem != null)
		{
			m_ObjectSelectedBeforeShow = m_CurrentEventSystem.currentSelectedGameObject;
			if (m_ObjectSelectedBeforeHide != null && m_ObjectSelectedBeforeHide.activeInHierarchy)
			{
				m_CurrentEventSystem.SetSelectedGameObject(null);
				m_CurrentEventSystem.SetSelectedGameObject(m_ObjectSelectedBeforeHide);
				m_ObjectSelectedBeforeHide = null;
			}
			else if (m_TopSelectable != null && m_bSelectTopElementOnShow)
			{
				m_CurrentEventSystem.SetSelectedGameObject(null);
				m_CurrentEventSystem.SetSelectedGameObject(m_TopSelectable.gameObject);
			}
		}
		if (m_MenuBody != null && m_MenuBody.m_MenuTitle != null)
		{
			m_MenuBody.m_MenuTitle.SetNewPlaceHolder(MenuName);
			m_MenuBody.m_MenuTitle.SetNewLocalizationTag(MenuName);
		}
		Debug.Log("**DART** Menu " + MenuName);
		return true;
	}

	protected override void Update()
	{
		base.Update();
		if (BaseMenuBehaviour.LastMenuThatCalledShow == this && !T17DialogBoxManager.HasAnyOpenDialogs())
		{
			if (m_CurrentEventSystem == null)
			{
				m_CurrentEventSystem = T17EventSystemsManager.Instance.GetEventSystemForGamer(base.CurrentGamer);
			}
			if (m_CurrentEventSystem != null && (m_CurrentEventSystem.currentSelectedGameObject == null || !m_CurrentEventSystem.currentSelectedGameObject.activeInHierarchy) && m_TopSelectable != null && m_bSelectTopElementOnShow)
			{
				m_CurrentEventSystem.SetSelectedGameObject(null);
				m_CurrentEventSystem.SetSelectedGameObject(m_TopSelectable.gameObject);
			}
		}
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		Gamer currentGamer = base.CurrentGamer;
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		if (currentGamer != null)
		{
			T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(currentGamer);
			if (eventSystemForGamer != null)
			{
				m_ObjectSelectedBeforeHide = eventSystemForGamer.currentSelectedGameObject;
				if (m_ObjectSelectedBeforeShow != null)
				{
					eventSystemForGamer.SetSelectedGameObject(null);
					eventSystemForGamer.SetSelectedGameObject(m_ObjectSelectedBeforeShow);
					m_ObjectSelectedBeforeShow = null;
				}
			}
		}
		if (m_MenuBody != null && m_MenuBody.m_MenuTitle != null && m_Parent != null)
		{
			FrontendMenuBehaviour frontendMenuBehaviour = m_Parent as FrontendMenuBehaviour;
			if (frontendMenuBehaviour != null)
			{
				m_MenuBody.m_MenuTitle.SetNewPlaceHolder(frontendMenuBehaviour.MenuName);
				m_MenuBody.m_MenuTitle.SetNewLocalizationTag(frontendMenuBehaviour.MenuName);
			}
		}
		return true;
	}

	public virtual void Close()
	{
		Hide();
	}

	public virtual GameObject FindValidSelectableForLostFocus()
	{
		if (m_TopSelectable != null && m_TopSelectable.IsInteractable())
		{
			return m_TopSelectable.gameObject;
		}
		return null;
	}

	public void InvokeNavigateOnUICancel()
	{
		if (m_NavigateOnUICancel != null && m_NavigateOnUICancel.m_DoThisOnUICancel != null)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Reject, AudioController.UI_Audio_GO);
			m_NavigateOnUICancel.m_DoThisOnUICancel.Invoke();
		}
	}
}
