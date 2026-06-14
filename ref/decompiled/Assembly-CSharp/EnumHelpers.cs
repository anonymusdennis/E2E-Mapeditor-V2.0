using System;
using System.Collections.Generic;

public static class EnumHelpers
{
	private static Dictionary<Enum, Attribute> m_sAttributeLookup = new Dictionary<Enum, Attribute>(256);

	public static TAttribute GetAttribute<TAttribute>(ref Enum value) where TAttribute : Attribute
	{
		TAttribute val = (TAttribute)null;
		if (m_sAttributeLookup.TryGetValue(value, out var value2))
		{
			val = value2 as TAttribute;
		}
		else
		{
			Type type = value.GetType();
			string name = Enum.GetName(type, value);
			object[] customAttributes = type.GetField(name).GetCustomAttributes(typeof(TAttribute), inherit: false);
			if (customAttributes.Length > 0)
			{
				val = customAttributes[0] as TAttribute;
				m_sAttributeLookup.Add(value, val);
			}
		}
		return val;
	}
}
