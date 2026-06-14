using UnityEngine;

[CreateAssetMenu(fileName = "OpinionConfig", menuName = "Team17/Config/Create Opinion Config")]
public class OpinionConfig : ScriptableObject
{
	[Header("Like / Hate Thresholds")]
	[Range(0f, 100f)]
	public int m_LowOpinionThreshold;

	[Range(0f, 100f)]
	public int m_HighOpinionThreshold;

	[Header("Starting Opinions")]
	public OpinionManager.DefaultOpinionInfo m_InitialOpinionOfPlayers = new OpinionManager.DefaultOpinionInfo();

	[Header("Other Settings")]
	[Range(0f, 2f)]
	public float m_ItemGiftValueModifier = 1f;

	public OpinionManager.SightCheckInfo m_LowOpinionSightChecks = new OpinionManager.SightCheckInfo();

	public OpinionManager.AttackOpinionInfos m_CharacterAttackOpinionLosses = new OpinionManager.AttackOpinionInfos();
}
