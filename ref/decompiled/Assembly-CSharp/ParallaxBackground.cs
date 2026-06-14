using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ParallaxBackground : MonoBehaviour, ICutsceneActorResettable
{
	[Serializable]
	public class ParallaxLayer
	{
		public string m_LayerName = "ParallaxLayer";

		public int m_PanelCount = 6;

		public float m_DefaultPanelHeight = 30f;

		public float m_ScrollSpeed = 0.5f;

		public Material m_Material;

		public float m_YOffset;

		public float m_XOffset;

		[HideInInspector]
		public Transform m_LayerParent;

		[HideInInspector]
		public MeshRenderer[] m_GeneratedPanels;
	}

	public List<ParallaxLayer> m_Layers = new List<ParallaxLayer>();

	[Range(-1f, 1f)]
	public float m_UpdateModifier = 1f;

	private float m_PreCutsceneUpdateModifier = 1f;

	private void Start()
	{
		for (int i = 0; i < m_Layers.Count; i++)
		{
			ParallaxLayer parallaxLayer = m_Layers[i];
			if (parallaxLayer != null && parallaxLayer.m_LayerParent != null)
			{
				parallaxLayer.m_GeneratedPanels = parallaxLayer.m_LayerParent.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
			}
		}
	}

	private void LateUpdate()
	{
		UpdateParallaxLayers(Time.deltaTime);
	}

	private void UpdateParallaxLayers(float dt)
	{
		for (int i = 0; i < m_Layers.Count; i++)
		{
			ParallaxLayer parallaxLayer = m_Layers[i];
			if (parallaxLayer == null || parallaxLayer.m_GeneratedPanels == null)
			{
				continue;
			}
			for (int j = 0; j < parallaxLayer.m_GeneratedPanels.Length; j++)
			{
				MeshRenderer meshRenderer = parallaxLayer.m_GeneratedPanels[j];
				Vector2 mainTextureOffset = meshRenderer.material.mainTextureOffset;
				mainTextureOffset.x += dt * parallaxLayer.m_ScrollSpeed * m_UpdateModifier;
				if (mainTextureOffset.x > 1f)
				{
					mainTextureOffset.x -= 1f;
				}
				if (mainTextureOffset.x < -1f)
				{
					mainTextureOffset.x += 1f;
				}
				meshRenderer.material.mainTextureOffset = mainTextureOffset;
			}
		}
	}

	public void Cutscene_PrepareForUse()
	{
		m_PreCutsceneUpdateModifier = m_UpdateModifier;
	}

	public void Cutscene_FinishedUse()
	{
		m_UpdateModifier = m_PreCutsceneUpdateModifier;
		base.enabled = true;
	}
}
