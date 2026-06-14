using System;
using UnityEngine;

public class FloatingZoneIcon : MonoBehaviour
{
	[Flags]
	public enum UpdateFlags
	{
		None = 0,
		Icon = 1,
		Position = 2,
		Layer = 4,
		Display = 8
	}

	private static FloatingZoneIcon m_FloatingZoneIcon;

	[Tooltip("The Meshrenderer we will be controlling")]
	public MeshRenderer m_ZoneImageMat;

	public Vector3 m_Offset = Vector3.zero;

	private int m_iX = -1;

	private int m_iY = -1;

	private BaseLevelManager.LevelLayers m_Layer = BaseLevelManager.LevelLayers.TOTAL;

	private ZoneDetailsManager.ZoneTypes m_ZoneType = ZoneDetailsManager.ZoneTypes.TOTAL;

	private bool m_bDisplaying;

	private UpdateFlags m_UpdateFlags;

	private Transform m_MyTransform;

	public static FloatingZoneIcon GetInstance()
	{
		return m_FloatingZoneIcon;
	}

	private void Awake()
	{
		m_FloatingZoneIcon = this;
	}

	private void Start()
	{
		m_MyTransform = base.transform;
		m_ZoneImageMat.enabled = m_bDisplaying;
	}

	private void OnDestroy()
	{
		m_FloatingZoneIcon = null;
	}

	private void LateUpdate()
	{
		RepositionIcon();
		UpdateIcon();
		UpdateLayer();
		UpdateDisplay();
	}

	public void SetTheTilePosition(int iX, int iY)
	{
		if (iX != m_iX || iY != m_iY)
		{
			m_iX = iX;
			m_iY = iY;
			m_UpdateFlags |= UpdateFlags.Position;
		}
	}

	public void SetTheZoneType(ZoneDetailsManager.ZoneTypes type)
	{
		if (type != m_ZoneType)
		{
			m_ZoneType = type;
			m_UpdateFlags |= UpdateFlags.Icon;
		}
	}

	public void SetTheLayer(BaseLevelManager.LevelLayers layer)
	{
		if (layer != m_Layer)
		{
			m_Layer = layer;
			m_UpdateFlags |= UpdateFlags.Layer;
		}
	}

	public void Show(bool bShow)
	{
		if (bShow != m_bDisplaying)
		{
			m_bDisplaying = bShow;
			m_UpdateFlags |= UpdateFlags.Display;
		}
	}

	private void RepositionIcon()
	{
		if ((m_UpdateFlags & UpdateFlags.Position) == UpdateFlags.Position)
		{
			m_MyTransform.localPosition = new Vector3(m_iX, m_iY - 119, -30f) + m_Offset;
		}
	}

	private void UpdateIcon()
	{
		if ((m_UpdateFlags & UpdateFlags.Icon) != UpdateFlags.Icon)
		{
			return;
		}
		ZoneDetailsManager instance = ZoneDetailsManager.GetInstance();
		if (instance != null)
		{
			ZoneDetailsManager.ZoneDetails zoneDetails = instance.GetZoneDetails(m_ZoneType);
			if (zoneDetails != null)
			{
				m_ZoneImageMat.material = zoneDetails.m_ZoneImage;
				m_UpdateFlags &= ~UpdateFlags.Icon;
			}
		}
	}

	private void UpdateLayer()
	{
		if ((m_UpdateFlags & UpdateFlags.Layer) == UpdateFlags.Layer)
		{
			BaseLevelManager instance = BaseLevelManager.GetInstance();
			if (instance != null)
			{
				m_MyTransform.parent = instance.m_BuildingLayers[(uint)m_Layer].m_Tiles.transform;
				m_UpdateFlags &= ~UpdateFlags.Layer;
			}
		}
	}

	private void UpdateDisplay()
	{
		if ((m_UpdateFlags & UpdateFlags.Display) == UpdateFlags.Display)
		{
			m_ZoneImageMat.enabled = m_bDisplaying;
			m_UpdateFlags &= ~UpdateFlags.Display;
		}
	}
}
