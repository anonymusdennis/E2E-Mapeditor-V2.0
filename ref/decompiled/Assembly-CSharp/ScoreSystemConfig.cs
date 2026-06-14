using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScoreSystemConfig", menuName = "Team17/Config/Create Score System Config")]
public class ScoreSystemConfig : ScriptableObject
{
	[Serializable]
	public class GradeTimeRange
	{
		public int m_Days;

		public int m_Hours;

		public int m_Minutes;

		public string m_LocalisedGradeText;

		public Sprite m_GradeSprite;
	}

	public List<GradeTimeRange> m_Grades;
}
