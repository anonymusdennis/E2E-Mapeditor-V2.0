using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(T17NetView))]
public class CharacterBarrier : T17MonoBehaviour
{
	public KeyFunctionality.KeyColour m_UnderlyingDoorColour = KeyFunctionality.KeyColour.Silver;

	public float m_PauseMovementTime = 1f;

	public float m_DelayBeforePlayerSpeech = 1.5f;

	public SpeechPODO m_PlayerCollisionText;

	public FakeCharacter m_Watchman;

	public SpeechPODO m_WatchmanPlayerText;

	public float m_WatchmanSpeechCooldown = 3f;

	public List<CharacterRole> m_RolesToRespondTo = new List<CharacterRole>();

	private RoutineManager m_RoutineManager;

	private SpeechManager m_SpeechManager;

	private float m_lastWatchmanSpeechTimestamp;

	private T17NetView m_NetView;

	protected override void Awake()
	{
		base.Awake();
		m_NetView = GetComponent<T17NetView>();
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UpdateAiNodes(componentsInChildren[i].transform.position);
		}
		m_RoutineManager = RoutineManager.GetInstance();
		m_SpeechManager = SpeechManager.GetInstance();
		return base.StartInit();
	}

	protected void OnDestroy()
	{
		m_RoutineManager = null;
		m_SpeechManager = null;
		m_NetView = null;
	}

	protected void OnCollisionEnter(Collision other)
	{
		Character component = other.gameObject.GetComponent<Character>();
		if (!(component == null) && component.m_NetView.isMine && m_RolesToRespondTo.Contains(component.m_CharacterRole))
		{
			m_NetView.RPC("RPC_ALL_HandleCollision", NetTargets.All, component.m_NetView.viewID);
		}
	}

	[PunRPC]
	private void RPC_ALL_HandleCollision(int characterViewId)
	{
		Character character = T17NetView.Find<Character>(characterViewId);
		if (character != null)
		{
			HandleCharacterCollision(character);
		}
	}

	protected virtual void HandleCharacterCollision(Character collidingCharacter)
	{
		bool flag = false;
		if (!collidingCharacter.IsPlayer())
		{
			return;
		}
		Routines routines = ((!(m_RoutineManager != null)) ? Routines.UNASSIGNED : m_RoutineManager.GetCurrentRoutineBaseType());
		if (routines == Routines.Lockdown)
		{
			flag = true;
		}
		else if (collidingCharacter.m_bIsMissing && routines == Routines.LightsOut)
		{
			if (T17NetManager.IsMasterClient)
			{
				PrisonAlertnessManager.GetInstance().IncrementAlertnessBy(11, collidingCharacter, PrisonAlertnessManager.AlertnessReason.OutDuringLightsOut);
			}
			flag = true;
		}
		if (T17NetManager.IsMasterClient && m_PlayerCollisionText.IsSet() && !flag)
		{
			StartCoroutine(SaySpeechAfterDelayRPC(collidingCharacter, m_PlayerCollisionText, m_DelayBeforePlayerSpeech));
		}
		if (m_Watchman != null && m_lastWatchmanSpeechTimestamp + m_WatchmanSpeechCooldown < UpdateManager.time)
		{
			m_Watchman.SaySomethingLocally(m_WatchmanPlayerText);
			m_lastWatchmanSpeechTimestamp = UpdateManager.time;
		}
		if (collidingCharacter.m_NetView.isMine && !flag)
		{
			collidingCharacter.PauseMovement(m_PauseMovementTime, force: true);
		}
	}

	private void UpdateAiNodes(Vector3 pivotPosition)
	{
		GraphNode nearestGraphNode = NavMeshUtil.GetNearestGraphNode(pivotPosition);
		UpdateNode(nearestGraphNode, pivotPosition);
	}

	private void UpdateNode(GraphNode node, Vector3 pivotPosition)
	{
		if (node != null)
		{
			uint keyTag = (uint)AIMovement.GetKeyTag(m_UnderlyingDoorColour);
			NavMeshUtil.SetNodeTag(node, keyTag);
		}
	}

	protected IEnumerator SaySpeechAfterDelayRPC(Character character, SpeechPODO speech, float delay)
	{
		yield return new WaitForSeconds(delay);
		if (m_SpeechManager != null)
		{
			m_SpeechManager.SaySomething(character, speech);
		}
	}
}
