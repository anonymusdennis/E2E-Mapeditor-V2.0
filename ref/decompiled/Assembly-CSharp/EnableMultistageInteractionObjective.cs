using Newtonsoft.Json.Linq;
using UnityEngine;

public class EnableMultistageInteractionObjective : BaseObjective
{
	private ObjectiveSceneElement m_MultistageInteractionElement;

	private MultistageInputInteraction m_Interaction;

	protected override void Child_PickAllTargets()
	{
		if (m_MultistageInteractionElement != null)
		{
			m_Interaction = m_MultistageInteractionElement.GetComponent<MultistageInputInteraction>();
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
		if (!(m_Interaction == null))
		{
			m_Interaction.EnableInteraction();
			MultiStageTransferInteraction component = m_Interaction.GetComponent<MultiStageTransferInteraction>();
			if (component != null)
			{
				component.m_bIsEnabled = true;
			}
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
		if (m_MultistageInteractionElement != null)
		{
			baseObj.Add(new JProperty("SceneRef_ID", m_MultistageInteractionElement.m_ObjectiveElementID));
			baseObj.Add(new JProperty("SceneRef_Scene", m_MultistageInteractionElement.m_UsedInScene));
		}
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject json, bool ingameLoad)
	{
		JProperty jProperty = json.Property("SceneRef_ID");
		if (jProperty == null)
		{
			return;
		}
		string text = (string)json.Property("SceneRef_Scene").Value;
		int id = (int)jProperty.Value;
		if (Application.isPlaying || !(text != SceneManagerHelper.ActiveSceneName))
		{
			if (Application.isPlaying)
			{
				m_MultistageInteractionElement = ObjectiveSceneElement.FindSceneReference(id);
			}
			if (m_MultistageInteractionElement != null)
			{
				m_Interaction = m_MultistageInteractionElement.GetComponent<MultistageInputInteraction>();
			}
		}
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.EnableMultistageInteractionObjective;
	}

	public string GetTitle()
	{
		return "Enable " + ((!(m_MultistageInteractionElement == null)) ? m_MultistageInteractionElement.gameObject.name : "*not set*");
	}
}
