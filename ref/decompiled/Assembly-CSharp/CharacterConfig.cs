using UnityEngine;

[CreateAssetMenu(fileName = "CharacterConfig", menuName = "Team17/Config/Create Character Config")]
public class CharacterConfig : ScriptableObject
{
	[Header("Baseline Stats")]
	public float m_HealthBaseLine;

	public float m_StrengthBaseLine;

	public float m_CardioBaseLine;

	public float m_IntellectBaseLine;

	public float m_EnergyBaseLine;

	public float m_HeatBaseLine;

	public float m_MoneyBaseLine;

	public int m_SentenceBaseLine;

	[Header("Stat Decay Rates (per second)")]
	public float m_HealthRestoreRate;

	public float m_EnergyRestoreRate;

	public float m_EnergyRestoreRateBlocking;

	public float m_StrengthDecayRate;

	public float m_IntellectDecayRate;

	public float m_CardioDecayRate;

	public float m_HeatDecayRate;
}
