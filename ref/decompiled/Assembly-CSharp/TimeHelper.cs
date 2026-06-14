using System;
using UnityEngine;

public static class TimeHelper
{
	[Serializable]
	public class EC2InspectorTime
	{
		[Range(0f, 23f)]
		public int Hour;

		[Range(0f, 59f)]
		public int Minute;

		public EC2InspectorTime(int hour, int min)
		{
			Hour = Mathf.Clamp(hour, 0, 23);
			Minute = Mathf.Clamp(min, 0, 59);
		}
	}

	public static bool IsTimeWithinRange(int hour, int min, int lowerHour, int lowerMin, int upperHour, int upperMin)
	{
		if (lowerHour == 0 && upperHour == 0)
		{
			return true;
		}
		if (upperHour < lowerHour)
		{
			upperHour += 24;
			if (hour < lowerHour)
			{
				hour += 24;
			}
		}
		return (hour > lowerHour || (hour == lowerHour && min >= lowerMin)) && (hour < upperHour || (hour == upperHour && min <= upperMin));
	}

	public static bool DoesTimeRangeGoAcrossMidnight(int startHour, int endHour)
	{
		return startHour > endHour;
	}
}
