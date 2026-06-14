using System;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

[AddComponentMenu("T17_UI/T17BUtton", 30)]
public class T17Button : Button, IT17EventHelper
{
	public delegate void T17ButtonDelegate(T17Button sender);

	public delegate void T17ButtonDeselectDelegate(T17Button sender, BaseEventData eventData);

	private Text m_ButtonText;

	public Func<bool> m_CanUIReselectDelegate;

	public Func<bool> m_ReleaseOnPointerClickDelegate;

	public T17ButtonDelegate OnButtonSelect;

	public T17ButtonDeselectDelegate OnButtonDeselect;

	public T17ButtonDelegate OnButtonPointerEnter;

	public T17ButtonDelegate OnButtonPointerExit;

	public T17ButtonDelegate OnButtonPointerDown;

	[HideInInspector]
	public bool m_bCanInteractWithMouse = true;

	public UnityEvent OnButtonSelectEvent = new UnityEvent();

	public bool m_bPlaySound = true;

	public string m_OnPressSoundEvent = "Play_UI_Select";

	[HideInInspector]
	public UI_AnimationToRenderTexture m_PC_UIAnimToRenderTex_HoverCapture;

	private T17EventSystem m_EventSystem;

	private bool m_bIsBeingReselectedOnClick;

	public override Selectable FindSelectableOnDown()
	{
		Selectable selectable = base.FindSelectableOnDown();
		if (selectable != null && (!selectable.isActiveAndEnabled || !selectable.IsInteractable()))
		{
			return selectable.FindSelectableOnDown();
		}
		return selectable;
	}

	public override Selectable FindSelectableOnLeft()
	{
		Selectable selectable = base.FindSelectableOnLeft();
		if (selectable != null && (!selectable.isActiveAndEnabled || !selectable.IsInteractable()))
		{
			return selectable.FindSelectableOnLeft();
		}
		return selectable;
	}

	public override Selectable FindSelectableOnRight()
	{
		Selectable selectable = base.FindSelectableOnRight();
		if (selectable != null && (!selectable.isActiveAndEnabled || !selectable.IsInteractable()))
		{
			return selectable.FindSelectableOnRight();
		}
		return selectable;
	}

	public override Selectable FindSelectableOnUp()
	{
		Selectable selectable = base.FindSelectableOnUp();
		if (selectable != null && (!selectable.isActiveAndEnabled || !selectable.IsInteractable()))
		{
			return selectable.FindSelectableOnUp();
		}
		return selectable;
	}

	protected override void Start()
	{
		base.Start();
		m_ButtonText = GetComponentInChildren<Text>(includeInactive: true);
		m_bCanInteractWithMouse = true;
	}

	public void SetText(string text)
	{
		if (m_ButtonText != null)
		{
			m_ButtonText.text = text;
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		Debug.Log("**DART** OnSelect " + base.name);
		base.OnSelect(eventData);
		if (OnButtonSelect != null)
		{
			OnButtonSelect(this);
		}
		if (OnButtonSelectEvent != null)
		{
			OnButtonSelectEvent.Invoke();
		}
		if (m_bPlaySound)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Highlight, AudioController.UI_Audio_GO);
		}
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		base.OnDeselect(eventData);
		if (OnButtonDeselect != null)
		{
			OnButtonDeselect(this, eventData);
		}
		if (m_PC_UIAnimToRenderTex_HoverCapture != null)
		{
			m_PC_UIAnimToRenderTex_HoverCapture.StopAnimation();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (m_PC_UIAnimToRenderTex_HoverCapture != null)
		{
			m_PC_UIAnimToRenderTex_HoverCapture.StopAnimation();
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		if (T17RewiredStandaloneInputModule.ShouldSelectableAcceptPointer(this) && m_bCanInteractWithMouse && eventData.button == PointerEventData.InputButton.Left)
		{
			if (IsInteractable() && base.navigation.mode != 0 && IsThereAnEventSystem())
			{
				m_EventSystem.SetSelectedGameObject(base.gameObject, eventData);
			}
			if (OnButtonPointerDown != null)
			{
				OnButtonPointerDown(this);
			}
			base.OnPointerDown(eventData);
		}
	}

	public override void Select()
	{
		if (IsThereAnEventSystem() && !m_EventSystem.alreadySelecting)
		{
			m_EventSystem.SetSelectedGameObject(base.gameObject);
		}
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (!T17RewiredStandaloneInputModule.ShouldSelectableAcceptPointer(this) || !m_bCanInteractWithMouse || !IsActive() || !IsInteractable())
		{
			return;
		}
		OnPress();
		base.OnPointerClick(eventData);
		if (IsThereAnEventSystem() && m_EventSystem.currentSelectedGameObject == base.gameObject)
		{
			m_bIsBeingReselectedOnClick = true;
		}
		if (IsThereAnEventSystem() && m_EventSystem.currentSelectedGameObject != null)
		{
			IT17EventHelper component = m_EventSystem.currentSelectedGameObject.GetComponent<IT17EventHelper>();
			if (component == null || component.ReleaseSelectionOnPointerClickOrExit())
			{
				m_EventSystem.SetSelectedGameObject(null, eventData);
			}
		}
		m_bIsBeingReselectedOnClick = false;
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
		if (!string.IsNullOrEmpty(m_OnPressSoundEvent) && m_bPlaySound)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, m_OnPressSoundEvent, base.gameObject);
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (T17RewiredStandaloneInputModule.ShouldSelectableAcceptPointer(this) && m_bCanInteractWithMouse)
		{
			base.OnPointerEnter(eventData);
			if (IsInteractable() && base.navigation.mode != 0 && IsThereAnEventSystem())
			{
				m_EventSystem.SetCurrentPointerOverGameobject(base.gameObject);
			}
			if (m_PC_UIAnimToRenderTex_HoverCapture != null)
			{
				m_PC_UIAnimToRenderTex_HoverCapture.StartAnimation();
			}
			if (OnButtonPointerEnter != null)
			{
				OnButtonPointerEnter(this);
			}
			T17RewiredStandaloneInputModule.EventHelperPointerEntered(this);
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		if (!T17RewiredStandaloneInputModule.ShouldSelectableAcceptPointer(this) || (!m_bCanInteractWithMouse && !(m_EventSystem.GetCurrentPointerOverGameobject() == this)))
		{
			return;
		}
		base.OnPointerExit(eventData);
		if (OnButtonPointerExit != null)
		{
			OnButtonPointerExit(this);
		}
		T17RewiredStandaloneInputModule.EventHelperPointerExited(this);
		if (IsInteractable() && base.navigation.mode != 0 && IsThereAnEventSystem())
		{
			if (m_EventSystem.currentSelectedGameObject != null)
			{
				IT17EventHelper component = m_EventSystem.currentSelectedGameObject.GetComponent<IT17EventHelper>();
				if (component == null || component.ReleaseSelectionOnPointerClickOrExit())
				{
					m_EventSystem.SetSelectedGameObject(null, eventData);
				}
			}
			m_EventSystem.SetCurrentPointerOverGameobject(null);
		}
		if (m_PC_UIAnimToRenderTex_HoverCapture != null)
		{
			m_PC_UIAnimToRenderTex_HoverCapture.StopAnimation();
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

	public bool CanReselectOnMouseDisable()
	{
		if (m_CanUIReselectDelegate == null)
		{
			return true;
		}
		return m_CanUIReselectDelegate();
	}

	public bool ReleaseSelectionOnPointerClickOrExit()
	{
		if (m_ReleaseOnPointerClickDelegate == null)
		{
			return true;
		}
		return m_ReleaseOnPointerClickDelegate();
	}

	public bool IsThereAnEventSystem()
	{
		if (m_EventSystem != null)
		{
			return true;
		}
		Gamer primaryGamer = Gamer.GetPrimaryGamer();
		if (primaryGamer != null)
		{
			m_EventSystem = T17EventSystemsManager.Instance.GetEventSystemForGamer(primaryGamer);
		}
		if (m_EventSystem != null)
		{
			return true;
		}
		return false;
	}

	public bool IsBeingReselectedViaClick()
	{
		return m_bIsBeingReselectedOnClick;
	}
}
