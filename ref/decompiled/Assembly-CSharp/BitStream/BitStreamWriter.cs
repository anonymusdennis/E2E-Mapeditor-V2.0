using System;
using System.Collections.Generic;

namespace BitStream;

public class BitStreamWriter
{
	private FastList<byte> _targetBuffer;

	private int _remaining;

	public BitStreamWriter(FastList<byte> bufferToWriteTo)
	{
		if (bufferToWriteTo == null)
		{
			throw new ArgumentNullException("bufferToWriteTo");
		}
		_targetBuffer = bufferToWriteTo;
	}

	public void Reset(FastList<byte> bufferToWriteTo)
	{
		_targetBuffer = bufferToWriteTo;
		_remaining = 0;
	}

	public unsafe void Write(float value)
	{
		uint num = 0u;
		uint* ptr = (uint*)(&value);
		num = *ptr;
		Write(num, 32);
	}

	public unsafe void Write(double value)
	{
		ulong num = 0uL;
		ulong* ptr = (ulong*)(&value);
		num = *ptr;
		Write(num, 64);
	}

	public void Write(bool value)
	{
		Write((byte)(value ? byte.MaxValue : 0), 1);
	}

	public void Write(byte[] bytes, int countOfBits)
	{
		if (bytes != null && countOfBits > 0 && countOfBits <= bytes.Length << 3)
		{
			int num = countOfBits / 8;
			int num2 = countOfBits % 8;
			int i;
			for (i = 0; i < num; i++)
			{
				Write(bytes[i], 8);
			}
			if (num2 > 0)
			{
				Write(bytes[i], num2);
			}
		}
	}

	public void Write(ulong bits, int countOfBits)
	{
		if (countOfBits <= 0 || countOfBits > 64)
		{
			return;
		}
		int num = countOfBits / 8;
		int num2 = countOfBits % 8;
		while (num >= 0)
		{
			byte bits2 = (byte)(bits >> num * 8);
			if (num2 > 0)
			{
				Write(bits2, num2);
			}
			if (num > 0)
			{
				num2 = 8;
			}
			num--;
		}
	}

	public void Write(uint bits, int countOfBits)
	{
		if (countOfBits <= 0 || countOfBits > 32)
		{
			return;
		}
		int num = countOfBits / 8;
		int num2 = countOfBits % 8;
		while (num >= 0)
		{
			byte bits2 = (byte)(bits >> num * 8);
			if (num2 > 0)
			{
				Write(bits2, num2);
			}
			if (num > 0)
			{
				num2 = 8;
			}
			num--;
		}
	}

	public void WriteReverse(uint bits, int countOfBits)
	{
		if (countOfBits > 0 && countOfBits <= 32)
		{
			int num = countOfBits / 8;
			int num2 = countOfBits % 8;
			if (num2 > 0)
			{
				num++;
			}
			for (int i = 0; i < num; i++)
			{
				byte bits2 = (byte)(bits >> i * 8);
				Write(bits2, 8);
			}
		}
	}

	public void Write(byte bits, int countOfBits)
	{
		if (countOfBits > 0 && countOfBits <= 8)
		{
			if (_remaining > 0)
			{
				byte b = _targetBuffer[_targetBuffer.Count - 1];
				b = ((countOfBits <= _remaining) ? ((byte)(b | (byte)((bits & (255 >> 8 - countOfBits)) << _remaining - countOfBits))) : ((byte)(b | (byte)((bits & (255 >> 8 - countOfBits)) >> countOfBits - _remaining))));
				_targetBuffer[_targetBuffer.Count - 1] = b;
			}
			if (countOfBits > _remaining)
			{
				_remaining = 8 - (countOfBits - _remaining);
				byte b = (byte)(bits << _remaining);
				_targetBuffer.Add(b);
			}
			else
			{
				_remaining -= countOfBits;
			}
		}
	}

	public void Overwrite(byte bits, int countOfBits, int startingBit)
	{
		if (startingBit + countOfBits <= GetUsedBitCount() && countOfBits > 0 && countOfBits <= 8)
		{
			int num = startingBit / 8;
			int num2 = 8 - startingBit % 8;
			byte b = _targetBuffer[num];
			if (countOfBits > num2)
			{
				b &= (byte)(~(255 >> 8 - countOfBits >> countOfBits - num2));
				b |= (byte)((bits & (255 >> 8 - countOfBits)) >> countOfBits - num2);
			}
			else
			{
				b &= (byte)(~(255 >> 8 - countOfBits << num2 - countOfBits));
				b |= (byte)((bits & (255 >> 8 - countOfBits)) << num2 - countOfBits);
			}
			_targetBuffer[num] = b;
			if (countOfBits > num2)
			{
				b = _targetBuffer[++num];
				num2 = 8 - (countOfBits - num2);
				b &= (byte)(~(255 << num2));
				b |= (byte)(bits << num2);
				_targetBuffer[num] = b;
			}
		}
	}

	public int GetUsedBitCount()
	{
		return _targetBuffer.Count * 8 - _remaining;
	}
}
