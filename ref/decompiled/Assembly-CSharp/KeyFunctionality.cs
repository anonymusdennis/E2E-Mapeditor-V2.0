using UnityEngine;

[CreateAssetMenu(fileName = "Key Functionality", menuName = "Team17/Items/Functionalities/Create Key Functionality")]
public class KeyFunctionality : BaseItemFunctionality
{
	public enum KeyColour
	{
		None = -999,
		Black = 0,
		Cyan = 100,
		Red = 200,
		Green = 300,
		Yellow = 400,
		Purple = 500,
		Silver = 600,
		Solitary = 700,
		Ghost = 800
	}

	public KeyColour m_KeyColour;

	public int m_ItemDecayPerUse;

	private int m_SubCode;

	private bool m_bIsHidden;

	public bool IsDurable => m_ItemDecayPerUse <= 0;

	public int SubCode => m_SubCode;

	public bool IsHidden => m_bIsHidden;

	public override void Init()
	{
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
		return true;
	}

	public override bool IsImmediateUse()
	{
		return true;
	}

	public override bool CanUse(bool intendsOnUsingImmediately = false)
	{
		return false;
	}

	public override bool StartUsing(AnimState useAnimation, float useTime)
	{
		base.StartUsing(useAnimation, useTime);
		return false;
	}

	public override bool UpdateUsing()
	{
		return false;
	}

	public override bool CancelUsing()
	{
		if (!base.CancelUsing())
		{
			return false;
		}
		return true;
	}

	public void OnPostUse()
	{
		if (base.ParentItem != null && !IsDurable)
		{
			base.ParentItem.DecreaseHealth(m_ItemDecayPerUse);
		}
	}

	public void SetKeySubCode(int subcode)
	{
		m_SubCode = Mathf.Clamp(subcode, 0, 63);
	}

	public void SetKeyHidden(bool isHidden)
	{
		m_bIsHidden = isHidden;
	}

	public override Functionality GetFunctionalityType()
	{
		return Functionality.Key;
	}
}
