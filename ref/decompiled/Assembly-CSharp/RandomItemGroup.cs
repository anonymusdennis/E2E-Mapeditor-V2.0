using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RandomItemGroup : T17MonoBehaviour
{
	public List<ItemData> m_Items = new List<ItemData>();

	[ReadOnly]
	public int m_RandomItemGroupID = 999;

	private int[] m_PoolIndices;

	private int m_NextPoolIndex = -1;

	public ItemData GetRandomItem(bool bUniqueItems)
	{
		if (m_Items.Count > 0)
		{
			if (bUniqueItems)
			{
				if (m_PoolIndices == null || m_PoolIndices.Length <= 0)
				{
					m_PoolIndices = new int[m_Items.Count];
					for (int i = 0; i < m_Items.Count; i++)
					{
						m_PoolIndices[i] = i;
					}
					for (int i = 0; i < 100; i++)
					{
						int num = UnityEngine.Random.Range(0, m_Items.Count);
						int num2 = UnityEngine.Random.Range(0, m_Items.Count);
						if (num != num2)
						{
							int num3 = m_PoolIndices[num];
							m_PoolIndices[num] = m_PoolIndices[num2];
							m_PoolIndices[num2] = num3;
						}
					}
					m_NextPoolIndex = 0;
				}
				if (m_NextPoolIndex >= m_PoolIndices.Length)
				{
					m_NextPoolIndex = 0;
				}
				int index = m_PoolIndices[m_NextPoolIndex];
				m_NextPoolIndex++;
				if (m_NextPoolIndex == m_Items.Count)
				{
					m_PoolIndices = null;
				}
				return m_Items[index];
			}
			int index2 = UnityEngine.Random.Range(0, m_Items.Count);
			return m_Items[index2];
		}
		return null;
	}

	protected override void Awake()
	{
		base.Awake();
	}
}
