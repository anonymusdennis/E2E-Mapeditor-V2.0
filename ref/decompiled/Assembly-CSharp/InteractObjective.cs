using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class InteractObjective : BaseObjective
{
	public enum InteractPerson
	{
		QuestReceiver,
		AnyPlayer
	}

	public InteractPerson m_InteractPerson = InteractPerson.AnyPlayer;

	public ObjectiveSceneElement m_InteractReference;

	public bool m_bSpecificInteraction;

	public int m_InteractionID = -1;

	public bool m_InteractWithQuestGiver;

	public string m_QuestGiverMessage = string.Empty;

	private InteractiveObject m_InteractiveObject;

	private int m_InteractHUDPin = -1;

	private bool m_bInteractedWithObject;

	~InteractObjective()
	{
		if (m_PlayerOwner != null)
		{
			m_PlayerOwner.OnInteractEvent -= OnInteractedWithTarget;
		}
	}

	protected override void Child_PickAllTargets()
	{
		if (m_InteractWithQuestGiver && m_QuestGiver != null)
		{
			NetObjectLock netObjectLock = m_QuestGiver.GetNetObjectLock();
			m_InteractiveObject = netObjectLock.GetFirstInteractionOfType<TalkInteraction>();
			if (m_InteractiveObject == null)
			{
				m_ObjectiveStatus = ObjectiveStatus.Invalid;
			}
		}
		else if (m_InteractReference != null && m_InteractReference.m_LinksTo == ObjectiveSceneElement.ObjectiveSceneElementType.InteractiveObject)
		{
			NetObjectLock component = m_InteractReference.GetComponent<NetObjectLock>();
			if (component != null)
			{
				int interactionID = (m_bSpecificInteraction ? m_InteractionID : 0);
				m_InteractiveObject = component.GetInteractiveObject(interactionID);
			}
			if (m_InteractiveObject == null)
			{
				m_ObjectiveStatus = ObjectiveStatus.Invalid;
			}
		}
		else
		{
			m_ObjectiveStatus = ObjectiveStatus.Invalid;
		}
	}

	protected override void Child_RegisterTokens(ref ObjectiveTree objectiveTree)
	{
	}

	protected override void Child_Reset()
	{
		m_bInteractedWithObject = false;
	}

	protected override void Child_Initialize()
	{
	}

	protected override void Child_PreAction()
	{
		switch (m_InteractPerson)
		{
		case InteractPerson.QuestReceiver:
			m_PlayerOwner.OnInteractEvent += OnInteractedWithTarget;
			break;
		case InteractPerson.AnyPlayer:
		{
			List<Player> allPlayers = Player.GetAllPlayers();
			for (int i = 0; i < allPlayers.Count; i++)
			{
				allPlayers[i].OnInteractEvent += OnInteractedWithTarget;
			}
			break;
		}
		}
		if (!m_QuestGiver.m_bIsRobinsonCharacter || string.IsNullOrEmpty(m_QuestGiverMessage))
		{
			return;
		}
		QuestManager instance = QuestManager.GetInstance();
		if (instance != null)
		{
			QuestManager.QuestGiver questGiver = instance.GetQuestGiver(m_QuestGiver);
			if (questGiver != null)
			{
				questGiver.m_QuestGiverMessage = m_QuestGiverMessage;
			}
		}
	}

	protected override bool Child_EvaluateDependencies()
	{
		return m_bInteractedWithObject;
	}

	protected override bool Child_EvaluateStatus()
	{
		return m_bInteractedWithObject;
	}

	protected override void Child_SetHUDPins(bool on)
	{
		if (!(m_InteractiveObject != null))
		{
			return;
		}
		if (on)
		{
			if (m_InteractWithQuestGiver && m_QuestGiver != null)
			{
				m_QuestGiver.SetPinImage(null, PinManager.Pin.PinFilterType.Objectives, ObjectiveManager.GetInstance().m_QuestTargetAnimation, edgeable: true, floorTrackable: true);
				return;
			}
			m_InteractHUDPin = PinManager.GetInstance().CreatePin(bForMainMap: true, bForMiniMap: true, m_InteractiveObject.gameObject, null, bUpdatePosition: true, FloorManager.GetInstance().FindFloorAtZ(m_InteractiveObject.transform.position.z), new Player[1] { m_PlayerOwner }, PinManager.Pin.PinFilterType.Objectives, edgable: true, floorTrackable: true, directional: false, animation: ObjectiveManager.GetInstance().m_QuestTargetAnimation, toolTipTag: string.Empty);
		}
		else if (m_InteractWithQuestGiver && m_QuestGiver != null)
		{
			m_QuestGiver.ResetPinImage(PinManager.Pin.PinFilterType.Objectives);
		}
		else if (m_InteractHUDPin != -1)
		{
			PinManager.GetInstance().RemovePin(m_InteractHUDPin);
			m_InteractHUDPin = -1;
		}
	}

	protected override void Child_SetHUDArrow(bool on)
	{
		if (m_InteractiveObject != null)
		{
			if (on)
			{
				m_PlayerOwner.SetObjectiveArrowTarget(m_InteractiveObject.m_NetObjectLock.m_NetView);
			}
			else
			{
				m_PlayerOwner.CancelObjectiveArrow();
			}
		}
	}

	protected override void Child_PostAction()
	{
		switch (m_InteractPerson)
		{
		case InteractPerson.QuestReceiver:
			m_PlayerOwner.OnInteractEvent -= OnInteractedWithTarget;
			break;
		case InteractPerson.AnyPlayer:
		{
			List<Player> allPlayers = Player.GetAllPlayers();
			for (int i = 0; i < allPlayers.Count; i++)
			{
				allPlayers[i].OnInteractEvent -= OnInteractedWithTarget;
			}
			break;
		}
		}
	}

	private void OnInteractedWithTarget(InteractiveObject obj)
	{
		if (obj == m_InteractiveObject)
		{
			m_bInteractedWithObject = true;
		}
	}

	protected override string Child_Save(JObject baseObj, bool ingameSave)
	{
		if (ingameSave)
		{
			if (m_InteractiveObject != null)
			{
				baseObj.Add(new JProperty("InteractiveObject", m_InteractiveObject.m_NetObjectLock.m_NetView.viewID));
			}
			baseObj.Add(new JProperty("Interacted", m_bInteractedWithObject));
		}
		if (m_InteractReference != null)
		{
			baseObj.Add(new JProperty("SceneInteractObject", m_InteractReference.m_ObjectiveElementID));
			baseObj.Add(new JProperty("SceneInteractObject_Scene", m_InteractReference.m_UsedInScene));
		}
		baseObj.Add(new JProperty("InteractPerson", (int)m_InteractPerson));
		baseObj.Add(new JProperty("IsSpecific", m_bSpecificInteraction));
		baseObj.Add(new JProperty("InteractionID", m_InteractionID));
		baseObj.Add(new JProperty("InteractWithQuestGiver", m_InteractWithQuestGiver));
		baseObj.Add(new JProperty("QuestGiverMessage", m_QuestGiverMessage));
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject json, bool ingameLoad)
	{
		JProperty jProperty = json.Property("SceneInteractObject");
		if (jProperty != null)
		{
			string text = (string)json.Property("SceneInteractObject_Scene").Value;
			int id = (int)jProperty.Value;
			if ((Application.isPlaying || !(text != SceneManagerHelper.ActiveSceneName)) && Application.isPlaying)
			{
				m_InteractReference = ObjectiveSceneElement.FindSceneReference(id);
			}
		}
		JProperty jProperty2 = json.Property("InteractWithQuestGiver");
		if (jProperty2 != null)
		{
			m_InteractWithQuestGiver = (bool)jProperty2.Value;
		}
		if (ingameLoad)
		{
			if (m_InteractWithQuestGiver)
			{
				if (m_QuestGiver != null)
				{
					NetObjectLock netObjectLock = m_QuestGiver.GetNetObjectLock();
					m_InteractiveObject = netObjectLock.GetFirstInteractionOfType<TalkInteraction>();
					if (m_InteractiveObject == null)
					{
						m_ObjectiveStatus = ObjectiveStatus.Invalid;
					}
				}
			}
			else
			{
				JProperty jProperty3 = json.Property("InteractiveObject");
				if (jProperty3 != null)
				{
					int num = (int)jProperty3.Value;
					if (m_InteractReference != null)
					{
						m_InteractiveObject = m_InteractReference.GetComponentInChildren<InteractiveObject>();
						int viewID = m_InteractiveObject.m_NetObjectLock.m_NetView.viewID;
						if (m_InteractiveObject != null && viewID == num)
						{
						}
					}
				}
			}
			JProperty jProperty4 = json.Property("Interacted");
			if (jProperty4 != null)
			{
				m_bInteractedWithObject = (bool)jProperty4.Value;
			}
		}
		JProperty jProperty5 = json.Property("InteractPerson");
		if (jProperty5 != null)
		{
			int interactPerson = (int)jProperty5.Value;
			m_InteractPerson = (InteractPerson)interactPerson;
		}
		JProperty jProperty6 = json.Property("IsSpecific");
		if (jProperty6 != null)
		{
			m_bSpecificInteraction = (bool)jProperty6.Value;
		}
		JProperty jProperty7 = json.Property("InteractionID");
		if (jProperty7 != null)
		{
			m_InteractionID = (int)jProperty7.Value;
		}
		JProperty jProperty8 = json.Property("QuestGiverMessage");
		if (jProperty8 != null)
		{
			m_QuestGiverMessage = (string)jProperty8.Value;
		}
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.InteractObjective;
	}
}
