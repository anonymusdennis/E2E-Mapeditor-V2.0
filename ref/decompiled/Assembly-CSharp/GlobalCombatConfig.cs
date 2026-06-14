using UnityEngine;

[CreateAssetMenu(fileName = "GlobalCombatConfig", menuName = "Team17/Combat/Create Global CombatConfig")]
public class GlobalCombatConfig : ScriptableObject
{
	public Item_Combat m_UnarmedCombatConfig;

	public float m_fCombatNearHitDistance = 0.9f;

	public float m_fCombatDoggieNearHitDistance = 3f;

	public float m_fSmashAttackFullChargeTime = 1f;

	public float m_fSmashAttackDashTime = 1.8f;

	public float m_fSmashAttackAttackTime = 0.3f;

	public float m_fSmashAttackCommitTime = 1f;

	public float m_fKnockBackStunTime = 0.2f;

	public float m_fKnockBackPowerOnDamage = 4f;

	public float m_fKnockBackPowerOnBlock = 2f;
}
