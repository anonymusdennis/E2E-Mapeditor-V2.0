using System.Collections.Generic;
using UnityEngine;

public class CustomLightManager
{
	internal FastList<CustomLight> m_Lights = new FastList<CustomLight>();

	internal FastList<CustomLight> m_LightsDynamic = new FastList<CustomLight>();

	public const float kLightUpdateDist = 3f;

	private int m_floor = -1;

	private Vector2 m_camPos = new Vector2(0f, 0f);

	internal int m_FrameDirty;

	public int currentFloor => m_floor;

	public bool ShouldUpdateLights(int floor, Vector2 campos)
	{
		if (m_floor != floor)
		{
			return true;
		}
		if ((campos - m_camPos).sqrMagnitude > 9f)
		{
			return true;
		}
		return false;
	}

	public void UpdateCameraPos(int floor, Vector2 campos)
	{
		m_floor = floor;
		m_camPos = campos;
		m_FrameDirty = UpdateManager.frameCount;
	}

	public void ForceUpdate()
	{
		m_floor = -1;
	}

	public void Add(CustomLight o)
	{
		m_Lights.Add(o);
	}

	public void AddDynamic(CustomLight o)
	{
		m_LightsDynamic.Add(o);
	}

	public void RemoveAny(CustomLight o)
	{
		if (!m_Lights.Remove(o))
		{
			m_LightsDynamic.Remove(o);
		}
	}

	public void Remove(CustomLight o)
	{
		m_Lights.Remove(o);
	}

	public void RemoveDynamic(CustomLight o)
	{
		m_LightsDynamic.Remove(o);
	}

	public void RemoveAllLights()
	{
		RemoveAllStaticLights();
		RemoveAllDynamicLights();
	}

	public void RemoveAllStaticLights()
	{
		m_Lights.Clear();
	}

	public void RemoveAllDynamicLights()
	{
		m_LightsDynamic.Clear();
	}
}
