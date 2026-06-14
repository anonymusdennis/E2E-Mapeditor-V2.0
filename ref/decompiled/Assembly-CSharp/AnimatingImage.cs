using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimatingImage : MonoBehaviour
{
	[Serializable]
	public struct IconAnimInfo
	{
		public List<Sprite> m_IconAnimationSpriteList;

		public float m_AnimationTime;

		public float m_PauseBetweenAnimationLoops;

		public Sprite m_StillImageSprite;
	}

	public T17Image m_Image;

	private IconAnimInfo m_ActiveInfo;

	private float m_ElapsedAnimationTime;

	private float m_ElapsedPauseTime = 2f;

	private float m_AnimationFrameTime;

	private int m_CurrentFrame;

	public bool m_bIsStillImage;

	private void Awake()
	{
		if (m_Image == null)
		{
			m_Image = GetComponent<T17Image>();
		}
	}

	public void SetAnim(IconAnimInfo activeInfo)
	{
		m_ActiveInfo = activeInfo;
		if (m_Image == null)
		{
			return;
		}
		if (activeInfo.m_IconAnimationSpriteList != null)
		{
			for (int num = activeInfo.m_IconAnimationSpriteList.Count - 1; num >= 0; num--)
			{
				if (m_ActiveInfo.m_IconAnimationSpriteList[num] == null)
				{
					m_ActiveInfo.m_IconAnimationSpriteList.RemoveAt(num);
				}
			}
			if (m_ActiveInfo.m_IconAnimationSpriteList.Count > 0)
			{
				m_AnimationFrameTime = m_ActiveInfo.m_AnimationTime / (float)m_ActiveInfo.m_IconAnimationSpriteList.Count;
				m_ElapsedPauseTime = m_ActiveInfo.m_PauseBetweenAnimationLoops + 1f;
				m_CurrentFrame = 0;
				m_Image.sprite = m_ActiveInfo.m_IconAnimationSpriteList[m_CurrentFrame];
			}
			else
			{
				Debug.LogErrorFormat("IconDisplayHUD: {0} : has no sprites in the animation list!", base.gameObject.name);
			}
		}
		else if (m_bIsStillImage)
		{
			Debug.LogWarningFormat("IconDisplayHUD: {0} : has no sprites in the animation list!", base.gameObject.name);
		}
		else
		{
			Debug.LogErrorFormat("IconDisplayHUD: {0} : has no sprites in the animation list!", base.gameObject.name);
		}
		if (m_bIsStillImage)
		{
			if (m_ActiveInfo.m_StillImageSprite != null)
			{
				m_Image.sprite = m_ActiveInfo.m_StillImageSprite;
			}
			else if (m_ActiveInfo.m_IconAnimationSpriteList != null && m_ActiveInfo.m_IconAnimationSpriteList.Count > 0)
			{
				m_Image.sprite = m_ActiveInfo.m_IconAnimationSpriteList[0];
			}
			else
			{
				m_Image.sprite = null;
			}
		}
	}

	public void Update()
	{
		if (m_bIsStillImage || m_ActiveInfo.m_IconAnimationSpriteList == null)
		{
			return;
		}
		if (m_ElapsedPauseTime >= m_ActiveInfo.m_PauseBetweenAnimationLoops)
		{
			m_ElapsedAnimationTime += UpdateManager.deltaTime;
			if (m_ElapsedAnimationTime >= m_AnimationFrameTime)
			{
				m_ElapsedAnimationTime = 0f;
				m_CurrentFrame++;
				if (m_CurrentFrame >= m_ActiveInfo.m_IconAnimationSpriteList.Count)
				{
					m_CurrentFrame = 0;
					m_ElapsedPauseTime = 0f;
				}
				if (m_ActiveInfo.m_IconAnimationSpriteList.Count > 0)
				{
					m_Image.sprite = m_ActiveInfo.m_IconAnimationSpriteList[m_CurrentFrame];
				}
			}
		}
		else
		{
			m_ElapsedPauseTime += UpdateManager.deltaTime;
		}
	}

	public void CopyFrom(AnimatingImage other)
	{
		SetAnim(other.m_ActiveInfo);
		m_ElapsedAnimationTime = other.m_ElapsedAnimationTime;
		m_ElapsedPauseTime = other.m_ElapsedPauseTime;
		m_AnimationFrameTime = other.m_AnimationFrameTime;
	}
}
