using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LevelSetup_RequiresLayerShift : BaseComponentSetup
{
	public Animator[] m_Animators = new Animator[0];

	public MeshRenderer[] m_Renderers = new MeshRenderer[0];

	public MeshFilter[] m_MeshFilters = new MeshFilter[0];

	public ParticleSystem[] m_ParticleSystem = new ParticleSystem[0];

	private bool m_ForceZ;

	private float m_ForceZValue;

	public float m_OffsetAmount;

	public float m_Span = 1f;

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_9;
	}

	[ContextMenu("Setup")]
	public void setup()
	{
		Setup();
	}

	public override SetupReturnState Setup()
	{
		if (m_Animators.Length == 0 && m_Renderers.Length == 0 && m_ParticleSystem.Length == 0)
		{
			return FinishedAndRemove();
		}
		float num = 0f;
		if (m_ForceZ)
		{
			num = m_ForceZValue;
		}
		else
		{
			num = LayerHelper.GetZOffset(base.transform, 0f, m_Span) + m_ForceZValue;
			num += m_OffsetAmount;
		}
		int num2;
		if (m_Animators.Length > 0)
		{
			num2 = m_Animators.Length;
			for (int i = 0; i < num2; i++)
			{
				if (m_Animators[i] != null)
				{
					Vector3 localPosition = m_Animators[i].transform.localPosition;
					localPosition.z += num;
					m_Animators[i].transform.localPosition = localPosition;
				}
			}
		}
		num2 = m_Renderers.Length;
		for (int i = 0; i < num2; i++)
		{
			if (!(m_Renderers[i] != null))
			{
				continue;
			}
			if (m_Renderers[i].transform == base.transform)
			{
				MeshFilter meshFilter = m_MeshFilters[i];
				if (!(meshFilter != null))
				{
					continue;
				}
				Mesh mesh = Object.Instantiate(meshFilter.sharedMesh);
				if (mesh != null)
				{
					Vector3[] vertices = mesh.vertices;
					int num3 = vertices.Length;
					for (int j = 0; j < num3; j++)
					{
						vertices[j].z += num;
					}
					mesh.vertices = vertices;
					meshFilter.sharedMesh = mesh;
				}
			}
			else if (m_Renderers[i].shadowCastingMode != ShadowCastingMode.ShadowsOnly)
			{
				Vector3 localPosition2 = m_Renderers[i].transform.localPosition;
				localPosition2.z += num;
				m_Renderers[i].transform.localPosition = localPosition2;
			}
		}
		num2 = m_ParticleSystem.Length;
		for (int i = 0; i < num2; i++)
		{
			if (m_ParticleSystem[i] != null)
			{
				Vector3 localPosition3 = m_ParticleSystem[i].transform.localPosition;
				localPosition3.z += num;
				m_ParticleSystem[i].transform.localPosition = localPosition3;
			}
		}
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		return Setup();
	}

	[ContextMenu("Scan")]
	public void ScanForContents()
	{
		m_Renderers = new MeshRenderer[0];
		m_MeshFilters = new MeshFilter[0];
		m_Animators = GetComponentsInChildren<Animator>();
		List<ParticleSystem> list = new List<ParticleSystem>();
		List<MeshRenderer> list2 = new List<MeshRenderer>();
		list2.AddRange(GetComponentsInChildren<MeshRenderer>());
		list.AddRange(GetComponentsInChildren<ParticleSystem>());
		for (int num = m_Animators.Length - 1; num >= 0; num--)
		{
			ParticleSystem[] componentsInChildren = m_Animators[num].GetComponentsInChildren<ParticleSystem>();
			for (int num2 = componentsInChildren.Length - 1; num2 >= 0; num2--)
			{
				list.Remove(componentsInChildren[num2]);
			}
			MeshRenderer[] componentsInChildren2 = m_Animators[num].GetComponentsInChildren<MeshRenderer>();
			for (int num3 = componentsInChildren2.Length - 1; num3 >= 0; num3--)
			{
				list2.Remove(componentsInChildren2[num3]);
			}
		}
		m_ParticleSystem = list.ToArray();
		m_Renderers = list2.ToArray();
		m_MeshFilters = new MeshFilter[m_Renderers.Length];
		for (int i = 0; i < m_Renderers.Length; i++)
		{
			m_MeshFilters[i] = m_Renderers[i].GetComponent<MeshFilter>();
		}
	}
}
