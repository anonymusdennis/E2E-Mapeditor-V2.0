using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class HUDItemLayoutGroup
{
	[Serializable]
	public enum PlatformOverride
	{
		none,
		SwitchHandHeld,
		SwitchDocked
	}

	public string m_Name = "New layout group";

	[Tooltip("Use this to force this group to be used on a given platform rather than override the layout group with the same aspect ratio on a particular platform")]
	public PlatformOverride m_PlatformOverride;

	[Tooltip("Used to draw the HUD plan")]
	public Vector2 m_ReferenceResolution;

	public float[] m_ApplicableRatios;

	[Header("World HUD Scaling")]
	public float m_MasterWorldHUDScale = 1f;

	[FormerlySerializedAs("m_ThreePlayerScale")]
	public float m_ThreePlayerConsoleScale = 1f;

	[FormerlySerializedAs("m_ThreePlayerScale")]
	public float m_FourPlayerConsoleScale = 1f;

	public float m_ThreePlayerPcScale = 1f;

	public float m_FourPlayerPcScale = 1f;

	[Header("Element Positioning/Scaling")]
	public HUDItemsLayout m_OnePlayerConfig;

	public HUDItemsLayout[] m_TwoPlayerConfig = new HUDItemsLayout[2];

	public HUDItemsLayout[] m_ThreePlayerConfig = new HUDItemsLayout[3];

	public HUDItemsLayout[] m_FourPlayerConfig = new HUDItemsLayout[4];

	public bool CoversRatio(float fAspectRatio)
	{
		for (int i = 0; i != m_ApplicableRatios.Length; i++)
		{
			if (IsRoughlyEqual(fAspectRatio, m_ApplicableRatios[i]))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsRoughlyEqual(float a, float b)
	{
		return ((!(a < b)) ? (a - b) : (b - a)) <= 0.05f;
	}
}
