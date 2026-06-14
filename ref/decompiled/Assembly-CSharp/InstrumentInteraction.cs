using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class InstrumentInteraction : AnimatedInteraction
{
	public Player_Musical_Instruments m_MusicTrack = Player_Musical_Instruments.Prison_01;

	public Game_Parameter m_Instrument = Game_Parameter.Musical_Instrument_Bongos;

	private readonly Game_Parameter[] m_InstrumentsToClear = new Game_Parameter[5]
	{
		Game_Parameter.Musical_Instrument_Bongos,
		Game_Parameter.Musical_Instrument_DoubleBass,
		Game_Parameter.Musical_Instrument_Guitar,
		Game_Parameter.Musical_Instrument_Harmonica,
		Game_Parameter.Musical_Instrument_Washboard
	};

	public GameObject m_MusicSource;

	private static List<int> m_Instrumentalists = new List<int>();

	private float m_TimeWhenStartingToPlay;

	protected override void Init()
	{
		base.Init();
		for (int num = m_InstrumentsToClear.Length - 1; num >= 0; num--)
		{
			AudioController.SetParameter(m_InstrumentsToClear[num], 0f);
		}
		m_Instrumentalists.Clear();
	}

	protected override void OnDestroy()
	{
		m_Instrumentalists.Clear();
		base.OnDestroy();
	}

	public override void InteractionReadyStart()
	{
		base.InteractionReadyStart();
		m_interactingCharacter.m_CharacterStats.SetCharacterState(StatModifierEnum.Sitting);
	}

	public override void InteractionReadyEnd(bool interruption = false)
	{
		base.InteractionReadyEnd(interruption);
		m_interactingCharacter.m_CharacterStats.UnSetCharacterState(StatModifierEnum.Sitting);
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		if (localCharacter.m_CharacterStats.m_bIsPlayer)
		{
			m_TimeWhenStartingToPlay = RoutineManager.GetInstance().GetElapsedSeconds();
		}
		base.OnStartInteraction(localCharacter);
	}

	public override void InteractionStartedEvent(Character interactingCharacter)
	{
		base.InteractionStartedEvent(interactingCharacter);
		bool flag = m_Instrumentalists.Count == 0;
		if (null != interactingCharacter)
		{
			int viewID = interactingCharacter.m_NetView.viewID;
			if (!m_Instrumentalists.Contains(viewID))
			{
				m_Instrumentalists.Add(viewID);
			}
		}
		if (flag && m_MusicSource != null)
		{
			AudioController.SetSwitch(Switch_Group.Player_Musical_Instruments, m_MusicTrack.ToString(), m_MusicSource);
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Instruments_Full_Track, m_MusicSource);
		}
		AudioController.SetParameter(m_Instrument, 1f);
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		AudioController.SetParameter(m_Instrument, 0f);
		if (null != interactingCharacter)
		{
			int viewID = interactingCharacter.m_NetView.viewID;
			m_Instrumentalists.Remove(viewID);
		}
		if (m_Instrumentalists.Count == 0 && m_MusicSource != null)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_Instruments_Full_Track, m_MusicSource);
		}
		if (null != interactingCharacter)
		{
			interactingCharacter.ClearRemoteInterativeObject();
		}
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		if (null != localCharacter && null != localCharacter.m_CharacterStats && localCharacter.m_CharacterStats.m_bIsPlayer)
		{
			float num = RoutineManager.GetInstance().GetElapsedSeconds() - m_TimeWhenStartingToPlay;
			num /= 60f;
			if (num > 1f)
			{
				StatSystem.GetInstance().IncStat(12, num, ((Player)localCharacter).m_Gamer, string.Empty);
			}
			m_TimeWhenStartingToPlay = 0f;
		}
		base.OnExitInteraction(localCharacter);
	}
}
