using System;

public class EnumCacheContiguous<T> where T : struct, IConvertible
{
	private Enum[] m_cached;

	private Array m_values;

	public EnumCacheContiguous()
	{
		m_values = Enum.GetValues(typeof(T));
		int length = m_values.Length;
		m_cached = new Enum[length];
		for (int i = 0; i < length; i++)
		{
			if (Convert.ToInt32((T)m_values.GetValue(i)) != i)
			{
			}
			m_cached[i] = (Enum)m_values.GetValue(i);
		}
	}

	public Enum Get(T value)
	{
		return m_cached[Convert.ToInt32(value)];
	}
}
