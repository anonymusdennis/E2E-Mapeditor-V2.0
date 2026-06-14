using System.Collections.Generic;

namespace DataHelpers;

public class ByteArrayConversion
{
	public static void StoreLong(long value, ref List<byte> bytes, ref int iIndex)
	{
		bytes[iIndex++] = (byte)value;
		bytes[iIndex++] = (byte)(value >> 8);
		bytes[iIndex++] = (byte)(value >> 16);
		bytes[iIndex++] = (byte)(value >> 24);
		bytes[iIndex++] = (byte)(value >> 32);
		bytes[iIndex++] = (byte)(value >> 40);
		bytes[iIndex++] = (byte)(value >> 48);
		bytes[iIndex++] = (byte)(value >> 56);
	}

	public static void AddLong(long value, ref List<byte> bytes)
	{
		bytes.Add((byte)value);
		bytes.Add((byte)(value >> 8));
		bytes.Add((byte)(value >> 16));
		bytes.Add((byte)(value >> 24));
		bytes.Add((byte)(value >> 32));
		bytes.Add((byte)(value >> 40));
		bytes.Add((byte)(value >> 48));
		bytes.Add((byte)(value >> 56));
	}

	public static long GetLong(ref List<byte> bytes, ref int iIndex)
	{
		long result = ((long)(int)bytes[iIndex + 7] << 56) + ((long)(int)bytes[iIndex + 6] << 48) + ((long)(int)bytes[iIndex + 5] << 40) + ((long)(int)bytes[iIndex + 4] << 32) + ((long)(int)bytes[iIndex + 3] << 24) + ((long)(int)bytes[iIndex + 2] << 16) + ((long)(int)bytes[iIndex + 1] << 8) + (int)bytes[iIndex];
		iIndex += 8;
		return result;
	}

	public static void StoreULong(ulong value, ref List<byte> bytes, ref int iIndex)
	{
		bytes[iIndex++] = (byte)value;
		bytes[iIndex++] = (byte)(value >> 8);
		bytes[iIndex++] = (byte)(value >> 16);
		bytes[iIndex++] = (byte)(value >> 24);
		bytes[iIndex++] = (byte)(value >> 32);
		bytes[iIndex++] = (byte)(value >> 40);
		bytes[iIndex++] = (byte)(value >> 48);
		bytes[iIndex++] = (byte)(value >> 56);
	}

	public static void AddULong(ulong value, ref List<byte> bytes)
	{
		bytes.Add((byte)value);
		bytes.Add((byte)(value >> 8));
		bytes.Add((byte)(value >> 16));
		bytes.Add((byte)(value >> 24));
		bytes.Add((byte)(value >> 32));
		bytes.Add((byte)(value >> 40));
		bytes.Add((byte)(value >> 48));
		bytes.Add((byte)(value >> 56));
	}

	public static ulong GetULong(ref List<byte> bytes, ref int iIndex)
	{
		ulong result = ((ulong)bytes[iIndex + 7] << 56) + ((ulong)bytes[iIndex + 6] << 48) + ((ulong)bytes[iIndex + 5] << 40) + ((ulong)bytes[iIndex + 4] << 32) + ((ulong)bytes[iIndex + 3] << 24) + ((ulong)bytes[iIndex + 2] << 16) + ((ulong)bytes[iIndex + 1] << 8) + bytes[iIndex];
		iIndex += 8;
		return result;
	}

	public static void StoreInt(int value, ref List<byte> bytes, ref int iIndex)
	{
		bytes[iIndex++] = (byte)value;
		bytes[iIndex++] = (byte)(value >> 8);
		bytes[iIndex++] = (byte)(value >> 16);
		bytes[iIndex++] = (byte)(value >> 24);
	}

	public static void AddInt(int value, ref List<byte> bytes)
	{
		bytes.Add((byte)value);
		bytes.Add((byte)(value >> 8));
		bytes.Add((byte)(value >> 16));
		bytes.Add((byte)(value >> 24));
	}

	public static int GetInt(ref List<byte> bytes, ref int iIndex)
	{
		int result = (bytes[iIndex + 3] << 24) + (bytes[iIndex + 2] << 16) + (bytes[iIndex + 1] << 8) + bytes[iIndex];
		iIndex += 4;
		return result;
	}

	public static void StoreUInt(uint value, ref List<byte> bytes, ref int iIndex)
	{
		bytes[iIndex++] = (byte)value;
		bytes[iIndex++] = (byte)(value >> 8);
		bytes[iIndex++] = (byte)(value >> 16);
		bytes[iIndex++] = (byte)(value >> 24);
	}

	public static void AddUInt(uint value, ref List<byte> bytes)
	{
		bytes.Add((byte)value);
		bytes.Add((byte)(value >> 8));
		bytes.Add((byte)(value >> 16));
		bytes.Add((byte)(value >> 24));
	}

	public static uint GetUInt(ref List<byte> bytes, ref int iIndex)
	{
		uint result = (uint)((bytes[iIndex + 3] << 24) + (bytes[iIndex + 2] << 16) + (bytes[iIndex + 1] << 8) + bytes[iIndex]);
		iIndex += 4;
		return result;
	}

	public static void StoreShort(short value, ref List<byte> bytes, ref int iIndex)
	{
		bytes[iIndex++] = (byte)value;
		bytes[iIndex++] = (byte)(value >> 8);
	}

	public static void AddShort(short value, ref List<byte> bytes)
	{
		bytes.Add((byte)value);
		bytes.Add((byte)(value >> 8));
	}

	public static short GetShort(ref List<byte> bytes, ref int iIndex)
	{
		short result = (short)((bytes[iIndex + 1] << 8) + bytes[iIndex]);
		iIndex += 2;
		return result;
	}

	public static void StoreUShort(ushort value, ref List<byte> bytes, ref int iIndex)
	{
		bytes[iIndex++] = (byte)value;
		bytes[iIndex++] = (byte)(value >> 8);
	}

	public static void AddUShort(ushort value, ref List<byte> bytes)
	{
		bytes.Add((byte)value);
		bytes.Add((byte)(value >> 8));
	}

	public static ushort GetUShort(ref List<byte> bytes, ref int iIndex)
	{
		ushort result = (ushort)((bytes[iIndex + 1] << 8) + bytes[iIndex]);
		iIndex += 2;
		return result;
	}

	public static void StoreFloat(float value, ref List<byte> bytes, ref int iIndex)
	{
		FloatIntConversion floatIntConversion = default(FloatIntConversion);
		floatIntConversion.m_ValueFloat = value;
		StoreInt((int)floatIntConversion.m_ValueInt, ref bytes, ref iIndex);
	}

	public static void AddFloat(float value, ref List<byte> bytes)
	{
		FloatIntConversion floatIntConversion = default(FloatIntConversion);
		floatIntConversion.m_ValueFloat = value;
		AddInt((int)floatIntConversion.m_ValueInt, ref bytes);
	}

	public static float GetFloat(ref List<byte> bytes, ref int iIndex)
	{
		FloatIntConversion floatIntConversion = default(FloatIntConversion);
		floatIntConversion.m_ValueInt = (uint)GetInt(ref bytes, ref iIndex);
		return floatIntConversion.m_ValueFloat;
	}

	public static void AddString(string value, ref List<byte> bytes, bool bEncript = false)
	{
		if (string.IsNullOrEmpty(value))
		{
			AddInt(0, ref bytes);
			return;
		}
		char[] array = value.ToCharArray();
		int num = array.Length;
		AddInt(num, ref bytes);
		int count = bytes.Count;
		for (int i = 0; i < num; i++)
		{
			bytes.Add((byte)array[i]);
			bytes.Add((byte)((int)array[i] >> 8));
		}
		int count2 = bytes.Count;
		if (bEncript)
		{
			byte b = 99;
			for (int j = count; j < count2; j++)
			{
				bytes[j] = (byte)((uint)(bytes[j] + b) ^ 0x7Bu);
				b = bytes[j];
			}
		}
	}

	public static string GetString(ref List<byte> bytes, ref int iIndex, bool bEncript = false)
	{
		int @int = GetInt(ref bytes, ref iIndex);
		if (@int == 0)
		{
			return string.Empty;
		}
		char[] array = new char[@int];
		byte b = 99;
		byte b2 = 0;
		byte b3 = 0;
		for (int i = 0; i < @int; i++)
		{
			b2 = bytes[iIndex++];
			b3 = bytes[iIndex++];
			if (bEncript)
			{
				byte b4 = b2;
				b2 = (byte)((b2 ^ 0x7B) - b);
				b = b3;
				b3 = (byte)((b3 ^ 0x7B) - b4);
			}
			array[i] = (char)((b3 << 8) + b2);
		}
		return new string(array);
	}
}
