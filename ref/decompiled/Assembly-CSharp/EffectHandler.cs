using UnityEngine;

public class EffectHandler : MonoBehaviour
{
	private float m_TimeAlive;

	public bool m_bUseAnimationCurveAsLifetime = true;

	public float m_Lifetime;

	private Vector3 m_StartScale;

	public bool m_bUseTransparencyCurve = true;

	public AnimationCurve m_TransparencyCurve;

	public bool m_bUseScaleCurve = true;

	public AnimationCurve m_ScaleCurve;

	public bool m_isUVScrolling;

	public Vector2 m_UVScrollSpeed = default(Vector2);

	private Vector2 m_uvOffset = Vector2.zero;

	public bool m_isAnimatingSprite;

	public int m_SpriteSheetTilesX = 1;

	public int m_SpriteSheetTilesY = 1;

	private int m_AnimationFrameX;

	private int m_AnimationFrameY;

	private float m_AnimationFrameTime;

	private float m_AnimationFrameLength;

	private MeshRenderer m_CachedRenderer;

	private void Awake()
	{
		m_StartScale = base.gameObject.transform.localScale;
		m_CachedRenderer = base.gameObject.GetComponent<MeshRenderer>();
	}

	private void Update()
	{
		m_TimeAlive += UpdateManager.deltaTime;
		if (m_TimeAlive > m_Lifetime)
		{
			ReturnEffect(shouldChildToManager: true);
			return;
		}
		if (m_CachedRenderer != null && m_bUseTransparencyCurve)
		{
			Color color = m_CachedRenderer.material.color;
			m_CachedRenderer.material.color = new Color(color.r, color.g, color.b, m_TransparencyCurve.Evaluate(m_TimeAlive));
		}
		if (m_bUseScaleCurve)
		{
			base.gameObject.transform.localScale = m_StartScale * m_ScaleCurve.Evaluate(m_TimeAlive);
		}
		if (m_isUVScrolling)
		{
			UVScroll();
		}
		if (m_isAnimatingSprite)
		{
			m_AnimationFrameTime += UpdateManager.deltaTime;
			if (m_AnimationFrameTime > m_AnimationFrameLength)
			{
				m_AnimationFrameTime = 0f;
				NextSpriteFrame();
			}
		}
	}

	public void ReturnEffect(bool shouldChildToManager)
	{
		if (shouldChildToManager)
		{
			ReturnToEffectTransform();
		}
		ResetValues();
		if (base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: false);
		}
		CullingObjectCollector instance = CullingObjectCollector.GetInstance();
		if (instance != null)
		{
			instance.RemoveRuntimeEffect(base.gameObject);
		}
	}

	private void ReturnToEffectTransform()
	{
		EffectManager instance = EffectManager.GetInstance();
		if (instance != null)
		{
			base.gameObject.transform.parent = instance.gameObject.transform;
		}
	}

	private void ResetValues()
	{
		m_TimeAlive = 0f;
		m_AnimationFrameTime = 0f;
		if (m_bUseAnimationCurveAsLifetime)
		{
			float time = m_TransparencyCurve[m_TransparencyCurve.length - 1].time;
			float time2 = m_ScaleCurve[m_ScaleCurve.length - 1].time;
			if (time > time2)
			{
				m_Lifetime = time;
			}
			else
			{
				m_Lifetime = time2;
			}
		}
		if (m_isAnimatingSprite)
		{
			m_AnimationFrameLength = m_Lifetime / (float)(m_SpriteSheetTilesX * m_SpriteSheetTilesY);
			m_CachedRenderer.material.SetTextureScale("_MainTex", new Vector2(1f / (float)m_SpriteSheetTilesX, 1f / (float)m_SpriteSheetTilesY));
		}
		if (m_CachedRenderer != null && m_bUseTransparencyCurve)
		{
			Color color = base.gameObject.GetComponent<MeshRenderer>().material.color;
			m_CachedRenderer.material.color = new Color(color.r, color.g, color.b, m_TransparencyCurve.Evaluate(0f));
		}
		if (m_bUseScaleCurve)
		{
			base.gameObject.transform.localScale = m_StartScale * m_ScaleCurve.Evaluate(0f);
		}
	}

	public void UVScroll()
	{
		m_uvOffset += m_UVScrollSpeed * UpdateManager.deltaTime;
		base.gameObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", m_uvOffset);
	}

	public void NextSpriteFrame()
	{
		m_AnimationFrameX++;
		if (m_AnimationFrameX >= m_SpriteSheetTilesX)
		{
			m_AnimationFrameY++;
			m_AnimationFrameX = 0;
			if (m_AnimationFrameY >= m_SpriteSheetTilesY)
			{
				m_AnimationFrameY = 0;
			}
		}
		Vector2 offset = new Vector2(1f / (float)m_SpriteSheetTilesX * (float)m_AnimationFrameX, 1f / (float)m_SpriteSheetTilesY * (float)m_AnimationFrameY);
		base.gameObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", offset);
	}

	private void OnDisable()
	{
		ReturnEffect(shouldChildToManager: false);
	}

	public void PrepareForCullerVisiblity(bool isVisible)
	{
		if (!isVisible)
		{
			ReturnToEffectTransform();
		}
	}
}
