using System;
using Slate;
using Slate.ActionClips;
using UnityEngine;

namespace T17.Slate;

[Attachable(new Type[] { typeof(ActorActionTrack) })]
[Category("T17 Characters")]
public class CharacterSetAppearance : ActorActionClip
{
	public Customisation m_Appearance = new Customisation();

	private Customisation m_OriginalAppearance;

	private Customisation m_OriginalOverrides;

	private CS_HijackIngameCharacter m_HijackedCharacter;

	[SerializeField]
	[HideInInspector]
	private float _length = 1f;

	public override float length
	{
		get
		{
			return _length;
		}
		set
		{
			_length = value;
		}
	}

	public override string info => "Set Appearance";

	protected override void OnEnter()
	{
		base.OnEnter();
		if (m_HijackedCharacter == null)
		{
			m_HijackedCharacter = base.actor.GetAddComponent<CS_HijackIngameCharacter>();
		}
		if (!(m_HijackedCharacter != null))
		{
			return;
		}
		Customisation customisation = new Customisation(m_Appearance);
		CharacterCustomisation component = m_HijackedCharacter.GetComponent<CharacterCustomisation>();
		if (component != null)
		{
			m_OriginalAppearance = new Customisation();
			m_OriginalAppearance.body = component.m_BodyType;
			m_OriginalAppearance.skin = component.m_SkinColour;
			m_OriginalAppearance.hair = component.m_Hair;
			m_OriginalAppearance.hat = component.m_Hat;
			m_OriginalAppearance.upperFace = component.m_UpperFaceAccessory;
			m_OriginalAppearance.lowerFace = component.m_LowerFaceAccessory;
			m_OriginalAppearance.defaultOutfit = component.m_DefaultOutfit;
			m_OriginalOverrides = new Customisation();
			m_OriginalOverrides.hair = component.m_HairOverride;
			m_OriginalOverrides.hat = component.m_HatOverride;
			m_OriginalOverrides.upperFace = component.m_UpperFaceOverride;
			m_OriginalOverrides.lowerFace = component.m_LowerFaceOverride;
			m_OriginalOverrides.defaultOutfit = component.m_Outfit;
			if (customisation.hair != CustomisationData.Hair.NULL)
			{
				component.m_HairOverride = CustomisationData.Hair.NULL;
			}
			if (customisation.hat != CustomisationData.Hat.NULL)
			{
				component.m_HatOverride = CustomisationData.Hat.NULL;
			}
			if (customisation.upperFace != CustomisationData.UpperFaceAccessory.NULL)
			{
				component.m_UpperFaceOverride = CustomisationData.UpperFaceAccessory.NULL;
			}
			if (customisation.lowerFace != CustomisationData.LowerFaceAccessory.NULL)
			{
				component.m_LowerFaceOverride = CustomisationData.LowerFaceAccessory.NULL;
			}
			customisation.body = ((customisation.body != CustomisationData.BodyType.NULL) ? customisation.body : m_OriginalAppearance.body);
			customisation.skin = ((customisation.skin != CustomisationData.SkinColour.NULL) ? customisation.skin : m_OriginalAppearance.skin);
			customisation.hair = ((customisation.hair != CustomisationData.Hair.NULL) ? customisation.hair : m_OriginalAppearance.hair);
			customisation.hat = ((customisation.hat != CustomisationData.Hat.NULL) ? customisation.hat : m_OriginalAppearance.hat);
			customisation.upperFace = ((customisation.upperFace != CustomisationData.UpperFaceAccessory.NULL) ? customisation.upperFace : m_OriginalAppearance.upperFace);
			customisation.lowerFace = ((customisation.lowerFace != CustomisationData.LowerFaceAccessory.NULL) ? customisation.lowerFace : m_OriginalAppearance.lowerFace);
			customisation.defaultOutfit = ((customisation.defaultOutfit != CustomisationData.Outfit.NULL) ? customisation.defaultOutfit : m_OriginalOverrides.defaultOutfit);
		}
		m_HijackedCharacter.SetCharacterAppearance(customisation);
	}

	protected override void OnExit()
	{
		base.OnEnter();
		if (m_HijackedCharacter != null && m_OriginalAppearance != null)
		{
			CharacterCustomisation component = m_HijackedCharacter.GetComponent<CharacterCustomisation>();
			if (component != null && m_OriginalOverrides != null)
			{
				component.m_HairOverride = m_OriginalOverrides.hair;
				component.m_HatOverride = m_OriginalOverrides.hat;
				component.m_UpperFaceOverride = m_OriginalOverrides.upperFace;
				component.m_LowerFaceOverride = m_OriginalOverrides.lowerFace;
			}
			m_HijackedCharacter.SetCharacterAppearance(m_OriginalAppearance, m_OriginalOverrides);
		}
	}
}
