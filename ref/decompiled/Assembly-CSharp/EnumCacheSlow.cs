using System;

public class EnumCacheSlow<T> where T : struct, IConvertible
{
	private Enum[] m_cached;

	private Array m_values;

	public EnumCacheSlow()
	{
		m_values = Enum.GetValues(typeof(T));
		int length = m_values.Length;
		m_cached = new Enum[length];
		for (int i = 0; i < length; i++)
		{
			m_cached[i] = (Enum)m_values.GetValue(i);
		}
	}

	private int GetIndex(T value)
	{
		int length = m_values.Length;
		for (int i = 0; i < length; i++)
		{
			if (((T)m_values.GetValue(i)).Equals(value))
			{
				return i;
			}
		}
		return -1;
	}

	public Enum Get(T value)
	{
		return m_cached[GetIndex(value)];
	}
}
