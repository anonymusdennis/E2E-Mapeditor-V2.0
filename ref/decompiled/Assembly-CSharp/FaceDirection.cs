using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Action")]
public class FaceDirection : ActionTask<AICharacter>
{
	public BBParameter<int> m_FourWayFaceDirection;

	protected override string info
	{
		get
		{
			string empty = string.Empty;
			empty = ((m_FourWayFaceDirection.name == null) ? ((Directionx4)m_FourWayFaceDirection.value).ToString() : m_FourWayFaceDirection.name);
			return "Face: " + empty;
		}
	}

	protected override string OnInit()
	{
		return base.OnInit();
	}

	protected override void OnExecute()
	{
		if (m_FourWayFaceDirection.value != 0)
		{
			base.agent.m_Character.SetFaceDirection((FacingDirectionIncInvalid)m_FourWayFaceDirection.value);
		}
	}

	protected override void OnUpdate()
	{
		EndAction(true);
	}
}
