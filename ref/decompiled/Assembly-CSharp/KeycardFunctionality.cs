using UnityEngine;

[CreateAssetMenu(fileName = "Keycard Functionality", menuName = "Team17/Items/Functionalities/Create Keycard Functionality")]
public class KeycardFunctionality : BaseItemFunctionality
{
	public enum KeycardColour
	{
		None = -1,
		Cyan,
		Red,
		Green,
		Yellow,
		Purple
	}

	public KeycardColour m_KeycardColour = KeycardColour.None;

	public override Functionality GetFunctionalityType()
	{
		return Functionality.Keycard;
	}

	public override bool CanUse(bool intendsOnUsingImmediately = false)
	{
		return false;
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

	public override void Init()
	{
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
		return base.CancelUsing();
	}
}
