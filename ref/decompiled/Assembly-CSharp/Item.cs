using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class Item : T17MonoBehaviour, IControlledUpdate
{
	public delegate void OnItemContainerChangedEvent(Item item);

	public delegate void OnQuestItemStatusChangedEvent();

	public ItemData m_ItemData;

	private const float USEITEM_Z_OFFSET = -0.2f;

	private bool m_bMarkedForUse;

	private int _m_ContainerViewID;

	private int m_OldContainerViewID;

	private ItemContainer _m_ItemContainer;

	[ReadOnly]
	public bool m_bHidden;

	private MeshRenderer m_MeshRenderer;

	private BoxCollider m_BoxCollider;

	private TrackableUIElementsReporter m_TrackableElementsReporter;

	public T17NetView m_NetView;

	private BaseItemFunctionality m_FunctionalityToUpdate;

	[ReadOnly]
	public bool m_bIsAQuestItem;

	[ReadOnly]
	public int m_QuestItemOwnerID = -1;

	[ReadOnly]
	public int m_QuestItemGroupID = -1;

	public bool m_bIsAMagicItem;

	[ReadOnly]
	public DamagableTile m_CoveringTile;

	private Character m_Owner;

	private Vector3 m_SpawnPos = new Vector3(-10f, -10f, 0f);

	private bool m_bIsOnFloor;

	private Vector3 m_OldColliderSize;

	private const string DYNAMIC_MAP_OBJ_LAYER = "DynamicMapObject";

	private const string ITEMS_LAYER = "Items";

	public float m_ProximityPenalty = 1f;

	public ProximityPriorityLayers m_ProximityPriority = ProximityPriorityLayers.High;

	[ReadOnly]
	public int m_DroppedByCharacterViewID = -1;

	public bool MarkedForUse
	{
		get
		{
			return m_bMarkedForUse;
		}
		set
		{
			m_bMarkedForUse = value;
		}
	}

	public int m_ContainerViewID
	{
		get
		{
			return _m_ContainerViewID;
		}
		set
		{
			if (value != _m_ContainerViewID)
			{
				_m_ContainerViewID = value;
				if (this.OnItemContainerChanged != null)
				{
					this.OnItemContainerChanged(this);
				}
			}
		}
	}

	public ItemContainer m_ItemContainer
	{
		get
		{
			if (m_ContainerViewID != m_OldContainerViewID)
			{
				m_OldContainerViewID = m_ContainerViewID;
				_m_ItemContainer = T17NetView.Find<ItemContainer>(m_ContainerViewID);
			}
			return _m_ItemContainer;
		}
	}

	public MeshRenderer MeshRendererProp
	{
		get
		{
			if (m_MeshRenderer == null)
			{
				m_MeshRenderer = GetComponentInChildren<MeshRenderer>(includeInactive: true);
			}
			return m_MeshRenderer;
		}
	}

	public BoxCollider BoxColliderProp
	{
		get
		{
			if (m_BoxCollider == null)
			{
				m_BoxCollider = GetComponent<BoxCollider>();
			}
			return m_BoxCollider;
		}
	}

	public TrackableUIElementsReporter TrackableUIElementReporter => m_TrackableElementsReporter;

	public string ItemName
	{
		get
		{
			if (m_ItemData != null)
			{
				return m_ItemData.m_ItemLocalizationTag;
			}
			return "No ItemData Set";
		}
	}

	public int Health
	{
		get
		{
			if (m_ItemData != null)
			{
				return m_ItemData.m_ItemHealth;
			}
			return 0;
		}
	}

	public int ItemDataID
	{
		get
		{
			if (m_ItemData != null)
			{
				return m_ItemData.m_ItemDataID;
			}
			return -1;
		}
	}

	public int Value
	{
		get
		{
			if (m_ItemData != null)
			{
				return m_ItemData.m_ItemValue;
			}
			return 0;
		}
	}

	public Item_Combat CombatData
	{
		get
		{
			if (m_ItemData != null)
			{
				return m_ItemData.m_CombatData;
			}
			return null;
		}
	}

	public Item_Outfit OutfitData
	{
		get
		{
			if (m_ItemData != null)
			{
				return m_ItemData.m_OutfitData;
			}
			return null;
		}
	}

	public Material HeldMaterial
	{
		get
		{
			if (m_ItemData != null)
			{
				return m_ItemData.m_ItemHeldMaterial;
			}
			return null;
		}
	}

	public Material BoundMaterial
	{
		get
		{
			if (m_ItemData != null)
			{
				return m_ItemData.m_ItemBoundMaterial;
			}
			return null;
		}
	}

	public ItemData.ITEM_ANIMATION_TYPE HeldType
	{
		get
		{
			if (m_ItemData != null)
			{
				return m_ItemData.m_ItemHeldType;
			}
			return ItemData.ITEM_ANIMATION_TYPE.IAT_NOT_SET;
		}
	}

	public Material UseMaterial
	{
		get
		{
			if (m_ItemData != null)
			{
				return m_ItemData.m_ItemUseMaterial;
			}
			return null;
		}
	}

	public ItemData.ITEM_ANIMATION_TYPE UseType
	{
		get
		{
			if (m_ItemData != null)
			{
				return m_ItemData.m_ItemUseType;
			}
			return ItemData.ITEM_ANIMATION_TYPE.IAT_NOT_SET;
		}
	}

	public ItemData.MATERIAL_TYPE MaterialType
	{
		get
		{
			if (m_ItemData != null)
			{
				return m_ItemData.m_MaterialType;
			}
			return ItemData.MATERIAL_TYPE.MAT_GENERIC;
		}
	}

	public event OnItemContainerChangedEvent OnItemContainerChanged;

	public event OnQuestItemStatusChangedEvent OnQuestItemStatusChanged;

	protected override void Awake()
	{
		base.Awake();
		m_NetView = GetComponent<T17NetView>();
		base.transform.position = m_SpawnPos;
	}

	protected virtual void OnDestroy()
	{
		m_NetView = null;
		m_CoveringTile = null;
		m_FunctionalityToUpdate = null;
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		if (m_ItemData != null)
		{
			m_ItemData.Init();
		}
		return T17BehaviourManager.INITSTATE.IS_FINISHED;
	}

	public void ControlledUpdate()
	{
		if (m_FunctionalityToUpdate != null && !m_FunctionalityToUpdate.UpdateUsing())
		{
			m_FunctionalityToUpdate = null;
		}
	}

	public void ControlledFixedUpdate()
	{
	}

	public void SetAsQuestItem(bool isQuestItem, Player questOwner, ref int questItemGroupID)
	{
		int num = ((!(questOwner != null)) ? (-1) : questOwner.m_NetView.viewID);
		LOCAL_SetAsQuestItem(isQuestItem, num, ref questItemGroupID);
		m_NetView.RPC("RPC_SetAsQuestItem", NetTargets.Others, isQuestItem, num, questItemGroupID);
	}

	[PunRPC]
	public void RPC_SetAsQuestItem(bool isQuestItem, int playerOwnerViewID, int questItemGroupID)
	{
		LOCAL_SetAsQuestItem(isQuestItem, playerOwnerViewID, ref questItemGroupID);
	}

	private void LOCAL_SetAsQuestItem(bool isQuestItem, int playerOwnerViewID, ref int questItemGroupID)
	{
		if (isQuestItem)
		{
			if (playerOwnerViewID != -1)
			{
				QuestManager.GetInstance().AddQuestItem(this, playerOwnerViewID, ref questItemGroupID);
			}
		}
		else
		{
			QuestManager.GetInstance().RemoveQuestItem(this);
		}
		bool flag = m_bIsAQuestItem != isQuestItem;
		m_bIsAQuestItem = isQuestItem;
		m_QuestItemOwnerID = playerOwnerViewID;
		m_QuestItemGroupID = questItemGroupID;
		if (flag && this.OnQuestItemStatusChanged != null)
		{
			this.OnQuestItemStatusChanged();
		}
	}

	public void LOADING_SetQuestGroupID(Player owner, int desiredID)
	{
		int playerOwnerViewID = ((!(owner != null)) ? (-1) : owner.m_NetView.viewID);
		int questItemGroupID = desiredID;
		LOCAL_SetAsQuestItem(m_bIsAQuestItem, playerOwnerViewID, ref questItemGroupID);
	}

	public void ResetItem()
	{
		m_ItemData = null;
		if (m_bIsAQuestItem)
		{
			int questItemGroupID = -1;
			SetAsQuestItem(isQuestItem: false, null, ref questItemGroupID);
		}
		m_bHidden = false;
		m_ContainerViewID = 0;
		m_Owner = null;
		m_FunctionalityToUpdate = null;
	}

	public bool IsQuestItem()
	{
		return m_bIsAQuestItem;
	}

	public bool IsMagicItem()
	{
		return m_bIsAMagicItem;
	}

	public int GetGiftValue(Character receiving)
	{
		return GetGiftValue(receiving.m_CharacterRole);
	}

	public int GetGiftValue(CharacterRole receivingRole)
	{
		int result = 0;
		switch (receivingRole)
		{
		case CharacterRole.Inmate:
			result = m_ItemData.m_GiftOpinionValues.Inmate;
			break;
		case CharacterRole.Guard:
			result = m_ItemData.m_GiftOpinionValues.Guard;
			break;
		}
		return result;
	}

	public void DecreaseHealth(int amount)
	{
		if (amount > 0)
		{
			m_NetView.PostLevelLoadRPC("RPC_DecreaseHealth", NetTargets.All, amount);
		}
	}

	public void HandleRespawnedRPC()
	{
		m_NetView.RPC("RPC_All_ItemRespawned", NetTargets.All);
	}

	[PunRPC]
	public void RPC_All_ItemRespawned()
	{
		if (m_ItemData != null)
		{
			m_ItemData.ResetHealth();
		}
	}

	[PunRPC]
	public void RPC_DecreaseHealth(int amount, PhotonMessageInfo info)
	{
		if (!(m_ItemData != null))
		{
			return;
		}
		m_ItemData.m_ItemHealth -= amount;
		if (m_ItemData.m_ItemHealth > 0)
		{
			return;
		}
		m_ItemData.m_ItemHealth = 0;
		if (m_Owner != null)
		{
			m_Owner.RemoveItemRPC(this, RPC_CallContexts.All, release: true);
		}
		else if (m_ContainerViewID != 0)
		{
			ItemContainer itemContainer = T17NetView.Find<ItemContainer>(m_ContainerViewID);
			if (itemContainer != null)
			{
				itemContainer.RemoveItemRPC(this, releaseToManager: true, RPC_CallContexts.All);
			}
		}
	}

	public void SetOwner(Character owner)
	{
		bool flag = m_Owner != owner;
		m_Owner = owner;
		if (m_ItemData != null)
		{
			for (int i = 0; i < m_ItemData.m_ItemFunctionalities.Count; i++)
			{
				if (m_ItemData.m_ItemFunctionalities[i] != null && m_ItemData.m_ItemFunctionalities[i].m_Functionality != null)
				{
					m_ItemData.m_ItemFunctionalities[i].m_Functionality.Owner = m_Owner;
				}
			}
		}
		if (flag && this.OnItemContainerChanged != null)
		{
			this.OnItemContainerChanged(this);
		}
	}

	public Character GetOwner()
	{
		return m_Owner;
	}

	public bool IsImmobilisingOwner()
	{
		if (m_FunctionalityToUpdate != null)
		{
			return m_FunctionalityToUpdate.ImmobilisesOwner();
		}
		return false;
	}

	public bool RequiresTargetting()
	{
		if (m_ItemData == null)
		{
			return false;
		}
		for (int i = 0; i < m_ItemData.m_ItemFunctionalities.Count; i++)
		{
			if (m_ItemData.m_ItemFunctionalities[i] != null && m_ItemData.m_ItemFunctionalities[i].m_Functionality != null && m_ItemData.m_ItemFunctionalities[i].m_Functionality.RequiresTargetting())
			{
				return true;
			}
		}
		return false;
	}

	public bool IsImmediateUse()
	{
		if (m_ItemData == null)
		{
			return true;
		}
		for (int i = 0; i < m_ItemData.m_ItemFunctionalities.Count; i++)
		{
			if (m_ItemData.m_ItemFunctionalities[i] != null && m_ItemData.m_ItemFunctionalities[i].m_Functionality != null && !m_ItemData.m_ItemFunctionalities[i].m_Functionality.IsImmediateUse())
			{
				return false;
			}
		}
		return true;
	}

	public bool CanUse()
	{
		if (m_ItemData == null)
		{
			return false;
		}
		for (int i = 0; i < m_ItemData.m_ItemFunctionalities.Count; i++)
		{
			if (m_ItemData.m_ItemFunctionalities[i] != null && m_ItemData.m_ItemFunctionalities[i].m_Functionality != null && m_ItemData.m_ItemFunctionalities[i].m_Functionality.CanUse())
			{
				return true;
			}
		}
		return false;
	}

	public bool CanUse(Directionx4 facingDirection, out Sprite validTargetHUDSpriteOverride)
	{
		if (m_ItemData != null)
		{
			for (int i = 0; i < m_ItemData.m_ItemFunctionalities.Count; i++)
			{
				if (m_ItemData.m_ItemFunctionalities[i] != null && m_ItemData.m_ItemFunctionalities[i].m_Functionality != null && m_ItemData.m_ItemFunctionalities[i].m_Functionality.CanUse())
				{
					validTargetHUDSpriteOverride = m_ItemData.m_ItemFunctionalities[i].m_Functionality.GetValidTargetHUDSpriteOverride(facingDirection);
					return true;
				}
			}
		}
		validTargetHUDSpriteOverride = null;
		return false;
	}

	public BaseItemFunctionality GetFirstFunctionality()
	{
		if (m_ItemData != null)
		{
			for (int i = 0; i < m_ItemData.m_ItemFunctionalities.Count; i++)
			{
				ItemData.FunctionalityData functionalityData = m_ItemData.m_ItemFunctionalities[i];
				if (functionalityData != null && functionalityData.m_Functionality != null)
				{
					return functionalityData.m_Functionality;
				}
			}
		}
		return null;
	}

	public BaseItemFunctionality GetFirstUsableFunctionality()
	{
		if (m_ItemData != null)
		{
			for (int i = 0; i < m_ItemData.m_ItemFunctionalities.Count; i++)
			{
				ItemData.FunctionalityData functionalityData = m_ItemData.m_ItemFunctionalities[i];
				if (functionalityData != null && functionalityData.m_Functionality != null && functionalityData.m_Functionality.CanUse())
				{
					return functionalityData.m_Functionality;
				}
			}
		}
		return null;
	}

	public void GetAllUsableFunctionalities(List<BaseItemFunctionality> outputContainer)
	{
		outputContainer?.Clear();
		if (!(m_ItemData != null))
		{
			return;
		}
		for (int i = 0; i < m_ItemData.m_ItemFunctionalities.Count; i++)
		{
			ItemData.FunctionalityData functionalityData = m_ItemData.m_ItemFunctionalities[i];
			if (functionalityData != null && functionalityData.m_Functionality != null && functionalityData.m_Functionality.CanUse())
			{
				outputContainer.Add(functionalityData.m_Functionality);
			}
		}
	}

	public void Use()
	{
		if (m_ItemData == null)
		{
			return;
		}
		for (int i = 0; i < m_ItemData.m_ItemFunctionalities.Count; i++)
		{
			ItemData.FunctionalityData functionalityData = m_ItemData.m_ItemFunctionalities[i];
			if (functionalityData != null && functionalityData.m_Functionality != null && functionalityData.m_Functionality.CanUse(intendsOnUsingImmediately: true))
			{
				PositionOwner(functionalityData);
				if (functionalityData.m_Functionality.StartUsing(functionalityData.m_UseAnimation, functionalityData.m_UseTime))
				{
					m_FunctionalityToUpdate = functionalityData.m_Functionality;
				}
				break;
			}
		}
	}

	public void Use(BaseItemFunctionality.Functionality function)
	{
		if (m_ItemData == null)
		{
			return;
		}
		for (int i = 0; i < m_ItemData.m_ItemFunctionalities.Count; i++)
		{
			ItemData.FunctionalityData functionalityData = m_ItemData.m_ItemFunctionalities[i];
			if (functionalityData == null || !(functionalityData.m_Functionality != null) || functionalityData.m_Functionality.GetFunctionalityType() != function)
			{
				continue;
			}
			if (functionalityData.m_Functionality.CanUse(intendsOnUsingImmediately: true))
			{
				PositionOwner(functionalityData);
				if (functionalityData.m_Functionality.StartUsing(functionalityData.m_UseAnimation, functionalityData.m_UseTime))
				{
					m_FunctionalityToUpdate = functionalityData.m_Functionality;
				}
			}
			break;
		}
	}

	public void CancelUsing()
	{
		if (m_FunctionalityToUpdate != null && !m_FunctionalityToUpdate.IsCancelInProgress() && m_FunctionalityToUpdate.CancelUsing())
		{
			m_FunctionalityToUpdate = null;
		}
	}

	public bool IsInUse()
	{
		return m_FunctionalityToUpdate != null;
	}

	public bool CanBeSwitchedOut()
	{
		return !IsInUse() || m_ItemData.m_bCanSwitchInUse;
	}

	public void SetTrackableUIElementReporter(TrackableUIElementsReporter reporter)
	{
		m_TrackableElementsReporter = reporter;
		if (m_TrackableElementsReporter != null)
		{
			m_TrackableElementsReporter.SetProximityPriority(m_ProximityPriority, m_ProximityPenalty);
		}
	}

	public BaseItemFunctionality HasFunctionality(BaseItemFunctionality.Functionality function)
	{
		if (m_ItemData != null)
		{
			return m_ItemData.HasFunctionality(function);
		}
		return null;
	}

	private void PositionOwner(ItemData.FunctionalityData funcData)
	{
		if (m_Owner == null)
		{
			return;
		}
		bool flag = true;
		if (!m_Owner.m_bIsStandingOnDesk && funcData.m_Functionality.RequiresTargetting() && funcData.m_Functionality.RequiresPositioning() && FloorManager.GetInstance().GetTileCentrePosition(m_Owner.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, m_Owner.GetTargetTileRow(), m_Owner.GetTargetTileColumn(), out var worldPosition))
		{
			Directionx4 headAndBodyDirection = Direction.VectorToNearestDirectionx4(worldPosition - m_Owner.m_CachedCurrentPosition);
			m_Owner.SetFaceDirection(headAndBodyDirection);
			Vector2 useOffset = funcData.m_UseOffset;
			if (m_Owner.m_x4FacingDirection == Directionx4.Left)
			{
				useOffset.x = 0f - useOffset.x;
			}
			else if (m_Owner.m_x4FacingDirection == Directionx4.Down)
			{
				float x = useOffset.x;
				useOffset.x = 0f - useOffset.y;
				useOffset.y = 0f - x;
				flag = false;
			}
			else if (m_Owner.m_x4FacingDirection == Directionx4.Up)
			{
				float x2 = useOffset.x;
				useOffset.x = 0f - useOffset.y;
				useOffset.y = x2;
			}
			Vector3 vector = default(Vector3);
			vector.x = worldPosition.x + useOffset.x;
			vector.y = worldPosition.y + useOffset.y;
			vector.z = m_Owner.transform.position.z;
			m_Owner.transform.position = vector;
			m_Owner.m_CachedCurrentPosition = vector;
		}
		float num = m_Owner.GetZOffsetForCharacter();
		if (flag)
		{
			num += -0.2f;
		}
		m_Owner.SetUseItemZ(num);
	}

	public void DropItemInLevel(Character character, Vector3 position)
	{
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Drop_Item, AudioController.UI_Audio_GO);
		if (character != null)
		{
			m_NetView.RPC("RPC_MASTER_DropItemInLevel", NetTargets.MasterClient, m_NetView.viewID, character.m_NetView.viewID, position.x, position.y, position.z);
		}
		else
		{
			m_NetView.RPC("RPC_MASTER_DropItemInLevel", NetTargets.MasterClient, m_NetView.viewID, -1, position.x, position.y, position.z);
		}
	}

	[PunRPC]
	public void RPC_MASTER_DropItemInLevel(int itemViewID, int characterViewID, float x, float y, float z)
	{
		Vector3 vector = default(Vector3);
		vector.x = x;
		vector.y = y;
		vector.z = z;
		if (itemViewID == -1)
		{
			return;
		}
		Item item = T17NetView.Find<Item>(itemViewID);
		if (!(item != null))
		{
			return;
		}
		Vector3 nodePos = vector;
		if (characterViewID != -1)
		{
			LevelScript.GetInstance().DropItemInLevel_AllRPC(item);
			if (!NavMeshUtil.GetPositionOnNavMesh(vector, out nodePos))
			{
				nodePos = vector;
			}
			else
			{
				nodePos.z -= 0.01f;
			}
		}
		else
		{
			nodePos.z -= 0.01f;
			LevelScript.GetInstance().DropItemInLevel_AllRPC(item);
		}
		m_NetView.PostLevelLoadRPC("RPC_ItemDroppedInLevelResponse", NetTargets.All, itemViewID, characterViewID, nodePos.x, nodePos.y, nodePos.z);
	}

	public void AddToCullingSystem()
	{
		CullingObjectCollector.GetInstance().Runtime_AddToBucket(MeshRendererProp, bCheckForMaterialBlock: false, bAlsoFloorsAbove: true);
	}

	public void SetContrabandItemDropped(int characterViewID)
	{
		Transform transform = base.transform;
		m_DroppedByCharacterViewID = characterViewID;
		if (characterViewID != -1)
		{
			Character character = T17NetView.Find<Character>(characterViewID);
			if (character != null && m_ItemData != null && m_ItemData.IsContraband())
			{
				AIEventManager.GetInstance().SetContrabandItemDropped(base.transform, character, setActive: true);
			}
		}
	}

	[PunRPC]
	private void RPC_ItemDroppedInLevelResponse(int itemViewID, int characterViewID, float x, float y, float z)
	{
		Vector3 position = default(Vector3);
		position.x = x;
		position.y = y;
		position.z = z;
		if (itemViewID == -1)
		{
			return;
		}
		Item item = T17NetView.Find<Item>(itemViewID);
		if (!(item != null))
		{
			return;
		}
		Transform transform = item.transform;
		transform.position = position;
		if (MeshRendererProp != null)
		{
			MeshRendererProp.enabled = true;
			AddToCullingSystem();
		}
		m_DroppedByCharacterViewID = characterViewID;
		if (characterViewID != -1)
		{
			Character character = T17NetView.Find<Character>(characterViewID);
			if (character != null && item.m_ItemData != null && item.m_ItemData.IsContraband())
			{
				AIEventManager.GetInstance().SetContrabandItemDropped(base.transform, character, setActive: true);
			}
		}
	}

	public void ItemDropped()
	{
		if (TrackableUIElementReporter == null)
		{
			TrackableUIElementsReporter trackableUIElementReporter = base.gameObject.AddComponent<TrackableUIElementsReporter>();
			SetTrackableUIElementReporter(trackableUIElementReporter);
		}
		if (TrackableUIElementReporter != null)
		{
			string localized = string.Empty;
			Localization.Get(ItemName, out localized);
			TrackableUIElementReporter.SetDisplayName((!string.IsNullOrEmpty(localized)) ? localized : ItemName);
		}
		if ((bool)HasFunctionality(BaseItemFunctionality.Functionality.Ladder))
		{
			ClimbableObject component = base.gameObject.GetComponent<ClimbableObject>();
			if (component == null)
			{
				base.gameObject.AddComponent<ClimbableObject>();
				base.gameObject.layer = LayerMask.NameToLayer("DynamicMapObject");
				m_OldColliderSize = BoxColliderProp.size;
				if (BoxColliderProp != null)
				{
					BoxColliderProp.isTrigger = false;
					BoxColliderProp.size = Vector3.one;
					BoxColliderProp.center = new Vector3(0f, 0f, 0.1f);
					BoxColliderProp.enabled = true;
				}
			}
		}
		else if (BoxColliderProp != null)
		{
			BoxColliderProp.enabled = true;
		}
		m_bIsOnFloor = true;
	}

	public void ItemPickedUp()
	{
		if (MeshRendererProp != null)
		{
			if (CullingObjectCollector.GetInstance() != null)
			{
				CullingObjectCollector.GetInstance().Runtime_RemoveFromBucket(MeshRendererProp);
			}
			MeshRendererProp.enabled = false;
		}
		if (BoxColliderProp != null)
		{
			BoxColliderProp.enabled = false;
		}
		if (TrackableUIElementReporter != null)
		{
			Object.Destroy(TrackableUIElementReporter);
		}
		if ((bool)HasFunctionality(BaseItemFunctionality.Functionality.Ladder))
		{
			ClimbableObject component = base.gameObject.GetComponent<ClimbableObject>();
			if (component != null)
			{
				Object.Destroy(component);
			}
			if (BoxColliderProp != null)
			{
				BoxColliderProp.isTrigger = true;
				BoxColliderProp.size = m_OldColliderSize;
				BoxColliderProp.center = Vector3.zero;
				BoxColliderProp.enabled = false;
			}
			base.gameObject.layer = LayerMask.NameToLayer("Items");
		}
		AIEventManager.GetInstance().SetContrabandItemDropped(base.transform, null, setActive: false);
		m_bIsOnFloor = false;
		base.transform.position = m_SpawnPos;
	}

	public bool GetWorldPosition(out Vector3 worldPos)
	{
		worldPos = base.transform.position;
		if (m_bIsOnFloor)
		{
			worldPos = base.transform.position;
			return true;
		}
		if (m_Owner != null)
		{
			worldPos = m_Owner.m_Transform.position;
			return true;
		}
		if (m_ItemContainer != null)
		{
			worldPos = m_ItemContainer.transform.position;
			return true;
		}
		return false;
	}

	public void LocateAndDestroyItemRPC(RPC_CallContexts callContext)
	{
		if (m_Owner != null)
		{
			m_Owner.RemoveItemRPC(this, callContext, release: true);
		}
		if (m_ContainerViewID != 0)
		{
			m_ItemContainer.RemoveItemRPC(this, releaseToManager: true, callContext);
		}
	}

	public bool IsOnInmate()
	{
		if (m_Owner != null)
		{
			if (m_Owner.m_CharacterRole == CharacterRole.Inmate)
			{
				return true;
			}
			return false;
		}
		if (m_ContainerViewID == 0)
		{
			return false;
		}
		if (m_ItemContainer != null)
		{
			return m_ItemContainer.m_ContainerType == ItemContainer.ItemContainerType.Inmate;
		}
		return false;
	}

	public bool IsOnFloor()
	{
		return m_bIsOnFloor;
	}

	public Character GetCharacterHoldingItem()
	{
		if (m_Owner != null)
		{
			return m_Owner;
		}
		if (m_ItemContainer != null)
		{
			return m_ItemContainer.GetCharacterOwner();
		}
		return null;
	}

	public void ControlledLateUpdate()
	{
	}

	public void ControlledPreFixedUpdate()
	{
	}

	public void ControlledPreUpdate()
	{
	}

	public bool RequiresControlledUpdate()
	{
		return true;
	}

	public bool RequiresControlledFixedUpdate()
	{
		return false;
	}

	public bool RequiresControlledLateUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreFixedUpdate()
	{
		return false;
	}

	public bool IsPressAndHoldMultiUse()
	{
		if (m_ItemData != null)
		{
			for (int i = 0; i < m_ItemData.m_ItemFunctionalities.Count; i++)
			{
				if (m_ItemData.m_ItemFunctionalities[i].m_Functionality.m_PressAndHoldForMultipleUses)
				{
					return true;
				}
			}
		}
		return false;
	}
}
