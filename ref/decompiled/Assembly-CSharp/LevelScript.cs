using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelScript : MonoBehaviour
{
	[Serializable]
	public enum PRISON_ENUM
	{
		CustomPrison = -1,
		Unassigned = 0,
		Centre_Perks = 1,
		OldWestFort = 2,
		POW_Camp = 3,
		Space_Prison = 4,
		Gulag_Prison = 5,
		Oil_Rig = 6,
		Transport_Train = 7,
		Transport_Boat = 8,
		Transport_Plane = 9,
		Area_17 = 10,
		Dictator = 11,
		GDC_Centre_Perks = 12,
		Tutorial = 13,
		DLC02 = 14,
		DLC03 = 15,
		DLC04 = 16,
		DLC05 = 17,
		DLC06 = 18,
		JamesTest = 99,
		AITest = 100,
		ImportedPrison = 1000
	}

	[Serializable]
	public enum LEADERBOARD_PRISON_ENUM
	{
		Centre_Perks = 1,
		OldWestFort,
		POW_Camp,
		Space_Prison,
		Gulag_Prison,
		Oil_Rig,
		Transport_Train,
		Transport_Boat,
		Transport_Plane,
		Area_17,
		Dictator,
		DLC02,
		DLC03,
		DLC04,
		DLC05,
		DLC06,
		kMaxLeaderboardPrisons
	}

	[Serializable]
	public enum PRISON_ENUM_MASK
	{
		Centre_Perks = 1,
		OldWestFort = 2,
		POW_Camp = 4,
		Space_Prison = 8,
		Gulag_Prison = 0x10,
		Oil_Rig = 0x20,
		Transport_Train = 0x40,
		Transport_Boat = 0x80,
		Transport_Plane = 0x100,
		Area_17 = 0x200,
		Dictator = 0x400,
		PlayerMade = 0x800,
		Tutorial = 0x1000,
		DLC02 = 0x2000,
		DLC03 = 0x4000,
		DLC04 = 0x8000,
		DLC05 = 0x10000,
		DLC06 = 0x20000
	}

	[Serializable]
	public enum PRISON_TYPE
	{
		Normal,
		Transport,
		Tutorial
	}

	[Serializable]
	public class SubLevel
	{
		public Transform m_Root;

		public string m_SceneName;
	}

	[Serializable]
	public class LS_Bucket_YX
	{
		public LS_Bucket_X[] m_Buckets;
	}

	[Serializable]
	public class LS_Bucket_X
	{
		public LS_Bucket[] m_Buckets;
	}

	[Serializable]
	public class LS_Bucket
	{
		public bool m_bValid;

		public MeshRenderer[] m_BakedFloorQuads;

		public int[] m_RenderableKeys;

		public LS_Renderables[] m_RenderableVal;
	}

	[Serializable]
	public class ListOfMeshInfos
	{
		public CullingBuckets.MeshInfo[] m_Meshes;
	}

	[Serializable]
	public class LS_DictIntToListOfMeshInfos
	{
		public int[] m_Keys;

		public ListOfMeshInfos[] m_Vals;

		public void Set(Dictionary<int, FastList<CullingBuckets.MeshInfo>> org)
		{
			int count = org.Keys.Count;
			m_Keys = new int[count];
			m_Vals = new ListOfMeshInfos[count];
			int num = 0;
			foreach (int key in org.Keys)
			{
				m_Keys[num] = key;
				m_Vals[num] = new ListOfMeshInfos();
				m_Vals[num].m_Meshes = new CullingBuckets.MeshInfo[org[key].Count];
				for (int i = 0; i < org[key].Count; i++)
				{
					m_Vals[num].m_Meshes[i] = org[key][i];
				}
				num++;
			}
		}

		public void Get(ref Dictionary<int, FastList<CullingBuckets.MeshInfo>> org)
		{
			int num = m_Keys.Length;
			for (int i = 0; i < num; i++)
			{
				org[m_Keys[i]] = new FastList<CullingBuckets.MeshInfo>();
				int num2 = m_Vals[i].m_Meshes.Length;
				for (int j = 0; j < num2; j++)
				{
					org[m_Keys[i]].Add(m_Vals[i].m_Meshes[j]);
				}
			}
		}
	}

	[Serializable]
	public class ListOfAnimatedWrappers
	{
		public CullingObjectCollector.AnimatedWrapper[] m_AnimWraps;
	}

	[Serializable]
	public class LS_DictIntToListOfAnimatedWrappers
	{
		public int[] m_Keys;

		public ListOfAnimatedWrappers[] m_Vals;

		public void Set(Dictionary<int, FastList<CullingObjectCollector.AnimatedWrapper>> org)
		{
			int count = org.Keys.Count;
			m_Keys = new int[count];
			m_Vals = new ListOfAnimatedWrappers[count];
			int num = 0;
			foreach (int key in org.Keys)
			{
				m_Keys[num] = key;
				m_Vals[num] = new ListOfAnimatedWrappers();
				m_Vals[num].m_AnimWraps = new CullingObjectCollector.AnimatedWrapper[org[key].Count];
				for (int i = 0; i < org[key].Count; i++)
				{
					m_Vals[num].m_AnimWraps[i] = org[key][i];
				}
				num++;
			}
		}

		public void Get(ref Dictionary<int, FastList<CullingObjectCollector.AnimatedWrapper>> org)
		{
			int num = m_Keys.Length;
			for (int i = 0; i < num; i++)
			{
				org[m_Keys[i]] = new FastList<CullingObjectCollector.AnimatedWrapper>();
				int num2 = m_Vals[i].m_AnimWraps.Length;
				for (int j = 0; j < num2; j++)
				{
					org[m_Keys[i]].Add(m_Vals[i].m_AnimWraps[j]);
				}
			}
		}
	}

	[Serializable]
	public class LRWData
	{
		public int m_BucketCount;

		public int m_ID;

		public Vector3[] m_Buckets;

		public LRWData(int bc, int id)
		{
			m_BucketCount = bc;
			m_Buckets = new Vector3[bc];
			m_ID = id;
		}
	}

	[Serializable]
	public class ListOfLargeRendererWrappers
	{
		public int[] m_LRWrapsIDs;
	}

	[Serializable]
	public class LS_DictIntToListOfLargeRendererWrappers
	{
		public int[] m_Keys;

		public ListOfLargeRendererWrappers[] m_Vals;

		public void Set(Dictionary<int, FastList<CullingObjectCollector.LargeRendererWrapper>> org)
		{
			int count = org.Keys.Count;
			m_Keys = new int[count];
			m_Vals = new ListOfLargeRendererWrappers[count];
			int num = 0;
			foreach (int key in org.Keys)
			{
				m_Keys[num] = key;
				m_Vals[num] = new ListOfLargeRendererWrappers();
				m_Vals[num].m_LRWrapsIDs = new int[org[key].Count];
				for (int i = 0; i < org[key].Count; i++)
				{
					m_Vals[num].m_LRWrapsIDs[i] = org[key][i].m_ID;
				}
				num++;
			}
		}

		public void Get(ref Dictionary<int, FastList<CullingObjectCollector.LargeRendererWrapper>> org)
		{
			if (m_Keys == null || m_Vals == null)
			{
				return;
			}
			int num = m_Keys.Length;
			for (int i = 0; i < num; i++)
			{
				org[m_Keys[i]] = new FastList<CullingObjectCollector.LargeRendererWrapper>();
				if (m_Vals[i] != null && m_Vals[i].m_LRWrapsIDs != null)
				{
					int num2 = m_Vals[i].m_LRWrapsIDs.Length;
					for (int j = 0; j < num2; j++)
					{
						CullingObjectCollector.LargeRendererWrapper item = CullingObjectCollector.GetInstance().FindLRW(m_Vals[i].m_LRWrapsIDs[j]);
						org[m_Keys[i]].Add(item);
					}
				}
			}
		}
	}

	[Serializable]
	public class ListOfParticleWrappers
	{
		public CullingObjectCollector.ParticleWrapper[] m_ParticleWraps;
	}

	[Serializable]
	public class LS_Renderables
	{
		public int m_BuildingID;

		public LS_DictIntToListOfMeshInfos m_MeshIndi;

		public LS_DictIntToListOfMeshInfos m_MeshDamagableCombined;

		public LS_DictIntToListOfMeshInfos m_MeshDamagableOrig;

		public LS_DictIntToListOfMeshInfos m_FacadeMeshes;

		public LS_DictIntToListOfAnimatedWrappers m_Animated;

		public LS_DictIntToListOfAnimatedWrappers m_FacadeAnimated;

		public ListOfParticleWrappers m_Particles;

		public ListOfParticleWrappers m_FacadeParticles;

		public LS_DictIntToListOfMeshInfos m_RenderersWithCMB;

		public LS_DictIntToListOfLargeRendererWrappers m_LargeRenderer;
	}

	[Serializable]
	public class ListOfItemDatas
	{
		public ItemData[] m_Datas;
	}

	public const int kPRISONCOUNT = 11;

	public LRWData[] m_LargeRendererWarpData;

	public PrisonData m_LevelSetup;

	public string m_LocalizationKeyOverride = string.Empty;

	public bool m_Processed;

	public bool m_PreBuildBehaviourLists;

	public bool m_PreBuildBuckets;

	public bool m_PreBuildHUDElements;

	public bool m_PreBuildItemCreatePool;

	public bool m_PreBuildSwapPrefabRefs;

	public int m_TileSystemBoundsX;

	public int m_TileSystemBoundsY;

	public LS_Bucket_YX[] m_SerialBuckets;

	public Renderer[] m_backGround;

	public Character[] m_CharacterWrappers;

	public GameObject[] m_DeskWrappers;

	public GameObject[] m_LargeRendererWrappers;

	public Light[] m_NormalLightWrappers;

	public CustomLight[] m_CustomLightWrappers;

	public ListOfItemDatas m_AllowedItems;

	public ListOfItemDatas m_KeyItems;

	[HideInInspector]
	public T17NetworkBehaviour[] m_NetworkBehaviourClasses;

	[HideInInspector]
	public T17MonoBehaviour[] m_MonoBehaviourClasses;

	public ItemContainer m_LevelItemContainer;

	public SubLevel[] m_SubLevels;

	public List<WorldCanvasTrackedUIElements> m_WorldSpaceCanvases = new List<WorldCanvasTrackedUIElements>();

	[SerializeField]
	protected bool m_hasWalkableUnderground;

	protected static LevelScript m_Instance;

	private static int m_LevelItemContainerViewID = -1;

	private bool m_bAreWeTheOriginalHost;

	public bool hasWalkableUnderground => m_hasWalkableUnderground;

	public static int LevelItemContainerViewID
	{
		get
		{
			if (m_LevelItemContainerViewID == -1 && GetInstance().m_LevelItemContainer != null)
			{
				m_LevelItemContainerViewID = GetInstance().m_LevelItemContainer.NetView.viewID;
			}
			return m_LevelItemContainerViewID;
		}
	}

	public static PRISON_ENUM MapLeaderboardToPrisonEnum(LEADERBOARD_PRISON_ENUM leaderboardID)
	{
		return leaderboardID switch
		{
			LEADERBOARD_PRISON_ENUM.Centre_Perks => PRISON_ENUM.Centre_Perks, 
			LEADERBOARD_PRISON_ENUM.OldWestFort => PRISON_ENUM.OldWestFort, 
			LEADERBOARD_PRISON_ENUM.POW_Camp => PRISON_ENUM.POW_Camp, 
			LEADERBOARD_PRISON_ENUM.Space_Prison => PRISON_ENUM.Space_Prison, 
			LEADERBOARD_PRISON_ENUM.Gulag_Prison => PRISON_ENUM.Gulag_Prison, 
			LEADERBOARD_PRISON_ENUM.Oil_Rig => PRISON_ENUM.Oil_Rig, 
			LEADERBOARD_PRISON_ENUM.Transport_Train => PRISON_ENUM.Transport_Train, 
			LEADERBOARD_PRISON_ENUM.Transport_Boat => PRISON_ENUM.Transport_Boat, 
			LEADERBOARD_PRISON_ENUM.Transport_Plane => PRISON_ENUM.Transport_Plane, 
			LEADERBOARD_PRISON_ENUM.Area_17 => PRISON_ENUM.Area_17, 
			LEADERBOARD_PRISON_ENUM.Dictator => PRISON_ENUM.Dictator, 
			LEADERBOARD_PRISON_ENUM.DLC02 => PRISON_ENUM.DLC02, 
			LEADERBOARD_PRISON_ENUM.DLC03 => PRISON_ENUM.DLC03, 
			LEADERBOARD_PRISON_ENUM.DLC04 => PRISON_ENUM.DLC04, 
			LEADERBOARD_PRISON_ENUM.DLC05 => PRISON_ENUM.DLC05, 
			LEADERBOARD_PRISON_ENUM.DLC06 => PRISON_ENUM.DLC06, 
			_ => PRISON_ENUM.Unassigned, 
		};
	}

	public static bool IsPrisonEnumInMask(PRISON_ENUM_MASK mask, PRISON_ENUM toCheck)
	{
		switch (mask)
		{
		case (PRISON_ENUM_MASK)(-1):
			return true;
		case (PRISON_ENUM_MASK)0:
			return false;
		default:
		{
			int num = 1;
			switch (toCheck)
			{
			case PRISON_ENUM.CustomPrison:
				num = 2048;
				break;
			case PRISON_ENUM.Unassigned:
			case PRISON_ENUM.GDC_Centre_Perks:
			case PRISON_ENUM.JamesTest:
			case PRISON_ENUM.AITest:
				return true;
			default:
				num = 1 << (int)(toCheck - 1);
				break;
			}
			return ((uint)mask & (uint)num) != 0;
		}
		}
	}

	public static int GetPrisonBit(PRISON_ENUM prison)
	{
		int num = -1;
		switch (prison)
		{
		case PRISON_ENUM.CustomPrison:
			return 2048;
		case PRISON_ENUM.Unassigned:
		case PRISON_ENUM.GDC_Centre_Perks:
		case PRISON_ENUM.JamesTest:
		case PRISON_ENUM.AITest:
			return -1;
		default:
			return 1 << (int)(prison - 1);
		}
	}

	private void Awake()
	{
		m_Instance = this;
		m_bAreWeTheOriginalHost = T17NetManager.IsMasterClient;
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	private void Start()
	{
	}

	public static LevelScript GetInstance()
	{
		return m_Instance;
	}

	public virtual void PreInit()
	{
		if (m_LevelItemContainer == null)
		{
			m_LevelItemContainer = GetComponent<ItemContainer>();
			m_LevelItemContainer.m_MaxSize = 100;
			ItemContainer levelItemContainer = m_LevelItemContainer;
			levelItemContainer.OnItemAddedEvent = (ItemContainer.ItemContainerAddedHandler)Delegate.Remove(levelItemContainer.OnItemAddedEvent, new ItemContainer.ItemContainerAddedHandler(m_LevelItemContainer.ItemAddedToFloor));
			ItemContainer levelItemContainer2 = m_LevelItemContainer;
			levelItemContainer2.OnItemAddedEvent = (ItemContainer.ItemContainerAddedHandler)Delegate.Combine(levelItemContainer2.OnItemAddedEvent, new ItemContainer.ItemContainerAddedHandler(m_LevelItemContainer.ItemAddedToFloor));
			ItemContainer levelItemContainer3 = m_LevelItemContainer;
			levelItemContainer3.OnItemRemovedEvent = (ItemContainer.ItemContainerEvent)Delegate.Remove(levelItemContainer3.OnItemRemovedEvent, new ItemContainer.ItemContainerEvent(m_LevelItemContainer.ItemRemovedFromFloor));
			ItemContainer levelItemContainer4 = m_LevelItemContainer;
			levelItemContainer4.OnItemRemovedEvent = (ItemContainer.ItemContainerEvent)Delegate.Combine(levelItemContainer4.OnItemRemovedEvent, new ItemContainer.ItemContainerEvent(m_LevelItemContainer.ItemRemovedFromFloor));
		}
	}

	public virtual void FinalInit()
	{
		FloorManager.GetInstance().Init();
	}

	public static PrisonData.LevelInfo GetCurrentLevelInfo()
	{
		LevelScript instance = GetInstance();
		if (instance != null && instance.m_LevelSetup != null)
		{
			return instance.m_LevelSetup.m_LevelInfo;
		}
		return null;
	}

	public static PRISON_ENUM[] GetPlayableLevels()
	{
		int num = Enum.GetValues(typeof(PRISON_ENUM)).Length - 2;
		PRISON_ENUM[] array = new PRISON_ENUM[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = (PRISON_ENUM)(i + 1);
		}
		return array;
	}

	public void DropItemInLevel_AllRPC(Item theItem)
	{
		m_LevelItemContainer.CyclicalAdd_AllRPC(theItem, intoHidden: false);
	}

	public void SetPreBuiltBehaviourLists(List<T17NetworkBehaviour> nets, List<T17MonoBehaviour> monos)
	{
		m_NetworkBehaviourClasses = new T17NetworkBehaviour[nets.Count];
		for (int i = 0; i < m_NetworkBehaviourClasses.Length; i++)
		{
			m_NetworkBehaviourClasses[i] = nets[i];
		}
		m_MonoBehaviourClasses = new T17MonoBehaviour[monos.Count];
		for (int i = 0; i < m_MonoBehaviourClasses.Length; i++)
		{
			m_MonoBehaviourClasses[i] = monos[i];
		}
	}

	public void SetBucketArray(int xSize, int ySize, int zSize)
	{
		m_SerialBuckets = new LS_Bucket_YX[zSize];
		for (int i = 0; i < zSize; i++)
		{
			m_SerialBuckets[i] = new LS_Bucket_YX();
			m_SerialBuckets[i].m_Buckets = new LS_Bucket_X[ySize];
			for (int j = 0; j < ySize; j++)
			{
				m_SerialBuckets[i].m_Buckets[j] = new LS_Bucket_X();
				m_SerialBuckets[i].m_Buckets[j].m_Buckets = new LS_Bucket[xSize];
			}
		}
	}

	public void SetBucketEmpty(int x, int y, int z)
	{
		m_SerialBuckets[z].m_Buckets[y].m_Buckets[x] = null;
	}

	public void SetBucket(int x, int y, int z, int data)
	{
		m_SerialBuckets[z].m_Buckets[y].m_Buckets[x] = new LS_Bucket();
		m_SerialBuckets[z].m_Buckets[y].m_Buckets[x].m_bValid = true;
	}

	public void SetTileSystemBounds(int xbounds, int yBounds)
	{
		m_TileSystemBoundsX = xbounds;
		m_TileSystemBoundsY = yBounds;
	}

	public void SetBucket_FloorQuads(int x, int y, int z, MeshRenderer[] bakedFloorQuads)
	{
		m_SerialBuckets[z].m_Buckets[y].m_Buckets[x].m_BakedFloorQuads = bakedFloorQuads;
	}

	public void SetBucket_Renderables(int x, int y, int z, Dictionary<int, CullingBuckets.BuildingRenderables> renderables)
	{
		int count = renderables.Keys.Count;
		m_SerialBuckets[z].m_Buckets[y].m_Buckets[x].m_RenderableKeys = new int[count];
		m_SerialBuckets[z].m_Buckets[y].m_Buckets[x].m_RenderableVal = new LS_Renderables[count];
		int num = 0;
		foreach (int key in renderables.Keys)
		{
			m_SerialBuckets[z].m_Buckets[y].m_Buckets[x].m_RenderableKeys[num] = key;
			LS_Renderables lS_Renderables = new LS_Renderables();
			CullingBuckets.BuildingRenderables buildingRenderables = renderables[key];
			lS_Renderables.m_BuildingID = buildingRenderables.m_buildingId;
			if (buildingRenderables.m_meshesIndi.Count > 0)
			{
				lS_Renderables.m_MeshIndi = new LS_DictIntToListOfMeshInfos();
				lS_Renderables.m_MeshIndi.Set(buildingRenderables.m_meshesIndi);
			}
			if (buildingRenderables.m_meshesDamagableCombined.Count > 0)
			{
				lS_Renderables.m_MeshDamagableCombined = new LS_DictIntToListOfMeshInfos();
				lS_Renderables.m_MeshDamagableCombined.Set(buildingRenderables.m_meshesDamagableCombined);
			}
			if (buildingRenderables.m_meshesDamagableOrig.Count > 0)
			{
				lS_Renderables.m_MeshDamagableOrig = new LS_DictIntToListOfMeshInfos();
				lS_Renderables.m_MeshDamagableOrig.Set(buildingRenderables.m_meshesDamagableOrig);
			}
			if (buildingRenderables.m_facadeMeshes.Count > 0)
			{
				lS_Renderables.m_FacadeMeshes = new LS_DictIntToListOfMeshInfos();
				lS_Renderables.m_FacadeMeshes.Set(buildingRenderables.m_facadeMeshes);
			}
			if (buildingRenderables.m_animated.Count > 0)
			{
				lS_Renderables.m_Animated = new LS_DictIntToListOfAnimatedWrappers();
				lS_Renderables.m_Animated.Set(buildingRenderables.m_animated);
			}
			if (buildingRenderables.m_facadeAnimated.Count > 0)
			{
				lS_Renderables.m_FacadeAnimated = new LS_DictIntToListOfAnimatedWrappers();
				lS_Renderables.m_FacadeAnimated.Set(buildingRenderables.m_facadeAnimated);
			}
			if (buildingRenderables.m_LargeRenderer.Count > 0)
			{
				lS_Renderables.m_LargeRenderer = new LS_DictIntToListOfLargeRendererWrappers();
				lS_Renderables.m_LargeRenderer.Set(buildingRenderables.m_LargeRenderer);
			}
			if (buildingRenderables.m_particles.Count > 0)
			{
				lS_Renderables.m_Particles = new ListOfParticleWrappers();
				lS_Renderables.m_Particles.m_ParticleWraps = new CullingObjectCollector.ParticleWrapper[buildingRenderables.m_particles.Count];
				for (int i = 0; i < buildingRenderables.m_particles.Count; i++)
				{
					lS_Renderables.m_Particles.m_ParticleWraps[i] = buildingRenderables.m_particles[i];
				}
			}
			if (buildingRenderables.m_facadeParticles.Count > 0)
			{
				lS_Renderables.m_FacadeParticles = new ListOfParticleWrappers();
				lS_Renderables.m_FacadeParticles.m_ParticleWraps = new CullingObjectCollector.ParticleWrapper[buildingRenderables.m_facadeParticles.Count];
				for (int i = 0; i < buildingRenderables.m_facadeParticles.Count; i++)
				{
					lS_Renderables.m_FacadeParticles.m_ParticleWraps[i] = buildingRenderables.m_facadeParticles[i];
				}
			}
			if (buildingRenderables.m_RenderersWithCustomMaterialBlocks.Count > 0)
			{
				lS_Renderables.m_RenderersWithCMB = new LS_DictIntToListOfMeshInfos();
				lS_Renderables.m_RenderersWithCMB.Set(buildingRenderables.m_RenderersWithCustomMaterialBlocks);
			}
			m_SerialBuckets[z].m_Buckets[y].m_Buckets[x].m_RenderableVal[num] = lS_Renderables;
			num++;
		}
	}

	public void SetBackground(List<Renderer> backGroundList)
	{
		if (backGroundList != null)
		{
			int count = backGroundList.Count;
			if (count > 0)
			{
				m_backGround = new Renderer[count];
				for (int i = 0; i < count; i++)
				{
					m_backGround[i] = backGroundList[i];
				}
			}
			else
			{
				m_backGround = null;
			}
		}
		else
		{
			m_backGround = null;
		}
	}

	public void GetBackground(out List<Renderer> backGroundList)
	{
		if (m_backGround == null)
		{
			backGroundList = null;
			return;
		}
		backGroundList = new List<Renderer>();
		for (int i = 0; i < m_backGround.Length; i++)
		{
			backGroundList.Add(m_backGround[i]);
		}
	}

	public void SetCharacterWrappers(FastList<CullingObjectCollector.CharacterWrapper> characterWrappers)
	{
		m_CharacterWrappers = new Character[characterWrappers.Count];
		for (int i = 0; i < m_CharacterWrappers.Length; i++)
		{
			m_CharacterWrappers[i] = characterWrappers[i].m_Character;
		}
	}

	public void SetDeskWrappers(FastList<CullingObjectCollector.DeskWrapper> deskWrappers)
	{
		m_DeskWrappers = new GameObject[deskWrappers.Count];
		for (int i = 0; i < m_DeskWrappers.Length; i++)
		{
			m_DeskWrappers[i] = deskWrappers[i].m_GameObject;
		}
	}

	public void SetLargeRendererWrappers(FastList<CullingObjectCollector.LargeRendererWrapper> largeRendererWrapper)
	{
		m_LargeRendererWrappers = new GameObject[largeRendererWrapper.Count];
		m_LargeRendererWarpData = new LRWData[largeRendererWrapper.Count];
		for (int i = 0; i < m_LargeRendererWrappers.Length; i++)
		{
			m_LargeRendererWrappers[i] = largeRendererWrapper[i].m_GameObject;
			m_LargeRendererWarpData[i] = new LRWData(largeRendererWrapper[i].m_SpanningBuckets.Count, largeRendererWrapper[i].m_ID);
			for (int j = 0; j < largeRendererWrapper[i].m_SpanningBuckets.Count; j++)
			{
				ref Vector3 reference = ref m_LargeRendererWarpData[i].m_Buckets[j];
				reference = new Vector3(largeRendererWrapper[i].m_SpanningBuckets[j].m_Bucket.m_bucketX, largeRendererWrapper[i].m_SpanningBuckets[j].m_Bucket.m_bucketY, largeRendererWrapper[i].m_SpanningBuckets[j].m_Bucket.m_bucketZ);
			}
		}
	}

	public void SetLightWrappers(FastList<CullingObjectCollector.LightWrapper> lightWrappers)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < lightWrappers.Count; i++)
		{
			if (lightWrappers[i].m_Light != null)
			{
				num++;
			}
			else
			{
				num2++;
			}
		}
		m_NormalLightWrappers = new Light[num];
		m_CustomLightWrappers = new CustomLight[num2];
		num = 0;
		num2 = 0;
		for (int i = 0; i < lightWrappers.Count; i++)
		{
			if (lightWrappers[i].m_Light != null)
			{
				m_NormalLightWrappers[num++] = lightWrappers[i].m_Light;
			}
			else
			{
				m_CustomLightWrappers[num2++] = lightWrappers[i].m_CustomLight;
			}
		}
	}

	public void GetBucketArraySize(ref int xSize, ref int ySize, ref int zSize)
	{
		xSize = m_SerialBuckets[0].m_Buckets[0].m_Buckets.GetLength(0);
		ySize = m_SerialBuckets[0].m_Buckets.GetLength(0);
		zSize = m_SerialBuckets.GetLength(0);
	}

	public void GetTileSystemBounds(ref int xbounds, ref int yBounds)
	{
		xbounds = m_TileSystemBoundsX;
		yBounds = m_TileSystemBoundsY;
	}

	public void GetBucket(int x, int y, int z, out LS_Bucket lsBucket)
	{
		lsBucket = m_SerialBuckets[z].m_Buckets[y].m_Buckets[x];
	}

	public void GetBucket_FloorQuads(int x, int y, int z, out MeshRenderer[] bakedFloorQuads)
	{
		bakedFloorQuads = m_SerialBuckets[z].m_Buckets[y].m_Buckets[x].m_BakedFloorQuads;
	}

	public void GetBucket_Renderables(int x, int y, int z, ref Dictionary<int, CullingBuckets.BuildingRenderables> renderables)
	{
		int num = m_SerialBuckets[z].m_Buckets[y].m_Buckets[x].m_RenderableKeys.Length;
		for (int i = 0; i < num; i++)
		{
			CullingBuckets.BuildingRenderables buildingRenderables = new CullingBuckets.BuildingRenderables();
			LS_Renderables lS_Renderables = m_SerialBuckets[z].m_Buckets[y].m_Buckets[x].m_RenderableVal[i];
			buildingRenderables.m_buildingId = lS_Renderables.m_BuildingID;
			if (lS_Renderables.m_MeshIndi != null)
			{
				lS_Renderables.m_MeshIndi.Get(ref buildingRenderables.m_meshesIndi);
			}
			if (lS_Renderables.m_MeshDamagableCombined != null)
			{
				lS_Renderables.m_MeshDamagableCombined.Get(ref buildingRenderables.m_meshesDamagableCombined);
			}
			if (lS_Renderables.m_MeshDamagableOrig != null)
			{
				lS_Renderables.m_MeshDamagableOrig.Get(ref buildingRenderables.m_meshesDamagableOrig);
			}
			if (lS_Renderables.m_FacadeMeshes != null)
			{
				lS_Renderables.m_FacadeMeshes.Get(ref buildingRenderables.m_facadeMeshes);
			}
			if (lS_Renderables.m_Animated != null)
			{
				lS_Renderables.m_Animated.Get(ref buildingRenderables.m_animated);
			}
			if (lS_Renderables.m_LargeRenderer != null)
			{
				lS_Renderables.m_LargeRenderer.Get(ref buildingRenderables.m_LargeRenderer);
			}
			if (lS_Renderables.m_FacadeAnimated != null)
			{
				lS_Renderables.m_FacadeAnimated.Get(ref buildingRenderables.m_facadeAnimated);
			}
			if (lS_Renderables.m_Particles != null)
			{
				for (int j = 0; j < lS_Renderables.m_Particles.m_ParticleWraps.Length; j++)
				{
					buildingRenderables.m_particles.Add(lS_Renderables.m_Particles.m_ParticleWraps[j]);
				}
			}
			if (lS_Renderables.m_FacadeParticles != null)
			{
				for (int j = 0; j < lS_Renderables.m_FacadeParticles.m_ParticleWraps.Length; j++)
				{
					buildingRenderables.m_facadeParticles.Add(lS_Renderables.m_FacadeParticles.m_ParticleWraps[j]);
				}
			}
			if (lS_Renderables.m_RenderersWithCMB != null)
			{
				lS_Renderables.m_RenderersWithCMB.Get(ref buildingRenderables.m_RenderersWithCustomMaterialBlocks);
			}
			renderables[m_SerialBuckets[z].m_Buckets[y].m_Buckets[x].m_RenderableKeys[i]] = buildingRenderables;
		}
	}

	public void GetCharacterWrappers(out Character[] characterWrappers)
	{
		characterWrappers = m_CharacterWrappers;
	}

	public void GetDeskWrappers(out GameObject[] deskWrappers)
	{
		deskWrappers = m_DeskWrappers;
	}

	public void GetLightWrappers(out Light[] normalLightWrappers, out CustomLight[] customLightWrappers)
	{
		normalLightWrappers = m_NormalLightWrappers;
		customLightWrappers = m_CustomLightWrappers;
	}

	public void GetLargeRendererWrappers(out GameObject[] largeRendererWrappers, out LRWData[] data)
	{
		largeRendererWrappers = m_LargeRendererWrappers;
		data = m_LargeRendererWarpData;
	}

	public void SetAllowedItems(List<ItemData> allowedItems)
	{
		int count = allowedItems.Count;
		m_AllowedItems.m_Datas = new ItemData[count];
		for (int i = 0; i < count; i++)
		{
			m_AllowedItems.m_Datas[i] = allowedItems[i];
		}
	}

	public void SetKeyItems(List<ItemData> keyItems)
	{
		int count = keyItems.Count;
		m_KeyItems.m_Datas = new ItemData[count];
		for (int i = 0; i < count; i++)
		{
			m_KeyItems.m_Datas[i] = keyItems[i];
		}
	}

	public static bool AreWeOriginalHost()
	{
		if ((bool)m_Instance)
		{
			return m_Instance.m_bAreWeTheOriginalHost;
		}
		return false;
	}
}
