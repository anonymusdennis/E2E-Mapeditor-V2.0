public class NumberToStringCache
{
	private static string[] m_PreAllocatedIntReturnStrings;

	private static string[] m_PreAllocatedPercentageReturnStrings;

	public static void Init()
	{
		int num = 111;
		m_PreAllocatedIntReturnStrings = new string[num];
		for (int i = 0; i < 100; i++)
		{
			m_PreAllocatedIntReturnStrings[i] = $"{i:D2}";
		}
		m_PreAllocatedIntReturnStrings[100] = $"{100:D3}";
		for (int i = 0; i < 10; i++)
		{
			m_PreAllocatedIntReturnStrings[i + 101] = $"{i:D1}";
		}
		m_PreAllocatedPercentageReturnStrings = new string[101];
		for (int i = 0; i <= 100; i++)
		{
			m_PreAllocatedPercentageReturnStrings[i] = $"{i}%";
		}
	}

	public static string GetIntAsString(int value, bool bSingleAs2)
	{
		if (value >= 0 && value <= 100)
		{
			int num = ((value >= 10 || bSingleAs2) ? value : (value + 101));
			return m_PreAllocatedIntReturnStrings[num];
		}
		return "INT VALUE NOT IN STRINGS";
	}

	public static string GetPercentageString(int percentage)
	{
		if (percentage >= 0 && percentage <= 100)
		{
			return m_PreAllocatedPercentageReturnStrings[percentage];
		}
		return "PERCENTAGE NOT IN STRINGS";
	}
}
