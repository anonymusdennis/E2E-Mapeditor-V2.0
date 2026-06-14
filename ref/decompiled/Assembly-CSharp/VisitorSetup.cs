using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "New VisitorSetup", menuName = "Team17/Character/Create New Visitor Setup")]
public class VisitorSetup : ScriptableObject
{
	[Header("Speech Lines")]
	public List<string> m_SpeechLines;

	[Header("Item Pool")]
	public RandomItemGroup m_GiftItems;

	[Header("Character Customisation")]
	public CustomisationConfig m_CustomisationPool;

	public Customisation m_CharacterCustomisation;
}
