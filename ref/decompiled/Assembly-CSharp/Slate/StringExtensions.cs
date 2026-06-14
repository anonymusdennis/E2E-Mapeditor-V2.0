using System.Text.RegularExpressions;

namespace Slate;

public static class StringExtensions
{
	public static string SplitCamelCase(this string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return s;
		}
		s = char.ToUpper(s[0]) + s.Substring(1);
		return Regex.Replace(s, "(?<=[a-z])([A-Z])", " $1").Trim();
	}
}
