using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Blur/Blur (Optimized)")]
public class BlurOptimized : PostEffectsBase
{
	public enum BlurType
	{
		StandardGauss,
		SgxGauss
	}

	[Range(0f, 2f)]
	public int m_downsample = 1;

	[Range(0f, 10f)]
	public float m_blurSize = 3f;

	[Range(1f, 4f)]
	public int m_blurIterations = 2;

	public float m_depthSoftening = 10f;

	[Tooltip("The position of the actual ground tiles")]
	public float m_groundPosition = 3f;

	public BlurType m_blurType;

	public Shader m_blurShader;

	private Material m_blurMaterial;

	private Camera m_Camera;

	private Transform m_CamTransform;

	public bool m_benchmarkMode;

	public Texture m_fakeDepthTexture;

	private int m_origTexID;

	private int m_parameterID;

	private int m_distanceParamsID;

	private int m_depthSofteningID;

	private int m_blurredDepthID;

	private int m_fakeDepthTexID;

	private int m_PassOffset;

	private float m_WidthModMulBlurSize;

	private int m_IndexOfCamera;

	protected override void Start()
	{
		base.Start();
		m_origTexID = Shader.PropertyToID("_OrigTex");
		m_parameterID = Shader.PropertyToID("_Parameter");
		m_distanceParamsID = Shader.PropertyToID("_DistanceParams");
		m_depthSofteningID = Shader.PropertyToID("_DepthSoftening");
		m_blurredDepthID = Shader.PropertyToID("_BlurredDepth");
		m_fakeDepthTexID = Shader.PropertyToID("_FakeDepthTexture");
		if (m_blurType == BlurType.StandardGauss)
		{
			m_PassOffset = 0;
		}
		else
		{
			m_PassOffset = 2;
		}
		m_WidthModMulBlurSize = m_blurSize * (1f / (1f * (float)(1 << m_downsample)));
	}

	public override bool CheckResources()
	{
		CheckSupport(needDepth: false);
		m_blurMaterial = CheckShaderAndCreateMaterial(m_blurShader, m_blurMaterial);
		m_blurMaterial.SetFloat(m_depthSofteningID, m_depthSoftening);
		m_blurMaterial.SetVector(m_parameterID, new Vector4(m_WidthModMulBlurSize, 0f - m_WidthModMulBlurSize, 0f, 0f));
		if (!isSupported)
		{
			ReportAutoDisable();
		}
		if (m_Camera == null)
		{
			m_Camera = GetComponent<Camera>();
			if (m_Camera == null)
			{
				isSupported = false;
			}
			else
			{
				m_CamTransform = m_Camera.transform;
				if (!m_benchmarkMode && CameraManager.GetInstance() != null)
				{
					m_IndexOfCamera = CameraManager.GetInstance().GetCameraIndexInManager(m_Camera);
					CameraManager instance = CameraManager.GetInstance();
					instance.OnActiveCamerasUpdated = (CameraManager.CameraManagerHandler)Delegate.Remove(instance.OnActiveCamerasUpdated, new CameraManager.CameraManagerHandler(ActiveCamerasUpdated));
					CameraManager instance2 = CameraManager.GetInstance();
					instance2.OnActiveCamerasUpdated = (CameraManager.CameraManagerHandler)Delegate.Combine(instance2.OnActiveCamerasUpdated, new CameraManager.CameraManagerHandler(ActiveCamerasUpdated));
				}
			}
		}
		return isSupported;
	}

	public void OnEnable()
	{
		if (!m_blurMaterial)
		{
			CheckResources();
		}
		if (m_Camera != null && CameraManager.GetInstance() != null)
		{
			m_IndexOfCamera = CameraManager.GetInstance().GetCameraIndexInManager(m_Camera);
			CameraManager instance = CameraManager.GetInstance();
			instance.OnActiveCamerasUpdated = (CameraManager.CameraManagerHandler)Delegate.Remove(instance.OnActiveCamerasUpdated, new CameraManager.CameraManagerHandler(ActiveCamerasUpdated));
			CameraManager instance2 = CameraManager.GetInstance();
			instance2.OnActiveCamerasUpdated = (CameraManager.CameraManagerHandler)Delegate.Combine(instance2.OnActiveCamerasUpdated, new CameraManager.CameraManagerHandler(ActiveCamerasUpdated));
		}
	}

	public void OnDisable()
	{
		if (CameraManager.GetInstance() != null)
		{
			CameraManager instance = CameraManager.GetInstance();
			instance.OnActiveCamerasUpdated = (CameraManager.CameraManagerHandler)Delegate.Remove(instance.OnActiveCamerasUpdated, new CameraManager.CameraManagerHandler(ActiveCamerasUpdated));
		}
		if ((bool)m_blurMaterial)
		{
			UnityEngine.Object.DestroyImmediate(m_blurMaterial);
		}
	}

	public void ActiveCamerasUpdated()
	{
		if (CameraManager.GetInstance() != null)
		{
			m_IndexOfCamera = CameraManager.GetInstance().GetCameraIndexInManager(m_Camera);
		}
	}

	public void RecalculateIndexOfCamera(CameraManager camManager)
	{
		m_IndexOfCamera = camManager.GetCameraIndexInManager(m_Camera);
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!(source == null))
		{
			Vector3 position = m_CamTransform.position;
			Vector3 zero = Vector3.zero;
			float num = 0f;
			if (!m_benchmarkMode)
			{
				num = CameraManager.GetInstance().GetFloorZOfCamera(m_IndexOfCamera);
			}
			else
			{
				m_blurMaterial.SetTexture(m_fakeDepthTexID, m_fakeDepthTexture);
			}
			float num2 = num - position.z;
			float num3 = num2 + 3f;
			float z = 1f / (num3 - num2);
			m_blurMaterial.SetTexture(m_origTexID, source);
			m_blurMaterial.SetVector(m_distanceParamsID, new Vector4(num2, num3, z, 0f));
			source.filterMode = FilterMode.Bilinear;
			int width = source.width >> m_downsample;
			int height = source.height >> m_downsample;
			RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, source.format);
			RenderTexture temporary2 = RenderTexture.GetTemporary(width, height, 0, source.format);
			if (temporary == null || temporary2 == null)
			{
				Graphics.Blit(source, destination);
				return;
			}
			temporary.filterMode = FilterMode.Bilinear;
			temporary2.filterMode = FilterMode.Bilinear;
			Graphics.Blit(source, temporary, m_blurMaterial, 0);
			Graphics.Blit(source, temporary2, m_blurMaterial, 7);
			RenderTexture temporary3 = RenderTexture.GetTemporary(width, height, 0, temporary.format);
			temporary3.filterMode = FilterMode.Bilinear;
			Graphics.Blit(temporary, temporary3, m_blurMaterial, 1 + m_PassOffset);
			RenderTexture.ReleaseTemporary(temporary);
			temporary = temporary3;
			temporary3 = RenderTexture.GetTemporary(width, height, 0, temporary.format);
			temporary3.filterMode = FilterMode.Bilinear;
			Graphics.Blit(temporary, temporary3, m_blurMaterial, 2 + m_PassOffset);
			RenderTexture.ReleaseTemporary(temporary);
			temporary = temporary3;
			temporary3 = RenderTexture.GetTemporary(width, height, 0, temporary2.format);
			temporary3.filterMode = FilterMode.Bilinear;
			Graphics.Blit(temporary2, temporary3, m_blurMaterial, 5);
			RenderTexture.ReleaseTemporary(temporary2);
			temporary2 = temporary3;
			temporary3 = RenderTexture.GetTemporary(width, height, 0, temporary2.format);
			temporary3.filterMode = FilterMode.Bilinear;
			Graphics.Blit(temporary2, temporary3, m_blurMaterial, 6);
			RenderTexture.ReleaseTemporary(temporary2);
			temporary2 = temporary3;
			m_blurMaterial.SetTexture(m_blurredDepthID, temporary2);
			Graphics.Blit(temporary, destination, m_blurMaterial, 8);
			RenderTexture.ReleaseTemporary(temporary);
			RenderTexture.ReleaseTemporary(temporary2);
		}
	}
}
