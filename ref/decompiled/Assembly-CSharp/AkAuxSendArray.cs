using System;
using System.Runtime.InteropServices;

public class AkAuxSendArray
{
	private const int MAX_COUNT = 4;

	private const int SIZE_OF_AKAUXSENDVALUE = 8;

	public IntPtr m_Buffer;

	public uint m_Count;

	public bool isFull => m_Count >= 4 || m_Buffer == IntPtr.Zero;

	public AkAuxSendArray()
	{
		m_Buffer = Marshal.AllocHGlobal(32);
		m_Count = 0u;
	}

	~AkAuxSendArray()
	{
		Marshal.FreeHGlobal(m_Buffer);
		m_Buffer = IntPtr.Zero;
	}

	private IntPtr GetObjectPtr(uint index)
	{
		return (IntPtr)(m_Buffer.ToInt64() + 8 * index);
	}

	public void Reset()
	{
		m_Count = 0u;
	}

	public void Add(uint in_EnvID, float in_fValue)
	{
		if (!isFull)
		{
			AkSoundEnginePINVOKE.CSharp_AkAuxSendValueProxy_set(GetObjectPtr(m_Count), in_EnvID, in_fValue);
			m_Count++;
		}
	}

	public bool Contains(uint in_EnvID)
	{
		if (m_Buffer == IntPtr.Zero)
		{
			return false;
		}
		for (uint num = 0u; num < m_Count; num++)
		{
			if (in_EnvID == AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_auxBusID_get(GetObjectPtr(num)))
			{
				return true;
			}
		}
		return false;
	}
}
