using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Action")]
public class CallTimedPlayerOnlyPIPEvent : ActionTask<Character>
{
	public BBParameter<AIEventMemory> m_Memory;

	public BBParameter<PIPManager.PIPEventType> m_PIPEventType;

	public BBParameter<int> m_PipDuration;

	protected override void OnExecute()
	{
		Player player = null;
		if (m_Memory.value != null)
		{
			if (m_Memory.value.m_TargetCharacter != null)
			{
				player = m_Memory.value.m_TargetCharacter as Player;
			}
			else
			{
				ItemContainer itemContainer = ((!(m_Memory.value.m_Target != null)) ? null : m_Memory.value.m_Target.GetComponent<ItemContainer>());
				if (itemContainer != null && itemContainer.GetCharacterOwner() != null)
				{
					player = itemContainer.GetCharacterOwner() as Player;
				}
			}
		}
		if (player != null)
		{
			PIPManager.GetInstance().NewPlayerPIPEvent(m_PIPEventType.value, player.m_NetView.viewID, base.agent.m_NetView.viewID, 1, m_PipDuration.value);
		}
		EndAction(true);
	}
}
