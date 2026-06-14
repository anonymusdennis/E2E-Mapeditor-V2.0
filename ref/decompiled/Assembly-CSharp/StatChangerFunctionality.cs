using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stat Changer Functionality", menuName = "Team17/Items/Functionalities/Create Stat Changer Functionality")]
public class StatChangerFunctionality : BaseItemFunctionality
{
	[Serializable]
	public class StatChange
	{
		[Tooltip("The character attribute we want to change")]
		public CharacterAttributes m_TargetAttribute;

		[Tooltip("The amount it will modify the target stat")]
		public float m_StatChangeModifier;
	}

	public enum CharacterAttributes
	{
		Unassigned,
		CurrentHealth,
		CurrentEnergy
	}

	public string m_PlayUseSoundHealth;

	public string m_PlayUseSoundEnergy;

	[Tooltip("The amount of item health that is deducted upon use")]
	public int m_ItemDecayPerUse = 100;

	[Tooltip("The effect that plays when using this item")]
	public EffectManager.effectType m_UseEffect = EffectManager.effectType.Unassigned;

	[Tooltip("The stats this item applies")]
	public List<StatChange> m_StatsToApply;

	public override void Init()
	{
	}

	public override Functionality GetFunctionalityType()
	{
		return Functionality.StatChange;
	}

	public override bool RequiresTargetting()
	{
		return false;
	}

	public override bool RequiresPositioning()
	{
		return false;
	}

	public override bool ImmobilisesOwner()
	{
		return false;
	}

	public override bool IsImmediateUse()
	{
		return true;
	}

	public override bool CanUse(bool intendsOnUsingImmediately = false)
	{
		return true;
	}

	private void ApplyStatChange(StatChange change)
	{
		switch (change.m_TargetAttribute)
		{
		case CharacterAttributes.CurrentEnergy:
			m_Owner.m_CharacterStats.IncreaseEnergyRPC(change.m_StatChangeModifier);
			if (!string.IsNullOrEmpty(m_PlayUseSoundEnergy))
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_PlayUseSoundEnergy, m_Owner.gameObject);
			}
			break;
		case CharacterAttributes.CurrentHealth:
			m_Owner.m_CharacterStats.IncreaseHealthRPC(change.m_StatChangeModifier);
			if (!string.IsNullOrEmpty(m_PlayUseSoundHealth))
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_PlayUseSoundHealth, m_Owner.gameObject);
			}
			break;
		}
	}

	public override bool StartUsing(AnimState useAnimation, float useTime)
	{
		base.StartUsing(useAnimation, useTime);
		int count = m_StatsToApply.Count;
		for (int i = 0; i < count; i++)
		{
			ApplyStatChange(m_StatsToApply[i]);
		}
		if (count == 0)
		{
		}
		if (m_UseEffect != EffectManager.effectType.Unassigned)
		{
			EffectManager.PlayEffect(m_UseEffect, m_Owner.GetStatChangeEffectPosition(), m_Owner.photonView);
		}
		if (base.ParentItem != null)
		{
			base.ParentItem.DecreaseHealth(m_ItemDecayPerUse);
		}
		if (OnStartOfUse != null)
		{
			OnStartOfUse();
		}
		return false;
	}

	public override bool UpdateUsing()
	{
		return false;
	}

	public override bool CancelUsing()
	{
		base.CancelUsing();
		return true;
	}
}
