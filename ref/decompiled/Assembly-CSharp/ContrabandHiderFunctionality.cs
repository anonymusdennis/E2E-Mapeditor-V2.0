using UnityEngine;

[CreateAssetMenu(fileName = "Contraband Hider Functionality", menuName = "Team17/Items/Functionalities/Create Contraband Hider Functionality")]
public class ContrabandHiderFunctionality : BaseItemFunctionality
{
	[Tooltip("The amount of item health that is deducted upon use")]
	public int m_ItemDecayPerUse = 100;

	[Tooltip("The effect that plays when using this item")]
	public EffectManager.effectType m_UseEffect = EffectManager.effectType.Unassigned;

	public override void Init()
	{
	}

	public override Functionality GetFunctionalityType()
	{
		return Functionality.HideContraband;
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
		return false;
	}

	public override bool IsDegradedByDetector()
	{
		return true;
	}

	public override bool StartUsing(AnimState useAnimation, float useTime)
	{
		base.StartUsing(useAnimation, useTime);
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

	public bool IsOnLastUse()
	{
		return base.ParentItem.Health <= m_ItemDecayPerUse && base.ParentItem.Health > 0;
	}
}
