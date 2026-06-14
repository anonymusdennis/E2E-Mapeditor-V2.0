using System.Collections.Generic;

public class LightOcclusionManager : T17MonoBehaviour
{
	public class MeshCollection
	{
		private FastList<RoomOcclusionMesh> m_OcclusionMeshes;

		public int Length => m_OcclusionMeshes.Count;

		public MeshCollection()
		{
			m_OcclusionMeshes = new FastList<RoomOcclusionMesh>();
		}

		public void Add(RoomOcclusionMesh mesh)
		{
			if (!m_OcclusionMeshes.Contains(mesh))
			{
				m_OcclusionMeshes.Add(mesh);
			}
		}

		public void Remove(RoomOcclusionMesh mesh)
		{
			m_OcclusionMeshes.Remove(mesh);
		}

		public void Clear()
		{
			m_OcclusionMeshes.Clear();
		}

		public RoomOcclusionMesh GetMesh(int index)
		{
			return m_OcclusionMeshes._items[index];
		}
	}

	private static LightOcclusionManager m_Instance;

	private MeshCollection m_LightMeshes;

	public MeshCollection Meshes => m_LightMeshes;

	public static LightOcclusionManager GetInstance()
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
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	public void AddOcclusionMesh(RoomOcclusionMesh occlusionMesh)
	{
		if (m_LightMeshes == null)
		{
			m_LightMeshes = new MeshCollection();
		}
		m_LightMeshes.Add(occlusionMesh);
	}

	public void RemoveOcclusionMesh(RoomOcclusionMesh occlusionMesh)
	{
		if (m_LightMeshes != null)
		{
			m_LightMeshes.Remove(occlusionMesh);
		}
	}

	public void OnDisable()
	{
		if (m_LightMeshes != null)
		{
			m_LightMeshes.Clear();
		}
	}
}
