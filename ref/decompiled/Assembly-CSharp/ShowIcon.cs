using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Action")]
public class ShowIcon : ActionTask<AICharacter>
{
	public CharacterIconHandler.IconType m_Icon;

	public float m_fDuration = 1f;

	protected override string info => string.Concat("Icon ", m_Icon, ":", m_fDuration);

	protected override void OnExecute()
	{
		base.agent.m_Character.m_IconHandler.DisplayIconRPC(m_Icon, m_fDuration);
		EndAction(true);
	}
}
