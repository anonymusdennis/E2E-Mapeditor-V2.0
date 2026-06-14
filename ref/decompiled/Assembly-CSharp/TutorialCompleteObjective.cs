using System;
using Newtonsoft.Json.Linq;

[Serializable]
public class TutorialCompleteObjective : BaseObjective
{
	public TutorialSubject m_TutorialSubject = TutorialSubject.UNASSIGNED;

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.TutorialCompleteObjective;
	}

	protected override bool Child_EvaluateDependencies()
	{
		return Child_EvaluateStatus();
	}

	protected override bool Child_EvaluateStatus()
	{
		return true;
	}

	protected override void Child_Initialize()
	{
	}

	protected override void Child_PickAllTargets()
	{
	}

	protected override void Child_PostAction()
	{
	}

	protected override void Child_PreAction()
	{
		TutorialManager.GetInstance().TutorialComplete(m_PlayerOwner, m_TutorialSubject);
	}

	protected override void Child_RegisterTokens(ref ObjectiveTree objectiveTree)
	{
	}

	protected override void Child_Reset()
	{
	}

	protected override string Child_Save(JObject baseObj, bool ingameSave)
	{
		baseObj.Add(new JProperty("TutorialSubject", m_TutorialSubject));
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject json, bool ingameLoad)
	{
		int tutorialSubject = (int)json.Property("TutorialSubject").Value;
		m_TutorialSubject = (TutorialSubject)tutorialSubject;
	}

	protected override void Child_SetHUDPins(bool on)
	{
	}

	protected override void Child_SetHUDArrow(bool on)
	{
	}
}
