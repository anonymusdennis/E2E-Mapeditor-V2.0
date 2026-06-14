using UnityEngine;

[CreateAssetMenu(fileName = "TYPE_ArmourConfig", menuName = "Team17/Combat/Create Armour Config")]
public class ArmourConfig : ScriptableObject
{
	public enum ReductionSummaries
	{
		Unassigned = -1,
		Low,
		Medium,
		High
	}

	[Range(0f, 1f)]
	public float DamageReduction;

	public ReductionSummaries m_ReductionSummary = ReductionSummaries.Unassigned;

	public float GetDamageReduction()
	{
		return DamageReduction;
	}
}
