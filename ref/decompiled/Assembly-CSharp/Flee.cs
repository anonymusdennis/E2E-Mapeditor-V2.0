using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("★T17 Action")]
public class Flee : ActionTask<AICharacter>
{
	public BBParameter<AIEventMemory> m_combatant;

	private Transform m_CombatantTransform;

	private Character m_CombatantCharacter;

	private bool m_bMovingToPosition;

	public float m_CloseEnoughDistance = 0.5f;

	public BBParameter<string> m_TextID = "Text.Inmates.AttackedRunsAway";

	public BBParameter<int> m_Variation = -1;

	public BBParameter<SpeechTone> m_Tone = SpeechTone.Negative;

	public BBParameter<float> m_Duration = 3f;

	public BBParameter<int> m_Priority = 1;

	private T17_ABPath.PathCallback m_OnTargetReachedDel;

	private T17_ABPath.PathCallback m_OnPathCancelledDel;

	protected override string OnInit()
	{
		m_OnTargetReachedDel = OnTargetReached;
		m_OnPathCancelledDel = OnPathCancelled;
		return base.OnInit();
	}

	protected override void OnExecute()
	{
		if (m_combatant.value == null)
		{
			EndAction(false);
			return;
		}
		m_CombatantCharacter = m_combatant.value.m_CharacterResponsible;
		m_CombatantTransform = m_CombatantCharacter.m_Transform;
		if (base.agent.m_Character.m_CharacterRole != 0)
		{
			EndAction(false);
		}
		m_bMovingToPosition = false;
	}

	protected override void OnUpdate()
	{
		if (!m_bMovingToPosition)
		{
			RunAway();
		}
		base.agent.SetRunning(running: true);
	}

	public void OnTargetReached()
	{
		m_bMovingToPosition = false;
		bool haveCollisionData = false;
		if (!base.agent.m_CharacterUtil.LineOfSight(m_CombatantTransform.gameObject, out haveCollisionData))
		{
			base.agent.ForgetEvent(m_combatant.value);
			EndAction(true);
		}
	}

	public void OnPathCancelled()
	{
		m_bMovingToPosition = false;
	}

	private void RunAway()
	{
		Vector3 pos = base.agent.m_Transform.position;
		if (!RoomManager.GetInstance().GetRandomPositionInWorld(base.agent.m_Character.m_CharacterRole, ref pos))
		{
			m_bMovingToPosition = false;
			return;
		}
		SpeechManager.GetInstance().SaySomething(base.agent.m_Character, m_TextID.value, m_Tone.value, m_Duration.value, m_Priority.value, m_Variation.value);
		m_bMovingToPosition = base.agent.m_AIMovement.TravelToPosition(m_OnTargetReachedDel, m_OnPathCancelledDel, pos, m_CloseEnoughDistance, throttled: true);
	}

	protected override void OnStop()
	{
		base.agent.SetRunning(running: false);
	}
}
