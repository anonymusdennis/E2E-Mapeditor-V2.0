using UnityEngine;

public class UIJobTutorialStep : MonoBehaviour
{
	public T17Text m_BodyLabel;

	public T17Image m_DisplayImage;

	public void SetupWithJobStep(JobTutorialStep step, string prefix = "")
	{
		Localization.Get(step.m_BodyText, out var localized);
		localized = prefix + localized;
		m_BodyLabel.m_bNeedsLocalization = false;
		m_BodyLabel.SetNewPlaceHolder(localized);
		m_BodyLabel.text = localized;
		if (m_DisplayImage != null)
		{
			m_DisplayImage.sprite = step.m_Image;
		}
	}
}
