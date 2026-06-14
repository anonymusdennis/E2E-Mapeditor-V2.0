using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class CustomLightRenderer : MonoBehaviour
{
	private class CameraData
	{
		public CommandBuffer buf;

		public LightOcclusionRenderer occlusion;

		public MaterialPropertyBlock mpb;
	}

	private static CustomLightManager m_CustomLightManager;

	public Shader m_LightShader;

	private Material m_LightMaterial;

	private int propParams = -1;

	private int propColor = -1;

	private int propOcclude = -1;

	private int propOcclusionTex = -1;

	private int propOcclusionTexOffsetX = -1;

	private int propOcclusionTexOffsetY = -1;

	public Mesh m_CubeMesh;

	private Camera m_Camera;

	private Dictionary<Camera, CameraData> m_Cameras = new Dictionary<Camera, CameraData>();

	public static CustomLightManager customLightManager
	{
		get
		{
			if (m_CustomLightManager == null)
			{
				m_CustomLightManager = new CustomLightManager();
			}
			return m_CustomLightManager;
		}
	}

	public void OnDisable()
	{
		foreach (KeyValuePair<Camera, CameraData> camera in m_Cameras)
		{
			if ((bool)camera.Key)
			{
				camera.Key.RemoveCommandBuffer(CameraEvent.AfterLighting, camera.Value.buf);
			}
		}
		Object.DestroyImmediate(m_LightMaterial);
	}

	public void OnEnable()
	{
		CommandBuffer commandBuffer = null;
		LightOcclusionRenderer lightOcclusionRenderer = null;
		MaterialPropertyBlock materialPropertyBlock = null;
		if (m_Camera == null)
		{
			m_Camera = GetComponentInParent<Camera>();
		}
		if (!m_LightMaterial)
		{
			m_LightMaterial = new Material(m_LightShader);
			m_LightMaterial.hideFlags = HideFlags.HideAndDontSave;
			if (propParams == -1)
			{
				propParams = Shader.PropertyToID("_CustomLightParams");
			}
			if (propColor == -1)
			{
				propColor = Shader.PropertyToID("_CustomLightColor");
			}
			if (propOcclude == -1)
			{
				propOcclude = Shader.PropertyToID("_OccludeLight");
			}
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
		}
		if (!m_Cameras.ContainsKey(m_Camera))
		{
			CameraData cameraData = new CameraData();
			commandBuffer = new CommandBuffer();
			commandBuffer.name = "Deferred custom lights";
			materialPropertyBlock = new MaterialPropertyBlock();
			materialPropertyBlock.Clear();
			lightOcclusionRenderer = m_Camera.gameObject.GetComponentInChildren<LightOcclusionRenderer>();
			cameraData.buf = commandBuffer;
			cameraData.occlusion = lightOcclusionRenderer;
			cameraData.mpb = materialPropertyBlock;
			m_Cameras.Add(m_Camera, cameraData);
			m_Camera.AddCommandBuffer(CameraEvent.AfterLighting, commandBuffer);
		}
		else
		{
			m_Camera.AddCommandBuffer(CameraEvent.AfterLighting, m_Cameras[m_Camera].buf);
		}
	}

	public void OnWillRenderObject()
	{
		if (!base.gameObject.activeInHierarchy || !base.enabled)
		{
			OnDisable();
			return;
		}
		Camera current = Camera.current;
		if (!current || current != m_Camera)
		{
			return;
		}
		CommandBuffer commandBuffer = null;
		LightOcclusionRenderer lightOcclusionRenderer = null;
		MaterialPropertyBlock materialPropertyBlock = null;
		RenderTexture renderTexture = null;
		Vector2 zero = Vector2.zero;
		commandBuffer = m_Cameras[current].buf;
		lightOcclusionRenderer = m_Cameras[current].occlusion;
		materialPropertyBlock = m_Cameras[current].mpb;
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
			}
		}
		if (m_CustomLightManager == null || m_CustomLightManager.m_FrameDirty < UpdateManager.frameCount)
		{
			return;
		}
		commandBuffer.Clear();
		materialPropertyBlock.Clear();
		int count = customLightManager.m_Lights.Count;
		CustomLight.LightArea lightArea = (CustomLight.LightArea)(-1);
		bool flag;
		if (renderTexture == null)
		{
			flag = false;
			materialPropertyBlock.SetFloat(propOcclude, 0f);
		}
		else
		{
			flag = true;
		}
		for (int i = 0; i < count; i++)
		{
			CustomLight customLight = customLightManager.m_Lights._items[i];
			if (customLight.enabled && customLight.gameObject.activeSelf)
			{
				materialPropertyBlock.SetVector(propParams, customLight.m_params);
				materialPropertyBlock.SetColor(propColor, customLight.GetLinearColour());
				if (lightArea != customLight.m_lightArea && flag)
				{
					lightArea = customLight.m_lightArea;
					materialPropertyBlock.SetFloat(propOcclude, (float)customLight.m_lightArea);
				}
				commandBuffer.DrawMesh(m_CubeMesh, customLight.m_trs, m_LightMaterial, 0, 0, materialPropertyBlock);
			}
		}
		count = customLightManager.m_LightsDynamic.Count;
		for (int j = 0; j < count; j++)
		{
			CustomLight customLight2 = customLightManager.m_LightsDynamic._items[j];
			if (customLight2.enabled && customLight2.gameObject.activeSelf)
			{
				materialPropertyBlock.SetVector(propParams, customLight2.m_params);
				materialPropertyBlock.SetColor(propColor, customLight2.GetLinearColour());
				if (lightArea != customLight2.m_lightArea && flag)
				{
					lightArea = customLight2.m_lightArea;
					materialPropertyBlock.SetFloat(propOcclude, (float)customLight2.m_lightArea);
				}
				commandBuffer.DrawMesh(m_CubeMesh, customLight2.m_trs, m_LightMaterial, 0, 0, materialPropertyBlock);
			}
		}
	}

	public static void Cleanup()
	{
		if (m_CustomLightManager != null)
		{
			m_CustomLightManager.RemoveAllLights();
			m_CustomLightManager = null;
		}
	}
}
