using System;
using Slate;
using UnityEngine;
using UnityEngine.Rendering;

public class LightOcclusionRenderer : MonoBehaviour
{
	public delegate void LightOcclusionRendererDelegate();

	public LightOcclusionRendererDelegate OnCameraPosUpdated;

	public Shader m_Shader;

	private Material m_MaterialValue1_0f;

	private Material m_MaterialValue0_5f;

	private int m_RedChannelValueID = -1;

	private CameraManager m_CameraManager;

	private Camera m_Camera;

	private Character m_CamTarget;

	private CommandBuffer m_CommandBuffer;

	private RenderTexture m_OcclusionMap;

	private int m_OcclusionMapID;

	private LightOcclusionManager.MeshCollection m_LightMeshes;

	public bool m_Occlude = true;

	private int m_Floor = -1;

	private Vector2 m_CamPos;

	private Vector2 m_updateDist = new Vector2(3f, 2f);

	private bool m_bCamerasDirty;

	private bool m_bDirtyOnNextRender;

	private bool m_bShouldHaveValidTexture;

	private bool m_bIsInCutscene;

	private IntPtr m_OcclusionColourBufferPtr = default(IntPtr);

	private void Awake()
	{
		CreateMaterials();
		m_Camera = GetComponentInParent<Camera>();
	}

	private void Start()
	{
		m_CameraManager = CameraManager.GetInstance();
		if (m_CameraManager != null)
		{
			CameraManager cameraManager = m_CameraManager;
			cameraManager.OnActiveCamerasUpdated = (CameraManager.CameraManagerHandler)Delegate.Combine(cameraManager.OnActiveCamerasUpdated, new CameraManager.CameraManagerHandler(ActiveCamerasUpdated));
			CameraManager cameraManager2 = m_CameraManager;
			cameraManager2.OnVsyncOptionChanged = (CameraManager.CameraManagerHandler)Delegate.Combine(cameraManager2.OnVsyncOptionChanged, new CameraManager.CameraManagerHandler(OnVsyncChanged));
			CameraManager cameraManager3 = m_CameraManager;
			cameraManager3.OnCameraOpModeChanged = (CameraManager.CameraManagerModeChangeHandler)Delegate.Combine(cameraManager3.OnCameraOpModeChanged, new CameraManager.CameraManagerModeChangeHandler(OnCameraModeChanged));
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_CameraManager != null)
		{
			CameraManager cameraManager = m_CameraManager;
			cameraManager.OnActiveCamerasUpdated = (CameraManager.CameraManagerHandler)Delegate.Remove(cameraManager.OnActiveCamerasUpdated, new CameraManager.CameraManagerHandler(ActiveCamerasUpdated));
			CameraManager cameraManager2 = m_CameraManager;
			cameraManager2.OnVsyncOptionChanged = (CameraManager.CameraManagerHandler)Delegate.Remove(cameraManager2.OnVsyncOptionChanged, new CameraManager.CameraManagerHandler(OnVsyncChanged));
			CameraManager cameraManager3 = m_CameraManager;
			cameraManager3.OnCameraOpModeChanged = (CameraManager.CameraManagerModeChangeHandler)Delegate.Remove(cameraManager3.OnCameraOpModeChanged, new CameraManager.CameraManagerModeChangeHandler(OnCameraModeChanged));
		}
		m_bShouldHaveValidTexture = false;
	}

	public void OnEnable()
	{
		CreateMaterials();
		CreateCommandBuffer();
	}

	public void OnDisable()
	{
		if (m_OcclusionMap != null)
		{
			RenderTargetManager.ReleaseRenderTarget(ref m_OcclusionMapID);
		}
		if (m_CommandBuffer != null && m_Camera != null)
		{
			m_Camera.RemoveCommandBuffer(CameraEvent.BeforeLighting, m_CommandBuffer);
		}
		if (m_CameraManager != null)
		{
			CameraManager cameraManager = m_CameraManager;
			cameraManager.OnActiveCamerasUpdated = (CameraManager.CameraManagerHandler)Delegate.Remove(cameraManager.OnActiveCamerasUpdated, new CameraManager.CameraManagerHandler(ActiveCamerasUpdated));
			CameraManager cameraManager2 = m_CameraManager;
			cameraManager2.OnVsyncOptionChanged = (CameraManager.CameraManagerHandler)Delegate.Remove(cameraManager2.OnVsyncOptionChanged, new CameraManager.CameraManagerHandler(OnVsyncChanged));
			CameraManager cameraManager3 = m_CameraManager;
			cameraManager3.OnCameraOpModeChanged = (CameraManager.CameraManagerModeChangeHandler)Delegate.Remove(cameraManager3.OnCameraOpModeChanged, new CameraManager.CameraManagerModeChangeHandler(OnCameraModeChanged));
		}
		DestroyMaterial(m_MaterialValue1_0f);
		DestroyMaterial(m_MaterialValue0_5f);
		m_bShouldHaveValidTexture = false;
	}

	private void CreateMaterials()
	{
		if (m_Shader.isSupported)
		{
			if (m_MaterialValue1_0f == null)
			{
				m_MaterialValue1_0f = CreateMaterial(m_Shader);
			}
			if (m_MaterialValue0_5f == null)
			{
				m_MaterialValue0_5f = CreateMaterial(m_Shader);
			}
		}
		if (m_RedChannelValueID == -1)
		{
			m_RedChannelValueID = Shader.PropertyToID("_RedChannelValue");
		}
		if (m_RedChannelValueID != -1)
		{
			m_MaterialValue1_0f.SetFloat(m_RedChannelValueID, 1f);
			m_MaterialValue0_5f.SetFloat(m_RedChannelValueID, 0.5f);
		}
	}

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

	private void CreateCommandBuffer()
	{
		m_CommandBuffer = new CommandBuffer();
		m_CommandBuffer.name = "Occlusion meshes";
		m_Camera.AddCommandBuffer(CameraEvent.BeforeLighting, m_CommandBuffer);
	}

	private void Update()
	{
		if (base.transform.localPosition.z != 0f - m_Camera.transform.localPosition.z)
		{
			base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, 0f - m_Camera.transform.localPosition.z);
		}
	}

	public RenderTexture GetOcclusionTexture(out Vector2 camPos)
	{
		camPos = m_CamPos;
		return m_OcclusionMap;
	}

	private void CalcRenderThreshold(Camera cam)
	{
	}

	public void OnWillRenderObject()
	{
		if (!base.gameObject.activeInHierarchy || !base.enabled)
		{
			OnDisable();
			TryDirtyOnNextRender();
			return;
		}
		if (m_LightMeshes == null || m_LightMeshes.Length == 0)
		{
			m_LightMeshes = LightOcclusionManager.GetInstance().Meshes;
			if (m_LightMeshes == null || m_LightMeshes.Length == 0)
			{
				TryDirtyOnNextRender();
				return;
			}
		}
		Camera current = Camera.current;
		if (!current || current != m_Camera)
		{
			TryDirtyOnNextRender();
			return;
		}
		if (m_CamTarget == null)
		{
			m_CamTarget = CameraManager.GetInstance().GetCameraTargetCharacter(current);
		}
		m_CommandBuffer.Clear();
		int num = -1;
		FloorManager.Floor floor = null;
		if (m_bIsInCutscene)
		{
			Cutscene currentPlayingCutscene = CutsceneManagerBase.GetInstance().GetCurrentPlayingCutscene();
			if (!(currentPlayingCutscene != null))
			{
				TryDirtyOnNextRender();
				return;
			}
			CutsceneCameraTrackableObject cameraTrackableObject = CutsceneManagerBase.GetCameraTrackableObject();
			if (cameraTrackableObject == null)
			{
				TryDirtyOnNextRender();
				return;
			}
			FloorManager.Floor floor2 = FloorManager.GetInstance().FindFloorbyIndex(cameraTrackableObject.m_FloorIndex);
			if (floor2 == null)
			{
				TryDirtyOnNextRender();
				return;
			}
			num = floor2.m_FloorIndex;
			floor = floor2;
		}
		else
		{
			if (!(m_CamTarget != null) || m_CamTarget.CurrentFloor == null)
			{
				TryDirtyOnNextRender();
				return;
			}
			num = m_CamTarget.CurrentFloor.m_FloorIndex;
			floor = m_CamTarget.CurrentFloor;
		}
		bool flag = num != m_Floor;
		flag |= m_bCamerasDirty;
		if (!flag)
		{
			Vector2 vector = m_Camera.transform.position;
			if (Mathf.Abs(m_CamPos.x - vector.x) > m_updateDist.x || Mathf.Abs(m_CamPos.y - vector.y) > m_updateDist.y)
			{
				flag = true;
			}
			if (!flag && m_bShouldHaveValidTexture)
			{
				if (m_OcclusionMap == null)
				{
					flag = true;
				}
				else if (m_OcclusionMap != null && !m_OcclusionMap.IsCreated())
				{
					flag = true;
				}
				else if (m_Camera.targetTexture != null && (m_OcclusionMap.width != m_Camera.targetTexture.width || m_OcclusionMap.height != m_Camera.targetTexture.height))
				{
					flag = true;
				}
			}
			if (!flag && m_OcclusionMap != null && m_OcclusionMap.IsCreated() && m_OcclusionColourBufferPtr != m_OcclusionMap.GetNativeTexturePtr())
			{
				flag = true;
				m_OcclusionColourBufferPtr = m_OcclusionMap.GetNativeTexturePtr();
			}
		}
		if (flag)
		{
			m_bCamerasDirty = false;
			m_Floor = num;
			m_CamPos = m_Camera.transform.position;
			if (OnCameraPosUpdated != null)
			{
				OnCameraPosUpdated();
			}
			int width = Screen.width;
			int height = Screen.height;
			if (m_Camera.targetTexture != null)
			{
				width = m_Camera.targetTexture.width;
				height = m_Camera.targetTexture.height;
			}
			if (m_OcclusionMap == null || m_OcclusionMap.width != width || m_OcclusionMap.height != height)
			{
				if (m_OcclusionMap != null)
				{
					RenderTargetManager.ReleaseRenderTarget(ref m_OcclusionMapID);
				}
				m_OcclusionMap = RenderTargetManager.RequestRenderTarget(width, height, 0, RenderTextureFormat.R8, ref m_OcclusionMapID, "LOR");
				m_bShouldHaveValidTexture = m_OcclusionMap != null && m_OcclusionMap.IsCreated();
				m_OcclusionColourBufferPtr = m_OcclusionMap.GetNativeTexturePtr();
			}
			m_CommandBuffer.SetRenderTarget(m_OcclusionMap);
			Color backgroundColor = (floor.IsUnderGround() ? Color.white : Color.black);
			m_CommandBuffer.ClearRenderTarget(clearDepth: true, clearColor: true, backgroundColor);
			if (m_Occlude)
			{
				for (int i = 0; i < m_LightMeshes.Length; i++)
				{
					RoomOcclusionMesh mesh = m_LightMeshes.GetMesh(i);
					if (num == mesh.m_FloorIndex)
					{
						m_CommandBuffer.DrawMesh(mesh.m_RoomMesh, mesh.transform.localToWorldMatrix, m_MaterialValue1_0f);
					}
					else if (floor.IsVent() && num - 1 == mesh.m_FloorIndex)
					{
						m_CommandBuffer.DrawMesh(mesh.m_RoomMesh, mesh.transform.localToWorldMatrix, m_MaterialValue0_5f);
					}
				}
			}
		}
		TryDirtyOnNextRender();
	}

	private void TryDirtyOnNextRender()
	{
		if (m_bDirtyOnNextRender)
		{
			m_bCamerasDirty = true;
			m_bDirtyOnNextRender = false;
		}
	}

	private void ActiveCamerasUpdated()
	{
		m_bCamerasDirty = true;
		if (m_CameraManager != null)
		{
			m_CamTarget = m_CameraManager.GetCameraTargetCharacter(m_Camera);
		}
	}

	private void OnVsyncChanged()
	{
		m_bDirtyOnNextRender = true;
	}

	private void OnCameraModeChanged(CameraManager.CameraOpModes newOpMode)
	{
		if (newOpMode == CameraManager.CameraOpModes.Cutscene)
		{
			m_bIsInCutscene = true;
		}
		else
		{
			m_bIsInCutscene = false;
		}
	}
}
