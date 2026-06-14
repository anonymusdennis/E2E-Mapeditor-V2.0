public class DLCCarousel : UICarousel<DLCFrontendData>
{
	public T17Image m_PreviewImage;

	public T17Text m_DLCTitle;

	public T17Text m_DLCDescription;

	public T17Text m_DLCCount;

	protected override void UpdateUIForSelectedIndex(int index)
	{
		DLCFrontendData dLCFrontendData = m_Options[index];
		if (m_DLCCount != null)
		{
			string text = index + 1 + "/" + GetNumOptions();
			m_DLCCount.SetNonLocalizedText(text);
		}
		if (dLCFrontendData != null)
		{
			if (DLCBreadcrumbManager.m_Instance != null)
			{
				DLCBreadcrumbManager.m_Instance.SetSeenDLC(dLCFrontendData);
			}
			if (m_PreviewImage != null && dLCFrontendData.m_PreviewImage != null)
			{
				m_PreviewImage.sprite = dLCFrontendData.m_PreviewImage;
			}
			if (m_DLCTitle != null)
			{
				m_DLCTitle.SetLocalisedTextCatchAll(dLCFrontendData.m_NameLocalizationKey);
			}
			if (m_DLCDescription != null)
			{
				m_DLCDescription.SetLocalisedTextCatchAll(dLCFrontendData.m_DescriptionLocalizationKey);
			}
		}
	}
}
