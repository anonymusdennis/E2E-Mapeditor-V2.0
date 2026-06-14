using System;

public struct HiddenInt
{
	private int m_Offset;

	private int m_Value;

	private static Random ms_random = new Random();

	public HiddenInt(int value = 0)
	{
		m_Offset = ms_random.Next(int.MinValue, int.MaxValue);
		m_Value = 0;
		SetValue(value);
	}

	public void SetValue(int value)
	{
		m_Value = value ^ m_Offset;
	}

	public int GetValue()
	{
		return m_Value ^ m_Offset;
	}

	public override string ToString()
	{
		return GetValue().ToString();
	}

	public static implicit operator int(HiddenInt value)
	{
		return value.GetValue();
	}

	public static implicit operator HiddenInt(int value)
	{
		return new HiddenInt(value);
	}
}
