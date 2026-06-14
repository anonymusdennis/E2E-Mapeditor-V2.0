using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class TutorialGuidedUIObjective : BaseObjective
{
	public enum Mode
	{
		ShowTutorial,
		CancelTutorial
	}

	public enum TutorialLocation
	{
		HUD,
		InGameMenus
	}

	public Mode m_Mode;

	public TutorialLocation m_TutorialLocation = TutorialLocation.InGameMenus;

	public int m_TutorialEnumValue;

	public List<ItemData> m_ItemsToGuide = new List<ItemData>();

	protected override void Child_PickAllTargets()
	{
		switch (m_TutorialLocation)
		{
		case TutorialLocation.HUD:
			if (HUDMenuFlow.Instance != null)
			{
				if (HUDMenuFlow.Instance.m_TutorialController == null)
				{
					m_ObjectiveStatus = ObjectiveStatus.Invalid;
				}
			}
			else
			{
				m_ObjectiveStatus = ObjectiveStatus.Invalid;
			}
			break;
		case TutorialLocation.InGameMenus:
			if (InGameMenuFlow.Instance != null)
			{
				if (InGameMenuFlow.Instance.m_TutorialController == null)
				{
					m_ObjectiveStatus = ObjectiveStatus.Invalid;
				}
			}
			else
			{
				m_ObjectiveStatus = ObjectiveStatus.Invalid;
			}
			break;
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
		switch (m_TutorialLocation)
		{
		case TutorialLocation.HUD:
		{
			HUDTutorialArrowController.HUDTutorial tutorialEnumValue2 = (HUDTutorialArrowController.HUDTutorial)m_TutorialEnumValue;
			switch (m_Mode)
			{
			case Mode.ShowTutorial:
				HUDMenuFlow.Instance.StartTutorial(m_ItemsToGuide, tutorialEnumValue2);
				break;
			case Mode.CancelTutorial:
				HUDMenuFlow.Instance.StopTutorial(tutorialEnumValue2);
				break;
			}
			break;
		}
		case TutorialLocation.InGameMenus:
		{
			IGMTutorialArrowController.IGMTutorial tutorialEnumValue = (IGMTutorialArrowController.IGMTutorial)m_TutorialEnumValue;
			switch (m_Mode)
			{
			case Mode.ShowTutorial:
				InGameMenuFlow.Instance.StartTutorial(m_ItemsToGuide, tutorialEnumValue);
				break;
			case Mode.CancelTutorial:
				InGameMenuFlow.Instance.StopTutorial(tutorialEnumValue);
				break;
			}
			break;
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

	protected override void Child_PostAction()
	{
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
		baseObj.Add("Mode", (int)m_Mode);
		baseObj.Add("Location", (int)m_TutorialLocation);
		baseObj.Add("Tutorial", m_TutorialEnumValue);
		JProperty jProperty = new JProperty("Items");
		JArray jArray = new JArray();
		for (int i = 0; i < m_ItemsToGuide.Count; i++)
		{
			jArray.Add(m_ItemsToGuide[i].m_ItemDataID);
		}
		jProperty.Add(jArray);
		baseObj.Add(jProperty);
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject json, bool ingameLoad)
	{
		if (ingameLoad)
		{
		}
		JProperty jProperty = json.Property("Mode");
		if (jProperty != null)
		{
			int mode = (int)jProperty.Value;
			m_Mode = (Mode)mode;
		}
		JProperty jProperty2 = json.Property("Location");
		if (jProperty2 != null)
		{
			int tutorialLocation = (int)jProperty2.Value;
			m_TutorialLocation = (TutorialLocation)tutorialLocation;
		}
		JProperty jProperty3 = json.Property("Tutorial");
		if (jProperty3 != null)
		{
			m_TutorialEnumValue = (int)jProperty3.Value;
		}
		m_ItemsToGuide.Clear();
		JProperty jProperty4 = json.Property("Items");
		if (jProperty4 == null)
		{
			return;
		}
		List<int> itemDataIDs = null;
		JArray jArray = (JArray)jProperty4.Value;
		if (jArray != null)
		{
			itemDataIDs = jArray.Select((JToken c) => (int)c).ToList();
		}
		if (itemDataIDs == null || itemDataIDs.Count <= 0)
		{
			return;
		}
		List<ItemData> source = Resources.LoadAll<ItemData>("Prefabs/Items").ToList();
		for (int i = 0; i < itemDataIDs.Count; i++)
		{
			m_ItemsToGuide.Add(source.FirstOrDefault((ItemData id) => id.m_ItemDataID == itemDataIDs[i]));
		}
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.TutorialGuidedUIObjective;
	}
}
