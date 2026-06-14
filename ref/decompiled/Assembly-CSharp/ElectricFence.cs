using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class ElectricFence : MonoBehaviour, IControlledUpdate
{
	public Animator m_ElectricFenceAnimator;

	public float m_fMaxAnimTimer = 5f;

	public float m_fMinAnimTimer = 1f;

	public float m_fDamage = 20f;

	public float m_fKnockBack = 5f;

	public float m_fSparkOnStartChance = 0.8f;

	public BoxCollider m_Collider;

	private bool m_bEnabled = true;

	private float m_fTime;

	private float m_fTimer;

	private int m_State;

	private int m_StateHash = -1;

	private int m_GoHash = -1;

	private DamagableTile m_DamagableTile;

	private void Awake()
	{
		m_StateHash = Animator.StringToHash("State");
		m_GoHash = Animator.StringToHash("Go");
		if (m_ElectricFenceAnimator != null)
		{
			m_ElectricFenceAnimator = GetComponent<Animator>();
		}
		m_DamagableTile = GetComponentInParent<DamagableTile>();
		Vector3 localPosition = base.transform.localPosition;
		base.transform.localPosition = new Vector3(localPosition.x, localPosition.y, -0.1f);
		UpdateCollider();
	}

	private void Start()
	{
		StepAnimation();
		if (null != UpdateManager.GetInstance())
		{
			UpdateManager.GetInstance().Register(this, UpdateCategory.World_Slow);
		}
	}

	protected virtual void OnDestroy()
	{
		if (null != UpdateManager.GetInstance())
		{
			UpdateManager.GetInstance().Unregister(this, UpdateCategory.World_Slow);
		}
	}

	public bool ShockCharacter(Character character)
	{
		if ((character.m_CharacterRole == CharacterRole.Guard || character.m_CharacterRole == CharacterRole.Inmate) && !character.GetIsKnockedOut())
		{
			character.CalcFaceDirection(base.transform.position - character.m_Transform.position);
			character.KnockBackCharacter(character.m_Transform.position - base.transform.position, m_fKnockBack);
			character.m_CharacterAnimator.StartAnimation(AnimState.CombatRecoil);
			character.DamageSelf(character, m_fDamage);
			if (m_ElectricFenceAnimator != null)
			{
				m_State = Random.Range(1, 4);
				m_ElectricFenceAnimator.SetInteger(m_StateHash, m_State);
				m_ElectricFenceAnimator.SetTrigger(m_GoHash);
			}
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Electric_Fence_Shock, base.gameObject);
			return true;
		}
		return false;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (m_bEnabled && collision != null && !(collision.gameObject == null))
		{
			Character component = collision.gameObject.GetComponent<Character>();
			if (!(component == null))
			{
				ShockCharacter(component);
			}
		}
	}

	public bool GetEnabled()
	{
		return m_bEnabled;
	}

	public void SetEnabled(bool enable)
	{
		m_bEnabled = enable;
		UpdateCollider();
		if (!(m_ElectricFenceAnimator == null))
		{
			if (enable && Random.value < m_fSparkOnStartChance)
			{
				m_State = Random.Range(1, 4);
				m_ElectricFenceAnimator.SetInteger(m_StateHash, m_State);
				m_ElectricFenceAnimator.SetTrigger(m_GoHash);
			}
			StepAnimation();
		}
	}

	public void SoftDisable()
	{
		m_bEnabled = false;
	}

	private void UpdateCollider()
	{
		if (m_Collider != null)
		{
			if (m_DamagableTile != null && m_DamagableTile.m_DamageAction == DamagableTile.DamageAction.Dig)
			{
				m_Collider.enabled = false;
			}
			else
			{
				m_Collider.enabled = m_bEnabled;
			}
		}
	}

	private void StepAnimation()
	{
		m_fTime = Random.Range(m_fMinAnimTimer, m_fMaxAnimTimer);
		m_fTimer = 0f;
		m_State = Random.Range(1, 4);
	}

	public void ControlledUpdate()
	{
		if (m_bEnabled && !(m_ElectricFenceAnimator == null) && m_fTimer < m_fTime)
		{
			m_fTimer += UpdateManager.deltaTime;
			if (m_fTimer >= m_fTime)
			{
				StepAnimation();
				m_ElectricFenceAnimator.SetInteger(m_StateHash, m_State);
				m_ElectricFenceAnimator.SetTrigger(m_GoHash);
			}
		}
	}

	public void ControlledFixedUpdate()
	{
	}

	public void ControlledLateUpdate()
	{
	}

	public void ControlledPreFixedUpdate()
	{
	}

	public void ControlledPreUpdate()
	{
	}

	public bool RequiresControlledUpdate()
	{
		return true;
	}

	public bool RequiresControlledFixedUpdate()
	{
		return false;
	}

	public bool RequiresControlledLateUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreFixedUpdate()
	{
		return false;
	}
}
