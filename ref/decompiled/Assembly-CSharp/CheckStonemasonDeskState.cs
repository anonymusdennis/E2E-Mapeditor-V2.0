using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Description("Conditional returns true if we are carrying an object of the specified type")]
[Category("★T17 Jobs")]
public class CheckStonemasonDeskState : ConditionTask<AICharacter>
{
	public BBParameter<StonemasonDesk> m_StonemasonDesk;

	public StonemasonDesk.State m_ExpectedState;

	protected override bool OnCheck()
	{
		if (m_StonemasonDesk.value == null)
		{
			return false;
		}
		return m_StonemasonDesk.value.GetState() == m_ExpectedState;
	}
}
