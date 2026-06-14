using System;
using System.Collections.Generic;
using UnityEngine;

public class GuardAlertnessCustomisation : MonoBehaviour
{
	[Serializable]
	public class GuardAlertnessStats
	{
		public float m_HealthBase = 100f;

		public float m_CardioBase = 50f;

		public float m_StrengthBase = 50f;

		public float m_EnergyBase = 100f;
	}

	private List<GuardAlertnessStats> m_GuardCustomStats = new List<GuardAlertnessStats>();

	public GuardAlertnessStats m_AlertnessZero;

	public GuardAlertnessStats m_AlertnessHalf;

	public GuardAlertnessStats m_AlertnessOne;

	public GuardAlertnessStats m_AlertnessOneAndHalf;

	public GuardAlertnessStats m_AlertnessTwo;

	public GuardAlertnessStats m_AlertnessTwoAndhalf;

	public GuardAlertnessStats m_AlertnessThree;

	public GuardAlertnessStats m_AlertnessThreeAndHalf;

	public GuardAlertnessStats m_AlertnessFour;

	public GuardAlertnessStats m_AlertnessFourAndHalf;

	public GuardAlertnessStats m_AlertnessFive;

	private void Awake()
	{
		m_GuardCustomStats.Clear();
		m_GuardCustomStats.Add(m_AlertnessZero);
		m_GuardCustomStats.Add(m_AlertnessHalf);
		m_GuardCustomStats.Add(m_AlertnessOne);
		m_GuardCustomStats.Add(m_AlertnessOneAndHalf);
		m_GuardCustomStats.Add(m_AlertnessTwo);
		m_GuardCustomStats.Add(m_AlertnessTwoAndhalf);
		m_GuardCustomStats.Add(m_AlertnessThree);
		m_GuardCustomStats.Add(m_AlertnessThreeAndHalf);
		m_GuardCustomStats.Add(m_AlertnessFour);
		m_GuardCustomStats.Add(m_AlertnessFourAndHalf);
		m_GuardCustomStats.Add(m_AlertnessFive);
		for (int i = 0; i < base.transform.childCount; i++)
		{
			AICharacter_Guard component = base.transform.GetChild(i).GetComponent<AICharacter_Guard>();
			if (component != null)
			{
				int activeAlertness = (int)component.m_ActiveAlertness;
				if (activeAlertness >= 0 && activeAlertness < m_GuardCustomStats.Count && m_GuardCustomStats[activeAlertness] != null)
				{
					component.m_CharacterStats.Strength = m_GuardCustomStats[activeAlertness].m_StrengthBase;
					component.m_CharacterStats.m_HealthBaseLine = m_GuardCustomStats[activeAlertness].m_HealthBase;
					component.m_CharacterStats.m_CardioBaseLine = m_GuardCustomStats[activeAlertness].m_CardioBase;
					component.m_CharacterStats.m_EnergyBaseLine = m_GuardCustomStats[activeAlertness].m_EnergyBase;
				}
			}
		}
	}
}
