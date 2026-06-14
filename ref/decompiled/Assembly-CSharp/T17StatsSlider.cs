using UnityEngine;

public class T17StatsSlider : MonoBehaviour
{
	public T17Slider m_VisibleSlider;

	public T17Text m_ValueText;

	public T17Image m_FillImage;

	public int m_MinValue;

	public int m_MaxValue = 100;

	public int m_StartValue;

	public bool m_bNeedsColorChange;

	public Color m_ColorOnMaxValue = Color.green;

	public Color m_ColorOnHalfValue = Color.yellow;

	public Color m_ColorOnMinValue = Color.red;

	private int m_CurrentValue;

	private bool m_bShouldSetStartValue = true;

	public int currentValue => m_CurrentValue;

	public float currentValuePercent => (!(m_VisibleSlider != null)) ? 0f : m_VisibleSlider.value;

	public void Start()
	{
		if (m_bShouldSetStartValue)
		{
			SetValue(m_StartValue);
		}
	}

	public void SetValue(int value)
	{
		int num = m_CurrentValue;
		m_CurrentValue = Mathf.Clamp(value, m_MinValue, m_MaxValue);
		if (m_CurrentValue == num && !m_bShouldSetStartValue)
		{
			return;
		}
		float num2 = (float)m_CurrentValue / (float)m_MaxValue;
		if (m_VisibleSlider != null)
		{
			m_VisibleSlider.value = num2;
		}
		if (m_ValueText != null)
		{
			m_ValueText.text = m_CurrentValue.ToString();
		}
		if (m_bNeedsColorChange)
		{
			Color colorOnMaxValue = m_ColorOnMaxValue;
			colorOnMaxValue = ((!(num2 <= 0.5f)) ? Color.Lerp(m_ColorOnHalfValue, m_ColorOnMaxValue, (num2 - 0.5f) * 2f) : Color.Lerp(m_ColorOnMinValue, m_ColorOnHalfValue, num2 * 2f));
			if (m_FillImage != null)
			{
				m_FillImage.color = colorOnMaxValue;
			}
		}
		m_bShouldSetStartValue = false;
	}

	public void IncreaseValue(int amount)
	{
		SetValue(m_CurrentValue + amount);
	}

	public void DecreaseValue(int amount)
	{
		SetValue(m_CurrentValue - amount);
	}
}
