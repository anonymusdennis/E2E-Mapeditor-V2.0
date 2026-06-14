using System;
using UnityEngine;

public class Personality : MonoBehaviour
{
	public enum ePersonality
	{
		Timid,
		Happy,
		Sad,
		Aggressive,
		Lazy,
		Sarcastic,
		Shy,
		Helpful
	}

	public enum PersonalityType
	{
		Allround,
		Psycho,
		Thief,
		GoodEgg,
		Brains
	}

	public enum CombatStyle
	{
		Allround,
		Aggressive,
		Nervous,
		Defensive,
		Tactical
	}

	[Serializable]
	public class FloatSetting
	{
		[SerializeField]
		private float[] data = new float[5];

		[SerializeField]
		private bool single;

		[SerializeField]
		private float singleValue;

		public float GetValue(PersonalityType type)
		{
			if (single)
			{
				return singleValue;
			}
			int num = (int)type;
			if (type == PersonalityType.Psycho)
			{
				num = UnityEngine.Random.Range(0, 4);
			}
			return data[num];
		}
	}

	public static ePersonality[] PersonalityAllround = new ePersonality[3]
	{
		ePersonality.Happy,
		ePersonality.Sad,
		ePersonality.Lazy
	};

	public static ePersonality[] PersonalityPsycho = new ePersonality[7]
	{
		ePersonality.Timid,
		ePersonality.Happy,
		ePersonality.Sad,
		ePersonality.Aggressive,
		ePersonality.Lazy,
		ePersonality.Sarcastic,
		ePersonality.Shy
	};

	public static ePersonality[] PersonalityThief = new ePersonality[2]
	{
		ePersonality.Happy,
		ePersonality.Sarcastic
	};

	public static ePersonality[] PersonalityGoodEgg = new ePersonality[3]
	{
		ePersonality.Happy,
		ePersonality.Sad,
		ePersonality.Timid
	};

	public static ePersonality[] PersonalityBrains = new ePersonality[3]
	{
		ePersonality.Shy,
		ePersonality.Sarcastic,
		ePersonality.Timid
	};

	public static CombatStyle GetCombatStyle(PersonalityType personalityType)
	{
		return personalityType switch
		{
			PersonalityType.Allround => CombatStyle.Allround, 
			PersonalityType.Psycho => CombatStyle.Aggressive, 
			PersonalityType.Thief => CombatStyle.Nervous, 
			PersonalityType.GoodEgg => CombatStyle.Defensive, 
			PersonalityType.Brains => CombatStyle.Tactical, 
			_ => CombatStyle.Allround, 
		};
	}

	public static ePersonality GetPersonality(PersonalityType personalityType)
	{
		return personalityType switch
		{
			PersonalityType.Allround => PersonalityAllround[UnityEngine.Random.Range(0, PersonalityAllround.Length)], 
			PersonalityType.Psycho => PersonalityPsycho[UnityEngine.Random.Range(0, PersonalityPsycho.Length)], 
			PersonalityType.Thief => PersonalityThief[UnityEngine.Random.Range(0, PersonalityThief.Length)], 
			PersonalityType.GoodEgg => PersonalityGoodEgg[UnityEngine.Random.Range(0, PersonalityGoodEgg.Length)], 
			PersonalityType.Brains => PersonalityBrains[UnityEngine.Random.Range(0, PersonalityBrains.Length)], 
			_ => ePersonality.Happy, 
		};
	}
}
