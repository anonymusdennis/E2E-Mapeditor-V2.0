using System;
using UnityEngine;

[AddComponentMenu("Image Effects/Rendering/Screen Space Directional Light")]
[RequireComponent(typeof(Camera))]
public class ScreenSpaceDirectionalLight : T17MonoBehaviour
{
	private const int kMediumQualityShadows = 2;

	private const int kLowQualityShadows = 4;

	private static RenderTexture m_ShadowTexture;

	private static int m_ShadowRenderTargetID;

	private static int m_DesiredWidth;

	private static int m_DesiredHeight;

	private static ShadowResolution m_CurrentShadowResolution;

	private Vector3 m_lightDirectionInCameraSpace;

	public Shader m_SSDLShader;

	private Material m_SSDLMaterial;

	private LightingManager m_LightingManager;

	private CameraManager m_CameraManager;

	public Texture m_FakeDepthTexture;

	private bool m_LightDirty = true;

	private bool m_CameraDirty = true;

	private DepthTextureMode m_OldDepthTextureMode;

	public bool m_benchmarkMode;

	private Camera m_Camera;

	private static Material CreateMaterial(Shader shader)
	{
		if (!shader)
		{
			return null;
		}
		Material material = new Material(shader);
		material.hideFlags = HideFlags.HideAndDontSave;
		return material;
	}

	private static void DestroyMaterial(Material mat)
	{
		if ((bool)mat)
		{
			UnityEngine.Object.DestroyImmediate(mat);
			mat = null;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_ShadowTexture = null;
		m_ShadowRenderTargetID = 0;
		m_Camera = GetComponent<Camera>();
	}

	private void Start()
	{
		if (!SystemInfo.supportsImageEffects || !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
		{
			base.enabled = false;
			return;
		}
		CreateMaterials();
		if (!m_SSDLMaterial)
		{
			base.enabled = false;
		}
		else if (!m_benchmarkMode)
		{
			m_LightingManager = LightingManager.GetInstance();
			m_CameraManager = CameraManager.GetInstance();
			LightingManager lightingManager = m_LightingManager;
			lightingManager.OnLightingUpdated = (LightingManager.LightingUpdated)Delegate.Remove(lightingManager.OnLightingUpdated, new LightingManager.LightingUpdated(SetLightDirty));
			LightingManager lightingManager2 = m_LightingManager;
			lightingManager2.OnLightingUpdated = (LightingManager.LightingUpdated)Delegate.Combine(lightingManager2.OnLightingUpdated, new LightingManager.LightingUpdated(SetLightDirty));
			CameraManager cameraManager = m_CameraManager;
			cameraManager.OnActiveCamerasUpdated = (CameraManager.CameraManagerHandler)Delegate.Remove(cameraManager.OnActiveCamerasUpdated, new CameraManager.CameraManagerHandler(SetCameraDirty));
			CameraManager cameraManager2 = m_CameraManager;
			cameraManager2.OnActiveCamerasUpdated = (CameraManager.CameraManagerHandler)Delegate.Combine(cameraManager2.OnActiveCamerasUpdated, new CameraManager.CameraManagerHandler(SetCameraDirty));
		}
	}

	private void OnEnable()
	{
		CreateMaterials();
		m_OldDepthTextureMode = m_Camera.depthTextureMode;
		m_Camera.depthTextureMode |= DepthTextureMode.Depth;
		if (!m_benchmarkMode)
		{
			if (m_LightingManager != null)
			{
				LightingManager lightingManager = m_LightingManager;
				lightingManager.OnLightingUpdated = (LightingManager.LightingUpdated)Delegate.Remove(lightingManager.OnLightingUpdated, new LightingManager.LightingUpdated(SetLightDirty));
				LightingManager lightingManager2 = m_LightingManager;
				lightingManager2.OnLightingUpdated = (LightingManager.LightingUpdated)Delegate.Combine(lightingManager2.OnLightingUpdated, new LightingManager.LightingUpdated(SetLightDirty));
			}
			if (m_CameraManager != null)
			{
				CameraManager cameraManager = m_CameraManager;
				cameraManager.OnActiveCamerasUpdated = (CameraManager.CameraManagerHandler)Delegate.Remove(cameraManager.OnActiveCamerasUpdated, new CameraManager.CameraManagerHandler(SetCameraDirty));
				CameraManager cameraManager2 = m_CameraManager;
				cameraManager2.OnActiveCamerasUpdated = (CameraManager.CameraManagerHandler)Delegate.Combine(cameraManager2.OnActiveCamerasUpdated, new CameraManager.CameraManagerHandler(SetCameraDirty));
			}
		}
	}

	private void OnDisable()
	{
		DestroyMaterial(m_SSDLMaterial);
		m_Camera.depthTextureMode = m_OldDepthTextureMode;
		if (!m_benchmarkMode)
		{
			if (m_LightingManager != null)
			{
				LightingManager lightingManager = m_LightingManager;
				lightingManager.OnLightingUpdated = (LightingManager.LightingUpdated)Delegate.Remove(lightingManager.OnLightingUpdated, new LightingManager.LightingUpdated(SetLightDirty));
			}
			if (m_CameraManager != null)
			{
				CameraManager cameraManager = m_CameraManager;
				cameraManager.OnActiveCamerasUpdated = (CameraManager.CameraManagerHandler)Delegate.Remove(cameraManager.OnActiveCamerasUpdated, new CameraManager.CameraManagerHandler(SetCameraDirty));
			}
		}
	}

	protected virtual void OnDestroy()
	{
		RenderTargetManager.ReleaseRenderTarget(ref m_ShadowRenderTargetID);
		m_ShadowTexture = null;
		if (m_LightingManager != null)
		{
			LightingManager lightingManager = m_LightingManager;
			lightingManager.OnLightingUpdated = (LightingManager.LightingUpdated)Delegate.Remove(lightingManager.OnLightingUpdated, new LightingManager.LightingUpdated(SetLightDirty));
			m_LightingManager = null;
		}
		if (m_CameraManager != null)
		{
			CameraManager cameraManager = m_CameraManager;
			cameraManager.OnActiveCamerasUpdated = (CameraManager.CameraManagerHandler)Delegate.Remove(cameraManager.OnActiveCamerasUpdated, new CameraManager.CameraManagerHandler(SetCameraDirty));
			m_CameraManager = null;
		}
	}

	private void CreateMaterials()
	{
		if (!m_SSDLMaterial && m_SSDLShader.isSupported)
		{
			m_SSDLMaterial = CreateMaterial(m_SSDLShader);
			m_SSDLMaterial.SetFloat("_Shadows", 1f);
		}
	}

	private void CreateShadowTexture(RenderTexture source)
	{
		switch (QualitySettings.shadowResolution)
		{
		case ShadowResolution.High:
			m_DesiredWidth = source.width;
			m_DesiredHeight = source.height;
			break;
		case ShadowResolution.Medium:
			m_DesiredWidth = source.width / 2;
			m_DesiredHeight = source.height / 2;
			break;
		case ShadowResolution.Low:
			m_DesiredWidth = source.width / 4;
			m_DesiredHeight = source.height / 4;
			break;
		}
		m_ShadowTexture = RenderTargetManager.RequestRenderTarget(m_DesiredWidth, m_DesiredHeight, 0, RenderTextureFormat.ARGB32, ref m_ShadowRenderTargetID, "SSDL");
		m_CurrentShadowResolution = QualitySettings.shadowResolution;
	}

	[ImageEffectOpaque]
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (source == null)
		{
			return;
		}
		bool flag = QualitySettings.shadows != ShadowQuality.Disable;
		if (flag || m_benchmarkMode)
		{
			if (m_ShadowTexture == null)
			{
				CreateShadowTexture(source);
			}
			else if (m_CurrentShadowResolution != QualitySettings.shadowResolution || m_ShadowTexture.width != m_DesiredWidth || m_ShadowTexture.height != m_DesiredHeight)
			{
				RenderTargetManager.ReleaseRenderTarget(ref m_ShadowRenderTargetID);
				CreateShadowTexture(source);
			}
		}
		else if (m_ShadowTexture != null)
		{
			RenderTargetManager.ReleaseRenderTarget(ref m_ShadowRenderTargetID);
			m_ShadowTexture = null;
		}
		if (m_LightDirty || m_benchmarkMode)
		{
			UpdateMaterial();
			m_LightDirty = false;
		}
		if (m_CameraDirty || m_benchmarkMode)
		{
			UpdateCamera();
			m_CameraDirty = false;
		}
		m_SSDLMaterial.SetFloat("_Shadows", (!flag) ? 0f : 1f);
		if (flag || m_benchmarkMode)
		{
			m_SSDLMaterial.SetMatrix("_Projection", m_Camera.projectionMatrix);
			if (m_benchmarkMode)
			{
				m_SSDLMaterial.SetTexture("_FakeDepthTexture", m_FakeDepthTexture);
			}
			Graphics.Blit(source, m_ShadowTexture, m_SSDLMaterial, 0);
			m_SSDLMaterial.SetTexture("_ShadowTexture", m_ShadowTexture);
			Graphics.Blit(source, destination, m_SSDLMaterial, 1);
		}
		else
		{
			Graphics.Blit(source, destination);
		}
	}

	private void SetLightDirty()
	{
		m_LightDirty = true;
	}

	private void SetCameraDirty()
	{
		m_CameraDirty = true;
	}

	private void UpdateCamera()
	{
		float num = (float)m_Camera.pixelWidth / m_Camera.rect.width;
		float num2 = (float)m_Camera.pixelHeight / m_Camera.rect.height;
		Ray ray = m_Camera.ScreenPointToRay(new Vector3(num * m_Camera.rect.x, num2 * m_Camera.rect.y + num2 * m_Camera.rect.height, 0f));
		float num3 = m_Camera.pixelWidth;
		float num4 = m_Camera.pixelHeight;
		m_SSDLMaterial.SetVector("_ScreenSize", new Vector2(num3, num4));
		m_SSDLMaterial.SetVector("_HalfScreenSize", new Vector2(num3 / 2f, num4 / 2f));
		m_SSDLMaterial.SetVector("_InvScreenSize", new Vector2(1f / num3, 1f / num4));
		m_SSDLMaterial.SetVector("_farPlaneCornerRay", ray.direction);
	}

	private void UpdateMaterial()
	{
		float revShadowIntensity = 1f;
		Color shadowColour = new Color(0f, 0f, 0f, 0f);
		Vector3 lightDir = new Vector3(1f, 0f, 0.3f);
		if (!m_benchmarkMode)
		{
			m_LightingManager.GetTripleOfCurrentDirectionalLightInfoWithShadowReverse(out revShadowIntensity, out lightDir, out shadowColour);
		}
		m_lightDirectionInCameraSpace = m_Camera.transform.InverseTransformDirection(lightDir.x, lightDir.y, lightDir.z);
		m_SSDLMaterial.SetVector("_LightDirectionInCameraSpace", m_lightDirectionInCameraSpace);
		m_SSDLMaterial.SetColor("_Colour", shadowColour);
		m_SSDLMaterial.SetFloat("_Intensity", revShadowIntensity);
	}
}
