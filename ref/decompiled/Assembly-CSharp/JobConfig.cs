using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "JobConfig", menuName = "Team17/Config/Create Job Config")]
public class JobConfig : ScriptableObject
{
	[Serializable]
	public class SpeechLines
	{
		public List<string> lines;
	}

	[Header("Job Officer")]
	[Range(0f, 60f)]
	public int m_CharacterLateTime = 30;

	[Range(0f, 100f)]
	public float m_MissedJobOfficerHeatIncrease = 10f;

	public float m_MoneyReward = 10f;

	public SpeechLines[] m_SpeechLines;
}
