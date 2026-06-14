using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
	[Range(0f, 20f)]
	public float m_fMaxSpeed = 5f;

	[Range(0f, 1f)]
	public float m_fWalkSpeedThreshold = 0.4f;

	[Range(0f, 1f)]
	public float m_fRunningSpeedThreshold = 0.8f;

	[Range(0f, 20f)]
	public float m_fMaxSpeedBlocking = 1f;

	[Range(0f, 20f)]
	public float m_fMaxSpeedDashing = 8f;

	[Range(0f, 1f)]
	public float m_fDashPlayerInfluence = 0.2f;

	public bool m_bUsePhysics;

	public Transform m_Transform;

	public Rigidbody m_RigidBody;

	public CharacterAnimator m_CharacterAnimator;

	public Character m_Character;

	public CharacterStats m_CharacterStats;

	private CharacterSpeed m_eSpeed;

	private Vector2 m_vMovementSpeed = Vector2.zero;

	private Vector2 m_vZero = Vector2.zero;

	private Vector2 m_previousDirection = Vector2.zero;

	public Vector2 MovementSpeed => m_vMovementSpeed;

	public void Awake()
	{
		m_Transform = base.transform;
		m_RigidBody = GetComponent<Rigidbody>();
		m_CharacterAnimator = GetComponent<CharacterAnimator>();
		m_CharacterStats = GetComponent<CharacterStats>();
		m_RigidBody.interpolation = RigidbodyInterpolation.None;
	}

	protected virtual void OnDestroy()
	{
		m_RigidBody = null;
		m_CharacterAnimator = null;
		m_Character = null;
		m_CharacterStats = null;
	}

	public float GetMaxSpeed()
	{
		float result = m_fMaxSpeed;
		if (m_Character.m_bIsBlocking)
		{
			result = m_fMaxSpeedBlocking;
		}
		else if (m_Character.m_bIsDashing)
		{
			result = m_fMaxSpeedDashing;
		}
		return result;
	}

	public bool Walk(Vector2 desiredVelocity, CharacterSpeed speedOverride = CharacterSpeed.COUNT)
	{
		bool result = false;
		Vector2 direction = desiredVelocity;
		if (m_Character.m_bIsDashing)
		{
			direction = m_Character.GetFacingDirection();
			m_eSpeed = CharacterSpeed.Run;
		}
		else if (speedOverride == CharacterSpeed.COUNT)
		{
			float magnitude = desiredVelocity.magnitude;
			if (magnitude < m_fWalkSpeedThreshold)
			{
				direction = m_vZero;
				m_eSpeed = CharacterSpeed.Stand;
				result = true;
			}
			else if (magnitude < m_fRunningSpeedThreshold)
			{
				m_eSpeed = CharacterSpeed.Walk;
			}
			else
			{
				m_eSpeed = CharacterSpeed.Run;
			}
		}
		else
		{
			m_eSpeed = speedOverride;
			if (m_eSpeed == CharacterSpeed.Stand)
			{
				result = true;
			}
		}
		Move(m_eSpeed, direction);
		return result;
	}

	public void Move(CharacterSpeed speed, Vector2 direction)
	{
		m_eSpeed = speed;
		m_CharacterAnimator.CharacterSpeedChanged(speed);
		float travelDistance = GetTravelDistance(speed);
		m_vMovementSpeed = travelDistance * direction.normalized;
		if (m_bUsePhysics || m_Character.m_bIsDashing)
		{
			if (m_previousDirection != direction && m_Character.m_bIsDashing)
			{
				m_Character.CalcFaceDirection(m_vMovementSpeed);
			}
			m_RigidBody.velocity = m_vMovementSpeed;
		}
		else if (m_eSpeed != 0)
		{
			Vector3 vector = (Vector3)m_vMovementSpeed * UpdateManager.fixedDeltaTime;
			Vector3 vector2 = m_Character.m_CachedCurrentPosition + vector;
			m_RigidBody.MovePosition(vector2);
			m_Character.m_CachedCurrentPosition = vector2;
		}
	}

	public void KnockBack(Vector2 velocity)
	{
		if (m_Character.m_CharacterStats != null && !m_Character.m_CharacterStats.m_bIsPlayer)
		{
			AIPlayer aIPlayer = (AIPlayer)m_Character;
			if (aIPlayer != null)
			{
				aIPlayer.TogglePhysicsControl(enable: true);
			}
		}
		m_RigidBody.velocity = velocity;
		m_vMovementSpeed = velocity;
	}

	public void Immobile()
	{
		m_RigidBody.velocity = m_vZero;
		m_vMovementSpeed = m_vZero;
		m_CharacterAnimator.CharacterSpeedChanged(CharacterSpeed.Stand);
	}

	public void Stand()
	{
		m_CharacterAnimator.CharacterSpeedChanged(CharacterSpeed.Stand);
	}

	public float GetTravelDistance(CharacterSpeed speed)
	{
		if (speed == CharacterSpeed.Stand)
		{
			return 0f;
		}
		float mod = GetMaxSpeed();
		if (m_CharacterStats != null)
		{
			m_CharacterStats.ModSpeed(ref mod);
		}
		if (speed == CharacterSpeed.Walk)
		{
			return m_fWalkSpeedThreshold * mod;
		}
		return mod;
	}

	public CharacterSpeed GetSpeed()
	{
		return m_eSpeed;
	}
}
