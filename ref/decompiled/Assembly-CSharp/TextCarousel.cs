public class TextCarousel : UICarousel<string>
{
	[Tooltip("Text to change upon select")]
	public T17Text m_TextElement;

	protected override void Awake()
	{
		base.Awake();
		if (!(m_TextElement == null))
		{
		}
	}

	protected override void UpdateUIForSelectedIndex(int index)
	{
		m_TextElement.SetNewPlaceHolder(m_Options[index]);
		m_TextElement.text = m_Options[index];
	}
}
