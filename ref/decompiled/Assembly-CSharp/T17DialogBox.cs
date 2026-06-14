using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;
using UnityEngine.UI;

public class T17DialogBox : T17MonoBehaviour, IT17EventHelper
{
	public delegate void DialogEvent(T17DialogBox dialog = null);

	public enum Symbols
	{
		Unassigned,
		Warning,
		Error,
		Keybinding
	}

	public DialogEvent OnConfirm;

	public DialogEvent OnDecline;

	public DialogEvent OnCancel;

	public DialogEvent OnUpdate;

	public T17Button m_ConfirmButton;

	public T17Button m_DeclineButton;

	public T17Button m_CancelButton;

	public T17Text m_Title;

	public T17Text m_Message;

	public GameObject m_SpinnerObject;

	public T17Image m_ImageObject;

	public T17Text m_ErrorCodeText;

	public bool IsActive;

	private Transform m_ParentToOnHide;

	private Gamer m_Gamer;

	private GameObject m_ObjectSelectedBeforeShow;

	public Sprite m_ErrorSprite;

	public Sprite m_WarningSprite;

	public Sprite m_KeybindingSprite;

	[HideInInspector]
	public T17Text m_LegendTextItem;

	[HideInInspector]
	public string m_LegendLocalisationTag;

	[HideInInspector]
	[SerializeField]
	public List<BaseMenuBehaviour.LegendIconData> m_LegendIconData;

	public T17EventSystem m_EventSystemForGamer;

	protected virtual void OnDestroy()
	{
		OnConfirm = null;
		OnDecline = null;
		OnCancel = null;
		OnUpdate = null;
		m_ConfirmButton = null;
		m_DeclineButton = null;
		m_CancelButton = null;
		m_Title = null;
		m_Message = null;
		m_SpinnerObject = null;
		m_ImageObject = null;
		m_ErrorCodeText = null;
		m_ParentToOnHide = null;
		m_Gamer = null;
		m_ObjectSelectedBeforeShow = null;
		m_LegendTextItem = null;
		m_LegendLocalisationTag = null;
		if (m_LegendIconData != null)
		{
			m_LegendIconData.Clear();
			m_LegendIconData = null;
		}
	}

	public void InitializeSpinner(bool hasCancelButton, string title, string message = "", string mainButtonText = "", bool bLocalizeTitle = true, bool bLocalizeBody = true)
	{
		bool hasConfirm = false;
		bool hasDecline = false;
		bool hasCancel = hasCancelButton;
		string empty = string.Empty;
		string empty2 = string.Empty;
		bool bLocalizeTitle2 = bLocalizeTitle;
		Initialize(hasConfirm, hasDecline, hasCancel, title, message, empty, empty2, mainButtonText, Symbols.Warning, bLocalizeTitle2, bLocalizeBody);
		if (m_SpinnerObject != null)
		{
			m_SpinnerObject.SetActive(value: true);
		}
		if (m_ImageObject != null)
		{
			m_ImageObject.gameObject.SetActive(value: false);
		}
	}

	public void Initialize(bool hasConfirm, bool hasDecline, bool hasCancel, string title, string message, string confirmBtn = "", string declineBtn = "", string cancelBtn = "", Symbols symbol = Symbols.Warning, bool bLocalizeTitle = true, bool bLocalizeMessage = true, ErrorDialogHandler.Type errorCode = ErrorDialogHandler.Type.None)
	{
		if (m_ConfirmButton != null)
		{
			m_ConfirmButton.gameObject.SetActive(hasConfirm);
			Navigation navigation = m_ConfirmButton.navigation;
			if (hasDecline)
			{
				navigation.selectOnRight = m_DeclineButton;
			}
			else if (hasCancel)
			{
				navigation.selectOnRight = m_CancelButton;
			}
			else
			{
				navigation.selectOnRight = null;
			}
			m_ConfirmButton.navigation = navigation;
			if (string.IsNullOrEmpty(confirmBtn))
			{
				confirmBtn = "Text.Dialog.Prompt.Yes";
			}
			SetButtonText(m_ConfirmButton, confirmBtn);
		}
		OnConfirm = null;
		OnUpdate = null;
		if (m_SpinnerObject != null)
		{
			m_SpinnerObject.SetActive(value: false);
		}
		if (m_ImageObject != null)
		{
			m_ImageObject.gameObject.SetActive(value: true);
		}
		if (m_DeclineButton != null)
		{
			m_DeclineButton.gameObject.SetActive(hasDecline);
			Navigation navigation2 = m_DeclineButton.navigation;
			if (hasConfirm)
			{
				navigation2.selectOnLeft = m_ConfirmButton;
			}
			else
			{
				navigation2.selectOnLeft = null;
			}
			if (hasCancel)
			{
				navigation2.selectOnRight = m_CancelButton;
			}
			else
			{
				navigation2.selectOnRight = null;
			}
			m_DeclineButton.navigation = navigation2;
			if (string.IsNullOrEmpty(declineBtn))
			{
				declineBtn = "Text.Dialog.Prompt.No";
			}
			SetButtonText(m_DeclineButton, declineBtn);
		}
		OnDecline = null;
		if (m_CancelButton != null)
		{
			m_CancelButton.gameObject.SetActive(hasCancel);
			Navigation navigation3 = m_CancelButton.navigation;
			if (hasCancel)
			{
				navigation3.selectOnLeft = m_CancelButton;
			}
			else if (hasConfirm)
			{
				navigation3.selectOnLeft = m_ConfirmButton;
			}
			else
			{
				navigation3.selectOnLeft = null;
			}
			m_CancelButton.navigation = navigation3;
			if (string.IsNullOrEmpty(cancelBtn))
			{
				cancelBtn = "Text.Dialog.Prompt.Cancel";
			}
			SetButtonText(m_CancelButton, cancelBtn);
		}
		OnCancel = null;
		if (m_Message != null)
		{
			m_Message.text = message;
			m_Message.m_bNeedsLocalization = bLocalizeMessage;
			m_Message.SetNewPlaceHolder(message);
			m_Message.SetNewLocalizationTag(message);
		}
		if (m_Title != null)
		{
			m_Title.text = title;
			m_Title.m_bNeedsLocalization = bLocalizeTitle;
			m_Title.SetNewPlaceHolder(title);
			m_Title.SetNewLocalizationTag(title);
		}
		if (m_ErrorCodeText != null)
		{
			m_ErrorCodeText.gameObject.SetActive(errorCode != ErrorDialogHandler.Type.None);
			if (errorCode != 0)
			{
				m_ErrorCodeText.SetNonLocalizedText(errorCode.ToString("X"));
			}
		}
		SetSymbol(symbol);
	}

	public void SetSymbol(Symbols symbol)
	{
		if ((!(m_SpinnerObject != null) || !m_SpinnerObject.activeSelf) && m_ImageObject != null)
		{
			m_ImageObject.gameObject.SetActive(value: true);
			switch (symbol)
			{
			case Symbols.Unassigned:
				m_ImageObject.gameObject.SetActive(value: false);
				break;
			case Symbols.Error:
				m_ImageObject.sprite = m_ErrorSprite;
				break;
			case Symbols.Warning:
				m_ImageObject.sprite = m_WarningSprite;
				break;
			case Symbols.Keybinding:
				m_ImageObject.sprite = m_KeybindingSprite;
				break;
			}
		}
	}

	public void SetMessage(string strMessage, bool bLocalizeMessage)
	{
		if (m_Message != null)
		{
			m_Message.text = strMessage;
			m_Message.m_bNeedsLocalization = bLocalizeMessage;
			m_Message.SetNewPlaceHolder(strMessage);
			m_Message.SetNewLocalizationTag(strMessage);
		}
	}

	public void Show()
	{
		T17DialogBoxManager.RequestDialogShow(this, OnAllowedToShow);
	}

	private void OnAllowedToShow()
	{
		base.gameObject.SetActive(value: true);
		IsActive = true;
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Save_Slot_Warning, base.gameObject);
		m_EventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Gamer);
		if (null != m_EventSystemForGamer)
		{
			m_ObjectSelectedBeforeShow = m_EventSystemForGamer.currentSelectedGameObject;
		}
		Focus();
		if (!(m_LegendTextItem != null))
		{
			return;
		}
		m_LegendTextItem.SetNewPlaceHolder(m_LegendLocalisationTag);
		m_LegendTextItem.SetNewLocalizationTag(m_LegendLocalisationTag);
		for (int i = 0; i < m_LegendIconData.Count; i++)
		{
			if (i < m_LegendTextItem.m_ImagesAttached.Count)
			{
				m_LegendTextItem.m_ImagesAttached[i].Size = m_LegendIconData[i].m_Size;
			}
		}
		m_LegendTextItem.Convert();
		m_LegendTextItem.SetVerticesDirty();
	}

	public void Focus()
	{
		if (null != m_EventSystemForGamer)
		{
			if (!base.gameObject.activeInHierarchy || !IsActive)
			{
				Hide();
			}
			else
			{
				if (m_Gamer.m_RewiredPlayer != null)
				{
					T17EventSystem.InputCateogryStates stateForRewiredPlayer = T17EventSystem.GetStateForRewiredPlayer(m_Gamer.m_RewiredPlayer);
					if (stateForRewiredPlayer != T17EventSystem.InputCateogryStates.Dialogbox)
					{
						T17EventSystem.ApplyCategories(m_Gamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.Dialogbox);
					}
				}
				if (m_EventSystemForGamer.currentSelectedGameObject != null && (m_ObjectSelectedBeforeShow == null || (m_ObjectSelectedBeforeShow != null && !m_ObjectSelectedBeforeShow.activeInHierarchy && m_ObjectSelectedBeforeShow != m_EventSystemForGamer.currentSelectedGameObject)))
				{
					m_ObjectSelectedBeforeShow = m_EventSystemForGamer.currentSelectedGameObject;
				}
				if (m_DeclineButton != null && m_DeclineButton.gameObject.activeSelf)
				{
					m_EventSystemForGamer.SetSelectedGameObject(null);
					m_EventSystemForGamer.SetSelectedGameObject(m_DeclineButton.gameObject);
				}
				else if (m_CancelButton != null && m_CancelButton.gameObject.activeSelf)
				{
					m_EventSystemForGamer.SetSelectedGameObject(null);
					m_EventSystemForGamer.SetSelectedGameObject(m_CancelButton.gameObject);
				}
				else if (m_ConfirmButton != null && m_ConfirmButton.gameObject.activeSelf)
				{
					m_EventSystemForGamer.SetSelectedGameObject(null);
					m_EventSystemForGamer.SetSelectedGameObject(m_ConfirmButton.gameObject);
				}
				else
				{
					m_EventSystemForGamer.SetSelectedGameObject(null);
				}
			}
		}
		Debug.Log("**DART** Dialog " + m_Title.text);
	}

	public bool HasFocus()
	{
		bool result = false;
		if (null != m_EventSystemForGamer)
		{
			GameObject currentSelectedGameObject = m_EventSystemForGamer.currentSelectedGameObject;
			result = ((!(null != currentSelectedGameObject)) ? ((m_DeclineButton == null || !m_DeclineButton.gameObject.activeSelf) && (m_CancelButton == null || !m_CancelButton.gameObject.activeSelf) && (m_ConfirmButton == null || !m_ConfirmButton.gameObject.activeSelf)) : (currentSelectedGameObject == m_DeclineButton.gameObject || currentSelectedGameObject == m_CancelButton.gameObject || currentSelectedGameObject == m_ConfirmButton.gameObject));
		}
		if (m_Gamer.m_RewiredPlayer != null && base.gameObject.activeInHierarchy && IsActive)
		{
			T17EventSystem.InputCateogryStates stateForRewiredPlayer = T17EventSystem.GetStateForRewiredPlayer(m_Gamer.m_RewiredPlayer);
			if (stateForRewiredPlayer != T17EventSystem.InputCateogryStates.Dialogbox)
			{
				T17EventSystem.ApplyCategories(m_Gamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.Dialogbox);
			}
		}
		return result;
	}

	public void ReparentToOnHide(Transform parent)
	{
		m_ParentToOnHide = parent;
	}

	public void Hide()
	{
		if (m_Gamer.m_RewiredPlayer != null)
		{
			T17EventSystem.ApplyLastRequestedStateSinceDialogboxWasUp(m_Gamer.m_RewiredPlayer);
		}
		base.gameObject.SetActive(value: false);
		IsActive = false;
		if (m_ParentToOnHide != null)
		{
			if (base.transform.parent.childCount <= 2)
			{
				base.transform.parent.gameObject.SetActive(value: false);
			}
			base.transform.SetParent(m_ParentToOnHide, worldPositionStays: false);
			base.transform.localScale = Vector3.one;
			base.transform.localPosition = Vector3.zero;
		}
		if (m_LegendTextItem != null)
		{
			m_LegendTextItem.SetLocalisedTextCatchAll(string.Empty);
		}
		T17DialogBoxManager.ReleaseMe(this);
		SetSelectedGameobjectToPrepopup();
	}

	public void SetSelectedGameobjectToPrepopup()
	{
		T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Gamer);
		if (eventSystemForGamer != null)
		{
			eventSystemForGamer.SetSelectedGameObject(null);
			eventSystemForGamer.SetSelectedGameObject(m_ObjectSelectedBeforeShow);
			m_ObjectSelectedBeforeShow = null;
		}
	}

	private void SetButtonText(T17Button button, string text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			T17Text componentInChildren = button.GetComponentInChildren<T17Text>(includeInactive: true);
			if (componentInChildren != null)
			{
				componentInChildren.SetNewPlaceHolder(text);
				componentInChildren.SetNewLocalizationTag(text);
			}
		}
	}

	public void Confirm()
	{
		Hide();
		if (OnConfirm != null)
		{
			OnConfirm(this);
		}
	}

	public void Decline()
	{
		Hide();
		if (OnDecline != null)
		{
			OnDecline(this);
		}
	}

	public void Cancel()
	{
		Hide();
		if (OnCancel != null)
		{
			OnCancel(this);
		}
	}

	public void SetGamerForEventSystem(Gamer gamer, T17EventSystem gamersEventSystem)
	{
		m_Gamer = gamer;
		if (m_ConfirmButton != null)
		{
			m_ConfirmButton.SetGamerForEventSystem(m_Gamer, gamersEventSystem);
		}
		if (m_DeclineButton != null)
		{
			m_DeclineButton.SetGamerForEventSystem(m_Gamer, gamersEventSystem);
		}
		if (m_CancelButton != null)
		{
			m_CancelButton.SetGamerForEventSystem(m_Gamer, gamersEventSystem);
		}
	}

	public T17EventSystem GetDomain()
	{
		return null;
	}

	public void SetNewLegendTag(string newTag, bool updatePlaceholderText = true)
	{
		m_LegendLocalisationTag = newTag;
		if (!(m_LegendTextItem != null))
		{
			return;
		}
		if (updatePlaceholderText)
		{
			m_LegendTextItem.SetNewPlaceHolder(newTag);
		}
		m_LegendTextItem.SetNewLocalizationTag(newTag);
		m_LegendTextItem.SetVerticesDirty();
		m_LegendTextItem.UpdateQuadImage();
		if (!m_LegendTextItem.HasIcons)
		{
			return;
		}
		if (m_LegendIconData == null)
		{
			m_LegendIconData = new List<BaseMenuBehaviour.LegendIconData>(m_LegendTextItem.m_ImagesAttached.Count);
		}
		int num = 0;
		for (int i = 0; i < m_LegendTextItem.m_ImagesAttached.Count; i++)
		{
			T17Text.ImageData imageData = m_LegendTextItem.m_ImagesAttached[i];
			if (!imageData.NeedsToBeDeleted)
			{
				if (num >= m_LegendIconData.Count)
				{
					m_LegendIconData.Add(new BaseMenuBehaviour.LegendIconData(imageData.Size, imageData.Offset.x, imageData.Offset.y));
				}
				else
				{
					m_LegendTextItem.m_ImagesAttached[i].Size = m_LegendIconData[num].m_Size;
				}
				num++;
			}
		}
		if (m_LegendIconData.Count > 0 && num < m_LegendIconData.Count)
		{
			m_LegendIconData.RemoveRange(num, m_LegendIconData.Count - num);
		}
		m_LegendTextItem.Convert();
		m_LegendTextItem.SetVerticesDirty();
	}

	public void SetLegendIconSize(int index, int size)
	{
		if (index < m_LegendIconData.Count)
		{
			m_LegendIconData[index].m_Size = size;
			if (m_LegendTextItem != null && index < m_LegendTextItem.m_ImagesAttached.Count)
			{
				m_LegendTextItem.m_ImagesAttached[index].Size = size;
				m_LegendTextItem.Convert();
				m_LegendTextItem.SetVerticesDirty();
			}
		}
	}

	public void SetLegendIconOffset(int index, Vector2 offset)
	{
		if (index < m_LegendIconData.Count)
		{
			m_LegendIconData[index].m_Offset = offset;
			if (m_LegendTextItem != null && index < m_LegendTextItem.m_ImagesAttached.Count)
			{
				m_LegendTextItem.m_ImagesAttached[index].Offset = offset;
				m_LegendTextItem.Convert();
				m_LegendTextItem.SetVerticesDirty();
			}
		}
	}

	private void Update()
	{
		if (OnUpdate != null)
		{
			OnUpdate(this);
		}
	}

	public GameObject GetGameobject()
	{
		return base.gameObject;
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
