using System;
using System.Collections.Generic;
using UnityEngine;

public class OpinionManager : T17MonoBehaviour, IControlledUpdate, IDeserializable
{
	public enum OpinionRange
	{
		Highest,
		High,
		Medium,
		Low,
		Lowest,
		RANGE_MAX
	}

	[Serializable]
	public class ThresholdInfo
	{
		[Range(0f, 100f)]
		public int High;

		[Range(0f, 100f)]
		public int Low;
	}

	[Serializable]
	public class DefaultOpinionInfo
	{
		public OpinionRange Inmates = OpinionRange.Medium;

		public OpinionRange Guards = OpinionRange.Medium;
	}

	[Serializable]
	public class SightCheckInfo
	{
		[Serializable]
		public class RandomTimer
		{
			public float Interval;

			public float Variance;
		}

		public RandomTimer InmateAttack = new RandomTimer();

		public RandomTimer GuardFollow = new RandomTimer();
	}

	[Serializable]
	public class AttackOpinionInfos
	{
		[Serializable]
		public class AttackOpinionInfo
		{
			[Range(0f, 100f)]
			public int OpinionLoss;

			public int Cooldown;

			[ReadOnly]
			public float CooldownRealtime;
		}

		public AttackOpinionInfo Inmates = new AttackOpinionInfo();

		public AttackOpinionInfo Guards = new AttackOpinionInfo();
	}

	[Serializable]
	public class NetSaveData
	{
		[Serializable]
		public class OpinionData
		{
			public int CharacterID = -1;

			public List<ulong> Opinions;
		}

		public List<OpinionData> m_SerializedData = new List<OpinionData>();
	}

	public const int MaxOpinion = 100;

	public const int MinOpinion = 0;

	private const int OpinionValueRange = 20;

	public const int DefaultNPCOpinion = 50;

	[ReadOnly]
	public int m_DefaultNPCOpinion = 50;

	[Header("Like / Hate Thresholds")]
	public ThresholdInfo m_OpinionThresholds = new ThresholdInfo();

	[Header("Starting Opinions")]
	public DefaultOpinionInfo m_InitialOpinionOfPlayers = new DefaultOpinionInfo();

	[Header("Other Settings")]
	[Range(0f, 2f)]
	public float m_ItemGiftValueModifier = 1f;

	public SightCheckInfo m_LowOpinionSightChecks = new SightCheckInfo();

	public AttackOpinionInfos m_CharacterAttackOpinionLosses = new AttackOpinionInfos();

	private List<Character> m_OpinionCharacters = new List<Character>();

	private NetSaveData m_NetSaveData = new NetSaveData();

	private bool m_IsSerializing;

	private bool m_ShouldReserialize;

	private T17NetView m_Netview;

	private static OpinionManager m_Instance;

	public static OpinionManager GetInstance()
	{
		return m_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		m_Instance = this;
		m_Netview = GetComponent<T17NetView>();
	}

	public void Initialise()
	{
		if (UpdateManager.GetInstance() != null)
		{
			UpdateManager.GetInstance().Register(this, UpdateCategory.SlowPeriodic);
		}
		if (ConfigManager.GetInstance() != null && (bool)ConfigManager.GetInstance().opinionConfig)
		{
			ApplyConfigData(ConfigManager.GetInstance().opinionConfig);
		}
		if (T17NetManager.IsMasterClient)
		{
			List<Player> allPlayers = Player.GetAllPlayers();
			for (int num = allPlayers.Count - 1; num >= 0; num--)
			{
				OnPlayerSpawned(allPlayers[num]);
			}
		}
	}

	protected virtual void OnDestroy()
	{
		if (UpdateManager.GetInstance() != null)
		{
			UpdateManager.GetInstance().Unregister(this, UpdateCategory.SlowPeriodic);
		}
		if (m_Instance == this)
		{
			m_Instance = null;
		}
		m_Netview = null;
	}

	private void ApplyConfigData(OpinionConfig config)
	{
		m_OpinionThresholds.High = config.m_HighOpinionThreshold;
		m_OpinionThresholds.Low = config.m_LowOpinionThreshold;
		m_InitialOpinionOfPlayers = config.m_InitialOpinionOfPlayers;
		m_ItemGiftValueModifier = config.m_ItemGiftValueModifier;
		m_LowOpinionSightChecks = config.m_LowOpinionSightChecks;
		m_CharacterAttackOpinionLosses = config.m_CharacterAttackOpinionLosses;
	}

	public void ControlledUpdate()
	{
		if (m_ShouldReserialize)
		{
			if (T17NetManager.OfflineMode || (T17NetManager.NetOnlineMode && T17NetManager.IsMasterClient))
			{
				UpdateNetPrisonViewData();
			}
			m_ShouldReserialize = false;
		}
	}

	public void ControlledFixedUpdate()
	{
	}

	public void RegisterOpinionCharacter(Character character)
	{
		m_OpinionCharacters.Add(character);
	}

	public void UnregisterOpinionCharacter(Character character)
	{
		m_OpinionCharacters.Remove(character);
	}

	public int GetRandomOpinionInRange(OpinionRange opinionRange)
	{
		int min = 100 - 20 * (int)(opinionRange + 1);
		int max = 100 - 20 * (int)opinionRange;
		return UnityEngine.Random.Range(min, max);
	}

	public int GetHighOpinionThreshold()
	{
		return m_OpinionThresholds.High;
	}

	public int GetLowOpinionThreshold()
	{
		return m_OpinionThresholds.Low;
	}

	public float GetItemGiftValueModifier()
	{
		return m_ItemGiftValueModifier;
	}

	public int GetAttackOpinionLoss(CharacterRole role)
	{
		int result = 0;
		switch (role)
		{
		case CharacterRole.Inmate:
			result = m_CharacterAttackOpinionLosses.Inmates.OpinionLoss;
			break;
		case CharacterRole.Guard:
			result = m_CharacterAttackOpinionLosses.Guards.OpinionLoss;
			break;
		}
		return result;
	}

	public int GetAttackOpinionLossInterval(CharacterRole role)
	{
		int result = 0;
		switch (role)
		{
		case CharacterRole.Inmate:
			result = m_CharacterAttackOpinionLosses.Inmates.Cooldown;
			break;
		case CharacterRole.Guard:
			result = m_CharacterAttackOpinionLosses.Guards.Cooldown;
			break;
		}
		return result;
	}

	public float GetInmateLowOpinionAttackInterval()
	{
		float interval = m_LowOpinionSightChecks.InmateAttack.Interval;
		float variance = m_LowOpinionSightChecks.InmateAttack.Variance;
		return Mathf.Max(interval + UnityEngine.Random.Range(0f - variance, variance), 0f);
	}

	public float GetGuardLowOpinionFollowInterval()
	{
		float interval = m_LowOpinionSightChecks.GuardFollow.Interval;
		float variance = m_LowOpinionSightChecks.GuardFollow.Variance;
		return Mathf.Max(interval + UnityEngine.Random.Range(0f - variance, variance), 0f);
	}

	private void AssignRandomOpinionToAll(Character target)
	{
		for (int i = 0; i < m_OpinionCharacters.Count; i++)
		{
			Character character = m_OpinionCharacters[i];
			if (character != null)
			{
				OpinionRange opinionRange = OpinionRange.Medium;
				switch (character.m_CharacterRole)
				{
				case CharacterRole.Inmate:
					opinionRange = m_InitialOpinionOfPlayers.Inmates;
					break;
				case CharacterRole.Guard:
					opinionRange = m_InitialOpinionOfPlayers.Guards;
					break;
				}
				int randomOpinionInRange = GetRandomOpinionInRange(opinionRange);
				character.m_CharacterOpinions.SetOpinionOf(target, randomOpinionInRange);
			}
		}
	}

	public void OnPlayerSpawned(Player player)
	{
		if (T17NetManager.OfflineMode || (T17NetManager.NetOnlineMode && T17NetManager.IsMasterClient))
		{
			AssignRandomOpinionToAll(player);
		}
	}

	public void ResetOpinionsOfPlayerRPC(Player player)
	{
		if (m_Netview == null)
		{
			m_Netview = GetComponent<T17NetView>();
		}
		if (m_Netview != null)
		{
			m_Netview.RPC("RPC_ResetOpinions", NetTargets.MasterClient, player.m_NetView.viewID);
		}
	}

	[PunRPC]
	private void RPC_ResetOpinions(int playerNetviewID)
	{
		if (T17NetManager.OfflineMode || (T17NetManager.NetOnlineMode && T17NetManager.IsMasterClient))
		{
			Player player = T17NetView.Find<Player>(playerNetviewID);
			if (player != null)
			{
				AssignRandomOpinionToAll(player);
			}
		}
	}

	public void OnOpinionUpdated()
	{
		if (!m_IsSerializing)
		{
			m_ShouldReserialize = true;
		}
	}

	private void UpdateNetPrisonViewData()
	{
		string opinionData = Serialize();
		if (NetPrisonViewDetails.Instance != null)
		{
			NetPrisonViewDetails.Instance.OpinionData = opinionData;
		}
	}

	private string Serialize()
	{
		m_IsSerializing = true;
		m_NetSaveData.m_SerializedData.Clear();
		for (int i = 0; i < m_OpinionCharacters.Count; i++)
		{
			if (!(m_OpinionCharacters[i] == null))
			{
				List<ulong> list = m_OpinionCharacters[i].m_CharacterOpinions.Serialize();
				if (list.Count > 0)
				{
					NetSaveData.OpinionData opinionData = new NetSaveData.OpinionData();
					opinionData.CharacterID = m_OpinionCharacters[i].m_NetView.viewID;
					opinionData.Opinions = list;
					m_NetSaveData.m_SerializedData.Add(opinionData);
				}
			}
		}
		m_IsSerializing = false;
		return JsonUtility.ToJson(m_NetSaveData);
	}

	public string GetSerializationData()
	{
		return NetPrisonViewDetails.Instance.OpinionData;
	}

	public bool Deserialize(string data, ref string error)
	{
		if (string.IsNullOrEmpty(data))
		{
			return true;
		}
		m_IsSerializing = true;
		bool result = true;
		NetSaveData netSaveData = JsonUtility.FromJson<NetSaveData>(data);
		if (netSaveData != null && netSaveData.m_SerializedData != null)
		{
			for (int i = 0; i < netSaveData.m_SerializedData.Count; i++)
			{
				NetSaveData.OpinionData opinionData = netSaveData.m_SerializedData[i];
				if (opinionData == null)
				{
					string text = $"Unable to deserialize opinions {i}";
					error = error + text + "\n";
					result = false;
				}
				else if (opinionData.CharacterID >= 0)
				{
					Character character = T17NetView.Find<Character>(opinionData.CharacterID);
					if (character != null && character.m_CharacterOpinions != null && !character.m_CharacterOpinions.Deserialize(opinionData.Opinions, ref error))
					{
						result = false;
					}
				}
			}
		}
		m_IsSerializing = false;
		return result;
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
