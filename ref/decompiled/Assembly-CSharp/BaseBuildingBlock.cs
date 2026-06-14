using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[ExecuteInEditMode]
public abstract class BaseBuildingBlock : MonoBehaviour
{
	public enum BuildingBlockType
	{
		UNKNOWN,
		Tile,
		Wall,
		Decoration,
		Object,
		Complex,
		Room,
		TOTAL
	}

	public enum BuildingBlockDrawingMode
	{
		INVALID,
		Stamp,
		Paint,
		Marquee,
		MarqueeLine,
		TOTAL
	}

	public enum CompletionState
	{
		Complete,
		Nearly_Complete,
		Unfinished,
		TOTAL
	}

	[Flags]
	public enum BlockSet
	{
		CentrePerks = 1,
		CougarCreek = 2,
		RattleSnake = 4,
		POW = 8,
		HMSOrca = 0x10,
		HMPOffshore = 0x20,
		FortTundra = 0x40,
		Area17 = 0x80,
		AirForceCon = 0x100,
		USSAnomaly = 0x200,
		GloriousRegime = 0x400,
		WickedWard = 0x800,
		SantasShakedown = 0x1000,
		TOTAL = 0xD,
		ALL = 0x1FFF,
		NONE = 0
	}

	[Flags]
	public enum PurposeGroups
	{
		General = 1,
		RollCall = 2,
		InmateCell = 4,
		SocialArea = 8,
		Library = 0x10,
		Gym = 0x20,
		Showers = 0x40,
		Kitchen = 0x80,
		MealHall = 0x100,
		GuardQuarters = 0x200,
		GuardRoom = 0x400,
		ControlRoom = 0x800,
		ContrabandRoom = 0x1000,
		WardensOffice = 0x2000,
		Kennels = 0x4000,
		Solitary = 0x8000,
		Infirmary = 0x10000,
		Maintenance = 0x20000,
		JobOffice = 0x40000,
		Job_Woodwork = 0x80000,
		Job_Blacksmith = 0x100000,
		Escape = 0x200000,
		TOTAL = 0x16,
		ALL = 0x3FFFFF,
		NONE = 0
	}

	[Flags]
	public enum GroupFlags
	{
		EMPTY = 0,
		Group_1 = 1,
		Group_2 = 2,
		Group_3 = 4,
		Group_4 = 8,
		Group_5 = 0x10,
		Group_6 = 0x20,
		Group_7 = 0x40,
		Group_8 = 0x80,
		Group_9 = 0x100,
		Group_10 = 0x200,
		Group_11 = 0x400,
		Group_12 = 0x800,
		Group_13 = 0x1000,
		Group_14 = 0x2000,
		Group_15 = 0x20000,
		Group_16 = 0x8000,
		Group_17 = 0x10000,
		Group_18 = 0x20000,
		Group_19 = 0x40000,
		Group_20 = 0x80000,
		Group_21 = 0x100000,
		Group_22 = 0x200000,
		Group_23 = 0x400000,
		Group_24 = 0x800000,
		Group_25 = 0x1000000,
		Group_26 = 0x2000000,
		Group_27 = 0x4000000,
		Group_28 = 0x8000000,
		Group_29 = 0x10000000,
		Group_30 = 0x20000000,
		Group_31 = 0x40000000,
		Group_32 = int.MinValue,
		InverseGroup1 = 0xFE,
		InverseGroup2 = 0xFD,
		InverseGroup3 = 0xFB,
		InverseGroup4 = 0xF7,
		InverseGroup5 = 0xEF,
		InverseGroup6 = 0xDF,
		InverseGroup7 = 0xBF,
		InverseGroup8 = 0x7F,
		InverseGroup9 = 0xFF,
		InverseGroup10 = 0xFF,
		InverseGroup11 = 0xFF,
		InverseGroup12 = 0xFF,
		InverseGroup13 = 0xFF,
		InverseGroup14 = 0xFF,
		InverseGroup15 = 0xFF,
		InverseGroup16 = 0xFF,
		InverseGroup17 = 0xFF,
		InverseGroup18 = 0xFF,
		InverseGroup19 = 0xFF,
		InverseGroup20 = 0xFF,
		InverseGroup21 = 0xFF,
		InverseGroup22 = 0xFF,
		InverseGroup23 = 0xFF,
		InverseGroup24 = 0xFF,
		InverseGroup25 = 0xFF,
		InverseGroup26 = 0xFF,
		InverseGroup27 = 0xFF,
		InverseGroup28 = 0xFF,
		InverseGroup29 = 0xFF,
		InverseGroup30 = 0xFF,
		InverseGroup31 = 0xFF,
		InverseGroup32 = 0xFF
	}

	public const int INVALID_BLOCK_ID = -1;

	public const int INVALID_ORDER = 9999999;

	public const int BLOCK_UNCHANGED = -2;

	public const int ANY_BLOCK = -3;

	public const int ANY_TILE = -4;

	public const int ANY_WALL = -5;

	public const int ANY_OBJECT = -6;

	public const int ANY_DECORATION = -7;

	public const int NOT_A_WALL = -8;

	public const int ANY_GROUP_1 = -9;

	public const int ANY_GROUP_2 = -10;

	public const int ANY_GROUP_3 = -11;

	public const int ANY_GROUP_4 = -12;

	public const int ANY_GROUP_5 = -13;

	public const int ANY_GROUP_6 = -14;

	public const int ANY_GROUP_7 = -15;

	public const int ANY_GROUP_8 = -16;

	public const int NO_FLOOR = -17;

	public const int IN_GROUP_1 = -18;

	public const int IN_GROUP_2 = -19;

	public const int IN_GROUP_3 = -20;

	public const int IN_GROUP_4 = -21;

	public const int IN_GROUP_5 = -22;

	public const int IN_GROUP_6 = -23;

	public const int IN_GROUP_7 = -24;

	public const int IN_GROUP_8 = -25;

	public const int IN_GROUP_9 = -26;

	public const int IN_GROUP_10 = -27;

	public const int IN_GROUP_11 = -28;

	public const int IN_GROUP_12 = -29;

	public const int IN_GROUP_13 = -30;

	public const int IN_GROUP_14 = -31;

	public const int IN_GROUP_15 = -32;

	public const int IN_GROUP_16 = -33;

	public const int IN_GROUP_17 = -34;

	public const int IN_GROUP_18 = -35;

	public const int IN_GROUP_19 = -36;

	public const int IN_GROUP_20 = -37;

	public const int IN_GROUP_21 = -38;

	public const int IN_GROUP_22 = -39;

	public const int IN_GROUP_23 = -40;

	public const int IN_GROUP_24 = -41;

	public const int IN_GROUP_25 = -42;

	public const int IN_GROUP_26 = -43;

	public const int IN_GROUP_27 = -44;

	public const int IN_GROUP_28 = -45;

	public const int IN_GROUP_29 = -46;

	public const int IN_GROUP_30 = -47;

	public const int IN_GROUP_31 = -48;

	public const int IN_GROUP_32 = -49;

	public const int INVALID_LIMITATION_ID = -1;

	public const int INVALID_VARIATION_ID = -1;

	public const int TRUST_THE_ID = -2;

	public const long ALL_FAMILIES = -1L;

	public int m_ID = -1;

	public int m_MyInstanceID = -1;

	public static int m_VisualRepBeingMade;

	public int m_ValidLayers = 12;

	public BuildingBlockDrawingMode m_DrawingTool;

	public bool m_EditorOnly;

	public Footprint m_Footprint;

	public Material m_UIImage;

	public Texture2D m_TextureStamp;

	public int m_Variation = -1;

	public bool m_VariationSelectable = true;

	public string m_BlockNameID = string.Empty;

	public string m_BlockDescriptionID = string.Empty;

	public int m_LimitationGroup = -1;

	public int m_LimitationCount = 1;

	public LevelDetailsManager.LevelEditorDataVersion m_FirstVersionAllowed = LevelDetailsManager.LevelEditorDataVersion.V1_InitialRelease;

	public LevelDetailsManager.LevelEditorDataVersion m_LastVersionAllowed = LevelDetailsManager.LevelEditorDataVersion.ALL_VERSIONS;

	public bool m_BlocksPlayer;

	public int m_OrderNumber = 9999999;

	public bool m_AutomaticBlock;

	public int m_HasPhotonViewCount;

	public GroupFlags m_GroupFlags;

	public bool m_RequiresClearence;

	public bool m_NoBlockingBelow;

	public GameObject m_Brush;

	public GameObject m_SelectionImageObject;

	public GameObject m_Representation;

	public GameObject m_RealObject;

	public GameObject[] m_Representations = new GameObject[0];

	public GameObject[] m_RealObjects = new GameObject[0];

	public BlockSet m_OurBlockSets = BlockSet.CentrePerks;

	public PurposeGroups m_BlocksPurpose;

	public int m_VariationNumber = -1;

	public long m_Family;

	private static GameObject m_SelectionImagePrefab;

	private const string EDITOR_ONLY_TAG = "EditorOnly";

	public virtual BuildingBlockType BlockType => BuildingBlockType.UNKNOWN;

	protected virtual void Awake()
	{
		TestID("Awake");
	}

	protected virtual void OnDrawGizmosSelected()
	{
		if (m_Footprint != null)
		{
			m_Footprint.DrawGizmo(base.transform.position);
		}
	}

	private IEnumerator ResetInstanceID()
	{
		yield return null;
		m_MyInstanceID = GetInstanceID();
	}

	public void TestID(string strFrom)
	{
	}

	public virtual void MakeVisualRepresentation(int iIndex)
	{
		m_Representations[iIndex] = new GameObject("Visual Rep " + iIndex);
		m_Representations[iIndex].SetActive(iIndex == 0);
		m_Representations[iIndex].transform.parent = m_Representation.transform;
		m_Representations[iIndex].transform.localPosition = new Vector3(0f, 0f, 0f);
	}

	public virtual void MakeActualObject(int iIndex)
	{
		m_RealObjects[iIndex] = new GameObject("Actual Object " + iIndex);
		m_RealObjects[iIndex].SetActive(value: false);
		m_RealObjects[iIndex].transform.parent = m_RealObject.transform;
		m_RealObjects[iIndex].transform.localPosition = new Vector3(0f, 0f, 0f);
	}

	public void MakeRepresentations()
	{
		m_Footprint = null;
		DestroyVisualRepresentations();
		int numberOfVersionsRequired = GetNumberOfVersionsRequired();
		m_Representations = new GameObject[numberOfVersionsRequired];
		if (m_Representation == null)
		{
			m_Representation = new GameObject("Visual Rep");
			m_Representation.SetActive(value: true);
			m_Representation.transform.parent = base.transform;
			m_Representation.transform.localPosition = new Vector3(0f, 0f, 0f);
		}
		else
		{
			m_Representation.transform.localPosition = new Vector3(0f, 0f, 0f);
		}
		m_VisualRepBeingMade++;
		for (int i = 0; i < numberOfVersionsRequired; i++)
		{
			MakeVisualRepresentation(i);
		}
		CreateSelectionImage();
		CreateBrushImage();
		CleanUpObject(m_Representation);
		m_VisualRepBeingMade--;
	}

	public void MakeActualRepresentations()
	{
		DestroyActualRepresentations();
		int numberOfVersionsRequired = GetNumberOfVersionsRequired();
		m_RealObjects = new GameObject[numberOfVersionsRequired];
		if (m_RealObject == null)
		{
			m_RealObject = new GameObject("Real Objects");
			m_RealObject.SetActive(value: false);
			m_RealObject.transform.parent = base.transform;
			m_RealObject.transform.localPosition = new Vector3(0f, 0f, 0f);
		}
		m_HasPhotonViewCount = 0;
		for (int i = 0; i < numberOfVersionsRequired; i++)
		{
			MakeActualObject(i);
			CleanUpActualObject(i);
		}
		CleanUpObject(m_RealObject);
	}

	public void DestroyVisualRepresentations()
	{
		for (int num = m_Representations.Length - 1; num >= 0; num--)
		{
			if (m_Representations[num] != null)
			{
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(m_Representations[num]);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(m_Representations[num]);
				}
			}
		}
		m_Representations = new GameObject[0];
	}

	public void DestroyActualRepresentations()
	{
		for (int num = m_RealObjects.Length - 1; num >= 0; num--)
		{
			if (m_RealObjects[num] != null)
			{
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(m_RealObjects[num]);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(m_RealObjects[num]);
				}
			}
		}
		m_RealObjects = new GameObject[0];
	}

	public void DestroyBrush()
	{
		if (m_Brush != null)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(m_Brush);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(m_Brush);
			}
			m_Brush = null;
		}
	}

	private void CleanUpActualObject(int iIndex)
	{
		BaseLevelEditorKeepers[] componentsInChildren = m_RealObjects[iIndex].GetComponentsInChildren<BaseLevelEditorKeepers>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i] != null)
			{
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(componentsInChildren[i]);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(componentsInChildren[i]);
				}
			}
		}
	}

	private void CleanUpObject(GameObject gO)
	{
		if (Application.isPlaying)
		{
			Recursive_DeleteEditorOnly(gO);
		}
	}

	private void Recursive_DeleteEditorOnly(GameObject obj)
	{
		int childCount = obj.transform.childCount;
		for (int num = childCount - 1; num >= 0; num--)
		{
			Transform child = obj.transform.GetChild(num);
			if (child != null)
			{
				GameObject obj2 = child.gameObject;
				Recursive_DeleteEditorOnly(obj2);
			}
		}
		if (obj.CompareTag("EditorOnly"))
		{
			if (Application.isPlaying)
			{
				obj.SetActive(value: false);
				UnityEngine.Object.Destroy(obj);
			}
			else
			{
				obj.SetActive(value: false);
				UnityEngine.Object.DestroyImmediate(obj);
			}
		}
	}

	public virtual void HouseKeeping()
	{
	}

	public virtual int GetNumberOfVersionsRequired()
	{
		return 1;
	}

	protected virtual void AddToFootprint(Footprint footPrint, int xOffset = 0, int yOffset = 0, BaseLevelManager.LevelLayers layer = BaseLevelManager.LevelLayers.TOTAL)
	{
		if (m_Footprint == null)
		{
			m_Footprint = new Footprint(footPrint, xOffset, yOffset, bMultiLevel: false, layer);
		}
		else
		{
			m_Footprint.CombineFootprints(xOffset, yOffset, footPrint);
		}
	}

	protected virtual void ProcessComponent(GameObject masterGameObject, Component comp, Type compType, ref bool bKeep, ref bool bClear, int iVersionIndex = 0)
	{
		if (compType == typeof(BoxCollider) || compType.IsSubclassOf(typeof(BoxCollider)))
		{
			BoxCollider boxCollider = comp as BoxCollider;
			Vector3 vector = new Vector3(boxCollider.size.x * boxCollider.transform.localScale.x, boxCollider.size.y * boxCollider.transform.localScale.y, boxCollider.size.z * boxCollider.transform.localScale.z);
			Vector3 center = boxCollider.center;
			int num = (int)vector.x;
			int num2 = (int)vector.y;
			if (num == 0)
			{
				num = 1;
			}
			if (num2 == 0)
			{
				num2 = 1;
			}
			if (((uint)num & (true ? 1u : 0u)) != 0)
			{
				center.x -= (float)(num - 1) / 2f;
			}
			else
			{
				center.x -= (float)num / 2f;
			}
			if (((uint)num2 & (true ? 1u : 0u)) != 0)
			{
				center.y -= (float)(num2 - 1) / 2f;
			}
			else
			{
				center.y -= (float)num2 / 2f;
			}
			if (boxCollider.transform != masterGameObject.transform)
			{
				Vector3 vector2 = boxCollider.transform.position - masterGameObject.transform.position;
				center.x += vector2.x;
				center.y += vector2.y;
			}
			Footprint footPrint = new Footprint((int)center.x, (int)center.y, num, num2, GetBlockAsFootprintType());
			AddToFootprint(footPrint);
			bClear = true;
			bKeep = false;
		}
		else if (compType == typeof(Renderer) || compType.IsSubclassOf(typeof(Renderer)))
		{
			bClear = true;
			bKeep = true;
		}
		else if (compType == typeof(BaseLevelEditorKeepers) || compType.IsSubclassOf(typeof(BaseLevelEditorKeepers)))
		{
			bClear = true;
			bKeep = true;
		}
		else if (compType == typeof(BaseVisualObjectKeeper) || compType.IsSubclassOf(typeof(BaseVisualObjectKeeper)))
		{
			bClear = true;
			bKeep = true;
		}
		else if (compType == typeof(MeshFilter) || compType.IsSubclassOf(typeof(MeshFilter)))
		{
			bClear = true;
			bKeep = true;
		}
		else if (compType == typeof(Animator) || compType.IsSubclassOf(typeof(Animator)))
		{
			bClear = true;
			bKeep = true;
			((Animator)comp).enabled = false;
		}
		else if (compType == typeof(Transform) || compType.IsSubclassOf(typeof(Transform)))
		{
			bClear = true;
			bKeep = true;
		}
		else if (compType == typeof(SnapshotImprover) || compType.IsSubclassOf(typeof(SnapshotImprover)))
		{
			bClear = true;
			bKeep = true;
		}
		else if (compType == typeof(BrushOffset) || compType.IsSubclassOf(typeof(BrushOffset)))
		{
			bClear = true;
			bKeep = true;
		}
	}

	public Footprint.BlockTypes GetBlockAsFootprintType()
	{
		Footprint.BlockTypes blockTypes = Footprint.BlockTypes.None;
		if (m_NoBlockingBelow)
		{
			blockTypes |= Footprint.BlockTypes.NoBlockingBelow;
		}
		switch (BlockType)
		{
		case BuildingBlockType.Tile:
			blockTypes |= Footprint.BlockTypes.Tiles;
			break;
		case BuildingBlockType.Wall:
			blockTypes |= Footprint.BlockTypes.Walls | Footprint.BlockTypes.Blocking;
			if (((BuildingBlock_Wall)this).m_FloorTileID != -1)
			{
				blockTypes |= Footprint.BlockTypes.SolidWall;
			}
			break;
		case BuildingBlockType.Decoration:
		case BuildingBlockType.Object:
			blockTypes |= Footprint.BlockTypes.Objects;
			if (((BuildingBlock_Object)this).m_Solid)
			{
				blockTypes |= Footprint.BlockTypes.Blocking;
			}
			break;
		case BuildingBlockType.Complex:
		case BuildingBlockType.Room:
			blockTypes |= Footprint.BlockTypes.All;
			break;
		default:
			return Footprint.BlockTypes.None;
		}
		return blockTypes;
	}

	public virtual CompletionState GetBlockCompletionState(ref string strProblems, bool bCreateErrorString = false)
	{
		CompletionState result = CompletionState.Complete;
		if (string.IsNullOrEmpty(m_BlockNameID))
		{
			if (bCreateErrorString)
			{
				strProblems += "The Name ID is empty\n";
			}
			result = CompletionState.Nearly_Complete;
		}
		if (string.IsNullOrEmpty(m_BlockDescriptionID))
		{
			if (bCreateErrorString)
			{
				strProblems += "The Desc ID is empty\n";
			}
			result = CompletionState.Nearly_Complete;
		}
		if (m_UIImage == null)
		{
			if (bCreateErrorString)
			{
				strProblems += "There is no image set up for the player to see\n";
			}
			result = CompletionState.Nearly_Complete;
		}
		if (m_ID == -1)
		{
			if (bCreateErrorString)
			{
				strProblems += "Invalid ID\n";
			}
			result = CompletionState.Unfinished;
		}
		return result;
	}

	public virtual CompletionState GetBlockCompletionState()
	{
		CompletionState result = CompletionState.Complete;
		if (string.IsNullOrEmpty(m_BlockNameID))
		{
			result = CompletionState.Nearly_Complete;
		}
		if (string.IsNullOrEmpty(m_BlockDescriptionID))
		{
			result = CompletionState.Nearly_Complete;
		}
		if (m_UIImage == null)
		{
			result = CompletionState.Nearly_Complete;
		}
		if (m_ID == -1)
		{
			result = CompletionState.Unfinished;
		}
		return result;
	}

	public GameObject GetVisualRep(int iIndex)
	{
		if (iIndex >= m_Representations.Length || m_Representations[iIndex] == null)
		{
			MakeRepresentations();
			if (iIndex >= m_Representations.Length || m_Representations[iIndex] == null)
			{
				return null;
			}
		}
		return m_Representations[iIndex];
	}

	public GameObject GetRealObject(int iIndex)
	{
		if (iIndex >= m_RealObjects.Length || m_RealObjects[iIndex] == null)
		{
			MakeActualRepresentations();
			if (iIndex >= m_RealObjects.Length || m_RealObjects[iIndex] == null)
			{
				return null;
			}
		}
		return m_RealObjects[iIndex];
	}

	public GameObject GetSelectionPrefab()
	{
		if (m_SelectionImagePrefab == null)
		{
			m_SelectionImagePrefab = Resources.Load<GameObject>("LevelEditor\\Prefabs\\SelectionPrefab");
			if (!(m_SelectionImagePrefab == null))
			{
			}
		}
		return m_SelectionImagePrefab;
	}

	public virtual void CreateSelectionImage()
	{
		if (m_SelectionImageObject == null)
		{
			if (GetSelectionPrefab() == null)
			{
				return;
			}
			m_SelectionImageObject = UnityEngine.Object.Instantiate(GetSelectionPrefab(), base.transform);
			m_SelectionImageObject.transform.localPosition = Vector3.zero;
			m_SelectionImageObject.name = "SelectionIcon";
			m_SelectionImageObject.SetActive(value: false);
		}
		if (m_UIImage != null)
		{
			MeshRenderer component = m_SelectionImageObject.GetComponent<MeshRenderer>();
			if (component != null)
			{
				component.material = m_UIImage;
			}
		}
	}

	public virtual GameObject GetDefaultRepresentation()
	{
		return GetVisualRep(0);
	}

	protected virtual void CreateBrushImage()
	{
		DestroyBrush();
		BuildingBlockManager instance = BuildingBlockManager.GetInstance();
		if (instance != null && instance.m_BrushPrefab != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(instance.m_BrushPrefab, base.transform);
			if (gameObject != null)
			{
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.SetActive(value: false);
				gameObject.name = "Brush";
				m_Brush = gameObject;
				LevelEditorBrushController component = m_Brush.GetComponent<LevelEditorBrushController>();
				if (component != null)
				{
					component.Setup(m_ID);
				}
				GameObject defaultRepresentation = GetDefaultRepresentation();
				if (defaultRepresentation != null)
				{
					GameObject gameObject2 = UnityEngine.Object.Instantiate(defaultRepresentation, gameObject.transform);
					gameObject2.SetActive(value: true);
					gameObject2.name = "Rep";
					component.m_VisualRep = gameObject2;
					BrushOffset[] componentsInChildren = gameObject2.GetComponentsInChildren<BrushOffset>(includeInactive: true);
					for (int num = componentsInChildren.Length - 1; num >= 0; num--)
					{
						if (componentsInChildren[num] != null)
						{
							Vector3 localPosition = componentsInChildren[num].transform.localPosition;
							localPosition.z += componentsInChildren[num].m_Offset_Z;
							componentsInChildren[num].transform.localPosition = localPosition;
						}
					}
				}
			}
		}
		if (!(m_Brush != null))
		{
			return;
		}
		BaseLevelEditorKeepers[] componentsInChildren2 = m_Brush.GetComponentsInChildren<BaseLevelEditorKeepers>(includeInactive: true);
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			if (componentsInChildren2[i] != null)
			{
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(componentsInChildren2[i]);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(componentsInChildren2[i]);
				}
			}
		}
		CleanUpObject(m_Brush);
	}

	public bool IsValidForLayer(BaseLevelManager.LevelLayers layer)
	{
		int num = 3 << (int)layer * 2;
		if ((m_ValidLayers & num) == 0)
		{
			return false;
		}
		return true;
	}
}
