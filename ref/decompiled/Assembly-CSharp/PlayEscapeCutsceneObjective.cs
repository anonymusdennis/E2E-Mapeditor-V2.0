using System;
using Newtonsoft.Json.Linq;
using Slate;
using UnityEngine;

[Serializable]
public class PlayEscapeCutsceneObjective : BaseObjective
{
	private ObjectiveSceneElement m_CutsceneElement;

	private Cutscene m_Cutscene;

	private EscapeMethod m_EscapeMethod;

	protected override void Child_PickAllTargets()
	{
		if (m_CutsceneElement != null)
		{
			m_Cutscene = m_CutsceneElement.GetComponent<Cutscene>();
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
		if (m_Cutscene != null)
		{
			EscapePrisonFunctionality instance = EscapePrisonFunctionality.GetInstance();
			instance.TriggerEscapeRPC(m_Cutscene, EscapeMethod.Unknown);
		}
	}

	protected override bool Child_EvaluateDependencies()
	{
		return true;
	}

	protected override bool Child_EvaluateStatus()
	{
		return !CutsceneManagerBase.IsACutscenePlaying();
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
		baseObj.Add(new JProperty("SceneRef_ID", m_CutsceneElement.m_ObjectiveElementID));
		baseObj.Add(new JProperty("SceneRef_Scene", m_CutsceneElement.m_UsedInScene));
		baseObj.Add(new JProperty("EscapeMethod", (int)m_EscapeMethod));
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject json, bool ingameLoad)
	{
		m_EscapeMethod = (EscapeMethod)(int)json.Property("EscapeMethod").Value;
		JProperty jProperty = json.Property("SceneRef_ID");
		if (jProperty != null)
		{
			string text = (string)json.Property("SceneRef_Scene").Value;
			int id = (int)jProperty.Value;
			if ((Application.isPlaying || !(text != SceneManagerHelper.ActiveSceneName)) && Application.isPlaying)
			{
				m_CutsceneElement = ObjectiveSceneElement.FindSceneReference(id);
			}
		}
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.PlayEscapeCutsceneObjective;
	}

	public string GetTitle()
	{
		return "Escape cutscene " + ((!(m_CutsceneElement == null)) ? m_CutsceneElement.transform.name : "*not set*");
	}
}
