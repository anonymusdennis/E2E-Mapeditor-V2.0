using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class TriggerObjective : BaseObjective
{
	public enum ObjectiveTriggerStateNeeded
	{
		OnTriggerEnter,
		OnTriggerStay,
		OnTriggerLeave
	}

	public enum ObjectiveTriggerPeopleNeeded
	{
		OnePlayer,
		TwoPlayers,
		ThreePlayers,
		FourPlayers,
		AllPlayers,
		OwnerOnly
	}

	public enum ArrowMode
	{
		Objective,
		Routine
	}

	public struct ObjectiveTriggerData
	{
		public ObjectiveTriggerStateNeeded m_ReactionState;

		public ObjectiveTriggerPeopleNeeded m_PeopleNeeded;

		public float m_StayTime;

		public Player m_PlayerOwner;
	}

	public ObjectiveSceneElement m_TriggerReference;

	public ObjectiveTriggerData m_TriggerData;

	public ArrowMode m_ArrowMode;

	private TriggerObjectiveObserver m_Observer;

	private int m_TriggerHUDPin = -1;

	private bool m_bIsCompleted;

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
		m_bIsCompleted = false;
	}

	protected override void Child_Initialize()
	{
		if (m_TriggerReference != null)
		{
			m_Observer = m_TriggerReference.gameObject.AddComponent<TriggerObjectiveObserver>();
			m_Observer.enabled = false;
		}
	}

	protected override void Child_PreAction()
	{
		if (m_Observer != null)
		{
			m_Observer.enabled = true;
			m_TriggerData.m_PlayerOwner = m_PlayerOwner;
			m_Observer.SetTriggerData(m_TriggerData);
		}
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
		return Child_EvaluateStatus();
	}

	protected override bool Child_EvaluateStatus()
	{
		if (m_Observer != null && !m_bIsCompleted)
		{
			m_bIsCompleted = m_Observer.IsCompleted();
		}
		return m_bIsCompleted;
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
		if (m_TriggerReference != null)
		{
			if (on)
			{
				m_TriggerHUDPin = PinManager.GetInstance().CreatePin(bForMainMap: true, bForMiniMap: true, m_TriggerReference.gameObject, null, bUpdatePosition: false, FloorManager.GetInstance().FindFloorAtZ(m_TriggerReference.transform.position.z), new Player[1] { m_PlayerOwner }, PinManager.Pin.PinFilterType.Objectives, edgable: true, floorTrackable: true, directional: false, animation: ObjectiveManager.GetInstance().m_QuestTargetAnimation, toolTipTag: string.Empty);
			}
			else
			{
				PinManager.GetInstance().RemovePin(m_TriggerHUDPin);
				m_TriggerHUDPin = -1;
			}
		}
	}

	protected override void Child_SetHUDArrow(bool on)
	{
		if (!(m_PlayerOwner != null))
		{
			return;
		}
		if (on && m_TriggerReference != null)
		{
			switch (m_ArrowMode)
			{
			case ArrowMode.Objective:
				m_PlayerOwner.SetObjectiveArrowTarget(m_TriggerReference.transform.position, showTargetIndicator: false);
				break;
			case ArrowMode.Routine:
				m_PlayerOwner.SetRoutineArrowTarget(m_TriggerReference.transform.position);
				break;
			}
		}
		else
		{
			switch (m_ArrowMode)
			{
			case ArrowMode.Objective:
				m_PlayerOwner.CancelObjectiveArrow();
				break;
			case ArrowMode.Routine:
				m_PlayerOwner.CancelRoutineArrow();
				break;
			}
		}
	}

	protected override void Child_PostAction()
	{
		if (!m_bIsADependency && m_Observer != null)
		{
			UnityEngine.Object.Destroy(m_Observer);
			m_Observer = null;
		}
	}

	protected override string Child_Save(JObject baseObj, bool ingameSave)
	{
		if (ingameSave)
		{
			baseObj.Add(new JProperty("IsCompleted", m_bIsCompleted));
		}
		if (m_TriggerReference != null)
		{
			baseObj.Add(new JProperty("SceneTrigger", m_TriggerReference.m_ObjectiveElementID));
			baseObj.Add(new JProperty("SceneTrigger_Scene", m_TriggerReference.m_UsedInScene));
		}
		baseObj.Add(new JProperty("ArrowMode", (int)m_ArrowMode));
		baseObj.Add(new JProperty("Data_Trig_State", (int)m_TriggerData.m_ReactionState));
		baseObj.Add(new JProperty("Data_Trig_PeopleNeeded", (int)m_TriggerData.m_PeopleNeeded));
		baseObj.Add(new JProperty("Data_Trig_StayTime", m_TriggerData.m_StayTime));
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
		JProperty jProperty = json.Property("SceneTrigger");
		if (jProperty != null)
		{
			string text = (string)json.Property("SceneTrigger_Scene").Value;
			int id2 = (int)jProperty.Value;
			if ((Application.isPlaying || !(text != SceneManagerHelper.ActiveSceneName)) && Application.isPlaying)
			{
				m_TriggerReference = ObjectiveSceneElement.FindSceneReference(id2);
			}
		}
		if (ingameLoad)
		{
			if (json.Property("IsCompleted") != null)
			{
				m_bIsCompleted = (bool)json.Property("IsCompleted").Value;
			}
			if (m_Observer == null && m_TriggerReference != null)
			{
				m_Observer = m_TriggerReference.gameObject.AddComponent<TriggerObjectiveObserver>();
				m_Observer.enabled = false;
			}
		}
		JProperty jProperty2 = json.Property("ArrowMode");
		if (jProperty2 != null)
		{
			int arrowMode = (int)jProperty2.Value;
			m_ArrowMode = (ArrowMode)arrowMode;
		}
		if (json.Property("Data_Trig_State") != null)
		{
			m_TriggerData.m_ReactionState = (ObjectiveTriggerStateNeeded)(int)json.Property("Data_Trig_State").Value;
		}
		if (json.Property("Data_Trig_PeopleNeeded") != null)
		{
			m_TriggerData.m_PeopleNeeded = (ObjectiveTriggerPeopleNeeded)(int)json.Property("Data_Trig_PeopleNeeded").Value;
		}
		if (json.Property("Data_Trig_StayTime") != null)
		{
			m_TriggerData.m_StayTime = (float)json.Property("Data_Trig_StayTime").Value;
		}
		m_TriggerData.m_PlayerOwner = m_PlayerOwner;
		JProperty jProperty3 = json.Property("ObjectiveToResetTo");
		if (jProperty3 != null)
		{
			m_ObjectiveToResetTo = (int)jProperty3.Value;
		}
		JProperty jProperty4 = json.Property("MustHaveCertainItems");
		if (jProperty4 != null)
		{
			m_bMustHaveCertainItems = (bool)jProperty4.Value;
		}
		JProperty jProperty5 = json.Property("RequiredItems");
		if (jProperty5 == null || jProperty5.Value.Type != JTokenType.Array)
		{
			return;
		}
		m_RequiredItems.Clear();
		JArray source = (JArray)jProperty5.Value;
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
		return ObjectiveType.TriggerObjective;
	}
}
