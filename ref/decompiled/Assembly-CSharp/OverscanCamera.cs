using UnityEngine;

public class OverscanCamera : MonoBehaviour
{
	public const float m_OverscanFactor = 1.2f;

	private Camera m_camera;

	private RenderTexture m_cameraTexture;

	private int m_CameraRenderTargetID;

	private int m_OverdrawWidth;

	private int m_OverdrawHeight;

	private float m_defaultFov;

	public RenderTexture CameraTexture => m_cameraTexture;

	public Camera PlayerCamera
	{
		get
		{
			if (m_camera == null)
			{
				m_camera = GetComponent<Camera>();
			}
			return m_camera;
		}
	}

	private void Awake()
	{
		m_camera = GetComponent<Camera>();
		m_defaultFov = m_camera.fieldOfView;
		m_cameraTexture = null;
		m_CameraRenderTargetID = 0;
		Build();
	}

	private void Start()
	{
		Initialise(new Vector4(0f, 0f, 1f, 1f));
	}

	public void Initialise(Vector4 viewPort)
	{
		m_camera.fieldOfView = m_defaultFov * 1.2f;
	}

	private void Build()
	{
		m_OverdrawWidth = (int)((float)Screen.width * 1.2f);
		m_OverdrawHeight = (int)((float)Screen.height * 1.2f);
		m_cameraTexture = RenderTargetManager.RequestRenderTarget(m_OverdrawWidth, m_OverdrawHeight, 16, RenderTextureFormat.ARGB32, ref m_CameraRenderTargetID, "OC2");
		m_camera.targetTexture = m_cameraTexture;
	}

	protected virtual void OnDestroy()
	{
		RenderTargetManager.ReleaseRenderTarget(ref m_CameraRenderTargetID);
	}

	public RenderTexture Rebuild()
	{
		RenderTargetManager.ReleaseRenderTarget(ref m_CameraRenderTargetID);
		Build();
		return m_cameraTexture;
	}
}
