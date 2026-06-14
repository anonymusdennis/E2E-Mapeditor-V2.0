using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Description("Check if an Interactable object is reserved and return true for one frame if it matches the required event types")]
[Category("★T17 Events")]
public class CheckInteractableReserved : ConditionTask<AICharacter>
{
	[BlackboardOnly]
	public BBParameter<InteractiveObject> m_TargetObject;

	[BlackboardOnly]
	public BBParameter<int> m_InteractingCharacterID;

	protected override bool OnCheck()
	{
		if (m_TargetObject.value != null && m_TargetObject.value.ObjectReserved() && m_TargetObject.value.m_ReservingCharacterID > -1)
		{
			m_InteractingCharacterID.value = m_TargetObject.value.m_ReservingCharacterID;
			return true;
		}
		m_InteractingCharacterID.value = -1;
		return false;
	}
}
