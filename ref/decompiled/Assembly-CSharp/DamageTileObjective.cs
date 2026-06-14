using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Rotorz.Tile;
using UnityEngine;

[Serializable]
public class DamageTileObjective : BaseObjective
{
	public enum Mode
	{
		WallTile,
		FloorTile,
		Hole,
		HoleFromBelow
	}

	public Mode m_Mode;

	public int m_TargetHealth;

	public int m_TargetRow = -1;

	public int m_TargetColumn = -1;

	public int m_TargetFloor = -1;

	public bool m_bMustBeDuringRoutine;

	public Routines m_Routine = Routines.UNASSIGNED;

	public RoutineSubTypes m_RoutineSubtype;

	public bool m_bMustHaveCertainItems;

	public List<ItemData> m_RequiredItems = new List<ItemData>();

	public int m_ObjectiveToResetTo = -1;

	private DamagableTile m_DamagableTile;

	private Hole m_Hole;

	private int m_TargetHUDPin = -1;

	private bool m_bIsTargetTargetted;

	private bool m_bHasReachedRoutine;

	private bool m_bHasLeftRoutine;

	private ItemContainer m_PlayerItemContainer;

	private ItemContainer m_PlayerDeskItemContainer;

	private int m_TargetRowAdjusted = -1;

	private int m_TargetFloorAdjusted = -1;

	protected override void Child_Initialize()
	{
	}

	protected override void Child_PickAllTargets()
	{
		FloorManager instance = FloorManager.GetInstance();
		if (instance != null)
		{
			FindDamagableTile(instance);
			CalculateTargetFloor();
			if (m_Mode == Mode.WallTile || m_Mode == Mode.FloorTile)
			{
				if (m_DamagableTile == null)
				{
					m_ObjectiveStatus = ObjectiveStatus.Invalid;
				}
			}
			else if (m_Mode != Mode.Hole && m_Mode != Mode.HoleFromBelow)
			{
			}
		}
		else
		{
			m_ObjectiveStatus = ObjectiveStatus.Invalid;
		}
	}

	protected override void Child_RegisterTokens(ref ObjectiveTree objectiveTree)
	{
	}

	protected override void Child_Reset()
	{
		m_bHasReachedRoutine = false;
		m_bHasLeftRoutine = false;
	}

	protected override void Child_PreAction()
	{
		if (m_PlayerOwner != null)
		{
			m_PlayerItemContainer = m_PlayerOwner.m_ItemContainer;
			DeskInteraction myDesk = m_PlayerOwner.GetMyDesk();
			if (myDesk != null)
			{
				m_PlayerDeskItemContainer = myDesk.m_LinkedItemContainer;
			}
		}
	}

	protected override bool Child_EvaluateDependencies()
	{
		if (m_bMustBeDuringRoutine)
		{
			RoutineManager instance = RoutineManager.GetInstance();
			RoutinesData.Routine currentRoutine = instance.GetCurrentRoutine();
			if (currentRoutine != null && currentRoutine.m_BaseRoutineType == m_Routine && currentRoutine.m_SubRoutineType == m_RoutineSubtype)
			{
				m_bHasReachedRoutine = true;
			}
			else if (m_bHasReachedRoutine)
			{
				m_bHasLeftRoutine = true;
			}
		}
		switch (m_Mode)
		{
		case Mode.WallTile:
		case Mode.FloorTile:
			if (m_DamagableTile != null && m_DamagableTile.Health <= (float)m_TargetHealth)
			{
				return true;
			}
			break;
		case Mode.Hole:
		case Mode.HoleFromBelow:
			if (m_Hole != null && m_Hole.Health <= (float)m_TargetHealth)
			{
				return true;
			}
			break;
		}
		return false;
	}

	protected override bool Child_EvaluateStatus()
	{
		if ((m_Mode == Mode.Hole || m_Mode == Mode.HoleFromBelow) && m_Hole == null && m_TargetRow >= 0 && m_TargetColumn >= 0 && m_TargetFloor >= 0)
		{
			m_Hole = FloorManager.GetInstance().GetHole(m_TargetRow, m_TargetColumn, m_TargetFloor);
		}
		bool bIsTargetTargetted = m_bIsTargetTargetted;
		m_bIsTargetTargetted = m_PlayerOwner.GetTargetTileRow() == m_TargetRow;
		m_bIsTargetTargetted &= m_PlayerOwner.GetTargetTileColumn() == m_TargetColumn;
		m_bIsTargetTargetted &= m_PlayerOwner.GetFloorIndex() == m_TargetFloor;
		if (m_bIsTargetTargetted != bIsTargetTargetted)
		{
			Child_SetHUDArrow(m_bArrowOn);
		}
		return Child_EvaluateDependencies();
	}

	protected override void Child_PostAction()
	{
	}

	protected override void Child_SetHUDArrow(bool on)
	{
		if (!(base.PlayerOwner != null))
		{
			return;
		}
		if (on && !m_bIsTargetTargetted)
		{
			FloorManager instance = FloorManager.GetInstance();
			if (instance != null)
			{
				Vector3 worldPosition = Vector3.zero;
				if (instance.GetTileCentrePosition(m_TargetFloorAdjusted, GetTileSystemType(m_Mode), m_TargetRowAdjusted, m_TargetColumn, out worldPosition))
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
		if (on)
		{
			FloorManager instance = FloorManager.GetInstance();
			if (instance != null)
			{
				TileData tile = instance.GetTile(m_TargetFloorAdjusted, GetTileSystemType(m_Mode), m_TargetRowAdjusted, m_TargetColumn);
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

	protected override int Child_EvaluateResetCondition()
	{
		if (m_bHasLeftRoutine)
		{
			return m_ObjectiveToResetTo;
		}
		if (m_bMustHaveCertainItems && m_PlayerOwner != null)
		{
			Item equippedItem = m_PlayerOwner.GetEquippedItem();
			ItemData itemData = null;
			if (equippedItem != null)
			{
				itemData = equippedItem.m_ItemData;
			}
			bool flag = false;
			int i = 0;
			for (int count = m_RequiredItems.Count; i < count; i++)
			{
				if (!(m_RequiredItems[i] == null) && ((itemData != null && itemData.m_ItemDataID == m_RequiredItems[i].m_ItemDataID) || (m_PlayerItemContainer != null && m_PlayerItemContainer.HasItem(m_RequiredItems[i].m_ItemDataID, lookIntoHidden: true) > 0) || (m_PlayerDeskItemContainer != null && m_PlayerDeskItemContainer.HasItem(m_RequiredItems[i].m_ItemDataID, lookIntoHidden: true) > 0)))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return m_ObjectiveToResetTo;
			}
		}
		return -1;
	}

	private FloorManager.TileSystem_Type GetTileSystemType(Mode mode)
	{
		FloorManager.TileSystem_Type result = FloorManager.TileSystem_Type.TileSystem_Ground;
		switch (mode)
		{
		case Mode.WallTile:
			result = FloorManager.TileSystem_Type.TileSystem_Wall;
			break;
		case Mode.FloorTile:
		case Mode.Hole:
		case Mode.HoleFromBelow:
			result = FloorManager.TileSystem_Type.TileSystem_Ground;
			break;
		}
		return result;
	}

	private void CalculateTargetFloor()
	{
		if (m_TargetFloor > 0 && m_TargetColumn > 0 && m_Mode == Mode.HoleFromBelow)
		{
			FloorManager instance = FloorManager.GetInstance();
			if (!(instance != null))
			{
				return;
			}
			FloorManager.Floor floor = instance.FindFloorbyIndex(m_TargetFloor);
			if (floor != null)
			{
				floor = instance.DownAFloor(floor);
				if (floor != null)
				{
					m_TargetFloorAdjusted = floor.m_FloorIndex;
					m_TargetRowAdjusted = m_TargetRow + (m_TargetFloor - m_TargetFloorAdjusted);
				}
			}
		}
		else
		{
			m_TargetFloorAdjusted = m_TargetFloor;
			m_TargetRowAdjusted = m_TargetRow;
		}
	}

	private void FindDamagableTile(FloorManager floorManager)
	{
		if (floorManager != null && m_TargetRow >= 0 && m_TargetColumn >= 0 && m_TargetFloor >= 0)
		{
			TileData tile = floorManager.GetTile(m_TargetFloor, GetTileSystemType(m_Mode), m_TargetRow, m_TargetColumn);
			if (tile != null && tile.gameObject != null)
			{
				m_DamagableTile = tile.gameObject.GetComponent<DamagableTile>();
			}
			m_Hole = floorManager.GetHole(m_TargetRow, m_TargetColumn, m_TargetFloor);
		}
	}

	protected override string Child_Save(JObject baseObj, bool ingameSave)
	{
		if (ingameSave)
		{
			baseObj.Add(new JProperty("RountineReached", m_bHasReachedRoutine));
			baseObj.Add(new JProperty("RountineLeft", m_bHasLeftRoutine));
		}
		baseObj.Add(new JProperty("Mode", (int)m_Mode));
		baseObj.Add(new JProperty("TargetHealth", m_TargetHealth));
		baseObj.Add(new JProperty("TargetRow", m_TargetRow));
		baseObj.Add(new JProperty("TargetColumn", m_TargetColumn));
		baseObj.Add(new JProperty("TargetFloor", m_TargetFloor));
		baseObj.Add(new JProperty("MustBeDuringRoutine", m_bMustBeDuringRoutine));
		baseObj.Add(new JProperty("Routine", m_Routine));
		baseObj.Add(new JProperty("RoutineSubtype", m_RoutineSubtype));
		baseObj.Add(new JProperty("MustHaveCertainItems", m_bMustHaveCertainItems));
		JProperty jProperty = new JProperty("RequiredItems");
		JArray jArray = new JArray();
		for (int i = 0; i < m_RequiredItems.Count; i++)
		{
			jArray.Add(m_RequiredItems[i].m_ItemDataID);
		}
		jProperty.Add(jArray);
		baseObj.Add(jProperty);
		baseObj.Add(new JProperty("ObjectiveToResetTo", m_ObjectiveToResetTo));
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject baseObj, bool ingameLoad)
	{
		if (ingameLoad)
		{
			JProperty jProperty = baseObj.Property("RountineReached");
			if (jProperty != null)
			{
				m_bHasReachedRoutine = (bool)jProperty.Value;
			}
			JProperty jProperty2 = baseObj.Property("RountineLeft");
			if (jProperty2 != null)
			{
				m_bHasLeftRoutine = (bool)jProperty2.Value;
			}
		}
		JProperty jProperty3 = baseObj.Property("Mode");
		if (jProperty3 != null)
		{
			int mode = (int)jProperty3.Value;
			m_Mode = (Mode)mode;
		}
		JProperty jProperty4 = baseObj.Property("TargetHealth");
		if (jProperty4 != null)
		{
			m_TargetHealth = (int)jProperty4.Value;
		}
		JProperty jProperty5 = baseObj.Property("TargetRow");
		if (jProperty5 != null)
		{
			m_TargetRow = (int)jProperty5.Value;
		}
		JProperty jProperty6 = baseObj.Property("TargetColumn");
		if (jProperty6 != null)
		{
			m_TargetColumn = (int)jProperty6.Value;
		}
		JProperty jProperty7 = baseObj.Property("TargetFloor");
		if (jProperty7 != null)
		{
			m_TargetFloor = (int)jProperty7.Value;
		}
		JProperty jProperty8 = baseObj.Property("MustBeDuringRoutine");
		if (jProperty8 != null)
		{
			m_bMustBeDuringRoutine = (bool)jProperty8.Value;
		}
		JProperty jProperty9 = baseObj.Property("ObjectiveToResetTo");
		if (jProperty9 != null)
		{
			m_ObjectiveToResetTo = (int)jProperty9.Value;
		}
		JProperty jProperty10 = baseObj.Property("Routine");
		if (jProperty10 != null)
		{
			int routine = (int)jProperty10.Value;
			m_Routine = (Routines)routine;
		}
		JProperty jProperty11 = baseObj.Property("RoutineSubtype");
		if (jProperty11 != null)
		{
			int num = (int)jProperty11.Value;
			m_RoutineSubtype = (RoutineSubTypes)num;
		}
		JProperty jProperty12 = baseObj.Property("MustHaveCertainItems");
		if (jProperty12 != null)
		{
			m_bMustHaveCertainItems = (bool)jProperty12.Value;
		}
		JProperty jProperty13 = baseObj.Property("RequiredItems");
		if (jProperty13 != null && jProperty13.Value.Type == JTokenType.Array)
		{
			m_RequiredItems.Clear();
			JArray source = (JArray)jProperty13.Value;
			List<int> requiredItemIds = source.Select((JToken c) => (int)c).ToList();
			List<ItemData> source2 = Resources.LoadAll<ItemData>("Prefabs/Items").ToList();
			int count = requiredItemIds.Count;
			for (int i = 0; i < count; i++)
			{
				m_RequiredItems.Add(source2.FirstOrDefault((ItemData id) => id.m_ItemDataID == requiredItemIds[i]));
			}
		}
		FindDamagableTile(FloorManager.GetInstance());
		CalculateTargetFloor();
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.DamageTileObjective;
	}
}
