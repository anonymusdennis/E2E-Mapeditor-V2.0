using UnityEngine;

public class SpriteMaterialProxy : MonoBehaviour
{
	private const float DIFF_EPSILON = 0.001f;

	public T17Image m_Image;

	public string m_PropertyName = string.Empty;

	[ReadOnly]
	public float m_PropertyValue = -1f;

	private Material m_Material;

	private int m_PropertyHash = -1;

	private float m_PrevPropertyValue = -1f;

	private void Awake()
	{
		if (m_Image != null)
		{
			m_Material = m_Image.material;
		}
		if (m_Material == null || string.IsNullOrEmpty(m_PropertyName))
		{
			base.enabled = false;
			return;
		}
		m_PropertyHash = Shader.PropertyToID(m_PropertyName);
		m_PropertyValue = m_Material.GetFloat(m_PropertyHash);
		m_PrevPropertyValue = m_PropertyValue;
	}

	private void LateUpdate()
	{
		if (Mathf.Abs(m_PrevPropertyValue - m_PropertyValue) >= 0.001f)
		{
			m_Material.SetFloat(m_PropertyHash, m_PropertyValue);
			m_PrevPropertyValue = m_PropertyValue;
		}
	}
}
