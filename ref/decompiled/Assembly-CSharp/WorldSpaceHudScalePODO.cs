using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class WorldSpaceHudScalePODO
{
	[FormerlySerializedAs("m_SplitscreenDownscaleAdditionalOffset")]
	public Vector3 m_SplitscreenConsoleAdditionalOffset = Vector2.zero;

	[FormerlySerializedAs("m_SplitscreenUpscalescaleAdditionalOffset")]
	public Vector3 m_SplitscreenPcAdditionalOffset = Vector2.zero;

	public Vector3 m_NormalScale = Vector3.one;

	[FormerlySerializedAs("m_SplitscreenDownscale")]
	public Vector3 m_SplitscreenConsoleScale = Vector3.one;

	[FormerlySerializedAs("m_SplitscreenUpscale")]
	public Vector3 m_SplitscreenPcScale = Vector3.one;

	private WorldSpaceHudScalePODO()
	{
	}

	public void PositionTransform(Transform transform, Vector3 position, bool hasHorizontallySplitscreen)
	{
		if (hasHorizontallySplitscreen)
		{
			Vector3 correctHudScaleForPlatform = CameraManager.GetCorrectHudScaleForPlatform(m_SplitscreenConsoleAdditionalOffset, m_SplitscreenPcAdditionalOffset);
			Vector3 correctHudScaleForPlatform2 = CameraManager.GetCorrectHudScaleForPlatform(m_SplitscreenConsoleScale, m_SplitscreenPcScale);
			transform.position = position + correctHudScaleForPlatform;
			transform.localScale = correctHudScaleForPlatform2;
		}
		else
		{
			Vector3 playerWorldHudScale = HUDMenuFlow.Instance.GetPlayerWorldHudScale();
			Vector3 defaultPlayerWorldHudScale = HUDMenuFlow.Instance.GetDefaultPlayerWorldHudScale();
			transform.position = position;
			transform.localScale = new Vector3(defaultPlayerWorldHudScale.x / playerWorldHudScale.x * m_NormalScale.x, defaultPlayerWorldHudScale.y / playerWorldHudScale.y * m_NormalScale.y, defaultPlayerWorldHudScale.z / playerWorldHudScale.z) * m_NormalScale.z;
		}
	}
}
