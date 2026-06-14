using AUTOGEN_T17Wwise_Enums;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("T17_UI/Toggle", 32)]
public class T17Toggle : Toggle, IT17EventHelper
{
	private T17EventSystem m_EventSystem;

	public void SetGamerForEventSystem(Gamer gamer, T17EventSystem gamersEventSystem)
	{
		if (gamersEventSystem == null)
		{
			m_EventSystem = T17EventSystemsManager.Instance.GetEventSystemForGamer(gamer);
		}
		else
		{
			m_EventSystem = gamersEventSystem;
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			if (IsInteractable() && base.navigation.mode != 0)
			{
				m_EventSystem.SetSelectedGameObject(base.gameObject, eventData);
			}
			base.OnPointerDown(eventData);
		}
	}

	public override void Select()
	{
		if (!m_EventSystem.alreadySelecting)
		{
			m_EventSystem.SetSelectedGameObject(base.gameObject);
		}
	}

	public T17EventSystem GetDomain()
	{
		return m_EventSystem;
	}

	public GameObject GetGameobject()
	{
		return base.gameObject;
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Highlight, AudioController.UI_Audio_GO);
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (!T17RewiredStandaloneInputModule.ShouldSelectableAcceptPointer(this) || !IsActive() || !IsInteractable())
		{
			return;
		}
		OnPress();
		base.OnPointerClick(eventData);
		if (m_EventSystem.currentSelectedGameObject != null)
		{
			T17InputField component = m_EventSystem.currentSelectedGameObject.GetComponent<T17InputField>();
			if (!(component != null) || !component.isFocused)
			{
				m_EventSystem.SetSelectedGameObject(null, eventData);
			}
		}
	}

	public override void OnSubmit(BaseEventData eventData)
	{
		if (T17RewiredStandaloneInputModule.ShouldSelectableAcceptSubmit(this) && IsActive() && IsInteractable())
		{
			OnPress();
			base.OnSubmit(eventData);
		}
	}

	private void OnPress()
	{
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, "Play_UI_Select", base.gameObject);
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (T17RewiredStandaloneInputModule.ShouldSelectableAcceptPointer(this))
		{
			base.OnPointerEnter(eventData);
			if (IsInteractable() && base.navigation.mode != 0 && m_EventSystem != null)
			{
				m_EventSystem.SetCurrentPointerOverGameobject(base.gameObject);
			}
			T17RewiredStandaloneInputModule.EventHelperPointerEntered(this);
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		if (!T17RewiredStandaloneInputModule.ShouldSelectableAcceptPointer(this))
		{
			return;
		}
		base.OnPointerExit(eventData);
		T17RewiredStandaloneInputModule.EventHelperPointerExited(this);
		if (!IsInteractable() || base.navigation.mode == Navigation.Mode.None || !(m_EventSystem != null))
		{
			return;
		}
		if (m_EventSystem.currentSelectedGameObject != null)
		{
			T17InputField component = m_EventSystem.currentSelectedGameObject.GetComponent<T17InputField>();
			if (component == null || !component.isFocused)
			{
				m_EventSystem.SetSelectedGameObject(null, eventData);
			}
		}
		m_EventSystem.SetCurrentPointerOverGameobject(null);
	}

	public bool CanReselectOnMouseDisable()
	{
		return true;
	}

	public bool ReleaseSelectionOnPointerClickOrExit()
	{
		return true;
	}
}
