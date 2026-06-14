using System;
using Rewired;
using UnityEngine;

public class ControlsRemapEntry : MonoBehaviour
{
	public Rewired.Player m_RewiredPlayer;

	public T17Text ControlMapAction;

	public T17Text ControlMapInput;

	public T17Text DialogBodyLHS;

	public T17Text DialogBodyRHS;

	public int m_InputMapDataID;

	public Color32 MissingMapColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	private T17DialogBox m_InputDialog;

	private bool m_bWaitingForKeyUp;

	private bool m_bIsDirty;

	public bool IsDirty => m_bIsDirty;

	protected virtual void Awake()
	{
		if (ControlMapAction == null)
		{
			Transform transform = base.transform.FindChild("Action");
			if (transform != null)
			{
				ControlMapAction = transform.GetComponent<T17Text>();
			}
		}
		if (ControlMapInput == null)
		{
			Transform transform2 = base.transform.FindChild("Input");
			if (transform2 != null)
			{
				ControlMapInput = transform2.GetComponent<T17Text>();
			}
		}
		if (DialogBodyLHS == null)
		{
			Transform transform3 = base.transform.FindChild("DialogBodyLHS");
			if (transform3 != null)
			{
				DialogBodyLHS = transform3.GetComponent<T17Text>();
			}
		}
		if (DialogBodyRHS == null)
		{
			Transform transform4 = base.transform.FindChild("DialogBodyRHS");
			if (transform4 != null)
			{
				DialogBodyRHS = transform4.GetComponent<T17Text>();
			}
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_InputDialog != null)
		{
			T17DialogBox inputDialog = m_InputDialog;
			inputDialog.OnUpdate = (T17DialogBox.DialogEvent)Delegate.Remove(inputDialog.OnUpdate, new T17DialogBox.DialogEvent(UpdatePollingKeyboard));
			T17DialogBox inputDialog2 = m_InputDialog;
			inputDialog2.OnCancel = (T17DialogBox.DialogEvent)Delegate.Remove(inputDialog2.OnCancel, new T17DialogBox.DialogEvent(OnRemapCancelled));
		}
		InputMapData inputMapData = T17ControlMapper.Instance.GetInputMapData(m_RewiredPlayer, m_InputMapDataID);
		if (inputMapData != null)
		{
			inputMapData.OnInformationUpdated = (InputMapData.InputMapDataDelegate)Delegate.Remove(inputMapData.OnInformationUpdated, new InputMapData.InputMapDataDelegate(OnMapDataUpdated));
		}
	}

	public void SetUpMapEntry(Rewired.Player rewiredplayer, int inputMapID)
	{
		m_RewiredPlayer = rewiredplayer;
		m_InputMapDataID = inputMapID;
		InputMapData inputMapData = T17ControlMapper.Instance.GetInputMapData(m_RewiredPlayer, m_InputMapDataID);
		if (inputMapData != null)
		{
			inputMapData.OnInformationUpdated = (InputMapData.InputMapDataDelegate)Delegate.Combine(inputMapData.OnInformationUpdated, new InputMapData.InputMapDataDelegate(OnMapDataUpdated));
		}
		ControlMapAction.SetNewLocalizationTag(("Text.RewiredAction." + inputMapData.m_ActionName + inputMapData.m_AxisSuffix).Replace(" ", string.Empty));
		ControlMapAction.SetVerticesDirty();
		ControlMapInput.SetNonLocalizedText(Localization.GetPCInputTranslation(inputMapData.m_ActionElementMap.elementIdentifierName));
		ControlMapInput.SetVerticesDirty();
		m_bIsDirty = false;
	}

	public void OnEntryPressed(T17Button sender)
	{
		if (!(m_InputDialog != null))
		{
			m_InputDialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
			m_InputDialog.Initialize(hasConfirm: false, hasDecline: false, hasCancel: false, "Text.ControlsRemapping.RemapDialogTitle", DialogBodyLHS.text + ControlMapAction.text + DialogBodyRHS.text, string.Empty, string.Empty, string.Empty, T17DialogBox.Symbols.Keybinding, bLocalizeTitle: true, bLocalizeMessage: false);
			T17DialogBox inputDialog = m_InputDialog;
			inputDialog.OnUpdate = (T17DialogBox.DialogEvent)Delegate.Combine(inputDialog.OnUpdate, new T17DialogBox.DialogEvent(UpdatePollingKeyboard));
			T17DialogBox inputDialog2 = m_InputDialog;
			inputDialog2.OnCancel = (T17DialogBox.DialogEvent)Delegate.Combine(inputDialog2.OnCancel, new T17DialogBox.DialogEvent(OnRemapCancelled));
			m_InputDialog.Show();
		}
	}

	public void OnRemapCancelled(T17DialogBox dialogBox)
	{
		ShutdownDialog(dialogBox);
	}

	private void OnMapDataUpdated(InputMapData mapData)
	{
		if (ControlMapInput != null)
		{
			ControlMapAction.SetNewLocalizationTag(("Text.RewiredAction." + mapData.m_ActionName + mapData.m_AxisSuffix).Replace(" ", string.Empty));
			ControlMapAction.SetVerticesDirty();
			if (mapData.m_ActionElementMap.elementIdentifierName == "None")
			{
				ControlMapInput.SetNonLocalizedText("<color=#" + ColorUtility.ToHtmlStringRGBA(MissingMapColor) + ">" + Localization.GetPCInputTranslation(mapData.m_ActionElementMap.elementIdentifierName) + "</color>");
			}
			else
			{
				ControlMapInput.SetNonLocalizedText(Localization.GetPCInputTranslation(mapData.m_ActionElementMap.elementIdentifierName));
			}
			m_bIsDirty = true;
			ControlMapInput.SetVerticesDirty();
		}
	}

	private void UpdatePollingKeyboard(T17DialogBox dialogBox)
	{
		if (m_RewiredPlayer.GetButtonUp("UI_Close"))
		{
			OnRemapCancelled(dialogBox);
		}
		if (m_bWaitingForKeyUp)
		{
			InputMapData inputMapData = T17ControlMapper.Instance.GetInputMapData(m_RewiredPlayer, m_InputMapDataID);
			if (inputMapData != null && m_RewiredPlayer.controllers.Keyboard.GetKeyUp(inputMapData.m_ActionElementMap.keyCode))
			{
				if (ControlMapInput != null)
				{
					ControlMapInput.SetNonLocalizedText(Localization.GetPCInputTranslation(inputMapData.m_ActionElementMap.elementIdentifierName));
					ControlMapInput.SetVerticesDirty();
				}
				if (dialogBox != null)
				{
					ShutdownDialog(dialogBox);
				}
				m_InputDialog = null;
				m_bWaitingForKeyUp = false;
				m_bIsDirty = true;
			}
		}
		else
		{
			m_bWaitingForKeyUp = T17ControlMapper.Instance.OnKeyboardElementAssignmentPollingUpdate(m_RewiredPlayer, m_InputMapDataID);
		}
	}

	private void ShutdownDialog(T17DialogBox dialogBox)
	{
		T17DialogBox inputDialog = m_InputDialog;
		inputDialog.OnUpdate = (T17DialogBox.DialogEvent)Delegate.Remove(inputDialog.OnUpdate, new T17DialogBox.DialogEvent(UpdatePollingKeyboard));
		T17DialogBox inputDialog2 = m_InputDialog;
		inputDialog2.OnCancel = (T17DialogBox.DialogEvent)Delegate.Remove(inputDialog2.OnCancel, new T17DialogBox.DialogEvent(OnRemapCancelled));
		dialogBox.Hide();
		m_InputDialog = null;
		T17ControlMapper.Instance.GetInputMapData(m_RewiredPlayer, m_InputMapDataID);
	}
}
