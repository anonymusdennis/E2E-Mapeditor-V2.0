using System.Diagnostics;
using BitStream;
using SaveHelpers;
using UnityEngine;

public class CharacterStats : T17MonoBehaviour, IControlledUpdate
{
	public delegate void StateChangedHandler(StatModifierEnum oldState, StatModifierEnum newState);

	public delegate void CharacterStatsEvent(float newStatValue);

	public enum MessageGameplayReasons
	{
		Unassigned,
		DidDamageDuringCombat,
		KnockedOut
	}

	private const float MIN_STAT_CHANGE = 0.001f;

	public const float MIN_MODIFIER_STAT_VALUE = 30f;

	public const float MAX_MODIFIER_STAT_VALUE = 70f;

	public StatModifierEnum m_CharacterState;

	public bool m_bIsPlayer = true;

	public const float MaxHealth = 100f;

	public const float MaxStrength = 100f;

	public const float MaxCardio = 100f;

	public const float MaxIntellect = 100f;

	public const float MaxEnergy = 100f;

	public const float MaxHeat = 100f;

	public const float MaxMoney = 999f;

	[Header("Baseline Stats")]
	public HiddenFloat m_HealthBaseLine = new HiddenFloat(100f);

	public HiddenFloat m_StrengthBaseLine = new HiddenFloat(50f);

	public HiddenFloat m_CardioBaseLine = new HiddenFloat(50f);

	public HiddenFloat m_IntellectBaseLine = new HiddenFloat(50f);

	public HiddenFloat m_EnergyBaseLine = new HiddenFloat(50f);

	public HiddenFloat m_HeatBaseLine = new HiddenFloat(0f);

	public HiddenFloat m_MoneyBaseLine = new HiddenFloat(0f);

	public HiddenInt m_SentenceBaseLine = new HiddenInt(30);

	[Header("Character Stats")]
	private HiddenFloat m_Health;

	private HiddenFloat m_Energy;

	private HiddenFloat m_Strength;

	private HiddenFloat m_Intellect;

	private HiddenFloat m_Cardio;

	private HiddenFloat m_Heat;

	private HiddenFloat m_Money;

	private HiddenInt m_RemainingSentence;

	private HiddenInt m_TimesSentToSolitary;

	private Character m_Character;

	[Header("Stat Decay Rates (per second)")]
	public HiddenFloat m_HealthRestoreRate = new HiddenFloat(0.25f);

	public HiddenFloat m_EnergyRestoreRate = new HiddenFloat(0.5f);

	public HiddenFloat m_EnergyRestoreRateBlocking = new HiddenFloat(0.2f);

	public HiddenFloat m_StrengthDecayRate = new HiddenFloat(0.5f);

	public HiddenFloat m_IntellectDecayRate = new HiddenFloat(0.5f);

	public HiddenFloat m_CardioDecayRate = new HiddenFloat(0.5f);

	public HiddenFloat m_HeatDecayRate = new HiddenFloat(0.5f);

	public float m_AIMaxSpeedBoost = 0.3f;

	private float m_SpeedMod;

	private T17NetView m_NetView;

	public CharacterStatsEvent OnHealthStatChanged;

	public CharacterStatsEvent OnMoneyStatChanged;

	public CharacterStatsEvent OnStrengthStatChanged;

	public CharacterStatsEvent OnIntellectStatChanged;

	public CharacterStatsEvent OnCardioStatChanged;

	public CharacterStatsEvent OnEnergyStatChanged;

	public CharacterStatsEvent OnHeatStatChanged;

	private static bool m_bInfinitePlayerEnergyOn;

	private static bool m_bInfinitePlayerHealthOn;

	private float m_LastSyncedHealth = -1f;

	private float m_LastSyncedEnergy = -1f;

	private float m_LastSyncedHeat = -1f;

	private bool m_bAnyMinorStatsChangedSinceLastWrite;

	public float Health
	{
		get
		{
			return m_Health.GetValue();
		}
		set
		{
			m_Health.SetValue(Mathf.Clamp(value, 0f, 100f));
		}
	}

	public float Energy
	{
		get
		{
			return m_Energy.GetValue();
		}
		set
		{
			m_Energy.SetValue(Mathf.Clamp(value, 0f, 100f));
		}
	}

	public EnergyModifier EnergyLevel
	{
		get
		{
			float num = Mathf.Clamp01((m_Cardio.GetValue() - 30f) / 40f);
			return (EnergyModifier)Mathf.FloorToInt(num * 4f);
		}
	}

	public float Strength
	{
		get
		{
			return m_Strength.GetValue();
		}
		set
		{
			m_Strength.SetValue(Mathf.Clamp(value, 0f, 100f));
		}
	}

	public StrengthModifier StrengthLevel
	{
		get
		{
			float num = Mathf.Clamp01((m_Strength.GetValue() - 30f) / 40f);
			return (StrengthModifier)Mathf.FloorToInt(num * 4f);
		}
	}

	public float Intellect
	{
		get
		{
			return m_Intellect.GetValue();
		}
		set
		{
			m_Intellect.SetValue(Mathf.Clamp(value, 0f, 100f));
		}
	}

	public float Cardio
	{
		get
		{
			return m_Cardio.GetValue();
		}
		set
		{
			m_Cardio.SetValue(Mathf.Clamp(value, 0f, 100f));
		}
	}

	public float Heat
	{
		get
		{
			return m_Heat.GetValue();
		}
		set
		{
			m_Heat.SetValue(Mathf.Clamp(value, 0f, 100f));
		}
	}

	public float Money
	{
		get
		{
			return m_Money.GetValue();
		}
		set
		{
			m_Money.SetValue(Mathf.Clamp(value, 0f, 999f));
		}
	}

	public int RemainingSentence
	{
		get
		{
			return m_RemainingSentence.GetValue();
		}
		set
		{
			m_RemainingSentence.SetValue(value);
		}
	}

	public int TimesSentToSolitary
	{
		get
		{
			return m_TimesSentToSolitary.GetValue();
		}
		set
		{
			m_TimesSentToSolitary.SetValue(value);
		}
	}

	public event StateChangedHandler StateChangedEvent;

	protected override void Awake()
	{
		base.Awake();
		m_NetView = GetComponent<T17NetView>();
		m_Character = GetComponent<Character>();
		LimitBaseline();
		m_Health = m_HealthBaseLine;
		m_Energy = m_EnergyBaseLine;
		m_Strength = m_StrengthBaseLine;
		m_Intellect = m_IntellectBaseLine;
		m_Cardio = m_CardioBaseLine;
		m_Heat = m_HeatBaseLine;
		m_Money = m_MoneyBaseLine;
		m_SpeedMod = 0f;
		m_RemainingSentence = m_SentenceBaseLine;
		m_TimesSentToSolitary = 0;
		if (!m_bIsPlayer)
		{
			m_SpeedMod = Random.Range(0f, 0.2f);
		}
	}

	protected virtual void OnDestroy()
	{
		m_NetView = null;
		m_Character = null;
		OnHealthStatChanged = null;
		OnMoneyStatChanged = null;
		OnStrengthStatChanged = null;
		OnIntellectStatChanged = null;
		OnCardioStatChanged = null;
		OnEnergyStatChanged = null;
		OnHeatStatChanged = null;
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		if (RoutineManager.GetInstance() != null)
		{
			RoutineManager.GetInstance().OnDaysFromStartTimeChange += DecrementRemainingSentence;
		}
		if (!T17NetManager.IsMasterClient)
		{
			m_NetView.RPC("RPC_Master_SendCharacterStatsInfoToClient", NetTargets.MasterClient);
		}
		return base.StartInit();
	}

	[PunRPC]
	private void RPC_Master_SendCharacterStatsInfoToClient(PhotonMessageInfo info)
	{
		ulong stats = 0uL;
		ulong stats2 = 0uL;
		if (Serialize(ref stats, ref stats2))
		{
			m_NetView.RPC("RPC_Client_RecieveCharacterStatsData", info.sender, (long)stats, (long)stats2);
		}
	}

	[PunRPC]
	private void RPC_Client_RecieveCharacterStatsData(long serializedValues, long serializedValues2, PhotonMessageInfo info)
	{
		Deserialize((ulong)serializedValues, (ulong)serializedValues2);
	}

	private void LimitBaseline()
	{
		m_HealthBaseLine.SetValue(Mathf.Min(m_HealthBaseLine.GetValue(), 100f));
		m_StrengthBaseLine.SetValue(Mathf.Min(m_StrengthBaseLine.GetValue(), 100f));
		m_CardioBaseLine.SetValue(Mathf.Min(m_CardioBaseLine.GetValue(), 100f));
		m_IntellectBaseLine.SetValue(Mathf.Min(m_IntellectBaseLine.GetValue(), 100f));
		m_EnergyBaseLine.SetValue(Mathf.Min(m_EnergyBaseLine.GetValue(), 100f));
		m_HeatBaseLine.SetValue(Mathf.Min(m_HeatBaseLine.GetValue(), 100f));
	}

	public void ApplyCharacterConfig(CharacterConfig config)
	{
		m_HealthBaseLine.SetValue(config.m_HealthBaseLine);
		m_EnergyBaseLine.SetValue(config.m_EnergyBaseLine);
		m_StrengthBaseLine.SetValue(config.m_StrengthBaseLine);
		m_CardioBaseLine.SetValue(config.m_CardioBaseLine);
		m_IntellectBaseLine.SetValue(config.m_IntellectBaseLine);
		m_HeatBaseLine.SetValue(config.m_HeatBaseLine);
		m_MoneyBaseLine.SetValue(config.m_MoneyBaseLine);
		m_SentenceBaseLine.SetValue(config.m_SentenceBaseLine);
		LimitBaseline();
		m_HealthRestoreRate.SetValue(config.m_HealthRestoreRate);
		m_EnergyRestoreRate.SetValue(config.m_EnergyRestoreRate);
		m_EnergyRestoreRateBlocking.SetValue(config.m_EnergyRestoreRateBlocking);
		m_StrengthDecayRate.SetValue(config.m_StrengthDecayRate);
		m_CardioDecayRate.SetValue(config.m_CardioDecayRate);
		m_IntellectDecayRate.SetValue(config.m_IntellectDecayRate);
		m_HeatDecayRate.SetValue(config.m_HeatDecayRate);
		m_Health.SetValue(m_HealthBaseLine);
		m_Energy.SetValue(m_EnergyBaseLine);
		m_Strength.SetValue(m_StrengthBaseLine);
		m_Intellect.SetValue(m_IntellectBaseLine);
		m_Cardio.SetValue(m_CardioBaseLine);
		m_Heat.SetValue(m_HeatBaseLine);
		m_Money.SetValue(m_MoneyBaseLine);
		m_SpeedMod = 0f;
		m_RemainingSentence.SetValue(m_SentenceBaseLine);
		m_TimesSentToSolitary.SetValue(0);
		if (!m_bIsPlayer)
		{
			m_SpeedMod = Random.Range(0f, 0.2f);
		}
	}

	public void ControlledUpdate()
	{
		if (!IsInited())
		{
			return;
		}
		float deltaTime = UpdateManager.deltaTime;
		float num = deltaTime * RoutineManager.GetInstance().GetFastForwardFactor();
		if (Health < (float)m_HealthBaseLine)
		{
			Health = Mathf.Min(m_HealthBaseLine, (float)m_Health + (float)m_HealthRestoreRate * num);
			if (m_bIsPlayer && m_bInfinitePlayerHealthOn)
			{
				Health = m_HealthBaseLine;
			}
		}
		if (Energy < (float)m_EnergyBaseLine)
		{
			float num2 = ((m_CharacterState != StatModifierEnum.Blocking) ? m_EnergyRestoreRate : m_EnergyRestoreRateBlocking);
			Energy = Mathf.Min(m_EnergyBaseLine, (float)m_Energy + num2 * num);
			if (m_bIsPlayer && m_bInfinitePlayerEnergyOn)
			{
				Energy = m_EnergyBaseLine;
			}
		}
		if (Heat > (float)m_HeatBaseLine)
		{
			int num3 = (int)Heat;
			Heat = Mathf.Max(m_HeatBaseLine, (float)m_Heat - (float)m_HeatDecayRate * num);
			if (num3 != (int)Heat && OnHeatStatChanged != null)
			{
				OnHeatStatChanged(Heat);
			}
		}
	}

	public void ControlledFixedUpdate()
	{
	}

	public bool HasEnoughEnergyForTask(float taskFatigue)
	{
		if (Energy - taskFatigue > 0f)
		{
			return true;
		}
		return false;
	}

	public void RestoreHealthRPC()
	{
		SetHealth(m_HealthBaseLine);
	}

	public void RestoreEnergyRPC()
	{
		SetEnergy(m_EnergyBaseLine);
	}

	public void RestoreHeatRPC()
	{
		SetHeat(m_HeatBaseLine);
	}

	public void IncreaseHealthRPC(float amount)
	{
		if (!m_NetView.isMine)
		{
			m_NetView.RPC("RPC_ModifyHealth", m_NetView, amount);
		}
		ModifyStat_Internal(amount, ref m_Health, m_HealthBaseLine, OnHealthStatChanged);
	}

	public void DecreaseHealth(float amount)
	{
		if (!m_NetView.isMine)
		{
			m_NetView.RPC("RPC_ModifyHealth", m_NetView, 0f - amount);
		}
		ModifyStat_Internal(0f - amount, ref m_Health, m_HealthBaseLine, OnHealthStatChanged);
	}

	[PunRPC]
	private void RPC_ModifyHealth(float amount, PhotonMessageInfo info)
	{
		ModifyStat_Internal(amount, ref m_Health, m_HealthBaseLine, OnHealthStatChanged);
	}

	public void SetHealth(float amount)
	{
		if (!m_NetView.isMine)
		{
			m_NetView.RPC("RPC_ModifyHealth", m_NetView, amount - (float)m_Health);
		}
		ModifyStat_Internal(amount - (float)m_Health, ref m_Health, 100f, OnHealthStatChanged);
	}

	public void IncreaseEnergyRPC(float amount)
	{
		if (!m_NetView.isMine)
		{
			m_NetView.RPC("RPC_ModifyEnergy", m_NetView, amount);
		}
		ModifyStat_Internal(amount, ref m_Energy, m_EnergyBaseLine, OnEnergyStatChanged);
	}

	public void DecreaseEnergyRPC(float amount)
	{
		if (!m_NetView.isMine)
		{
			m_NetView.RPC("RPC_ModifyEnergy", m_NetView, 0f - amount);
		}
		ModifyStat_Internal(0f - amount, ref m_Energy, m_EnergyBaseLine, OnEnergyStatChanged);
	}

	[PunRPC]
	private void RPC_ModifyEnergy(float amount, PhotonMessageInfo info)
	{
		ModifyStat_Internal(amount, ref m_Energy, m_EnergyBaseLine, OnEnergyStatChanged);
	}

	public void SetEnergy(float amount)
	{
		if (!m_NetView.isMine)
		{
			m_NetView.RPC("RPC_ModifyEnergy", m_NetView, amount - (float)m_Energy);
		}
		ModifyStat_Internal(amount - (float)m_Energy, ref m_Energy, 100f, OnEnergyStatChanged);
	}

	public void IncreaseStrengthRPC(float amount)
	{
		if (!m_NetView.isMine)
		{
			m_NetView.RPC("RPC_IncreaseStrength", m_NetView, amount);
		}
		else if (m_bIsPlayer)
		{
			UpdateStatSystem(STAT_IDS.PlayerStrength, (float)m_Strength + amount);
		}
		if (m_NetView.isMine)
		{
			m_bAnyMinorStatsChangedSinceLastWrite = true;
		}
		ModifyStat_Internal(amount, ref m_Strength, 100f, OnStrengthStatChanged);
	}

	[PunRPC]
	private void RPC_IncreaseStrength(float amount, PhotonMessageInfo info)
	{
		if (m_bIsPlayer)
		{
			UpdateStatSystem(STAT_IDS.PlayerStrength, (float)m_Strength + amount);
		}
		m_bAnyMinorStatsChangedSinceLastWrite = true;
		ModifyStat_Internal(amount, ref m_Strength, 100f, OnStrengthStatChanged);
	}

	public void IncreaseIntellectRPC(float amount)
	{
		if (!m_NetView.isMine)
		{
			m_NetView.RPC("RPC_IncreaseIntellect", m_NetView, amount);
		}
		else if (m_bIsPlayer)
		{
			UpdateStatSystem(STAT_IDS.PlayerIntelligence, (float)m_Intellect + amount);
		}
		if (m_NetView.isMine)
		{
			m_bAnyMinorStatsChangedSinceLastWrite = true;
		}
		ModifyStat_Internal(amount, ref m_Intellect, 100f, OnIntellectStatChanged);
	}

	[PunRPC]
	private void RPC_IncreaseIntellect(float amount, PhotonMessageInfo info)
	{
		if (m_bIsPlayer)
		{
			UpdateStatSystem(STAT_IDS.PlayerIntelligence, (float)m_Intellect + amount);
		}
		m_bAnyMinorStatsChangedSinceLastWrite = true;
		ModifyStat_Internal(amount, ref m_Intellect, 100f, OnIntellectStatChanged);
	}

	public void IncreaseCardioRPC(float amount)
	{
		if (!m_NetView.isMine)
		{
			m_NetView.RPC("RPC_IncreaseCardio", m_NetView, amount);
		}
		else if (m_bIsPlayer)
		{
			UpdateStatSystem(STAT_IDS.PlayerCardio, (float)m_Cardio + amount);
		}
		if (m_NetView.isMine)
		{
			m_bAnyMinorStatsChangedSinceLastWrite = true;
		}
		ModifyStat_Internal(amount, ref m_Cardio, 100f, OnCardioStatChanged);
	}

	[PunRPC]
	private void RPC_IncreaseCardio(float amount, PhotonMessageInfo info)
	{
		if (m_bIsPlayer)
		{
			UpdateStatSystem(STAT_IDS.PlayerCardio, (float)m_Cardio + amount);
		}
		m_bAnyMinorStatsChangedSinceLastWrite = true;
		ModifyStat_Internal(amount, ref m_Cardio, 100f, OnCardioStatChanged);
	}

	public void IncreaseHeat(float amount, MessageGameplayReasons reason = MessageGameplayReasons.Unassigned)
	{
		if (!m_NetView.isMine)
		{
			m_NetView.RPC("RPC_ModifyHeat", m_NetView, amount, reason);
		}
		else if (m_bIsPlayer)
		{
			CheckStatusTutorialHeat((float)m_Heat + amount);
		}
		ModifyStat_Internal(amount, ref m_Heat, 100f, OnHeatStatChanged);
	}

	public void DecreaseHeat(float amount, MessageGameplayReasons reason = MessageGameplayReasons.Unassigned)
	{
		if (!m_NetView.isMine)
		{
			m_NetView.RPC("RPC_ModifyHeat", m_NetView, 0f - amount, reason);
		}
		ModifyStat_Internal(0f - amount, ref m_Heat, 100f, OnHeatStatChanged);
	}

	public void SetHeat(float amount, MessageGameplayReasons reason = MessageGameplayReasons.Unassigned)
	{
		if (!m_NetView.isMine)
		{
			m_NetView.RPC("RPC_ModifyHeat", m_NetView, amount - (float)m_Heat, reason);
		}
		ModifyStat_Internal(amount - (float)m_Heat, ref m_Heat, 100f, OnHeatStatChanged);
	}

	[PunRPC]
	private void RPC_ModifyHeat(float amount, MessageGameplayReasons reason, PhotonMessageInfo info)
	{
		if (!(amount > 0f) || reason != MessageGameplayReasons.DidDamageDuringCombat || !m_Character.GetIsKnockedOut())
		{
			ModifyStat_Internal(amount, ref m_Heat, 100f, OnHeatStatChanged);
			if (m_bIsPlayer)
			{
				CheckStatusTutorialHeat(m_Heat);
			}
		}
	}

	public void IncreaseMoney(float amount)
	{
		if (!m_NetView.isMine)
		{
			m_NetView.RPC("RPC_ModifyMoney", m_NetView, amount);
		}
		if (m_NetView.isMine)
		{
			m_bAnyMinorStatsChangedSinceLastWrite = true;
		}
		ModifyStat_Internal(amount, ref m_Money, 999f, OnMoneyStatChanged);
	}

	public void DecreaseMoney(float amount)
	{
		if (!m_NetView.isMine)
		{
			m_NetView.RPC("RPC_ModifyMoney", m_NetView, 0f - amount);
		}
		else
		{
			m_bAnyMinorStatsChangedSinceLastWrite = true;
		}
		ModifyStat_Internal(0f - amount, ref m_Money, 999f, OnMoneyStatChanged);
	}

	[PunRPC]
	private void RPC_ModifyMoney(float amount, PhotonMessageInfo info)
	{
		m_bAnyMinorStatsChangedSinceLastWrite = true;
		ModifyStat_Internal(amount, ref m_Money, 999f, OnMoneyStatChanged);
	}

	private void ModifyStat_Internal(float change, ref HiddenFloat stat, float max, CharacterStatsEvent ev = null)
	{
		if (!(Mathf.Abs(change) < 0.001f))
		{
			float num = stat;
			stat.SetValue(Mathf.Clamp((float)stat + change, 0f, max));
			if (ev != null && Mathf.Abs((float)stat - num) > 0.001f)
			{
				ev(stat);
			}
		}
	}

	private void UpdateStatSystem(STAT_IDS statID, float value)
	{
		if (m_NetView.ownerId == T17NetManager.PhotonPlayerID)
		{
			Gamer gamer = (m_Character as Player).m_Gamer;
			StatSystem.GetInstance().SetStat((int)statID, value, gamer);
		}
	}

	public void RandomiseStats(float variance)
	{
		m_HealthBaseLine = Mathf.Min(100f, (float)m_HealthBaseLine + Random.Range(0f - variance, variance));
		m_StrengthBaseLine = Mathf.Min(100f, (float)m_StrengthBaseLine + Random.Range(0f - variance, variance));
		m_IntellectBaseLine = Mathf.Min(100f, (float)m_IntellectBaseLine + Random.Range(0f - variance, variance));
	}

	public void ModSpeed(ref float mod)
	{
		if (!m_bIsPlayer)
		{
			float num = 0f;
			if (Health > (float)m_HealthBaseLine)
			{
				num += 0.5f * m_AIMaxSpeedBoost * (Health / 100f);
			}
			if (Energy > (float)m_EnergyBaseLine)
			{
				num += 0.5f * m_AIMaxSpeedBoost * (Energy / 100f);
			}
			mod += m_SpeedMod + num;
		}
	}

	private void DecrementRemainingSentence()
	{
		m_RemainingSentence = (int)m_RemainingSentence - 1;
		m_bAnyMinorStatsChangedSinceLastWrite = true;
	}

	public void IncreaseTimesSentToSolitary()
	{
		m_TimesSentToSolitary = (int)m_TimesSentToSolitary + 1;
		m_bAnyMinorStatsChangedSinceLastWrite = true;
	}

	public void SetCharacterState(StatModifierEnum state)
	{
		m_NetView.PostLevelLoadRPC("RPC_SetCharacterState", NetTargets.All, (int)state);
	}

	[PunRPC]
	private void RPC_SetCharacterState(int statEnum, PhotonMessageInfo info)
	{
		if (statEnum == 0)
		{
		}
		if (m_CharacterState != (StatModifierEnum)statEnum)
		{
			UnSetCharacterState_Internal(m_CharacterState);
		}
		AssignCharacterState((StatModifierEnum)statEnum);
	}

	public void UnSetCharacterState(StatModifierEnum state)
	{
		m_NetView.PostLevelLoadRPC("RPC_UnSetCharacterState", NetTargets.All, (int)state);
	}

	[PunRPC]
	private void RPC_UnSetCharacterState(int statEnum, PhotonMessageInfo info)
	{
		UnSetCharacterState_Internal((StatModifierEnum)statEnum);
	}

	private void UnSetCharacterState_Internal(StatModifierEnum state)
	{
		if (m_CharacterState == state)
		{
			AssignCharacterState(StatModifierEnum.None);
		}
	}

	protected virtual void AssignCharacterState(StatModifierEnum newState, bool suppressChangedEvent = false)
	{
		StatModifierEnum characterState = m_CharacterState;
		m_CharacterState = newState;
		if (this.StateChangedEvent != null && !suppressChangedEvent)
		{
			this.StateChangedEvent(characterState, newState);
		}
	}

	public StatModifierEnum GetCharacterState()
	{
		return m_CharacterState;
	}

	public bool Serialize(ref ulong stats, ref ulong stats2)
	{
		BitField bitField = new BitField();
		bitField.Reset();
		bitField.Set(7, (uint)(float)m_Health);
		bitField.Set(7, (uint)(float)m_Energy);
		bitField.Set(7, (uint)(float)m_Strength);
		bitField.Set(7, (uint)(float)m_Cardio);
		bitField.Set(7, (uint)(float)m_Intellect);
		bitField.Set(7, (uint)(float)m_Heat);
		bitField.Set(10, (uint)(float)m_Money);
		stats = (ulong)bitField;
		bitField.Reset();
		bitField.Set(10, (uint)m_RemainingSentence.GetValue());
		bitField.Set(10, (uint)m_TimesSentToSolitary.GetValue());
		stats2 = (ulong)bitField;
		return true;
	}

	public void Deserialize(ulong stats, ulong stats2)
	{
		BitField bitField = new BitField();
		bitField.Init(stats, 0);
		m_Health = bitField.GetUInt(7);
		m_Energy = bitField.GetUInt(7);
		m_Strength = bitField.GetUInt(7);
		m_Cardio = bitField.GetUInt(7);
		m_Intellect = bitField.GetUInt(7);
		m_Heat = bitField.GetUInt(7);
		m_Money = bitField.GetUInt(10);
		bitField.Init(stats2, 0);
		m_RemainingSentence = (int)bitField.GetUInt(10);
		m_TimesSentToSolitary = (int)bitField.GetUInt(10);
		if ((float)m_Strength < (float)m_StrengthBaseLine)
		{
			m_Strength = m_StrengthBaseLine;
		}
		if ((float)m_Cardio < (float)m_CardioBaseLine)
		{
			m_Cardio = m_CardioBaseLine;
		}
		if ((float)m_Intellect < (float)m_IntellectBaseLine)
		{
			m_Intellect = m_IntellectBaseLine;
		}
	}

	public void SerializeToView(BitStreamWriter bitWriter)
	{
		float health = Health;
		if (WriteThing(ref m_LastSyncedHealth, health, bitWriter))
		{
			bitWriter.Write((uint)health, 7);
		}
		float energy = Energy;
		if (WriteThing(ref m_LastSyncedEnergy, energy, bitWriter))
		{
			bitWriter.Write((uint)energy, 7);
		}
		float heat = Heat;
		if (WriteThing(ref m_LastSyncedHeat, heat, bitWriter))
		{
			bitWriter.Write((uint)heat, 7);
		}
		bitWriter.Write(m_bAnyMinorStatsChangedSinceLastWrite);
		if (m_bAnyMinorStatsChangedSinceLastWrite)
		{
			bitWriter.Write((uint)Strength, 7);
			bitWriter.Write((uint)Intellect, 7);
			bitWriter.Write((uint)Cardio, 7);
			bitWriter.Write((uint)Money, 10);
			bitWriter.Write((uint)RemainingSentence, 10);
			bitWriter.Write((uint)TimesSentToSolitary, 10);
			m_bAnyMinorStatsChangedSinceLastWrite = false;
		}
	}

	public void DeserializeFromView(BitStreamReader bitReader)
	{
		if (bitReader.ReadBit())
		{
			m_Health = (int)bitReader.ReadUInt16(7);
		}
		if (bitReader.ReadBit())
		{
			m_Energy = (int)bitReader.ReadUInt16(7);
		}
		if (bitReader.ReadBit())
		{
			m_Heat = (int)bitReader.ReadUInt16(7);
		}
		if (bitReader.ReadBit())
		{
			m_Strength = (int)bitReader.ReadUInt16(7);
			m_Intellect = (int)bitReader.ReadUInt16(7);
			m_Cardio = (int)bitReader.ReadUInt16(7);
			m_Money = (int)bitReader.ReadUInt16(10);
			m_RemainingSentence = bitReader.ReadUInt16(10);
			m_TimesSentToSolitary = bitReader.ReadUInt16(10);
		}
	}

	public void CallStatsTutorial()
	{
		Player player = m_Character as Player;
		if (player != null)
		{
			TutorialManager.GetInstance().StartTutorialRPC(player, TutorialSubject.Stats);
		}
	}

	public void CheckStatusTutorial(float stat)
	{
		if (stat <= 10f)
		{
			Player player = m_Character as Player;
			if (player != null)
			{
				TutorialManager.GetInstance().StartTutorialRPC(player, TutorialSubject.Status);
			}
		}
	}

	public void CheckStatusTutorialHeat(float heat)
	{
		if (heat > 50f)
		{
			Player player = m_Character as Player;
			if (player != null)
			{
				TutorialManager.GetInstance().StartTutorialRPC(player, TutorialSubject.Status);
			}
		}
	}

	public void ResetToBaseline()
	{
		m_Health = m_HealthBaseLine;
		m_Energy = m_EnergyBaseLine;
		m_Strength = m_StrengthBaseLine;
		m_Intellect = m_IntellectBaseLine;
		m_Cardio = m_CardioBaseLine;
		m_Heat = m_HeatBaseLine;
		m_Money = m_MoneyBaseLine;
		m_RemainingSentence = m_SentenceBaseLine;
		m_TimesSentToSolitary = 0;
		m_bAnyMinorStatsChangedSinceLastWrite = true;
	}

	public void LoadStatsFromPreviousGame()
	{
		string outValue = string.Empty;
		T17NetRoomGameView.GetCustomPropertyAsString(T17NetRoomGameView.CustomProperty.HostKey, ref outValue);
		MatchingGames.Game game = MatchingGames.GetInstance().FindGame(outValue);
		if (game != null && (game.m_Strength != 0f || game.m_Intellect != 0f || game.m_Cardio != 0f))
		{
			ResetToBaseline();
			SetHealth(game.m_Health);
			SetEnergy(game.m_Energy);
			IncreaseStrengthRPC(game.m_Strength - (float)m_Strength);
			IncreaseIntellectRPC(game.m_Intellect - (float)m_Intellect);
			IncreaseCardioRPC(game.m_Cardio - (float)m_Cardio);
			SetHeat(game.m_Heat);
			IncreaseMoney(game.m_Money - (float)m_Money);
		}
		else
		{
			ResetToBaseline();
		}
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

	[Conditional("LOGHEAT")]
	public void LogHeatChange(string strfrom, float fCurrent, float fAdjust, bool bSet = false, string strSubject = "HEAT")
	{
		if (m_Character != null && m_bIsPlayer && ((!bSet && fAdjust != 0f) || (bSet && fCurrent != fAdjust)))
		{
			string text = "[" + strfrom + "][" + base.name + "]";
			text = ((!m_Character.m_NetView.isMine) ? (text + "[Remote] ") : (text + "[Local] "));
			if (bSet)
			{
				text = text + "= " + fAdjust;
			}
			else
			{
				text = text + fCurrent + " -> " + (fCurrent + fAdjust);
			}
		}
	}

	public bool WriteThing<T>(ref T first, T second, BitStreamWriter bitWriter)
	{
		bool flag = !first.Equals(second);
		bitWriter.Write(flag);
		first = second;
		return flag;
	}
}
