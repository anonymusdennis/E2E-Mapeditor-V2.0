using System;
using System.Diagnostics;

namespace SaveHelpers;

public class BitField
{
	private ulong m_uValue;

	private int m_iBitsUsed;

	public BitField(ulong uStartValue)
	{
		m_uValue = uStartValue;
	}

	public BitField(ulong uStartValue, int iBitsUsed)
	{
		m_uValue = uStartValue;
		m_iBitsUsed = iBitsUsed;
	}

	public BitField()
	{
	}

	public static implicit operator BitField(ulong uValue)
	{
		return new BitField(uValue);
	}

	public static explicit operator ulong(BitField uValue)
	{
		return uValue.m_uValue;
	}

	public static explicit operator long(BitField uValue)
	{
		return (long)uValue.m_uValue;
	}

	public void Reset()
	{
		m_uValue = 0uL;
		m_iBitsUsed = 0;
	}

	public void Init(ulong uValue, int iBits)
	{
		m_uValue = uValue;
		m_iBitsUsed = iBits;
	}

	public void Set(int iBits, uint uValue)
	{
		Set(iBits, (ulong)uValue);
	}

	public void Set(int iBits, int iValue)
	{
		Set(iBits, (long)iValue);
	}

	public void Set(int iBits, byte uValue)
	{
		Set(iBits, (ulong)uValue);
	}

	public void Set(float uValue)
	{
		byte[] bytes = BitConverter.GetBytes(uValue);
		Set(32, bytes);
	}

	public void Set(bool bValue)
	{
		if (bValue)
		{
			ulong num = (ulong)(1L << m_iBitsUsed);
			m_uValue |= num;
		}
		m_iBitsUsed++;
	}

	public void Set(int iBits, ulong ulValue)
	{
		ulong num = (ulong)((1L << iBits) - 1);
		ulong num2 = num << m_iBitsUsed;
		m_uValue |= num2 & (ulValue << m_iBitsUsed);
		m_iBitsUsed += iBits;
	}

	public void Set(int iBits, byte[] bytes)
	{
		ulong num = (ulong)((1L << iBits) - 1);
		for (int i = 0; i < bytes.Length; i++)
		{
			ulong num2 = num << m_iBitsUsed;
			m_uValue |= num2 & ((ulong)bytes[i] << m_iBitsUsed);
			m_iBitsUsed += 8;
		}
	}

	public void Set(int iBits, long lValue)
	{
		ulong num = (ulong)((1L << iBits) - 1);
		ulong num2 = num << m_iBitsUsed;
		bool flag = lValue < 0;
		if (flag)
		{
			lValue *= -1;
		}
		m_uValue |= num2 & (ulong)(lValue << m_iBitsUsed);
		m_iBitsUsed += iBits;
		Set(flag);
	}

	public int GetBitCount()
	{
		return m_iBitsUsed;
	}

	public uint GetUInt(int iBits)
	{
		return (uint)GetULong(iBits);
	}

	public int GetInt(int iBits)
	{
		return (int)GetLong(iBits);
	}

	public byte GetByte(int iBits)
	{
		return (byte)GetULong(iBits);
	}

	public byte[] GetBytes(int iBits)
	{
		byte[] array = new byte[iBits >> 3];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = GetByte(8);
		}
		return array;
	}

	public float GetFloat()
	{
		return BitConverter.ToSingle(GetBytes(32), 0);
	}

	public bool GetBool()
	{
		bool result = (m_uValue & 1) == 1;
		m_uValue >>= 1;
		m_iBitsUsed--;
		if (m_iBitsUsed < 0)
		{
			m_iBitsUsed = 0;
		}
		return result;
	}

	public ulong GetULong(int iBits)
	{
		ulong result = m_uValue & (ulong)((1L << iBits) - 1);
		m_uValue >>= iBits;
		m_iBitsUsed -= iBits;
		if (m_iBitsUsed < 0)
		{
			m_iBitsUsed = 0;
		}
		return result;
	}

	public long GetLong(int iBits)
	{
		long num = (long)m_uValue & ((1L << iBits) - 1);
		m_uValue >>= iBits;
		if ((m_uValue & 1) == 1)
		{
			num *= -1;
		}
		m_uValue >>= 1;
		m_iBitsUsed -= iBits + 1;
		if (m_iBitsUsed < 0)
		{
			m_iBitsUsed = 0;
		}
		return num;
	}

	[Conditional("UNITY_PS4")]
	[Conditional("UNITY_EDITOR")]
	public void ErrorCheck(bool bCheck, string strDescription)
	{
		if (bCheck)
		{
		}
	}
}
