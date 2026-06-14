using System;
using System.Collections.Generic;
using Rotorz.Tile;
using UnityEngine;
using UnityEngine.Rendering;

public static class CullingBuckets
{
	public enum RenderableType
	{
		Depth,
		Facade
	}

	[Serializable]
	public class MeshInfo
	{
		public MeshRenderer m_renderer;

		public CullingObjectCollector.AMBIENT_LIGHT_MODE m_AmbientLightMode = CullingObjectCollector.AMBIENT_LIGHT_MODE.ALM_OUTSIDE;

		public MeshInfo(MeshRenderer renderer, CullingObjectCollector.AMBIENT_LIGHT_MODE ambientLightMode)
		{
			m_renderer = renderer;
			m_AmbientLightMode = ambientLightMode;
		}

		public bool LogRenderables(bool actuallyLog)
		{
			if (!actuallyLog)
			{
				return true;
			}
			if (m_renderer.name.Contains("OBJ_") || m_renderer.name.ToLower().Contains("plp_") || m_renderer.name.ToLower().Contains("plop"))
			{
				return true;
			}
			if (m_renderer.sharedMaterial.name.Contains("OBJ_") || m_renderer.sharedMaterial.name.ToLower().Contains("plp_") || m_renderer.sharedMaterial.name.ToLower().Contains("plop"))
			{
				return true;
			}
			Transform parent = m_renderer.transform.parent;
			if (parent != null)
			{
				Debug.LogFormat("\t{0} - {1}\t pos:{2}, parent:{3} @ {4}", m_renderer.name, m_renderer.sharedMaterial.name, m_renderer.transform.position, parent.name, parent.position);
			}
			else
			{
				Debug.LogFormat("\t{0} - {1}\t pos:{2}, parent:none", m_renderer.name, m_renderer.sharedMaterial.name, m_renderer.transform.position);
			}
			Matrix4x4 localToWorldMatrix = m_renderer.transform.localToWorldMatrix;
			Debug.Log(string.Concat("\t\t transform: ", localToWorldMatrix.GetRow(0), ", ", localToWorldMatrix.GetRow(1), ", ", localToWorldMatrix.GetRow(2), ", ", localToWorldMatrix.GetRow(3)));
			MeshFilter component = m_renderer.gameObject.GetComponent<MeshFilter>();
			if (component != null)
			{
				Mesh sharedMesh = component.sharedMesh;
				if (!(sharedMesh != null))
				{
				}
			}
			return true;
		}
	}

	public class BuildingRenderables
	{
		public int m_buildingId;

		public Dictionary<int, FastList<MeshInfo>> m_meshesIndi = new Dictionary<int, FastList<MeshInfo>>();

		public Dictionary<int, FastList<MeshInfo>> m_meshesDamagableCombined = new Dictionary<int, FastList<MeshInfo>>();

		public Dictionary<int, FastList<MeshInfo>> m_meshesDamagableOrig = new Dictionary<int, FastList<MeshInfo>>();

		public Dictionary<int, FastList<MeshInfo>> m_facadeMeshes = new Dictionary<int, FastList<MeshInfo>>();

		public Dictionary<int, FastList<CullingObjectCollector.AnimatedWrapper>> m_animated = new Dictionary<int, FastList<CullingObjectCollector.AnimatedWrapper>>();

		public Dictionary<int, FastList<CullingObjectCollector.AnimatedWrapper>> m_facadeAnimated = new Dictionary<int, FastList<CullingObjectCollector.AnimatedWrapper>>();

		public FastList<CullingObjectCollector.ParticleWrapper> m_particles = new FastList<CullingObjectCollector.ParticleWrapper>();

		public FastList<CullingObjectCollector.ParticleWrapper> m_facadeParticles = new FastList<CullingObjectCollector.ParticleWrapper>();

		public Dictionary<int, FastList<MeshInfo>> m_RenderersWithCustomMaterialBlocks = new Dictionary<int, FastList<MeshInfo>>();

		public Dictionary<int, FastList<CullingObjectCollector.LargeRendererWrapper>> m_LargeRenderer = new Dictionary<int, FastList<CullingObjectCollector.LargeRendererWrapper>>();
	}

	public class RunTimeMeshToBucket
	{
		public MeshRenderer m_MeshRenderer;

		public bool m_VisibleThroughFacade;

		public bool m_VisisbleFromFloorsAbove;

		public bool m_bCheckForMaterialBlock;

		public bool m_bCombined;

		public bool m_bAlsoFloorsAbove;

		public RunTimeMeshToBucket(MeshRenderer mR, bool vTF, bool vFFA, bool cFMB, bool C, bool above)
		{
			m_MeshRenderer = mR;
			m_VisibleThroughFacade = vTF;
			m_VisisbleFromFloorsAbove = vFFA;
			m_bCheckForMaterialBlock = cFMB;
			m_bCombined = C;
			m_bAlsoFloorsAbove = above;
		}
	}

	public class Bucket
	{
		public readonly int m_bucketX;

		public readonly int m_bucketY;

		public readonly int m_bucketZ;

		public Dictionary<int, BuildingRenderables> m_renderables = new Dictionary<int, BuildingRenderables>();

		public MeshRenderer[] m_bakedFloorQuads;

		public FastList<MeshRenderer> m_allRenderers = new FastList<MeshRenderer>();

		public FastList<CullingObjectCollector.ParticleWrapper> m_allParticles = new FastList<CullingObjectCollector.ParticleWrapper>();

		public FastList<CullingObjectCollector.AnimatedWrapper> m_allAnimated = new FastList<CullingObjectCollector.AnimatedWrapper>();

		public FastList<MeshRenderer> m_HugeRenderers = new FastList<MeshRenderer>();

		public FastList<CullingObjectCollector.LargeRendererWrapper> m_allLargeRenderers = new FastList<CullingObjectCollector.LargeRendererWrapper>();

		public bool m_forceUpdate;

		public bool m_RequiresBatching;

		public int m_lightingLastTouched;

		public int m_cameraVisMask;

		public bool m_isVisible;

		public Bucket(int x, int y, int z)
		{
			m_bucketX = x;
			m_bucketY = y;
			m_bucketZ = z;
			m_bakedFloorQuads = new MeshRenderer[z + 1];
		}

		public void SetCameraVisibility(int cameraID, bool isVisible)
		{
			int num = 1 << cameraID;
			if (isVisible)
			{
				m_cameraVisMask |= num;
			}
			else
			{
				m_cameraVisMask &= ~num;
			}
		}

		public void UpdateVisibilityState(bool isVisible)
		{
			m_isVisible = isVisible;
		}

		private FastList<int> GetBuildingIdsFromBitfield(int bitfield)
		{
			FastList<int> fastList = new FastList<int>();
			if (bitfield == 0)
			{
				fastList.Add(0);
			}
			else
			{
				for (int i = 0; i < 31; i++)
				{
					if ((bitfield & (1 << i)) != 0)
					{
						fastList.Add(i + 1);
					}
				}
			}
			return fastList;
		}

		public void AddRenderable(int buildingId, RenderableType type, int depth, MeshRenderer renderer, bool bCombined = false, bool bKnownDam = false)
		{
			bool isEditor = Application.isEditor;
			if (renderer == null || buildingId < 0 || depth < 0)
			{
				return;
			}
			CullingObjectCollector.AMBIENT_LIGHT_MODE ambientLightMode = CullingObjectCollector.AMBIENT_LIGHT_MODE.ALM_OUTSIDE;
			if (m_RoomManager != null)
			{
				RoomFloor floorFromZ = m_RoomManager.GetFloorFromZ(renderer.transform.position.z);
				if (floorFromZ != null)
				{
					Vector3 vector = RoomUtility.WorldToRoomGrid(renderer.transform.position, floorFromZ);
					RoomBlob roomBlob = m_RoomManager.LookUpRoom((int)vector.x, (int)vector.y, floorFromZ);
					if (roomBlob != null && roomBlob.m_subLocation == RoomBlob.RoomSubIdentity_Location.Indoors)
					{
						ambientLightMode = CullingObjectCollector.AMBIENT_LIGHT_MODE.ALM_INSIDE;
					}
				}
			}
			FastList<int> buildingIdsFromBitfield = GetBuildingIdsFromBitfield(buildingId);
			int num = 0;
			for (int i = 0; i < buildingIdsFromBitfield.Count; i++)
			{
				num = buildingIdsFromBitfield._items[i];
				if (!m_renderables.ContainsKey(num))
				{
					BuildingRenderables buildingRenderables = new BuildingRenderables();
					buildingRenderables.m_buildingId = num;
					m_renderables.Add(num, buildingRenderables);
				}
				switch (type)
				{
				case RenderableType.Depth:
				{
					Dictionary<int, FastList<MeshInfo>> dictionary = null;
					GameObject gameObject = renderer.gameObject;
					dictionary = ((isEditor || (!bKnownDam && !(gameObject.GetComponent<DamagableTile>() != null))) ? m_renderables[num].m_meshesIndi : ((!bCombined) ? m_renderables[num].m_meshesDamagableOrig : m_renderables[num].m_meshesDamagableCombined));
					if (dictionary.ContainsKey(depth))
					{
						if (dictionary[depth].Find((MeshInfo x) => x.m_renderer == renderer) == null)
						{
							dictionary[depth].Add(new MeshInfo(renderer, ambientLightMode));
						}
					}
					else
					{
						dictionary.Add(depth, new FastList<MeshInfo>());
						dictionary[depth].Add(new MeshInfo(renderer, ambientLightMode));
					}
					break;
				}
				case RenderableType.Facade:
					if (m_renderables[num].m_facadeMeshes.ContainsKey(depth))
					{
						if (m_renderables[num].m_facadeMeshes[depth].Find((MeshInfo x) => x.m_renderer == renderer) == null)
						{
							m_renderables[num].m_facadeMeshes[depth].Add(new MeshInfo(renderer, ambientLightMode));
						}
					}
					else
					{
						m_renderables[num].m_facadeMeshes.Add(depth, new FastList<MeshInfo>());
						m_renderables[num].m_facadeMeshes[depth].Add(new MeshInfo(renderer, ambientLightMode));
					}
					break;
				}
			}
		}

		public void RemoveRenderable(MeshRenderer mesh)
		{
			if (!(mesh != null))
			{
				return;
			}
			foreach (KeyValuePair<int, BuildingRenderables> renderable in m_renderables)
			{
				MeshInfo meshInfo = null;
				foreach (KeyValuePair<int, FastList<MeshInfo>> item in renderable.Value.m_meshesIndi)
				{
					meshInfo = item.Value.Find((MeshInfo x) => x.m_renderer == mesh);
					if (meshInfo != null)
					{
						item.Value.Remove(meshInfo);
					}
				}
				foreach (KeyValuePair<int, FastList<MeshInfo>> item2 in renderable.Value.m_meshesDamagableOrig)
				{
					meshInfo = item2.Value.Find((MeshInfo x) => x.m_renderer == mesh);
					if (meshInfo != null)
					{
						item2.Value.Remove(meshInfo);
					}
				}
				foreach (KeyValuePair<int, FastList<MeshInfo>> item3 in renderable.Value.m_meshesDamagableCombined)
				{
					meshInfo = item3.Value.Find((MeshInfo x) => x.m_renderer == mesh);
					if (meshInfo != null)
					{
						item3.Value.Remove(meshInfo);
					}
				}
				foreach (KeyValuePair<int, FastList<MeshInfo>> facadeMesh in renderable.Value.m_facadeMeshes)
				{
					meshInfo = facadeMesh.Value.Find((MeshInfo x) => x.m_renderer == mesh);
					if (meshInfo != null)
					{
						facadeMesh.Value.Remove(meshInfo);
					}
				}
			}
		}

		public void AddParticleWrapper(int buildingId, RenderableType type, CullingObjectCollector.ParticleWrapper particle)
		{
			if (particle == null || buildingId < 0)
			{
				return;
			}
			FastList<int> buildingIdsFromBitfield = GetBuildingIdsFromBitfield(buildingId);
			int num = 0;
			for (int i = 0; i < buildingIdsFromBitfield.Count; i++)
			{
				num = buildingIdsFromBitfield._items[i];
				if (!m_renderables.ContainsKey(num))
				{
					BuildingRenderables buildingRenderables = new BuildingRenderables();
					buildingRenderables.m_buildingId = num;
					m_renderables.Add(num, buildingRenderables);
				}
				switch (type)
				{
				case RenderableType.Depth:
					if (!m_renderables[num].m_particles.Contains(particle))
					{
						m_renderables[num].m_particles.Add(particle);
					}
					break;
				case RenderableType.Facade:
					if (!m_renderables[num].m_facadeParticles.Contains(particle))
					{
						m_renderables[num].m_facadeParticles.Add(particle);
					}
					break;
				}
			}
		}

		public void AddAnimatedWrapper(int buildingId, RenderableType type, int depth, CullingObjectCollector.AnimatedWrapper animated)
		{
			if (animated == null || buildingId < 0 || depth < 0)
			{
				return;
			}
			FastList<int> buildingIdsFromBitfield = GetBuildingIdsFromBitfield(buildingId);
			int num = 0;
			for (int i = 0; i < buildingIdsFromBitfield.Count; i++)
			{
				num = buildingIdsFromBitfield._items[i];
				if (!m_renderables.ContainsKey(num))
				{
					BuildingRenderables buildingRenderables = new BuildingRenderables();
					buildingRenderables.m_buildingId = num;
					m_renderables.Add(num, buildingRenderables);
				}
				switch (type)
				{
				case RenderableType.Depth:
					if (m_renderables[num].m_animated.ContainsKey(depth))
					{
						if (!m_renderables[num].m_animated[depth].Contains(animated))
						{
							m_renderables[num].m_animated[depth].Add(animated);
						}
					}
					else
					{
						m_renderables[num].m_animated.Add(depth, new FastList<CullingObjectCollector.AnimatedWrapper>());
						m_renderables[num].m_animated[depth].Add(animated);
					}
					break;
				case RenderableType.Facade:
					if (m_renderables[num].m_facadeAnimated.ContainsKey(depth))
					{
						if (!m_renderables[num].m_facadeAnimated[depth].Contains(animated))
						{
							m_renderables[num].m_facadeAnimated[depth].Add(animated);
						}
					}
					else
					{
						m_renderables[num].m_facadeAnimated.Add(depth, new FastList<CullingObjectCollector.AnimatedWrapper>());
						m_renderables[num].m_facadeAnimated[depth].Add(animated);
					}
					break;
				}
			}
		}

		public void AddLargeRendererWrapper(int buildingId, RenderableType type, int depth, CullingObjectCollector.LargeRendererWrapper lRW)
		{
			FastList<int> buildingIdsFromBitfield = GetBuildingIdsFromBitfield(buildingId);
			int num = 0;
			for (int i = 0; i < buildingIdsFromBitfield.Count; i++)
			{
				num = buildingIdsFromBitfield._items[i];
				if (!m_renderables.ContainsKey(num))
				{
					BuildingRenderables buildingRenderables = new BuildingRenderables();
					buildingRenderables.m_buildingId = num;
					m_renderables.Add(num, buildingRenderables);
				}
				if (type != 0)
				{
					continue;
				}
				if (m_renderables[num].m_LargeRenderer.ContainsKey(depth))
				{
					if (!m_renderables[num].m_LargeRenderer[depth].Contains(lRW))
					{
						m_renderables[num].m_LargeRenderer[depth].Add(lRW);
					}
				}
				else
				{
					m_renderables[num].m_LargeRenderer.Add(depth, new FastList<CullingObjectCollector.LargeRendererWrapper>());
					m_renderables[num].m_LargeRenderer[depth].Add(lRW);
				}
			}
		}

		public void AddRenderableWithCustomMaterialBlock(int buildingId, RenderableType type, int depth, MeshRenderer renderer)
		{
			if (renderer == null || buildingId < 0 || depth < 0)
			{
				return;
			}
			CullingObjectCollector.AMBIENT_LIGHT_MODE ambientLightMode = CullingObjectCollector.AMBIENT_LIGHT_MODE.ALM_OUTSIDE;
			if (m_RoomManager != null)
			{
				RoomFloor floorFromZ = m_RoomManager.GetFloorFromZ(renderer.transform.position.z);
				if (floorFromZ != null)
				{
					Vector3 vector = RoomUtility.WorldToRoomGrid(renderer.transform.position, floorFromZ);
					RoomBlob roomBlob = m_RoomManager.LookUpRoom((int)vector.x, (int)vector.y, floorFromZ);
					if (roomBlob != null && roomBlob.m_subLocation == RoomBlob.RoomSubIdentity_Location.Indoors)
					{
						ambientLightMode = CullingObjectCollector.AMBIENT_LIGHT_MODE.ALM_INSIDE;
					}
				}
			}
			FastList<int> buildingIdsFromBitfield = GetBuildingIdsFromBitfield(buildingId);
			int num = 0;
			for (int i = 0; i < buildingIdsFromBitfield.Count; i++)
			{
				num = buildingIdsFromBitfield._items[i];
				if (!m_renderables.ContainsKey(num))
				{
					BuildingRenderables buildingRenderables = new BuildingRenderables();
					buildingRenderables.m_buildingId = num;
					m_renderables.Add(num, buildingRenderables);
				}
				switch (type)
				{
				case RenderableType.Depth:
					if (m_renderables[num].m_RenderersWithCustomMaterialBlocks.ContainsKey(depth))
					{
						if (m_renderables[num].m_RenderersWithCustomMaterialBlocks[depth].Find((MeshInfo x) => x.m_renderer == renderer) == null)
						{
							m_renderables[num].m_RenderersWithCustomMaterialBlocks[depth].Add(new MeshInfo(renderer, ambientLightMode));
						}
					}
					else
					{
						m_renderables[num].m_RenderersWithCustomMaterialBlocks.Add(depth, new FastList<MeshInfo>());
						m_renderables[num].m_RenderersWithCustomMaterialBlocks[depth].Add(new MeshInfo(renderer, ambientLightMode));
					}
					break;
				}
			}
		}

		public void RemoveRenderableWithCustomMaterialBlock(MeshRenderer mesh)
		{
			if (!(mesh != null))
			{
				return;
			}
			foreach (KeyValuePair<int, BuildingRenderables> renderable in m_renderables)
			{
				MeshInfo meshInfo = null;
				foreach (KeyValuePair<int, FastList<MeshInfo>> renderersWithCustomMaterialBlock in renderable.Value.m_RenderersWithCustomMaterialBlocks)
				{
					meshInfo = renderersWithCustomMaterialBlock.Value.Find((MeshInfo x) => x.m_renderer == mesh);
					if (meshInfo != null)
					{
						renderersWithCustomMaterialBlock.Value.Remove(meshInfo);
					}
				}
			}
		}

		private bool LogRenderables(bool actuallyLog, Dictionary<int, FastList<MeshInfo>> list, string tag)
		{
			bool flag = false;
			if (list.Count > 0)
			{
				if (actuallyLog)
				{
					Debug.LogFormat("{0} x{1}", tag, list.Count);
				}
				foreach (KeyValuePair<int, FastList<MeshInfo>> item in list)
				{
					for (int i = 0; i < item.Value.Count; i++)
					{
						MeshInfo meshInfo = item.Value[i];
						flag |= meshInfo.LogRenderables(actuallyLog);
					}
					if (actuallyLog)
					{
						Debug.Log(string.Empty);
					}
				}
			}
			return flag;
		}

		private void LogRenderables(bool actuallyLog, Dictionary<int, FastList<CullingObjectCollector.AnimatedWrapper>> dict, string tag)
		{
			if (dict.Count <= 0)
			{
				return;
			}
			if (actuallyLog)
			{
				Debug.LogFormat("{0} x{1}", tag, dict.Count);
			}
			foreach (KeyValuePair<int, FastList<CullingObjectCollector.AnimatedWrapper>> item in dict)
			{
				for (int i = 0; i < item.Value.Count; i++)
				{
					item.Value[i].LogRenderables();
				}
				if (actuallyLog)
				{
					Debug.Log(string.Empty);
				}
			}
		}

		private void LogRenderables(FastList<CullingObjectCollector.ParticleWrapper> list, string tag)
		{
			if (list.Count > 0)
			{
				Debug.LogFormat("{0} x{1}", tag, list.Count);
				for (int i = 0; i < list.Count; i++)
				{
					list[i].LogRenderables();
				}
			}
		}

		public bool LogRenderables(bool actuallyLog, string tag = "")
		{
			bool flag = false;
			if (actuallyLog)
			{
				Debug.Log("\n#########################");
				Debug.LogFormat("{3} Bucket {0},{1},{2}", m_bucketX, m_bucketY, m_bucketZ, tag);
			}
			if (m_renderables.Count > 0)
			{
				if (actuallyLog)
				{
					Debug.LogFormat("m_renderables:{0}", m_renderables.Count);
				}
				foreach (KeyValuePair<int, BuildingRenderables> renderable in m_renderables)
				{
					if (actuallyLog)
					{
						Debug.LogFormat("building id:{0}", renderable.Key);
					}
					flag |= LogRenderables(actuallyLog, renderable.Value.m_meshesIndi, "indi");
					flag |= LogRenderables(actuallyLog, renderable.Value.m_meshesDamagableCombined, "damagables combined");
					flag |= LogRenderables(actuallyLog, renderable.Value.m_meshesDamagableOrig, "damagables orig");
					flag |= LogRenderables(actuallyLog, renderable.Value.m_RenderersWithCustomMaterialBlocks, "custom");
				}
			}
			if (actuallyLog && m_bakedFloorQuads.Length > 0)
			{
				Debug.LogFormat("m_bakedFloorQuads:{0}", m_bakedFloorQuads.Length);
				MeshRenderer[] bakedFloorQuads = m_bakedFloorQuads;
				foreach (MeshRenderer meshRenderer in bakedFloorQuads)
				{
					if (!(meshRenderer != null))
					{
						continue;
					}
					if (meshRenderer.sharedMaterial != null)
					{
						Debug.LogFormat("\tfloor:{0}", meshRenderer.sharedMaterial.name);
					}
					MeshFilter component = meshRenderer.gameObject.GetComponent<MeshFilter>();
					if (!(component != null))
					{
						continue;
					}
					Mesh mesh = component.mesh;
					if (!(mesh != null))
					{
						continue;
					}
					Vector3[] vertices = mesh.vertices;
					if (vertices != null)
					{
						for (int j = 0; j < vertices.Length; j++)
						{
							Vector3 vector = vertices[j];
							Debug.LogFormat("\t\t vert {0} {1},{2},{3}", j, vector.x, vector.y, vector.z);
						}
					}
					Vector2[] uv = mesh.uv;
					int num = uv.Length;
					if (uv != null)
					{
						for (int k = 0; k < num; k++)
						{
							Debug.LogFormat("\t\t uv {0} {1},{2}", k, uv[k].x, uv[k].y);
						}
					}
				}
			}
			return flag;
		}
	}

	private class BucketQueueEntry
	{
		public int x;

		public int y;

		public int z;

		public BucketQueueEntry(Bucket bucket)
		{
			x = bucket.m_bucketX;
			y = bucket.m_bucketY;
			z = bucket.m_bucketZ;
		}
	}

	private static Mesh s_floorQuadMesh = null;

	private static List<GameObject> s_ObjectsToCleanUp = new List<GameObject>();

	private static List<Transform> m_ReusableTransformList = new List<Transform>();

	public static FastList<RunTimeMeshToBucket> m_RunTimeMeshToBuckets = new FastList<RunTimeMeshToBucket>();

	private static int s_bucketSizeX = 10;

	private static int s_bucketSizeY = 6;

	public static int s_tileSystemBoundsX = 0;

	public static int s_tileSystemBoundsY = 0;

	public static Bucket[,,] s_buckets = null;

	public static List<Renderer> s_backGround = new List<Renderer>();

	private static List<Vector3> s_BucketsToUpdateAfterInitialise = new List<Vector3>();

	private static FastList<BucketQueueEntry> m_BucketUpdateQueue = new FastList<BucketQueueEntry>();

	public static FloorManager m_FloorManager = null;

	public static FacadesManager m_FacadesManager = null;

	public static RoomManager m_RoomManager = null;

	public static int bucketSizeX => s_bucketSizeX;

	public static int bucketSizeY => s_bucketSizeY;

	public static bool GetBucketCoordsForWorldPosition(Vector3 position, ref int x, ref int y)
	{
		if (s_buckets == null || m_FloorManager == null || s_tileSystemBoundsX * s_tileSystemBoundsY == 0)
		{
			Debug.LogError("[Culling] system not yet initialized!");
			return false;
		}
		int b = s_buckets.GetLength(0) - 1;
		int b2 = s_buckets.GetLength(1) - 1;
		int a = Mathf.FloorToInt((position.x + (float)(s_tileSystemBoundsX / 2)) / (float)s_bucketSizeX);
		int a2 = Mathf.FloorToInt(((float)(s_tileSystemBoundsY / 2) - position.y) / (float)s_bucketSizeY);
		x = Mathf.Max(Mathf.Min(a, b), 0);
		y = Mathf.Max(Mathf.Min(a2, b2), 0);
		return true;
	}

	public static Bucket GetBucketForWorldPosition(Vector3 position, bool createIfDoesntExist, bool bIsLargeMeshCanBeOutsideTileSystem = false)
	{
		if (s_buckets == null || m_FloorManager == null || s_tileSystemBoundsX * s_tileSystemBoundsY == 0)
		{
			Debug.LogError("[Culling] system not yet initialized!");
			return null;
		}
		int num = Mathf.FloorToInt((position.x + (float)(s_tileSystemBoundsX / 2)) / (float)s_bucketSizeX);
		int num2 = Mathf.FloorToInt(((float)(s_tileSystemBoundsY / 2) - position.y) / (float)s_bucketSizeY);
		int num3 = m_FloorManager.FindFloorIndexForRendererZ(position.z);
		if (num >= 0 && num < s_buckets.GetLength(0) && num2 >= 0 && num2 < s_buckets.GetLength(1) && num3 >= 0 && num3 < s_buckets.GetLength(2))
		{
			Bucket bucket = s_buckets[num, num2, num3];
			if (bucket == null && createIfDoesntExist)
			{
				bucket = new Bucket(num, num2, num3);
				s_buckets[num, num2, num3] = bucket;
			}
			return bucket;
		}
		if (!bIsLargeMeshCanBeOutsideTileSystem)
		{
			Debug.LogErrorFormat("[Culling] invalid bucket index +1+ ({0}, {1}, {2})", num, num2, num3);
		}
		return null;
	}

	public static bool GetWorldPositionFromBucketCoords(int x, int y, int z, FloorManager.Floor floorAtZ, out Vector3 worldPos)
	{
		worldPos = Vector3.zero;
		if (x < 0 || x >= s_buckets.GetLength(0) || y < 0 || y >= s_buckets.GetLength(1) || z < 0 || z >= s_buckets.GetLength(2))
		{
			Debug.LogErrorFormat("[Culling] invalid bucket index +2+ ({0}, {1}, {2})", x, y, z);
			return false;
		}
		if (floorAtZ.m_FloorRootObject == null)
		{
			Debug.LogErrorFormat("[Culling] couldn't match bucket z to a valid floor ({0}, {1}, {2})", x, y, z);
			return false;
		}
		worldPos.x = x * s_bucketSizeX - s_tileSystemBoundsX / 2;
		worldPos.y = s_tileSystemBoundsY / 2 - y * s_bucketSizeY;
		worldPos.z = floorAtZ.m_FloorRootObject.position.z;
		return true;
	}

	public static bool GetWorldPositionFromBucketCoords(int x, int y, int z, out Vector3 worldPos)
	{
		worldPos = Vector3.zero;
		if (s_buckets == null || m_FloorManager == null || s_tileSystemBoundsX * s_tileSystemBoundsY == 0)
		{
			Debug.LogError("[Culling] system not yet initialized!");
			return false;
		}
		if (x < 0 || x >= s_buckets.GetLength(0) || y < 0 || y >= s_buckets.GetLength(1) || z < 0 || z >= s_buckets.GetLength(2))
		{
			Debug.LogErrorFormat("[Culling] invalid bucket index +2+ ({0}, {1}, {2})", x, y, z);
			return false;
		}
		FloorManager.Floor[] floors = m_FloorManager.GetFloors();
		if (floors == null || z >= floors.Length || floors[z] == null || floors[z].m_FloorRootObject == null)
		{
			Debug.LogErrorFormat("[Culling] couldn't match bucket z to a valid floor ({0}, {1}, {2})", x, y, z);
			return false;
		}
		worldPos.x = x * s_bucketSizeX - s_tileSystemBoundsX / 2;
		worldPos.y = s_tileSystemBoundsY / 2 - y * s_bucketSizeY;
		worldPos.z = floors[z].m_FloorRootObject.position.z;
		return true;
	}

	public static void CreateBuckets(List<Transform> sceneRoots, FastList<CullingObjectCollector.ParticleWrapper> particleWrappers, FastList<CullingObjectCollector.AnimatedWrapper> animatedWrappers, FastList<CullingObjectCollector.CharacterWrapper> characterWrappers, FastList<CullingObjectCollector.DeskWrapper> deskWrappers, CullingObjectCollector.LightWrapperContainer[] lightWrapperContainers, FastList<CullingObjectCollector.LargeRendererWrapper> largeRendererWrappers, bool bPreBuildBuckets, bool bUpdateNetworkService = false)
	{
		Debug.Log("[Culling] CreateBuckets");
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		m_FloorManager = FloorManager.GetInstance();
		m_FacadesManager = FacadesManager.GetInstance();
		m_RoomManager = RoomManager.GetInstance();
		if (m_FloorManager == null || m_FacadesManager == null || m_RoomManager == null)
		{
			Debug.LogErrorFormat("[Culling] failed to get reference to FloorManager, FacadesManager and RoomManager");
			return;
		}
		if (!bPreBuildBuckets)
		{
			FastList<MeshRenderer> fastList = new FastList<MeshRenderer>();
			FastList<SkinnedMeshRenderer> fastList2 = new FastList<SkinnedMeshRenderer>();
			FastList<ParticleSystem> fastList3 = new FastList<ParticleSystem>();
			FastList<Light> fastList4 = new FastList<Light>();
			FastList<CustomLight> fastList5 = new FastList<CustomLight>();
			if (bUpdateNetworkService)
			{
				GlobalStart.TimedNetworkService();
			}
			for (int i = 0; i < sceneRoots.Count; i++)
			{
				MeshRenderer[] componentsInChildren = sceneRoots[i].GetComponentsInChildren<MeshRenderer>();
				if (componentsInChildren.Length > 0)
				{
					fastList.AddRange(componentsInChildren);
				}
				if (bUpdateNetworkService)
				{
					GlobalStart.TimedNetworkService();
				}
				SkinnedMeshRenderer[] componentsInChildren2 = sceneRoots[i].GetComponentsInChildren<SkinnedMeshRenderer>();
				if (componentsInChildren2.Length > 0)
				{
					fastList2.AddRange(componentsInChildren2);
				}
				if (bUpdateNetworkService)
				{
					GlobalStart.TimedNetworkService();
				}
				ParticleSystem[] componentsInChildren3 = sceneRoots[i].GetComponentsInChildren<ParticleSystem>();
				if (componentsInChildren3.Length > 0)
				{
					fastList3.AddRange(componentsInChildren3);
				}
				if (bUpdateNetworkService)
				{
					GlobalStart.TimedNetworkService();
				}
				Light[] componentsInChildren4 = sceneRoots[i].GetComponentsInChildren<Light>(includeInactive: true);
				if (componentsInChildren4.Length > 0)
				{
					fastList4.AddRange(componentsInChildren4);
				}
				if (bUpdateNetworkService)
				{
					GlobalStart.TimedNetworkService();
				}
				CustomLight[] componentsInChildren5 = sceneRoots[i].GetComponentsInChildren<CustomLight>(includeInactive: true);
				if (componentsInChildren5.Length > 0)
				{
					fastList5.AddRange(componentsInChildren5);
				}
				if (bUpdateNetworkService)
				{
					GlobalStart.TimedNetworkService();
				}
			}
			Debug.Log("[Culling] CreateBuckets  Create Lists  " + (Time.realtimeSinceStartup - realtimeSinceStartup));
			int num = 0;
			int num2 = 0;
			FastList<TileSystem> fastList6 = new FastList<TileSystem>();
			FastList<TileSystem> fastList7 = new FastList<TileSystem>();
			TileSystem tileSystem = null;
			FloorManager.Floor[] floors = m_FloorManager.GetFloors();
			for (int j = 0; j < floors.Length; j++)
			{
				if (bUpdateNetworkService)
				{
					GlobalStart.TimedNetworkService();
				}
				if (floors[j] != null)
				{
					tileSystem = floors[j].m_TileSystems[0];
					if (tileSystem != null)
					{
						fastList6.Add(tileSystem);
						num = Mathf.Max(num, tileSystem.ColumnCount);
						num2 = Mathf.Max(num2, tileSystem.RowCount);
					}
					tileSystem = floors[j].m_TileSystems[1];
					if (tileSystem != null)
					{
						fastList7.Add(tileSystem);
						num = Mathf.Max(num, tileSystem.ColumnCount);
						num2 = Mathf.Max(num2, tileSystem.RowCount);
					}
				}
			}
			Debug.Log("[Culling] CreateBuckets bound tilesystem  " + (Time.realtimeSinceStartup - realtimeSinceStartup));
			int num3 = num / s_bucketSizeX + ((num % s_bucketSizeX > 0) ? 1 : 0) + 1;
			int num4 = num2 / s_bucketSizeY + ((num2 % s_bucketSizeY > 0) ? 1 : 0) + 1;
			int currentMaxFloor = m_FloorManager.currentMaxFloor;
			s_buckets = new Bucket[num3, num4, currentMaxFloor];
			s_tileSystemBoundsX = num;
			s_tileSystemBoundsY = num2;
			int[] array = new int[5]
			{
				LayerMask.NameToLayer("StaticMapObject"),
				LayerMask.NameToLayer("Wall"),
				LayerMask.NameToLayer("Floor"),
				LayerMask.NameToLayer("Fence"),
				LayerMask.NameToLayer("DynamicMapObject")
			};
			List<GameObject> list = new List<GameObject>();
			SkinnedMeshRenderer skinnedMeshRenderer = null;
			for (int k = 0; k < fastList2.Count; k++)
			{
				if (bUpdateNetworkService)
				{
					GlobalStart.TimedNetworkService();
				}
				skinnedMeshRenderer = fastList2._items[k];
				if (skinnedMeshRenderer != null && skinnedMeshRenderer.enabled && skinnedMeshRenderer.GetComponentInParent<Character>() != null)
				{
					Character componentInParent = skinnedMeshRenderer.GetComponentInParent<Character>();
					if (componentInParent != null && !list.Contains(componentInParent.gameObject))
					{
						list.Add(componentInParent.gameObject);
						CullingObjectCollector.CharacterWrapper characterWrapper = new CullingObjectCollector.CharacterWrapper(componentInParent);
						characterWrapper.Init();
						characterWrappers.Add(characterWrapper);
					}
				}
			}
			Debug.Log("[Culling] CreateBuckets CharacterWrapper  " + (Time.realtimeSinceStartup - realtimeSinceStartup));
			MeshRenderer meshRenderer = null;
			for (int l = 0; l < fastList.Count; l++)
			{
				if (bUpdateNetworkService)
				{
					GlobalStart.TimedNetworkService();
				}
				meshRenderer = fastList._items[l];
				if (!(meshRenderer != null) || !meshRenderer.enabled || (meshRenderer.transform.parent != null && meshRenderer.transform.parent.GetComponent<SwagBagInteraction>() != null))
				{
					continue;
				}
				meshRenderer.enabled = false;
				bool flag = false;
				bool flag2 = false;
				bool flag3 = false;
				for (int m = 0; m < array.Length; m++)
				{
					if (meshRenderer.gameObject.layer == array[m])
					{
						flag3 = true;
						break;
					}
				}
				if (meshRenderer.gameObject.name.Contains("OBJ_ScreenBackground") || meshRenderer.gameObject.name.Contains("OBJ_BeachScene"))
				{
					flag = true;
				}
				else
				{
					if (meshRenderer.gameObject.name.Contains("OBJ_HugeQuad_AlwaysVisible"))
					{
						s_backGround.Add(meshRenderer);
						meshRenderer.enabled = true;
						continue;
					}
					if (meshRenderer.GetComponentInParent<Character>() != null)
					{
						Character componentInParent2 = meshRenderer.GetComponentInParent<Character>();
						if (componentInParent2 != null && !list.Contains(componentInParent2.gameObject))
						{
							list.Add(componentInParent2.gameObject);
							CullingObjectCollector.CharacterWrapper characterWrapper2 = new CullingObjectCollector.CharacterWrapper(componentInParent2);
							characterWrapper2.Init();
							characterWrappers.Add(characterWrapper2);
						}
						continue;
					}
					if (meshRenderer.GetComponentInParent<DeskInteraction>() != null)
					{
						DeskInteraction componentInParent3 = meshRenderer.GetComponentInParent<DeskInteraction>();
						if (componentInParent3 != null && !list.Contains(componentInParent3.gameObject))
						{
							list.Add(componentInParent3.gameObject);
							CullingObjectCollector.DeskWrapper deskWrapper = new CullingObjectCollector.DeskWrapper(componentInParent3.gameObject);
							deskWrapper.UpdateLogic(1, 0);
							deskWrapper.Init();
							deskWrappers.Add(deskWrapper);
						}
						continue;
					}
					if ((bool)meshRenderer.GetComponentInParent<SwagBagInteraction>())
					{
						GameObject gameObject = meshRenderer.transform.parent.gameObject;
						if (!list.Contains(gameObject))
						{
							list.Add(gameObject);
							CullingObjectCollector.DeskWrapper deskWrapper2 = new CullingObjectCollector.DeskWrapper(gameObject);
							deskWrapper2.UpdateLogic(1, 0);
							deskWrapper2.Init();
							deskWrappers.Add(deskWrapper2);
						}
						continue;
					}
					if (meshRenderer.GetComponentInParent<PlopInstance>() != null)
					{
						flag = true;
					}
					else if (meshRenderer.gameObject.GetComponentInParent<Animator>() != null || meshRenderer.gameObject.layer == 17)
					{
						Animator animator = meshRenderer.gameObject.GetComponentInParent<Animator>();
						if (animator == null)
						{
							animator = meshRenderer.gameObject.GetComponent<Animator>();
						}
						if (animator != null && !list.Contains(animator.gameObject))
						{
							list.Add(animator.gameObject);
							CullingObjectCollector.AnimatedWrapper animatedWrapper = new CullingObjectCollector.AnimatedWrapper(animator.gameObject);
							animatedWrapper.Init();
							animatedWrapper.DisableLogic();
							animatedWrappers.Add(animatedWrapper);
							Bucket bucketForWorldPosition = GetBucketForWorldPosition(animator.transform.position, createIfDoesntExist: true);
							if (bucketForWorldPosition != null)
							{
								int buildingIDAtPos = m_FacadesManager.GetBuildingIDAtPos(animator.transform.position);
								bucketForWorldPosition.AddAnimatedWrapper(buildingIDAtPos, RenderableType.Depth, 0, animatedWrapper);
								bucketForWorldPosition.m_allAnimated.Add(animatedWrapper);
							}
							else
							{
								Debug.LogErrorFormat("[Culling] animated mesh: {0} could not be placed into a bucket (out of bounds)", animator);
							}
							continue;
						}
					}
					else if (flag3)
					{
						flag = true;
						LevelScript.PRISON_ENUM prisonEnum = LevelScript.GetInstance().m_LevelSetup.m_LevelInfo.m_PrisonEnum;
						if ((prisonEnum == LevelScript.PRISON_ENUM.DLC04 || prisonEnum == LevelScript.PRISON_ENUM.DLC05) && meshRenderer.bounds.extents.y > 3f)
						{
							flag2 = true;
						}
					}
					else if (meshRenderer.gameObject.transform.parent.gameObject.layer == 17)
					{
						flag = true;
					}
					else if (!meshRenderer.gameObject.name.Contains("Door"))
					{
						meshRenderer.enabled = true;
					}
				}
				if (bUpdateNetworkService)
				{
					GlobalStart.TimedNetworkService();
				}
				if (!flag)
				{
					continue;
				}
				if (meshRenderer.gameObject.name.Contains("OBJ_ScreenBackground") || meshRenderer.gameObject.name.Contains("OBJ_BeachScene"))
				{
					MeshFilter component = meshRenderer.gameObject.GetComponent<MeshFilter>();
					if (!(component != null))
					{
						continue;
					}
					Bounds bounds = component.mesh.bounds;
					Vector3 localScale = meshRenderer.transform.localScale;
					for (int n = 0; (float)n <= bounds.size.x; n++)
					{
						for (int num5 = 0; (float)num5 <= bounds.size.z; num5++)
						{
							Vector3 position = meshRenderer.transform.position + new Vector3(bounds.min.x * localScale.x, bounds.min.z * localScale.z, 0f) + new Vector3((float)n * localScale.x, (float)num5 * localScale.z, 0f);
							position.z = 0f;
							Bucket bucketForWorldPosition2 = GetBucketForWorldPosition(position, createIfDoesntExist: true, bIsLargeMeshCanBeOutsideTileSystem: true);
							if (bucketForWorldPosition2 != null && !bucketForWorldPosition2.m_allRenderers.Contains(meshRenderer))
							{
								bucketForWorldPosition2.m_allRenderers.Add(meshRenderer);
								bucketForWorldPosition2.m_HugeRenderers.Add(meshRenderer);
								meshRenderer.enabled = false;
							}
						}
					}
					continue;
				}
				if (flag2)
				{
					Bounds bounds2 = meshRenderer.bounds;
					int num6 = 1111;
					int num7 = -1;
					int num8 = (int)meshRenderer.bounds.size.x;
					int num9 = (int)meshRenderer.bounds.size.y;
					FastList<Bucket> fastList8 = new FastList<Bucket>();
					for (int num10 = 0; num10 <= num8; num10++)
					{
						for (int num11 = 0; num11 <= num9; num11++)
						{
							Vector3 position2 = new Vector3(meshRenderer.bounds.min.x, meshRenderer.bounds.min.y, 0f) + new Vector3(num10, num11, 0f);
							position2.z = 0f;
							Bucket bucketForWorldPosition3 = GetBucketForWorldPosition(position2, createIfDoesntExist: true, bIsLargeMeshCanBeOutsideTileSystem: true);
							if (bucketForWorldPosition3 != null && !fastList8.Contains(bucketForWorldPosition3))
							{
								fastList8.Add(bucketForWorldPosition3);
								if (bucketForWorldPosition3.m_bucketY < num6)
								{
									num6 = bucketForWorldPosition3.m_bucketY;
								}
								if (bucketForWorldPosition3.m_bucketY > num7)
								{
									num7 = bucketForWorldPosition3.m_bucketY;
								}
							}
						}
					}
					bool flag4 = false;
					if (fastList8.Count >= 2 && (num7 - num6 >= 2 || meshRenderer.bounds.extents.y >= 4f))
					{
						flag4 = true;
						meshRenderer.enabled = false;
						CullingObjectCollector.LargeRendererWrapper largeRendererWrapper = new CullingObjectCollector.LargeRendererWrapper(meshRenderer.gameObject, fastList8, largeRendererWrappers.Count);
						largeRendererWrapper.Init();
						largeRendererWrappers.Add(largeRendererWrapper);
						for (int num10 = 0; num10 < fastList8.Count; num10++)
						{
							int buildingIDAtPos2 = m_FacadesManager.GetBuildingIDAtPos(meshRenderer.transform.position);
							fastList8[num10].AddLargeRendererWrapper(buildingIDAtPos2, RenderableType.Depth, 0, largeRendererWrapper);
							fastList8[num10].m_allLargeRenderers.Add(largeRendererWrapper);
						}
					}
					if (flag4)
					{
						continue;
					}
					Vector3 position3 = meshRenderer.transform.position;
					CullingForceFloor component2 = meshRenderer.transform.gameObject.GetComponent<CullingForceFloor>();
					if (component2 != null)
					{
						if (!component2.m_bALittleBitOfExtraMagic)
						{
							position3.z -= component2.m_ForceZShift;
							meshRenderer.transform.position = position3;
						}
						else
						{
							position3.z -= 5.9999f;
							meshRenderer.transform.position = position3;
						}
					}
					Bucket bucketForWorldPosition4 = GetBucketForWorldPosition(position3, createIfDoesntExist: true);
					if (bucketForWorldPosition4 != null && !bucketForWorldPosition4.m_allRenderers.Contains(meshRenderer))
					{
						bucketForWorldPosition4.m_allRenderers.Add(meshRenderer);
						meshRenderer.enabled = false;
					}
					else if (meshRenderer.transform.parent == null)
					{
						Debug.LogErrorFormat("[Culling] mesh: {0} could not be placed into a bucket (out of bounds)", meshRenderer.name);
					}
					else
					{
						Debug.LogErrorFormat("[Culling] mesh: {0} {1} could not be placed into a bucket (out of bounds)", meshRenderer.name, meshRenderer.transform.parent.name);
					}
					continue;
				}
				Vector3 position4 = meshRenderer.transform.position;
				CullingForceFloor component3 = meshRenderer.transform.gameObject.GetComponent<CullingForceFloor>();
				if (component3 != null)
				{
					if (!component3.m_bALittleBitOfExtraMagic)
					{
						position4.z -= component3.m_ForceZShift;
						meshRenderer.transform.position = position4;
					}
					else
					{
						position4.z -= 5.9999f;
						meshRenderer.transform.position = position4;
					}
				}
				Bucket bucketForWorldPosition5 = GetBucketForWorldPosition(position4, createIfDoesntExist: true);
				if (bucketForWorldPosition5 != null && !bucketForWorldPosition5.m_allRenderers.Contains(meshRenderer))
				{
					bucketForWorldPosition5.m_allRenderers.Add(meshRenderer);
					meshRenderer.enabled = false;
				}
				else if (meshRenderer.transform.parent == null)
				{
					Debug.LogErrorFormat("[Culling] mesh: {0} could not be placed into a bucket (out of bounds)", meshRenderer.name);
				}
				else
				{
					Debug.LogErrorFormat("[Culling] mesh: {0} {1} could not be placed into a bucket (out of bounds)", meshRenderer.name, meshRenderer.transform.parent.name);
				}
			}
			Debug.Log("[Culling] CreateBuckets Go RenderList  " + (Time.realtimeSinceStartup - realtimeSinceStartup));
			if (bUpdateNetworkService)
			{
				GlobalStart.TimedNetworkService();
			}
			for (int num12 = 0; num12 < animatedWrappers.Count; num12++)
			{
				animatedWrappers[num12].CullThisObjectsParticlesSystemsFromList(fastList3);
			}
			ParticleSystem[] items = fastList3._items;
			int count = fastList3.Count;
			for (int num13 = 0; num13 < count; num13++)
			{
				if (bUpdateNetworkService)
				{
					GlobalStart.TimedNetworkService();
				}
				ParticleSystem particleSystem = items[num13];
				if (!particleSystem.name.Contains("Engine_Smoke"))
				{
					CullingObjectCollector.ParticleWrapper particleWrapper = new CullingObjectCollector.ParticleWrapper(particleSystem);
					particleWrapper.SetInvisible();
					particleWrappers.Add(particleWrapper);
					Bucket bucketForWorldPosition6 = GetBucketForWorldPosition(particleSystem.transform.position, createIfDoesntExist: true);
					if (bucketForWorldPosition6 != null)
					{
						int buildingIDAtPos3 = m_FacadesManager.GetBuildingIDAtPos(particleSystem.transform.position);
						bucketForWorldPosition6.AddParticleWrapper(buildingIDAtPos3, RenderableType.Depth, particleWrapper);
						bucketForWorldPosition6.m_allParticles.Add(particleWrapper);
					}
					else
					{
						Debug.LogErrorFormat("[Culling] particle: {0} could not be placed into a bucket (out of bounds)", particleSystem);
					}
				}
			}
			Debug.Log("[Culling] CreateBuckets PArticles  " + (Time.realtimeSinceStartup - realtimeSinceStartup));
			int num14 = 20;
			for (int num15 = 0; num15 < fastList4.Count; num15++)
			{
				if (bUpdateNetworkService)
				{
					GlobalStart.TimedNetworkService();
				}
				if (fastList4._items[num15].type != LightType.Directional)
				{
					CharacterMovement characterMovement = null;
					if (fastList4._items[num15].transform.parent != null)
					{
						characterMovement = fastList4._items[num15].transform.parent.GetComponentInChildren<CharacterMovement>();
					}
					if (characterMovement == null)
					{
						CullingObjectCollector.LightWrapper lightWrapper = new CullingObjectCollector.LightWrapper(fastList4._items[num15]);
						lightWrapper.Init();
						int num16 = ((lightWrapper.m_floorIndex >= num14) ? num14 : lightWrapper.m_floorIndex);
						lightWrapperContainers[num16].m_NormalLightWrappers.Add(lightWrapper);
					}
				}
			}
			for (int num17 = 0; num17 < fastList5.Count; num17++)
			{
				if (bUpdateNetworkService)
				{
					GlobalStart.TimedNetworkService();
				}
				CharacterMovement characterMovement2 = null;
				if (fastList5._items[num17].transform.parent != null)
				{
					characterMovement2 = fastList5._items[num17].transform.parent.GetComponentInChildren<CharacterMovement>();
				}
				if (characterMovement2 == null)
				{
					bool flag5 = fastList5._items[num17].GetComponent<GuardTowerSpotlight>() != null;
					if (fastList5._items[num17].CompareTag("ForceNonStaticLight"))
					{
						flag5 = true;
					}
					CullingObjectCollector.LightWrapper lightWrapper2 = new CullingObjectCollector.LightWrapper(fastList5._items[num17], !flag5);
					lightWrapper2.Init();
					int num18 = ((lightWrapper2.m_floorIndex >= num14 || fastList5._items[num17].m_lightArea == CustomLight.LightArea.OutdoorsOnly) ? num14 : lightWrapper2.m_floorIndex);
					if (lightWrapper2.m_isStatic)
					{
						lightWrapperContainers[num18].m_CustomLightWrappers.Add(lightWrapper2);
					}
					else
					{
						lightWrapperContainers[num18].m_CustomLightDynamicWrappers.Add(lightWrapper2);
					}
				}
			}
			Debug.Log("[Culling] CreateBuckets Lights  " + (Time.realtimeSinceStartup - realtimeSinceStartup));
			if (s_floorQuadMesh == null)
			{
				s_floorQuadMesh = new Mesh();
				Vector3[] array2 = new Vector3[4];
				array2[0].Set(-0.5f, -0.5f, 0f);
				array2[1].Set(0.5f, 0.5f, 0f);
				array2[2].Set(0.5f, -0.5f, 0f);
				array2[3].Set(-0.5f, 0.5f, 0f);
				s_floorQuadMesh.vertices = array2;
				Vector2[] array3 = new Vector2[4];
				array3[0].Set(0f, 0f);
				array3[1].Set(1f, 1f);
				array3[2].Set(1f, 0f);
				array3[3].Set(0f, 1f);
				s_floorQuadMesh.uv = array3;
				Vector3[] array4 = new Vector3[4];
				array4[0].Set(0f, 0f, -1f);
				array4[1].Set(0f, 0f, -1f);
				array4[2].Set(0f, 0f, -1f);
				array4[3].Set(0f, 0f, -1f);
				s_floorQuadMesh.normals = array4;
				s_floorQuadMesh.triangles = new int[6] { 0, 1, 2, 0, 3, 1 };
				s_floorQuadMesh.RecalculateBounds();
			}
			for (int num19 = 0; num19 < currentMaxFloor; num19++)
			{
				if (bUpdateNetworkService)
				{
					GlobalStart.TimedNetworkService();
				}
				if (num19 >= fastList6.Count || num19 >= fastList7.Count)
				{
					Debug.LogErrorFormat("[Culling] no matching wall/floor TileSystem for z: {0}", num19);
					continue;
				}
				Material material = null;
				Vector2 bakedTextureOffset = Vector2.zero;
				bool flag6 = false;
				if (m_FloorManager.m_floorBakeMaterials != null && num19 < m_FloorManager.m_floorBakeMaterials.Length)
				{
					material = m_FloorManager.m_floorBakeMaterials[num19];
					bakedTextureOffset = m_FloorManager.m_FloorBakeTextureOffsets[num19];
					flag6 = material != null && material.mainTexture != null;
				}
				TileSystem tileSystem2 = fastList6._items[num19];
				TileSystem tileSystem3 = fastList7._items[num19];
				FloorManager.Floor floor = m_FloorManager.FindFloorbyIndex(num19);
				bool flag7 = floor.IsVent();
				bool flag8 = floor.IsUnderGround();
				for (int num20 = 0; num20 < num4; num20++)
				{
					for (int num21 = 0; num21 < num3; num21++)
					{
						Bucket bucket = s_buckets[num21, num20, num19];
						if (bucket == null)
						{
							bucket = new Bucket(num21, num20, num19);
							s_buckets[num21, num20, num19] = bucket;
						}
						int num22 = num21 * s_bucketSizeX;
						int num23 = num20 * s_bucketSizeY;
						MeshRenderer meshRenderer2 = null;
						if (flag6)
						{
							bool flag9 = false;
							for (int num24 = 0; num24 < s_bucketSizeY; num24++)
							{
								int row = num23 + num24;
								for (int num25 = 0; num25 < s_bucketSizeX; num25++)
								{
									int column = num22 + num25;
									TileData tileOrNull = tileSystem2.GetTileOrNull(row, column);
									if (tileOrNull != null && tileOrNull.gameObject != null && tileOrNull.HasGameObject)
									{
										meshRenderer2 = CreateBakedFloorQuad(tileSystem2.gameObject.transform, material, bakedTextureOffset, bucket, floor);
										flag9 = true;
									}
									if (flag9)
									{
										break;
									}
								}
								if (flag9)
								{
									break;
								}
							}
						}
						if (meshRenderer2 != null && bucket.m_bakedFloorQuads.Length > 0)
						{
							bucket.m_bakedFloorQuads[0] = meshRenderer2;
						}
						FastList<MeshRenderer> range = bucket.m_allRenderers.GetRange(0, bucket.m_allRenderers.Count);
						for (int num26 = 0; num26 < s_bucketSizeY; num26++)
						{
							int row2 = num23 + num26;
							for (int num27 = 0; num27 < s_bucketSizeX; num27++)
							{
								int column2 = num22 + num27;
								Vector3 vector = tileSystem2.WorldPositionFromTileIndex(row2, column2);
								int buildingIDAtPos4 = m_FacadesManager.GetBuildingIDAtPos(vector);
								MeshRenderer meshRenderer3 = null;
								MeshRenderer meshRenderer4 = null;
								TileData tileOrNull2 = tileSystem2.GetTileOrNull(row2, column2);
								if (tileOrNull2 != null && tileOrNull2.gameObject != null && tileOrNull2.HasGameObject)
								{
									meshRenderer3 = tileOrNull2.gameObject.GetComponent<MeshRenderer>();
									if (meshRenderer3 != null)
									{
										DamagableTile component4 = tileOrNull2.gameObject.GetComponent<DamagableTile>();
										if (meshRenderer2 == null || component4 != null)
										{
											bucket.AddRenderable(buildingIDAtPos4, RenderableType.Depth, 0, meshRenderer3);
										}
										range.Remove(meshRenderer3);
									}
								}
								tileOrNull2 = tileSystem3.GetTileOrNull(row2, column2);
								if (tileOrNull2 != null && tileOrNull2.gameObject != null && tileOrNull2.HasGameObject)
								{
									meshRenderer4 = tileOrNull2.gameObject.GetComponent<MeshRenderer>();
									if (meshRenderer4 != null)
									{
										bucket.AddRenderable(buildingIDAtPos4, RenderableType.Depth, 0, meshRenderer4);
										range.Remove(meshRenderer4);
									}
								}
								for (int num28 = bucket.m_HugeRenderers.Count - 1; num28 >= 0; num28--)
								{
									bucket.AddRenderable(0, RenderableType.Depth, 0, bucket.m_HugeRenderers._items[num28]);
									range.Remove(bucket.m_HugeRenderers._items[num28]);
								}
								bucket.m_HugeRenderers.Clear();
								Vector2 vector2 = vector - tileSystem2.CellSize * 0.5f;
								Vector2 vector3 = vector + tileSystem2.CellSize * 0.5f;
								for (int num29 = range.Count - 1; num29 >= 0; num29--)
								{
									Vector2 vector4 = range._items[num29].transform.position;
									if (vector4.x >= vector2.x && vector4.x <= vector3.x && vector4.y >= vector2.y && vector4.y <= vector3.y)
									{
										bucket.AddRenderable(buildingIDAtPos4, RenderableType.Depth, 0, range._items[num29]);
										range.RemoveAt(num29);
									}
								}
								if (meshRenderer3 == null || meshRenderer3.gameObject.CompareTag("PartialTile") || flag7)
								{
									for (int num30 = num19 - 1; num30 >= 0; num30--)
									{
										if (!m_FloorManager.FindFloorbyIndex(num30).IsVent())
										{
											int num31 = num19 - num30;
											Bucket bucket2 = s_buckets[num21, num20, num30];
											if (bucket2 != null)
											{
												MeshRenderer meshRenderer5 = ((bucket2.m_bakedFloorQuads == null || bucket2.m_bakedFloorQuads.Length <= 0) ? null : bucket2.m_bakedFloorQuads[0]);
												FastList<MeshRenderer> range2 = bucket2.m_allRenderers.GetRange(0, bucket2.m_allRenderers.Count);
												MeshRenderer meshRenderer6 = null;
												tileOrNull2 = fastList6._items[num30].GetTileOrNull(row2, column2);
												if (tileOrNull2 != null && tileOrNull2.gameObject != null && tileOrNull2.HasGameObject)
												{
													meshRenderer6 = tileOrNull2.gameObject.GetComponent<MeshRenderer>();
													if (meshRenderer6 != null)
													{
														DamagableTile component5 = tileOrNull2.gameObject.GetComponent<DamagableTile>();
														if (meshRenderer5 == null || component5 != null)
														{
															bucket.AddRenderable(buildingIDAtPos4, RenderableType.Depth, num31, meshRenderer6);
														}
														range2.Remove(meshRenderer6);
													}
												}
												if (meshRenderer6 != null && meshRenderer5 != null && num31 < bucket.m_bakedFloorQuads.Length && bucket.m_bakedFloorQuads[num31] == null)
												{
													FloorManager.Floor floor2 = m_FloorManager.FindFloorbyIndex(num30);
													if (floor2 == null || (!floor2.IsUnderGround() && !floor2.IsVent()))
													{
														bucket.m_bakedFloorQuads[num31] = meshRenderer5;
													}
												}
												for (int num32 = 0; num32 < range2.Count; num32++)
												{
													Vector2 vector5 = range2._items[num32].transform.position;
													if (vector5.x >= vector2.x && vector5.x <= vector3.x && vector5.y >= vector2.y && vector5.y <= vector3.y)
													{
														bucket.AddRenderable(buildingIDAtPos4, RenderableType.Depth, num31, range2._items[num32]);
													}
												}
												for (int num33 = 0; num33 < bucket2.m_allAnimated.Count; num33++)
												{
													Vector2 vector6 = bucket2.m_allAnimated._items[num33].m_GameObject.transform.position;
													if (vector6.x >= vector2.x && vector6.x <= vector3.x && vector6.y >= vector2.y && vector6.y <= vector3.y)
													{
														bucket.AddAnimatedWrapper(buildingIDAtPos4, RenderableType.Depth, num31, bucket2.m_allAnimated._items[num33]);
													}
												}
												for (int num34 = 0; num34 < bucket2.m_allLargeRenderers.Count; num34++)
												{
													bucket.AddLargeRendererWrapper(buildingIDAtPos4, RenderableType.Depth, num31, bucket2.m_allLargeRenderers._items[num34]);
												}
												if (meshRenderer6 != null && !meshRenderer6.gameObject.CompareTag("PartialTile"))
												{
													break;
												}
											}
										}
									}
								}
								if (buildingIDAtPos4 <= 0 || num19 >= currentMaxFloor - 1 || flag8)
								{
									continue;
								}
								for (int num35 = fastList6.Count - 1; num35 >= 0; num35--)
								{
									int num36 = fastList6.Count - 1 - num35;
									Bucket bucket3 = s_buckets[num21, num20, num35];
									if (bucket3 == null)
									{
										bucket3 = new Bucket(num21, num20, num35);
										s_buckets[num21, num20, num35] = bucket3;
									}
									FastList<MeshRenderer> range3 = bucket3.m_allRenderers.GetRange(0, bucket3.m_allRenderers.Count);
									MeshRenderer meshRenderer7 = null;
									tileOrNull2 = fastList6._items[num35].GetTileOrNull(row2, column2);
									if (tileOrNull2 != null && tileOrNull2.gameObject != null && tileOrNull2.HasGameObject)
									{
										meshRenderer7 = tileOrNull2.gameObject.GetComponent<MeshRenderer>();
										if (meshRenderer7 != null)
										{
											if (num35 > num19 || bucket3.m_bakedFloorQuads[0] == null)
											{
												bucket.AddRenderable(buildingIDAtPos4, RenderableType.Facade, num36, meshRenderer7);
											}
											range3.Remove(meshRenderer7);
										}
									}
									for (int num37 = 0; num37 < range3.Count; num37++)
									{
										Vector2 vector7 = range3._items[num37].transform.position;
										if (vector7.x >= vector2.x && vector7.x <= vector3.x && vector7.y >= vector2.y && vector7.y <= vector3.y)
										{
											bucket.AddRenderable(buildingIDAtPos4, RenderableType.Facade, num36, range3._items[num37]);
										}
									}
									for (int num38 = 0; num38 < bucket3.m_allAnimated.Count; num38++)
									{
										Vector2 vector8 = bucket3.m_allAnimated._items[num38].m_GameObject.transform.position;
										if (vector8.x >= vector2.x && vector8.x <= vector3.x && vector8.y >= vector2.y && vector8.y <= vector3.y)
										{
											bucket.AddAnimatedWrapper(buildingIDAtPos4, RenderableType.Facade, num36, bucket3.m_allAnimated._items[num38]);
										}
									}
									if (num36 == 0)
									{
										for (int num39 = 0; num39 < bucket3.m_allParticles.Count; num39++)
										{
											Vector2 vector9 = bucket3.m_allParticles._items[num39].m_ParticleSystem.transform.position;
											if (vector9.x >= vector2.x && vector9.x <= vector3.x && vector9.y >= vector2.y && vector9.y <= vector3.y)
											{
												bucket.AddParticleWrapper(buildingIDAtPos4, RenderableType.Facade, bucket3.m_allParticles._items[num39]);
											}
										}
									}
									if (meshRenderer7 != null && !meshRenderer7.gameObject.CompareTag("PartialTile"))
									{
										break;
									}
								}
							}
						}
						for (int num40 = 0; num40 < range.Count; num40++)
						{
							bucket.AddRenderable(0, RenderableType.Depth, 0, range[num40]);
						}
						range.Clear();
						if (num19 == currentMaxFloor - 1)
						{
							foreach (KeyValuePair<int, BuildingRenderables> renderable in bucket.m_renderables)
							{
								if (renderable.Key != 0)
								{
									renderable.Value.m_facadeMeshes = renderable.Value.m_meshesIndi;
								}
							}
						}
						if (bucket.m_renderables.Count != 0)
						{
							continue;
						}
						bool flag10 = false;
						for (int num41 = 0; num41 < bucket.m_bakedFloorQuads.Length; num41++)
						{
							if (bucket.m_bakedFloorQuads[num41] != null)
							{
								flag10 = true;
								break;
							}
						}
						if (!flag10)
						{
							s_buckets[num21, num20, num19] = null;
						}
					}
				}
			}
			Debug.Log("[Culling] CreateBuckets BUCKETS  " + (Time.realtimeSinceStartup - realtimeSinceStartup));
			if (!Application.isEditor)
			{
				LevelScript instance = LevelScript.GetInstance();
				if ((bool)instance && instance.m_LevelSetup != null && instance.m_LevelSetup.m_LevelInfo != null && instance.m_LevelSetup.m_LevelInfo.m_PrisonEnum == LevelScript.PRISON_ENUM.CustomPrison)
				{
					int length = s_buckets.GetLength(2);
					int length2 = s_buckets.GetLength(1);
					int length3 = s_buckets.GetLength(0);
					for (int num42 = 0; num42 < length; num42++)
					{
						for (int num43 = 0; num43 < length2; num43++)
						{
							for (int num44 = 0; num44 < length3; num44++)
							{
								Bucket bucket4 = s_buckets[num44, num43, num42];
								if (bucket4 != null)
								{
									bucket4.m_RequiresBatching = true;
								}
							}
						}
					}
				}
				else
				{
					Debug.Log("[Culling] CombineIndiTiles   time to here is " + (Time.realtimeSinceStartup - realtimeSinceStartup));
					CombineIndiTiles();
					Debug.Log("[Culling] InitialCombineDamagables " + (Time.realtimeSinceStartup - realtimeSinceStartup));
					InitialCombineDamagables();
					Debug.Log("[Culling] InitialCombineDamagables done " + (Time.realtimeSinceStartup - realtimeSinceStartup));
				}
			}
			else
			{
				Debug.Log("[Culling] Combine tiles skipped");
			}
			StaticBatchAllFloorQuads();
			Debug.Log("[Culling] StaticBatchAllFloorQuads done");
		}
		else
		{
			LevelScript instance2 = LevelScript.GetInstance();
			int xSize = 0;
			int ySize = 0;
			int zSize = 0;
			instance2.GetTileSystemBounds(ref s_tileSystemBoundsX, ref s_tileSystemBoundsY);
			instance2.GetBucketArraySize(ref xSize, ref ySize, ref zSize);
			s_buckets = new Bucket[xSize, ySize, zSize];
			Dictionary<GameObject, CullingObjectCollector.AnimatedWrapper> dictionary = new Dictionary<GameObject, CullingObjectCollector.AnimatedWrapper>();
			instance2.GetLargeRendererWrappers(out var largeRendererWrappers2, out var data);
			if (bUpdateNetworkService)
			{
				GlobalStart.TimedNetworkService();
			}
			int num45;
			if (largeRendererWrappers2 != null)
			{
				num45 = largeRendererWrappers2.Length;
				for (int num46 = 0; num46 < num45; num46++)
				{
					CullingObjectCollector.LargeRendererWrapper largeRendererWrapper2 = new CullingObjectCollector.LargeRendererWrapper(largeRendererWrappers2[num46], data[num46], data[num46].m_ID);
					largeRendererWrapper2.Init();
					largeRendererWrappers.Add(largeRendererWrapper2);
				}
			}
			for (int num47 = 0; num47 < zSize; num47++)
			{
				for (int num48 = 0; num48 < ySize; num48++)
				{
					for (int num49 = 0; num49 < xSize; num49++)
					{
						if (bUpdateNetworkService)
						{
							GlobalStart.TimedNetworkService();
						}
						instance2.GetBucket(num49, num48, num47, out var lsBucket);
						if (lsBucket == null || !lsBucket.m_bValid)
						{
							continue;
						}
						s_buckets[num49, num48, num47] = new Bucket(num49, num48, num47);
						instance2.GetBucket_FloorQuads(num49, num48, num47, out s_buckets[num49, num48, num47].m_bakedFloorQuads);
						instance2.GetBucket_Renderables(num49, num48, num47, ref s_buckets[num49, num48, num47].m_renderables);
						Dictionary<int, BuildingRenderables>.Enumerator enumerator2 = s_buckets[num49, num48, num47].m_renderables.GetEnumerator();
						while (enumerator2.MoveNext())
						{
							BuildingRenderables value = enumerator2.Current.Value;
							if (value.m_animated.Count > 0)
							{
								Dictionary<int, FastList<CullingObjectCollector.AnimatedWrapper>>.Enumerator enumerator3 = value.m_animated.GetEnumerator();
								while (enumerator3.MoveNext())
								{
									FastList<CullingObjectCollector.AnimatedWrapper> value2 = enumerator3.Current.Value;
									num45 = value2.Count;
									for (int num46 = 0; num46 < num45; num46++)
									{
										CullingObjectCollector.AnimatedWrapper animatedWrapper2 = value2[num46];
										if (!dictionary.ContainsKey(animatedWrapper2.m_GameObject))
										{
											animatedWrapper2.Init();
											animatedWrapper2.DisableLogic();
											animatedWrappers.Add(animatedWrapper2);
											dictionary.Add(animatedWrapper2.m_GameObject, animatedWrapper2);
										}
										else
										{
											CullingObjectCollector.AnimatedWrapper value3 = dictionary[animatedWrapper2.m_GameObject];
											value2[num46] = value3;
										}
									}
								}
							}
							if (value.m_facadeAnimated.Count > 0)
							{
								Dictionary<int, FastList<CullingObjectCollector.AnimatedWrapper>>.Enumerator enumerator4 = value.m_facadeAnimated.GetEnumerator();
								while (enumerator4.MoveNext())
								{
									FastList<CullingObjectCollector.AnimatedWrapper> value4 = enumerator4.Current.Value;
									num45 = value4.Count;
									for (int num46 = 0; num46 < num45; num46++)
									{
										CullingObjectCollector.AnimatedWrapper animatedWrapper3 = value4[num46];
										if (!dictionary.ContainsKey(animatedWrapper3.m_GameObject))
										{
											animatedWrapper3.Init();
											animatedWrapper3.DisableLogic();
											animatedWrappers.Add(animatedWrapper3);
											dictionary.Add(animatedWrapper3.m_GameObject, animatedWrapper3);
										}
										else
										{
											CullingObjectCollector.AnimatedWrapper value5 = dictionary[animatedWrapper3.m_GameObject];
											value4[num46] = value5;
										}
									}
								}
							}
							if (value.m_particles.Count > 0)
							{
								for (int num50 = 0; num50 < value.m_particles.Count; num50++)
								{
									CullingObjectCollector.ParticleWrapper particleWrapper2 = value.m_particles[num50];
									if (!particleWrappers.Contains(particleWrapper2))
									{
										particleWrapper2.SetInvisible();
										particleWrappers.Add(particleWrapper2);
									}
								}
							}
							if (value.m_facadeParticles.Count <= 0)
							{
								continue;
							}
							for (int num51 = 0; num51 < value.m_facadeParticles.Count; num51++)
							{
								CullingObjectCollector.ParticleWrapper particleWrapper3 = value.m_facadeParticles[num51];
								if (!particleWrappers.Contains(particleWrapper3))
								{
									particleWrapper3.SetInvisible();
									particleWrappers.Add(particleWrapper3);
								}
							}
						}
					}
				}
			}
			if (largeRendererWrappers != null)
			{
				for (int num46 = 0; num46 < largeRendererWrappers.Count; num46++)
				{
					largeRendererWrappers[num46].ReSetBuckets(data[num46], data[num46].m_ID);
				}
			}
			if (bUpdateNetworkService)
			{
				GlobalStart.TimedNetworkService();
			}
			dictionary.Clear();
			instance2.GetBackground(out s_backGround);
			instance2.GetCharacterWrappers(out var characterWrappers2);
			num45 = characterWrappers2.Length;
			for (int num46 = 0; num46 < num45; num46++)
			{
				CullingObjectCollector.CharacterWrapper characterWrapper3 = new CullingObjectCollector.CharacterWrapper(characterWrappers2[num46]);
				characterWrapper3.Init();
				characterWrappers.Add(characterWrapper3);
			}
			List<Player> allPlayers = Player.GetAllPlayers();
			num45 = allPlayers.Count;
			for (int num46 = 0; num46 < num45; num46++)
			{
				CullingObjectCollector.CharacterWrapper characterWrapper4 = new CullingObjectCollector.CharacterWrapper(allPlayers[num46]);
				characterWrapper4.Init();
				characterWrappers.Add(characterWrapper4);
			}
			instance2.GetDeskWrappers(out var deskWrappers2);
			num45 = deskWrappers2.Length;
			for (int num46 = 0; num46 < num45; num46++)
			{
				CullingObjectCollector.DeskWrapper deskWrapper3 = new CullingObjectCollector.DeskWrapper(deskWrappers2[num46]);
				deskWrapper3.Init();
				deskWrappers.Add(deskWrapper3);
			}
			instance2.GetLightWrappers(out var normalLightWrappers, out var customLightWrappers);
			num45 = customLightWrappers.Length;
			int num52 = 20;
			for (int num46 = 0; num46 < num45; num46++)
			{
				CustomLight customLight = customLightWrappers[num46];
				CharacterMovement characterMovement3 = null;
				if (customLight.transform.parent != null)
				{
					characterMovement3 = customLight.transform.parent.GetComponentInChildren<CharacterMovement>();
				}
				if (characterMovement3 == null)
				{
					bool flag11 = customLight.GetComponent<GuardTowerSpotlight>() != null;
					if (customLight.CompareTag("ForceNonStaticLight"))
					{
						flag11 = true;
					}
					CullingObjectCollector.LightWrapper lightWrapper3 = new CullingObjectCollector.LightWrapper(customLightWrappers[num46], !flag11);
					lightWrapper3.Init();
					int num53 = ((lightWrapper3.m_floorIndex >= num52 || customLight.m_lightArea == CustomLight.LightArea.OutdoorsOnly) ? num52 : lightWrapper3.m_floorIndex);
					if (lightWrapper3.m_isStatic)
					{
						lightWrapperContainers[num53].m_CustomLightWrappers.Add(lightWrapper3);
					}
					else
					{
						lightWrapperContainers[num53].m_CustomLightDynamicWrappers.Add(lightWrapper3);
					}
				}
			}
			num45 = normalLightWrappers.Length;
			for (int num46 = 0; num46 < num45; num46++)
			{
				CullingObjectCollector.LightWrapper lightWrapper4 = new CullingObjectCollector.LightWrapper(normalLightWrappers[num46]);
				lightWrapper4.Init();
				int num54 = ((lightWrapper4.m_floorIndex >= num52) ? num52 : lightWrapper4.m_floorIndex);
				lightWrapperContainers[num54].m_NormalLightWrappers.Add(lightWrapper4);
			}
		}
		float num55 = Time.realtimeSinceStartup - realtimeSinceStartup;
		Debug.LogFormat("[Culler] CreateBuckets took {0} seconds", num55);
		for (int num56 = 0; num56 < m_RunTimeMeshToBuckets.Count; num56++)
		{
			if (bUpdateNetworkService)
			{
				GlobalStart.TimedNetworkService();
			}
			AddToBucketAtRuntime(m_RunTimeMeshToBuckets[num56].m_MeshRenderer, m_RunTimeMeshToBuckets[num56].m_VisibleThroughFacade, m_RunTimeMeshToBuckets[num56].m_VisisbleFromFloorsAbove, m_RunTimeMeshToBuckets[num56].m_bCheckForMaterialBlock, m_RunTimeMeshToBuckets[num56].m_bCombined, m_RunTimeMeshToBuckets[num56].m_bAlsoFloorsAbove);
		}
		m_RunTimeMeshToBuckets.Clear();
	}

	private static void StaticBatchAllFloorQuads()
	{
		if (s_buckets == null)
		{
			return;
		}
		bool isPlaying = Application.isPlaying;
		List<MeshRenderer> list = new List<MeshRenderer>();
		Dictionary<int, List<GameObject>> dictionary = new Dictionary<int, List<GameObject>>();
		Bucket bucket = null;
		for (int i = 0; i < s_buckets.GetLength(2); i++)
		{
			for (int j = 0; j < s_buckets.GetLength(1); j++)
			{
				for (int k = 0; k < s_buckets.GetLength(0); k++)
				{
					bucket = s_buckets[k, j, i];
					if (bucket == null || bucket.m_bakedFloorQuads == null || bucket.m_bakedFloorQuads.Length <= 0)
					{
						continue;
					}
					MeshRenderer meshRenderer = bucket.m_bakedFloorQuads[0];
					if (meshRenderer != null && isPlaying && meshRenderer.sharedMaterial != null)
					{
						int instanceID = meshRenderer.sharedMaterial.GetInstanceID();
						if (!dictionary.ContainsKey(instanceID))
						{
							dictionary.Add(instanceID, new List<GameObject>());
						}
						dictionary[instanceID].Add(meshRenderer.gameObject);
						list.Add(meshRenderer);
					}
				}
			}
		}
		if (isPlaying)
		{
			for (int l = 0; l < list.Count; l++)
			{
				list[l].enabled = true;
			}
			int num = 0;
			Dictionary<int, List<GameObject>>.Enumerator enumerator = dictionary.GetEnumerator();
			while (enumerator.MoveNext())
			{
				GameObject gameObject = new GameObject($"floorquad_batch_{num++}");
				s_ObjectsToCleanUp.Add(gameObject);
				StaticBatchingUtility.Combine(enumerator.Current.Value.ToArray(), gameObject);
				Debug.LogFormat("[Culling] static batched {0} floor quads", enumerator.Current.Value.Count);
			}
			for (int m = 0; m < list.Count; m++)
			{
				list[m].enabled = false;
			}
		}
	}

	public static bool FilterMeshesToCombine(FastList<MeshInfo> renderers, Dictionary<int, List<GameObject>> meshCombineList, Dictionary<int, List<GameObject>> staticBatching, bool bAddSkippedRenderers, Dictionary<int, string> rootNames = null)
	{
		if (renderers == null)
		{
			return false;
		}
		for (int i = 0; i < renderers.Count; i++)
		{
			if (renderers._items[i] == null)
			{
				continue;
			}
			MeshRenderer renderer = renderers._items[i].m_renderer;
			if (renderer == null)
			{
				continue;
			}
			if (renderer.sharedMaterial == null)
			{
				if (bAddSkippedRenderers)
				{
					AddToBucketAtRuntime(renderer, visibleThroughFacade: true, visisbleFromFloorsAbove: true, bCheckForMaterialBlock: false, bCombined: true);
				}
				continue;
			}
			GameObject gameObject = renderers._items[i].m_renderer.gameObject;
			if (gameObject.layer == 16)
			{
				if (bAddSkippedRenderers)
				{
					AddToBucketAtRuntime(renderer, visibleThroughFacade: true, visisbleFromFloorsAbove: true, bCheckForMaterialBlock: false, bCombined: true);
				}
				continue;
			}
			if (gameObject.GetComponent<PhotonView>() != null)
			{
				if (bAddSkippedRenderers)
				{
					AddToBucketAtRuntime(renderer, visibleThroughFacade: true, visisbleFromFloorsAbove: true, bCheckForMaterialBlock: false, bCombined: true);
				}
				continue;
			}
			if (gameObject.GetComponent<MeshFilter>() == null)
			{
				if (bAddSkippedRenderers)
				{
					AddToBucketAtRuntime(renderer, visibleThroughFacade: true, visisbleFromFloorsAbove: true, bCheckForMaterialBlock: false, bCombined: true);
				}
				continue;
			}
			DamagableTile component = gameObject.GetComponent<DamagableTile>();
			if (component != null && !gameObject.activeSelf)
			{
				continue;
			}
			if (gameObject.name.Contains("Render") || gameObject.name.Contains("Combined") || gameObject.name.Contains("OBJ_ScreenBackground") || gameObject.name.Contains("OBJ_BeachScene") || gameObject.name == "Mat")
			{
				if (bAddSkippedRenderers)
				{
					AddToBucketAtRuntime(renderer, visibleThroughFacade: true, visisbleFromFloorsAbove: true, bCheckForMaterialBlock: false, bCombined: true);
				}
				continue;
			}
			Material sharedMaterial = renderer.sharedMaterial;
			int instanceID = sharedMaterial.GetInstanceID();
			if (rootNames != null)
			{
				string name = sharedMaterial.name;
				if (name.Contains("Shadow"))
				{
					rootNames[instanceID] = "Occluder";
				}
				else if (!rootNames.ContainsKey(instanceID))
				{
					rootNames[instanceID] = name;
				}
			}
			if (!meshCombineList.ContainsKey(instanceID))
			{
				meshCombineList.Add(instanceID, new List<GameObject>());
			}
			meshCombineList[instanceID].Add(renderer.gameObject);
			if (staticBatching != null)
			{
				if (!staticBatching.ContainsKey(instanceID))
				{
					staticBatching[instanceID] = new List<GameObject>();
				}
				staticBatching[instanceID].Add(renderer.gameObject);
			}
		}
		return false;
	}

	public static void CombineMeshList(int x, int y, int z, Dictionary<int, List<GameObject>> meshCombineList, string prefix, Dictionary<int, string> rootNames = null, List<GameObject> objectsToStrip = null)
	{
		Vector3 worldPos = Vector3.zero;
		if (!GetWorldPositionFromBucketCoords(x, y, z, out worldPos))
		{
			return;
		}
		Dictionary<int, List<GameObject>>.Enumerator enumerator = meshCombineList.GetEnumerator();
		while (enumerator.MoveNext())
		{
			List<GameObject> value = enumerator.Current.Value;
			int count = value.Count;
			Transform transform = value[0].GetComponent<MeshFilter>().transform;
			Matrix4x4 worldToLocalMatrix = transform.worldToLocalMatrix;
			Vector3 vector = transform.position - worldPos;
			Vector4 column = worldToLocalMatrix.GetColumn(3);
			Vector3 lossyScale = transform.lossyScale;
			column.x += vector.x / lossyScale.x;
			column.y += vector.y / lossyScale.y;
			column.z += vector.z / lossyScale.z;
			worldToLocalMatrix.SetColumn(3, column);
			CombineInstance[] array = new CombineInstance[count];
			for (int i = 0; i < count; i++)
			{
				MeshFilter component = value[i].GetComponent<MeshFilter>();
				array[i].mesh = component.sharedMesh;
				array[i].transform = worldToLocalMatrix * component.transform.localToWorldMatrix;
				if (objectsToStrip != null)
				{
					objectsToStrip.Add(value[i]);
				}
				else
				{
					value[i].GetComponent<Renderer>().enabled = false;
				}
			}
			string text = prefix + " " + x + "," + y + "," + z;
			if (rootNames != null)
			{
				text = text + " " + rootNames[enumerator.Current.Key];
				if (count > 1)
				{
					text = text + "+" + (count - 1);
				}
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(value[0]);
			gameObject.name = text;
			s_ObjectsToCleanUp.Add(gameObject);
			MeshFilter component2 = gameObject.GetComponent<MeshFilter>();
			component2.sharedMesh = new Mesh();
			component2.sharedMesh.CombineMeshes(array);
			gameObject.transform.position = worldPos;
			AddToBucketAtRuntime(gameObject.GetComponent<MeshRenderer>(), visibleThroughFacade: true, visisbleFromFloorsAbove: true, bCheckForMaterialBlock: false, bCombined: true);
			bool isPlaying = Application.isPlaying;
			Component component3 = gameObject.GetComponent<VentCover>();
			if (component3 != null)
			{
				if (isPlaying)
				{
					UnityEngine.Object.Destroy(component3);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(component3);
				}
			}
			Component component4 = gameObject.GetComponent<DamagableTile>();
			if (component4 != null)
			{
				if (isPlaying)
				{
					UnityEngine.Object.Destroy(component4);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(component4);
				}
			}
			ElectricFence component5 = gameObject.GetComponent<ElectricFence>();
			if (component5 != null && isPlaying)
			{
				component5.SoftDisable();
			}
			Component[] components = gameObject.GetComponents<Component>();
			foreach (Component component6 in components)
			{
				if (!(component6 == null) && !(component6 as Transform) && !(component6 as MeshRenderer) && !(component6 as MeshFilter))
				{
					if (isPlaying)
					{
						UnityEngine.Object.Destroy(component6);
					}
					else
					{
						UnityEngine.Object.DestroyImmediate(component6);
					}
				}
			}
			foreach (Transform item in gameObject.transform)
			{
				m_ReusableTransformList.Add(item);
			}
			for (int num = m_ReusableTransformList.Count - 1; num >= 0; num--)
			{
				m_ReusableTransformList[num].gameObject.SetActive(value: false);
				if (isPlaying)
				{
					UnityEngine.Object.Destroy(m_ReusableTransformList[num].gameObject);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(m_ReusableTransformList[num].gameObject);
				}
			}
			m_ReusableTransformList.Clear();
		}
	}

	public static void CombineStripObjects(List<GameObject> objectsToStrip)
	{
		bool isPlaying = Application.isPlaying;
		while (objectsToStrip.Count > 0)
		{
			MeshRenderer component = objectsToStrip[0].GetComponent<MeshRenderer>();
			RemoveFromBucketAtRuntime(component);
			if (isPlaying)
			{
				UnityEngine.Object.Destroy(component);
				UnityEngine.Object.Destroy(objectsToStrip[0].GetComponent<MeshFilter>());
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(component);
				UnityEngine.Object.DestroyImmediate(objectsToStrip[0].GetComponent<MeshFilter>());
			}
			objectsToStrip.RemoveAt(0);
		}
	}

	public static void CombineStaticBatch(Dictionary<int, List<GameObject>> staticBatching)
	{
		int num = 0;
		foreach (KeyValuePair<int, List<GameObject>> item in staticBatching)
		{
			if (item.Value.Count > 0)
			{
				GameObject gameObject = new GameObject($"batching_root_{num++}");
				s_ObjectsToCleanUp.Add(gameObject);
				StaticBatchingUtility.Combine(item.Value.ToArray(), gameObject);
			}
		}
	}

	public static void Init()
	{
		s_buckets = null;
		s_backGround.Clear();
		s_tileSystemBoundsX = 0;
		s_tileSystemBoundsY = 0;
	}

	public static void CleanUp()
	{
		Debug.Log("Cleaning up static batch objects");
		for (int i = 0; i < s_ObjectsToCleanUp.Count; i++)
		{
			UnityEngine.Object.Destroy(s_ObjectsToCleanUp[i]);
		}
		s_ObjectsToCleanUp.Clear();
		s_BucketsToUpdateAfterInitialise.Clear();
		UnityEngine.Object.Destroy(s_floorQuadMesh);
		s_floorQuadMesh = null;
		Bucket bucket = null;
		if (s_buckets != null)
		{
			int length = s_buckets.GetLength(2);
			int length2 = s_buckets.GetLength(1);
			int length3 = s_buckets.GetLength(0);
			for (int j = 0; j < length; j++)
			{
				for (int k = 0; k < length2; k++)
				{
					for (int l = 0; l < length3; l++)
					{
						bucket = s_buckets[l, k, j];
						if (bucket != null)
						{
							Dictionary<int, BuildingRenderables>.Enumerator enumerator = bucket.m_renderables.GetEnumerator();
							while (enumerator.MoveNext())
							{
								BuildingRenderables value = enumerator.Current.Value;
								Dictionary<int, FastList<MeshInfo>>.Enumerator enumerator2 = value.m_meshesIndi.GetEnumerator();
								while (enumerator2.MoveNext())
								{
									FastList<MeshInfo> value2 = enumerator2.Current.Value;
									value2.Clear();
								}
								enumerator2 = value.m_meshesDamagableCombined.GetEnumerator();
								while (enumerator2.MoveNext())
								{
									FastList<MeshInfo> value3 = enumerator2.Current.Value;
									value3.Clear();
								}
								enumerator2 = value.m_meshesDamagableOrig.GetEnumerator();
								while (enumerator2.MoveNext())
								{
									FastList<MeshInfo> value4 = enumerator2.Current.Value;
									value4.Clear();
								}
								Dictionary<int, FastList<CullingObjectCollector.AnimatedWrapper>>.Enumerator enumerator3 = value.m_animated.GetEnumerator();
								while (enumerator3.MoveNext())
								{
									FastList<CullingObjectCollector.AnimatedWrapper> value5 = enumerator3.Current.Value;
									value5.Clear();
								}
								enumerator3 = value.m_facadeAnimated.GetEnumerator();
								while (enumerator3.MoveNext())
								{
									FastList<CullingObjectCollector.AnimatedWrapper> value6 = enumerator3.Current.Value;
									value6.Clear();
								}
								value.m_particles.Clear();
								value.m_facadeParticles.Clear();
								enumerator2 = value.m_RenderersWithCustomMaterialBlocks.GetEnumerator();
								while (enumerator2.MoveNext())
								{
									FastList<MeshInfo> value7 = enumerator2.Current.Value;
									value7.Clear();
								}
							}
						}
						s_buckets[l, k, j] = null;
					}
				}
			}
		}
		s_buckets = null;
		s_backGround.Clear();
		s_tileSystemBoundsX = 0;
		s_tileSystemBoundsY = 0;
		m_BucketUpdateQueue.Clear();
		m_RunTimeMeshToBuckets.Clear();
		m_FacadesManager = null;
		m_FloorManager = null;
		m_RoomManager = null;
	}

	public static void CombineIndiTiles()
	{
		List<GameObject> objectsToStrip = new List<GameObject>();
		Dictionary<int, List<GameObject>> staticBatching = new Dictionary<int, List<GameObject>>();
		Bucket bucket = null;
		int length = s_buckets.GetLength(2);
		int length2 = s_buckets.GetLength(1);
		int length3 = s_buckets.GetLength(0);
		Dictionary<int, List<GameObject>> dictionary = new Dictionary<int, List<GameObject>>();
		Dictionary<int, string> dictionary2 = new Dictionary<int, string>();
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length2; j++)
			{
				for (int k = 0; k < length3; k++)
				{
					bucket = s_buckets[k, j, i];
					if (bucket == null)
					{
						continue;
					}
					dictionary.Clear();
					dictionary2.Clear();
					Dictionary<int, BuildingRenderables>.Enumerator enumerator = bucket.m_renderables.GetEnumerator();
					while (enumerator.MoveNext())
					{
						BuildingRenderables value = enumerator.Current.Value;
						Dictionary<int, FastList<MeshInfo>>.Enumerator enumerator2 = value.m_meshesIndi.GetEnumerator();
						while (enumerator2.MoveNext())
						{
							FastList<MeshInfo> value2 = enumerator2.Current.Value;
							FilterMeshesToCombine(value2, dictionary, staticBatching, bAddSkippedRenderers: false, dictionary2);
						}
					}
					CombineMeshList(k, j, i, dictionary, "CombinedIndi", dictionary2, objectsToStrip);
				}
			}
		}
		CombineStripObjects(objectsToStrip);
		CombineStaticBatch(staticBatching);
	}

	public static void InitialCombineDamagables()
	{
		Bucket bucket = null;
		Dictionary<int, List<GameObject>> dictionary = new Dictionary<int, List<GameObject>>();
		for (int i = 0; i < s_buckets.GetLength(2); i++)
		{
			for (int j = 0; j < s_buckets.GetLength(1); j++)
			{
				for (int k = 0; k < s_buckets.GetLength(0); k++)
				{
					bucket = s_buckets[k, j, i];
					if (bucket == null)
					{
						continue;
					}
					Dictionary<int, List<GameObject>> meshCombineList = new Dictionary<int, List<GameObject>>();
					Dictionary<int, string> rootNames = new Dictionary<int, string>();
					Dictionary<int, BuildingRenderables>.Enumerator enumerator = bucket.m_renderables.GetEnumerator();
					while (enumerator.MoveNext())
					{
						BuildingRenderables value = enumerator.Current.Value;
						Dictionary<int, FastList<MeshInfo>>.Enumerator enumerator2 = value.m_meshesDamagableOrig.GetEnumerator();
						while (enumerator2.MoveNext())
						{
							FastList<MeshInfo> value2 = enumerator2.Current.Value;
							FilterMeshesToCombine(value2, meshCombineList, dictionary, bAddSkippedRenderers: true, rootNames);
						}
					}
					CombineMeshList(k, j, i, meshCombineList, "CombinedDam", rootNames);
					CombineStaticBatch(dictionary);
					dictionary.Clear();
				}
			}
		}
	}

	public static void RecombineBucketDamagable(Bucket bucket)
	{
		Dictionary<int, List<GameObject>> meshCombineList = new Dictionary<int, List<GameObject>>();
		Dictionary<int, List<GameObject>> staticBatching = new Dictionary<int, List<GameObject>>();
		Dictionary<int, BuildingRenderables>.Enumerator enumerator = bucket.m_renderables.GetEnumerator();
		while (enumerator.MoveNext())
		{
			BuildingRenderables value = enumerator.Current.Value;
			Dictionary<int, FastList<MeshInfo>>.Enumerator enumerator2 = value.m_meshesDamagableCombined.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				FastList<MeshInfo> value2 = enumerator2.Current.Value;
				while (value2.Count > 0)
				{
					MeshRenderer renderer = value2[0].m_renderer;
					GameObject gameObject = renderer.gameObject;
					RemoveFromBucketAtRuntime(renderer);
					renderer.enabled = false;
					UnityEngine.Object.Destroy(renderer);
					UnityEngine.Object.Destroy(gameObject.GetComponent<MeshFilter>());
					UnityEngine.Object.Destroy(gameObject);
				}
			}
			Dictionary<int, FastList<MeshInfo>>.Enumerator enumerator3 = value.m_meshesDamagableOrig.GetEnumerator();
			while (enumerator3.MoveNext())
			{
				FastList<MeshInfo> value3 = enumerator3.Current.Value;
				FilterMeshesToCombine(value3, meshCombineList, staticBatching, bAddSkippedRenderers: true);
			}
		}
		CombineMeshList(bucket.m_bucketX, bucket.m_bucketY, bucket.m_bucketZ, meshCombineList, "CombinedDam");
		CombineStaticBatch(staticBatching);
	}

	public static void ProcessBucketsToUpdateAfterInitalise()
	{
		if (s_buckets == null)
		{
			return;
		}
		List<Bucket> list = new List<Bucket>();
		for (int num = s_BucketsToUpdateAfterInitialise.Count - 1; num >= 0; num--)
		{
			Bucket bucketForWorldPosition = GetBucketForWorldPosition(s_BucketsToUpdateAfterInitialise[num], createIfDoesntExist: true);
			if (bucketForWorldPosition != null && !list.Contains(bucketForWorldPosition))
			{
				list.Add(bucketForWorldPosition);
			}
		}
		Gamer[] localGamers = Gamer.GetLocalGamers();
		int[] playerFloorIndexes = new int[localGamers.Length];
		for (int i = 0; i < localGamers.Length; i++)
		{
			if (localGamers[i].m_PlayerObject != null)
			{
				playerFloorIndexes[i] = localGamers[i].m_PlayerObject.GetFloorIndex();
			}
			else
			{
				playerFloorIndexes[i] = 0;
			}
		}
		list.Sort(delegate(Bucket a, Bucket b)
		{
			bool flag = false;
			bool flag2 = false;
			for (int k = 0; k < playerFloorIndexes.Length; k++)
			{
				if (a.m_bucketZ == playerFloorIndexes[k])
				{
					flag = true;
				}
				if (b.m_bucketZ == playerFloorIndexes[k])
				{
					flag2 = true;
				}
			}
			if (flag && flag2)
			{
				return 0;
			}
			return (!flag) ? 1 : (-1);
		});
		int j = 0;
		for (int count = list.Count; j < count; j++)
		{
			RequestBucketDamagableUpdate(list[j]);
		}
		s_BucketsToUpdateAfterInitialise.Clear();
	}

	public static void RequestBucketDamagableUpdate(Vector3 position)
	{
		if (s_buckets == null)
		{
			s_BucketsToUpdateAfterInitialise.Add(position);
			return;
		}
		Bucket bucketForWorldPosition = GetBucketForWorldPosition(position, createIfDoesntExist: true);
		if (bucketForWorldPosition != null)
		{
			RequestBucketDamagableUpdate(bucketForWorldPosition);
		}
	}

	public static void RequestBucketDamagableUpdate(Bucket bucket, bool bHighPriority = true)
	{
		int bucketZ = bucket.m_bucketZ;
		int length = s_buckets.GetLength(2);
		for (int i = bucketZ; i < length; i++)
		{
			BucketQueueEntry bucketQueueEntry = new BucketQueueEntry(bucket);
			bucketQueueEntry.z = i;
			if (bHighPriority)
			{
				m_BucketUpdateQueue.Insert(0, bucketQueueEntry);
			}
			else
			{
				m_BucketUpdateQueue.Add(bucketQueueEntry);
			}
		}
	}

	public static void ProcessBucketUpdateQueue()
	{
		if (m_BucketUpdateQueue.Count <= 0 || !UpdateManager.AquireHeavyCpuLock())
		{
			return;
		}
		BucketQueueEntry bucketQueueEntry = m_BucketUpdateQueue[0];
		m_BucketUpdateQueue.RemoveAt(0);
		if (s_buckets != null && bucketQueueEntry.x >= 0 && bucketQueueEntry.x < s_buckets.GetLength(0) && bucketQueueEntry.y >= 0 && bucketQueueEntry.y < s_buckets.GetLength(1) && bucketQueueEntry.z >= 0 && bucketQueueEntry.z < s_buckets.GetLength(2))
		{
			Bucket bucket = s_buckets[bucketQueueEntry.x, bucketQueueEntry.y, bucketQueueEntry.z];
			if (bucket != null)
			{
				RecombineBucketDamagable(bucket);
				bucket.m_forceUpdate = true;
			}
		}
	}

	public static void AddToBucketAtRuntime(MeshRenderer mesh, bool visibleThroughFacade = true, bool visisbleFromFloorsAbove = true, bool bCheckForMaterialBlock = false, bool bCombined = false, bool bAlsoFloorsAbove = false, int forceBuildingID = -1)
	{
		if (mesh == null)
		{
			return;
		}
		if (s_buckets != null)
		{
			Bucket bucketForWorldPosition = GetBucketForWorldPosition(mesh.transform.position, createIfDoesntExist: true);
			if (bucketForWorldPosition != null)
			{
				mesh.enabled = false;
				int buildingId = 0;
				if (m_FacadesManager != null)
				{
					buildingId = m_FacadesManager.GetBuildingIDAtPos(mesh.transform.position);
				}
				if (forceBuildingID != -1)
				{
					buildingId = forceBuildingID;
				}
				int bucketZ = bucketForWorldPosition.m_bucketZ;
				int num = ((!bAlsoFloorsAbove) ? (bucketZ + 1) : s_buckets.GetLength(2));
				for (int i = bucketZ; i < num; i++)
				{
					Bucket bucket = s_buckets[bucketForWorldPosition.m_bucketX, bucketForWorldPosition.m_bucketY, i];
					if (bucket == null)
					{
						bucket = new Bucket(bucketForWorldPosition.m_bucketX, bucketForWorldPosition.m_bucketY, i);
						s_buckets[bucketForWorldPosition.m_bucketX, bucketForWorldPosition.m_bucketY, i] = bucket;
					}
					if (bucket != null)
					{
						if (bCheckForMaterialBlock)
						{
							bucket.AddRenderableWithCustomMaterialBlock(buildingId, RenderableType.Depth, 0, mesh);
						}
						else
						{
							bucket.AddRenderable(buildingId, RenderableType.Depth, 0, mesh, bCombined);
						}
						bucket.m_forceUpdate = true;
					}
				}
			}
			else
			{
				Debug.LogErrorFormat("[Culling] mesh (runtime): {0} could not be placed into a bucket (out of bounds)", mesh);
				Debug.Log("    ***  Error  *** AddToBucketAtRuntime  ");
			}
		}
		else
		{
			m_RunTimeMeshToBuckets.Add(new RunTimeMeshToBucket(mesh, visibleThroughFacade, visisbleFromFloorsAbove, bCheckForMaterialBlock, bCombined, bAlsoFloorsAbove));
		}
	}

	public static void RemoveFromBucketAtRuntime(MeshRenderer mesh, bool bCheckForMaterialBlock = false)
	{
		if (mesh == null)
		{
			return;
		}
		mesh.enabled = true;
		Bucket bucketForWorldPosition = GetBucketForWorldPosition(mesh.transform.position, createIfDoesntExist: false);
		if (bucketForWorldPosition == null)
		{
			return;
		}
		Bucket bucket = null;
		for (int i = 0; i < s_buckets.GetLength(2); i++)
		{
			bucket = s_buckets[bucketForWorldPosition.m_bucketX, bucketForWorldPosition.m_bucketY, i];
			if (bucket != null)
			{
				if (bCheckForMaterialBlock)
				{
					bucket.RemoveRenderableWithCustomMaterialBlock(mesh);
				}
				else
				{
					bucket.RemoveRenderable(mesh);
				}
			}
		}
	}

	public static void AddAnimWrapperAtRuntime(CullingObjectCollector.AnimatedWrapper animWrapper)
	{
		if (animWrapper != null && s_buckets != null)
		{
			Bucket bucketForWorldPosition = GetBucketForWorldPosition(animWrapper.m_Animators[0].transform.position, createIfDoesntExist: true);
			if (bucketForWorldPosition != null)
			{
				int buildingIDAtPos = m_FacadesManager.GetBuildingIDAtPos(animWrapper.m_Animators[0].transform.position);
				bucketForWorldPosition.AddAnimatedWrapper(buildingIDAtPos, RenderableType.Depth, 0, animWrapper);
				bucketForWorldPosition.m_allAnimated.Add(animWrapper);
				bucketForWorldPosition.m_forceUpdate = true;
			}
		}
	}

	private static MeshRenderer CreateBakedFloorQuad(Transform parent, Material bakedMaterial, Vector2 bakedTextureOffset, Bucket bucket, FloorManager.Floor floorAtZ)
	{
		if (bucket.m_bucketX == 18)
		{
			Debug.LogFormat("CreateFloorQuad bucket {0},{1},{2} floor {3} texOffset {4} parent {5}", bucket.m_bucketX, bucket.m_bucketY, bucket.m_bucketZ, floorAtZ.m_FloorIndex, bakedTextureOffset, parent.position);
		}
		GameObject gameObject = new GameObject("floor_quad");
		gameObject.transform.SetParent(parent);
		gameObject.transform.localScale = new Vector3(s_bucketSizeX, s_bucketSizeY, 1f);
		gameObject.transform.position = new Vector3(0f, 0f, 0f);
		Vector3 worldPos = Vector3.zero;
		if (GetWorldPositionFromBucketCoords(bucket.m_bucketX, bucket.m_bucketY, bucket.m_bucketZ, floorAtZ, out worldPos))
		{
			worldPos += new Vector3((float)s_bucketSizeX * 0.5f, (float)s_bucketSizeY * -0.5f, 0f);
			gameObject.transform.position = worldPos;
			if (bucket.m_bucketX == 18)
			{
				Debug.LogFormat("\tquadPos:{0}", worldPos);
			}
		}
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		if (meshFilter != null)
		{
			Vector2 vector = new Vector2((float)s_bucketSizeX * 32f / (float)bakedMaterial.mainTexture.width, (float)s_bucketSizeY * 32f / (float)bakedMaterial.mainTexture.height);
			float num = (float)(s_bucketSizeX * bucket.m_bucketX) * 32f - bakedTextureOffset.x;
			num /= (float)bakedMaterial.mainTexture.width;
			float num2 = (float)(s_bucketSizeY * bucket.m_bucketY) * 32f - bakedTextureOffset.y;
			num2 /= (float)bakedMaterial.mainTexture.height;
			Vector2 vector2 = new Vector2(num, 1f - num2 - vector.y);
			Mesh mesh = UnityEngine.Object.Instantiate(s_floorQuadMesh);
			Vector2[] uv = mesh.uv;
			uv[0].Set(vector2.x, vector2.y);
			uv[1].Set(vector2.x + vector.x, vector2.y + vector.y);
			uv[2].Set(vector2.x + vector.x, vector2.y);
			uv[3].Set(vector2.x, vector2.y + vector.y);
			mesh.uv = uv;
			meshFilter.sharedMesh = mesh;
			if (bucket.m_bucketX == 18)
			{
				Debug.LogFormat("\toffset:{0} size:{1} texW:{2} texH:{3}", vector2, vector, bakedMaterial.mainTexture.width, bakedMaterial.mainTexture.height);
			}
		}
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		if (meshRenderer != null)
		{
			meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
			meshRenderer.sharedMaterial = bakedMaterial;
			meshRenderer.enabled = false;
		}
		if (meshFilter != null && meshRenderer != null)
		{
			return meshRenderer;
		}
		return null;
	}

	public static void GenerateDebugBucketObjects(FastList<Vector3> positions)
	{
	}
}
