using System.Collections.Generic;
using UnityEngine;

public class AnimSyncher : MonoBehaviour
{
	public enum SyncState
	{
		WaitingToEnable,
		EnableAnimators,
		Finished
	}

	private static AnimSyncher c_TheInstance;

	public List<Animator> m_Animators;

	public SyncState m_SyncState;

	public static AnimSyncher instance => c_TheInstance;

	private void Awake()
	{
		c_TheInstance = this;
		m_SyncState = SyncState.WaitingToEnable;
	}

	protected virtual void OnDestroy()
	{
		if (c_TheInstance == this)
		{
			c_TheInstance = null;
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (m_Animators == null)
		{
			m_SyncState = SyncState.Finished;
		}
		switch (m_SyncState)
		{
		case SyncState.WaitingToEnable:
			m_SyncState = SyncState.EnableAnimators;
			break;
		case SyncState.EnableAnimators:
		{
			int i = 0;
			for (int count = m_Animators.Count; i < count; i++)
			{
				m_Animators[i].SetBool("On", value: true);
				m_Animators[i].Play("Idle", 0, 0f);
			}
			m_SyncState = SyncState.Finished;
			break;
		}
		case SyncState.Finished:
			base.enabled = false;
			break;
		}
	}

	public bool DoesContain(Animator theAnimator)
	{
		if (m_Animators != null && m_Animators.Count > 0)
		{
			return m_Animators.Contains(theAnimator);
		}
		return false;
	}
}
