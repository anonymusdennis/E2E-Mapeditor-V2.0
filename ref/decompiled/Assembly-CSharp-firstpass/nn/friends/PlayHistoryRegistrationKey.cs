using System;
using System.Runtime.InteropServices;

namespace nn.friends;

[StructLayout(LayoutKind.Sequential)]
public class PlayHistoryRegistrationKey
{
	public byte[] value = new byte[64];

	public PlayHistoryRegistrationKey()
	{
	}

	public PlayHistoryRegistrationKey(string key)
	{
		if (!string.IsNullOrEmpty(key))
		{
			value = Convert.FromBase64String(key);
		}
	}

	public bool IsValid()
	{
		if (value[0] == 0)
		{
			return false;
		}
		return true;
	}

	public bool Equals(PlayHistoryRegistrationKey other)
	{
		bool result = true;
		for (int i = 0; i < value.Length; i++)
		{
			if (other.value[i] != value[i])
			{
				result = false;
				break;
			}
		}
		return result;
	}

	public override string ToString()
	{
		return Convert.ToBase64String(value);
	}
}
