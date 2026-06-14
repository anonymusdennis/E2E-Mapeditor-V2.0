using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Description("Check to see if there is a carryable object on the floor")]
[Category("★T17 Jobs")]
public class CheckCarryObjectOnFloor : ConditionTask<AICharacter>
{
	[BlackboardOnly]
	public BBParameter<InteractiveObject> m_FoundCarryObject;

	public int m_ExpectedTag = -1;

	protected override bool OnCheck()
	{
		m_FoundCarryObject.value = null;
		if (base.agent == null || base.agent.m_Character == null)
		{
			return false;
		}
		RoomBlob jobRoom = base.agent.m_Character.GetJobRoom();
		if (jobRoom == null)
		{
			return false;
		}
		if (jobRoom.m_CarryObjectInteractions == null || jobRoom.m_CarryObjectInteractions.Count == 0)
		{
			return false;
		}
		List<CarryObjectInteraction> carryObjectInteractions = jobRoom.m_CarryObjectInteractions;
		for (int i = 0; i < carryObjectInteractions.Count; i++)
		{
			CarryObjectInteraction carryObjectInteraction = carryObjectInteractions[i];
			if (!(carryObjectInteraction == null) && carryObjectInteraction.m_Decoration == CarryObjectInteraction.AI_Decorations.Job && (m_ExpectedTag == -1 || carryObjectInteraction.m_Tag == m_ExpectedTag) && !carryObjectInteraction.IsPickedUp && carryObjectInteraction.isActiveAndEnabled)
			{
				m_FoundCarryObject.value = carryObjectInteraction;
				return true;
			}
		}
		return false;
	}
}
