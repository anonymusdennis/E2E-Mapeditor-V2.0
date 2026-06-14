using UnityEngine;

public class PlayerSelectSlot : MonoBehaviour
{
	public delegate void ButtonPressed(PlayerSelectSlot selectedSlot);

	private int m_Index;

	private Customisation m_Customisation;

	private RenderTexture m_RenderTexture;

	public int m_RenderTextureID;

	public T17RawImage m_RawImage;

	public T17Button m_Button;

	public ButtonPressed OnPressed;

	public int index
	{
		get
		{
			return m_Index;
		}
		set
		{
			m_Index = value;
		}
	}

	public Customisation customisation
	{
		get
		{
			return m_Customisation;
		}
		set
		{
			m_Customisation = value;
		}
	}

	public RenderTexture RenderTex
	{
		get
		{
			return m_RenderTexture;
		}
		set
		{
			m_RenderTexture = value;
		}
	}

	public void OnClick()
	{
		if (OnPressed != null)
		{
			OnPressed(this);
		}
	}
}
