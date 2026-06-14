using System.Collections.Generic;
using UnityEngine;

public class DogAnimator : CharacterAnimator
{
	public static Dictionary<AnimState, int> m_AnimStateToDogIndex;

	protected override void SetUpAnimStateLookup()
	{
		if (m_AnimStateToDogIndex != null || !(m_CharacterAnimator != null))
		{
			return;
		}
		m_AnimStateToDogIndex = new Dictionary<AnimState, int>(CharacterAnimator.AnimStateTComparer);
		for (int i = 0; i < 150; i++)
		{
			AnimState key = (AnimState)i;
			string text = key.ToString();
			int num = Animator.StringToHash(text);
			if (m_CharacterAnimator.HasState(0, num))
			{
				m_AnimStateToDogIndex.Add(key, num);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (m_AnimStateToDogIndex != null)
		{
			m_AnimStateToDogIndex.Clear();
			m_AnimStateToDogIndex = null;
		}
	}

	protected override void UpdateState()
	{
		int value = -1;
		if (m_AnimStateToDogIndex.TryGetValue(m_eAnimState, out value) && m_CharacterAnimator != null)
		{
			m_CharacterAnimator.CrossFade(value, 0f, 0, 0f);
		}
	}
}
