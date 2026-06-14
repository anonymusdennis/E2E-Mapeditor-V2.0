using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class WaitUntilTimeObjective : BaseObjective
{
	public enum Mode
	{
		GameTime,
		Routine
	}

	public Mode m_Mode;

	public int m_Hours;

	public int m_Mins;

	public Routines m_Routine = Routines.UNASSIGNED;

	public RoutineSubTypes m_RoutineSubtype;

	private bool m_bWaited;

	public bool m_bMustHaveCertainItems;

	public List<ItemData> m_RequiredItems = new List<ItemData>();

	public int m_ObjectiveToResetTo = -1;

	private ItemContainer m_PlayerItemContainer;

	private ItemContainer m_PlayerDeskItemContainer;

	protected override void Child_PickAllTargets()
	{
	}

	protected override void Child_RegisterTokens(ref ObjectiveTree objectiveTree)
	{
	}

	protected override void Child_Reset()
	{
		m_bWaited = false;
	}

	protected override void Child_Initialize()
	{
	}

	protected override void Child_PreAction()
	{
		if (m_PlayerOwner != null)
		{
			m_PlayerItemContainer = m_PlayerOwner.m_ItemContainer;
			DeskInteraction myDesk = m_PlayerOwner.GetMyDesk();
			if (myDesk != null)
			{
				m_PlayerDeskItemContainer = myDesk.m_LinkedItemContainer;
			}
		}
	}

	protected override bool Child_EvaluateDependencies()
	{
		return m_bWaited;
	}

	protected override bool Child_EvaluateStatus()
	{
		if (!m_bWaited)
		{
			RoutineManager instance = RoutineManager.GetInstance();
			if (instance != null)
			{
				switch (m_Mode)
				{
				case Mode.GameTime:
				{
					int timeHourPart = instance.TimeHourPart;
					if (timeHourPart > m_Hours)
					{
						m_bWaited = true;
					}
					else if (timeHourPart == m_Hours)
					{
						int timeMinutePart = instance.TimeMinutePart;
						m_bWaited = timeMinutePart >= m_Mins;
					}
					break;
				}
				case Mode.Routine:
				{
					RoutinesData.Routine currentRoutine = instance.GetCurrentRoutine();
					if (currentRoutine != null && currentRoutine.m_BaseRoutineType == m_Routine && currentRoutine.m_SubRoutineType == m_RoutineSubtype)
					{
						m_bWaited = true;
					}
					break;
				}
				}
			}
		}
		return m_bWaited;
	}

	protected override int Child_EvaluateResetCondition()
	{
		if (m_bMustHaveCertainItems && m_PlayerOwner != null)
		{
			Item equippedItem = m_PlayerOwner.GetEquippedItem();
			ItemData itemData = null;
			if (equippedItem != null)
			{
				itemData = equippedItem.m_ItemData;
			}
			bool flag = false;
			int i = 0;
			for (int count = m_RequiredItems.Count; i < count; i++)
			{
				if (!(m_RequiredItems[i] == null) && ((itemData != null && itemData.m_ItemDataID == m_RequiredItems[i].m_ItemDataID) || (m_PlayerItemContainer != null && m_PlayerItemContainer.HasItem(m_RequiredItems[i].m_ItemDataID, lookIntoHidden: true) > 0) || (m_PlayerDeskItemContainer != null && m_PlayerDeskItemContainer.HasItem(m_RequiredItems[i].m_ItemDataID, lookIntoHidden: true) > 0)))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return m_ObjectiveToResetTo;
			}
		}
		return -1;
	}

	protected override void Child_SetHUDPins(bool on)
	{
	}

	protected override void Child_SetHUDArrow(bool on)
	{
	}

	protected override void Child_PostAction()
	{
	}

	protected override string Child_Save(JObject baseObj, bool ingameSave)
	{
		if (ingameSave)
		{
			baseObj.Add(new JProperty("Waited", m_bWaited));
		}
		baseObj.Add(new JProperty("Mode", (int)m_Mode));
		baseObj.Add(new JProperty("Hours", m_Hours));
		baseObj.Add(new JProperty("Mins", m_Mins));
		baseObj.Add(new JProperty("Routine", (int)m_Routine));
		baseObj.Add(new JProperty("RoutineSubtype", (int)m_RoutineSubtype));
		baseObj.Add(new JProperty("MustHaveCertainItems", m_bMustHaveCertainItems));
		JProperty jProperty = new JProperty("RequiredItems");
		JArray jArray = new JArray();
		for (int i = 0; i < m_RequiredItems.Count; i++)
		{
			jArray.Add(m_RequiredItems[i].m_ItemDataID);
		}
		jProperty.Add(jArray);
		baseObj.Add(jProperty);
		baseObj.Add(new JProperty("ObjectiveToResetTo", m_ObjectiveToResetTo));
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject json, bool ingameLoad)
	{
		if (ingameLoad)
		{
			JProperty jProperty = json.Property("Waited");
			if (jProperty != null)
			{
				m_bWaited = (bool)jProperty.Value;
			}
		}
		JProperty jProperty2 = json.Property("Mode");
		if (jProperty2 != null)
		{
			int mode = (int)jProperty2.Value;
			m_Mode = (Mode)mode;
		}
		JProperty jProperty3 = json.Property("Hours");
		if (jProperty3 != null)
		{
			m_Hours = (int)jProperty3.Value;
		}
		JProperty jProperty4 = json.Property("Mins");
		if (jProperty4 != null)
		{
			m_Mins = (int)jProperty4.Value;
		}
		JProperty jProperty5 = json.Property("Routine");
		if (jProperty5 != null)
		{
			int routine = (int)jProperty5.Value;
			m_Routine = (Routines)routine;
		}
		JProperty jProperty6 = json.Property("RoutineSubtype");
		if (jProperty6 != null)
		{
			int num = (int)jProperty6.Value;
			m_RoutineSubtype = (RoutineSubTypes)num;
		}
		JProperty jProperty7 = json.Property("ObjectiveToResetTo");
		if (jProperty7 != null)
		{
			m_ObjectiveToResetTo = (int)jProperty7.Value;
		}
		JProperty jProperty8 = json.Property("MustHaveCertainItems");
		if (jProperty8 != null)
		{
			m_bMustHaveCertainItems = (bool)jProperty8.Value;
		}
		JProperty jProperty9 = json.Property("RequiredItems");
		if (jProperty9 == null || jProperty9.Value.Type != JTokenType.Array)
		{
			return;
		}
		m_RequiredItems.Clear();
		JArray source = (JArray)jProperty9.Value;
		List<int> requiredItemIds = source.Select((JToken c) => (int)c).ToList();
		List<ItemData> source2 = Resources.LoadAll<ItemData>("Prefabs/Items").ToList();
		int count = requiredItemIds.Count;
		for (int i = 0; i < count; i++)
		{
			m_RequiredItems.Add(source2.FirstOrDefault((ItemData id) => id.m_ItemDataID == requiredItemIds[i]));
		}
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.WaitUntilTimeObjective;
	}
}
