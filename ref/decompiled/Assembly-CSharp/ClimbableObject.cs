using System;
using System.Collections.Generic;
using UnityEngine;

public class ClimbableObject : T17MonoBehaviour
{
	private class EncroachingCharacter
	{
		public enum State
		{
			ProcessClimbInput,
			OnObject
		}

		public Character m_Character;

		public float m_CollisionRadius = 0.25f;

		public Vector3 m_CollisionOffset = Vector3.zero;

		public bool m_CollidedThisFrame;

		public State m_State;

		public float m_Timer;

		public bool m_IsPrimed;
	}

	public float m_SecondsUntilPrime = 0.3f;

	public float m_SecondsUntilClimb = 0.2f;

	private T17NetView m_NetView;

	private BoxCollider m_ObjectCollider;

	private List<EncroachingCharacter> m_EncroachingCharacters = new List<EncroachingCharacter>();

	private List<Character> m_CharactersOnUs = new List<Character>();

	private bool m_bHaveAnEncroacher;

	private int m_CharactersLayer = -1;

	private DeskInteraction m_DeskInteraction;

	private Collider m_TriggerStayCached;

	private Character m_TriggerStayCachedCharacter;

	private Collision m_CollisionStayCached;

	private Character m_CollisionStayCachedCharacter;

	public T17NetView NetView => m_NetView;

	public int NumCharactersOnUs => m_CharactersOnUs.Count;

	protected override void Awake()
	{
		base.Awake();
		m_NetView = GetComponent<T17NetView>();
	}

	private void Start()
	{
		m_ObjectCollider = GetComponent<BoxCollider>();
		m_CharactersLayer = LayerMask.NameToLayer("Characters");
		m_DeskInteraction = GetComponent<DeskInteraction>();
	}

	protected virtual void OnDestroy()
	{
		if (m_bHaveAnEncroacher)
		{
			for (int num = m_EncroachingCharacters.Count - 1; num >= 0; num--)
			{
				EncroachingCharacter encroacher = m_EncroachingCharacters[num];
				ForceEncroachingCharacterOffObject(encroacher, shouldRepositionForDismount: true);
			}
		}
		m_EncroachingCharacters.Clear();
		m_CharactersOnUs.Clear();
		m_ObjectCollider = null;
		m_DeskInteraction = null;
		m_NetView = null;
	}

	private void FixedUpdate()
	{
		if (!m_bHaveAnEncroacher)
		{
			return;
		}
		for (int num = m_EncroachingCharacters.Count - 1; num >= 0; num--)
		{
			EncroachingCharacter encroachingCharacter = m_EncroachingCharacters[num];
			bool flag = false;
			if (encroachingCharacter != null)
			{
				switch (encroachingCharacter.m_State)
				{
				case EncroachingCharacter.State.ProcessClimbInput:
					flag = Update_ProcessClimbInput(encroachingCharacter);
					break;
				case EncroachingCharacter.State.OnObject:
					flag = Update_OnObject(encroachingCharacter);
					break;
				}
				encroachingCharacter.m_CollidedThisFrame = false;
			}
			if (!flag)
			{
				if (encroachingCharacter == null || encroachingCharacter.m_Character != null)
				{
				}
				m_EncroachingCharacters.RemoveAt(num);
				if (m_EncroachingCharacters.Count == 0)
				{
					m_bHaveAnEncroacher = false;
				}
			}
		}
	}

	private void OnCollisionStay(Collision other)
	{
		if (m_CollisionStayCached != other)
		{
			m_CollisionStayCached = other;
			if (other.collider != null)
			{
				m_CollisionStayCachedCharacter = other.gameObject.GetComponent<Character>();
			}
			else
			{
				m_CollisionStayCachedCharacter = null;
			}
		}
		if (m_CollisionStayCachedCharacter != null && m_CollisionStayCachedCharacter.m_NetView.isMine && m_CollisionStayCachedCharacter.m_CharacterStats.m_bIsPlayer && !m_CollisionStayCachedCharacter.IsCarryingObject())
		{
			float collisionRadius = 0.25f;
			Vector3 collisionOffset = Vector3.zero;
			SphereCollider sphereCollider = other.collider as SphereCollider;
			if (sphereCollider != null)
			{
				collisionRadius = sphereCollider.radius;
				collisionOffset = sphereCollider.center;
			}
			EncroachingCharacter encroachingCharacter = GetEncroacher(m_CollisionStayCachedCharacter);
			if (encroachingCharacter == null)
			{
				encroachingCharacter = new EncroachingCharacter();
				encroachingCharacter.m_Character = m_CollisionStayCachedCharacter;
				encroachingCharacter.m_CollisionRadius = collisionRadius;
				encroachingCharacter.m_CollisionOffset = collisionOffset;
				encroachingCharacter.m_State = EncroachingCharacter.State.ProcessClimbInput;
				encroachingCharacter.m_Timer = 0f;
				encroachingCharacter.m_IsPrimed = false;
				m_EncroachingCharacters.Add(encroachingCharacter);
				m_bHaveAnEncroacher = true;
			}
			encroachingCharacter.m_CollidedThisFrame = true;
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (m_TriggerStayCached != other)
		{
			m_TriggerStayCached = other;
			if (other.gameObject.layer == m_CharactersLayer)
			{
				m_TriggerStayCachedCharacter = other.gameObject.GetComponentInParent<Character>();
			}
			else
			{
				m_TriggerStayCachedCharacter = null;
			}
		}
		if (!(m_TriggerStayCachedCharacter != null) || !m_TriggerStayCachedCharacter.m_NetView.isMine || m_TriggerStayCachedCharacter.m_CharacterStats.m_bIsPlayer || m_TriggerStayCachedCharacter.IsInteracting() || ((Vector2)(base.transform.position - m_TriggerStayCachedCharacter.transform.position)).sqrMagnitude >= 0.36f || !(m_TriggerStayCachedCharacter.m_CharacterSphereTrigger != null))
		{
			return;
		}
		SphereCollider characterSphereTrigger = m_TriggerStayCachedCharacter.m_CharacterSphereTrigger;
		if (characterSphereTrigger != null)
		{
			float collisionRadius = 0.25f;
			Vector3 collisionOffset = Vector3.zero;
			if (characterSphereTrigger != null)
			{
				collisionRadius = characterSphereTrigger.radius;
				collisionOffset = characterSphereTrigger.center;
			}
			EncroachingCharacter encroachingCharacter = GetEncroacher(m_TriggerStayCachedCharacter);
			if (encroachingCharacter == null)
			{
				encroachingCharacter = new EncroachingCharacter();
				encroachingCharacter.m_Character = m_TriggerStayCachedCharacter;
				encroachingCharacter.m_CollisionRadius = collisionRadius;
				encroachingCharacter.m_CollisionOffset = collisionOffset;
				encroachingCharacter.m_State = EncroachingCharacter.State.ProcessClimbInput;
				encroachingCharacter.m_Timer = 0f;
				encroachingCharacter.m_IsPrimed = false;
				m_EncroachingCharacters.Add(encroachingCharacter);
				m_bHaveAnEncroacher = true;
			}
			encroachingCharacter.m_CollidedThisFrame = true;
		}
	}

	public void SetCharacterOnUs(Character character, bool onOff)
	{
		if (character == null)
		{
			return;
		}
		if (onOff)
		{
			if (!(m_CharactersOnUs.Find((Character x) => x == character) == null))
			{
				return;
			}
			m_CharactersOnUs.Add(character);
			if (!character.m_NetView.isMine)
			{
				return;
			}
			EncroachingCharacter encroacher = GetEncroacher(character);
			if (encroacher != null)
			{
				return;
			}
			float collisionRadius = 0.25f;
			Vector3 collisionOffset = Vector3.zero;
			if (character.m_PhysicsCollider != null)
			{
				SphereCollider physicsSphereCol = character.m_PhysicsSphereCol;
				if (physicsSphereCol != null)
				{
					collisionRadius = physicsSphereCol.radius;
					collisionOffset = physicsSphereCol.center;
				}
			}
			encroacher = new EncroachingCharacter();
			encroacher.m_Character = character;
			encroacher.m_CollisionRadius = collisionRadius;
			encroacher.m_CollisionOffset = collisionOffset;
			encroacher.m_State = EncroachingCharacter.State.OnObject;
			encroacher.m_Timer = 0f;
			encroacher.m_IsPrimed = false;
			m_EncroachingCharacters.Add(encroacher);
			m_bHaveAnEncroacher = true;
		}
		else
		{
			m_CharactersOnUs.RemoveAll((Character x) => x == character);
		}
	}

	public void ForceCharacterOffObject(Character character, bool shouldRepositionForDismount)
	{
		if (character != null)
		{
			EncroachingCharacter encroacher = GetEncroacher(character);
			if (encroacher != null)
			{
				ForceEncroachingCharacterOffObject(encroacher, shouldRepositionForDismount);
			}
		}
	}

	private void ForceEncroachingCharacterOffObject(EncroachingCharacter encroacher, bool shouldRepositionForDismount)
	{
		if (encroacher != null)
		{
			encroacher.m_Character.OnClimbOffObject(this, isMovingOnToNewClimbable: false, shouldRepositionForDismount);
			m_EncroachingCharacters.Remove(encroacher);
			if (m_EncroachingCharacters.Count == 0)
			{
				m_bHaveAnEncroacher = false;
			}
		}
	}

	private EncroachingCharacter GetEncroacher(Character character)
	{
		if (m_bHaveAnEncroacher)
		{
			for (int i = 0; i < m_EncroachingCharacters.Count; i++)
			{
				if (m_EncroachingCharacters[i] != null && m_EncroachingCharacters[i].m_Character == character)
				{
					return m_EncroachingCharacters[i];
				}
			}
		}
		return null;
	}

	private bool Update_ProcessClimbInput(EncroachingCharacter character)
	{
		if (character.m_Character == null || !character.m_CollidedThisFrame)
		{
			return false;
		}
		if (character.m_Character.m_CharacterStats.m_bIsPlayer)
		{
			bool flag = false;
			Vector2 vector = base.transform.position - character.m_Character.m_CachedCurrentPosition;
			if (Mathf.Abs(vector.x) <= m_ObjectCollider.bounds.extents.x && Mathf.Abs(vector.y) <= m_ObjectCollider.bounds.extents.y)
			{
				flag = true;
			}
			bool flag2 = false;
			if (!flag)
			{
				Vector2 walkVector = character.m_Character.WalkVector;
				if (walkVector != Vector2.zero)
				{
					flag2 = Vector2.Dot(vector.normalized, walkVector.normalized) >= Mathf.Cos((float)Math.PI / 4f);
				}
			}
			if (m_DeskInteraction != null && (m_DeskInteraction.IsOpen || m_DeskInteraction.IsOpening))
			{
				flag2 = false;
				flag = false;
			}
			if (flag)
			{
				if (character.m_IsPrimed)
				{
					character.m_Character.OnCancelClimbOnObject(this);
					character.m_IsPrimed = false;
				}
				character.m_Timer = m_SecondsUntilClimb;
			}
			else if (flag2)
			{
				character.m_Timer += UpdateManager.fixedDeltaTime;
				if (character.m_Timer >= m_SecondsUntilPrime && !character.m_IsPrimed)
				{
					character.m_Character.OnStartToClimbOnObject(this);
					character.m_IsPrimed = true;
				}
			}
			else
			{
				character.m_Timer = 0f;
				if (character.m_IsPrimed)
				{
					character.m_Character.OnCancelClimbOnObject(this);
					character.m_IsPrimed = false;
				}
			}
		}
		else if (m_DeskInteraction != null && (m_DeskInteraction.IsOpen || m_DeskInteraction.IsOpening))
		{
			character.m_Timer = 0f;
		}
		else
		{
			character.m_Timer = m_SecondsUntilClimb;
		}
		if (character.m_Timer >= m_SecondsUntilClimb && !character.m_Character.IsClimbingOnObject())
		{
			character.m_Character.OnClimbOnObject(this);
			character.m_State = EncroachingCharacter.State.OnObject;
		}
		return true;
	}

	private bool Update_OnObject(EncroachingCharacter character)
	{
		if (character.m_Character == null)
		{
			return false;
		}
		Vector3 vector = character.m_Character.transform.position + character.m_CollisionOffset;
		Vector3 ourPosition = m_ObjectCollider.transform.position + m_ObjectCollider.center;
		ClimbableObject climbableObject = null;
		if (character.m_Character.m_CharacterStats.m_bIsPlayer)
		{
			Vector2 walkVector = character.m_Character.WalkVector;
			if (walkVector != Vector2.zero && !character.m_Character.GetIsImmobilised())
			{
				Vector3 vector2 = walkVector.normalized;
				Vector3 ourPosition2 = vector + vector2 * 0.2f;
				if (FloorManager.GetInstance().GetTileGridPoint(character.m_Character.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, ourPosition2, out var row, out var column) && FloorManager.GetInstance().GetTileGridPoint(character.m_Character.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, ourPosition, out var row2, out var column2) && (row != row2 || column != column2))
				{
					RaycastHit[] hitList = null;
					int hitCount = 0;
					if (!FloorManager.GetInstance().IsFloorClear(character.m_Character.CurrentFloor, row, column, out hitList, out hitCount))
					{
						bool flag = false;
						if (hitCount == 1)
						{
							Door component = hitList[0].transform.GetComponent<Door>();
							if (component != null)
							{
								flag = DoorManager.GetInstance().IsDoorAllowedForCharacter(character.m_Character, component);
							}
							climbableObject = hitList[0].transform.GetComponent<ClimbableObject>();
							if (climbableObject != null)
							{
								flag = true;
							}
						}
						if (!flag)
						{
							character.m_Character.m_CharacterMovement.Immobile();
							return true;
						}
					}
				}
			}
		}
		float num = 0f - (Mathf.Abs(vector.x - ourPosition.x) - character.m_CollisionRadius - m_ObjectCollider.size.x / 2f);
		float num2 = 0f - (Mathf.Abs(vector.y - ourPosition.y) - character.m_CollisionRadius - m_ObjectCollider.size.y / 2f);
		float num3 = 0f - (Mathf.Abs(vector.z - ourPosition.z) - character.m_CollisionRadius - m_ObjectCollider.size.z / 2f);
		if (num > 0.1f && num2 > 0.1f && num3 > 0.1f)
		{
			return true;
		}
		character.m_Character.OnClimbOffObject(this, climbableObject != null, shouldRepositionOnDismount: true);
		if (climbableObject != null)
		{
			character.m_Character.OnClimbOnObject(climbableObject);
		}
		return false;
	}
}
