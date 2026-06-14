using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class AICharacter_Ghost : AICharacter
{
	public SkinnedMeshRenderer m_MeshRenderer;

	public Vector2 m_RandomInterval = new Vector2(5f, 15f);

	public float m_LerpSpeed = 2f;

	private float m_Cutoff = 0.9f;

	private float m_Alpha = 1f;

	private float m_Timer;

	private float m_NextLerpTime;

	private bool m_bOpaque = true;

	private uint m_GhostNodeTag;

	private RoomBlob m_CurrentRoom;

	protected override void OnAwake()
	{
		base.OnAwake();
		m_Timer = 0f;
		m_NextLerpTime = Random.Range(m_RandomInterval.x, m_RandomInterval.y);
		m_GhostNodeTag = (uint)AIMovement.GetKeyTag(KeyFunctionality.KeyColour.Ghost);
		m_CurrentRoom = null;
		if (m_MeshRenderer != null && m_MeshRenderer.material != null)
		{
			m_MeshRenderer.material.SetFloat("_Cutoff", m_Cutoff);
		}
	}

	protected override void OnStart()
	{
		m_Character.OnRoomChanged += RoomChanged;
	}

	protected override void OnDestroy()
	{
		m_Character.OnRoomChanged -= RoomChanged;
		base.OnDestroy();
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		if (m_AIMovement != null)
		{
			m_AIMovement.AddGhostKeyAccess();
		}
		return base.StartInit();
	}

	protected override void OnUpdate()
	{
		m_Timer += Time.deltaTime;
		if (m_Timer >= m_NextLerpTime)
		{
			if (m_bOpaque)
			{
				m_Alpha = Mathf.Lerp(m_Alpha, 0f, Time.deltaTime * m_LerpSpeed);
				if (m_Alpha <= 0.08f)
				{
					m_bOpaque = false;
					m_NextLerpTime = m_Timer + Random.Range(m_RandomInterval.x, m_RandomInterval.y);
				}
			}
			else
			{
				m_Alpha = Mathf.Lerp(m_Alpha, 1f, Time.deltaTime * m_LerpSpeed);
				if (m_Alpha >= 0.92f)
				{
					m_bOpaque = true;
					m_NextLerpTime = m_Timer + Random.Range(m_RandomInterval.x, m_RandomInterval.y);
				}
			}
		}
		if (m_MeshRenderer != null && m_MeshRenderer.material != null)
		{
			m_MeshRenderer.material.SetFloat("_Alpha", m_Alpha);
		}
	}

	public void RoomChanged(RoomBlob oldRoom, RoomBlob newRoom)
	{
		m_CurrentRoom = newRoom;
	}

	public override void QueryCurrentNode(GraphNode node)
	{
		if (!m_bOpaque || CutsceneManagerBase.IsACutscenePlaying() || node.Tag != m_GhostNodeTag)
		{
			return;
		}
		List<Character> charactersInRoom = m_CurrentRoom.GetCharactersInRoom();
		int count = charactersInRoom.Count;
		for (int i = 0; i < count; i++)
		{
			if (charactersInRoom[i].m_CharacterStats.m_bIsPlayer)
			{
				Player player = (Player)charactersInRoom[i];
				if (player != null && player.m_Gamer != null && player.m_Gamer.IsLocal() && player.m_Gamer == Gamer.GetPrimaryGamer())
				{
					StatSystem.GetInstance().IncStat(49, 1f, Gamer.GetPrimaryGamer(), string.Empty);
				}
			}
		}
	}
}
