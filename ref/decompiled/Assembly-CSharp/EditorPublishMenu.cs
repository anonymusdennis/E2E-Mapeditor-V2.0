using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EditorPublishMenu : MonoBehaviour
{
	private class PublishAsItem
	{
		public enum PublishAsType
		{
			eNew,
			eOverride
		}

		public PublishAsType m_PublishType;

		public string m_strTitle = "Text.Editor.PublishAsNew";

		public ulong m_PublishID;

		public PublishAsItem()
		{
			m_PublishType = PublishAsType.eNew;
			m_strTitle = "Text.Editor.PublishAsNew";
			m_PublishID = 0uL;
		}

		public PublishAsItem(PublishAsType type, string strTitle, ulong publishID)
		{
			m_PublishType = type;
			m_strTitle = strTitle;
			m_PublishID = publishID;
		}
	}

	public T17InputField m_PrisonNameInput;

	public T17InputField m_PrisonDescriptionInput;

	public T17Text m_PublishAsText;

	public T17Text m_VisibilityText;

	private const string m_strUploadDirectory = "TempUpload";

	private int m_UploadID = -1;

	private T17DialogBox m_UploadDialog;

	private Platform.UGCUploadData.UGCVisibility m_UGCVisibility;

	private string[] m_VisibilityStrings = new string[3] { "Text.Editor.Visibility.Public", "Text.Editor.Visibility.FriendsOnly", "Text.Editor.Visibility.Hidden" };

	private const string m_PublishAsNewString = "Text.Editor.PublishAsNew";

	private bool m_bIsUploading;

	private List<PublishAsItem> m_PublishAsItems = new List<PublishAsItem>();

	private int m_PublishAsIndex;

	private void Awake()
	{
		if (Platform.GetInstance() != null)
		{
			Platform.GetInstance().RegisterUGCUploadCallback(OnUploadCallback);
		}
	}

	public void ShowPublishDialog()
	{
		base.gameObject.SetActive(value: true);
		Gamer primaryGamer = Gamer.GetPrimaryGamer();
		if (primaryGamer != null && primaryGamer.m_RewiredPlayer != null)
		{
			T17EventSystem.InputCateogryStates stateForRewiredPlayer = T17EventSystem.GetStateForRewiredPlayer(primaryGamer.m_RewiredPlayer);
			if (stateForRewiredPlayer != T17EventSystem.InputCateogryStates.Dialogbox)
			{
				T17EventSystem.ApplyCategories(primaryGamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.Dialogbox);
			}
		}
		LevelDetailsManager instance = LevelDetailsManager.GetInstance();
		if (instance != null)
		{
			m_PrisonNameInput.text = instance.GetLevelName();
			m_PrisonDescriptionInput.text = instance.GetLevelDecription();
		}
		m_UGCVisibility = Platform.UGCUploadData.UGCVisibility.ePublic;
		if (m_VisibilityText != null)
		{
			m_VisibilityText.SetLocalisedTextCatchAll(m_VisibilityStrings[(int)m_UGCVisibility]);
		}
		m_PublishAsItems.Clear();
		m_PublishAsItems.Add(new PublishAsItem(PublishAsItem.PublishAsType.eNew, "Text.Editor.PublishAsNew", 0uL));
		m_PublishAsIndex = 0;
		if (m_PublishAsText != null)
		{
			m_PublishAsText.SetLocalisedTextCatchAll(m_PublishAsItems[0].m_strTitle);
		}
		if ((bool)Platform.GetInstance())
		{
			Platform.GetInstance().EnumeratePublishedUGCItems(Platform.UGCType.eCustomLevel, OnPublishedItemsRecieved);
		}
	}

	public void HidePublishDialog()
	{
		Gamer primaryGamer = Gamer.GetPrimaryGamer();
		if (primaryGamer != null && primaryGamer.m_RewiredPlayer != null)
		{
			T17EventSystem.ApplyLastRequestedStateSinceDialogboxWasUp(primaryGamer.m_RewiredPlayer);
			T17EventSystem.InputCateogryStates stateForRewiredPlayer = T17EventSystem.GetStateForRewiredPlayer(primaryGamer.m_RewiredPlayer);
			if (stateForRewiredPlayer != T17EventSystem.InputCateogryStates.LevelEditor)
			{
				T17EventSystem.ApplyCategories(primaryGamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.LevelEditor);
			}
		}
		m_bIsUploading = false;
		base.gameObject.SetActive(value: false);
	}

	public void OnPublishButtonClicked()
	{
		if (!(m_PrisonDescriptionInput != null) || !(m_PrisonNameInput != null) || !(Platform.GetInstance() != null))
		{
			return;
		}
		m_bIsUploading = true;
		LevelDetailsManager instance = LevelDetailsManager.GetInstance();
		if (!(instance != null))
		{
			return;
		}
		LevelEditor_Controller instance2 = LevelEditor_Controller.GetInstance();
		if (instance != null && instance2 != null)
		{
			if (string.Compare(m_PrisonDescriptionInput.text, instance.GetLevelName()) != 0 || string.Compare(m_PrisonNameInput.text, instance.GetLevelDecription()) != 0)
			{
				instance.SetLevelName(m_PrisonNameInput.text);
				instance.SetLevelDecription(m_PrisonDescriptionInput.text);
			}
			instance2.SaveTheLevel(bForceNew: false, OnSaveCompleted);
		}
	}

	private void OnSaveCompleted(LevelDetailsManager.RequestResultEnum eResult)
	{
		if (eResult == LevelDetailsManager.RequestResultEnum.Success)
		{
			PublishPrison();
		}
	}

	private bool ValidatePrisonName()
	{
		return true;
	}

	private void PublishPrison()
	{
		if (!ValidatePrisonName())
		{
			T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
			if (dialog != null)
			{
				dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Editor.InvalidTitle", "Text.Edit.TitleIsInvalid", "Text.Dialog.Prompt.Ok", string.Empty, string.Empty, T17DialogBox.Symbols.Error);
				dialog.Show();
				dialog.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnConfirm, new T17DialogBox.DialogEvent(ResetUploading));
			}
			return;
		}
		LevelDetailsManager instance = LevelDetailsManager.GetInstance();
		if (instance != null)
		{
			string path = PlatformIO.GetInstance().GetPath("ESC2U" + instance.GetLevelDirectory());
			char seperationCharacter = PlatformIO.GetInstance().GetSeperationCharacter();
			string text = path + seperationCharacter + "TempUpload";
			if (Directory.Exists(text))
			{
				Directory.Delete(text, recursive: true);
			}
			Directory.CreateDirectory(text);
			File.Copy(path + PlatformIO.GetInstance().GetSeperationCharacter() + "Level_Finished.dat", text + seperationCharacter + "Level_Finished.dat");
			File.Copy(path + PlatformIO.GetInstance().GetSeperationCharacter() + "Level_snap.png", text + seperationCharacter + "Level_snap.png");
			Platform.UGCUploadData uGCUploadData = new Platform.UGCUploadData();
			uGCUploadData.m_strName = m_PrisonNameInput.text;
			uGCUploadData.m_strDescription = m_PrisonDescriptionInput.text;
			uGCUploadData.m_ugcType = Platform.UGCType.eCustomLevel;
			uGCUploadData.m_eVisibility = m_UGCVisibility;
			uGCUploadData.m_strPreviewPath = path + seperationCharacter + "Level_Snap.png";
			uGCUploadData.m_strContentPath = text;
			uGCUploadData.m_PublishID = m_PublishAsItems[m_PublishAsIndex].m_PublishID;
			m_UploadID = Platform.GetInstance().UploadUGCItem(uGCUploadData);
			m_UploadDialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
			if (m_UploadDialog != null)
			{
				m_UploadDialog.InitializeSpinner(hasCancelButton: false, "Text.Editor.UploadingTitle", "Text.Editor.UploadingNotStarted", string.Empty);
				m_UploadDialog.Show();
			}
		}
	}

	private void OnUploadCallback(Platform.UGCUploadState uploadState)
	{
		if (m_UploadID == -1 || m_UploadID != uploadState.m_UploadID)
		{
			return;
		}
		if (uploadState.m_bCompleted)
		{
			if (m_UploadDialog != null)
			{
				m_UploadDialog.Hide();
				m_UploadDialog = null;
			}
			GoogleAnalyticsV3.LogCommericalAnalyticEvent("Custom Prison Published", "Custom Prison Published", string.Empty, 0L);
			m_UploadID = -1;
			T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
			if (uploadState.m_bError)
			{
				if (dialog != null)
				{
					dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Editor.UploadFailedTitle", "Text.Edit.UploadFailed", "Text.Dialog.Prompt.Ok", string.Empty, string.Empty, T17DialogBox.Symbols.Error);
					dialog.Show();
					dialog.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnConfirm, new T17DialogBox.DialogEvent(ResetUploading));
				}
			}
			else
			{
				if (!(dialog != null))
				{
					return;
				}
				dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Editor.UploadCompleteTitle", "Text.Edit.UploadComplete", "Text.Dialog.Prompt.Ok", string.Empty, string.Empty, T17DialogBox.Symbols.Unassigned);
				dialog.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnConfirm, new T17DialogBox.DialogEvent(ClosePublishDialog));
				dialog.Show();
				LevelDetailsManager instance = LevelDetailsManager.GetInstance();
				if (instance != null)
				{
					instance.SetLastUploadedToID(uploadState.m_ulFinalPublishedID);
					LevelEditor_Controller instance2 = LevelEditor_Controller.GetInstance();
					if (instance2 != null)
					{
						instance2.SaveTheLevel(bForceNew: false);
					}
				}
			}
		}
		else if (m_UploadDialog != null)
		{
			if (Localization.Get("Text.Editor.Uploading", out var localized))
			{
				string text = localized.Replace("$BytesProcessed", uploadState.m_ulBytesProcessed.ToString());
				string strMessage = text.Replace("$BytesTotal", uploadState.m_ulBytesTotal.ToString());
				m_UploadDialog.SetMessage(strMessage, bLocalizeMessage: false);
			}
			else
			{
				m_UploadDialog.SetMessage("Uploading " + uploadState.m_ulBytesProcessed + " / " + uploadState.m_ulBytesTotal, bLocalizeMessage: false);
			}
		}
	}

	private void ClosePublishDialog(T17DialogBox dialog)
	{
		dialog.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Remove(dialog.OnConfirm, new T17DialogBox.DialogEvent(ClosePublishDialog));
		HidePublishDialog();
		LevelDetailsManager instance = LevelDetailsManager.GetInstance();
		if (instance != null)
		{
			string path = PlatformIO.GetInstance().GetPath("ESC2U" + instance.GetLevelDirectory());
			char seperationCharacter = PlatformIO.GetInstance().GetSeperationCharacter();
			string path2 = path + seperationCharacter + "TempUpload";
			if (Directory.Exists(path2))
			{
				Directory.Delete(path2, recursive: true);
			}
		}
	}

	public void OnShowTermsClicked()
	{
		if ((bool)Platform.GetInstance())
		{
			Platform.GetInstance().RequestShowUGCTerms();
		}
	}

	public void PublishAsClicked(int iDirection)
	{
		m_PublishAsIndex += iDirection;
		if (m_PublishAsIndex < 0)
		{
			m_PublishAsIndex = m_PublishAsItems.Count - 1;
		}
		else if (m_PublishAsIndex >= m_PublishAsItems.Count)
		{
			m_PublishAsIndex = 0;
		}
		if (m_PublishAsText != null)
		{
			if (m_PublishAsItems[m_PublishAsIndex].m_PublishType == PublishAsItem.PublishAsType.eNew)
			{
				m_PublishAsText.SetLocalisedTextCatchAll(m_PublishAsItems[m_PublishAsIndex].m_strTitle);
				return;
			}
			m_PublishAsText.m_bNeedsLocalization = false;
			m_PublishAsText.text = m_PublishAsItems[m_PublishAsIndex].m_strTitle;
		}
	}

	public void VisibilityClicked(int iDirection)
	{
		m_UGCVisibility += iDirection;
		if (m_UGCVisibility > Platform.UGCUploadData.UGCVisibility.eHidden)
		{
			m_UGCVisibility = Platform.UGCUploadData.UGCVisibility.ePublic;
		}
		else if (m_UGCVisibility < Platform.UGCUploadData.UGCVisibility.ePublic)
		{
			m_UGCVisibility = Platform.UGCUploadData.UGCVisibility.eHidden;
		}
		if (m_VisibilityText != null)
		{
			m_VisibilityText.SetLocalisedTextCatchAll(m_VisibilityStrings[(int)m_UGCVisibility]);
		}
	}

	private void OnPublishedItemsRecieved(List<Platform.UGCItem> publishedItems)
	{
		m_PublishAsItems.Clear();
		m_PublishAsItems.Add(new PublishAsItem(PublishAsItem.PublishAsType.eNew, "Text.Editor.PublishAsNew", 0uL));
		for (int i = 0; i < publishedItems.Count; i++)
		{
			PublishAsItem publishAsItem = new PublishAsItem();
			publishAsItem.m_PublishID = publishedItems[i].m_ID;
			publishAsItem.m_strTitle = publishedItems[i].m_AbsolutePath;
			publishAsItem.m_PublishType = PublishAsItem.PublishAsType.eOverride;
			m_PublishAsItems.Add(publishAsItem);
		}
		m_PublishAsIndex = 0;
		if (m_PublishAsText != null)
		{
			m_PublishAsText.SetLocalisedTextCatchAll(m_PublishAsItems[0].m_strTitle);
		}
		LevelDetailsManager instance = LevelDetailsManager.GetInstance();
		if (!(instance != null))
		{
			return;
		}
		ulong lastUploadedToID = instance.GetLastUploadedToID();
		if (lastUploadedToID == 0)
		{
			return;
		}
		for (int j = 0; j < m_PublishAsItems.Count; j++)
		{
			PublishAsItem publishAsItem2 = m_PublishAsItems[j];
			if (publishAsItem2 == null || publishAsItem2.m_PublishID != lastUploadedToID)
			{
				continue;
			}
			m_PublishAsIndex = j;
			if (m_PublishAsText != null)
			{
				if (m_PublishAsItems[m_PublishAsIndex].m_PublishType == PublishAsItem.PublishAsType.eNew)
				{
					m_PublishAsText.SetLocalisedTextCatchAll(m_PublishAsItems[m_PublishAsIndex].m_strTitle);
					continue;
				}
				m_PublishAsText.m_bNeedsLocalization = false;
				m_PublishAsText.text = m_PublishAsItems[m_PublishAsIndex].m_strTitle;
			}
		}
	}

	public bool IsBusy()
	{
		return m_bIsUploading;
	}

	private void ResetUploading(T17DialogBox dialogbox)
	{
		m_bIsUploading = false;
	}
}
