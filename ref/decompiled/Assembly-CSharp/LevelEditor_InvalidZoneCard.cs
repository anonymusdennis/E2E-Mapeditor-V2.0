using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditor_InvalidZoneCard : LevelEditor_ZoneCard
{
	public GameObject m_InvalidNumberMessage;

	public GameObject m_OutsideInsideTilesMessage;

	public GameObject m_ObjectOutOfBoundsMessage;

	public GameObject m_ObjectUnreachableMessage;

	private RawImage m_InvalidNumberMessageImage;

	private RawImage m_ObjectOutOfBoundsMessageImage;

	private RawImage m_ObjectUnreachableMessageImage;

	private LevelEditor_ZoneManager.Zone m_CurrentZone;

	private int m_CurrentZoneUpdateCount = -1;

	private void Awake()
	{
		m_InvalidNumberMessage.SetActive(value: false);
		m_ObjectOutOfBoundsMessage.SetActive(value: false);
		m_ObjectUnreachableMessage.SetActive(value: false);
		m_OutsideInsideTilesMessage.SetActive(value: false);
		m_InvalidNumberMessageImage = m_InvalidNumberMessage.GetComponentInChildren<RawImage>();
		m_ObjectOutOfBoundsMessageImage = m_ObjectOutOfBoundsMessage.GetComponentInChildren<RawImage>();
		m_ObjectUnreachableMessageImage = m_ObjectUnreachableMessage.GetComponentInChildren<RawImage>();
	}

	private void Update()
	{
		if (m_CurrentZoneUpdateCount != m_CurrentZone.m_ZoneUpdateCount)
		{
			m_CurrentZoneUpdateCount = m_CurrentZone.m_ZoneUpdateCount;
			RefreshInvalidMessages();
		}
	}

	public override void SetCardDataForZone(LevelEditor_ZoneManager.Zone newZone)
	{
		if (!object.ReferenceEquals(newZone, m_CurrentZone))
		{
			base.SetCardDataForZone(newZone);
			m_CurrentZone = newZone;
			RefreshInvalidMessages();
			m_CurrentZoneUpdateCount = -1;
		}
	}

	public void RefreshInvalidMessages()
	{
		if (m_CurrentZone.IsFullyValid())
		{
			return;
		}
		if (m_CurrentZone.m_Required.Count > 0)
		{
			m_InvalidNumberMessage.SetActive(value: true);
			LevelEditor_ZoneManager.Zone.StillRequired stillRequired = m_CurrentZone.m_Required[0];
			BuildingBlockGroupManager instance = BuildingBlockGroupManager.GetInstance();
			BuildingBlockGroupManager.Group groupByIndex = instance.GetGroupByIndex(stillRequired.m_BlockGroupIndex);
			if (groupByIndex.m_Blocks.Length > 0)
			{
				BaseBuildingBlock block = BuildingBlockManager.GetBlock(groupByIndex.m_Blocks[0]);
				SetUIImage(m_InvalidNumberMessageImage, block.m_UIImage);
			}
		}
		else if (m_InvalidNumberMessage.activeSelf)
		{
			m_InvalidNumberMessage.SetActive(value: false);
		}
		List<LevelEditor_ZoneManager.Zone.ObjectsInZone> blocksInZone = m_CurrentZone.m_BlocksInZone;
		int num = -1;
		int num2 = -1;
		int i = 0;
		for (int count = blocksInZone.Count; i < count; i++)
		{
			if (num == -1 && blocksInZone[i].m_BeingBlocked)
			{
				num = i;
			}
			if (num2 == -1 && blocksInZone[i].m_OnlyPartiallyIn)
			{
				num2 = i;
			}
		}
		if (num != -1)
		{
			m_ObjectUnreachableMessage.SetActive(value: true);
			BaseBuildingBlock block2 = BuildingBlockManager.GetBlock(blocksInZone[num].m_BlockID);
			SetUIImage(m_ObjectUnreachableMessageImage, block2.m_UIImage);
		}
		else if (m_ObjectUnreachableMessage.activeSelf)
		{
			m_ObjectUnreachableMessage.SetActive(value: false);
		}
		if (num2 != -1)
		{
			m_ObjectOutOfBoundsMessage.SetActive(value: true);
			BaseBuildingBlock block3 = BuildingBlockManager.GetBlock(blocksInZone[num2].m_BlockID);
			SetUIImage(m_ObjectOutOfBoundsMessageImage, block3.m_UIImage);
		}
		else if (m_ObjectOutOfBoundsMessage.activeSelf)
		{
			m_ObjectOutOfBoundsMessage.SetActive(value: false);
		}
	}

	private void SetUIImage(RawImage image, Material mat)
	{
		image.texture = mat.mainTexture;
		Rect uvRect = new Rect(mat.GetTextureOffset("_MainTex"), mat.GetTextureScale("_MainTex"));
		image.uvRect = uvRect;
	}
}
