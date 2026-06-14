using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class EnableInteractionObjective : BaseObjective
{
	public ObjectiveSceneElement m_ObjectSceneRef;

	public int m_InteractionID = -1;

	public bool m_bNewInteractionState = true;

	public bool m_bRestoreStateWhenFinished;

	private InteractiveObject m_InteractiveObject;

	private NetObjectLock m_NetObjectLock;

	private bool m_bOriginalState;

	~EnableInteractionObjective()
	{
		if (m_NetObjectLock != null)
		{
			if (m_NetObjectLock.IsLocked() && m_NetObjectLock.m_NetView != null)
			{
				m_NetObjectLock.ReleaseLock();
			}
			m_NetObjectLock = null;
		}
		m_InteractiveObject = null;
	}

	protected override void Child_PickAllTargets()
	{
		if (m_ObjectSceneRef != null && m_ObjectSceneRef.m_LinksTo == ObjectiveSceneElement.ObjectiveSceneElementType.InteractiveObject)
		{
			m_NetObjectLock = m_ObjectSceneRef.GetComponent<NetObjectLock>();
			if (m_NetObjectLock != null)
			{
				m_InteractiveObject = m_NetObjectLock.GetInteractiveObject(m_InteractionID);
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
	}

	protected override void Child_Initialize()
	{
	}

	protected override void Child_PreAction()
	{
		if (m_InteractiveObject != null)
		{
			m_bOriginalState = m_InteractiveObject.IsEnabled();
			m_InteractiveObject.SetEnabled(m_bNewInteractionState);
		}
	}

	protected override bool Child_EvaluateDependencies()
	{
		return true;
	}

	protected override bool Child_EvaluateStatus()
	{
		return true;
	}

	protected override void Child_PostAction()
	{
		if (m_bRestoreStateWhenFinished && m_InteractiveObject != null)
		{
			m_InteractiveObject.SetEnabled(m_bOriginalState);
		}
	}

	protected override void Child_SetHUDPins(bool on)
	{
	}

	protected override void Child_SetHUDArrow(bool on)
	{
	}

	protected override string Child_Save(JObject baseObj, bool ingameSave)
	{
		if (ingameSave)
		{
		}
		if (m_ObjectSceneRef != null)
		{
			baseObj.Add(new JProperty("ObjectTargetRef", m_ObjectSceneRef.m_ObjectiveElementID));
			baseObj.Add(new JProperty("ObjectTargetRef_Scene", m_ObjectSceneRef.m_UsedInScene));
		}
		baseObj.Add(new JProperty("InteractionID", m_InteractionID));
		baseObj.Add(new JProperty("NewState", m_bNewInteractionState));
		baseObj.Add(new JProperty("RestoreWhenFinished", m_bRestoreStateWhenFinished));
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject json, bool ingameLoad)
	{
		if (ingameLoad)
		{
		}
		JProperty jProperty = json.Property("ObjectTargetRef");
		if (jProperty != null)
		{
			string text = (string)json.Property("ObjectTargetRef_Scene").Value;
			int id = (int)jProperty.Value;
			if ((Application.isPlaying || !(text != SceneManagerHelper.ActiveSceneName)) && Application.isPlaying)
			{
				m_ObjectSceneRef = ObjectiveSceneElement.FindSceneReference(id);
			}
		}
		JProperty jProperty2 = json.Property("InteractionID");
		if (jProperty2 != null)
		{
			m_InteractionID = (int)jProperty2.Value;
		}
		JProperty jProperty3 = json.Property("NewState");
		if (jProperty3 != null)
		{
			m_bNewInteractionState = (bool)jProperty3.Value;
		}
		JProperty jProperty4 = json.Property("RestoreWhenFinished");
		if (jProperty4 != null)
		{
			m_bRestoreStateWhenFinished = (bool)jProperty4.Value;
		}
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.EnableInteractionObjective;
	}
}
