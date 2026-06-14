using System.Collections;
using Peter.UnityEngine.UI;
using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("T17_UI/InputField", 34)]
public class T17InputField : InputFieldFixedCaret, IT17EventHelper
{
	private enum State
	{
		Unassigned,
		Idle,
		CountingDownStartInputMaps,
		BeingUsed,
		CountingDownReleaseControllerMaps
	}

	public delegate void SelectedHandler();

	public delegate void PointerHandler();

	public delegate void SelectionHandler();

	private State m_State;

	private GameObject m_LastSelectedGO;

	private string m_PreEditText;

	private Rewired.Player m_CachedRewiredPlayer;

	private PlayerMapsSnapshot m_GamerMapsSnapshot;

	public bool m_bClearOnSubmit;

	public bool m_bSelectAllOnFocus = true;

	public bool m_bMoveCaretToEndOnFocus;

	public float m_DelayBeforeRestoringUIControls = 0.5f;

	private float m_RestoreUIControlCountdown;

	public float m_DelayBeforeCanCanelUI = 0.5f;

	public SelectedHandler FieldSelectedEvent;

	public PointerHandler OnInputFieldPointerEnter;

	public PointerHandler OnInputFieldPointerExit;

	public PointerHandler OnInputFieldSelect;

	public PointerHandler OnInputFieldDeselect;

	public bool m_bCanSelect = true;

	public bool m_bSelectOnEventSystemReady;

	private T17EventSystem m_EventSystem;

	protected override BaseInput input
	{
		get
		{
			if ((bool)m_EventSystem && (bool)m_EventSystem.currentInputModule)
			{
				return m_EventSystem.currentInputModule.input;
			}
			return null;
		}
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
		m_CachedRewiredPlayer = gamer.m_RewiredPlayer;
	}

	protected override void Awake()
	{
		base.Awake();
		m_State = State.Idle;
	}

	protected override void LateUpdate()
	{
		EventSystem current = EventSystem.current;
		EventSystem.current = m_EventSystem;
		base.LateUpdate();
		EventSystem.current = current;
	}

	protected void Update()
	{
		bool flag = false;
		if (m_bSelectOnEventSystemReady && !m_EventSystem.alreadySelecting)
		{
			UserSelected(bForceSelection: true);
			m_bSelectOnEventSystemReady = false;
		}
		bool flag2 = false;
		if (m_CachedRewiredPlayer != null)
		{
			flag2 = m_CachedRewiredPlayer.GetButtonDown("Accept");
		}
		flag = m_State == State.BeingUsed || m_State == State.CountingDownStartInputMaps;
		if (m_EventSystem != null && m_EventSystem.currentSelectedGameObject == base.gameObject)
		{
			string text = base.text;
			Platform.OSKGetTextResponses oSKGetTextResponses = Platform.OSKGetTextResponses.Undefined;
			if (m_CachedRewiredPlayer.GetButtonDown("Cancel"))
			{
				oSKGetTextResponses = ((base.lineType != 0) ? Platform.OSKGetTextResponses.Accepted : Platform.OSKGetTextResponses.Cancelled);
			}
			else if (flag2 && base.lineType == InputField.LineType.SingleLine)
			{
				base.onConfirmValue.Invoke(base.text);
				oSKGetTextResponses = Platform.OSKGetTextResponses.Accepted;
			}
			switch (oSKGetTextResponses)
			{
			case Platform.OSKGetTextResponses.Accepted:
				if (m_bClearOnSubmit)
				{
					base.text = string.Empty;
				}
				else
				{
					base.text = text;
				}
				break;
			case Platform.OSKGetTextResponses.Cancelled:
				base.text = m_PreEditText;
				break;
			}
			if (oSKGetTextResponses == Platform.OSKGetTextResponses.Accepted || oSKGetTextResponses == Platform.OSKGetTextResponses.Cancelled)
			{
				base.onValueChanged.Invoke(base.text);
				ReleaseSelectionToLastObject();
			}
		}
		else if (flag)
		{
			base.onValueChanged.Invoke(base.text);
			ReleaseSelectionToLastObject();
			if (flag2 && base.lineType == InputField.LineType.SingleLine)
			{
				base.onConfirmValue.Invoke(base.text);
				if (m_bClearOnSubmit)
				{
					base.text = string.Empty;
				}
			}
			SetGameplayMapsForInputActive(inputFieldActive: false);
			Platform.GetInstance().DismissOSK(m_CachedRewiredPlayer.id);
			m_State = State.Idle;
		}
		else if (m_State == State.CountingDownReleaseControllerMaps)
		{
			m_RestoreUIControlCountdown -= Time.unscaledDeltaTime;
			if (m_RestoreUIControlCountdown <= 0f)
			{
				SetGameplayMapsForInputActive(inputFieldActive: false);
				m_State = State.Idle;
			}
		}
	}

	private void ReleaseSelectionToLastObject()
	{
		if (!(m_EventSystem.currentSelectedGameObject != base.gameObject))
		{
			if (m_LastSelectedGO == base.gameObject)
			{
				m_EventSystem.SetSelectedGameObject(null);
			}
			else
			{
				m_EventSystem.SetSelectedGameObject(m_LastSelectedGO);
			}
			m_State = State.CountingDownReleaseControllerMaps;
			m_RestoreUIControlCountdown = m_DelayBeforeRestoringUIControls;
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		if (m_bCanSelect)
		{
			base.OnSelect(eventData);
			if (FieldSelectedEvent != null)
			{
				FieldSelectedEvent();
			}
		}
	}

	protected override bool SelectAllOnFocus()
	{
		return m_bSelectAllOnFocus;
	}

	protected override bool MoveCaretToEndOnFocus()
	{
		return m_bMoveCaretToEndOnFocus;
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		base.OnDeselect(eventData);
		StartCoroutine(DelayedReleaseSelection());
		if (OnInputFieldDeselect != null)
		{
			OnInputFieldDeselect();
		}
	}

	private IEnumerator DelayedReleaseSelection()
	{
		yield return new WaitForEndOfFrame();
		ReleaseSelectionToLastObject();
	}

	public override void Select()
	{
		if (m_bCanSelect)
		{
			if (m_EventSystem.alreadySelecting)
			{
				m_bSelectOnEventSystemReady = true;
			}
			else
			{
				UserSelected(bForceSelection: true);
			}
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		if (m_EventSystem.currentSelectedGameObject != base.gameObject)
		{
			m_bCanSelect = true;
			EventSystem current = EventSystem.current;
			EventSystem.current = m_EventSystem;
			base.OnPointerDown(eventData);
			EventSystem.current = current;
			UserSelected();
		}
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
			if (OnInputFieldPointerEnter != null)
			{
				OnInputFieldPointerEnter();
			}
			T17RewiredStandaloneInputModule.EventHelperPointerEntered(this);
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		if (T17RewiredStandaloneInputModule.ShouldSelectableAcceptPointer(this))
		{
			base.OnPointerExit(eventData);
			if (IsInteractable() && base.navigation.mode != 0 && m_EventSystem != null)
			{
				m_EventSystem.SetCurrentPointerOverGameobject(null);
			}
			if (OnInputFieldPointerExit != null)
			{
				OnInputFieldPointerExit();
			}
			T17RewiredStandaloneInputModule.EventHelperPointerExited(this);
		}
	}

	protected void UserSelected(bool bForceSelection = false)
	{
		if (m_bCanSelect && (m_State == State.Idle || m_State == State.CountingDownReleaseControllerMaps))
		{
			m_State = State.BeingUsed;
			m_LastSelectedGO = m_EventSystem.currentSelectedGameObject;
			m_EventSystem.SetSelectedGameObject(base.gameObject, bForceSelection);
			SetGameplayMapsForInputActive(inputFieldActive: true);
			m_PreEditText = base.text;
			if (m_CachedRewiredPlayer != null)
			{
				T17EventSystem.ApplyCategories(m_CachedRewiredPlayer, T17EventSystem.InputCateogryStates.InputField);
			}
		}
	}

	private void SetGameplayMapsForInputActive(bool inputFieldActive)
	{
		Gamer[] localGamers = Gamer.GetLocalGamers();
		for (int i = 0; i < localGamers.Length; i++)
		{
			if (localGamers[i] == null || localGamers[i].m_RewiredPlayer != m_CachedRewiredPlayer)
			{
				continue;
			}
			if (inputFieldActive)
			{
				if (T17EventSystem.GetStateForRewiredPlayer(localGamers[i].m_RewiredPlayer) != T17EventSystem.InputCateogryStates.InputField)
				{
					m_GamerMapsSnapshot = PlayerMapsSnapshot.CreateSnapshotForGamer(localGamers[i], disableAllMaps: true);
				}
			}
			else if (m_GamerMapsSnapshot != null)
			{
				m_GamerMapsSnapshot.RestoreControllerMaps();
			}
			break;
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

	public bool CanReselectOnMouseDisable()
	{
		return false;
	}

	public bool ReleaseSelectionOnPointerClickOrExit()
	{
		return false;
	}
}
