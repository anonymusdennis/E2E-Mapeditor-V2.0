using System;
using System.Collections.Generic;

public class BarEventManager : EventManager
{
	public AIEventData m_SheetOnBars;

	private AIEvent m_SheetOnBarsEvent;

	private bool m_IsCovered;

	private bool m_IsHighStarRating;

	private bool m_IsCoveredVisible;

	private List<AIEvent> m_VisibleEvents = new List<AIEvent>();

	private int m_TakeDownSheetAlertness = 11;

	private int m_FloorIndex;

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		if (ConfigManager.GetInstance() == null || ConfigManager.GetInstance().aiConfig == null)
		{
			return T17BehaviourManager.INITSTATE.IS_DEPS;
		}
		m_SheetOnBars = ConfigManager.GetInstance().ApplyAIEventOverride(m_SheetOnBars);
		m_TakeDownSheetAlertness = (int)ConfigManager.GetInstance().aiConfig.GetTakeDownBedSheetAlertness();
		m_FloorIndex = FloorManager.GetInstance().FindFloorIndexAtZ(base.transform.position.z);
		m_TargetOffsets.Add(Direction.m_vUp * 0.6f);
		m_TargetOffsets.Add(Direction.m_vDown * 0.6f);
		m_TargetOffsets.Add(Direction.m_vLeft * 0.6f);
		m_TargetOffsets.Add(Direction.m_vRight * 0.6f);
		PrisonAlertnessManager.GetInstance().OnPrisonAlertnessChanged += PrisonAlertnessChanged;
		if (AIEventManager.GetInstance().SetupDone())
		{
			Setup();
		}
		else
		{
			AIEventManager instance = AIEventManager.GetInstance();
			instance.m_AIEventDataSetup = (AIEventManager.AIEventDataSetup)Delegate.Combine(instance.m_AIEventDataSetup, new AIEventManager.AIEventDataSetup(Setup));
		}
		return base.StartInit();
	}

	public void Setup()
	{
		GlobalStart.EnteredLevelEvent -= Setup;
		GetEventByType(AIEvent.EventType.Bars_Covered);
		T17NetManager.OnBecameMasterClient += UpdateEventVisibility;
	}

	protected override void OnManagerDestroyed()
	{
		T17NetManager.OnBecameMasterClient -= UpdateEventVisibility;
	}

	public void PrisonAlertnessChanged(PrisonAlertness alertness)
	{
		bool highStarRating = (int)alertness >= m_TakeDownSheetAlertness;
		IsHighStarRating(highStarRating);
	}

	private void IsHighStarRating(bool highStarRating)
	{
		if (m_IsHighStarRating != highStarRating)
		{
			m_IsHighStarRating = highStarRating;
			UpdateEventVisibility();
		}
	}

	public void IsCovered(bool isCovered)
	{
		if (m_IsCovered != isCovered)
		{
			m_IsCovered = isCovered;
			UpdateEventVisibility();
		}
	}

	public void UpdateEventVisibility()
	{
		if (!T17NetManager.IsMasterClient || m_SheetOnBarsEvent == null)
		{
			return;
		}
		bool flag = m_IsHighStarRating && m_IsCovered;
		if (flag && !m_IsCoveredVisible)
		{
			m_IsCoveredVisible = true;
			m_VisibleEvents.Add(m_SheetOnBarsEvent);
			m_SheetOnBarsEvent.OnEventStarted();
			AIEventManager.GetInstance().UpdatePosition(this, m_FloorIndex);
		}
		else if (!flag && m_IsCoveredVisible)
		{
			m_IsCoveredVisible = false;
			m_VisibleEvents.Remove(m_SheetOnBarsEvent);
			m_SheetOnBarsEvent.OnEventStopped();
			if (m_VisibleEvents.Count == 0)
			{
				AIEventManager.GetInstance().RemoveManager(this);
			}
		}
	}

	public override uint GetEventManagerID()
	{
		return AIEventManager.GetEventManagerIDForNetObject(m_NetView.viewID);
	}

	public override List<AIEvent> GetVisibleEvents()
	{
		return m_VisibleEvents;
	}

	public override AIEvent GetEventByType(AIEvent.EventType eventType)
	{
		if (eventType == AIEvent.EventType.Bars_Covered)
		{
			if (m_SheetOnBarsEvent == null)
			{
				m_SheetOnBarsEvent = new AIEvent(m_SheetOnBars, null, null, base.gameObject, this);
			}
			return m_SheetOnBarsEvent;
		}
		return null;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		AIEventManager instance = AIEventManager.GetInstance();
		if (instance != null)
		{
			instance.m_AIEventDataSetup = (AIEventManager.AIEventDataSetup)Delegate.Remove(instance.m_AIEventDataSetup, new AIEventManager.AIEventDataSetup(Setup));
		}
	}
}
