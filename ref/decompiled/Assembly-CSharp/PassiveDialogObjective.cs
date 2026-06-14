using System;
using Newtonsoft.Json.Linq;

[Serializable]
public class PassiveDialogObjective : BaseObjective
{
	public string m_PassiveDialogTitle = "Text.Dialog.Title";

	public string m_PassiveDialogText = "Text.Dialog.Message";

	public TutorialPopup m_PassiveDialogPrefab;

	private bool m_PassiveDialogueOpened;

	public float m_ShowTime = 5f;

	protected override void Child_PickAllTargets()
	{
	}

	protected override void Child_Initialize()
	{
	}

	protected override void Child_RegisterTokens(ref ObjectiveTree objectiveTree)
	{
	}

	protected override void Child_Reset()
	{
	}

	protected override void Child_PreAction()
	{
		if (m_PassiveDialogPrefab != null && m_PlayerOwner != null && !m_PassiveDialogueOpened)
		{
			m_PassiveDialogueOpened = HUDMenuFlow.Instance.ShowPopupDialogue(m_PassiveDialogPrefab.gameObject, m_PlayerOwner.m_PlayerCameraManagerBindingID, m_ShowTime);
		}
	}

	protected override bool Child_EvaluateDependencies()
	{
		return Child_EvaluateStatus();
	}

	protected override bool Child_EvaluateStatus()
	{
		if (!m_PassiveDialogueOpened && m_PassiveDialogPrefab != null && m_PlayerOwner != null)
		{
			m_PassiveDialogueOpened = HUDMenuFlow.Instance.ShowPopupDialogue(m_PassiveDialogPrefab.gameObject, m_PlayerOwner.m_PlayerCameraManagerBindingID, m_ShowTime);
		}
		return m_PassiveDialogueOpened;
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
			baseObj.Add(new JProperty("DialogOpened", m_PassiveDialogueOpened));
		}
		baseObj.Add(new JProperty("PassiveTitle", m_PassiveDialogTitle));
		baseObj.Add(new JProperty("PassiveText", m_PassiveDialogText));
		baseObj.Add(new JProperty("ShowTime", m_ShowTime));
		if (m_PassiveDialogPrefab != null)
		{
			baseObj.Add(new JProperty("PassiveDialogPrefab", m_PassiveDialogPrefab.m_TutorialPopupID));
		}
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject json, bool ingameLoad)
	{
		if (ingameLoad)
		{
			JProperty jProperty = json.Property("DialogOpened");
			if (jProperty != null)
			{
				m_PassiveDialogueOpened = (bool)jProperty.Value;
			}
		}
		m_PassiveDialogTitle = (string)json.Property("PassiveTitle").Value;
		m_PassiveDialogText = (string)json.Property("PassiveText").Value;
		m_ShowTime = (float)json.Property("ShowTime").Value;
		JProperty jProperty2 = json.Property("PassiveDialogPrefab");
		if (jProperty2 != null)
		{
			int groupID = (int)jProperty2.Value;
			TutorialManager instance = TutorialManager.GetInstance();
			if (instance != null)
			{
				m_PassiveDialogPrefab = instance.GetTutorialPrefabByID(groupID);
			}
		}
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.PassiveDialogObjective;
	}
}
