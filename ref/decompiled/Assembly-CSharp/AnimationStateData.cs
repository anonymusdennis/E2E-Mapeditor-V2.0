using System;
using UnityEngine;

public class AnimationStateData : ScriptableObject
{
	[Serializable]
	public struct AnimStateData
	{
		public string m_name;

		public float m_time;

		public bool m_oneShot;

		public short m_priority;
	}

	public AnimStateData[] animStateData;

	public float GetAnimationTime(AnimState animState)
	{
		return animStateData[(int)animState].m_time;
	}

	public bool GetIsAnimationOneShot(AnimState animState)
	{
		return animStateData[(int)animState].m_oneShot;
	}

	public short GetPriority(AnimState animState)
	{
		if (animState >= AnimState.NOTUSED_BedEnter && (int)animState < animStateData.Length)
		{
			return animStateData[(int)animState].m_priority;
		}
		return 0;
	}
}
