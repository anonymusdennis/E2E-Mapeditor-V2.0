using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ParadoxNotion;

public static class StringUtils
{
	public static string SplitCamelCase(this string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return s;
		}
		s = s.Replace("_", " ");
		s = char.ToUpper(s[0]) + s.Substring(1);
		return Regex.Replace(s, "(?<=[a-z])([A-Z])", " $1").Trim();
	}

	public static string GetCapitals(this string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return string.Empty;
		}
		string text = string.Empty;
		for (int i = 0; i < s.Length; i++)
		{
			char c = s[i];
			if (char.IsUpper(c))
			{
				text += c;
			}
		}
		return text.Trim();
	}

	public static string GetAlphabetLetter(int index)
	{
		if (index < 0)
		{
			return null;
		}
		string text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		if (index >= text.Length)
		{
			return index.ToString();
		}
		return text[index].ToString();
	}

	public static string ToStringAdvanced(this object o)
	{
		if (o == null || o.Equals(null))
		{
			return "NULL";
		}
		if (o is string)
		{
			return $"\"{(string)o}\"";
		}
		if (o is UnityEngine.Object)
		{
			return (o as UnityEngine.Object).name;
		}
		Type type = o.GetType();
		if (type.RTIsSubclassOf(typeof(Enum)))
		{
			FlagsAttribute flagsAttribute = type.RTGetAttribute<FlagsAttribute>(inherited: true);
			if (flagsAttribute != null)
			{
				string text = string.Empty;
				int num = 0;
				Array values = Enum.GetValues(type);
				foreach (object item in values)
				{
					if ((Convert.ToInt32(item) & Convert.ToInt32(o)) == Convert.ToInt32(item))
					{
						num++;
						text = ((!(text == string.Empty)) ? "Mixed..." : item.ToString());
					}
				}
				if (num == 0)
				{
					return "Nothing";
				}
				if (num == values.Length)
				{
					return "Everything";
				}
				return text;
			}
		}
		return o.ToString();
	}
}
