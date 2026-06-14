using UnityEngine;

public class ProgressBarHUD : MonoBehaviour
{
	public T17Slider m_Slider;

	public T17Image m_SliderFillImage;

	public T17Text m_SliderText;

	private float m_CurrentPercentage;

	public virtual void SetVisible(bool visible)
	{
		base.gameObject.SetActive(visible);
	}

	public void SetText(string localizationTag)
	{
		if (m_SliderText != null)
		{
			m_SliderText.SetNewLocalizationTag(localizationTag);
			m_SliderText.SetNewPlaceHolder(localizationTag);
		}
	}

	public void SetCurrentPercentage(float percentage)
	{
		m_CurrentPercentage = Mathf.Clamp01(percentage);
		if (m_Slider != null)
		{
			m_Slider.value = m_CurrentPercentage;
		}
	}

	public void SetColor(Color color)
	{
		if (m_SliderFillImage != null)
		{
			m_SliderFillImage.color = color;
		}
	}
}
