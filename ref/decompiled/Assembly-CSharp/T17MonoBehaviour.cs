using UnityEngine;

public abstract class T17MonoBehaviour : MonoBehaviour
{
	private bool m_bBehaviourInit;

	protected virtual void Awake()
	{
	}

	public bool IsInited()
	{
		return m_bBehaviourInit;
	}

	public virtual T17BehaviourManager.INITSTATE StartInit()
	{
		m_bBehaviourInit = true;
		return T17BehaviourManager.INITSTATE.IS_FINISHED;
	}

	public virtual void StartInitLast()
	{
	}
}
