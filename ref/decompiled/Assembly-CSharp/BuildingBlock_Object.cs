using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildingBlock_Object : BuildingBlock_Single
{
	public enum BlockingDirection
	{
		BlocksAll,
		BlocksHorizontal,
		BlocksVerticle
	}

	[Flags]
	public enum SpecialFlagsEnum
	{
		NONE = 0,
		Entrance = 1,
		Exit = 2,
		Marker = 4,
		EscapeMarker = 8,
		RollCallMarker = 0x10,
		InteractDirLeft = 0x20,
		InteractDirRight = 0x40,
		InteractDirUp = 0x80,
		InteractDirDown = 0x100,
		KeepClearMarker = 0x200,
		EscapeMarkerV2 = 0x400,
		BlockEscapeMarker = 0x400,
		AnyMarker = 0xC,
		AnyMarkerV2 = 0x410,
		InteractMarker = 0x1E0,
		InverseEntrance = -2,
		InverseExit = -3,
		InverseMarker = -5,
		InverseRollCallMarker = -17,
		InverseEscapeMarker = -9,
		InverseInteractMarker = -481,
		InverseAnyMarker = -13,
		InverseAnyMarkerV2 = -1041,
		InverseClearMarker = -513,
		InverseBlockEscapeMarker = -1025
	}

	public bool m_Solid = true;

	public bool m_ItsADoor;

	public bool m_ZoneObject;

	public List<int> m_InBlockGroups = new List<int>();

	public BlockingDirection m_BlockingDirection;

	public SpecialFlagsEnum m_SpecialFlags;

	public override BuildingBlockType BlockType => BuildingBlockType.Object;

	public override UnityEngine.Object GetPrefab(int iIndex, bool bVisual = false)
	{
		if (bVisual && m_VisualPrefab != null)
		{
			return m_VisualPrefab;
		}
		return m_Prefab;
	}

	public override void MakeVisualRepresentation(int iIndex)
	{
		base.MakeVisualRepresentation(iIndex);
		if (m_Footprint == null)
		{
			Footprint.BlockTypes blockTypes = Footprint.BlockTypes.Objects;
			if (m_NoBlockingBelow)
			{
				blockTypes |= Footprint.BlockTypes.NoBlockingBelow;
			}
			if (m_Solid)
			{
				blockTypes |= Footprint.BlockTypes.Blocking;
			}
			Footprint footPrint = new Footprint(0, 0, 1, 1, blockTypes);
			AddToFootprint(footPrint);
		}
		float num = (float)m_Footprint.m_iW - 1f;
		float num2 = (float)m_Footprint.m_iH - 1f;
		float num3 = m_Footprint.m_iLeft;
		float num4 = m_Footprint.m_iBottom;
		Vector3 zero = Vector3.zero;
		if (((uint)m_Footprint.m_iW & (true ? 1u : 0u)) != 0)
		{
			zero.x = (num3 + num / 2f) / 2f;
		}
		else
		{
			zero.x = (num3 + (num - 1f) / 2f) / 2f;
		}
		if (((uint)m_Footprint.m_iH & (true ? 1u : 0u)) != 0)
		{
			zero.y = (num4 + num2 / 2f) / 2f;
		}
		else
		{
			zero.y = (num4 + (num2 - 1f) / 2f) / 2f;
		}
		m_Representations[iIndex].transform.localPosition = zero;
	}

	public override void MakeActualObject(int iIndex)
	{
		base.MakeActualObject(iIndex);
		float num = (float)m_Footprint.m_iW - 1f;
		float num2 = (float)m_Footprint.m_iH - 1f;
		float num3 = m_Footprint.m_iLeft;
		float num4 = m_Footprint.m_iBottom;
		Vector3 zero = Vector3.zero;
		if (((uint)m_Footprint.m_iW & (true ? 1u : 0u)) != 0)
		{
			zero.x = (num3 + num / 2f) / 2f;
		}
		else
		{
			zero.x = (num3 + (num - 1f) / 2f) / 2f;
		}
		if (((uint)m_Footprint.m_iH & (true ? 1u : 0u)) != 0)
		{
			zero.y = (num4 + num2 / 2f) / 2f;
		}
		else
		{
			zero.y = (num4 + (num2 - 1f) / 2f) / 2f;
		}
		m_RealObjects[iIndex].transform.localPosition = zero;
		SpecialTextureStamp[] array = m_RealObjects[iIndex].GetComponentsInChildren<SpecialTextureStamp>(includeInactive: true);
		if (array.Length > 1)
		{
		}
		bool flag = true;
		if (m_TextureStamp == null)
		{
			flag = false;
		}
		if (!flag)
		{
			for (int i = 0; i < array.Length; i++)
			{
				UnityEngine.Object.DestroyImmediate(array[i]);
			}
		}
		else
		{
			if (array.Length == 0)
			{
				array = new SpecialTextureStamp[1] { m_RealObjects[iIndex].AddComponent<SpecialTextureStamp>() };
			}
			for (int j = 0; j < array.Length; j++)
			{
				array[j].m_Stamp = m_TextureStamp;
			}
		}
		m_HasPhotonViewCount = m_RealObjects[iIndex].GetComponentsInChildren<PhotonView>().Length;
		if (m_Solid && m_BlockingDirection != 0)
		{
			LevelSetup_LimitTravelDirection levelSetup_LimitTravelDirection = m_RealObjects[iIndex].AddComponent<LevelSetup_LimitTravelDirection>();
			if (levelSetup_LimitTravelDirection != null)
			{
				levelSetup_LimitTravelDirection.m_LimitHorizontal = m_BlockingDirection == BlockingDirection.BlocksHorizontal;
			}
		}
	}
}
