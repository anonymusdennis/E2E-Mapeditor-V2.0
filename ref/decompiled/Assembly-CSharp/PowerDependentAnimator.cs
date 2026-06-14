using UnityEngine;

public class PowerDependentAnimator : T17MonoBehaviour
{
	public string m_AnimatorPowerActiveBool = "IsPowerActive";

	public Animator m_Animator;

	protected override void Awake()
	{
		base.Awake();
		if (m_Animator == null)
		{
			m_Animator = GetComponentInChildren<Animator>(includeInactive: true);
		}
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		base.StartInit();
		PrisonPowerManager instance = PrisonPowerManager.GetInstance();
		if (instance != null)
		{
			instance.PowerChangedEvent += PowerManager_PowerChangedEvent;
			SetAnimatorPowerActive(instance.PowerIsActive());
		}
		return T17BehaviourManager.INITSTATE.IS_FINISHED;
	}

	protected virtual void OnDestroy()
	{
		if (PrisonPowerManager.GetInstance() != null)
		{
			PrisonPowerManager.GetInstance().PowerChangedEvent -= PowerManager_PowerChangedEvent;
		}
	}

	private void PowerManager_PowerChangedEvent(PrisonPowerManager sender, bool isPowerActive)
	{
		SetAnimatorPowerActive(isPowerActive);
	}

	private void SetAnimatorPowerActive(bool state)
	{
		if (m_Animator != null)
		{
			m_Animator.SetBool(m_AnimatorPowerActiveBool, state);
		}
	}
}
