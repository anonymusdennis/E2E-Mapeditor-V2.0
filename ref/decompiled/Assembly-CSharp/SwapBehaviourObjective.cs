using System;
using Newtonsoft.Json.Linq;
using NodeCanvas.BehaviourTrees;
using UnityEngine;

[Serializable]
public class SwapBehaviourObjective : BaseObjective
{
	public const string BEHAVIOUR_REF_PATH = "ObjectiveBehaviourRefs";

	public ObjectiveSceneElement m_CharacterSceneRef;

	public BehaviourTree m_NewBehaviourTree;

	public bool m_bRestoreTreeWhenFinished;

	private Character m_TargetCharacter;

	private BehaviourTreeOwner m_BehaviourTreeOwner;

	private BehaviourTree m_OriginalBehaviourTree;

	private bool m_bWasRunning;

	protected override void Child_PickAllTargets()
	{
		if (m_CharacterSceneRef != null && m_CharacterSceneRef.m_LinksTo == ObjectiveSceneElement.ObjectiveSceneElementType.Character)
		{
			m_TargetCharacter = m_CharacterSceneRef.GetComponent<Character>();
			if (m_TargetCharacter != null)
			{
				m_BehaviourTreeOwner = m_TargetCharacter.GetComponent<BehaviourTreeOwner>();
			}
			if (m_BehaviourTreeOwner == null)
			{
				m_ObjectiveStatus = ObjectiveStatus.Invalid;
			}
		}
		else
		{
			m_ObjectiveStatus = ObjectiveStatus.Invalid;
		}
		if (m_NewBehaviourTree == null)
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
		if (m_BehaviourTreeOwner != null && m_NewBehaviourTree != null)
		{
			m_OriginalBehaviourTree = m_BehaviourTreeOwner.behaviour;
			m_bWasRunning = m_BehaviourTreeOwner.isRunning;
			m_BehaviourTreeOwner.SwitchBehaviour(m_NewBehaviourTree);
			if (!m_bWasRunning)
			{
				m_BehaviourTreeOwner.StopBehaviour();
			}
		}
	}

	protected override bool Child_EvaluateDependencies()
	{
		return true;
	}

	protected override bool Child_EvaluateStatus()
	{
		return Child_EvaluateDependencies();
	}

	protected override void Child_PostAction()
	{
		if (m_bRestoreTreeWhenFinished && m_BehaviourTreeOwner != null && m_OriginalBehaviourTree != null)
		{
			m_BehaviourTreeOwner.SwitchBehaviour(m_OriginalBehaviourTree);
			if (!m_bWasRunning)
			{
				m_BehaviourTreeOwner.StopBehaviour();
			}
		}
		m_OriginalBehaviourTree = null;
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
		if (m_NewBehaviourTree != null)
		{
			int num = -1;
			RuntimeBehaviours runtimeBehaviours = Resources.Load<RuntimeBehaviours>("ObjectiveBehaviourRefs");
			if (runtimeBehaviours != null)
			{
				num = runtimeBehaviours.m_Behaviours.IndexOf(m_NewBehaviourTree);
			}
			baseObj.Add(new JProperty("TreeID", num));
		}
		baseObj.Add(new JProperty("ResetWhenFinished", m_bRestoreTreeWhenFinished));
		if (m_CharacterSceneRef != null)
		{
			baseObj.Add(new JProperty("CharacterTargetRef", m_CharacterSceneRef.m_ObjectiveElementID));
			baseObj.Add(new JProperty("CharacterTargetRef_Scene", m_CharacterSceneRef.m_UsedInScene));
		}
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject json, bool ingameLoad)
	{
		if (ingameLoad)
		{
		}
		JProperty jProperty = json.Property("TreeID");
		if (jProperty != null)
		{
			int num = (int)jProperty.Value;
			if (num >= -1)
			{
				RuntimeBehaviours runtimeBehaviours = Resources.Load<RuntimeBehaviours>("ObjectiveBehaviourRefs");
				if (runtimeBehaviours != null && num >= 0 && num < runtimeBehaviours.m_Behaviours.Count)
				{
					m_NewBehaviourTree = runtimeBehaviours.m_Behaviours[num];
				}
			}
			if (!(m_NewBehaviourTree == null))
			{
			}
		}
		JProperty jProperty2 = json.Property("ResetWhenFinished");
		if (jProperty2 != null)
		{
			m_bRestoreTreeWhenFinished = (bool)jProperty2.Value;
		}
		JProperty jProperty3 = json.Property("CharacterTargetRef");
		if (jProperty3 != null)
		{
			string text = (string)json.Property("CharacterTargetRef_Scene").Value;
			int id = (int)jProperty3.Value;
			if ((Application.isPlaying || !(text != SceneManagerHelper.ActiveSceneName)) && Application.isPlaying)
			{
				m_CharacterSceneRef = ObjectiveSceneElement.FindSceneReference(id);
			}
		}
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.SwapBehaviourObjective;
	}
}
