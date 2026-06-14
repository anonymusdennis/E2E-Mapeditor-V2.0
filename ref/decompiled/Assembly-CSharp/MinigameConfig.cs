using UnityEngine;

[CreateAssetMenu(fileName = "MinigameConfig", menuName = "Team17/Config/Create Minigame Config")]
public class MinigameConfig : ScriptableObject
{
	[Header("Gym Equipment")]
	[Range(0f, 2f)]
	public float m_Weights_RewardModifier = 1f;

	[Range(0f, 2f)]
	public float m_KettleBells_RewardModifier = 1f;

	[Range(0f, 2f)]
	public float m_Pullups_RewardModifier = 1f;

	[Range(0f, 2f)]
	public float m_ExcersiseBike_RewardModifier = 1f;

	[Range(0f, 2f)]
	public float m_Threadmill_RewardModifier = 1f;

	[Range(0f, 2f)]
	public float m_PommelHorse_RewardModifier = 1f;

	[Range(0f, 2f)]
	public float m_Footbag_RewardModifier = 1f;

	[Range(0f, 2f)]
	[Header("Reading")]
	public float m_Reading_RewardModifier = 1f;

	[Header("Solitary")]
	[Range(0f, 2f)]
	public float m_Solitary_StaminaLossModifier = 1f;
}
