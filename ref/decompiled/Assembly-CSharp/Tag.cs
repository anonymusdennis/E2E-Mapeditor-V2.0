using System;
using SaveHelpers;
using UnityEngine;

public class Tag : T17MonoBehaviour
{
	public struct DeserializedTag
	{
		public int viewID;

		public int playerID;

		public int row;

		public int column;

		public int floor;

		public bool active;
	}

	private const float FLOOR_Z_OFFSET = -0.0025f;

	private const float WALL_Z_OFFSET = -0.05f;

	private const string LAYER_CHECK_TAG = "InteractableObject";

	public T17NetView m_NetView;

	[SerializeField]
	[Header("Layer Check Settings")]
	private LayerMask m_LayerCheckMask = default(LayerMask);

	[SerializeField]
	private float m_LayerCheckRadius;

	[Header("Map Settings")]
	[SerializeField]
	private Sprite m_MapIcon;

	[SerializeField]
	private string m_MapToolTipKey = string.Empty;

	private int m_TileRow;

	private int m_TileColumn;

	private int m_TileFloor;

	private int m_PlayerViewID = -1;

	private int m_PinID = -1;

	private int m_CameraCullingMask;

	private bool m_bVisibleByAll;

	public int tileRow => m_TileRow;

	public int tileColumn => m_TileColumn;

	public int tileFloor => m_TileFloor;

	public int playerID
	{
		get
		{
			return m_PlayerViewID;
		}
		set
		{
			m_PlayerViewID = value;
		}
	}

	public int cullingMask => m_CameraCullingMask;

	protected override void Awake()
	{
		base.Awake();
	}

	protected virtual void OnDestroy()
	{
		m_NetView = null;
		m_MapIcon = null;
	}

	public void SetPosition(Vector3 position)
	{
		base.transform.position = position;
		FloorManager.Floor floor = FloorManager.GetInstance().FindFloorAtZ(base.transform.position.z);
		m_TileFloor = floor.m_FloorIndex;
		if (!FloorManager.GetInstance().GetTileGridPoint(m_TileFloor, FloorManager.TileSystem_Type.TileSystem_Ground, base.transform.position, out m_TileRow, out m_TileColumn))
		{
			throw new Exception("Unable to find tile row/column");
		}
		m_bVisibleByAll = ConfigManager.GetInstance() == null || ConfigManager.GetInstance().gameType == PrisonConfig.ConfigType.Cooperative;
		UpdateZPosition();
		m_CameraCullingMask = CreateCullingMask();
		CullingObjectCollector.GetInstance().InGameAddDynamic(base.gameObject);
		if (PinManager.GetInstance() != null)
		{
			Player[] players = null;
			if (!m_bVisibleByAll)
			{
				players = new Player[1] { T17NetView.Find<Player>(playerID) };
			}
			PinManager instance = PinManager.GetInstance();
			bool bForMainMap = true;
			bool bForMiniMap = true;
			GameObject target = base.gameObject;
			Sprite mapIcon = m_MapIcon;
			FloorManager.Floor floor2 = floor;
			m_PinID = instance.CreatePin(bForMainMap, bForMiniMap, target, mapIcon, bUpdatePosition: false, floor2, players, PinManager.Pin.PinFilterType.Tags, edgable: false, floorTrackable: false, directional: false, m_MapToolTipKey);
		}
	}

	private int CreateCullingMask()
	{
		int result = 0;
		if (!m_bVisibleByAll)
		{
			Player player = T17NetView.Find<Player>(playerID);
			if (player != null)
			{
				int num = (int)(player.m_PlayerCameraManagerBindingID - 1);
				result = 1 << num;
			}
		}
		else
		{
			result = -1;
		}
		return result;
	}

	public void UpdateZPosition()
	{
		if (ShouldLayerAboveTile(m_TileRow, m_TileColumn, m_TileFloor))
		{
			float zOffset = LayerHelper.GetZOffset(base.transform);
			Vector3 localPosition = base.transform.localPosition;
			localPosition.z += zOffset;
			localPosition.z += -0.05f;
			base.transform.localPosition = localPosition;
		}
		else
		{
			Vector3 position = base.transform.position;
			base.transform.position = new Vector3(position.x, position.y, position.z + -0.0025f);
		}
	}

	private bool ShouldLayerAboveTile(int row, int column, int floor)
	{
		if (FloorManager.GetInstance().GetTile(m_TileFloor, FloorManager.TileSystem_Type.TileSystem_Wall, m_TileRow, m_TileColumn) != null && LevelScript.GetInstance().m_Processed)
		{
			return true;
		}
		int num = EscapistsRaycast.OverlapSphereNonAlloc(base.transform.position, m_LayerCheckRadius, m_LayerCheckMask);
		if (num > 0)
		{
			bool flag = false;
			Collider[] colliderOverlapList = EscapistsRaycast.ColliderOverlapList;
			for (int i = 0; i < num; i++)
			{
				if (!colliderOverlapList[i].CompareTag("InteractableObject"))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	public void SetTagActive(bool active)
	{
		base.gameObject.SetActive(active);
		if (!active)
		{
			if (CullingObjectCollector.GetInstance() != null)
			{
				CullingObjectCollector.GetInstance().InGameRemoveDynamic(base.gameObject);
			}
			if (CameraManager.GetInstance() != null)
			{
				CameraManager.GetInstance().ForceAnUpdateForActiveCameras();
			}
			if (m_PinID != -1 && PinManager.GetInstance() != null)
			{
				PinManager.GetInstance().RemovePin(m_PinID);
				m_PinID = -1;
			}
		}
	}

	public ulong Serialize()
	{
		BitField bitField = new BitField();
		bitField.Set(12, (uint)m_NetView.viewID);
		bitField.Set(12, (uint)playerID);
		bitField.Set(10, (uint)tileRow);
		bitField.Set(10, (uint)tileColumn);
		bitField.Set(4, (uint)tileFloor);
		bitField.Set(1, base.gameObject.activeSelf ? 1u : 0u);
		return (ulong)bitField;
	}

	public static DeserializedTag GlobalDeserialize(ulong data)
	{
		DeserializedTag result = default(DeserializedTag);
		BitField bitField = new BitField(data);
		result.viewID = (int)bitField.GetUInt(12);
		result.playerID = (int)bitField.GetUInt(12);
		result.row = (int)bitField.GetUInt(10);
		result.column = (int)bitField.GetUInt(10);
		result.floor = (int)bitField.GetUInt(4);
		int uInt = (int)bitField.GetUInt(1);
		result.active = uInt == 1;
		return result;
	}
}
