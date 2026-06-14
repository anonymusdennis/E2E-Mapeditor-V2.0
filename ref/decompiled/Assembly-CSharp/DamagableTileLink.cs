using UnityEngine;

public class DamagableTileLink : MonoBehaviour
{
	public DamagableTile m_DamagableTile;

	public Material[] m_Materials;

	private int m_CurrentSetStage = -1;

	private Renderer m_Renderer;

	private MeshFilter m_filter;

	private void Awake()
	{
		m_Renderer = GetComponent<Renderer>();
		m_filter = GetComponent<MeshFilter>();
	}

	private void Update()
	{
		if (m_DamagableTile != null && m_DamagableTile.DamageStage != m_CurrentSetStage && m_Materials != null && m_Renderer != null && m_DamagableTile.DamageStage != -1 && m_DamagableTile.DamageStage < m_Materials.Length && m_Materials[m_DamagableTile.DamageStage] != null)
		{
			m_Renderer.material = m_Materials[m_DamagableTile.DamageStage];
			m_CurrentSetStage = m_DamagableTile.DamageStage;
			if (m_filter != null)
			{
				m_filter.mesh.uv = (Vector2[])m_DamagableTile.GetFilterUVs().Clone();
			}
		}
	}
}
