using System;
using Rotorz.Tile;
using UnityEngine;

public class Hole : T17MonoBehaviour
{
	public const float MAX_HEALTH = 100f;

	public T17NetView m_NetView;

	public Character m_Digger;

	public Character m_FullyDugBy;

	public Material[] m_Materials = new Material[4];

	public Material m_RockUnderneathMaterial;

	public Material m_CoveredUpMaterial;

	public ItemData m_ReclaimedItem;

	public TrackableUIElementsReporter m_TrackableElementReporter;

	public Sprite m_MapIcon;

	public string m_MapToolTipTag;

	public bool m_ManuallyAdded;

	private int m_TileRow = -1;

	private int m_TileColumn = -1;

	private int m_TileFloor = -1;

	private float m_Health;

	private bool m_HasRockUnderneath;

	private bool m_IsCoveredUp;

	private int m_DiggerID = -1;

	private int m_PinID = -1;

	private int m_ItemMgrResponseID = -1;

	private Light m_DigLight;

	private MeshRenderer m_MeshRenderer;

	private DamagableTile m_DamagableTile;

	private DamagableTile m_DamagableTileBelow;

	public int TileRow => m_TileRow;

	public int TileColumn => m_TileColumn;

	public int TileFloor => m_TileFloor;

	public float Health => m_Health;

	public bool HasRockUnderneath
	{
		get
		{
			return m_HasRockUnderneath;
		}
		set
		{
			m_HasRockUnderneath = value;
			SetMaterial();
		}
	}

	public bool IsCoveredUp => m_IsCoveredUp;

	public int GetObjectNetID()
	{
		return m_NetView.viewID;
	}

	protected override void Awake()
	{
		base.Awake();
		HasRockUnderneath = false;
		m_IsCoveredUp = false;
		m_NetView = GetComponent<T17NetView>();
		m_DigLight = GetComponentInChildren<Light>();
		m_MeshRenderer = GetComponent<MeshRenderer>();
		if (m_TrackableElementReporter == null)
		{
			m_TrackableElementReporter = GetComponent<TrackableUIElementsReporter>();
		}
		if (m_ManuallyAdded)
		{
		}
		m_Health = 100f;
		SetMaterial();
		UpdateNamePlate();
	}

	private void Start()
	{
		InitPosition();
		FloorManager.GetInstance().AddHole(this);
		Vector3 position = base.transform.position;
		base.transform.position = new Vector3(position.x, position.y, position.z - 0.05f);
		FloorManager.Floor floor = FloorManager.GetInstance().DownAFloor(FloorManager.GetInstance().FindFloorbyIndex(m_TileFloor));
		if (floor.m_FloorIndex != m_TileFloor && floor.IsUnderGround())
		{
			int row = m_TileRow + 1;
			int tileColumn = m_TileColumn;
			HasRockUnderneath = FloorManager.GetInstance().GetRock(row, tileColumn, floor.m_FloorIndex) != null;
			m_DamagableTileBelow = FloorManager.GetInstance().GetDamagableTile(floor, FloorManager.TileSystem_Type.TileSystem_Wall, row, tileColumn);
		}
		UpdateDamagedTile();
		CullingObjectCollector.GetInstance().Runtime_AddToBucket(GetMeshRenderer(), bCheckForMaterialBlock: false, bAlsoFloorsAbove: true);
	}

	private void InitPosition()
	{
		m_TileFloor = FloorManager.GetInstance().FindFloorAtZ(base.transform.position.z).m_FloorIndex;
		if (!FloorManager.GetInstance().GetTileGridPoint(m_TileFloor, FloorManager.TileSystem_Type.TileSystem_Ground, base.transform.position, out m_TileRow, out m_TileColumn))
		{
			throw new Exception("Unable to find tile row/column");
		}
	}

	protected virtual void OnDestroy()
	{
		FloorManager instance = FloorManager.GetInstance();
		if (instance != null)
		{
			instance.RemoveHole(this);
		}
		CullingObjectCollector instance2 = CullingObjectCollector.GetInstance();
		if (instance2 != null)
		{
			instance2.Runtime_RemoveFromBucket(GetMeshRenderer());
		}
		CameraManager instance3 = CameraManager.GetInstance();
		if (instance3 != null)
		{
			instance3.ForceAnUpdateForActiveCameras();
		}
		m_NetView = null;
	}

	private void SetMaterial()
	{
		if (m_IsCoveredUp && m_CoveredUpMaterial != null)
		{
			GetMeshRenderer().sharedMaterial = m_CoveredUpMaterial;
			return;
		}
		if (m_Health <= 0f && HasRockUnderneath && m_RockUnderneathMaterial != null)
		{
			GetMeshRenderer().sharedMaterial = m_RockUnderneathMaterial;
			return;
		}
		if (m_Materials != null && m_Materials.Length > 0)
		{
			float num = 100f - m_Health;
			int value = (int)(num / 100f * (float)(m_Materials.Length - 1));
			value = Mathf.Clamp(value, 0, m_Materials.Length - 1);
			if (m_Materials[value] != null)
			{
				GetMeshRenderer().sharedMaterial = m_Materials[value];
			}
		}
		SetLightWithHealth();
	}

	private void SetLightWithHealth()
	{
		if (m_DigLight != null)
		{
			float f = (100f - m_Health) / 100f;
			float spotAngle = 100f + 70f * Mathf.Sqrt(f);
			m_DigLight.spotAngle = spotAngle;
		}
	}

	private void UpdateNamePlate()
	{
		if (m_TrackableElementReporter != null)
		{
			string text;
			try
			{
				text = NumberToStringCache.GetPercentageString((int)m_Health);
			}
			catch
			{
				text = $"{(int)m_Health}%";
			}
			m_TrackableElementReporter.SetDisplayName(text);
		}
	}

	public bool Dig(float digAmount, bool bAllowReclaimItem, Character character)
	{
		m_Digger = character;
		m_DiggerID = character.m_NetView.viewID;
		bool flag = m_Health > 0f;
		m_Health -= digAmount;
		if (m_Health <= 0f)
		{
			m_Health = 0f;
		}
		m_IsCoveredUp = false;
		SetMaterial();
		UpdateNamePlate();
		UpdateDamagedTile();
		m_NetView.RPC("RPC_Dug", NetTargets.Others, character.m_NetView.viewID, m_Health);
		if (m_Health == 0f && flag)
		{
			m_FullyDugBy = character;
			if (bAllowReclaimItem && m_ReclaimedItem != null && m_FullyDugBy != null)
			{
				ItemManager.GetInstance().AssignItemRPC(m_FullyDugBy.photonView.ownerId, m_ReclaimedItem.m_ItemDataID, OnReclaimItemResponse, ref m_ItemMgrResponseID);
			}
			PinManager instance = PinManager.GetInstance();
			bool bForMainMap = true;
			bool bForMiniMap = true;
			GameObject target = base.gameObject;
			Sprite mapIcon = m_MapIcon;
			string mapToolTipTag = m_MapToolTipTag;
			Vector3 overrideIconScale = new Vector3(1f, 1f, 1f);
			m_PinID = instance.CreatePin(bForMainMap, bForMiniMap, target, mapIcon, bUpdatePosition: false, null, null, PinManager.Pin.PinFilterType.All, edgable: false, floorTrackable: false, directional: false, mapToolTipTag, localiseToolTipTag: true, bOverrideIconScale: true, overrideIconScale);
		}
		return m_Health == 0f;
	}

	public bool WouldBeFullyDug(float digAmount)
	{
		return !IsFullyDug() && digAmount >= m_Health;
	}

	public bool IsFullyDug()
	{
		return m_Health == 0f;
	}

	public bool IsAboveWall()
	{
		return m_DamagableTileBelow != null && !m_DamagableTileBelow.IsFullyDamaged();
	}

	[PunRPC]
	public void RPC_Dug(int diggerID, float newHealth, PhotonMessageInfo info)
	{
		Character digger = null;
		PhotonView photonView = PhotonView.Find(diggerID);
		if (photonView != null)
		{
			digger = photonView.GetComponent<Character>();
		}
		m_Digger = digger;
		m_DiggerID = diggerID;
		m_Health = newHealth;
		m_IsCoveredUp = false;
		SetMaterial();
		UpdateNamePlate();
		UpdateDamagedTile();
	}

	public void Fill(float healthAmount, bool bFakeCover)
	{
		if (healthAmount < 0f)
		{
			healthAmount = 100f - m_Health;
		}
		m_Health += healthAmount;
		if (m_Health > 100f)
		{
			m_Health = 100f;
		}
		m_IsCoveredUp = bFakeCover;
		SetMaterial();
		UpdateNamePlate();
		UpdateDamagedTile();
		if (m_PinID != -1)
		{
			PinManager.GetInstance().RemovePin(m_PinID);
			m_PinID = -1;
		}
		m_NetView.RPC("RPC_Fill", NetTargets.Others, m_Health, m_IsCoveredUp);
		if (m_Health == 100f && !m_IsCoveredUp)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	[PunRPC]
	public void RPC_Fill(float newHealth, bool bFakeCover, PhotonMessageInfo info)
	{
		m_Health = newHealth;
		m_IsCoveredUp = bFakeCover;
		SetMaterial();
		UpdateNamePlate();
		UpdateDamagedTile();
		if (m_PinID != -1)
		{
			PinManager.GetInstance().RemovePin(m_PinID);
			m_PinID = -1;
		}
		if (m_Health == 100f && !m_IsCoveredUp)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void UpdateDamagedTile()
	{
		if (m_DamagableTile == null)
		{
			m_DamagableTile = GetDamageableTile();
		}
		if (m_DamagableTile != null)
		{
			m_DamagableTile.SetHoleIsCovered(m_IsCoveredUp);
			m_DamagableTile.SetHealth(Health, m_DiggerID);
			FloorManager.GetInstance().UpdateModifiedDamagedTile(m_DamagableTile);
		}
	}

	private DamagableTile GetDamageableTile()
	{
		DamagableTile damagableTile = null;
		if (m_TileFloor == -1)
		{
			InitPosition();
		}
		FloorManager.Floor floor = FloorManager.GetInstance().FindFloorbyIndex(m_TileFloor);
		TileData tile = FloorManager.GetInstance().GetTile(floor, FloorManager.TileSystem_Type.TileSystem_Ground, m_TileRow, m_TileColumn);
		if (tile != null && tile.gameObject != null)
		{
			damagableTile = tile.gameObject.GetComponent<DamagableTile>();
			if (damagableTile == null)
			{
				damagableTile = tile.gameObject.AddComponent<DamagableTile>();
				damagableTile.m_DamageAction = DamagableTile.DamageAction.Hole;
				damagableTile.StartInit();
			}
		}
		return damagableTile;
	}

	private void OnReclaimItemResponse(Item newItem, int eventID)
	{
		if (newItem != null && m_FullyDugBy != null && eventID == m_ItemMgrResponseID && !m_FullyDugBy.m_ItemContainer.AddItemRPC(newItem))
		{
			newItem.DropItemInLevel(m_FullyDugBy, m_FullyDugBy.transform.position);
		}
	}

	public string SerializeData()
	{
		return $"{m_Health},{(m_IsCoveredUp ? 1 : 0)},{m_DiggerID}";
	}

	public void DeserializeData(string data)
	{
		if (!string.IsNullOrEmpty(data))
		{
			string[] array = data.Split(',');
			if (array.Length == 3)
			{
				m_Health = float.Parse(array[0]);
				m_IsCoveredUp = int.Parse(array[1]) == 1;
				m_DiggerID = int.Parse(array[2]);
				SetMaterial();
				UpdateNamePlate();
				UpdateDamagedTile();
			}
		}
	}

	private MeshRenderer GetMeshRenderer()
	{
		if (m_MeshRenderer != null)
		{
			return m_MeshRenderer;
		}
		m_MeshRenderer = GetComponent<MeshRenderer>();
		return m_MeshRenderer;
	}
}
