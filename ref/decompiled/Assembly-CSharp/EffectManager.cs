using System;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : T17MonoBehaviour
{
	[Serializable]
	public struct effectData
	{
		public effectType type;

		public int total;

		public GameObject prefab;

		[HideInInspector]
		public string prefabName;

		[HideInInspector]
		public List<GameObject> bucket;
	}

	public enum effectType : byte
	{
		AnimatedPunchEffect = 0,
		StrengthIncrease = 1,
		IntelligenceIncrease = 2,
		CardioIncrease = 3,
		StaminaIncrease = 4,
		StaminaDecrease = 5,
		LandedJump = 6,
		OpinionIncrease = 7,
		OpinionDecrease = 8,
		HealthRestored = 9,
		EnergyRestored = 10,
		FistCharge = 11,
		FeetChargeDash = 12,
		PlayerLeaveDust = 13,
		MoneyIncreased = 14,
		HeatIncreased = 15,
		ChargeAttackImpact = 16,
		DiggingUp = 17,
		DiggingDown = 18,
		Unassigned = byte.MaxValue
	}

	public List<effectData> m_effects;

	private static EffectManager m_Instance;

	private T17NetView m_NetView;

	public static EffectManager GetInstance()
	{
		return m_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		m_Instance = this;
		if (m_NetView == null)
		{
			m_NetView = GetComponent<T17NetView>();
		}
	}

	protected virtual void OnDestroy()
	{
		int count = m_effects.Count;
		for (int i = 0; i < count; i++)
		{
			int count2 = m_effects[i].bucket.Count;
			for (int j = 0; j < count2; j++)
			{
				UnityEngine.Object.Destroy(m_effects[i].bucket[j]);
				if (LevelScript.GetInstance() != null && LevelScript.GetInstance().m_PreBuildSwapPrefabRefs)
				{
					effectData effectData = m_effects[i];
					effectData.prefab = null;
				}
			}
		}
		m_NetView = null;
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	private void Start()
	{
		for (int i = 0; i < m_effects.Count; i++)
		{
			if (m_effects[i].prefab == null)
			{
				continue;
			}
			for (int j = 0; j < m_effects[i].total; j++)
			{
				if (LevelScript.GetInstance().m_PreBuildSwapPrefabRefs)
				{
					effectData effectData = m_effects[i];
					effectData.prefab = Resources.Load(m_effects[i].prefabName) as GameObject;
				}
				GameObject gameObject = UnityEngine.Object.Instantiate(m_effects[i].prefab);
				gameObject.transform.parent = base.gameObject.transform;
				gameObject.SetActive(value: false);
				m_effects[i].bucket.Add(gameObject);
			}
		}
		if (m_NetView != null)
		{
			int viewID = m_NetView.viewID;
			m_NetView.viewID = T17NetConfig.GetReservedNetID(T17NetConfig.ReservedNetID.LevelManager);
		}
	}

	public static void PlayEffect(effectType eType, Vector3 position, PhotonView parent = null, float characterID = -1f)
	{
		if (m_Instance != null)
		{
			int parentViewId = ((!(parent == null)) ? parent.viewID : (-1));
			m_Instance.NewEffectInstanceRPC(eType, position, parentViewId, characterID);
		}
	}

	public GameObject PlayEffect_LocalOnly(effectType eType, Vector3 position, Transform parentTransform = null, Character character = null)
	{
		return SpawnEffectInstance(eType, position, parentTransform, character);
	}

	public void ReturnEffect(GameObject effectObject)
	{
		EffectHandler component = effectObject.GetComponent<EffectHandler>();
		if (component != null)
		{
			component.ReturnEffect(shouldChildToManager: true);
		}
	}

	public void NewEffectInstanceRPC(effectType eType, Vector3 position, int parentViewId = -1, float characterID = -1f)
	{
		if (m_NetView != null)
		{
			byte b = (byte)eType;
			if (parentViewId == -1 && characterID == -1f)
			{
				m_NetView.GameplayRPC("RPC_NewEffectInstance", NetTargets.All, b, position.x, position.y, position.z);
			}
			else
			{
				m_NetView.GameplayRPC("RPC_NewEffectInstance", NetTargets.All, b, position.x, position.y, position.z, parentViewId, characterID);
			}
			if (!DebugHelpers.LogGroupActive(DebugHelpers.LogGroup.PlayerEffects))
			{
			}
		}
	}

	[PunRPC]
	public void RPC_NewEffectInstance(byte eTypeAsByte, float x, float y, float z, PhotonMessageInfo info)
	{
		RPC_NewEffectInstance(eTypeAsByte, x, y, z, -1, -1f, info);
	}

	[PunRPC]
	public void RPC_NewEffectInstance(byte eTypeAsByte, float x, float y, float z, int parentViewId, float characterID, PhotonMessageInfo info)
	{
		Vector3 position = default(Vector3);
		position.x = x;
		position.y = y;
		position.z = z;
		effectType effectType = (effectType)eTypeAsByte;
		Transform parent = null;
		if (parentViewId != -1)
		{
			PhotonView photonView = PhotonView.Find(parentViewId);
			if (photonView != null)
			{
				parent = photonView.transform;
			}
		}
		Character character = null;
		if (characterID != -1f)
		{
			character = Character.GetAllCharacters().Find((Character c) => c.GetCharacterID() == characterID);
		}
		GameObject newEffect = SpawnEffectInstance(effectType, position, parent, character);
		info.photonView.GetComponent<IEffectManagerListener>()?.OnEffectSpawned(newEffect, effectType);
		if (!DebugHelpers.LogGroupActive(DebugHelpers.LogGroup.PlayerEffects))
		{
		}
	}

	private GameObject SpawnEffectInstance(effectType eType, Vector3 position, Transform parent = null, Character character = null)
	{
		GameObject gameObject = FindInactive(eType, isFBX: false);
		if (gameObject != null)
		{
			gameObject.SetActive(value: true);
			gameObject.transform.position = position;
			if (parent != null)
			{
				gameObject.transform.SetParent(parent, worldPositionStays: true);
			}
			else
			{
				gameObject.transform.SetParent(base.transform, worldPositionStays: true);
			}
			CullingObjectCollector.GetInstance().AddRuntimeEffect(gameObject, character);
			return gameObject;
		}
		return null;
	}

	private GameObject FindInactive(effectType eType, bool isFBX)
	{
		for (int i = 0; i < m_effects.Count; i++)
		{
			if (m_effects[i].type != eType)
			{
				continue;
			}
			List<GameObject> bucket = m_effects[i].bucket;
			for (int j = 0; j < bucket.Count; j++)
			{
				if (!bucket[j].GetActive())
				{
					return bucket[j];
				}
			}
		}
		return null;
	}
}
