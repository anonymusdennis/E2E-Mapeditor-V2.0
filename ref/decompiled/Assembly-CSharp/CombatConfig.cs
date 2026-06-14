using UnityEngine;

[CreateAssetMenu(fileName = "TYPE_CombatConfig", menuName = "Team17/Combat/Create CombatConfig")]
public class CombatConfig : ScriptableObject
{
	public enum DamageSummaries
	{
		Unassigned = -1,
		Low,
		Medium,
		High,
		Highest
	}

	public DamageSummaries m_DamageSummary = DamageSummaries.Unassigned;

	public float[] NormalAttackEnergyCost = new float[5];

	public float[] HeavyAttackEnergyCost = new float[5];

	public float[] NormalAttackDamage = new float[5];

	public float[] HeavyAttackDamage = new float[5];

	public float[] NormalAttackBlockCost = new float[5];

	public float[] HeavyAttackBlockCost = new float[5];

	public float GetNormalAttackEnergyCost(EnergyModifier energy)
	{
		return NormalAttackEnergyCost[(int)energy];
	}

	public float GetHeavyAttackEnergyCost(EnergyModifier energy)
	{
		return HeavyAttackEnergyCost[(int)energy];
	}

	public float GetNormalAttackDamage(StrengthModifier strength)
	{
		return NormalAttackDamage[(int)strength];
	}

	public float GetHeavyAttackDamage(StrengthModifier strength)
	{
		return HeavyAttackDamage[(int)strength];
	}

	public float GetNormalAttackBlockCost(EnergyModifier energy)
	{
		return NormalAttackBlockCost[(int)energy];
	}

	public float GetHeavyAttackBlockCost(EnergyModifier energy)
	{
		return HeavyAttackBlockCost[(int)energy];
	}
}
