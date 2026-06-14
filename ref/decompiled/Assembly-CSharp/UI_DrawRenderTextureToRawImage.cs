using UnityEngine;

[RequireComponent(typeof(T17RawImage))]
public class UI_DrawRenderTextureToRawImage : MonoBehaviour
{
	public UI_AnimationToRenderTexture m_TargetAnimationToRenderTexture;

	private T17RawImage m_Image;

	private RenderTexture m_CachedRenderTexture;

	private int m_WaitFrame = 1;

	private T17Button m_Button;

	private void Awake()
	{
		m_Image = GetComponent<T17RawImage>();
		m_Image.enabled = false;
	}

	private void OnEnable()
	{
		m_WaitFrame = 1;
		m_Button = base.transform.parent.GetComponent<T17Button>();
		if (m_Button != null)
		{
			m_Button.m_PC_UIAnimToRenderTex_HoverCapture = m_TargetAnimationToRenderTexture;
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_TargetAnimationToRenderTexture != null)
		{
			m_TargetAnimationToRenderTexture.ForceClearRenderTarget();
		}
	}

	private void Update()
	{
		if (m_Image == null)
		{
			return;
		}
		if (m_CachedRenderTexture != null && !m_CachedRenderTexture.IsCreated())
		{
			m_WaitFrame = 1;
		}
		if (m_WaitFrame == 1 && m_TargetAnimationToRenderTexture != null)
		{
			int width = (int)m_Image.rectTransform.rect.width;
			int height = (int)m_Image.rectTransform.rect.height;
			m_TargetAnimationToRenderTexture.SetTargetSize(width, height);
			if (m_TargetAnimationToRenderTexture.IsRenderTextureReady())
			{
				m_Image.texture = m_TargetAnimationToRenderTexture.GetRenderTexture();
				m_Image.enabled = true;
				m_TargetAnimationToRenderTexture.DoneARender();
				m_CachedRenderTexture = (RenderTexture)m_Image.texture;
			}
			else
			{
				m_WaitFrame = 2;
			}
		}
		if (m_WaitFrame > 0)
		{
			m_WaitFrame--;
		}
	}
}
