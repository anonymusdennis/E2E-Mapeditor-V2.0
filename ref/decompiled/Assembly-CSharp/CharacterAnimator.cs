using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class CharacterAnimator : T17MonoBehaviour, IControlledUpdate
{
	public enum ANIMATOR_TYPE
	{
		AT_NOT_SET,
		AT_CLIVE,
		AT_CAMERAMAN,
		AT_DOG
	}

	public enum AttackState
	{
		None,
		NormalAttack,
		HeavyAttack
	}

	[Serializable]
	public class MaterialRenderers
	{
		public Material material;

		public Material preFabMaterial;

		public List<MeshRenderer> renderers;

		public CullingObjectCollector.CharacterWrapper.MeshRendererTransform[] mrtList;
	}

	public class AccessoryData
	{
		public Texture hairMask;

		public Texture hairTex;

		public Texture hatTex;

		public Texture upperFaceTex;

		public Texture lowerFaceTex;
	}

	public delegate void OneShotDone();

	public enum SpotlightTracking
	{
		FourDirections,
		EightDirections,
		Perfect
	}

	[Serializable]
	public class IdleAnimBlob
	{
		public AnimState m_State = AnimState.Idle2;

		public float m_Chance = 1f;

		public ItemData m_RequiredEquipItemData;
	}

	private enum AVATAR_MATERIAL_CHANNELS
	{
		AMC_GENERIC,
		AMC_FACE_ACC_HAIR,
		AMC_SHOVEL,
		AMC_LENS,
		AMC_HAMMER,
		AMC_MOP,
		AMC_TOOL,
		AMC_SWOOSH,
		AMC_ROPE,
		AMC_NUM
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct AnimStateComparer : IEqualityComparer<AnimState>
	{
		public bool Equals(AnimState x, AnimState y)
		{
			return x == y;
		}

		public int GetHashCode(AnimState obj)
		{
			return (int)obj;
		}
	}

	public Animator m_CharacterAnimator;

	public Character m_Character;

	private ANIMATOR_TYPE m_AnimatorType;

	private Snore m_Snore;

	private Knockout m_Knockout;

	private bool m_bShowingSnore;

	private bool m_bShowingKnockOut;

	private const string SNORE_PREFAB_NAME = "Prefabs/Animation/Snore";

	private const string KNOCKOUT_PREFAB_NAME = "Prefabs/Animation/Knockout";

	public T17NetView m_NetView;

	public List<MaterialRenderers> m_MaterialRenderers = new List<MaterialRenderers>();

	public AnimationStateData m_AnimStateData;

	public Transform m_HeadAttachPoint;

	[ReadOnly]
	public List<AnimState> m_AnimStateStack = new List<AnimState>();

	private bool m_bInitedMaterials;

	public bool m_bAlteredAnimatorMaterials;

	private static bool m_InitedAccPropertyIDs = false;

	public static int m_propId_Head_UVRefTex = 0;

	public static int m_propId_Head_HairTex = 0;

	public static int m_propId_Head_HairMaskTex = 0;

	public static int m_propId_Head_HatTex = 0;

	public static int m_propId_Head_UpperAccTex = 0;

	public static int m_propId_Head_LowerAccTex = 0;

	private const string HAIR_MASK_KEYWORD = "T17_CHARACTER_MASK_HAIR";

	private AccessoryData m_AccessoryData;

	private bool m_bInitedAccMaterial;

	private Material m_AccessoryMaterial;

	private const string CHARACTER_UI_HIGHLIGHT_KEYWORD = "T17_CHARACTER_UI";

	private const string NO_STENCIL_KEYWORD = "T17_CHARACTER_NO_STENCIL";

	public bool m_bSkipStencilCheck;

	protected AnimState m_eAnimState = AnimState.Idle;

	private CharacterSpeed m_eSpeed;

	private CombatState m_eCombatState;

	private Directionx4 m_eDirectionx4;

	private Directionx8 m_eDirectionx8;

	private TrayState m_eTrayState;

	private Directionx4 m_AnimatorFacingDirection;

	private bool m_bLockedOn;

	private float m_OneShotTimer;

	public OneShotDone OnOneShotDone;

	public Light m_Spotlight;

	private bool m_SpotlightsActiveRoutine;

	private bool m_SpotlightsActiveHijack = true;

	private bool m_SpotlightsActiveKnockedOut = true;

	public SpotlightTracking m_SpotlightTrackingMode;

	private float m_fAttackPendingTimer;

	private AttackState m_bAttackPending;

	private GameObject m_ChargeAttack_DashVFX;

	private GameObject m_ChargeAttack_ChargeVFX;

	private bool m_bIsCharging;

	public IdleAnimBlob[] m_PossibleStandingStillAnimStates = new IdleAnimBlob[1]
	{
		new IdleAnimBlob()
	};

	public static Dictionary<AnimState, int> m_AnimStateToCharacterIndex = null;

	private bool m_bUseLensMaterial;

	private static Vector3[] m_ChargeAttackOffsets = new Vector3[4]
	{
		new Vector3(0.5f, 0.8f, -0.3f),
		new Vector3(0.26f, 0.8f, -0.3f),
		new Vector3(-0.4f, 0.7f, -0.3f),
		new Vector3(-0.26f, 0.8f, -0.3f)
	};

	private static Vector3[] m_ChargeAttackImpactOffsets = new Vector3[4]
	{
		new Vector3(0f, 1f, -0.3f),
		new Vector3(-0.4f, 0.4f, -0.3f),
		new Vector3(0f, 0f, -0.3f),
		new Vector3(0.4f, 0.4f, -0.3f)
	};

	private static float m_BottomOfMapThreshold = -8.7f;

	private static Vector3[] m_ChargeAttackOffsetsBottomMap = new Vector3[4]
	{
		new Vector3(0.5f, 0.8f, -0.1f),
		new Vector3(0.26f, 0.8f, -0.1f),
		new Vector3(-0.4f, 0.7f, -0.1f),
		new Vector3(-0.26f, 0.8f, -0.1f)
	};

	private static Vector3[] m_ChargeAttackImpactOffsetsBottomMap = new Vector3[4]
	{
		new Vector3(0f, 1f, -0.1f),
		new Vector3(-0.4f, 0.4f, -0.1f),
		new Vector3(0f, 0f, -0.1f),
		new Vector3(0.4f, 0.4f, -0.1f)
	};

	private static GameObject m_SnorePrefab = null;

	private static GameObject m_KnockoutPrefab = null;

	private float m_AnimatorNormalizedTime = -1f;

	private bool m_bControllingNormalizedTime;

	public bool m_bRenderingDirty = true;

	private bool m_bHasUnappliedStateChange;

	public static AnimStateComparer AnimStateTComparer = default(AnimStateComparer);

	private bool m_SpotlightsActive => m_SpotlightsActiveRoutine && m_SpotlightsActiveHijack && m_SpotlightsActiveKnockedOut && base.enabled;

	public float AnimatorNormalizedTime => m_AnimatorNormalizedTime;

	public bool ControllingNormalizedTime => m_bControllingNormalizedTime;

	public AccessoryData GetCurrentAccessoryData()
	{
		return m_AccessoryData;
	}

	public void SetSkipStencilCheck(bool skip)
	{
		m_bSkipStencilCheck = skip;
	}

	public AnimState GetAnimState()
	{
		return m_eAnimState;
	}

	public CharacterSpeed GetCharacterSpeed()
	{
		return m_eSpeed;
	}

	public CombatState GetCombatState()
	{
		return m_eCombatState;
	}

	public Directionx4 GetDirectionx4()
	{
		return m_eDirectionx4;
	}

	public TrayState GetTrayState()
	{
		return m_eTrayState;
	}

	public Directionx4 GetAnimatorFacingDirection()
	{
		return m_AnimatorFacingDirection;
	}

	public bool GetLockedOn()
	{
		return m_bLockedOn;
	}

	protected override void Awake()
	{
		base.Awake();
		m_bInitedMaterials = false;
		m_bAlteredAnimatorMaterials = false;
		if (m_CharacterAnimator == null)
		{
			m_CharacterAnimator = GetComponent<Animator>();
		}
		if (m_Character == null)
		{
			m_Character = GetComponent<Character>();
		}
		if (m_SnorePrefab == null)
		{
			m_SnorePrefab = Resources.Load("Prefabs/Animation/Snore") as GameObject;
		}
		if (m_KnockoutPrefab == null)
		{
			m_KnockoutPrefab = Resources.Load("Prefabs/Animation/Knockout") as GameObject;
		}
	}

	private void Start()
	{
		if (m_NetView == null)
		{
			m_NetView = GetComponent<T17NetView>();
			if (m_NetView == null && Helpers.IsInGameplayScene())
			{
				Debug.LogWarningFormat("CharacterAnimator.Start: Failed to find NetView : {0}", base.gameObject.name);
			}
		}
		SetUpAnimStateLookup();
		StartAnimation(AnimState.Idle);
		if (m_Spotlight != null)
		{
			m_Spotlight.enabled = false;
		}
		float num = 0f;
		for (int i = 0; i < m_PossibleStandingStillAnimStates.Length; i++)
		{
			num += m_PossibleStandingStillAnimStates[i].m_Chance;
		}
		if (num > 0f && num != 1f)
		{
			float num2 = 1f / num;
			for (int i = 0; i < m_PossibleStandingStillAnimStates.Length; i++)
			{
				m_PossibleStandingStillAnimStates[i].m_Chance *= num2;
			}
		}
		m_AnimatorFacingDirection = Directionx4.Up;
	}

	public void OnAnimatorEnabled()
	{
		SetUpAnimStateLookup();
		CharacterSpeedChanged(m_eSpeed, force: true);
		CombatStateChanged(m_eCombatState, force: true);
		HeadAndBodyFaceDirection(m_eDirectionx4, force: true);
		TrayStateChanged(m_eTrayState, force: true);
		CombatLockedState(m_bLockedOn, force: true);
		UpdateSpotlight();
		if (m_Character == null || m_Character.GetPermissionToForceAnimatorUpdateOnEnable())
		{
			ForceStateUpdate();
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_AccessoryMaterial != null)
		{
			UnityEngine.Object.Destroy(m_AccessoryMaterial);
		}
		m_SnorePrefab = null;
		m_KnockoutPrefab = null;
		if (m_AnimStateToCharacterIndex != null)
		{
			m_AnimStateToCharacterIndex.Clear();
			m_AnimStateToCharacterIndex = null;
		}
		if (m_ChargeAttack_DashVFX != null)
		{
			if (EffectManager.GetInstance() != null)
			{
				EffectManager.GetInstance().ReturnEffect(m_ChargeAttack_DashVFX);
			}
			m_ChargeAttack_DashVFX = null;
		}
		if (m_ChargeAttack_ChargeVFX != null)
		{
			if (EffectManager.GetInstance() != null)
			{
				EffectManager.GetInstance().ReturnEffect(m_ChargeAttack_ChargeVFX);
			}
			m_ChargeAttack_ChargeVFX = null;
		}
		m_CharacterAnimator = null;
		m_Character = null;
		m_NetView = null;
	}

	public void SetCharacterAnimatorType(ANIMATOR_TYPE type)
	{
		m_AnimatorType = type;
	}

	public ANIMATOR_TYPE GetCharacterAnimatorType()
	{
		return m_AnimatorType;
	}

	public void ResetAnims()
	{
		if (m_CharacterAnimator != null)
		{
			CharacterSpeedChanged(CharacterSpeed.Stand, force: true);
			CombatStateChanged(CombatState.UnarmedCombat, force: true);
			HeadAndBodyFaceDirection(Directionx4.Up, force: true);
			TrayStateChanged(TrayState.Without, force: true);
			CombatLockedState(lockedOn: false, force: true);
		}
		m_OneShotTimer = 0f;
		OnOneShotDone = null;
		m_bIsCharging = false;
		m_AnimStateStack.Clear();
		m_AnimStateStack.Add(AnimState.Idle);
		m_eAnimState = AnimState.Idle;
		UpdateState();
	}

	public void SetUseLens(bool bYes)
	{
		m_bUseLensMaterial = bYes;
	}

	protected virtual void SetUpAnimStateLookup()
	{
		if (m_AnimStateToCharacterIndex == null)
		{
			m_AnimStateToCharacterIndex = new Dictionary<AnimState, int>(AnimStateTComparer);
			for (int i = 0; i < 150; i++)
			{
				AnimState key = (AnimState)i;
				string text = key.ToString();
				int value = Animator.StringToHash(text);
				m_AnimStateToCharacterIndex.Add(key, value);
			}
		}
	}

	public void SetSpotlightDirection(Directionx8 directionx8, Vector2 vFacingDirection)
	{
		if (m_Spotlight == null || !m_SpotlightsActive)
		{
			return;
		}
		if (m_SpotlightTrackingMode == SpotlightTracking.Perfect)
		{
			m_Spotlight.transform.rotation = Quaternion.LookRotation(vFacingDirection);
		}
		else if (m_eDirectionx8 != directionx8)
		{
			m_eDirectionx8 = directionx8;
			if (m_SpotlightTrackingMode == SpotlightTracking.EightDirections)
			{
				m_Spotlight.transform.rotation = Direction.DirectionToRotation(m_eDirectionx8);
			}
			else
			{
				m_Spotlight.transform.rotation = Direction.DirectionToRotation(m_eDirectionx4);
			}
		}
	}

	public float GetAnimationLength(AnimState animState)
	{
		return m_AnimStateData.GetAnimationTime(animState);
	}

	public void StartAnimation(AnimState animState)
	{
		if (animState != AnimState.COUNT && animState != AnimState.INVALID)
		{
			RequestAnimation(animState);
		}
	}

	public void DoAttackAnimation(bool normalAttack, bool playRandom = false)
	{
		if (m_bAttackPending != 0)
		{
			return;
		}
		AnimState animState = ((!normalAttack) ? AnimState.CombatAttack2 : AnimState.CombatAttack1);
		if (playRandom && UnityEngine.Random.value > 0.5f)
		{
			animState = ((!normalAttack) ? AnimState.CombatAttack1 : AnimState.CombatAttack2);
		}
		StartAnimation(animState);
		if (m_eAnimState == animState)
		{
			m_bAttackPending = (normalAttack ? AttackState.NormalAttack : AttackState.HeavyAttack);
			m_fAttackPendingTimer = ((!normalAttack) ? 0.15f : 0.15f);
			if (!normalAttack)
			{
				Vector3 vector = ((!(m_CharacterAnimator.transform.position.z < m_BottomOfMapThreshold)) ? m_ChargeAttackImpactOffsets[(int)m_eDirectionx4 / 2] : m_ChargeAttackImpactOffsetsBottomMap[(int)m_eDirectionx4 / 2]);
				Vector3 position = m_CharacterAnimator.transform.position + vector;
				EffectManager.PlayEffect(EffectManager.effectType.ChargeAttackImpact, position, null, m_Character.GetCharacterID());
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Combat_Super_Charge_Hit, m_Character.gameObject);
			}
		}
	}

	[PunRPC]
	public void RPC_StartOneShotAnimation(AnimState animState, PhotonMessageInfo info)
	{
		RequestAnimation(animState);
	}

	public void StopAnimation(AnimState animState)
	{
		if (animState != AnimState.COUNT && animState != AnimState.Idle && animState != AnimState.INVALID && m_AnimStateStack != null && m_AnimStateStack.Count != 0)
		{
			if (m_AnimStateStack[0] == animState && m_AnimStateStack.Count > 1)
			{
				m_eAnimState = m_AnimStateStack[1];
				UpdateState();
			}
			if (m_AnimStateStack.Contains(animState))
			{
				m_AnimStateStack.Remove(animState);
			}
		}
	}

	public void StopOneShotAnim(AnimState oneShotAnim)
	{
		if (m_AnimStateStack != null && m_AnimStateStack.Count != 0 && m_eAnimState == oneShotAnim)
		{
			m_eAnimState = m_AnimStateStack[0];
			UpdateState();
		}
	}

	private void RequestAnimation(AnimState animState)
	{
		int count = m_AnimStateStack.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_AnimStateStack[i] == animState)
			{
				return;
			}
		}
		if (m_AnimStateToCharacterIndex == null)
		{
			return;
		}
		int value = -1;
		if (!m_AnimStateToCharacterIndex.TryGetValue(m_eAnimState, out value))
		{
			return;
		}
		short priority = m_AnimStateData.GetPriority(animState);
		int num = 0;
		for (int num2 = m_AnimStateStack.Count - 1; num2 >= 0; num2--)
		{
			short priority2 = m_AnimStateData.GetPriority(m_AnimStateStack[num2]);
			if (priority < priority2)
			{
				num = num2 + 1;
				break;
			}
		}
		if (m_AnimStateData.GetIsAnimationOneShot(animState))
		{
			if (num <= 0)
			{
				m_eAnimState = animState;
				m_OneShotTimer = m_AnimStateData.GetAnimationTime(animState);
				UpdateState();
			}
			return;
		}
		m_AnimStateStack.Insert(num, animState);
		if (num == 0)
		{
			m_eAnimState = m_AnimStateStack[0];
			m_OneShotTimer = 0f;
			UpdateState();
		}
	}

	public void NetStateChanged(AnimState animState)
	{
		if (m_AnimStateData.GetIsAnimationOneShot(animState))
		{
			m_eAnimState = animState;
			UpdateState();
		}
		else if (m_eAnimState != animState)
		{
			m_eAnimState = animState;
			UpdateState();
		}
	}

	public void ForceStateUpdate()
	{
		UpdateState();
	}

	public void PostCullingObjetCollectorEnabled()
	{
		InteractiveObject interactiveObject = ((!m_Character.m_NetView.isMine) ? m_Character.GetRemoteInteractiveObject() : m_Character.GetInteractiveObject());
		if (interactiveObject != null && interactiveObject.ShouldResetAnimatorWithInteractiveUser())
		{
			interactiveObject.ForceNormalisedAnimTime(0f);
		}
	}

	public void HeadAndBodyFaceDirection(Directionx4 directionx4, bool force = false)
	{
		if (m_eDirectionx4 != directionx4 || force)
		{
			m_eDirectionx4 = directionx4;
			m_AnimatorFacingDirection = m_eDirectionx4;
			if (m_CharacterAnimator.enabled)
			{
				m_CharacterAnimator.SetFloat("x4Direction", (int)directionx4);
			}
			if (m_bShowingKnockOut && m_Knockout != null && m_Knockout.m_Animator != null)
			{
				m_Knockout.m_Animator.SetFloat("x4Direction", (int)directionx4);
			}
			if (m_bShowingSnore && m_Snore != null && m_Snore.m_Animator != null)
			{
				m_Snore.m_Animator.SetFloat("x4Direction", (int)directionx4);
			}
		}
	}

	public void CharacterSpeedChanged(CharacterSpeed speed, bool force = false)
	{
		if (m_eSpeed != speed || force)
		{
			if (null != m_NetView && null != m_Character && m_NetView.isMine && m_eSpeed != speed && (m_eSpeed == CharacterSpeed.Stand || speed == CharacterSpeed.Stand))
			{
				m_Character.m_SerializeRateOverride = CharacterSerializer.CharacterSerializerListType.High;
			}
			m_eSpeed = speed;
			if (m_CharacterAnimator.enabled)
			{
				m_CharacterAnimator.SetFloat("Speed", (float)speed);
			}
		}
	}

	public void CombatStateChanged(CombatState combatState, bool force = false)
	{
		if (m_eCombatState != combatState || force)
		{
			m_eCombatState = combatState;
			if (m_CharacterAnimator.enabled)
			{
				m_CharacterAnimator.SetFloat("CombatState", (float)combatState);
			}
		}
	}

	public void CombatLockedState(bool lockedOn, bool force = false)
	{
		if (m_bLockedOn != lockedOn || force)
		{
			m_bLockedOn = lockedOn;
			if (m_CharacterAnimator.enabled)
			{
				m_CharacterAnimator.SetFloat("CombatLocked", (!lockedOn) ? 0f : 1f);
			}
		}
	}

	public void TrayStateChanged(TrayState trayState, bool force = false)
	{
		if (m_eTrayState != trayState || force)
		{
			m_eTrayState = trayState;
			if (m_CharacterAnimator.enabled)
			{
				m_CharacterAnimator.SetFloat("Tray", (float)trayState);
			}
		}
	}

	public void SetIsCharging(bool charging)
	{
		m_bIsCharging = charging;
	}

	public bool IsCharging()
	{
		return m_bIsCharging;
	}

	protected virtual void UpdateState()
	{
		int value = -1;
		if (m_AnimStateToCharacterIndex != null && m_AnimStateToCharacterIndex.TryGetValue(m_eAnimState, out value) && m_CharacterAnimator != null)
		{
			m_fAttackPendingTimer = 0f;
			m_bAttackPending = AttackState.None;
			if (m_CharacterAnimator.enabled)
			{
				m_CharacterAnimator.CrossFade(value, 0f, 0, 0f);
				m_bHasUnappliedStateChange = false;
			}
			else
			{
				m_bHasUnappliedStateChange = true;
			}
			ShowKnockedOut(m_eAnimState == AnimState.Knockout || m_eAnimState == AnimState.KnockoutHold);
		}
	}

	public void ControlledUpdate()
	{
		if ((base.gameObject != null && !base.gameObject.activeSelf) || m_CharacterAnimator == null || m_Character == null)
		{
			return;
		}
		if (m_OneShotTimer > 0f)
		{
			m_OneShotTimer -= UpdateManager.deltaTime;
			if (m_OneShotTimer < 0f)
			{
				if (OnOneShotDone != null)
				{
					OnOneShotDone();
					OnOneShotDone = null;
				}
				StopOneShotAnim(m_eAnimState);
			}
		}
		if (m_fAttackPendingTimer > 0f)
		{
			m_fAttackPendingTimer -= UpdateManager.deltaTime;
			if (m_fAttackPendingTimer <= 0f)
			{
				m_Character.AttackAnimationDoDamageEvent(m_bAttackPending == AttackState.NormalAttack, Character.GamelogicRunModes.NonAudioOnly);
				m_bAttackPending = AttackState.None;
			}
		}
		bool flag = !m_bIsCharging && m_eAnimState == AnimState.CombatCharge && m_eSpeed == CharacterSpeed.Run;
		if (flag && m_ChargeAttack_DashVFX == null)
		{
			if (EffectManager.GetInstance() != null)
			{
				m_ChargeAttack_DashVFX = EffectManager.GetInstance().PlayEffect_LocalOnly(EffectManager.effectType.FeetChargeDash, m_CharacterAnimator.transform.position, m_CharacterAnimator.transform.parent, m_Character);
			}
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Play_Player_Combat_Charge_Dash, m_Character.gameObject);
		}
		else if (!flag && m_ChargeAttack_DashVFX != null)
		{
			if (EffectManager.GetInstance() != null)
			{
				EffectManager.GetInstance().ReturnEffect(m_ChargeAttack_DashVFX);
			}
			m_ChargeAttack_DashVFX = null;
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_Play_Player_Combat_Charge_Dash, m_Character.gameObject);
		}
		if (m_bIsCharging)
		{
			Vector3 vector = ((!(m_CharacterAnimator.transform.position.z < m_BottomOfMapThreshold)) ? m_ChargeAttackOffsets[(int)m_eDirectionx4 / 2] : m_ChargeAttackOffsetsBottomMap[(int)m_eDirectionx4 / 2]);
			if (m_ChargeAttack_ChargeVFX == null)
			{
				Vector3 position = m_CharacterAnimator.transform.position + vector;
				if (EffectManager.GetInstance() != null)
				{
					m_ChargeAttack_ChargeVFX = EffectManager.GetInstance().PlayEffect_LocalOnly(EffectManager.effectType.FistCharge, position, m_CharacterAnimator.transform.parent, m_Character);
				}
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Combat_Super_Charge_Up, m_Character.gameObject);
			}
			else
			{
				Vector3 position2 = m_CharacterAnimator.transform.position + vector;
				m_ChargeAttack_ChargeVFX.transform.position = position2;
			}
		}
		else if (!m_bIsCharging && m_ChargeAttack_ChargeVFX != null)
		{
			if (EffectManager.GetInstance() != null)
			{
				EffectManager.GetInstance().ReturnEffect(m_ChargeAttack_ChargeVFX);
			}
			m_ChargeAttack_ChargeVFX = null;
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_Player_Combat_Super_Charge_Up, m_Character.gameObject);
		}
		if (m_bControllingNormalizedTime)
		{
			SetNormaisedTime(m_AnimatorNormalizedTime);
		}
	}

	public void ControlledFixedUpdate()
	{
	}

	public void InitMaterialSetting()
	{
		m_MaterialRenderers.Clear();
		SkinnedMeshRenderer componentInChildren = base.gameObject.transform.GetComponentInChildren<SkinnedMeshRenderer>();
		if (componentInChildren == null)
		{
			MeshRenderer[] componentsInChildren = base.gameObject.transform.GetComponentsInChildren<MeshRenderer>();
			Dictionary<Material, List<MeshRenderer>> dictionary = new Dictionary<Material, List<MeshRenderer>>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Material sharedMaterial = componentsInChildren[i].sharedMaterial;
				List<MeshRenderer> value = null;
				if (sharedMaterial == null)
				{
					Character component = GetComponent<Character>();
					if (!(null != component))
					{
					}
					continue;
				}
				dictionary.TryGetValue(sharedMaterial, out value);
				if (value == null)
				{
					value = new List<MeshRenderer>();
				}
				value.Add(componentsInChildren[i]);
				dictionary[sharedMaterial] = value;
			}
			foreach (Material key in dictionary.Keys)
			{
				MaterialRenderers materialRenderers = new MaterialRenderers();
				materialRenderers.material = key;
				materialRenderers.preFabMaterial = key;
				materialRenderers.renderers = dictionary[key];
				m_MaterialRenderers.Add(materialRenderers);
			}
			m_bInitedMaterials = true;
			Transform[] componentsInChildren2 = base.gameObject.transform.GetComponentsInChildren<Transform>();
			if (componentsInChildren2 != null)
			{
				for (int i = 0; i < componentsInChildren2.Length; i++)
				{
					if (componentsInChildren2[i].name.Equals("HeadVFX_01"))
					{
						m_HeadAttachPoint = componentsInChildren2[i];
						break;
					}
				}
			}
		}
		else
		{
			for (int i = 0; i < componentInChildren.sharedMaterials.Length; i++)
			{
				MaterialRenderers materialRenderers2 = new MaterialRenderers();
				materialRenderers2.material = componentInChildren.sharedMaterials[i];
				materialRenderers2.renderers = null;
				materialRenderers2.preFabMaterial = materialRenderers2.material;
				m_MaterialRenderers.Add(materialRenderers2);
			}
			m_bInitedMaterials = true;
			Transform[] componentsInChildren3 = base.gameObject.transform.GetComponentsInChildren<Transform>();
			if (componentsInChildren3 != null)
			{
				for (int i = 0; i < componentsInChildren3.Length; i++)
				{
					if (componentsInChildren3[i].name.Equals("HeadVFX_01"))
					{
						m_HeadAttachPoint = componentsInChildren3[i];
						break;
					}
				}
			}
		}
		SetMaterialHandHeld(null);
		SetMaterialTorch(null);
	}

	public void GetCoreDirectionMeshRenderers(List<CullingObjectCollector.CharacterWrapper.MeshRendererTransform>[] coreRenderers)
	{
		MaterialRenderers materialRenderers = null;
		if (!m_bInitedMaterials)
		{
			InitMaterialSetting();
		}
		m_bAlteredAnimatorMaterials = false;
		switch (m_AnimatorType)
		{
		case ANIMATOR_TYPE.AT_NOT_SET:
			break;
		case ANIMATOR_TYPE.AT_CAMERAMAN:
		case ANIMATOR_TYPE.AT_DOG:
		{
			for (int i = 0; i < m_MaterialRenderers.Count; i++)
			{
				materialRenderers = m_MaterialRenderers[i];
				if (materialRenderers.mrtList == null)
				{
					int count = materialRenderers.renderers.Count;
					materialRenderers.mrtList = new CullingObjectCollector.CharacterWrapper.MeshRendererTransform[count];
					for (int k = 0; k < count; k++)
					{
						materialRenderers.mrtList[k] = null;
					}
				}
				for (int j = 0; j < materialRenderers.renderers.Count; j++)
				{
					MeshRenderer meshRenderer2 = materialRenderers.renderers[j];
					CullingObjectCollector.CharacterWrapper.MeshRendererTransform meshRendererTransform4 = new CullingObjectCollector.CharacterWrapper.MeshRendererTransform();
					meshRendererTransform4.m_MeshRenderer = meshRenderer2;
					meshRendererTransform4.m_Transform = meshRenderer2.transform;
					meshRendererTransform4.m_Textures = new Texture[1] { meshRenderer2.material.mainTexture };
					meshRendererTransform4.m_bCanRender = meshRenderer2.material != null;
					materialRenderers.mrtList[j] = meshRendererTransform4;
					coreRenderers[0].Add(meshRendererTransform4);
					coreRenderers[1].Add(meshRendererTransform4);
					coreRenderers[2].Add(meshRendererTransform4);
					coreRenderers[3].Add(meshRendererTransform4);
				}
			}
			break;
		}
		case ANIMATOR_TYPE.AT_CLIVE:
		{
			for (int i = 0; i < 9; i++)
			{
				materialRenderers = m_MaterialRenderers[i];
				if (materialRenderers.renderers == null)
				{
					continue;
				}
				for (int j = 0; j < materialRenderers.renderers.Count; j++)
				{
					MeshRenderer meshRenderer = materialRenderers.renderers[j];
					switch ((AVATAR_MATERIAL_CHANNELS)i)
					{
					case AVATAR_MATERIAL_CHANNELS.AMC_GENERIC:
					case AVATAR_MATERIAL_CHANNELS.AMC_FACE_ACC_HAIR:
						if (meshRenderer.material.mainTexture != null)
						{
							CullingObjectCollector.CharacterWrapper.MeshRendererTransform meshRendererTransform3 = new CullingObjectCollector.CharacterWrapper.MeshRendererTransform();
							meshRendererTransform3.m_MeshRenderer = meshRenderer;
							meshRendererTransform3.m_Transform = meshRenderer.transform;
							meshRendererTransform3.m_Textures = new Texture[1] { meshRenderer.material.mainTexture };
							meshRendererTransform3.m_bCanRender = meshRenderer.material != null;
							coreRenderers[0].Add(meshRendererTransform3);
							coreRenderers[1].Add(meshRendererTransform3);
							coreRenderers[2].Add(meshRendererTransform3);
							coreRenderers[3].Add(meshRendererTransform3);
						}
						break;
					case AVATAR_MATERIAL_CHANNELS.AMC_SHOVEL:
					case AVATAR_MATERIAL_CHANNELS.AMC_MOP:
					case AVATAR_MATERIAL_CHANNELS.AMC_TOOL:
					{
						CullingObjectCollector.CharacterWrapper.MeshRendererTransform meshRendererTransform2 = new CullingObjectCollector.CharacterWrapper.MeshRendererTransform();
						meshRendererTransform2.m_MeshRenderer = meshRenderer;
						meshRendererTransform2.m_Transform = meshRenderer.transform;
						meshRendererTransform2.m_Textures = new Texture[1] { meshRenderer.material.mainTexture };
						meshRendererTransform2.m_bCanRender = meshRenderer.material != null;
						coreRenderers[0].Add(meshRendererTransform2);
						coreRenderers[1].Add(meshRendererTransform2);
						coreRenderers[2].Add(meshRendererTransform2);
						coreRenderers[3].Add(meshRendererTransform2);
						break;
					}
					case AVATAR_MATERIAL_CHANNELS.AMC_LENS:
					{
						if (!m_bUseLensMaterial)
						{
							meshRenderer.enabled = false;
							break;
						}
						CullingObjectCollector.CharacterWrapper.MeshRendererTransform meshRendererTransform = new CullingObjectCollector.CharacterWrapper.MeshRendererTransform();
						meshRendererTransform.m_MeshRenderer = meshRenderer;
						meshRendererTransform.m_Transform = meshRenderer.transform;
						meshRendererTransform.m_Textures = new Texture[1] { meshRenderer.material.mainTexture };
						meshRendererTransform.m_bCanRender = meshRenderer.material != null;
						coreRenderers[0].Add(meshRendererTransform);
						coreRenderers[1].Add(meshRendererTransform);
						coreRenderers[2].Add(meshRendererTransform);
						coreRenderers[3].Add(meshRendererTransform);
						break;
					}
					}
				}
			}
			break;
		}
		}
	}

	public void GetActionMeshRenderers(List<CullingObjectCollector.CharacterWrapper.MeshRendererTransform> actionRenderers)
	{
		MaterialRenderers materialRenderers = null;
		if (!m_bInitedMaterials)
		{
			InitMaterialSetting();
		}
		m_bAlteredAnimatorMaterials = false;
		ANIMATOR_TYPE animatorType = m_AnimatorType;
		if (animatorType == ANIMATOR_TYPE.AT_DOG || animatorType == ANIMATOR_TYPE.AT_CAMERAMAN || animatorType != ANIMATOR_TYPE.AT_CLIVE)
		{
			return;
		}
		for (int i = 0; i < 9; i++)
		{
			materialRenderers = m_MaterialRenderers[i];
			if (materialRenderers.renderers == null)
			{
				continue;
			}
			for (int j = 0; j < materialRenderers.renderers.Count; j++)
			{
				MeshRenderer meshRenderer = materialRenderers.renderers[j];
				switch ((AVATAR_MATERIAL_CHANNELS)i)
				{
				case AVATAR_MATERIAL_CHANNELS.AMC_HAMMER:
				case AVATAR_MATERIAL_CHANNELS.AMC_SWOOSH:
				case AVATAR_MATERIAL_CHANNELS.AMC_ROPE:
				{
					Material material = meshRenderer.material;
					if (material.mainTexture != null)
					{
						CullingObjectCollector.CharacterWrapper.MeshRendererTransform meshRendererTransform = new CullingObjectCollector.CharacterWrapper.MeshRendererTransform();
						meshRendererTransform.m_MeshRenderer = meshRenderer;
						meshRendererTransform.m_Transform = meshRenderer.transform;
						meshRendererTransform.m_Textures = new Texture[1] { material.mainTexture };
						meshRendererTransform.m_bCanRender = material != null;
						actionRenderers.Add(meshRendererTransform);
					}
					break;
				}
				case AVATAR_MATERIAL_CHANNELS.AMC_LENS:
					meshRenderer.enabled = false;
					break;
				}
			}
		}
	}

	public void GetAccessoryMeshRenderers(List<CullingObjectCollector.CharacterWrapper.MeshRendererTransform>[] accRenderersList)
	{
		MaterialRenderers materialRenderers = null;
		if (!m_bInitedMaterials)
		{
			InitMaterialSetting();
		}
		m_bAlteredAnimatorMaterials = false;
		ANIMATOR_TYPE animatorType = m_AnimatorType;
		if (animatorType == ANIMATOR_TYPE.AT_DOG || animatorType == ANIMATOR_TYPE.AT_CAMERAMAN || animatorType != ANIMATOR_TYPE.AT_CLIVE)
		{
			return;
		}
		for (int i = 0; i < 9; i++)
		{
			materialRenderers = m_MaterialRenderers[i];
			if (materialRenderers.renderers == null)
			{
				continue;
			}
			for (int j = 0; j < materialRenderers.renderers.Count; j++)
			{
				MeshRenderer meshRenderer = materialRenderers.renderers[j];
				switch ((AVATAR_MATERIAL_CHANNELS)i)
				{
				case AVATAR_MATERIAL_CHANNELS.AMC_FACE_ACC_HAIR:
				{
					CullingObjectCollector.CharacterWrapper.MeshRendererTransform meshRendererTransform = new CullingObjectCollector.CharacterWrapper.MeshRendererTransform();
					meshRendererTransform.m_MeshRenderer = meshRenderer;
					meshRendererTransform.m_Transform = meshRenderer.transform;
					meshRendererTransform.m_Textures = new Texture[5];
					meshRendererTransform.m_bCanRender = meshRenderer.material != null;
					accRenderersList[0].Add(meshRendererTransform);
					accRenderersList[1].Add(meshRendererTransform);
					accRenderersList[2].Add(meshRendererTransform);
					accRenderersList[3].Add(meshRendererTransform);
					break;
				}
				}
			}
		}
	}

	public void SetMaterialAppearance(Material bodyMaterial, Material hairMaterial, Material hatMaterial, Material upperFaceMaterial, Material lowerFaceMaterial)
	{
		if (m_AnimatorType != ANIMATOR_TYPE.AT_CAMERAMAN)
		{
			if (!m_bInitedMaterials)
			{
				InitMaterialSetting();
			}
			SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_GENERIC, bodyMaterial, bControlEnabledFlag: true);
			SetMaterialAccessories(hairMaterial, hatMaterial, upperFaceMaterial, lowerFaceMaterial);
			if (m_AnimatorType == ANIMATOR_TYPE.AT_CLIVE)
			{
				SetMaterialTorch(null);
			}
			m_bAlteredAnimatorMaterials = true;
		}
	}

	private void SetMaterialAccessories(Material hair, Material hat, Material upperFace, Material lowerFace)
	{
		if (!m_InitedAccPropertyIDs)
		{
			m_propId_Head_UVRefTex = Shader.PropertyToID("_MainTex");
			m_propId_Head_HairTex = Shader.PropertyToID("_HairTex");
			m_propId_Head_HairMaskTex = Shader.PropertyToID("_HairMaskTex");
			m_propId_Head_HatTex = Shader.PropertyToID("_HatTex");
			m_propId_Head_UpperAccTex = Shader.PropertyToID("_UpperAccTex");
			m_propId_Head_LowerAccTex = Shader.PropertyToID("_LowerAccTex");
			m_InitedAccPropertyIDs = true;
		}
		if (!m_bInitedAccMaterial)
		{
			int num = 1;
			if (num >= 0 && num < m_MaterialRenderers.Count)
			{
				MaterialRenderers materialRenderers = m_MaterialRenderers[num];
				if (materialRenderers != null && materialRenderers.preFabMaterial != null)
				{
					if (m_AccessoryMaterial != null)
					{
						UnityEngine.Object.Destroy(m_AccessoryMaterial);
					}
					m_AccessoryMaterial = new Material(materialRenderers.preFabMaterial);
					m_bInitedAccMaterial = true;
				}
			}
			if (!m_bInitedAccMaterial)
			{
				return;
			}
			m_AccessoryData = new AccessoryData();
		}
		if (hair != null || hat != null || upperFace != null || lowerFace != null)
		{
			Texture texture = ExtractTextureFromMaterialProperty(hair, m_propId_Head_HairTex);
			Texture texture2 = ExtractTextureFromMaterialProperty(hat, m_propId_Head_HatTex);
			Texture texture3 = ExtractTextureFromMaterialProperty(upperFace, m_propId_Head_UpperAccTex);
			Texture texture4 = ExtractTextureFromMaterialProperty(lowerFace, m_propId_Head_LowerAccTex);
			Texture texture5 = ExtractTextureFromMaterialProperty(hat, m_propId_Head_HairMaskTex);
			if (texture5 == null)
			{
				texture5 = ExtractTextureFromMaterialProperty(hair, m_propId_Head_HairMaskTex);
			}
			if (texture2 == null || texture5 == null)
			{
				int index = 1;
				Material preFabMaterial = m_MaterialRenderers[index].preFabMaterial;
				texture5 = ExtractTextureFromMaterialProperty(preFabMaterial, m_propId_Head_HairMaskTex);
			}
			CacheAccessoryData(texture5, texture, texture2, texture3, texture4);
			m_AccessoryMaterial.SetTexture(m_propId_Head_HairTex, texture);
			m_AccessoryMaterial.SetTexture(m_propId_Head_HairMaskTex, texture5);
			m_AccessoryMaterial.SetTexture(m_propId_Head_HatTex, texture2);
			m_AccessoryMaterial.SetTexture(m_propId_Head_UpperAccTex, texture3);
			m_AccessoryMaterial.SetTexture(m_propId_Head_LowerAccTex, texture4);
			SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_FACE_ACC_HAIR, m_AccessoryMaterial, bControlEnabledFlag: true);
		}
		else
		{
			SkinnedMeshRenderer componentInChildren = base.gameObject.transform.GetComponentInChildren<SkinnedMeshRenderer>();
			if (componentInChildren != null)
			{
				int index2 = 1;
				Material preFabMaterial2 = m_MaterialRenderers[index2].preFabMaterial;
				Texture texture6 = ExtractTextureFromMaterialProperty(preFabMaterial2, m_propId_Head_HairMaskTex);
				CacheAccessoryData(texture6, null, null, null, null);
				m_AccessoryMaterial.SetTexture(m_propId_Head_HairMaskTex, texture6);
				m_AccessoryMaterial.SetTexture(m_propId_Head_HairTex, null);
				m_AccessoryMaterial.SetTexture(m_propId_Head_HatTex, null);
				m_AccessoryMaterial.SetTexture(m_propId_Head_UpperAccTex, null);
				m_AccessoryMaterial.SetTexture(m_propId_Head_LowerAccTex, null);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_FACE_ACC_HAIR, m_AccessoryMaterial, bControlEnabledFlag: true);
			}
			else
			{
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_FACE_ACC_HAIR, null, bControlEnabledFlag: true);
			}
		}
	}

	private void CacheAccessoryData(Texture hairMask, Texture hairTex, Texture hatTex, Texture upperFaceTex, Texture lowerFaceTex)
	{
		if (m_AccessoryData != null)
		{
			m_AccessoryData.hairMask = hairMask;
			m_AccessoryData.hairTex = hairTex;
			m_AccessoryData.hatTex = hatTex;
			m_AccessoryData.upperFaceTex = upperFaceTex;
			m_AccessoryData.lowerFaceTex = lowerFaceTex;
			m_bRenderingDirty = true;
		}
	}

	private Texture ExtractTextureFromMaterialProperty(Material material, int propertyID)
	{
		if (material != null && material.HasProperty(propertyID))
		{
			return material.GetTexture(propertyID);
		}
		return null;
	}

	public void SetMaterialHandHeld(Material material, ItemData.ITEM_ANIMATION_TYPE type = ItemData.ITEM_ANIMATION_TYPE.IAT_NOT_SET)
	{
		if (m_AnimatorType != ANIMATOR_TYPE.AT_CAMERAMAN)
		{
			if (!m_bInitedMaterials)
			{
				InitMaterialSetting();
			}
			SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_LENS, null, bControlEnabledFlag: true);
			switch (type)
			{
			case ItemData.ITEM_ANIMATION_TYPE.IAT_NOT_SET:
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_TOOL, null, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_SHOVEL, null, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_HAMMER, null, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_MOP, null, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_ROPE, null, bControlEnabledFlag: true);
				break;
			case ItemData.ITEM_ANIMATION_TYPE.IAT_SINGLE:
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_TOOL, material, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_SHOVEL, null, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_HAMMER, null, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_MOP, null, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_ROPE, null, bControlEnabledFlag: true);
				break;
			case ItemData.ITEM_ANIMATION_TYPE.IAT_DOUBLE:
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_SHOVEL, material, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_TOOL, null, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_HAMMER, null, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_MOP, null, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_ROPE, null, bControlEnabledFlag: true);
				break;
			case ItemData.ITEM_ANIMATION_TYPE.IAT_MOP:
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_TOOL, null, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_SHOVEL, null, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_HAMMER, null, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_MOP, material, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_ROPE, null, bControlEnabledFlag: true);
				break;
			case ItemData.ITEM_ANIMATION_TYPE.IAT_HAMMER:
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_TOOL, null, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_SHOVEL, null, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_HAMMER, material, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_MOP, null, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_ROPE, null, bControlEnabledFlag: true);
				break;
			case ItemData.ITEM_ANIMATION_TYPE.IAT_ROPE:
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_TOOL, null, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_SHOVEL, null, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_HAMMER, null, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_MOP, null, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_ROPE, material, bControlEnabledFlag: true);
				break;
			case ItemData.ITEM_ANIMATION_TYPE.IAT_OTHER:
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_TOOL, null, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_SHOVEL, null, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_HAMMER, material, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_MOP, material, bControlEnabledFlag: true);
				SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_ROPE, null, bControlEnabledFlag: true);
				break;
			}
			m_bAlteredAnimatorMaterials = true;
		}
	}

	public void SetMaterialTorch(Material material)
	{
	}

	private void SetMaterial(AVATAR_MATERIAL_CHANNELS channel, Material material, bool bControlEnabledFlag = false)
	{
		if (channel < AVATAR_MATERIAL_CHANNELS.AMC_GENERIC || (int)channel >= m_MaterialRenderers.Count || m_AnimatorType == ANIMATOR_TYPE.AT_CAMERAMAN)
		{
			return;
		}
		MaterialRenderers materialRenderers = m_MaterialRenderers[(int)channel];
		if (materialRenderers == null)
		{
			return;
		}
		materialRenderers.material = material;
		bool flag = channel == AVATAR_MATERIAL_CHANNELS.AMC_FACE_ACC_HAIR;
		List<MeshRenderer> renderers = materialRenderers.renderers;
		if (renderers != null)
		{
			for (int i = 0; i < renderers.Count; i++)
			{
				renderers[i].material = materialRenderers.material;
				if (!bControlEnabledFlag)
				{
					continue;
				}
				renderers[i].enabled = material != null;
				if (materialRenderers.mrtList != null)
				{
					if (flag)
					{
						materialRenderers.mrtList[i].m_Textures[0] = ExtractTextureFromMaterialProperty(material, m_propId_Head_HairMaskTex);
						materialRenderers.mrtList[i].m_Textures[1] = ExtractTextureFromMaterialProperty(material, m_propId_Head_HairTex);
						materialRenderers.mrtList[i].m_Textures[2] = ExtractTextureFromMaterialProperty(material, m_propId_Head_HatTex);
						materialRenderers.mrtList[i].m_Textures[3] = ExtractTextureFromMaterialProperty(material, m_propId_Head_UpperAccTex);
						materialRenderers.mrtList[i].m_Textures[4] = ExtractTextureFromMaterialProperty(material, m_propId_Head_LowerAccTex);
					}
					else
					{
						materialRenderers.mrtList[i].m_Textures[0] = material.mainTexture;
					}
					materialRenderers.mrtList[i].m_bCanRender = material != null;
				}
			}
			return;
		}
		SkinnedMeshRenderer componentInChildren = base.gameObject.transform.GetComponentInChildren<SkinnedMeshRenderer>();
		ViewSkinnedData componentInChildren2 = base.gameObject.transform.GetComponentInChildren<ViewSkinnedData>();
		if ((bool)componentInChildren2)
		{
			if (material != null)
			{
				Material[] materials = componentInChildren.materials;
				materials[(int)channel] = material;
				componentInChildren.materials = materials;
			}
			else
			{
				componentInChildren.materials[(int)channel].mainTexture = componentInChildren2.m_Blank;
			}
		}
		else
		{
			componentInChildren.sharedMaterials[(int)channel] = null;
		}
	}

	public bool IsSpotlightActive()
	{
		return m_SpotlightsActive;
	}

	public void ActivateSpotlightRoutine(bool enabled)
	{
		m_SpotlightsActiveRoutine = enabled;
		UpdateSpotlight();
		if (!enabled)
		{
			SetMaterial(AVATAR_MATERIAL_CHANNELS.AMC_LENS, null, bControlEnabledFlag: true);
		}
	}

	public void ShowSpotlightKnockedOut(bool enabled)
	{
		m_SpotlightsActiveKnockedOut = enabled;
		if (UpdateSpotlight() && m_SpotlightsActive)
		{
			m_Spotlight.transform.rotation = Direction.DirectionToRotation(m_eDirectionx8);
		}
	}

	public void Cutscenes_SetHijacked(bool isHijacked)
	{
		if (isHijacked)
		{
			m_SpotlightsActiveHijack = false;
		}
		else
		{
			m_SpotlightsActiveHijack = true;
		}
		UpdateSpotlight();
	}

	public bool UpdateSpotlight()
	{
		if (m_Spotlight == null)
		{
			return false;
		}
		bool flag = m_Spotlight.enabled;
		m_Spotlight.enabled = m_SpotlightsActive;
		return flag != m_Spotlight.enabled;
	}

	public AnimState GetRandomStandingStillAnimState(ref ItemData itemDataID)
	{
		int num = m_PossibleStandingStillAnimStates.Length;
		if (num > 1)
		{
			float num2 = UnityEngine.Random.Range(0f, 1f);
			for (int i = 0; i < num; i++)
			{
				if (m_PossibleStandingStillAnimStates[i].m_Chance >= num2)
				{
					itemDataID = m_PossibleStandingStillAnimStates[i].m_RequiredEquipItemData;
					return m_PossibleStandingStillAnimStates[i].m_State;
				}
				num2 -= m_PossibleStandingStillAnimStates[i].m_Chance;
			}
		}
		itemDataID = m_PossibleStandingStillAnimStates[0].m_RequiredEquipItemData;
		return m_PossibleStandingStillAnimStates[0].m_State;
	}

	public void ShowSleeping(bool bIsSleep)
	{
		if (!m_Snore)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_SnorePrefab, Vector3.zero, Quaternion.identity);
			m_Snore = gameObject.GetComponent<Snore>();
			m_Snore.transform.parent = m_HeadAttachPoint;
			m_Snore.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			m_Snore.gameObject.transform.localRotation = Quaternion.identity;
		}
		if (!m_Snore)
		{
			return;
		}
		if (bIsSleep)
		{
			Vector3 position = m_HeadAttachPoint.position;
			m_Snore.gameObject.transform.position = position;
			m_Snore.gameObject.SetActive(value: true);
			CullingObjectCollector instance = CullingObjectCollector.GetInstance();
			if (instance != null)
			{
				instance.AddRuntimeEffect(m_Snore.gameObject, m_Character);
			}
			m_bShowingSnore = true;
		}
		else
		{
			CullingObjectCollector instance2 = CullingObjectCollector.GetInstance();
			if (instance2 != null)
			{
				instance2.RemoveRuntimeEffect(m_Snore.gameObject);
			}
			m_Snore.gameObject.SetActive(value: false);
			m_bShowingSnore = false;
		}
	}

	private void ShowKnockedOut(bool bIsKnockedOut)
	{
		if (m_Knockout == null && bIsKnockedOut && m_KnockoutPrefab != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_KnockoutPrefab, Vector3.zero, Quaternion.identity);
			if (gameObject != null)
			{
				m_Knockout = gameObject.GetComponent<Knockout>();
				m_Knockout.transform.parent = m_HeadAttachPoint;
				m_Knockout.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
				m_Knockout.gameObject.transform.localRotation = Quaternion.identity;
			}
		}
		if (!m_Knockout)
		{
			return;
		}
		if (bIsKnockedOut)
		{
			Vector3 position = Vector3.zero;
			if (m_HeadAttachPoint != null)
			{
				position = m_HeadAttachPoint.position;
			}
			m_Knockout.gameObject.transform.position = position;
			m_Knockout.gameObject.SetActive(value: true);
			m_bShowingKnockOut = true;
			CullingObjectCollector instance = CullingObjectCollector.GetInstance();
			if (instance != null)
			{
				instance.AddRuntimeEffect(m_Knockout.gameObject, m_Character);
			}
		}
		else
		{
			m_Knockout.gameObject.SetActive(value: false);
			CullingObjectCollector instance2 = CullingObjectCollector.GetInstance();
			if (instance2 != null)
			{
				instance2.RemoveRuntimeEffect(m_Knockout.gameObject);
			}
			m_bShowingKnockOut = false;
		}
	}

	public bool IsAnimStateOneShot(AnimState state)
	{
		if (state == AnimState.COUNT || state == AnimState.INVALID)
		{
			return false;
		}
		return m_AnimStateData.GetIsAnimationOneShot(state);
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

	public void ChangeToUIMaterails()
	{
		MaterialRenderers materialRenderers = null;
		if (!m_bInitedMaterials)
		{
			InitMaterialSetting();
		}
		for (int i = 0; i < m_MaterialRenderers.Count; i++)
		{
			materialRenderers = m_MaterialRenderers[i];
			if (materialRenderers.material != null)
			{
				materialRenderers.material.EnableKeyword("T17_CHARACTER_UI");
			}
		}
	}

	public void SetMatBlock(MaterialPropertyBlock materialPropertyBlock)
	{
		MaterialRenderers materialRenderers = null;
		if (!m_bInitedMaterials)
		{
			InitMaterialSetting();
		}
		for (int i = 0; i < m_MaterialRenderers.Count; i++)
		{
			materialRenderers = m_MaterialRenderers[i];
			if (!(materialRenderers.material != null))
			{
				continue;
			}
			List<MeshRenderer> renderers = materialRenderers.renderers;
			if (renderers != null)
			{
				for (int j = 0; j < renderers.Count; j++)
				{
					renderers[j].SetPropertyBlock(materialPropertyBlock);
				}
			}
		}
	}

	public void SetNormaisedTime(float normalisedTime)
	{
		int value = -1;
		if (m_AnimStateToCharacterIndex != null && m_AnimStateToCharacterIndex.TryGetValue(m_eAnimState, out value) && m_CharacterAnimator != null)
		{
			m_fAttackPendingTimer = 0f;
			m_bAttackPending = AttackState.None;
			m_CharacterAnimator.CrossFade(value, 0f, 0, normalisedTime);
			m_AnimatorNormalizedTime = normalisedTime;
			m_CharacterAnimator.playbackTime = 0f;
			InteractiveObject interactiveObject = null;
			interactiveObject = ((!m_NetView.isMine) ? m_Character.CurrentDeserializedAnimatedInteraction : m_Character.GetInteractiveObject());
			if (interactiveObject != null)
			{
				interactiveObject.SetNormalizedAnimTime(normalisedTime);
			}
		}
	}

	public void FinishControllingNormalizedTime()
	{
		m_bControllingNormalizedTime = false;
		if (!(m_CharacterAnimator != null))
		{
			return;
		}
		m_fAttackPendingTimer = 0f;
		m_bAttackPending = AttackState.None;
		if (m_CharacterAnimator.enabled)
		{
			m_AnimatorNormalizedTime = 0f;
			m_CharacterAnimator.playbackTime = 1f;
			AnimatedInteraction animatedInteraction = null;
			animatedInteraction = ((!m_NetView.isMine) ? m_Character.CurrentDeserializedAnimatedInteraction : m_Character.CurrentSerializedAnimatedInteraction);
			if (animatedInteraction != null)
			{
				animatedInteraction.ResetNormalizedAnimTime();
			}
		}
	}

	public void BeginControllingNormalizedTime()
	{
		m_bControllingNormalizedTime = true;
	}

	public bool HasUnappliedStateChange()
	{
		return m_bHasUnappliedStateChange;
	}
}
