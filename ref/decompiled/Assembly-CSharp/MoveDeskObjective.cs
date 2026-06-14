using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class MoveDeskObjective : BaseObjective
{
	public ObjectiveSceneElement m_DeskSceneRef;

	public int m_TargetRow = -1;

	public int m_TargetColumn = -1;

	public int m_TargetFloor = -1;

	private CarryObjectInteraction m_TargetDesk;

	private bool m_bIsPlayerCarryingDesk;

	private bool m_bShowObjectivePin;

	private bool m_bShowObjectiveArrow;

	private int m_TargetPinID = -1;

	protected override void Child_PickAllTargets()
	{
		if (m_DeskSceneRef != null && m_DeskSceneRef.m_LinksTo == ObjectiveSceneElement.ObjectiveSceneElementType.ItemContainer)
		{
			m_TargetDesk = m_DeskSceneRef.GetComponent<CarryObjectInteraction>();
			if (m_TargetDesk == null)
			{
				m_ObjectiveStatus = ObjectiveStatus.Invalid;
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
	}

	protected override void Child_Initialize()
	{
	}

	protected override void Child_PreAction()
	{
	}

	protected override bool Child_EvaluateDependencies()
	{
		if (m_TargetDesk != null && !m_TargetDesk.IsPickedUp)
		{
			Vector3 position = m_TargetDesk.transform.position;
			FloorManager instance = FloorManager.GetInstance();
			if (instance != null)
			{
				int num = instance.FindFloorIndexAtZ(position.z);
				if (num >= 0 && num == m_TargetFloor)
				{
					int row = -1;
					int column = -1;
					if (instance.GetTileGridPoint(num, FloorManager.TileSystem_Type.TileSystem_Ground, position, out row, out column))
					{
						return row == m_TargetRow && column == m_TargetColumn;
					}
				}
			}
		}
		return false;
	}

	protected override bool Child_EvaluateStatus()
	{
		bool flag = false;
		if (m_PlayerOwner.m_bIsCarryingObject)
		{
			InteractiveObject interactiveObject = m_PlayerOwner.GetInteractiveObject();
			if (interactiveObject != null && interactiveObject.GetInteractionClassType() == InteractiveObject.InteractionType.PortableInteractiveObject)
			{
				flag = interactiveObject.gameObject == m_TargetDesk.gameObject;
			}
		}
		if (flag != m_bIsPlayerCarryingDesk)
		{
			m_bIsPlayerCarryingDesk = flag;
			UpdateHUDPin();
			UpdateHUDArrow();
		}
		return Child_EvaluateDependencies();
	}

	protected override void Child_PostAction()
	{
	}

	protected override void Child_SetHUDPins(bool on)
	{
		if (on)
		{
			m_bShowObjectivePin = true;
			UpdateHUDPin();
		}
		else if (m_bShowObjectivePin)
		{
			if (m_TargetPinID != -1)
			{
				PinManager.GetInstance().RemovePin(m_TargetPinID);
				m_TargetPinID = -1;
			}
			m_bShowObjectivePin = false;
		}
	}

	private void UpdateHUDPin()
	{
		if (!m_bShowObjectivePin)
		{
			return;
		}
		if (!m_bIsPlayerCarryingDesk)
		{
			if (m_TargetPinID == -1 && m_TargetDesk != null)
			{
				m_TargetPinID = PinManager.GetInstance().CreatePin(bForMainMap: true, bForMiniMap: true, m_TargetDesk.gameObject, null, bUpdatePosition: true, FloorManager.GetInstance().FindFloorAtZ(m_TargetDesk.transform.position.z), new Player[1] { m_PlayerOwner }, PinManager.Pin.PinFilterType.Objectives, edgable: true, floorTrackable: true, directional: false, animation: ObjectiveManager.GetInstance().m_QuestTargetAnimation, toolTipTag: string.Empty);
			}
			if (m_TargetDesk != null)
			{
				PinManager.GetInstance().UpdatePinTarget(m_TargetPinID, m_TargetDesk.gameObject);
			}
		}
		else if (m_TargetPinID != -1)
		{
			PinManager.GetInstance().RemovePin(m_TargetPinID);
			m_TargetPinID = -1;
		}
	}

	protected override void Child_SetHUDArrow(bool on)
	{
		if (on)
		{
			m_bShowObjectiveArrow = true;
			UpdateHUDArrow();
		}
		else if (m_bShowObjectiveArrow)
		{
			m_PlayerOwner.CancelObjectiveArrow();
			m_bShowObjectiveArrow = false;
		}
	}

	private void UpdateHUDArrow()
	{
		if (!m_bShowObjectiveArrow)
		{
			return;
		}
		if (!m_bIsPlayerCarryingDesk)
		{
			if (m_TargetDesk != null)
			{
				m_PlayerOwner.SetObjectiveArrowTarget(m_TargetDesk.m_NetViewID);
			}
			return;
		}
		Vector3 worldPosition = Vector3.zero;
		FloorManager instance = FloorManager.GetInstance();
		if (instance != null && instance.GetTileCentrePosition(m_TargetFloor, FloorManager.TileSystem_Type.TileSystem_Ground, m_TargetRow, m_TargetColumn, out worldPosition))
		{
			m_PlayerOwner.SetObjectiveArrowTarget(worldPosition);
		}
	}

	protected override string Child_Save(JObject baseObj, bool ingameSave)
	{
		if (ingameSave)
		{
		}
		if (m_DeskSceneRef != null)
		{
			baseObj.Add(new JProperty("DeskTargetRef", m_DeskSceneRef.m_ObjectiveElementID));
			baseObj.Add(new JProperty("DeskTargetRef_Scene", m_DeskSceneRef.m_UsedInScene));
		}
		baseObj.Add(new JProperty("TargetRow", m_TargetRow));
		baseObj.Add(new JProperty("TargetColumn", m_TargetColumn));
		baseObj.Add(new JProperty("TargetFloor", m_TargetFloor));
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject json, bool ingameLoad)
	{
		if (ingameLoad)
		{
		}
		JProperty jProperty = json.Property("DeskTargetRef");
		if (jProperty != null)
		{
			string text = (string)json.Property("DeskTargetRef_Scene").Value;
			int id = (int)jProperty.Value;
			if ((Application.isPlaying || !(text != SceneManagerHelper.ActiveSceneName)) && Application.isPlaying)
			{
				m_DeskSceneRef = ObjectiveSceneElement.FindSceneReference(id);
			}
		}
		JProperty jProperty2 = json.Property("TargetRow");
		if (jProperty2 != null)
		{
			m_TargetRow = (int)jProperty2.Value;
		}
		JProperty jProperty3 = json.Property("TargetColumn");
		if (jProperty3 != null)
		{
			m_TargetColumn = (int)jProperty3.Value;
		}
		JProperty jProperty4 = json.Property("TargetFloor");
		if (jProperty4 != null)
		{
			m_TargetFloor = (int)jProperty4.Value;
		}
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.MoveDeskObjective;
	}
}
