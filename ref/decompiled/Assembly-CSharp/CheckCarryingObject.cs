using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Description("Conditional returns true if we are carrying an object of the specified type")]
[Category("★T17 Jobs")]
public class CheckCarryingObject : ConditionTask<AICharacter>
{
	public int m_ExpectedCarryTag = -1;

	protected override string info => (m_ExpectedCarryTag != -1) ? ("CheckCarryingObject : " + m_ExpectedCarryTag) : "CheckCarryingObject";

	protected override bool OnCheck()
	{
		if (base.agent.m_Character == null)
		{
			return false;
		}
		if (!base.agent.m_Character.IsCarryingObject())
		{
			return false;
		}
		if (m_ExpectedCarryTag == -1)
		{
			return true;
		}
		CarryObjectInteraction carriedObject = base.agent.m_Character.GetCarriedObject();
		if (carriedObject == null)
		{
			return false;
		}
		return carriedObject.m_Tag == m_ExpectedCarryTag;
	}
}
