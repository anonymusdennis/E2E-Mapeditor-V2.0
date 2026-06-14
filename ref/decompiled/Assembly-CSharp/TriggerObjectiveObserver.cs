using UnityEngine;

public class TriggerObjectiveObserver : MonoBehaviour
{
	private TriggerObjective.ObjectiveTriggerData m_Data;

	private bool m_bIsCompleted;

	private int m_CurrentPlayerCount;

	private bool m_bOwnerInTrigger;

	private bool m_bReachedEnoughPeople;

	private bool m_bIncrementStayTimer;

	private float m_ElapsedStayTime;

	public void SetTriggerData(TriggerObjective.ObjectiveTriggerData data)
	{
		m_Data = data;
	}

	private void Update()
	{
		if (m_bIncrementStayTimer)
		{
			m_ElapsedStayTime += UpdateManager.deltaTime;
			if (m_ElapsedStayTime >= m_Data.m_StayTime)
			{
				m_bIsCompleted = true;
			}
		}
	}

	private void FixedUpdate()
	{
		bool flag = false;
		switch (m_Data.m_PeopleNeeded)
		{
		case TriggerObjective.ObjectiveTriggerPeopleNeeded.OnePlayer:
			flag = m_CurrentPlayerCount >= 1;
			break;
		case TriggerObjective.ObjectiveTriggerPeopleNeeded.TwoPlayers:
			flag = m_CurrentPlayerCount >= 2;
			break;
		case TriggerObjective.ObjectiveTriggerPeopleNeeded.ThreePlayers:
			flag = m_CurrentPlayerCount >= 3;
			break;
		case TriggerObjective.ObjectiveTriggerPeopleNeeded.FourPlayers:
			flag = m_CurrentPlayerCount >= 4;
			break;
		case TriggerObjective.ObjectiveTriggerPeopleNeeded.AllPlayers:
			flag = m_CurrentPlayerCount >= Player.GetAllPlayers().Count;
			break;
		case TriggerObjective.ObjectiveTriggerPeopleNeeded.OwnerOnly:
			flag = m_bOwnerInTrigger;
			break;
		}
		if (flag)
		{
			switch (m_Data.m_ReactionState)
			{
			case TriggerObjective.ObjectiveTriggerStateNeeded.OnTriggerEnter:
				m_bIsCompleted = true;
				break;
			case TriggerObjective.ObjectiveTriggerStateNeeded.OnTriggerStay:
				m_bIncrementStayTimer = true;
				break;
			case TriggerObjective.ObjectiveTriggerStateNeeded.OnTriggerLeave:
				m_bReachedEnoughPeople = true;
				break;
			}
		}
		else if (m_bReachedEnoughPeople && m_CurrentPlayerCount == 0)
		{
			m_bIsCompleted = true;
		}
		else
		{
			m_bIncrementStayTimer = false;
			m_ElapsedStayTime = 0f;
		}
		m_CurrentPlayerCount = 0;
		m_bOwnerInTrigger = false;
	}

	private void OnTriggerStay(Collider colStay)
	{
		if (!base.enabled)
		{
			return;
		}
		Player component = colStay.transform.parent.GetComponent<Player>();
		if (component != null)
		{
			m_CurrentPlayerCount++;
			if (object.ReferenceEquals(component, m_Data.m_PlayerOwner))
			{
				m_bOwnerInTrigger = true;
			}
		}
	}

	public bool IsCompleted()
	{
		return m_bIsCompleted;
	}

	public int PeopleInTrigger()
	{
		return m_CurrentPlayerCount;
	}

	public bool HasReachedNeededPeople()
	{
		return m_bReachedEnoughPeople;
	}

	public float GetElapsedStayTime()
	{
		return m_ElapsedStayTime;
	}

	public bool IsElapsingStayTime()
	{
		return m_bIncrementStayTimer;
	}
}
