using UnityEngine;

public abstract class BaseIngamePassiveUI : T17MonoBehaviour
{
	[HideInInspector]
	protected Player m_LinkedPlayer;

	private bool m_bAlreadyInitialised;

	public virtual bool Init(Player owner)
	{
		if (!m_bAlreadyInitialised || owner != m_LinkedPlayer)
		{
			m_LinkedPlayer = owner;
			m_bAlreadyInitialised = true;
			return true;
		}
		return false;
	}

	protected bool IsAlreadyInitialised()
	{
		return m_bAlreadyInitialised;
	}

	protected virtual void OnDestroy()
	{
		m_LinkedPlayer = null;
	}
}
