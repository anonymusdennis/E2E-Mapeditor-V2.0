using UnityEngine;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Rendering/Global Fog")]
public class GlobalFog : PostEffectsBase
{
	[Tooltip("Colour of the fog")]
	public Color fogColour;

	[Range(0f, 1f)]
	[Tooltip("Density of the fog")]
	public float fogDensity = 0.5f;

	[Tooltip("The position of the actual ground tiles")]
	public float groundPosition;

	[Tooltip("Push fog away from the camera by this amount")]
	public float startDistance;

	public Shader fogShader;

	private Material fogMaterial;

	private Camera m_Camera;

	private Transform m_CamTransform;

	private LightingManager m_LightingManager;

	private Color m_CurrentFogColor;

	private float m_CurrentFogDensity;

	private float m_CurrentFogStartDistance;

	public override bool CheckResources()
	{
		CheckSupport(needDepth: true);
		fogMaterial = CheckShaderAndCreateMaterial(fogShader, fogMaterial);
		if (!isSupported)
		{
			ReportAutoDisable();
		}
		if (m_LightingManager == null)
		{
			m_LightingManager = LightingManager.GetInstance();
		}
		return isSupported;
	}

	protected override void OnDestroy()
	{
		m_LightingManager = null;
		base.OnDestroy();
	}

	[ImageEffectOpaque]
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (source == null)
		{
			return;
		}
		if (!CheckResources())
		{
			Graphics.Blit(source, destination);
			return;
		}
		if (m_Camera == null)
		{
			m_Camera = GetComponent<Camera>();
			m_CamTransform = m_Camera.transform;
		}
		Vector3 cameraCullTarget = CameraManager.GetInstance().GetCameraCullTarget(m_Camera);
		Vector3 position = m_CamTransform.position;
		if (m_LightingManager != null)
		{
			m_CurrentFogColor = m_LightingManager.GetFogColour();
			m_CurrentFogDensity = m_LightingManager.GetFogDensity();
			m_CurrentFogStartDistance = m_LightingManager.GetFogStartDistance();
		}
		float num = cameraCullTarget.z - position.z;
		float num2 = groundPosition + m_CurrentFogStartDistance - position.z;
		float z = 1f / (num2 - num);
		fogMaterial.SetColor("_FogColor", m_CurrentFogColor);
		fogMaterial.SetFloat("_FogDensity", m_CurrentFogDensity);
		fogMaterial.SetVector("_DistanceParams", new Vector4(num, num2, z, 0f));
		Graphics.Blit(source, destination, fogMaterial, 0);
	}
}
