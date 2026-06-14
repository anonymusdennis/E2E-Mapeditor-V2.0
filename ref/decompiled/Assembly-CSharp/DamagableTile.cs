using System;
using System.Collections.Generic;
using Pathfinding;
using Rotorz.Tile;
using UnityEngine;

public class DamagableTile : T17MonoBehaviour, ISaveableTileComponent
{
	public enum DamageAction
	{
		Dig,
		Chip,
		Cut,
		Unscrew,
		Hole
	}

	public DamageAction m_DamageAction;

	public ItemData[] m_LimitDamageToItems;

	public ItemData[] m_LimitCoverToItems;

	public float m_InitialHealth = -1f;

	public bool m_StayVisible;

	public bool m_AllowRandomRock = true;

	public Material[] m_Materials;

	public ItemData m_ReclaimedItem;

	public TrackableUIElementsReporter m_TrackableElementReporter;

	public Sprite m_MapIcon;

	public string m_MapToolTipTag;

	public Renderer m_Renderer;

	public Collider m_Collider;

	public MeshFilter m_filter;

	private float m_Health;

	private ElectricFence m_ElectricFence;

	private int m_HoldingItemViewID = -1;

	private string m_HoldingItemName = string.Empty;

	private Item m_HoldingItemItem;

	private ItemCover m_ItemCover;

	private bool m_bHoleIsCovered;

	private ItemContainer m_FloorManagerItemContainer;

	private int m_DamageStage;

	private Character m_DestroyedBy;

	private int m_LastDamagedBy = -1;

	private bool m_bPreviouslyDestroyed;

	private bool m_bHasTransition;

	private FloorManager.Floor m_CurrentFloor;

	private int m_TileRow = -1;

	private int m_TileColumn = -1;

	private GameObject m_GroundTileUnder;

	private int m_PinID = -1;

	private int m_ItemMgrResponseID = -1;

	private List<DamagableTile> m_DiggableNeighbours = new List<DamagableTile>();

	private List<DamagableTile> m_ChippableNeighbours = new List<DamagableTile>();

	private List<IndestructibleTile> m_IndestructableNeighbours = new List<IndestructibleTile>();

	private static int[,] m_NeighbourOffsets = new int[8, 2]
	{
		{ -1, -1 },
		{ -1, 0 },
		{ -1, 1 },
		{ 0, -1 },
		{ 0, 1 },
		{ 1, -1 },
		{ 1, 0 },
		{ 1, 1 }
	};

	private static int[] m_CardinalNeighbourIndices = new int[4] { 1, 3, 4, 6 };

	[HideInInspector]
	public bool m_usingUvOffsetForDamage;

	private float[] m_initialUvsX = new float[4];

	public GameObject m_ChildTransformObject;

	private const float kMaxHealth = 100f;

	private const int kDestroyedWallCost = 15000;

	private const int kUndergroundTileFixedCost = 25000;

	private const int kDestroyedGroundTransitionCost = 50000;

	private const int kPreviouslyDestroyedConnectionsCost = 8000000;

	private bool m_HoldingItem => m_HoldingItemViewID != -1 || m_bHoleIsCovered;

	public float Health => m_Health;

	public int DamageStage => m_DamageStage;

	public FloorManager.Floor CurrentFloor => m_CurrentFloor;

	public int TileRow => m_TileRow;

	public int TileColumn => m_TileColumn;

	public event Action<float> m_OnHealthUpdated;

	public event Action<Item> m_OnHeldItemChanged;

	protected override void Awake()
	{
		base.Awake();
		if (m_Renderer == null)
		{
			m_Renderer = base.gameObject.GetComponent<Renderer>();
		}
		if (m_Collider == null)
		{
			m_Collider = base.gameObject.GetComponent<Collider>();
		}
		if (m_filter == null)
		{
			m_filter = base.gameObject.GetComponent<MeshFilter>();
		}
		if (m_DamageAction == DamageAction.Cut)
		{
			m_ElectricFence = base.gameObject.GetComponentInChildren<ElectricFence>();
		}
		Vector3 position = base.transform.position;
		Vector3 localScale = base.transform.localScale;
		if (localScale.y > 1.5f)
		{
			position.y -= 0.5f;
			m_ChildTransformObject = new GameObject();
			m_ChildTransformObject.transform.position = position;
			m_ChildTransformObject.transform.parent = base.transform;
			m_ChildTransformObject.name = "TallFenceAITarget";
		}
		if (localScale.x > 1.25f || localScale.y > 2.25f)
		{
		}
		if (m_usingUvOffsetForDamage && m_filter != null && m_filter.sharedMesh != null)
		{
			Vector2[] uv = m_filter.sharedMesh.uv;
			if (uv.Length == 4)
			{
				for (int i = 0; i < uv.Length; i++)
				{
					m_initialUvsX[i] = uv[i].x;
				}
			}
		}
		if (m_TrackableElementReporter == null)
		{
			m_TrackableElementReporter = GetComponent<TrackableUIElementsReporter>();
		}
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		m_DamageStage = -1;
		m_CurrentFloor = FloorManager.GetInstance().FindFloorAtZ(base.transform.position.z);
		FloorManager.GetInstance().GetTileGridPoint(m_CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, GetCorrectedTilePosition(), out m_TileRow, out m_TileColumn);
		m_FloorManagerItemContainer = FloorManager.GetInstance().GetItemContainer();
		if (m_Collider != null && m_DamageAction == DamageAction.Dig)
		{
			m_Collider.enabled = false;
		}
		if (m_DamageAction == DamageAction.Dig || m_CurrentFloor.m_FloorType == FloorManager.FLOOR_TYPE.Floor_UnderGround)
		{
			int row = -1;
			int column = -1;
			if (FloorManager.GetInstance().GetTileGridPoint(m_CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Wall, GetCorrectedTilePosition(), out row, out column))
			{
				int num = -1;
				int num2 = -1;
				DamagableTile damagableTile = null;
				DamagableTile damagableTile2 = null;
				for (int i = 0; i < m_NeighbourOffsets.GetLength(0); i++)
				{
					num = row + m_NeighbourOffsets[i, 0];
					num2 = column + m_NeighbourOffsets[i, 1];
					damagableTile = FloorManager.GetInstance().GetDamagableTile(m_CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Wall, num, num2, DamageAction.Dig);
					m_DiggableNeighbours.Add(damagableTile);
					damagableTile2 = FloorManager.GetInstance().GetDamagableTile(m_CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Wall, num, num2, DamageAction.Chip);
					m_ChippableNeighbours.Add(damagableTile2);
					if (damagableTile2 == null && damagableTile == null)
					{
						m_IndestructableNeighbours.Add(FloorManager.GetInstance().GetTileComponent<IndestructibleTile>(m_CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Wall, num, num2));
					}
					else
					{
						m_IndestructableNeighbours.Add(null);
					}
				}
			}
		}
		SetHealth(100f, -1, shouldUpdateBucketSystem: false);
		return base.StartInit();
	}

	private bool CanBeCoveredByItem(int coverItemID)
	{
		if (m_LimitCoverToItems == null || m_LimitCoverToItems.Length == 0)
		{
			return true;
		}
		for (int i = 0; i < m_LimitCoverToItems.Length; i++)
		{
			if (m_LimitCoverToItems[i] != null && m_LimitCoverToItems[i].m_ItemDataID == coverItemID)
			{
				return true;
			}
		}
		return false;
	}

	private bool CanBeDamagedByAction(DamageAction action)
	{
		return action == m_DamageAction;
	}

	private bool CanBeDamagedByItem(int damagingItemID)
	{
		if (m_LimitDamageToItems == null || m_LimitDamageToItems.Length == 0)
		{
			return true;
		}
		for (int i = 0; i < m_LimitDamageToItems.Length; i++)
		{
			if (m_LimitDamageToItems[i] != null && m_LimitDamageToItems[i].m_ItemDataID == damagingItemID)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanBeCovered(int coverItemID)
	{
		if (m_DamageAction == DamageAction.Hole)
		{
			return false;
		}
		return CanBeCoveredByItem(coverItemID) && Mathf.Approximately(m_Health, 0f) && !m_HoldingItem;
	}

	public bool CanBeDamaged(DamageAction action, int damagingItemID)
	{
		if (m_DamageAction == DamageAction.Hole)
		{
			return false;
		}
		return CanBeDamagedByAction(action) && CanBeDamagedByItem(damagingItemID) && m_Health > 0f && !m_HoldingItem;
	}

	public bool HasBeenDamaged()
	{
		return !Mathf.Approximately(m_Health, 100f);
	}

	public bool IsFullyDamaged()
	{
		return Mathf.Approximately(m_Health, 0f);
	}

	public bool WouldFullyDamage(DamageAction action, int damagingItemID, float damageAmount, out ItemData itemReclaimed)
	{
		if (CanBeDamaged(action, damagingItemID) && (damageAmount < 0f || damageAmount >= m_Health))
		{
			itemReclaimed = m_ReclaimedItem;
			return true;
		}
		itemReclaimed = null;
		return false;
	}

	public bool IsHoldingItem()
	{
		return m_HoldingItem;
	}

	public bool Damage(DamageAction action, int damagingItemID, float damageAmount, bool bAllowReclaimItem, Character character)
	{
		if (CanBeDamagedByAction(action) && CanBeDamagedByItem(damagingItemID))
		{
			if (damageAmount < 0f)
			{
				damageAmount = m_Health;
			}
			bool flag = m_Health > 0f;
			SetHealth(m_Health - damageAmount, character.m_NetView.viewID);
			if (flag && Mathf.Approximately(m_Health, 0f))
			{
				m_DestroyedBy = character;
				if (bAllowReclaimItem && m_ReclaimedItem != null && m_DestroyedBy != null && m_DestroyedBy.IsPlayer())
				{
					ItemManager.GetInstance().AssignItemRPC(m_DestroyedBy.m_NetView.viewID, m_ReclaimedItem.m_ItemDataID, OnReclaimItemResponse, ref m_ItemMgrResponseID);
				}
			}
			return true;
		}
		return false;
	}

	public bool Repair(float fHealthRestoreAmount)
	{
		if (fHealthRestoreAmount < 0f)
		{
			fHealthRestoreAmount = 100f - m_Health;
		}
		SetHealth(m_Health + fHealthRestoreAmount, m_LastDamagedBy);
		return true;
	}

	public void SetHealth(float fHealth, int damagedBy = -1, bool shouldUpdateBucketSystem = true)
	{
		float health = m_Health;
		m_Health = Mathf.Clamp(fHealth, 0f, 100f);
		if (m_Health != health && this.m_OnHealthUpdated != null)
		{
			this.m_OnHealthUpdated(m_Health);
		}
		UpdateAIEvents(damagedBy);
		if (m_DamageAction == DamageAction.Hole)
		{
			return;
		}
		UpdateDamageStateMaterial();
		UpdateNamePlate();
		SetTileVisibility();
		UpdatePins();
		if (shouldUpdateBucketSystem)
		{
			UpdateBucketSystem();
		}
		if (!Mathf.Approximately(m_Health, 0f) || (m_DamageAction != 0 && m_CurrentFloor.m_FloorType != FloorManager.FLOOR_TYPE.Floor_UnderGround))
		{
			return;
		}
		if (m_Collider != null)
		{
			m_Collider.enabled = false;
		}
		for (int i = 0; i < m_CardinalNeighbourIndices.Length; i++)
		{
			DamagableTile damagableTile = m_DiggableNeighbours[m_CardinalNeighbourIndices[i]];
			if (damagableTile != null && damagableTile.m_Health > 0f && damagableTile.m_Collider != null)
			{
				damagableTile.m_Collider.enabled = true;
			}
		}
		for (int j = 0; j < m_DiggableNeighbours.Count; j++)
		{
			DamagableTile damagableTile2 = m_DiggableNeighbours[j];
			if (damagableTile2 != null && damagableTile2.m_Health > 0f)
			{
				damagableTile2.UpdateAppearance();
			}
		}
	}

	private void UpdatePins()
	{
		if (m_Health > 0f)
		{
			if (m_PinID != -1)
			{
				PinManager.GetInstance().RemovePin(m_PinID);
				m_PinID = -1;
			}
		}
		else
		{
			m_PinID = PinManager.GetInstance().CreatePin(bForMainMap: true, bForMiniMap: true, base.gameObject, m_MapIcon, floor: m_CurrentFloor, overrideIconScale: new Vector3(0.5f, 0.5f, 1f), bUpdatePosition: false, players: null, filterType: PinManager.Pin.PinFilterType.All, edgable: false, floorTrackable: false, directional: false, toolTipTag: m_MapToolTipTag, localiseToolTipTag: true, bOverrideIconScale: true);
		}
	}

	private void UpdateBucketSystem()
	{
		CullingBuckets.RequestBucketDamagableUpdate(base.transform.position);
	}

	private void SetTileVisibility()
	{
		if (m_StayVisible || m_HoldingItem)
		{
			base.gameObject.SetActive(value: true);
			bool active = !IsFullyDamaged() || m_HoldingItem;
			if ((bool)m_Collider)
			{
				m_Collider.enabled = active;
			}
			if ((bool)m_ElectricFence)
			{
				m_ElectricFence.gameObject.SetActive(active);
			}
		}
		else
		{
			bool active2 = !IsFullyDamaged();
			base.gameObject.SetActive(active2);
			if (m_Collider != null)
			{
				m_Collider.enabled = active2;
			}
			if (m_GroundTileUnder != null)
			{
				m_GroundTileUnder.SetActive(Mathf.Approximately(m_Health, 0f));
			}
		}
		if (m_HoldingItem)
		{
			if (m_ItemCover == null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(FloorManager.GetInstance().m_ItemCoverPrefab, base.transform.position, Quaternion.identity, base.transform);
				if (gameObject != null)
				{
					m_ItemCover = gameObject.GetComponent<ItemCover>();
				}
			}
			if (m_ItemCover != null && m_HoldingItemItem != null && m_HoldingItemItem.m_ItemData != null)
			{
				if (ShowVertical())
				{
					m_ItemCover.SetMaterial(m_HoldingItemItem.m_ItemData.m_ItemTileCoverVerticalMaterial);
				}
				else
				{
					m_ItemCover.SetMaterial(m_HoldingItemItem.m_ItemData.m_ItemTileCoverHorizontalMaterial);
				}
			}
		}
		else if (m_ItemCover != null)
		{
			UnityEngine.Object.Destroy(m_ItemCover.gameObject);
		}
	}

	private bool ShowVertical()
	{
		if (m_DamageAction == DamageAction.Dig || m_DamageAction == DamageAction.Unscrew)
		{
			return true;
		}
		if (FloorManager.GetInstance().GetTileGridPoint(m_CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Wall, GetCorrectedTilePosition(), out var row, out var column))
		{
			TileData tile = FloorManager.GetInstance().GetTile(m_CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Wall, row + 1, column);
			TileData tile2 = FloorManager.GetInstance().GetTile(m_CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Wall, row - 1, column);
			if ((tile != null && tile.gameObject.layer == base.gameObject.layer) || (tile2 != null && tile2.gameObject.layer == base.gameObject.layer))
			{
				return false;
			}
		}
		return true;
	}

	public bool CanGiveItem()
	{
		if (m_HoldingItemViewID == -1)
		{
			return m_FloorManagerItemContainer.GetFreeSpaceCount() > 0;
		}
		return false;
	}

	public bool GiveItem(int itemViewID)
	{
		if (m_HoldingItemViewID == -1)
		{
			Item item = T17NetView.Find<Item>(itemViewID);
			if (item != null && m_FloorManagerItemContainer.AddItemRPC(item))
			{
				SetItem(itemViewID);
				return true;
			}
			return false;
		}
		return false;
	}

	public bool TransferItemTo(ItemContainer toContainer)
	{
		if (m_HoldingItemViewID == -1)
		{
			return false;
		}
		bool flag = false;
		if (!toContainer.IsVisibleFull())
		{
			m_FloorManagerItemContainer.MoveItemToAnotherContainerRPC(m_HoldingItemViewID, toContainer.NetView.viewID);
			flag = true;
		}
		else if (toContainer.m_ContainerType == ItemContainer.ItemContainerType.Inmate)
		{
			Character characterOwner = toContainer.GetCharacterOwner();
			if (characterOwner != null && characterOwner.GetEquippedItem() == null)
			{
				m_FloorManagerItemContainer.MoveItemToCharacterEquipedSlot(m_HoldingItemViewID, characterOwner.m_NetView.viewID);
				flag = true;
			}
		}
		if (flag)
		{
			if (m_HoldingItemItem != null)
			{
				CoverTileFunctionality coverTileFunctionality = m_HoldingItemItem.HasFunctionality(BaseItemFunctionality.Functionality.CoverTile) as CoverTileFunctionality;
				if (coverTileFunctionality != null)
				{
					coverTileFunctionality.OnPickUpCover();
				}
			}
			SetItem(-1);
		}
		return flag;
	}

	public bool DestroyItem()
	{
		if (m_HoldingItemViewID == -1)
		{
			return false;
		}
		Item item = T17NetView.Find<Item>(m_HoldingItemViewID);
		if (item != null)
		{
			m_FloorManagerItemContainer.RemoveItemRPC(item, releaseToManager: true);
			SetItem(-1);
			return true;
		}
		return false;
	}

	public void SetItem(int itemViewID, bool shouldUpdateBucketSystem = true)
	{
		if (m_DamageAction == DamageAction.Hole)
		{
			return;
		}
		m_HoldingItemViewID = itemViewID;
		if (itemViewID != -1)
		{
			m_HoldingItemItem = T17NetView.Find<Item>(itemViewID);
			if (m_HoldingItemItem != null)
			{
				Localization.Get(m_HoldingItemItem.ItemName, out m_HoldingItemName);
				m_HoldingItemItem.m_CoveringTile = this;
			}
		}
		else
		{
			if (m_HoldingItemItem != null)
			{
				m_HoldingItemItem.m_CoveringTile = null;
			}
			m_HoldingItemItem = null;
			m_HoldingItemName = string.Empty;
		}
		UpdateDamageStateMaterial();
		UpdateNamePlate();
		UpdateAIEvents(m_LastDamagedBy);
		SetTileVisibility();
		UpdatePins();
		if (shouldUpdateBucketSystem)
		{
			UpdateBucketSystem();
		}
		if (this.m_OnHeldItemChanged != null)
		{
			this.m_OnHeldItemChanged(m_HoldingItemItem);
		}
	}

	public void SetHoleIsCovered(bool covered)
	{
		m_bHoleIsCovered = covered;
	}

	public void UpdateAppearance()
	{
		int num = 0;
		int length = m_NeighbourOffsets.GetLength(0);
		int row = -1;
		int column = -1;
		if (FloorManager.GetInstance().GetTileGridPoint(m_CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Wall, GetCorrectedTilePosition(), out row, out column))
		{
			for (int i = 0; i < length; i++)
			{
				DamagableTile damagableTile = m_DiggableNeighbours[i];
				DamagableTile damagableTile2 = m_ChippableNeighbours[i];
				if ((damagableTile == null || Mathf.Approximately(damagableTile.m_Health, 0f)) && (damagableTile2 == null || Mathf.Approximately(damagableTile2.m_Health, 0f)) && m_IndestructableNeighbours[i] == null)
				{
					num += 1 << i;
				}
			}
		}
		Material groundMaterial = null;
		FloorManager.GetInstance().GetUndergroundMaterials(num, ref m_Materials, ref groundMaterial);
		UpdateDamageStateMaterial();
	}

	private void UpdateDamageStateMaterial()
	{
		if (m_Materials == null || m_Materials.Length <= 0 || !(m_Renderer != null))
		{
			return;
		}
		int value;
		if (IsFullyDamaged())
		{
			value = 0;
		}
		else if (m_StayVisible || m_HoldingItem)
		{
			value = m_Materials.Length - 1 - (int)((100f - m_Health) / 100f * (float)(m_Materials.Length - 1));
		}
		else
		{
			float num = 100f / (float)m_Materials.Length;
			value = (int)((m_Health - 1f) / num);
		}
		value = (m_DamageStage = Mathf.Clamp(value, 0, m_Materials.Length - 1));
		if (!m_usingUvOffsetForDamage)
		{
			if (m_Materials[value] != null)
			{
				m_Renderer.material = m_Materials[value];
			}
		}
		else
		{
			if (!(m_filter != null) || !(m_filter.mesh != null) || !(m_Renderer != null) || !(m_Renderer.sharedMaterial != null) || !(m_Renderer.sharedMaterial.mainTexture != null))
			{
				return;
			}
			float num2 = 32f / (float)m_Renderer.sharedMaterial.mainTexture.width;
			Vector2[] uv = m_filter.mesh.uv;
			if (uv.Length == 4)
			{
				for (int i = 0; i < uv.Length; i++)
				{
					uv[i].x = m_initialUvsX[i] + num2 * (float)(m_Materials.Length - 1 - m_DamageStage);
				}
			}
			m_filter.mesh.uv = uv;
		}
	}

	private void UpdateNamePlate()
	{
		if (m_TrackableElementReporter != null)
		{
			if (m_HoldingItem)
			{
				m_TrackableElementReporter.SetDisplayName(m_HoldingItemName);
			}
			else
			{
				m_TrackableElementReporter.SetDisplayName(NumberToStringCache.GetPercentageString((int)m_Health));
			}
		}
	}

	private void OnReclaimItemResponse(Item newItem, int eventID)
	{
		if (newItem != null && m_DestroyedBy != null && eventID == m_ItemMgrResponseID && !m_DestroyedBy.m_ItemContainer.AddItemRPC(newItem))
		{
			newItem.DropItemInLevel(m_DestroyedBy, m_DestroyedBy.transform.position);
		}
	}

	public void UpdateAIEvents(int damagedBy)
	{
		if (Mathf.Approximately(m_Health, 0f))
		{
			m_bPreviouslyDestroyed = true;
		}
		if (HasBeenDamaged())
		{
			m_LastDamagedBy = damagedBy;
			AttachDamageTileToNavMesh();
		}
		UpdateAITileEvent();
		UpdateAINavigation();
		UpdateAINavigationTransitions();
	}

	private void UpdateAITileEvent()
	{
		if (m_DamageAction == DamageAction.Dig)
		{
			return;
		}
		bool flag = m_DamageAction == DamageAction.Cut || m_DamageAction == DamageAction.Chip || m_DamageAction == DamageAction.Dig;
		if (m_CurrentFloor.IsVent() && flag)
		{
			return;
		}
		Transform location = base.transform;
		if (m_ChildTransformObject != null)
		{
			location = m_ChildTransformObject.transform;
		}
		if (m_HoldingItem)
		{
			AIEventManager.GetInstance().SetTileDamagedEvent(location, base.transform, null, setActive: false, flag);
			AIEventManager.GetInstance().SetTileMissingEvent(location, base.transform, null, setActive: false, flag);
			AIEventManager.GetInstance().SetTileDugHoleEvent(location, base.transform, null, setActive: false);
			return;
		}
		Character characterResponsible = null;
		if (m_LastDamagedBy != -1)
		{
			characterResponsible = T17NetView.Find<Character>(m_LastDamagedBy);
		}
		bool setActive = HasBeenDamaged() && !IsFullyDamaged();
		bool setActive2 = IsFullyDamaged();
		AIEventManager.GetInstance().SetTileDamagedEvent(location, base.transform, characterResponsible, setActive, flag);
		if (m_DamageAction == DamageAction.Hole)
		{
			AIEventManager.GetInstance().SetTileDugHoleEvent(location, base.transform, characterResponsible, setActive2);
		}
		else
		{
			AIEventManager.GetInstance().SetTileMissingEvent(location, base.transform, characterResponsible, setActive2, flag);
		}
	}

	private void UpdateAINavigation()
	{
		if (m_DamageAction == DamageAction.Unscrew)
		{
			return;
		}
		bool flag = IsFullyDamaged();
		bool flag2 = flag || m_bPreviouslyDestroyed;
		if (!flag2)
		{
			return;
		}
		Vector3 correctedTilePosition = GetCorrectedTilePosition();
		bool flag3 = m_DamageAction == DamageAction.Hole || m_DamageAction == DamageAction.Unscrew;
		if (flag3)
		{
			FloorManager.Floor floor = FloorManager.GetInstance().DownAFloor(m_CurrentFloor);
			correctedTilePosition.z = floor.m_zPos;
			bool flag4 = m_DamageAction == DamageAction.Unscrew && m_CurrentFloor.m_FloorType == FloorManager.FLOOR_TYPE.Floor_Vent;
			correctedTilePosition += (Vector3)((!flag4) ? new Vector2(0f, -1f) : Vector2.zero);
		}
		else
		{
			correctedTilePosition = GetCorrectedTilePosition();
		}
		GraphNode nearestGraphNode = NavMeshUtil.GetNearestGraphNode(correctedTilePosition, onMesh: false);
		if (nearestGraphNode == null)
		{
			return;
		}
		int penalty = 1000;
		if (m_DamageAction != 0 && !m_CurrentFloor.IsVent() && !flag3)
		{
			penalty = 8000000;
			if (flag && !m_HoldingItem)
			{
				penalty = 15000;
			}
		}
		if (m_DamageAction == DamageAction.Dig)
		{
			penalty = 25000;
		}
		NavMeshUtil.SetNodeWalkable(nearestGraphNode, flag2, penalty);
	}

	public void UpdateAINavigationTransitions()
	{
		if ((m_DamageAction == DamageAction.Unscrew || m_DamageAction == DamageAction.Hole) && IsFullyDamaged())
		{
			m_bHasTransition = true;
		}
		if (m_bHasTransition)
		{
			bool flag = m_DamageAction == DamageAction.Unscrew && m_CurrentFloor.m_FloorType == FloorManager.FLOOR_TYPE.Floor_Vent;
			NavMeshUtil.TransitionDirection direction = NavMeshUtil.TransitionDirection.Down;
			Vector2 offset = ((!flag) ? new Vector2(0f, -1f) : Vector2.zero);
			int connectionCost = 8000000;
			if (IsFullyDamaged() && !m_HoldingItem)
			{
				connectionCost = 50000;
			}
			NavMeshUtil.CreateTransition(direction, m_CurrentFloor, GetCorrectedTilePosition(), offset, connectionCost);
		}
	}

	private void AttachDamageTileToNavMesh()
	{
		GraphNode nearestGraphNode = NavMeshUtil.GetNearestGraphNode(GetCorrectedTilePosition(), onMesh: false);
		if (nearestGraphNode == null)
		{
			return;
		}
		if (m_DamageAction == DamageAction.Hole || m_DamageAction == DamageAction.Unscrew)
		{
			if (nearestGraphNode.m_DamagableGroundTile == null)
			{
				NavMeshUtil.SetDamagableTile(nearestGraphNode, this, wallTile: false);
			}
		}
		else if (nearestGraphNode.m_DamagableWallTile == null)
		{
			NavMeshUtil.SetDamagableTile(nearestGraphNode, this, wallTile: true);
		}
	}

	private Vector3 GetCorrectedTilePosition()
	{
		if (m_ChildTransformObject != null)
		{
			return m_ChildTransformObject.transform.position;
		}
		return base.transform.position;
	}

	public bool RequiresSaving()
	{
		bool flag = false;
		flag = ((!(m_InitialHealth >= 0f)) ? (!Mathf.Approximately(m_Health, 100f)) : (!Mathf.Approximately(m_Health, m_InitialHealth)));
		return flag || m_bPreviouslyDestroyed || m_HoldingItemViewID != -1 || m_bHasTransition;
	}

	public string SerializeData()
	{
		return $"{m_Health},{m_LastDamagedBy},{(m_bPreviouslyDestroyed ? 1 : 0)},{m_HoldingItemViewID},{(m_bHasTransition ? 1 : 0)}";
	}

	public void DeserializeData(string data)
	{
		if (!string.IsNullOrEmpty(data))
		{
			string[] array = data.Split(',');
			if (array.Length == 5)
			{
				m_bHasTransition = int.Parse(array[4]) == 1;
				m_bPreviouslyDestroyed = int.Parse(array[2]) == 1;
				m_HoldingItemViewID = int.Parse(array[3]);
				float num = float.Parse(array[0]);
				bool shouldUpdateBucketSystem = !Mathf.Approximately(num, 100f) || m_HoldingItemViewID != -1;
				SetHealth(num, int.Parse(array[1]), shouldUpdateBucketSystem: false);
				SetItem(m_HoldingItemViewID, shouldUpdateBucketSystem);
			}
		}
	}

	public void SetGroundTileUnder(GameObject groundTile)
	{
		m_GroundTileUnder = groundTile;
	}

	public Collider GetCollider()
	{
		return m_Collider;
	}

	public Vector2[] GetFilterUVs()
	{
		return m_filter.mesh.uv;
	}
}
