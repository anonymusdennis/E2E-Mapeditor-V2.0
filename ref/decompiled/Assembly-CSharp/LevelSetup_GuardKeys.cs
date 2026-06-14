using System.Collections.Generic;
using UnityEngine;

public class LevelSetup_GuardKeys : BaseComponentSetup
{
	[Tooltip("The parents of all the layers we wish to search for guards")]
	public Transform[] m_Roots = new Transform[0];

	[Tooltip("What keys should be spread out between Guards.")]
	public ItemData[] m_Keys = new ItemData[0];

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_5;
	}

	public override SetupReturnState Setup()
	{
		int num = m_Roots.Length;
		if (num == 0)
		{
			return FinishedAndRemove();
		}
		int num2 = m_Keys.Length;
		if (num2 == 0)
		{
			return FinishedAndRemove();
		}
		List<AICharacter_Guard> list = new List<AICharacter_Guard>();
		for (int i = 0; i < num; i++)
		{
			if (!(m_Roots[i] == null))
			{
				list.AddRange(m_Roots[i].GetComponentsInChildren<AICharacter_Guard>(includeInactive: true));
			}
		}
		int count = list.Count;
		if (count == 0)
		{
			return FinishedAndRemove();
		}
		int num3 = 0;
		int num4 = 0;
		int num5 = Mathf.Max(count, num2);
		for (int j = 0; j < num5; j++)
		{
			if (!(list[num4].m_ItemContainer == null))
			{
				list[num4].m_ItemContainer.m_TrackedItems.Add(m_Keys[num3]);
				num3 = (num3 + 1) % num2;
			}
			num4 = (num4 + 1) % count;
		}
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		return Setup();
	}
}
