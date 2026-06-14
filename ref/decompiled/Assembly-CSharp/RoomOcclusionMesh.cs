using UnityEngine;

public class RoomOcclusionMesh : MonoBehaviour
{
	public int m_FloorIndex;

	public Mesh m_RoomMesh;

	private LightOcclusionManager m_LightOcclusionManager;

	public void Start()
	{
		m_LightOcclusionManager = LightOcclusionManager.GetInstance();
		if (m_RoomMesh != null)
		{
			m_LightOcclusionManager.AddOcclusionMesh(this);
		}
		else
		{
			base.enabled = false;
		}
	}

	public void OnEnable()
	{
		if (m_LightOcclusionManager == null)
		{
			m_LightOcclusionManager = LightOcclusionManager.GetInstance();
		}
		if (m_LightOcclusionManager != null && m_RoomMesh != null)
		{
			m_LightOcclusionManager.AddOcclusionMesh(this);
		}
	}

	public void OnDisable()
	{
		if (m_LightOcclusionManager == null)
		{
			m_LightOcclusionManager = LightOcclusionManager.GetInstance();
		}
		if (m_LightOcclusionManager != null && m_RoomMesh != null)
		{
			m_LightOcclusionManager.RemoveOcclusionMesh(this);
		}
	}

	protected virtual void OnDestroy()
	{
		m_LightOcclusionManager = null;
	}
}
