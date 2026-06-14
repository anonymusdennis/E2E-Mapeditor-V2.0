using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class UI_AnimationToRenderTexture : MonoBehaviour
{
	[Serializable]
	public struct AnimationParams
	{
		public string Parameter;

		public bool Enabled;
	}

	[Serializable]
	public struct ScrollProperities
	{
		public MeshRenderer m_ScrollUVMeshRenderer;

		public Vector2 m_ScrollSpeed;
	}

	public class ScrollData
	{
		public Material m_ScrollUVMaterial;

		public MaterialPropertyBlock m_scrollPropertyBlock;

		public int m_propId_ScrollSpeed;
	}

	public Animator m_ControllingAnimator;

	public List<AnimationParams> m_StartParams = new List<AnimationParams>();

	public List<AnimationParams> m_StopParams = new List<AnimationParams>();

	private Camera m_Camera;

	private RenderTexture m_RenderTexture;

	private int m_RenderTextureID;

	private int m_TargetWidth;

	private int m_TargetHeight;

	private bool m_bTextureReady;

	private bool m_bAnimating;

	private const float INV_ALLOWED_DELTA = 24f;

	private const float ALLOWED_DELTA = 1f / 24f;

	private ScrollData[] m_ScrollData;

	public ScrollProperities[] m_ScrollProperities;

	private void Awake()
	{
		m_Camera = GetComponent<Camera>();
		m_Camera.enabled = false;
		if (m_ControllingAnimator != null)
		{
			m_ControllingAnimator.gameObject.SetActive(value: false);
		}
		if (m_ScrollProperities != null && m_ScrollProperities.Length > 0)
		{
			m_ScrollData = new ScrollData[m_ScrollProperities.Length];
			for (int i = 0; i < m_ScrollProperities.Length; i++)
			{
				if (m_ScrollProperities[i].m_ScrollUVMeshRenderer != null)
				{
					m_ScrollData[i] = new ScrollData();
					m_ScrollData[i].m_ScrollUVMaterial = m_ScrollProperities[i].m_ScrollUVMeshRenderer.sharedMaterial;
					m_ScrollData[i].m_scrollPropertyBlock = new MaterialPropertyBlock();
					m_ScrollData[i].m_propId_ScrollSpeed = Shader.PropertyToID("_ScrollSpeed");
					m_ScrollData[i].m_scrollPropertyBlock.SetVector(m_ScrollData[i].m_propId_ScrollSpeed, m_ScrollProperities[i].m_ScrollSpeed);
				}
			}
		}
		m_TargetWidth = 0;
		m_TargetHeight = 0;
	}

	protected virtual void OnDestroy()
	{
		if (m_RenderTexture != null)
		{
			RenderTargetManager.ReleaseRenderTarget(ref m_RenderTextureID);
			m_RenderTexture = null;
		}
	}

	private void OnDisable()
	{
		if (m_RenderTexture != null)
		{
			RenderTargetManager.ReleaseRenderTarget(ref m_RenderTextureID);
			m_RenderTexture = null;
		}
	}

	public void ForceClearRenderTarget()
	{
		if (m_RenderTexture != null)
		{
			RenderTargetManager.ReleaseRenderTarget(ref m_RenderTextureID);
			m_RenderTexture = null;
		}
	}

	public RenderTexture GetRenderTexture()
	{
		return m_RenderTexture;
	}

	public bool IsRenderTextureReady()
	{
		return m_bTextureReady;
	}

	public void SetTargetSize(int width, int height)
	{
		float width2 = (float)width / (float)height;
		if (width == 0 || height == 0 || m_Camera == null)
		{
			return;
		}
		m_Camera.rect.Set(0f, 0f, width2, 1f);
		if (m_RenderTexture != null && m_TargetWidth != width && m_TargetHeight != height)
		{
			RenderTargetManager.ReleaseRenderTarget(ref m_RenderTextureID);
			m_RenderTexture = null;
		}
		if (m_RenderTexture == null)
		{
			string debugName = base.gameObject.scene.name + ":" + base.gameObject.name;
			m_RenderTexture = RenderTargetManager.RequestRenderTarget(width, height, 24, RenderTextureFormat.ARGB32, ref m_RenderTextureID, debugName);
			m_TargetWidth = width;
			m_TargetHeight = height;
			m_Camera.targetTexture = m_RenderTexture;
			m_bTextureReady = true;
		}
		if (m_Camera.enabled)
		{
			return;
		}
		if (m_ControllingAnimator != null)
		{
			m_ControllingAnimator.gameObject.SetActive(value: true);
			m_ControllingAnimator.Update(0f);
		}
		m_Camera.Render();
		if (m_StartParams.Count > 0)
		{
			if (m_ControllingAnimator != null)
			{
				m_ControllingAnimator.gameObject.SetActive(value: false);
			}
		}
		else
		{
			m_Camera.enabled = true;
			m_bAnimating = true;
		}
	}

	public void DoneARender()
	{
		if (!m_bAnimating)
		{
			if (m_Camera.enabled)
			{
				m_Camera.enabled = false;
			}
			if (m_ControllingAnimator != null)
			{
				m_ControllingAnimator.gameObject.SetActive(value: false);
			}
		}
	}

	public void StartAnimation()
	{
		m_bAnimating = true;
		m_Camera.enabled = true;
		if (m_ControllingAnimator != null)
		{
			m_ControllingAnimator.gameObject.SetActive(value: true);
			for (int i = 0; i < m_StartParams.Count; i++)
			{
				m_ControllingAnimator.SetBool(m_StartParams[i].Parameter, m_StartParams[i].Enabled);
			}
		}
		if (m_ScrollData == null || m_ScrollData.Length <= 0)
		{
			return;
		}
		for (int i = 0; i < m_ScrollData.Length; i++)
		{
			if (m_ScrollData[i].m_ScrollUVMaterial != null)
			{
				m_ScrollData[i].m_scrollPropertyBlock.SetVector(m_ScrollData[i].m_propId_ScrollSpeed, m_ScrollProperities[i].m_ScrollSpeed);
				m_ScrollProperities[i].m_ScrollUVMeshRenderer.SetPropertyBlock(m_ScrollData[i].m_scrollPropertyBlock);
			}
		}
	}

	public void StopAnimation()
	{
		m_bAnimating = false;
		if (m_ControllingAnimator != null)
		{
			for (int i = 0; i < m_StopParams.Count; i++)
			{
				m_ControllingAnimator.SetBool(m_StopParams[i].Parameter, m_StopParams[i].Enabled);
			}
		}
		if (m_ScrollData != null && m_ScrollData.Length > 0)
		{
			for (int i = 0; i < m_ScrollData.Length; i++)
			{
				if (m_ScrollData[i].m_ScrollUVMaterial != null)
				{
					m_ScrollData[i].m_scrollPropertyBlock.SetVector(m_ScrollData[i].m_propId_ScrollSpeed, Vector2.zero);
					m_ScrollProperities[i].m_ScrollUVMeshRenderer.SetPropertyBlock(m_ScrollData[i].m_scrollPropertyBlock);
				}
			}
		}
		StartCoroutine(DelayedOneFrameCameraDisable());
	}

	private IEnumerator DelayedOneFrameCameraDisable()
	{
		yield return new WaitForEndOfFrame();
		if (!m_bAnimating)
		{
			m_Camera.enabled = false;
			m_ControllingAnimator.gameObject.SetActive(value: false);
		}
	}
}
