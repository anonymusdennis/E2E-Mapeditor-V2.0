using System;
using System.Collections;
using UnityEngine;

public class ResolutionOptionItem : BaseOptionItem
{
	private T17Toggle m_WindowedToggle;

	private ResolutionSelector m_OptionSelector;

	private T17DialogBox m_confirmBox;

	private Resolution m_LastResolution;

	private bool m_bLastFullScreen;

	private Coroutine m_WaitForResponse_Coroutine;

	public ResolutionOptionItem(MonoBehaviour theUIObject)
		: base(theUIObject, 0f)
	{
	}

	public override void Initialise()
	{
		if (m_theUIObject != null)
		{
			m_OptionSelector = m_theUIObject as ResolutionSelector;
			m_OptionSelector.Initialise(this);
			m_WindowedToggle = m_OptionSelector.m_WindowedToggle;
			if (m_WindowedToggle != null)
			{
				m_WindowedToggle.isOn = !Screen.fullScreen;
			}
		}
		m_SaveKey = null;
		base.Initialise();
	}

	public override void ResetToDefault()
	{
		if (m_OptionSelector != null)
		{
			m_OptionSelector.ResetToDefault();
		}
	}

	public override void OnValueChanged()
	{
		m_bDirty = HasPendingChanges();
	}

	public bool HasPendingChanges()
	{
		if (m_OptionSelector == null)
		{
			return false;
		}
		Resolution currentResolution = GetCurrentResolution();
		Resolution currentSelected = m_OptionSelector.GetCurrentSelected();
		bool flag = Screen.fullScreen;
		if (m_WindowedToggle != null)
		{
			flag = !m_WindowedToggle.isOn;
		}
		return currentSelected.width != currentResolution.width || currentSelected.height != currentResolution.height || flag != Screen.fullScreen;
	}

	private Resolution GetCurrentResolution()
	{
		Resolution currentResolution = Screen.currentResolution;
		currentResolution.width = Screen.width;
		currentResolution.height = Screen.height;
		return currentResolution;
	}

	public override void OnApply()
	{
		if (!(m_confirmBox != null) && !(m_OptionSelector == null) && HasPendingChanges())
		{
			bool bFullscreen = Screen.fullScreen;
			if (m_WindowedToggle != null)
			{
				bFullscreen = !m_WindowedToggle.isOn;
			}
			SetResolution(m_OptionSelector.GetCurrentSelected(), bFullscreen);
		}
	}

	private void SetResolution(Resolution newResolution, bool bFullscreen)
	{
		if (m_OptionSelector != null)
		{
			m_OptionSelector.StartCoroutine(ChangeResolutionTo(newResolution, bFullscreen));
		}
	}

	private IEnumerator ChangeResolutionTo(Resolution newResolution, bool bNewFullscreen)
	{
		m_LastResolution = GetCurrentResolution();
		m_bLastFullScreen = Screen.fullScreen;
		QualityManager.SetResolution(newResolution.width, newResolution.height, bNewFullscreen);
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		OnResolutionChanged(bShowConfirmationBox: true);
	}

	private void OnResolutionChanged(bool bShowConfirmationBox)
	{
		if (bShowConfirmationBox)
		{
			DisplayConfirmationBox();
		}
		if (m_WindowedToggle != null)
		{
			m_WindowedToggle.isOn = !Screen.fullScreen;
		}
		RenderTargetManager.CheckForLostRTs();
		CameraManager instance = CameraManager.GetInstance();
		if (instance != null)
		{
			instance.OnScreenResolutionChanged();
		}
	}

	private void DisplayConfirmationBox()
	{
		if (m_OptionSelector == null)
		{
			return;
		}
		m_confirmBox = T17DialogBoxManager.GetDialog(forSingleUser: false);
		if (m_confirmBox != null)
		{
			m_confirmBox.Initialize(hasConfirm: true, hasDecline: true, hasCancel: false, "Text.UI.ConfirmResolutionTitle", "Text.UI.ConfirmResolutionBody", "Text.Yes", "Text.No", string.Empty);
			T17DialogBox confirmBox = m_confirmBox;
			confirmBox.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(confirmBox.OnConfirm, (T17DialogBox.DialogEvent)delegate
			{
				OnConfirmationResponse(bKeepChanges: true);
			});
			T17DialogBox confirmBox2 = m_confirmBox;
			confirmBox2.OnDecline = (T17DialogBox.DialogEvent)Delegate.Combine(confirmBox2.OnDecline, (T17DialogBox.DialogEvent)delegate
			{
				OnConfirmationResponse(bKeepChanges: false);
			});
			m_confirmBox.Show();
			m_WaitForResponse_Coroutine = m_OptionSelector.StartCoroutine(WaitForResponse());
		}
	}

	private IEnumerator WaitForResponse()
	{
		T17Text dialogueMessage = m_confirmBox.m_Message;
		int timeRemaining = m_OptionSelector.m_ConfirmChangeTime;
		string localisedText = dialogueMessage.text;
		while (timeRemaining != 0)
		{
			dialogueMessage.text = localisedText.Replace("%", timeRemaining.ToString());
			dialogueMessage.m_bNeedsLocalization = false;
			yield return new WaitForSecondsRealtime(1f);
			timeRemaining--;
		}
		m_confirmBox.Hide();
		OnConfirmationResponse(bKeepChanges: false);
	}

	private void OnConfirmationResponse(bool bKeepChanges)
	{
		if (m_OptionSelector == null)
		{
			return;
		}
		m_OptionSelector.StopCoroutine(m_WaitForResponse_Coroutine);
		m_WaitForResponse_Coroutine = null;
		if (!bKeepChanges)
		{
			m_OptionSelector.StartCoroutine(RevertChanges());
			if (m_WindowedToggle != null)
			{
				m_WindowedToggle.isOn = m_bLastFullScreen;
			}
		}
		m_confirmBox = null;
		m_bDirty = false;
		if (m_OptionSelector != null && m_OptionSelector.IsBackingOut())
		{
			FrontEndFlow.Instance.SwitchBackToFrontendMenu(m_OptionSelector.GetPendingMenu());
		}
	}

	private IEnumerator RevertChanges()
	{
		QualityManager.SetResolution(m_LastResolution.width, m_LastResolution.height, m_bLastFullScreen);
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		OnResolutionChanged(bShowConfirmationBox: false);
	}
}
