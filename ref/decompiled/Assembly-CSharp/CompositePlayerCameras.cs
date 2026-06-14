using System;
using UnityEngine;

[AddComponentMenu("Image Effects/Rendering/Composite Player Cameras")]
[RequireComponent(typeof(Camera))]
public class CompositePlayerCameras : T17MonoBehaviour
{
	private static CompositePlayerCameras m_Instance;

	private CameraManager m_CameraManager;

	public OverscanCamera[] m_PlayerCameras;

	public Material m_RenderMaterial;

	private Vector4[] m_CameraViewports;

	private Vector4[] m_InverseCameraViewports;

	private int m_ActiveCamerasCount;

	private Camera m_Camera;

	public Camera camera_
	{
		get
		{
			if (m_Camera == null)
			{
				m_Camera = GetComponent<Camera>();
			}
			return m_Camera;
		}
	}

	public static CompositePlayerCameras GetInstance()
	{
		return m_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		if (m_Instance == null)
		{
			m_Instance = this;
		}
		m_ActiveCamerasCount = -1;
	}

	private void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
			return;
		}
		m_Camera = GetComponent<Camera>();
		m_CameraManager = CameraManager.GetInstance();
		CameraManager cameraManager = m_CameraManager;
		cameraManager.OnActiveCamerasUpdated = (CameraManager.CameraManagerHandler)Delegate.Combine(cameraManager.OnActiveCamerasUpdated, new CameraManager.CameraManagerHandler(ActiveCamerasUpdated));
	}

	protected virtual void OnDestroy()
	{
		if (m_CameraManager != null)
		{
			CameraManager cameraManager = m_CameraManager;
			cameraManager.OnActiveCamerasUpdated = (CameraManager.CameraManagerHandler)Delegate.Remove(cameraManager.OnActiveCamerasUpdated, new CameraManager.CameraManagerHandler(ActiveCamerasUpdated));
			m_CameraManager = null;
		}
		int num = m_PlayerCameras.Length;
		for (int i = 0; i < num; i++)
		{
			m_PlayerCameras[i] = null;
		}
		m_PlayerCameras = null;
		if (m_Instance != null)
		{
			m_Instance = null;
		}
	}

	private void SetUpSizes()
	{
		RenderTexture targetTexture = m_PlayerCameras[0].PlayerCamera.targetTexture;
		float num = m_Camera.pixelWidth;
		float num2 = m_Camera.pixelHeight;
		int num3 = (int)(((float)targetTexture.width - num) / 2f);
		int num4 = (int)(((float)targetTexture.height - num2) / 2f);
		float num5 = (float)targetTexture.width / num;
		float value = 1f / num5;
		m_ActiveCamerasCount = m_CameraManager.m_ActiveCameraCount;
		m_RenderMaterial.SetInt("_ActiveCameras", m_ActiveCamerasCount);
		m_RenderMaterial.SetFloat("_OffsetX", (float)num3 / num);
		m_RenderMaterial.SetFloat("_OffsetY", (float)num4 / num2);
		m_RenderMaterial.SetFloat("_Scale", value);
		for (int i = 0; i < m_ActiveCamerasCount; i++)
		{
			m_RenderMaterial.SetTexture("_CamTex" + i, m_PlayerCameras[i].PlayerCamera.targetTexture);
			m_RenderMaterial.SetVector("_CamRect" + i, m_InverseCameraViewports[i]);
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!(source == null) && m_Camera.enabled && m_PlayerCameras != null && m_PlayerCameras.Length != 0)
		{
			Graphics.Blit(source, destination, m_RenderMaterial);
		}
	}

	private void ActiveCamerasUpdated()
	{
		if (m_Camera.enabled)
		{
			Rect[] cameraViewportRects = m_CameraManager.GetCameraViewportRects();
			Rect[] combineCameraViewportRects = m_CameraManager.GetCombineCameraViewportRects();
			m_CameraViewports = new Vector4[cameraViewportRects.Length];
			m_InverseCameraViewports = new Vector4[cameraViewportRects.Length];
			for (int i = 0; i < cameraViewportRects.Length; i++)
			{
				ref Vector4 reference = ref m_CameraViewports[i];
				reference = new Vector4(cameraViewportRects[i].x, cameraViewportRects[i].y, cameraViewportRects[i].width, cameraViewportRects[i].height);
				ref Vector4 reference2 = ref m_InverseCameraViewports[i];
				reference2 = new Vector4(combineCameraViewportRects[i].x, combineCameraViewportRects[i].y, 1f / combineCameraViewportRects[i].width, 1f / combineCameraViewportRects[i].height);
				m_PlayerCameras[i].Rebuild();
				m_PlayerCameras[i].Initialise(m_CameraViewports[i]);
			}
		}
		SetUpSizes();
	}

	public void ToggleCameraCompositing(bool bEnabled)
	{
		for (int i = 0; i < m_ActiveCamerasCount; i++)
		{
			if (!bEnabled)
			{
				m_PlayerCameras[i].PlayerCamera.targetTexture = null;
				m_PlayerCameras[i].PlayerCamera.fieldOfView = 1f;
				m_PlayerCameras[i].PlayerCamera.rect = new Rect(m_CameraViewports[i].x, m_CameraViewports[i].y, m_CameraViewports[i].z, m_CameraViewports[i].w);
				m_CameraManager.m_ForceRefresh = true;
			}
			else
			{
				m_PlayerCameras[i].Rebuild();
				m_PlayerCameras[i].Initialise(m_CameraViewports[i]);
			}
			m_PlayerCameras[i].enabled = bEnabled;
		}
		m_Camera.enabled = bEnabled;
	}

	private void Update()
	{
	}
}
