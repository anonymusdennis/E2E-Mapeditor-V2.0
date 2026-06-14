using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Rotorz.Tile;
using UnityEngine;

[Serializable]
public class UseItemObjective : BaseObjective
{
	public enum Mode
	{
		Anywhere,
		WallTile,
		FloorTile,
		FloorTileFromBelow,
		PlayerBed
	}

	public Mode m_Mode;

	public ItemData m_ItemType;

	public BaseItemFunctionality.Functionality m_DesiredFunctionality = BaseItemFunctionality.Functionality.UNUSED_01;

	public int m_TargetRow = -1;

	public int m_TargetColumn = -1;

	public int m_TargetFloor = -1;

	public bool m_bMustHaveItemToBeUsed;

	public int m_ObjectiveToResetTo = -1;

	private bool m_bItemUsed;

	private Item m_PlayerItem;

	private int m_UseLocationRow = -1;

	private int m_UseLocationColumn = -1;

	private int m_UseLocationFloor = -1;

	private int m_TargetFloorAdjusted = -1;

	private FloorManager.TileSystem_Type m_TileSystemType;

	private int m_TargetHUDPin = -1;

	private bool m_bIsTargetTargetted;

	private ItemContainer m_PlayerItemContainer;

	private ItemContainer m_PlayerDeskItemContainer;

	private BedInteraction m_PlayerBed;

	private CellBed_ItemInteraction m_PlayerCellBedItemInteraction;

	protected override void Child_Initialize()
	{
	}

	protected override void Child_PickAllTargets()
	{
		m_TileSystemType = GetTileSystemForMode(m_Mode);
		if (m_ItemType != null)
		{
			BaseItemFunctionality baseItemFunctionality = m_ItemType.HasFunctionality(m_DesiredFunctionality);
			if (baseItemFunctionality == null)
			{
				m_ObjectiveStatus = ObjectiveStatus.Invalid;
			}
		}
		FloorManager instance = FloorManager.GetInstance();
		if (instance != null)
		{
			bool flag = false;
			if (m_TargetRow >= 0 && m_TargetColumn >= 0 && m_TargetFloor >= 0)
			{
				flag = instance.CheckTileExists(m_TargetFloor, m_TileSystemType, m_TargetRow, m_TargetColumn, bIncludeInactive: true);
			}
			if (!flag)
			{
				m_ObjectiveStatus = ObjectiveStatus.Invalid;
			}
		}
		else
		{
			m_ObjectiveStatus = ObjectiveStatus.Invalid;
		}
		if (m_Mode == Mode.FloorTileFromBelow && m_ObjectiveStatus != ObjectiveStatus.Invalid)
		{
			FloorManager.Floor floor = instance.FindFloorbyIndex(m_TargetFloor);
			if (floor != null)
			{
				floor = instance.DownAFloor(floor);
				if (floor != null)
				{
					m_TargetFloorAdjusted = floor.m_FloorIndex;
				}
			}
		}
		else
		{
			m_TargetFloorAdjusted = m_TargetFloor;
		}
	}

	protected override void Child_RegisterTokens(ref ObjectiveTree objectiveTree)
	{
	}

	protected override void Child_Reset()
	{
	}

	protected override void Child_PreAction()
	{
		if (base.PlayerOwner == null)
		{
			return;
		}
		Player playerOwner = m_PlayerOwner;
		playerOwner.OnEquipedItemChanged = (Character.CharacterEvent)Delegate.Remove(playerOwner.OnEquipedItemChanged, new Character.CharacterEvent(PlayerEquippedItemChanged));
		Player playerOwner2 = m_PlayerOwner;
		playerOwner2.OnEquipedItemChanged = (Character.CharacterEvent)Delegate.Combine(playerOwner2.OnEquipedItemChanged, new Character.CharacterEvent(PlayerEquippedItemChanged));
		PlayerEquippedItemChanged();
		if (m_DesiredFunctionality == BaseItemFunctionality.Functionality.BraceTunnel)
		{
			FloorManager instance = FloorManager.GetInstance();
			if (instance != null && instance.GetTunnelBrace(m_TargetRow, m_TargetColumn, m_TargetFloor) != null)
			{
				m_bItemUsed = true;
			}
		}
		m_PlayerItemContainer = m_PlayerOwner.m_ItemContainer;
		DeskInteraction myDesk = m_PlayerOwner.GetMyDesk();
		if (myDesk != null)
		{
			m_PlayerDeskItemContainer = myDesk.m_LinkedItemContainer;
		}
		if (m_Mode != Mode.PlayerBed)
		{
			return;
		}
		RoomBlob myCell = m_PlayerOwner.GetMyCell();
		if (!(myCell != null))
		{
			return;
		}
		RoomBlob_Cell roomBlobData = myCell.GetRoomBlobData<RoomBlob_Cell>();
		if (roomBlobData != null)
		{
			SpawnPoint spawnPointForCharacter = roomBlobData.GetSpawnPointForCharacter(m_PlayerOwner);
			if (spawnPointForCharacter != null && spawnPointForCharacter.m_AttachedBed != null)
			{
				m_PlayerBed = spawnPointForCharacter.m_AttachedBed;
				m_PlayerCellBedItemInteraction = m_PlayerBed.GetComponent<CellBed_ItemInteraction>();
			}
		}
	}

	protected override bool Child_EvaluateDependencies()
	{
		return m_bItemUsed;
	}

	protected override bool Child_EvaluateStatus()
	{
		bool bIsTargetTargetted = m_bIsTargetTargetted;
		m_bIsTargetTargetted = m_PlayerOwner.GetTargetTileRow() == m_TargetRow;
		m_bIsTargetTargetted &= m_PlayerOwner.GetTargetTileColumn() == m_TargetColumn;
		m_bIsTargetTargetted &= m_PlayerOwner.GetFloorIndex() == m_TargetFloorAdjusted;
		if (m_bIsTargetTargetted != bIsTargetTargetted)
		{
			Child_SetHUDArrow(m_bArrowOn);
		}
		return Child_EvaluateDependencies();
	}

	protected override void Child_PostAction()
	{
		if (m_PlayerItem != null)
		{
			BaseItemFunctionality baseItemFunctionality = m_PlayerItem.HasFunctionality(m_DesiredFunctionality);
			if (baseItemFunctionality != null)
			{
				baseItemFunctionality.OnStartOfUse = (BaseItemFunctionality.ItemFuncEvent)Delegate.Remove(baseItemFunctionality.OnStartOfUse, new BaseItemFunctionality.ItemFuncEvent(FunctionalityStarted));
				baseItemFunctionality.OnEndOfUse = (BaseItemFunctionality.ItemFuncEvent)Delegate.Remove(baseItemFunctionality.OnEndOfUse, new BaseItemFunctionality.ItemFuncEvent(FunctionalityUsed));
			}
			m_PlayerItem = null;
		}
		Player playerOwner = m_PlayerOwner;
		playerOwner.OnEquipedItemChanged = (Character.CharacterEvent)Delegate.Remove(playerOwner.OnEquipedItemChanged, new Character.CharacterEvent(PlayerEquippedItemChanged));
	}

	private void PlayerEquippedItemChanged()
	{
		if (m_PlayerOwner == null)
		{
			return;
		}
		Item equippedItem = m_PlayerOwner.GetEquippedItem();
		if (!(equippedItem != m_PlayerItem))
		{
			return;
		}
		if (m_PlayerItem != null)
		{
			BaseItemFunctionality baseItemFunctionality = m_PlayerItem.HasFunctionality(m_DesiredFunctionality);
			if (baseItemFunctionality != null)
			{
				baseItemFunctionality.OnStartOfUse = (BaseItemFunctionality.ItemFuncEvent)Delegate.Remove(baseItemFunctionality.OnStartOfUse, new BaseItemFunctionality.ItemFuncEvent(FunctionalityStarted));
				baseItemFunctionality.OnEndOfUse = (BaseItemFunctionality.ItemFuncEvent)Delegate.Remove(baseItemFunctionality.OnEndOfUse, new BaseItemFunctionality.ItemFuncEvent(FunctionalityUsed));
			}
		}
		if (equippedItem != null && (m_ItemType == null || equippedItem.ItemDataID == m_ItemType.m_ItemDataID))
		{
			BaseItemFunctionality baseItemFunctionality2 = equippedItem.HasFunctionality(m_DesiredFunctionality);
			if (baseItemFunctionality2 != null)
			{
				baseItemFunctionality2.OnStartOfUse = (BaseItemFunctionality.ItemFuncEvent)Delegate.Remove(baseItemFunctionality2.OnStartOfUse, new BaseItemFunctionality.ItemFuncEvent(FunctionalityStarted));
				baseItemFunctionality2.OnStartOfUse = (BaseItemFunctionality.ItemFuncEvent)Delegate.Combine(baseItemFunctionality2.OnStartOfUse, new BaseItemFunctionality.ItemFuncEvent(FunctionalityStarted));
				baseItemFunctionality2.OnEndOfUse = (BaseItemFunctionality.ItemFuncEvent)Delegate.Remove(baseItemFunctionality2.OnEndOfUse, new BaseItemFunctionality.ItemFuncEvent(FunctionalityUsed));
				baseItemFunctionality2.OnEndOfUse = (BaseItemFunctionality.ItemFuncEvent)Delegate.Combine(baseItemFunctionality2.OnEndOfUse, new BaseItemFunctionality.ItemFuncEvent(FunctionalityUsed));
			}
		}
		m_PlayerItem = equippedItem;
	}

	private void FunctionalityStarted()
	{
		if (m_Mode != 0 && m_PlayerOwner != null)
		{
			m_UseLocationRow = m_PlayerOwner.GetTargetTileRow();
			m_UseLocationColumn = m_PlayerOwner.GetTargetTileColumn();
			m_UseLocationFloor = m_PlayerOwner.GetFloorIndex();
		}
	}

	private void FunctionalityUsed()
	{
		switch (m_Mode)
		{
		case Mode.WallTile:
		case Mode.FloorTile:
		case Mode.FloorTileFromBelow:
			if (m_TargetRow == m_UseLocationRow && m_TargetColumn == m_UseLocationColumn && m_TargetFloorAdjusted == m_UseLocationFloor)
			{
				m_bItemUsed = true;
			}
			break;
		case Mode.PlayerBed:
			if (m_PlayerBed != null && m_PlayerItem != null && m_DesiredFunctionality == BaseItemFunctionality.Functionality.ItemTransfer)
			{
				TransferItemFunctionality transferItemFunctionality = (TransferItemFunctionality)m_PlayerItem.HasFunctionality(m_DesiredFunctionality);
				if (transferItemFunctionality != null && m_PlayerCellBedItemInteraction != null && m_PlayerCellBedItemInteraction.HasRealPillowAndSheet())
				{
					m_bItemUsed = transferItemFunctionality.IsItemInteractionMyTarget(m_PlayerCellBedItemInteraction);
				}
			}
			break;
		case Mode.Anywhere:
			m_bItemUsed = true;
			break;
		}
	}

	private FloorManager.TileSystem_Type GetTileSystemForMode(Mode mode)
	{
		FloorManager.TileSystem_Type result = FloorManager.TileSystem_Type.TileSystem_Ground;
		switch (m_Mode)
		{
		case Mode.WallTile:
			m_TileSystemType = FloorManager.TileSystem_Type.TileSystem_Wall;
			break;
		case Mode.FloorTile:
		case Mode.FloorTileFromBelow:
			m_TileSystemType = FloorManager.TileSystem_Type.TileSystem_Ground;
			break;
		}
		return result;
	}

	protected override int Child_EvaluateResetCondition()
	{
		if (m_bMustHaveItemToBeUsed && m_PlayerOwner != null)
		{
			Item equippedItem = m_PlayerOwner.GetEquippedItem();
			ItemData itemData = null;
			if (equippedItem != null)
			{
				itemData = equippedItem.m_ItemData;
			}
			int itemDataID = m_ItemType.m_ItemDataID;
			if ((itemData == null || itemData.m_ItemDataID != itemDataID) && (m_PlayerItemContainer == null || m_PlayerItemContainer.HasItem(itemDataID, lookIntoHidden: true) == 0) && (m_PlayerDeskItemContainer == null || m_PlayerDeskItemContainer.HasItem(itemDataID, lookIntoHidden: true) == 0))
			{
				return m_ObjectiveToResetTo;
			}
		}
		return -1;
	}

	protected override void Child_SetHUDArrow(bool on)
	{
		if (!(base.PlayerOwner != null))
		{
			return;
		}
		if (on && m_Mode != 0 && !m_bIsTargetTargetted)
		{
			if (m_Mode == Mode.PlayerBed && m_PlayerBed != null)
			{
				base.PlayerOwner.SetObjectiveArrowTarget(m_PlayerBed.m_NetViewID);
				return;
			}
			FloorManager instance = FloorManager.GetInstance();
			if (instance != null)
			{
				Vector3 worldPosition = Vector3.zero;
				if (instance.GetTileCentrePosition(m_TargetFloorAdjusted, m_TileSystemType, m_TargetRow, m_TargetColumn, out worldPosition))
				{
					base.PlayerOwner.SetObjectiveArrowTarget(worldPosition);
				}
			}
		}
		else
		{
			base.PlayerOwner.CancelObjectiveArrow();
		}
	}

	protected override void Child_SetHUDPins(bool on)
	{
		if (on && m_Mode != 0)
		{
			if (m_Mode == Mode.PlayerBed && m_PlayerBed != null)
			{
				m_TargetHUDPin = PinManager.GetInstance().CreatePin(bForMainMap: true, bForMiniMap: true, m_PlayerBed.gameObject, null, bUpdatePosition: true, FloorManager.GetInstance().FindFloorAtZ(m_PlayerBed.transform.position.z), new Player[1] { m_PlayerOwner }, PinManager.Pin.PinFilterType.Objectives, edgable: true, floorTrackable: true, directional: false, animation: ObjectiveManager.GetInstance().m_QuestTargetAnimation, toolTipTag: string.Empty);
				return;
			}
			FloorManager instance = FloorManager.GetInstance();
			if (instance != null)
			{
				TileData tile = instance.GetTile(m_TargetFloorAdjusted, m_TileSystemType, m_TargetRow, m_TargetColumn);
				if (tile != null)
				{
					m_TargetHUDPin = PinManager.GetInstance().CreatePin(bForMainMap: true, bForMiniMap: true, tile.gameObject, null, bUpdatePosition: false, instance.FindFloorbyIndex(m_TargetFloorAdjusted), new Player[1] { m_PlayerOwner }, PinManager.Pin.PinFilterType.Objectives, edgable: true, floorTrackable: true, directional: false, animation: ObjectiveManager.GetInstance().m_QuestTargetAnimation, toolTipTag: string.Empty);
				}
			}
		}
		else
		{
			PinManager.GetInstance().RemovePin(m_TargetHUDPin);
			m_TargetHUDPin = -1;
		}
	}

	protected override string Child_Save(JObject baseObj, bool ingameSave)
	{
		if (ingameSave)
		{
			baseObj.Add(new JProperty("ItemUsed", m_bItemUsed));
		}
		baseObj.Add(new JProperty("Mode", (int)m_Mode));
		baseObj.Add(new JProperty("DesiredFunctionality", (int)m_DesiredFunctionality));
		baseObj.Add(new JProperty("TargetRow", m_TargetRow));
		baseObj.Add(new JProperty("TargetColumn", m_TargetColumn));
		baseObj.Add(new JProperty("TargetFloor", m_TargetFloor));
		baseObj.Add(new JProperty("MustHaveItemToBeUsed", m_bMustHaveItemToBeUsed));
		baseObj.Add(new JProperty("ObjectiveToResetTo", m_ObjectiveToResetTo));
		if (m_ItemType != null)
		{
			baseObj.Add(new JProperty("ItemType", m_ItemType.m_ItemDataID));
		}
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject baseObj, bool ingameLoad)
	{
		JProperty jProperty = baseObj.Property("Mode");
		if (jProperty != null)
		{
			int mode = (int)jProperty.Value;
			m_Mode = (Mode)mode;
		}
		JProperty jProperty2 = baseObj.Property("DesiredFunctionality");
		if (jProperty2 != null)
		{
			int desiredFunctionality = (int)jProperty2.Value;
			m_DesiredFunctionality = (BaseItemFunctionality.Functionality)desiredFunctionality;
		}
		JProperty jProperty3 = baseObj.Property("TargetRow");
		if (jProperty3 != null)
		{
			m_TargetRow = (int)jProperty3.Value;
		}
		JProperty jProperty4 = baseObj.Property("TargetColumn");
		if (jProperty4 != null)
		{
			m_TargetColumn = (int)jProperty4.Value;
		}
		JProperty jProperty5 = baseObj.Property("TargetFloor");
		if (jProperty5 != null)
		{
			m_TargetFloor = (int)jProperty5.Value;
		}
		JProperty jProperty6 = baseObj.Property("ItemType");
		if (jProperty6 != null)
		{
			int itemID = (int)jProperty6.Value;
			m_ItemType = Resources.LoadAll<ItemData>("Prefabs/Items").ToList().FirstOrDefault((ItemData id) => id.m_ItemDataID == itemID);
		}
		JProperty jProperty7 = baseObj.Property("MustHaveItemToBeUsed");
		if (jProperty7 != null)
		{
			m_bMustHaveItemToBeUsed = (bool)jProperty7.Value;
		}
		JProperty jProperty8 = baseObj.Property("ObjectiveToResetTo");
		if (jProperty8 != null)
		{
			m_ObjectiveToResetTo = (int)jProperty8.Value;
		}
		if (!ingameLoad)
		{
			return;
		}
		JProperty jProperty9 = baseObj.Property("ItemUsed");
		if (jProperty9 != null)
		{
			m_bItemUsed = (bool)jProperty9.Value;
		}
		if (m_Mode == Mode.FloorTileFromBelow)
		{
			FloorManager instance = FloorManager.GetInstance();
			FloorManager.Floor floor = instance.FindFloorbyIndex(m_TargetFloor);
			if (floor != null)
			{
				floor = instance.DownAFloor(floor);
				if (floor != null)
				{
					m_TargetFloorAdjusted = floor.m_FloorIndex;
				}
			}
		}
		else
		{
			m_TargetFloorAdjusted = m_TargetFloor;
		}
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.UseItemObjective;
	}
}
