public class DLCPopup : FrontendPopup
{
	public T17Text m_DialogTitle;

	public T17Text m_DLCTitle;

	public T17Text m_DLCDescription;

	public T17Image m_DLCPreviewImage;

	public T17Button m_StoreButton;

	private DLCFrontendData m_DLCData;

	public void ShowDLCPopup(DLCFrontendData dlcData)
	{
		m_DLCData = dlcData;
		if (!(dlcData != null))
		{
			return;
		}
		if (m_DLCTitle != null)
		{
			m_DLCTitle.SetLocalisedTextCatchAll(dlcData.m_NameLocalizationKey);
		}
		if (m_DLCDescription != null)
		{
			m_DLCDescription.SetLocalisedTextCatchAll(dlcData.m_DescriptionLocalizationKey);
		}
		if (m_DLCPreviewImage != null)
		{
			m_DLCPreviewImage.sprite = dlcData.m_PopupImage;
		}
		if (m_StoreButton != null)
		{
			if (dlcData.m_bFreeDLC)
			{
				m_StoreButton.gameObject.SetActive(value: false);
			}
			else
			{
				m_StoreButton.gameObject.SetActive(value: true);
			}
		}
		if (dlcData.m_ForceDialogTitle != string.Empty)
		{
			m_DialogTitle.SetLocalisedTextCatchAll(dlcData.m_ForceDialogTitle);
		}
		else
		{
			m_DialogTitle.SetLocalisedTextCatchAll("Text.Menu.NewDLC");
		}
	}

	public void ShowDLCStorePage()
	{
		if (m_DLCData != null && Platform.GetInstance() != null && !m_DLCData.m_bFreeDLC && !Platform.GetInstance().ShowDLCStorePage(m_DLCData.m_DLCID))
		{
			T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
			if (dialog != null)
			{
				dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Dialog.FailedToOpenStore.Title", "Text.Dialog.FailedToOpenStore.Description", "Text.Dialog.Prompt.Ok", string.Empty, string.Empty, T17DialogBox.Symbols.Error);
				dialog.Show();
			}
		}
	}
}
