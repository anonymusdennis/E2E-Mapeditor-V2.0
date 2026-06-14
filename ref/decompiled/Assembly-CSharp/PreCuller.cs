using System;
using UnityEngine;

public class PreCuller : MonoBehaviour
{
	private Character m_CharacterTarget;

	private Vector3 m_TargetPosition = Vector3.zero;

	private CullingObjectCollector m_Culler;

	private CameraManager m_CameraManager;

	private int m_CameraIndex = -1;

	private bool m_bEnabled = true;

	[ReadOnly]
	public int CurrentCameraID = 1;

	public Vector3 CameraViewSize;

	private Camera m_Camera;

	private Vector3 m_ActualCurrentCameraViewSize;

	public uint m_UpdateListAfterXFrames = 3u;

	private static uint m_FrameCounter;

	private static int m_CamerasUpdated;

	private static uint m_PreCullUpdateFrequency = 1u;

	[Range(-1f, 6f)]
	public int DEBUG_FloorOverride = -1;

	public static uint PreCullUpdateFrequency => m_PreCullUpdateFrequency;

	public static int UpdateDelayCount(int cameraID)
	{
		return (int)m_PreCullUpdateFrequency - Mathf.Abs((int)m_FrameCounter - cameraID % (int)m_PreCullUpdateFrequency);
	}

	private void Start()
	{
		m_Culler = CullingObjectCollector.GetInstance();
		m_CameraManager = CameraManager.GetInstance();
		CameraManager cameraManager = m_CameraManager;
		cameraManager.OnActiveCamerasUpdated = (CameraManager.CameraManagerHandler)Delegate.Combine(cameraManager.OnActiveCamerasUpdated, new CameraManager.CameraManagerHandler(ActiveCamerasUpdated));
		m_Camera = GetComponent<Camera>();
		if (m_UpdateListAfterXFrames > m_PreCullUpdateFrequency)
		{
			m_PreCullUpdateFrequency = m_UpdateListAfterXFrames;
		}
		else
		{
			m_UpdateListAfterXFrames = m_PreCullUpdateFrequency;
		}
	}

	protected virtual void OnDestroy()
	{
		m_Culler = null;
		if (m_CameraManager != null)
		{
			CameraManager cameraManager = m_CameraManager;
			cameraManager.OnActiveCamerasUpdated = (CameraManager.CameraManagerHandler)Delegate.Remove(cameraManager.OnActiveCamerasUpdated, new CameraManager.CameraManagerHandler(ActiveCamerasUpdated));
		}
		m_CameraManager = null;
	}

	private void OnPreCull()
	{
		bool flag = true;
		if (m_bEnabled && (m_CharacterTarget != null || m_TargetPosition != Vector3.zero))
		{
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			zero = m_CameraManager.GetCameraCullTarget(m_Camera, m_CameraIndex);
			zero2 = m_CameraManager.GetCameraFacadeTarget(m_Camera, m_CameraIndex);
			if (m_TargetPosition != Vector3.zero)
			{
				m_TargetPosition = m_CameraManager.GetCameraTargetPosition(m_Camera);
			}
			CameraView cameraView = m_CameraManager.GetCameraView(m_Camera, m_CameraIndex);
			CullerUpdateMode cullerUpdateMode = m_CameraManager.GetCullerUpdateMode(m_Camera, m_CameraIndex);
			CharacterStencilRenderer characterStencilRenderer = m_CameraManager.GetCharacterStencilRenderer(m_Camera, m_CameraIndex);
			bool flag2 = cullerUpdateMode == CullerUpdateMode.ForcedNextFrameOnly || cullerUpdateMode == CullerUpdateMode.ForcedIndefinitely || m_FrameCounter == CurrentCameraID % m_PreCullUpdateFrequency;
			if (flag2)
			{
				CullingBuckets.ProcessBucketUpdateQueue();
			}
			m_bEnabled = m_Culler.GetMeshRenderers(zero, CurrentCameraID, m_ActualCurrentCameraViewSize, flag2, m_CharacterTarget, cameraView, characterStencilRenderer, zero2);
			m_CamerasUpdated++;
		}
		else
		{
			m_Culler = CullingObjectCollector.GetInstance();
			if (m_Culler.IsEnabled())
			{
				m_bEnabled = true;
			}
			m_CameraIndex = m_CameraManager.GetCameraIndexInManager(m_Camera);
			if (m_CharacterTarget == null)
			{
				m_CharacterTarget = m_CameraManager.GetCameraTargetCharacter(m_Camera);
			}
			if (m_TargetPosition == Vector3.zero)
			{
				m_TargetPosition = m_CameraManager.GetCameraTargetPosition(m_Camera);
			}
		}
	}

	public void ActiveCamerasUpdated()
	{
		if (m_CameraManager != null)
		{
			m_CharacterTarget = m_CameraManager.GetCameraTargetCharacter(m_Camera);
			m_CameraIndex = m_CameraManager.GetCameraIndexInManager(m_Camera);
		}
	}

	private void OnPostRender()
	{
		if (!(m_CameraManager != null))
		{
			return;
		}
		int num = m_CameraManager.GetUsedCameraCountForCulling() - 1;
		if (CurrentCameraID == num)
		{
			if (m_FrameCounter >= m_PreCullUpdateFrequency - 1)
			{
				m_FrameCounter = 0u;
			}
			else
			{
				m_FrameCounter++;
			}
			m_CamerasUpdated = 0;
		}
	}

	public void CalculateActualCameraSize(float fWidthTiles, float fHeightTiles)
	{
		m_ActualCurrentCameraViewSize = CameraViewSize;
		m_ActualCurrentCameraViewSize.x *= fWidthTiles / 36f;
		m_ActualCurrentCameraViewSize.y *= fHeightTiles / 20.25f;
	}
}
