using System;

public class PRNG
{
	private uint m_ClosestPrime = 4294967291u;

	private uint m_Max;

	private ulong m_Seed;

	private uint m_InitialSeed;

	private bool m_bHasLoopedAround;

	public PRNG(uint max, uint seed)
	{
		m_Max = max;
		m_InitialSeed = seed;
		m_Seed = seed;
		if (m_Seed >= max)
		{
			m_Seed = 0uL;
		}
		m_bHasLoopedAround = false;
		if (m_Max > 2)
		{
			m_ClosestPrime = FindClosestPrime(max);
		}
	}

	public uint GetNextRandom()
	{
		if (m_Max == 0)
		{
			m_bHasLoopedAround = true;
			return 0u;
		}
		if (m_Max <= 2)
		{
			return --m_Max;
		}
		if (m_Seed >= m_ClosestPrime)
		{
			uint result = (uint)m_Seed;
			m_Seed++;
			if (m_Seed >= m_Max)
			{
				m_Seed = 0uL;
			}
			if (m_Seed == m_InitialSeed)
			{
				m_bHasLoopedAround = true;
			}
			return result;
		}
		uint num = (uint)(m_Seed * m_Seed % m_ClosestPrime);
		uint result2 = ((m_Seed > m_ClosestPrime / 2) ? (m_ClosestPrime - num) : num);
		m_Seed++;
		if (m_Seed >= m_Max)
		{
			m_Seed = 0uL;
		}
		if (m_Seed == m_InitialSeed)
		{
			m_bHasLoopedAround = true;
		}
		return result2;
	}

	public void Reset()
	{
		m_Seed = m_InitialSeed;
		m_bHasLoopedAround = false;
	}

	public bool HasLooped()
	{
		return m_bHasLoopedAround;
	}

	public void SetSeed(uint seed)
	{
		m_InitialSeed = seed;
		m_Seed = seed;
		if (m_Seed >= m_Max)
		{
			m_Seed = 0uL;
		}
		m_bHasLoopedAround = false;
	}

	public void SetCeiling(uint ceil)
	{
		m_Max = ceil;
		if (m_Seed >= m_Max)
		{
			m_Seed = 0uL;
		}
		m_bHasLoopedAround = false;
		if (m_Max > 2)
		{
			m_ClosestPrime = FindClosestPrime(m_Max);
		}
	}

	private static uint FindClosestPrime(uint max)
	{
		int listLength = PrimeList.ListLength;
		int num = Array.BinarySearch(PrimeList.PrimeListArray, max);
		if (num >= 0)
		{
			return PrimeList.PrimeListArray[num];
		}
		num = ~num;
		if (num == listLength)
		{
			return max;
		}
		if (num > listLength || num < 0 || num - 1 < 0)
		{
			return 1u;
		}
		return PrimeList.PrimeListArray[num - 1];
	}
}
