using UnityEngine;

[ExecuteInEditMode]
public class CustomLight : MonoBehaviour
{
	public enum LightArea
	{
		Everywhere,
		OutdoorsOnly,
		IndoorsOnly
	}

	public float m_Size = 3f;

	public float m_Range = 4f;

	public Color m_Color = Color.white;

	[Range(0f, 8f)]
	public float m_Intensity = 1f;

	[Range(0f, 1f)]
	public float m_Falloff = 0.2f;

	public LightArea m_lightArea;

	[HideInInspector]
	public Vector4 m_params;

	[HideInInspector]
	public Vector3 m_s;

	[HideInInspector]
	public Color m_linearColor;

	[HideInInspector]
	public Transform m_transform;

	[HideInInspector]
	public Matrix4x4 m_trs;

	public void Awake()
	{
		m_transform = base.transform;
		m_params.x = m_Size;
		m_params.y = 1f / (m_Size * m_Size);
		m_params.z = m_Range;
		m_params.w = m_Falloff;
		m_s.x = m_Size * 2f;
		m_s.y = m_Size * 2f;
		m_s.z = m_Range;
		m_linearColor = new Color(Mathf.GammaToLinearSpace(m_Color.r * m_Intensity), Mathf.GammaToLinearSpace(m_Color.g * m_Intensity), Mathf.GammaToLinearSpace(m_Color.b * m_Intensity), 1f);
		if (m_transform == null)
		{
			m_transform = base.transform;
		}
		if (m_lightArea == LightArea.OutdoorsOnly && GetComponent<GuardTowerSpotlight>() == null)
		{
			m_transform.position = new Vector3(m_transform.position.x, m_transform.position.y, -20f);
			m_Range = 50f;
		}
		m_trs = Matrix4x4.TRS(m_transform.position, m_transform.rotation, m_s);
	}

	public void Start()
	{
	}

	private void SetLinearColour()
	{
		m_linearColor = new Color(Mathf.GammaToLinearSpace(m_Color.r * m_Intensity), Mathf.GammaToLinearSpace(m_Color.g * m_Intensity), Mathf.GammaToLinearSpace(m_Color.b * m_Intensity), 1f);
	}

	public void UpdateMatrix()
	{
		if (m_transform == null)
		{
			m_transform = base.transform;
		}
		m_trs = Matrix4x4.TRS(m_transform.position, m_transform.rotation, m_s);
	}

	public void SetColour(Color color)
	{
		m_Color = color;
		SetLinearColour();
	}

	public void SetIntensity(float intensity)
	{
		m_Intensity = intensity;
		SetLinearColour();
	}

	public Color GetLinearColour()
	{
		return m_linearColor;
	}
}
