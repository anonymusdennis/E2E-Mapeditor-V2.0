using UnityEngine;

public class CellBars_ItemInteraction : ItemInteraction
{
	public ItemData m_BedSheet;

	public BarEventManager m_BarEventManager;

	public Animator m_BarAnimator;

	public GameObject m_BarShadows;

	public GameObject m_SolidShadows;

	private bool m_bHasSheet;

	private int m_PhysicsLayerWhenCovered;

	private int m_PhysicsLayerWhenSeeThrough;

	private const string FENCE = "Fence";

	private const string BLOCKVISION = "BlockVision";

	private const string COVERED_PARAM = "Covered";

	private const string COVERED_STATE = "On State On Horizontal";

	protected override void Init()
	{
		ItemData[] transferItemTypes = new ItemData[1] { m_BedSheet };
		SetTransferItemTypes(transferItemTypes);
		if (m_BarEventManager == null)
		{
			m_BarEventManager = GetComponent<BarEventManager>();
		}
		m_PhysicsLayerWhenCovered = LayerMask.NameToLayer("BlockVision");
		m_PhysicsLayerWhenSeeThrough = LayerMask.NameToLayer("Fence");
		base.Init();
	}

	protected override void UpdateState(bool isFromLoad)
	{
		if (!(m_BedSheet == null))
		{
			m_bHasSheet = m_ItemContainer.HasItem(m_BedSheet.m_ItemDataID) > 0;
			if (m_bHasSheet)
			{
				base.gameObject.layer = m_PhysicsLayerWhenCovered;
			}
			else
			{
				base.gameObject.layer = m_PhysicsLayerWhenSeeThrough;
			}
			if (m_BarEventManager != null)
			{
				m_BarEventManager.IsCovered(m_bHasSheet);
			}
			if (m_NetObjectLock != null)
			{
				m_NetObjectLock.m_bIsVisibleToProximityDetector = m_bHasSheet;
			}
			UpdateVisuals(m_bHasSheet, isFromLoad);
		}
	}

	private void UpdateVisuals(bool hasBedSheet, bool isFromLoad)
	{
		if (m_BarShadows != null)
		{
			m_BarShadows.SetActive(!hasBedSheet);
		}
		if (m_SolidShadows != null)
		{
			m_SolidShadows.SetActive(hasBedSheet);
		}
		if (m_BarAnimator != null)
		{
			if (isFromLoad && hasBedSheet)
			{
				m_BarAnimator.CrossFade("On State On Horizontal", 0f);
			}
			m_BarAnimator.SetBool("Covered", hasBedSheet);
		}
	}
}
