using UnityEngine;

[CreateAssetMenu(fileName = "TYPE_ItemCombat", menuName = "Team17/Items/Create New Combat Item Config")]
public class Item_Combat : ScriptableObject
{
	public enum AttackRangeSummaries
	{
		Unassigned = -1,
		Low,
		Medium,
		High
	}

	public enum AttackSpeedSummaries
	{
		Unassigned = -1,
		Low,
		Medium,
		High
	}

	public CombatConfig m_CombatConfig;

	public CombatState m_CombatAnimation;

	public float m_fAttackRange = 1f;

	public float m_fRecoveryTime = 1f;

	public float m_fAttackAngle = 90f;

	public int m_HealthDecay;

	public const float AttackRange_Low = 1f;

	public const float AttackRange_Medium = 1.5f;

	public const float RecoveryTime_Fast = 0.6f;

	public const float RecoveryTime_Medium = 1.2f;

	public AttackRangeSummaries GetRangeSummary()
	{
		if (m_fAttackRange <= 1f)
		{
			return AttackRangeSummaries.Low;
		}
		if (m_fAttackRange <= 1.5f)
		{
			return AttackRangeSummaries.Medium;
		}
		return AttackRangeSummaries.High;
	}

	public AttackSpeedSummaries GetRecoveryTimeSummary()
	{
		if (m_fRecoveryTime <= 0.6f)
		{
			return AttackSpeedSummaries.High;
		}
		if (m_fRecoveryTime <= 1.2f)
		{
			return AttackSpeedSummaries.Medium;
		}
		return AttackSpeedSummaries.Low;
	}
}
