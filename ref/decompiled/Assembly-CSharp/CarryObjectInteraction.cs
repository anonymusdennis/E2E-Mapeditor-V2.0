using System;
using System.Collections.Generic;
using System.Linq;
using AUTOGEN_T17Wwise_Enums;
using SaveHelpers;
using UnityEngine;

public class CarryObjectInteraction : InteractiveObject
{
	public enum AI_Decorations
	{
		Unassigned = -1,
		Desk,
		Job
	}

	[Serializable]
	public class NetSaveData
	{
		public List<ulong> m_Data;
	}

	public Vector3 m_CarryOffset = new Vector3(0f, 1.1f, -0.05f);

	public AI_Decorations m_Decoration;

	public bool m_bUpdateNetSaveData = true;

	private BoxCollider m_BoxCollider;

	private Transform m_Renderer;

	private Transform m_AttachPoint;

	private RoomBlob m_OurRoom;

	private ClimbableObject m_ClimableBehaviour;

	private bool m_IsPickedUp;

	private Rigidbody m_Rigidbody;

	public ulong m_CachedObjData;

	private static Dictionary<int, ulong> m_MovedObjects = new Dictionary<int, ulong>();

	private static NetSaveData m_NetSaveData = null;

	public bool IsPickedUp => m_IsPickedUp;

	protected override void Awake()
	{
		base.Awake();
		ReassignReferences();
	}

	protected override void OnDestroy()
	{
		if (m_MovedObjects != null)
		{
			m_MovedObjects.Clear();
		}
		m_NetSaveData = null;
		m_BoxCollider = null;
		m_Renderer = null;
		m_AttachPoint = null;
		m_OurRoom = null;
		m_ClimableBehaviour = null;
		m_Rigidbody = null;
		base.OnDestroy();
	}

	protected void ReassignReferences()
	{
		Animator componentInChildren = GetComponentInChildren<Animator>();
		if (componentInChildren != null)
		{
			m_Renderer = componentInChildren.transform;
		}
		else
		{
			MeshRenderer componentInChildren2 = GetComponentInChildren<MeshRenderer>();
			if (componentInChildren2 != null && componentInChildren2.gameObject != base.gameObject)
			{
				m_Renderer = componentInChildren2.transform;
			}
		}
		if (m_Renderer == null)
		{
		}
		m_ClimableBehaviour = GetComponent<ClimbableObject>();
		m_BoxCollider = GetComponent<BoxCollider>();
		m_Rigidbody = GetComponent<Rigidbody>();
	}

	public static void CleanUp()
	{
		if (m_MovedObjects != null)
		{
			m_MovedObjects.Clear();
		}
		m_NetSaveData = null;
	}

	public override InteractionType GetInteractionClassType()
	{
		return InteractionType.PortableInteractiveObject;
	}

	public override bool OverrideWalk()
	{
		return false;
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		return CanBePickedUp(localCharacter);
	}

	public override bool InteractionVisibility()
	{
		return CanBePickedUp(null);
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		CacheObjectsRoom();
		PickUpRPC(localCharacter);
	}

	public override void UpdateInteraction()
	{
		base.UpdateInteraction();
		UpdateCarriedPosition();
	}

	public override void RequestStopInteraction(Character localCharacter)
	{
		bool flag = false;
		int floorIndex = localCharacter.CurrentFloor.m_FloorIndex;
		int num = 0;
		int num2 = 0;
		if (FloorManager.GetInstance().GetTileGridPoint(floorIndex, FloorManager.TileSystem_Type.TileSystem_Ground, localCharacter.transform.position, out var row, out var column))
		{
			switch (localCharacter.m_x4FacingDirection)
			{
			case Directionx4.Up:
				num = row - 1;
				num2 = column;
				break;
			case Directionx4.Left:
				num = row;
				num2 = column - 1;
				break;
			case Directionx4.Down:
				num = row + 1;
				num2 = column;
				break;
			case Directionx4.Right:
				num = row;
				num2 = column + 1;
				break;
			default:
				num = row;
				num2 = column;
				break;
			}
			if (FloorManager.GetInstance().CheckIsInBounds(floorIndex, FloorManager.TileSystem_Type.TileSystem_Ground, num, num2))
			{
				flag = true;
				if (FloorManager.GetInstance().CheckTileExists(floorIndex, FloorManager.TileSystem_Type.TileSystem_Wall, num, num2, bIncludeInactive: true))
				{
					flag = false;
				}
				if (flag && FloorManager.GetInstance().GetTileCentrePosition(floorIndex, FloorManager.TileSystem_Type.TileSystem_Ground, num, num2, out var worldPosition))
				{
					RoomBlob roomBlob = RoomManager.GetInstance().LookUpRoom(worldPosition, localCharacter.CurrentFloor);
					if (roomBlob != m_OurRoom)
					{
						flag = false;
					}
				}
				if (flag)
				{
					int deltaRow = num - row;
					int deltaColumn = num2 - column;
					if (!FloorManager.GetInstance().IsFloorAheadClear(localCharacter.CurrentFloor, row, column, deltaRow, deltaColumn, this))
					{
						flag = false;
					}
				}
			}
		}
		if (flag)
		{
			Character interactingCharacter = m_interactingCharacter;
			PutDownRPC(floorIndex, num, num2);
			m_interactingCharacter = interactingCharacter;
			base.RequestStopInteraction(localCharacter);
			m_interactingCharacter = null;
		}
		else if (localCharacter != null && !localCharacter.IsPlayer())
		{
			ForceStopInteraction(localCharacter);
		}
	}

	public override void ForceStopInteraction(Character localCharacter)
	{
		FloorManager.Floor currentFloor = localCharacter.CurrentFloor;
		Vector3 vector = localCharacter.transform.position;
		FloorManager instance = FloorManager.GetInstance();
		RoomBlob objectsRoom = GetObjectsRoom();
		bool flag = false;
		if (instance != null && objectsRoom != null)
		{
			List<Vector3> eightSurroundingLocations = NavMeshUtil.GetEightSurroundingLocations(vector);
			for (int i = 0; i < eightSurroundingLocations.Count; i++)
			{
				Vector3 vector2 = eightSurroundingLocations[i];
				RoomBlob room = GetRoom(vector2, currentFloor);
				if (room != null && room == objectsRoom && instance.IsFloorClear(currentFloor, vector2))
				{
					vector = vector2;
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			vector = SwagBagManager.GetInstance().FindCleanestPositionInRoom(currentFloor, vector, objectsRoom);
		}
		int row = 0;
		int column = 0;
		FloorManager.GetInstance().GetTileGridPoint(currentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, vector, out row, out column);
		Character interactingCharacter = m_interactingCharacter;
		PutDownRPC(currentFloor.m_FloorIndex, row, column);
		m_interactingCharacter = interactingCharacter;
		base.ForceStopInteraction(localCharacter);
		m_interactingCharacter = null;
	}

	private bool CanBePickedUp(Character localCharacter)
	{
		if (m_ClimableBehaviour != null && m_ClimableBehaviour.NumCharactersOnUs != 0)
		{
			return false;
		}
		if (localCharacter != null)
		{
			RoomBlob objectsRoom = GetObjectsRoom();
			RoomBlob charactersRoom = GetCharactersRoom(localCharacter);
			if (objectsRoom != null && charactersRoom != null && objectsRoom != charactersRoom)
			{
				return false;
			}
		}
		return true;
	}

	private void CacheObjectsRoom()
	{
		if (m_OurRoom == null)
		{
			FloorManager.Floor floor = FloorManager.GetInstance().FindFloorAtZ(base.transform.position.z);
			m_OurRoom = RoomManager.GetInstance().LookUpRoom(base.transform.position, floor);
			if (m_OurRoom == null && base.transform.parent != null)
			{
				floor = FloorManager.GetInstance().FindFloorAtZ(base.transform.parent.position.z);
				m_OurRoom = RoomManager.GetInstance().LookUpRoom(base.transform.parent.position, floor);
			}
			if (m_OurRoom != null)
			{
				m_OurRoom.m_CarryObjectInteractions.Add(this);
			}
		}
	}

	private RoomBlob GetObjectsRoom()
	{
		if (m_OurRoom == null)
		{
			CacheObjectsRoom();
		}
		return m_OurRoom;
	}

	private RoomBlob GetCharactersRoom(Character localCharacter)
	{
		if (localCharacter != null)
		{
			return GetRoom(localCharacter.transform.position, localCharacter.CurrentFloor);
		}
		return null;
	}

	private RoomBlob GetRoom(Vector3 position, FloorManager.Floor floor)
	{
		return RoomManager.GetInstance().LookUpRoom(position, floor);
	}

	public override void OnLateJoiningInteractionCatchup(Character character)
	{
		base.OnLateJoiningInteractionCatchup(character);
		CacheObjectsRoom();
		m_IsPickedUp = true;
		if (character != null)
		{
			m_AttachPoint = character.m_CharacterAnimator.transform;
			character.SetCarriedObject(this);
			character.SetInteractingObject_Local(this);
		}
		if (m_BoxCollider != null)
		{
			m_BoxCollider.enabled = false;
		}
	}

	private void PickUpRPC(Character localCharacter)
	{
		m_interactingCharacter.m_CharacterAnimator.StartAnimation(AnimState.IdleCarryB);
		Vector3 position = base.transform.position;
		m_NetObjectLock.m_NetView.RPC("RPC_PickUp", NetTargets.All, localCharacter.m_NetView.viewID, position.x, position.y, position.z);
		UpdateCarriedPosition();
	}

	[PunRPC]
	public void RPC_PickUp(int characterViewID, float x, float y, float z, PhotonMessageInfo info)
	{
		CacheObjectsRoom();
		Vector3 position = default(Vector3);
		position.x = x;
		position.y = y;
		position.z = z;
		m_IsPickedUp = true;
		AIEventManager.GetInstance().SetGroundTileCovered(position, isCovered: false);
		m_interactingCharacter = T17NetView.Find<Character>(characterViewID);
		if (m_interactingCharacter != null)
		{
			m_AttachPoint = m_interactingCharacter.GetComponentInChildren<Animator>().transform;
			m_interactingCharacter.SetCarriedObject(this);
			m_interactingCharacter.SetInteractingObject_Local(this);
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Pickup_Item.ToString(), m_interactingCharacter.gameObject);
		}
		if (m_BoxCollider != null)
		{
			m_BoxCollider.enabled = false;
		}
	}

	private void PutDownRPC(int dropFloor, int dropRow, int dropColumn)
	{
		if (null != m_interactingCharacter)
		{
			m_interactingCharacter.SetCarriedObject(null);
			if (null != m_interactingCharacter.m_CharacterAnimator)
			{
				m_interactingCharacter.m_CharacterAnimator.StopAnimation(AnimState.IdleCarryB);
			}
		}
		m_NetObjectLock.m_NetView.RPC("RPC_PutDown", NetTargets.All, dropFloor, dropRow, dropColumn);
	}

	[PunRPC]
	public void RPC_PutDown(int dropFloor, int dropRow, int dropColumn, PhotonMessageInfo info)
	{
		m_IsPickedUp = false;
		SetPositionToDropLocation(dropFloor, dropRow, dropColumn);
		if (m_BoxCollider != null)
		{
			m_BoxCollider.enabled = true;
		}
		int num = EscapistsRaycast.OverlapBoxNonAlloc(base.transform.position, m_BoxCollider.size / 4f, -1);
		Collider[] colliderOverlapList = EscapistsRaycast.ColliderOverlapList;
		for (int i = 0; i < num; i++)
		{
			ICarryableObjectConsumer component = colliderOverlapList[i].gameObject.GetComponent<ICarryableObjectConsumer>();
			if (component != null && component.OnCarriedObjectDroppedOnUs(this))
			{
				break;
			}
		}
		m_CachedObjData = PackIntoULong(dropFloor, dropRow, dropColumn);
		if (m_bUpdateNetSaveData)
		{
			UpdateMovedObjects(m_NetObjectLock.m_NetView.viewID, m_CachedObjData);
			SerializeAll();
		}
		if (m_interactingCharacter != null)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_PutDown_Item.ToString(), m_interactingCharacter.gameObject);
			m_interactingCharacter.SetCarriedObject(null);
			m_interactingCharacter.SetInteractingObject_Local(null);
			m_interactingCharacter = null;
		}
		m_AttachPoint = null;
	}

	public ulong PackIntoULong(int dropFloor, int dropRow, int dropColumn)
	{
		BitField bitField = new BitField();
		FloorManager.GetInstance().GetFloorMetricsBitLength(dropFloor, 20, out var uXBitLength, out var uYBitLength);
		bitField.Set(12, (uint)m_NetObjectLock.m_NetView.viewID);
		bitField.Set(4, (uint)dropFloor);
		bitField.Set(uXBitLength, (uint)dropColumn);
		bitField.Set(uYBitLength, (uint)dropRow);
		return (ulong)bitField;
	}

	public void UpdateCarriedPosition()
	{
		if (IsPickedUp && m_AttachPoint != null)
		{
			Vector3 vector = m_AttachPoint.position + m_CarryOffset;
			base.transform.position = new Vector3(vector.x, vector.y, base.transform.position.z);
			if (m_Renderer != null)
			{
				m_Renderer.position = new Vector3(vector.x, vector.y, vector.z);
			}
		}
	}

	private void SetPositionToDropLocation(int dropFloor, int dropRow, int dropColumn)
	{
		Vector3 worldPosition = Vector3.zero;
		FloorManager.GetInstance().GetTileCentrePosition(dropFloor, FloorManager.TileSystem_Type.TileSystem_Ground, dropRow, dropColumn, out worldPosition);
		base.transform.position = new Vector3(worldPosition.x, worldPosition.y, worldPosition.z);
		if (m_Rigidbody != null)
		{
			m_Rigidbody.MovePosition(base.transform.position);
		}
		AIEventManager.GetInstance().SetGroundTileCovered(base.transform.position, isCovered: true);
		if (LevelScript.GetInstance().m_Processed)
		{
			float zOffset = LayerHelper.GetZOffset(base.transform);
			if (m_Renderer != null)
			{
				m_Renderer.localPosition = new Vector3(0f, 0f, zOffset);
			}
		}
		else if (m_Renderer != null)
		{
			m_Renderer.localPosition = new Vector3(0f, 0f, 0f);
		}
	}

	public static void UpdateMovedObjects(int objID, ulong objData)
	{
		if (m_MovedObjects.ContainsKey(objID))
		{
			m_MovedObjects[objID] = objData;
		}
		else
		{
			m_MovedObjects.Add(objID, objData);
		}
	}

	public static void SerializeAll()
	{
		if (T17NetManager.OfflineMode || (T17NetManager.NetOnlineMode && T17NetManager.IsMasterClient))
		{
			if (m_NetSaveData == null)
			{
				m_NetSaveData = new NetSaveData();
			}
			m_NetSaveData.m_Data = m_MovedObjects.Values.ToList();
			NetPrisonViewDetails.Instance.CarriedObjectsData = JsonUtility.ToJson(m_NetSaveData);
		}
	}

	public void OnDispenserDeserializeActive()
	{
		CacheObjectsRoom();
	}

	public static bool DeserializeAll(string data, ref string error)
	{
		if (string.IsNullOrEmpty(data))
		{
			return true;
		}
		NetSaveData netSaveData = null;
		try
		{
			netSaveData = JsonUtility.FromJson<NetSaveData>(data);
		}
		catch
		{
			error = "CarryObjectInteration: JSON data is corrupt";
			return false;
		}
		if (netSaveData != null && netSaveData.m_Data != null)
		{
			for (int i = 0; i < netSaveData.m_Data.Count; i++)
			{
				ulong num = netSaveData.m_Data[i];
				if (num != 0)
				{
					BitField bitField = new BitField(num);
					int uInt = (int)bitField.GetUInt(12);
					CarryObjectInteraction carryObjectInteraction = T17NetView.Find<CarryObjectInteraction>(uInt);
					if (carryObjectInteraction != null)
					{
						carryObjectInteraction.Deserialize(bitField);
					}
					UpdateMovedObjects(uInt, num);
				}
			}
		}
		return true;
	}

	public void Deserialize(BitField bitField, bool burnNetViewBits = false)
	{
		if (burnNetViewBits)
		{
			bitField.GetUInt(12);
		}
		int uInt = (int)bitField.GetUInt(4);
		FloorManager.GetInstance().GetFloorMetricsBitLength(uInt, 20, out var uXBitLength, out var uYBitLength);
		int uInt2 = (int)bitField.GetUInt(uXBitLength);
		int uInt3 = (int)bitField.GetUInt(uYBitLength);
		SetPositionToDropLocation(uInt, uInt3, uInt2);
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		base.InteractionEndedEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			interactingCharacter.ClearRemoteInterativeObject();
		}
	}
}
