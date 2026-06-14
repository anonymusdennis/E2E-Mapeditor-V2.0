using System;
using UnityEngine;

public class ObjectiveSubGoalHUD : MonoBehaviour
{
	public T17Text m_Info;

	public T17Image m_BackgroundTick;

	public Animator m_TickAnim;

	private Player m_Player;

	private BaseObjective m_BoundObjective;

	private bool m_bAnimating;

	private bool m_bPlayRequested;

	private float m_ElapsedTickTime;

	public float ElapsedTimeAfterTick => (!m_bAnimating) ? (-1f) : m_ElapsedTickTime;

	private void Start()
	{
	}

	private void Update()
	{
		if (m_Player == null || InGameMenuFlow.Instance.AnyMenusOpen(m_Player.m_PlayerCameraManagerBindingID))
		{
			return;
		}
		if (m_bAnimating)
		{
			m_ElapsedTickTime += UpdateManager.deltaTime;
		}
		if (m_bPlayRequested)
		{
			m_bPlayRequested = false;
			if (m_TickAnim != null && !m_bAnimating)
			{
				m_ElapsedTickTime = 0f;
				m_TickAnim.gameObject.SetActive(value: true);
				m_TickAnim.Play("HUD_Tick", 0);
				m_bAnimating = true;
			}
		}
	}

	public void Show(Player player)
	{
		m_Player = player;
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		if (base.gameObject.activeSelf)
		{
			ResetTick();
			base.gameObject.SetActive(value: false);
			SetObjective(null);
		}
	}

	public void ResetTick()
	{
		m_bAnimating = false;
		m_ElapsedTickTime = 0f;
		m_bPlayRequested = false;
		if (m_TickAnim != null && m_TickAnim.gameObject.activeSelf)
		{
			m_TickAnim.Play("Complete", 0);
			m_TickAnim.gameObject.SetActive(value: false);
		}
	}

	public void PlayTick()
	{
		m_bPlayRequested = true;
	}

	public void SetObjective(BaseObjective objective)
	{
		if (objective != m_BoundObjective && m_BoundObjective != null)
		{
			BaseObjective boundObjective = m_BoundObjective;
			boundObjective.OnObjectiveComplete = (BaseObjective.ObjectiveEvent)Delegate.Remove(boundObjective.OnObjectiveComplete, new BaseObjective.ObjectiveEvent(PlayTick));
		}
		m_BoundObjective = objective;
		if (m_BoundObjective != null)
		{
			BaseObjective boundObjective2 = m_BoundObjective;
			boundObjective2.OnObjectiveComplete = (BaseObjective.ObjectiveEvent)Delegate.Remove(boundObjective2.OnObjectiveComplete, new BaseObjective.ObjectiveEvent(PlayTick));
			BaseObjective boundObjective3 = m_BoundObjective;
			boundObjective3.OnObjectiveComplete = (BaseObjective.ObjectiveEvent)Delegate.Combine(boundObjective3.OnObjectiveComplete, new BaseObjective.ObjectiveEvent(PlayTick));
			string localizedDescription = m_BoundObjective.LocalizedDescription;
			m_Info.m_bNeedsLocalization = false;
			m_Info.text = localizedDescription;
		}
	}
}
