using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

internal class WeatherRenderer : MonoBehaviour
{
	private class CameraData
	{
		public CommandBuffer buf;

		public LightOcclusionRenderer occlusion;

		public MaterialPropertyBlock mpb;
	}

	public Shader m_WeatherShader;

	private Material m_WeatherMaterial;

	private int propOcclusionTex = -1;

	private int propOcclusionTexOffsetX = -1;

	private int propOcclusionTexOffsetY = -1;

	private int propEdgeCompensation = -1;

	private int propInvWeatherScale = -1;

	private int propWeatherTexture = -1;

	private int propWeatherTextureUVOffsetX = -1;

	private int propWeatherTextureUVOffsetY = -1;

	public Mesh m_WeatherMesh;

	public Vector3 m_WeatherMeshRotation = new Vector3(0f, 0f, 0f);

	public Texture2D m_WeatherTexture;

	public AnimationCurve m_XScrollCurve;

	public AnimationCurve m_YScrollCurve;

	public float m_EffectScale = 1f;

	private float m_LastRenderTime;

	private float m_CurrentXOffset;

	private float m_CurrentYOffset;

	private Camera m_Camera;

	private Dictionary<Camera, CameraData> m_Cameras = new Dictionary<Camera, CameraData>();

	public void OnDisable()
	{
		foreach (KeyValuePair<Camera, CameraData> camera in m_Cameras)
		{
			if ((bool)camera.Key)
			{
				camera.Key.RemoveCommandBuffer(CameraEvent.AfterImageEffects, camera.Value.buf);
			}
		}
		Object.DestroyImmediate(m_WeatherMaterial);
	}

	public void OnWillRenderObject()
	{
		if (!base.gameObject.activeInHierarchy || !base.enabled)
		{
			OnDisable();
			return;
		}
		if (m_Camera == null)
		{
			m_Camera = GetComponentInParent<Camera>();
		}
		Camera current = Camera.current;
		if (!current || current != m_Camera)
		{
			return;
		}
		if (!m_WeatherMaterial)
		{
			m_WeatherMaterial = new Material(m_WeatherShader);
			m_WeatherMaterial.hideFlags = HideFlags.HideAndDontSave;
			if (propOcclusionTex == -1)
			{
				propOcclusionTex = Shader.PropertyToID("_OcclusionTexture");
			}
			if (propOcclusionTexOffsetX == -1)
			{
				propOcclusionTexOffsetX = Shader.PropertyToID("_OcclusionTextureOffsetX");
			}
			if (propOcclusionTexOffsetY == -1)
			{
				propOcclusionTexOffsetY = Shader.PropertyToID("_OcclusionTextureOffsetY");
			}
			if (propEdgeCompensation == -1)
			{
				propEdgeCompensation = Shader.PropertyToID("_EdgeCompensation");
			}
			if (propInvWeatherScale == -1)
			{
				propInvWeatherScale = Shader.PropertyToID("_InvWeatherScale");
			}
			if (propWeatherTexture == -1)
			{
				propWeatherTexture = Shader.PropertyToID("_WeatherTexture");
			}
			if (propWeatherTextureUVOffsetX == -1)
			{
				propWeatherTextureUVOffsetX = Shader.PropertyToID("_WeatherTextureUVOffsetX");
			}
			if (propWeatherTextureUVOffsetY == -1)
			{
				propWeatherTextureUVOffsetY = Shader.PropertyToID("_WeatherTextureUVOffsetY");
			}
		}
		CommandBuffer commandBuffer = null;
		LightOcclusionRenderer lightOcclusionRenderer = null;
		MaterialPropertyBlock materialPropertyBlock = null;
		RenderTexture renderTexture = null;
		Vector2 zero = Vector2.zero;
		if (m_Cameras.ContainsKey(current))
		{
			commandBuffer = m_Cameras[current].buf;
			lightOcclusionRenderer = m_Cameras[current].occlusion;
			materialPropertyBlock = m_Cameras[current].mpb;
		}
		else
		{
			CameraData cameraData = new CameraData();
			commandBuffer = new CommandBuffer();
			commandBuffer.name = "Deferred weather";
			materialPropertyBlock = new MaterialPropertyBlock();
			materialPropertyBlock.Clear();
			lightOcclusionRenderer = current.gameObject.GetComponentInChildren<LightOcclusionRenderer>();
			cameraData.buf = commandBuffer;
			cameraData.occlusion = lightOcclusionRenderer;
			cameraData.mpb = materialPropertyBlock;
			m_Cameras.Add(current, cameraData);
			current.AddCommandBuffer(CameraEvent.AfterImageEffects, commandBuffer);
		}
		if (lightOcclusionRenderer != null)
		{
			Vector2 camPos = Vector2.zero;
			renderTexture = lightOcclusionRenderer.GetOcclusionTexture(out camPos);
			Vector2 vector = current.transform.position;
			Vector3 vector2 = current.WorldToViewportPoint(camPos);
			Vector3 vector3 = current.WorldToViewportPoint(vector);
			zero.x = vector3.x - vector2.x;
			zero.y = vector3.y - vector2.y;
			if (renderTexture != null)
			{
				Shader.SetGlobalTexture(propOcclusionTex, renderTexture);
				Shader.SetGlobalFloat(propOcclusionTexOffsetX, zero.x);
				Shader.SetGlobalFloat(propOcclusionTexOffsetY, zero.y);
				Shader.SetGlobalFloat(propEdgeCompensation, 0.015f);
			}
		}
		commandBuffer.Clear();
		materialPropertyBlock.Clear();
		float num = Time.time - m_LastRenderTime;
		m_LastRenderTime = Time.time;
		float[] array = new float[2]
		{
			m_CurrentXOffset += m_XScrollCurve.Evaluate(Mathf.Repeat(Time.time, m_XScrollCurve.length)) * num,
			m_CurrentYOffset += m_YScrollCurve.Evaluate(Mathf.Repeat(Time.time, m_YScrollCurve.length)) * num
		};
		materialPropertyBlock.SetFloat(propInvWeatherScale, 1f / m_EffectScale);
		materialPropertyBlock.SetTexture(propWeatherTexture, m_WeatherTexture);
		materialPropertyBlock.SetFloat(propWeatherTextureUVOffsetX, array[0]);
		materialPropertyBlock.SetFloat(propWeatherTextureUVOffsetY, array[1]);
		Vector3 pos = default(Vector3);
		CameraManager.CameraBinding cameraBinding = CameraManager.GetInstance().GetCameraBinding(current);
		pos = ((cameraBinding == null) ? new Vector3(current.transform.position.x, current.transform.position.y, 0f) : new Vector3(current.transform.position.x, current.transform.position.y, cameraBinding.m_NewTargetPosition.z));
		Vector3[] array2 = new Vector3[4];
		current.CalculateFrustumCorners(new Rect(0f, 0f, 1f, 1f), 0f - current.transform.position.z + cameraBinding.m_NewTargetPosition.z, Camera.MonoOrStereoscopicEye.Mono, array2);
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		for (int i = 0; i < 4; i++)
		{
			ref Vector3 reference = ref array2[i];
			reference = current.transform.TransformVector(array2[i]);
			if (array2[i].x < num2)
			{
				num2 = array2[i].x;
			}
			if (array2[i].y < num3)
			{
				num3 = array2[i].y;
			}
			if (array2[i].x > num4)
			{
				num4 = array2[i].x;
			}
			if (array2[i].y > num5)
			{
				num5 = array2[i].y;
			}
		}
		commandBuffer.DrawMesh(m_WeatherMesh, Matrix4x4.TRS(pos, Quaternion.Euler(m_WeatherMeshRotation), new Vector3(Mathf.Abs(num4 - num2), Mathf.Abs(num5 - num3), 1f)), m_WeatherMaterial, 0, 0, materialPropertyBlock);
	}
}
