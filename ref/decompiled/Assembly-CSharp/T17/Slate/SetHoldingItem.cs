using System;
using Slate;
using UnityEngine;

namespace T17.Slate;

[Attachable(new Type[] { typeof(ActorActionTrack) })]
[Category("T17 Characters")]
public class SetHoldingItem : ActionClip
{
	public ItemData m_ItemData;

	public override string info => "Holding " + ((!(m_ItemData == null)) ? m_ItemData.m_ItemLocalizationTag : "nothing");

	protected override void OnEnter()
	{
		base.OnEnter();
		CharacterAnimator component = base.actor.GetComponent<CharacterAnimator>();
		if (component != null)
		{
			if (m_ItemData == null)
			{
				component.SetMaterialHandHeld(null);
			}
			else
			{
				component.SetMaterialHandHeld(m_ItemData.m_ItemHeldMaterial, m_ItemData.m_ItemHeldType);
			}
		}
		else
		{
			Debug.LogError("Design: only use this on an actor that has a character animator component");
		}
	}
}
