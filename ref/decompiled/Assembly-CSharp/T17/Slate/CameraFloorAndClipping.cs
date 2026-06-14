using System;
using Slate;
using UnityEngine;

namespace T17.Slate;

[Attachable(new Type[] { typeof(ActorActionTrack) })]
[Category("T17 Camera")]
public class CameraFloorAndClipping : ActionClip
{
	public int m_FloorIndex;

	public int m_NearClippingFloorIndex;

	private CutsceneCameraTrackableObject m_CachedTrackableObject;

	[HideInInspector]
	public string InfoString = string.Empty;

	public override string info
	{
		get
		{
			if (string.IsNullOrEmpty(InfoString))
			{
				return "Clipping to floor " + m_NearClippingFloorIndex;
			}
			return InfoString;
		}
	}

	protected override void OnEnter()
	{
		base.OnEnter();
		if (m_CachedTrackableObject == null)
		{
			m_CachedTrackableObject = base.actor.GetAddComponent<CutsceneCameraTrackableObject>();
		}
		m_CachedTrackableObject.m_FloorIndex = m_FloorIndex;
		float nearClippingFloorOffset = CalculateNeededOffsetForClipPlane(m_FloorIndex, m_NearClippingFloorIndex);
		m_CachedTrackableObject.m_NearClippingFloorOffset = nearClippingFloorOffset;
	}

	private float CalculateNeededOffsetForClipPlane(int floorIndex, int nearClipFloorIndex)
	{
		float num = CalculateDifferenceBetweenFloors(floorIndex, nearClipFloorIndex);
		float num2 = 2 * FloorManager.GetInstance().m_FloorOffset;
		return 0f - num - num2;
	}

	private float CalculateDifferenceBetweenFloors(int floor1, int floor2)
	{
		float zPositionOfFloor = GetZPositionOfFloor(floor1);
		float zPositionOfFloor2 = GetZPositionOfFloor(floor2);
		return zPositionOfFloor - zPositionOfFloor2;
	}

	private float GetZPositionOfFloor(int floorIndex)
	{
		FloorManager.Floor[] array = null;
		if (FloorManager.GetInstance() != null)
		{
			array = FloorManager.GetInstance().m_PrisonFloors;
		}
		if (array != null)
		{
			return array[floorIndex].m_zPos;
		}
		return 0f;
	}
}
