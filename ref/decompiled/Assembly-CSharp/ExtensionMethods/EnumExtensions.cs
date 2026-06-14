using System;

namespace ExtensionMethods;

public static class EnumExtensions
{
	public static bool TryParse<TEnum>(this Enum theEnum, string value, bool ignoreCase, out TEnum result)
	{
		bool result2 = true;
		result = default(TEnum);
		try
		{
			result = (TEnum)Enum.Parse(typeof(TEnum), value, ignoreCase);
		}
		catch (ArgumentException)
		{
			result2 = false;
		}
		return result2;
	}

	public static bool TryParse<TEnum>(this Enum theEnum, string value, out TEnum result)
	{
		return theEnum.TryParse<TEnum>(value, ignoreCase: false, out result);
	}
}
