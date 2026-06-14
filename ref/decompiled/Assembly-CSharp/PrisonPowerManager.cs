using System;
using System.Collections.Generic;
using UnityEngine;

public class PrisonPowerManager : MonoBehaviour
{
	[Serializable]
	public class GeneratorData
	{
		public Color m_GeneratorColour;

		public Generator m_Generator;

		public List<ElectricFence> m_ElectricFences = new List<ElectricFence>();
	}

	public delegate void PowerChangedHandler(PrisonPowerManager sender, bool isPowerActive);

	public List<GeneratorData> m_Generators = new List<GeneratorData>();

	private static PrisonPowerManager m_Instance;

	private bool m_bPowerActive = true;

	public event PowerChangedHandler PowerChangedEvent;

	public static PrisonPowerManager GetInstance()
	{
		return m_Instance;
	}

	private void Awake()
	{
		m_Instance = this;
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	public void OnGeneratorStateChanged(Generator generator)
	{
		if (m_Generators == null || generator == null)
		{
			return;
		}
		GeneratorData generatorData = null;
		for (int i = 0; i < m_Generators.Count; i++)
		{
			if (m_Generators[i].m_Generator == generator)
			{
				generatorData = m_Generators[i];
				break;
			}
		}
		if (generatorData == null)
		{
			return;
		}
		bool flag = generator.GeneratorActive();
		if (generatorData.m_ElectricFences != null)
		{
			for (int j = 0; j < generatorData.m_ElectricFences.Count; j++)
			{
				if (generatorData.m_ElectricFences[j] != null)
				{
					generatorData.m_ElectricFences[j].SetEnabled(flag);
				}
			}
		}
		bool bPowerActive = m_bPowerActive;
		m_bPowerActive = false;
		for (int k = 0; k < m_Generators.Count; k++)
		{
			if (m_Generators[k].m_Generator != null)
			{
				m_bPowerActive |= m_Generators[k].m_Generator.GeneratorActive();
			}
		}
		GuardTowerManager.GetInstance().SetSpotlightsActive_Generator(m_bPowerActive);
		if (bPowerActive != m_bPowerActive && this.PowerChangedEvent != null)
		{
			this.PowerChangedEvent(this, m_bPowerActive);
		}
	}

	public bool PowerIsActive()
	{
		return m_bPowerActive;
	}
}
