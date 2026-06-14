using UnityEngine;

public class NamePlateHUD : MonoBehaviour
{
	public Sprite m_NormalImage;

	public Sprite m_HighLightImage;

	public T17Image m_Background;

	public T17Text m_Text;

	public void CopyFrom(NamePlateHUD other)
	{
		m_Background.sprite = other.m_Background.sprite;
		m_Text.text = other.m_Text.text;
		m_Text.color = other.m_Text.color;
	}
}
