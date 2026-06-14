using System;
using UnityEngine;

public class VentCover : MonoBehaviour
{
	public enum CoverType
	{
		Vent,
		Sewer
	}

	public CoverType m_CoverType;

	private int m_TileRow = -1;

	private int m_TileColumn = -1;

	private int m_TileFloor = -1;

	private DamagableTile m_DamagableTile;

	private Light m_Light;

	public int TileRow => m_TileRow;

	public int TileColumn => m_TileColumn;

	public int TileFloor => m_TileFloor;

	private void Awake()
	{
		m_DamagableTile = GetComponent<DamagableTile>();
		if (m_DamagableTile != null)
		{
			m_DamagableTile.m_OnHealthUpdated += OnHealthUpdated;
			m_DamagableTile.m_OnHeldItemChanged += DamagedTileHeldItemChanged;
		}
		m_Light = GetComponentInChildren<Light>(includeInactive: true);
	}

	protected virtual void OnDestroy()
	{
		if (m_DamagableTile != null)
		{
			m_DamagableTile.m_OnHealthUpdated -= OnHealthUpdated;
			m_DamagableTile.m_OnHeldItemChanged -= DamagedTileHeldItemChanged;
		}
	}

	private void Start()
	{
		FloorManager.Floor floor = FloorManager.GetInstance().FindFloorAtZ(base.transform.position.z);
		m_TileFloor = floor.m_FloorIndex;
		if (!FloorManager.GetInstance().GetTileGridPoint(m_TileFloor, FloorManager.TileSystem_Type.TileSystem_Ground, base.transform.position, out m_TileRow, out m_TileColumn))
		{
			LevelScript instance = LevelScript.GetInstance();
			string text = ((!instance || !instance.m_LevelSetup) ? "unknown" : instance.m_LevelSetup.m_NameLocalizationKey);
			throw new Exception("Unable to find tile row/column - level:" + text + ", floor:" + m_TileFloor + ", z:" + base.transform.position.z);
		}
		FloorManager.GetInstance().AddVentCover(this);
		if (floor.IsPrisonFloor() && m_Light != null)
		{
			Vector3 localPosition = m_Light.transform.localPosition;
			localPosition.y += -1f;
			m_Light.transform.localPosition = localPosition;
		}
	}

	public bool HasBeenRemoved()
	{
		if (m_DamagableTile != null)
		{
			return m_DamagableTile.Health == 0f && !IsTileHoldingItem();
		}
		return false;
	}

	public bool IsTileHoldingItem()
	{
		if (m_DamagableTile != null)
		{
			return m_DamagableTile.IsHoldingItem();
		}
		return false;
	}

	public float GetTileHealth()
	{
		if (m_DamagableTile != null)
		{
			return m_DamagableTile.Health;
		}
		return 100f;
	}

	private void OnHealthUpdated(float newHealth)
	{
		UpdateLightForVentState(newHealth);
	}

	private void DamagedTileHeldItemChanged(Item obj)
	{
		UpdateLightForVentState(m_DamagableTile.Health);
	}

	private void UpdateLightForVentState(float health)
	{
		if (m_Light != null)
		{
			bool flag = health <= Mathf.Epsilon;
			m_Light.gameObject.SetActive(flag && !IsTileHoldingItem());
		}
	}
}
