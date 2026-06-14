using System;
using System.ComponentModel;

public static class AttributesHelperExtension
{
	public static string ToDescription(this Enum value)
	{
		DescriptionAttribute[] array = (DescriptionAttribute[])value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), inherit: false);
		return (array.Length <= 0) ? value.ToString() : array[0].Description;
	}
}
