using System;
using UnityEngine;

[Serializable]
public class SpriteAnimation
{
	public Sprite[] m_Sprites = new Sprite[0];

	public float m_FrameDuration;

	public float m_LoopDelay;

	private float m_FrameTimer;

	private int m_FrameIndex;

	public Sprite currentSprite => (m_FrameIndex >= m_Sprites.Length) ? null : m_Sprites[m_FrameIndex];

	public SpriteAnimation()
	{
	}

	public SpriteAnimation(SpriteAnimation other)
	{
		m_Sprites = other.m_Sprites;
		m_FrameDuration = other.m_FrameDuration;
		m_LoopDelay = other.m_LoopDelay;
		m_FrameTimer = GetFrameDuration(0);
		m_FrameIndex = 0;
	}

	public void Update(float deltaTime)
	{
		m_FrameTimer -= deltaTime;
		if (!(m_FrameTimer <= 0f))
		{
			return;
		}
		int num = m_FrameIndex;
		if (m_FrameDuration > 0f)
		{
			float num2 = Mathf.Abs(m_FrameTimer);
			while (num2 >= 0f)
			{
				num2 -= GetFrameDuration(num);
				num++;
				if (num >= m_Sprites.Length)
				{
					num = 0;
				}
			}
		}
		else
		{
			num++;
			if (num >= m_Sprites.Length)
			{
				num = 0;
			}
		}
		m_FrameIndex = num;
		m_FrameTimer = GetFrameDuration(num);
	}

	private float GetFrameDuration(int index)
	{
		if (m_FrameIndex >= m_Sprites.Length - 1)
		{
			return m_FrameDuration + m_LoopDelay;
		}
		return m_FrameDuration;
	}
}
