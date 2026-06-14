using System;
using System.Collections.Generic;
using UnityEngine;

public class WeatherObjectRenderer : MonoBehaviour
{
	private int m_PropOcclusionTex = -1;

	private int m_PropOcclusionTexOffsetX = -1;

	private int m_PropOcclusionTexOffsetY = -1;

	private int m_PropEdgeCompensation = -1;

	private int m_PropRender = -1;

	private bool m_bPropertiesSet;

	private int[] m_PropRenderEffects = new int[WeatherEffectManager.MaxFullscreenWeatherEffects];

	private int[] m_PropInvWeatherScales = new int[WeatherEffectManager.MaxFullscreenWeatherEffects];

	private int[] m_PropWeatherTextures = new int[WeatherEffectManager.MaxFullscreenWeatherEffects];

	private int[] m_PropWeatherTextureUVOffsetXs = new int[WeatherEffectManager.MaxFullscreenWeatherEffects];

	private int[] m_PropWeatherTextureUVOffsetYs = new int[WeatherEffectManager.MaxFullscreenWeatherEffects];

	private int[] m_PropWeatherEffectAlphas = new int[WeatherEffectManager.MaxFullscreenWeatherEffects];

	private int[] m_PropWeatherEffectSpacingX = new int[WeatherEffectManager.MaxFullscreenWeatherEffects];

	private int[] m_PropWeatherEffectSpacingY = new int[WeatherEffectManager.MaxFullscreenWeatherEffects];

	private int[] m_PropWeatherEffectRotation = new int[WeatherEffectManager.MaxFullscreenWeatherEffects];

	private MeshRenderer m_WeatherObjRenderer;

	private Material m_WeatherMaterial;

	private Dictionary<Camera, LightOcclusionRenderer> m_Cameras = new Dictionary<Camera, LightOcclusionRenderer>();

	private Vector2 occRenCamPos = default(Vector2);

	public Camera m_ParentCam;

	public bool m_bIsInCutscene;

	private float[] m_uvOffsets = new float[2];

	private Vector2 m_DeltaVec = default(Vector2);

	private Vector3 m_Temp = default(Vector3);

	private Vector3[] m_Corners = new Vector3[4];

	private static Rect fixedRect = new Rect(0f, 0f, 1f, 1f);

	public void Start()
	{
		m_WeatherObjRenderer = GetComponent<MeshRenderer>();
		m_WeatherMaterial = m_WeatherObjRenderer.material;
		for (int num = WeatherEffectManager.MaxFullscreenWeatherEffects - 1; num >= 0; num--)
		{
			m_PropRenderEffects[num] = -1;
			m_PropInvWeatherScales[num] = -1;
			m_PropWeatherTextures[num] = -1;
			m_PropWeatherTextureUVOffsetXs[num] = -1;
			m_PropWeatherTextureUVOffsetYs[num] = -1;
			m_PropWeatherEffectAlphas[num] = -1;
			m_PropWeatherEffectSpacingX[num] = -1;
			m_PropWeatherEffectSpacingY[num] = -1;
		}
		LightOcclusionRenderer lightOcclusionRenderer = null;
		if (m_Cameras.ContainsKey(m_ParentCam))
		{
			lightOcclusionRenderer = m_Cameras[m_ParentCam];
		}
		else
		{
			lightOcclusionRenderer = m_ParentCam.gameObject.GetComponentInChildren<LightOcclusionRenderer>();
			m_Cameras.Add(m_ParentCam, lightOcclusionRenderer);
		}
		LightOcclusionRenderer lightOcclusionRenderer2 = lightOcclusionRenderer;
		lightOcclusionRenderer2.OnCameraPosUpdated = (LightOcclusionRenderer.LightOcclusionRendererDelegate)Delegate.Combine(lightOcclusionRenderer2.OnCameraPosUpdated, new LightOcclusionRenderer.LightOcclusionRendererDelegate(UpdateOcclusionRenderTextureOffset));
		CameraManager instance = CameraManager.GetInstance();
		if (instance != null)
		{
			instance.OnCameraOpModeChanged = (CameraManager.CameraManagerModeChangeHandler)Delegate.Combine(instance.OnCameraOpModeChanged, new CameraManager.CameraManagerModeChangeHandler(OnCameraModeChanged));
		}
	}

	protected void OnDisable()
	{
		RemoveFunctionReferences();
	}

	protected void OnDestroy()
	{
		RemoveFunctionReferences();
	}

	private void RemoveFunctionReferences()
	{
		LightOcclusionRenderer lightOcclusionRenderer = null;
		lightOcclusionRenderer = ((!m_Cameras.ContainsKey(m_ParentCam)) ? m_ParentCam.gameObject.GetComponentInChildren<LightOcclusionRenderer>() : m_Cameras[m_ParentCam]);
		if (lightOcclusionRenderer != null)
		{
			LightOcclusionRenderer lightOcclusionRenderer2 = lightOcclusionRenderer;
			lightOcclusionRenderer2.OnCameraPosUpdated = (LightOcclusionRenderer.LightOcclusionRendererDelegate)Delegate.Remove(lightOcclusionRenderer2.OnCameraPosUpdated, new LightOcclusionRenderer.LightOcclusionRendererDelegate(UpdateOcclusionRenderTextureOffset));
		}
		CameraManager instance = CameraManager.GetInstance();
		if (instance != null)
		{
			instance.OnCameraOpModeChanged = (CameraManager.CameraManagerModeChangeHandler)Delegate.Remove(instance.OnCameraOpModeChanged, new CameraManager.CameraManagerModeChangeHandler(OnCameraModeChanged));
		}
	}

	public void LateUpdate()
	{
		CameraManager instance = CameraManager.GetInstance();
		CameraManager.CameraBinding cameraBinding = instance.GetCameraBinding(m_ParentCam);
		m_Temp.x = m_ParentCam.transform.position.x;
		m_Temp.y = m_ParentCam.transform.position.y;
		if (cameraBinding != null)
		{
			if (m_bIsInCutscene)
			{
				Vector3 cameraTrackableTarget = instance.GetCameraTrackableTarget(m_ParentCam);
				m_Temp.z = cameraTrackableTarget.z - 0.5f;
			}
			else
			{
				m_Temp.z = cameraBinding.m_NewTargetPosition.z - 0.5f;
			}
		}
		else
		{
			m_Temp.z = 0f;
		}
		m_ParentCam.CalculateFrustumCorners(fixedRect, 0f - m_ParentCam.transform.position.z + m_Temp.z, Camera.MonoOrStereoscopicEye.Mono, m_Corners);
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		for (int i = 0; i < 4; i++)
		{
			ref Vector3 reference = ref m_Corners[i];
			reference = m_ParentCam.transform.TransformVector(m_Corners[i]);
			if (m_Corners[i].x < num)
			{
				num = m_Corners[i].x;
			}
			if (m_Corners[i].y < num2)
			{
				num2 = m_Corners[i].y;
			}
			if (m_Corners[i].x > num3)
			{
				num3 = m_Corners[i].x;
			}
			if (m_Corners[i].y > num4)
			{
				num4 = m_Corners[i].y;
			}
		}
		base.gameObject.transform.position = m_Temp;
		base.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
		m_Temp.x = Mathf.Abs(num3 - num);
		m_Temp.y = Mathf.Abs(num4 - num2);
		m_Temp.z = 1f;
		base.gameObject.transform.localScale = m_Temp;
	}

	public void OnWillRenderObject()
	{
		if (!base.gameObject.activeInHierarchy || !base.enabled || WeatherEffectManager.Instance == null)
		{
			return;
		}
		if (!m_bPropertiesSet)
		{
			m_PropRender = Shader.PropertyToID("_Render");
			m_PropOcclusionTex = Shader.PropertyToID("_OcclusionTexture");
			m_PropOcclusionTexOffsetX = Shader.PropertyToID("_OcclusionTextureOffsetX");
			m_PropOcclusionTexOffsetY = Shader.PropertyToID("_OcclusionTextureOffsetY");
			m_PropEdgeCompensation = Shader.PropertyToID("_EdgeCompensation");
			for (int num = WeatherEffectManager.MaxFullscreenWeatherEffects - 1; num >= 0; num--)
			{
				m_PropRenderEffects[num] = Shader.PropertyToID("_RenderEffect" + num);
				m_PropInvWeatherScales[num] = Shader.PropertyToID("_InvWeatherScale" + num);
				m_PropWeatherTextures[num] = Shader.PropertyToID("_WeatherTexture" + num);
				m_PropWeatherTextureUVOffsetXs[num] = Shader.PropertyToID("_WeatherTextureUVOffsetX" + num);
				m_PropWeatherTextureUVOffsetYs[num] = Shader.PropertyToID("_WeatherTextureUVOffsetY" + num);
				m_PropWeatherEffectAlphas[num] = Shader.PropertyToID("_EffectAlpha" + num);
				m_PropWeatherEffectSpacingX[num] = Shader.PropertyToID("_EffectSpacingX" + num);
				m_PropWeatherEffectSpacingY[num] = Shader.PropertyToID("_EffectSpacingY" + num);
				m_PropWeatherEffectRotation[num] = Shader.PropertyToID("_EffectRotation" + num);
			}
			m_bPropertiesSet = true;
		}
		Camera current = Camera.current;
		if (current != m_ParentCam)
		{
			m_WeatherMaterial.SetInt(m_PropRender, 0);
			return;
		}
		m_WeatherMaterial.SetInt(m_PropRender, 1);
		UpdateOcclusionRenderTextureOffset();
		for (int num2 = WeatherEffectManager.MaxFullscreenWeatherEffects - 1; num2 >= 0; num2--)
		{
			WeatherEffectData effectData = WeatherEffectManager.Instance.GetEffectData(num2);
			if (effectData == null || !effectData.m_EffectEnabled)
			{
				m_WeatherMaterial.SetInt(m_PropRenderEffects[num2], 0);
			}
			else
			{
				m_WeatherMaterial.SetInt(m_PropRenderEffects[num2], 1);
				m_uvOffsets[0] = effectData.m_CurrentXOffset;
				m_uvOffsets[1] = effectData.m_CurrentYOffset;
				m_WeatherMaterial.SetFloat(m_PropInvWeatherScales[num2], 1f / effectData.m_EffectScale);
				m_WeatherMaterial.SetTexture(m_PropWeatherTextures[num2], effectData.m_Texture);
				m_WeatherMaterial.SetFloat(m_PropWeatherTextureUVOffsetXs[num2], m_uvOffsets[0]);
				m_WeatherMaterial.SetFloat(m_PropWeatherTextureUVOffsetYs[num2], m_uvOffsets[1]);
				m_WeatherMaterial.SetFloat(m_PropWeatherEffectSpacingX[num2], effectData.m_EffectSpacingX);
				m_WeatherMaterial.SetFloat(m_PropWeatherEffectSpacingY[num2], effectData.m_EffectSpacingY);
				switch (effectData.m_RotationMode)
				{
				case WeatherEffectData.RotationMode.VelocityBased:
				{
					m_DeltaVec.x = effectData.m_DeltaXOffset;
					m_DeltaVec.y = effectData.m_DeltaYOffset;
					Vector2 normalized = m_DeltaVec.normalized;
					m_WeatherMaterial.SetFloat(m_PropWeatherEffectRotation[num2], (!(m_DeltaVec.x < 0f)) ? ((float)Math.PI / 180f * Vector2.Angle(Vector2.up, normalized)) : (0f - (float)Math.PI / 180f * Vector2.Angle(Vector2.up, normalized)));
					break;
				}
				case WeatherEffectData.RotationMode.Fixed:
					m_WeatherMaterial.SetFloat(m_PropWeatherEffectRotation[num2], (float)Math.PI / 180f * effectData.m_Rotation);
					break;
				default:
					m_WeatherMaterial.SetFloat(m_PropWeatherEffectRotation[num2], 0f);
					break;
				}
				if (effectData.m_EffectAlphaCurve.length > 0)
				{
					float value = effectData.m_EffectAlphaCurve.Evaluate(effectData.m_CurrentEffectTime);
					m_WeatherMaterial.SetFloat(m_PropWeatherEffectAlphas[num2], Mathf.Clamp(value, 0f, 1f));
				}
				else
				{
					m_WeatherMaterial.SetFloat(m_PropWeatherEffectAlphas[num2], 1f);
				}
			}
		}
	}

	private void UpdateOcclusionRenderTextureOffset()
	{
		LightOcclusionRenderer lightOcclusionRenderer = null;
		RenderTexture renderTexture = null;
		if (m_Cameras.ContainsKey(m_ParentCam))
		{
			lightOcclusionRenderer = m_Cameras[m_ParentCam];
		}
		else
		{
			lightOcclusionRenderer = m_ParentCam.gameObject.GetComponentInChildren<LightOcclusionRenderer>();
			m_Cameras.Add(m_ParentCam, lightOcclusionRenderer);
		}
		if (lightOcclusionRenderer != null && m_bPropertiesSet)
		{
			renderTexture = lightOcclusionRenderer.GetOcclusionTexture(out occRenCamPos);
			Vector2 zero = Vector2.zero;
			Vector2 vector = m_ParentCam.transform.position;
			Vector3 vector2 = m_ParentCam.WorldToViewportPoint(occRenCamPos);
			Vector3 vector3 = m_ParentCam.WorldToViewportPoint(vector);
			zero.x = vector3.x - vector2.x;
			zero.y = vector3.y - vector2.y;
			if (renderTexture != null && m_bPropertiesSet)
			{
				m_WeatherMaterial.SetTexture(m_PropOcclusionTex, renderTexture);
				m_WeatherMaterial.SetFloat(m_PropOcclusionTexOffsetX, zero.x);
				m_WeatherMaterial.SetFloat(m_PropOcclusionTexOffsetY, zero.y);
				m_WeatherMaterial.SetFloat(m_PropEdgeCompensation, 0.015f);
			}
		}
	}

	public void OnCameraModeChanged(CameraManager.CameraOpModes newOpMode)
	{
		m_bIsInCutscene = newOpMode == CameraManager.CameraOpModes.Cutscene;
	}
}
