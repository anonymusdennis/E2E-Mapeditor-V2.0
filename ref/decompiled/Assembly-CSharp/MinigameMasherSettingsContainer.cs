using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "MinigameConfig", menuName = "Team17/Config/Create Minigame Settings Container")]
public class MinigameMasherSettingsContainer : ScriptableObject
{
	[Header("WeightLifting")]
	public AlternateButtonMasher.AlternateMasherSettings m_AlternateMasherSettings;

	public float m_AlternateMasherTimeToKeepInThreshold = 3f;

	[Header("KettleBells")]
	public GymMasher_KettleBelts.HoldingMasherSettings m_KettleHolderMasherSettings;

	[Header("PullUps")]
	public GymMasher_Pullup.PullupMasherSettings m_PullUpsMasherSettings;

	[Header("ExerciseBike")]
	public GymMasher_Threadmill_ExerciseBike.ThreadMillMasherSettings m_ExerciseBikeMasherSettings;

	[Header("Threadmill")]
	public GymMasher_Threadmill_ExerciseBike.ThreadMillMasherSettings m_ThreadmillMasherSettings;

	[Header("PommelHorse")]
	public GymMasher_Pommel_Footbag.PommelMasherSettings m_PommelHorseMasherSettings;

	[Header("Footbag")]
	public GymMasher_Pommel_Footbag.PommelMasherSettings m_FootbagMasherSettings;

	[Header("Reading Masher")]
	public ReadingMasher.MasherSettings m_ReadingMasherSettings;

	[Header("Solitary Masher settings")]
	public SolitaryPotatoMasher.MasherSettings m_SolitaryMasherSettings;
}
