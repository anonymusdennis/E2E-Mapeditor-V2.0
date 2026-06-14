using System;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerPuzzle_Interaction_Button : MultiplayerPuzzle_Interaction
{
	[Serializable]
	protected class ButtonInteractionState : BaseInteractionState
	{
		public bool toggled;
	}

	[Header("Button Settings")]
	public bool m_MustHoldDown;

	public KeycardFunctionality.KeycardColour m_RequiredKeycardColour = KeycardFunctionality.KeycardColour.None;

	public string m_ButtonAnimParam = "Pushed";

	public Animator m_ButtonAnimator;

	private bool m_bNeedKeycard;

	private bool m_bFinished;

	private bool m_bToggleState;

	protected override void Init()
	{
		base.Init();
		if (m_ButtonAnimator != null)
		{
			m_ButtonAnimator.SetBool(m_ButtonAnimParam, value: false);
		}
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		m_bNeedKeycard = false;
		m_bFinished = false;
		if (m_RequiredKeycardColour != KeycardFunctionality.KeycardColour.None)
		{
			bool flag = false;
			List<Item> items = new List<Item>();
			localCharacter.m_ItemContainer.GetItems(ref items);
			Item equippedItem = localCharacter.GetEquippedItem();
			if (equippedItem != null)
			{
				items.Add(equippedItem);
			}
			for (int i = 0; i < items.Count; i++)
			{
				BaseItemFunctionality baseItemFunctionality = items[i].HasFunctionality(BaseItemFunctionality.Functionality.Keycard);
				if (baseItemFunctionality != null)
				{
					KeycardFunctionality keycardFunctionality = (KeycardFunctionality)baseItemFunctionality;
					if (keycardFunctionality.m_KeycardColour == m_RequiredKeycardColour)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				m_bNeedKeycard = true;
				return;
			}
		}
		if (m_MustHoldDown)
		{
			m_bToggleState = true;
			SetInteractionStateRPC(valid: true);
		}
		else
		{
			SetInteractionStateRPC(m_bToggleState = !m_bToggleState);
			m_bFinished = true;
		}
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		base.OnExitInteraction(localCharacter);
		if (m_MustHoldDown)
		{
			m_bToggleState = false;
			SetInteractionStateRPC(valid: false);
		}
	}

	public override void UpdateInteraction()
	{
		if (m_bNeedKeycard || m_bFinished)
		{
			if (m_bNeedKeycard)
			{
				SpeechManager.GetInstance().SaySomething(m_interactingCharacter, "Text.Player.NeedKeycard", SpeechTone.Negative, 3f, 10);
			}
			RequestStopInteraction(m_interactingCharacter);
		}
		base.UpdateInteraction();
	}

	protected override void OnInteractionStateChanged(bool valid)
	{
		if (m_ButtonAnimator != null)
		{
			m_ButtonAnimator.SetBool(m_ButtonAnimParam, valid);
		}
	}

	protected override BaseInteractionState CreateInteractionStateInfo()
	{
		ButtonInteractionState buttonInteractionState = new ButtonInteractionState();
		buttonInteractionState.toggled = m_bToggleState;
		return buttonInteractionState;
	}

	protected override void SetInteractionStateFromInfo(BaseInteractionState state)
	{
		ButtonInteractionState buttonInteractionState = (ButtonInteractionState)state;
		if (buttonInteractionState != null)
		{
			m_bToggleState = buttonInteractionState.toggled;
		}
	}
}
