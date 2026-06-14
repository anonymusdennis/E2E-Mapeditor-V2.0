using System.Collections.Generic;
using UnityEngine;

public class LightControl : MonoBehaviour
{
	public class LightSettings
	{
		public Color colour = Color.white;

		public float intensity = 1f;
	}

	[ReadOnly]
	public List<int> m_Groups = new List<int>();

	private LightSettings m_StoredSettings = new LightSettings();

	public float m_RandomTurnOnDelayMax;

	public float m_RandomTurnOffDelayMax;

	private bool m_TurnOn;

	private bool m_TurnOff;

	private float m_SwitchTimer;

	private Light m_Light;

	private CustomLight m_CustomLight;

	private bool m_isEnabled;

	public bool isEnabled => m_isEnabled;

	private void Start()
	{
		m_CustomLight = GetComponent<CustomLight>();
		if (m_CustomLight != null)
		{
			StoreLightSettings(m_CustomLight, ref m_StoredSettings);
			m_isEnabled = m_CustomLight.enabled;
			return;
		}
		m_Light = GetComponent<Light>();
		if (m_Light != null)
		{
			StoreLightSettings(m_Light, ref m_StoredSettings);
			m_isEnabled = m_Light.enabled;
		}
	}

	public void UpdateLight(float deltaTime)
	{
		if (m_SwitchTimer >= 0f)
		{
			m_SwitchTimer -= deltaTime;
		}
		if (m_SwitchTimer <= 0f)
		{
			if (m_TurnOn)
			{
				m_TurnOn = false;
				m_isEnabled = true;
			}
			else if (m_TurnOff)
			{
				m_TurnOff = false;
				m_isEnabled = false;
			}
		}
	}

	public void TurnOn()
	{
		m_TurnOn = true;
		m_TurnOff = false;
		if (m_RandomTurnOnDelayMax > 0f)
		{
			m_SwitchTimer = Random.Range(0f, m_RandomTurnOnDelayMax);
		}
		else
		{
			m_SwitchTimer = 0f;
		}
	}

	public void TurnOff()
	{
		m_TurnOn = false;
		m_TurnOff = true;
		if (m_RandomTurnOffDelayMax > 0f)
		{
			m_SwitchTimer = Random.Range(0f, m_RandomTurnOffDelayMax);
		}
		else
		{
			m_SwitchTimer = 0f;
		}
	}

	public Color GetStoredColour()
	{
		return m_StoredSettings.colour;
	}

	public void ResetColour()
	{
		SetColour(m_StoredSettings.colour);
	}

	public void SetColour(Color colour)
	{
		if (m_CustomLight != null)
		{
			m_CustomLight.SetColour(colour);
		}
		if (m_Light != null)
		{
			m_Light.color = colour;
		}
	}

	public float GetStoredIntensity()
	{
		return m_StoredSettings.intensity;
	}

	public void ResetIntensity()
	{
		SetIntensity(m_StoredSettings.intensity);
	}

	public void SetIntensityMultiplier(float multiplier)
	{
		SetIntensity(m_StoredSettings.intensity * multiplier);
	}

	public void SetIntensity(float intensity)
	{
		if (m_CustomLight != null)
		{
			m_CustomLight.SetIntensity(intensity);
		}
		if (m_Light != null)
		{
			m_Light.intensity = intensity;
		}
	}

	private void StoreLightSettings(Light light, ref LightSettings settings)
	{
		settings.colour = light.color;
		settings.intensity = light.intensity;
	}

	private void StoreLightSettings(CustomLight light, ref LightSettings settings)
	{
		settings.colour = light.m_Color;
		settings.intensity = light.m_Intensity;
	}

	public void ResetSettings()
	{
		if (m_Light != null)
		{
			m_Light.shadows = LightShadows.None;
			return;
		}
		Light component = GetComponent<Light>();
		if (component != null)
		{
			component.shadows = LightShadows.None;
			component = null;
		}
	}
}
