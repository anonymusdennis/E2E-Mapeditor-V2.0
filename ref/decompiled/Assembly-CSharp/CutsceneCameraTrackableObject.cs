using UnityEngine;

[ExecuteInEditMode]
public class CutsceneCameraTrackableObject : CutsceneFlooredMonobehaviour
{
	public CameraView m_CameraView;

	public float m_NearClippingFloorOffset;

	public void Reset()
	{
		m_NearClippingFloorOffset = 0f;
	}
}
