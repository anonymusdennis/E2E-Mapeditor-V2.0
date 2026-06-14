using System;

public struct HiddenFloat
{
	private int m_Offset;

	private int m_Value;

	private static Random ms_random = new Random();

	public HiddenFloat(float value = 0f)
	{
		m_Offset = ms_random.Next(int.MinValue, int.MaxValue);
		m_Value = 0;
		SetValue(value);
	}

	public unsafe void SetValue(float value)
	{
		int* ptr = (int*)(&value);
		m_Value = m_Offset ^ *ptr;
	}

	public unsafe float GetValue()
	{
		float num = 0f;
		int num2 = m_Offset ^ m_Value;
		float* ptr = (float*)(&num2);
		return *ptr;
	}

	public override string ToString()
	{
		return GetValue().ToString();
	}

	public static implicit operator float(HiddenFloat value)
	{
		return value.GetValue();
	}

	public static implicit operator HiddenFloat(float value)
	{
		return new HiddenFloat(value);
	}
}
