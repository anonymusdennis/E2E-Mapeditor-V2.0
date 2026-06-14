using System;
using UnityEngine;

public class CutsceneSpeechBubbleHandler : CharacterSpeechBubbleHandler
{
	[Serializable]
	public class SpeechBubblePreset
	{
		public Sprite m_BackgroundSprite;

		public Sprite m_TailSprite;
	}

	public enum BubblePresets
	{
		Unassigned = -1,
		Guard,
		Inmate,
		Medic,
		Dog,
		JobOfficer,
		MaintenanceMan,
		Visitor
	}

	public SpeechBubblePreset m_GuardPreset;

	public SpeechBubblePreset m_InmatePreset;

	public SpeechBubblePreset m_MedicPreset;

	public SpeechBubblePreset m_DogPreset;

	public SpeechBubblePreset m_JobOfficerPreset;

	public SpeechBubblePreset m_MaintenanceManPreset;

	public SpeechBubblePreset m_VisitorPreset;

	protected override void Awake()
	{
		if (m_BackgroundSprite == null || m_TailSprite == null)
		{
			SetPreset(BubblePresets.Guard);
		}
		base.Awake();
	}

	protected override void OnDestroy()
	{
		m_GuardPreset.m_BackgroundSprite = null;
		m_GuardPreset.m_TailSprite = null;
		m_InmatePreset.m_BackgroundSprite = null;
		m_InmatePreset.m_TailSprite = null;
		m_MedicPreset.m_BackgroundSprite = null;
		m_MedicPreset.m_TailSprite = null;
		m_DogPreset.m_BackgroundSprite = null;
		m_DogPreset.m_TailSprite = null;
		m_JobOfficerPreset.m_BackgroundSprite = null;
		m_JobOfficerPreset.m_TailSprite = null;
		m_MaintenanceManPreset.m_BackgroundSprite = null;
		m_MaintenanceManPreset.m_TailSprite = null;
		m_VisitorPreset.m_BackgroundSprite = null;
		m_VisitorPreset.m_TailSprite = null;
		base.OnDestroy();
	}

	public void SetPreset(BubblePresets newPreset)
	{
		SpeechBubblePreset speechBubblePreset = newPreset switch
		{
			BubblePresets.Dog => m_DogPreset, 
			BubblePresets.Guard => m_GuardPreset, 
			BubblePresets.Inmate => m_InmatePreset, 
			BubblePresets.JobOfficer => m_JobOfficerPreset, 
			BubblePresets.MaintenanceMan => m_MaintenanceManPreset, 
			BubblePresets.Medic => m_MedicPreset, 
			BubblePresets.Visitor => m_VisitorPreset, 
			_ => null, 
		};
		if (speechBubblePreset != null)
		{
			m_TailSprite = speechBubblePreset.m_TailSprite;
			m_BackgroundSprite = speechBubblePreset.m_BackgroundSprite;
		}
	}
}
