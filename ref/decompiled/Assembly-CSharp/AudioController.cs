using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class AudioController : MonoBehaviour
{
	public enum SOUND_AREA
	{
		SA_FRONTEND,
		SA_UI,
		SA_INGAME
	}

	private enum MusicState
	{
		Playing,
		Resumed,
		Stopped,
		Paused
	}

	public static AudioController Instance = null;

	[Range(0f, 100f)]
	public int m_MusicVolume = 50;

	[Range(0f, 100f)]
	public int m_SFXVolume = 50;

	public const string PLAY = "Play_";

	private const string STOP = "Stop_";

	private const string RESUME = "Resume_";

	private const string PAUSE = "Pause_";

	private const string MUSIC = "Music_";

	private bool m_bStartMusicVolumeSet;

	private bool m_bStartSFXVolumeSet;

	private static GameObject m_MusicAndAmbienceGO = null;

	private static GameObject m_UI_GO = null;

	private static Dictionary<Events, MusicState> m_RoutinesMusic = new Dictionary<Events, MusicState>();

	public static GameObject InGameMusicAndAmbienceObject => m_MusicAndAmbienceGO;

	public static GameObject UI_Audio_GO => m_UI_GO;

	private void Awake()
	{
		if (Instance != null)
		{
			Object.Destroy(this);
			return;
		}
		Instance = this;
		m_MusicAndAmbienceGO = new GameObject("InGame_MusicAndAmbienceGO");
		m_MusicAndAmbienceGO.transform.SetParent(base.transform);
		m_MusicAndAmbienceGO.transform.localPosition = Vector3.zero;
		m_UI_GO = new GameObject("UI_AUDIO_GO");
		m_UI_GO.transform.SetParent(base.transform);
		m_UI_GO.transform.localPosition = Vector3.zero;
	}

	protected virtual void OnDestroy()
	{
		if (Instance == this)
		{
			if (m_MusicAndAmbienceGO != null)
			{
				Object.Destroy(m_MusicAndAmbienceGO);
				m_MusicAndAmbienceGO = null;
			}
			if (m_UI_GO != null)
			{
				Object.Destroy(m_UI_GO);
				m_UI_GO = null;
			}
			if (m_RoutinesMusic != null)
			{
				m_RoutinesMusic.Clear();
			}
			Instance = null;
		}
	}

	public static void RegisterToPrisonAlertnessManager()
	{
		if (PrisonAlertnessManager.GetInstance() != null)
		{
			PrisonAlertnessManager.GetInstance().OnPrisonAlertnessChanged += Instance.OnPrisonAlertnessChanged;
		}
	}

	public static void UnregisterToPrisonAlertnessManager()
	{
		if (PrisonAlertnessManager.GetInstance() != null && Instance != null)
		{
			PrisonAlertnessManager.GetInstance().OnPrisonAlertnessChanged -= Instance.OnPrisonAlertnessChanged;
		}
	}

	public static uint GetListenerMask(SOUND_AREA area)
	{
		uint result = 0u;
		switch (area)
		{
		case SOUND_AREA.SA_FRONTEND:
		case SOUND_AREA.SA_UI:
			result = 1u;
			break;
		case SOUND_AREA.SA_INGAME:
			result = 30u;
			break;
		}
		return result;
	}

	private static void SetupMasks(GameObject obj, SOUND_AREA area)
	{
		if (Instance != null && !Instance.m_bStartSFXVolumeSet)
		{
			Instance.m_bStartSFXVolumeSet = true;
			SetParameter(Game_Parameter.SFX_Volume, (float)Instance.m_SFXVolume / 100f);
		}
		if (Instance != null && !Instance.m_bStartMusicVolumeSet)
		{
			Instance.m_bStartMusicVolumeSet = true;
			SetParameter(Game_Parameter.Music_Volume, (float)Instance.m_MusicVolume / 100f);
		}
		uint listenerMask = GetListenerMask(area);
		AkSoundEngine.SetActiveListeners(obj, listenerMask);
	}

	public static bool SendEvent(SOUND_AREA area, string soundEvent, GameObject objectThatRequested)
	{
		SetupMasks(objectThatRequested, area);
		if (AkSoundEngine.PostEvent(soundEvent, objectThatRequested) == 0)
		{
			if (objectThatRequested != null)
			{
			}
			return false;
		}
		return true;
	}

	public static bool SendEvent(SOUND_AREA area, Events soundEvent, GameObject objectThatRequested)
	{
		SetupMasks(objectThatRequested, area);
		if (AkSoundEngine.PostEvent(soundEvent.ToString(), objectThatRequested) == 0)
		{
			if (objectThatRequested != null)
			{
			}
			return false;
		}
		return true;
	}

	public static bool PlayRoutineMusic(Events soundEvent)
	{
		MusicState value = MusicState.Stopped;
		if (m_RoutinesMusic.TryGetValue(soundEvent, out value))
		{
			if (value == MusicState.Paused || value == MusicState.Stopped)
			{
				string text = soundEvent.ToString();
				text = text.Replace("Play_", "Resume_");
				if (SendEvent(SOUND_AREA.SA_INGAME, text, InGameMusicAndAmbienceObject))
				{
					m_RoutinesMusic[soundEvent] = MusicState.Resumed;
					return true;
				}
				if (SendEvent(SOUND_AREA.SA_INGAME, soundEvent, InGameMusicAndAmbienceObject))
				{
					m_RoutinesMusic[soundEvent] = MusicState.Playing;
					return true;
				}
				m_RoutinesMusic[soundEvent] = MusicState.Stopped;
				return false;
			}
			return true;
		}
		if (SendEvent(SOUND_AREA.SA_INGAME, soundEvent, InGameMusicAndAmbienceObject))
		{
			m_RoutinesMusic.Add(soundEvent, MusicState.Playing);
			return true;
		}
		return false;
	}

	public static bool PauseRoutineMusic(Events soundEvent)
	{
		MusicState value = MusicState.Stopped;
		if (m_RoutinesMusic.TryGetValue(soundEvent, out value) && (value == MusicState.Playing || value == MusicState.Resumed))
		{
			string text = soundEvent.ToString();
			if (text.Contains("Play_"))
			{
				text = text.Replace("Play_", "Pause_");
				if (!SendEvent(SOUND_AREA.SA_INGAME, text, InGameMusicAndAmbienceObject))
				{
					text = text.Replace("Pause_", "Stop_");
					if (!SendEvent(SOUND_AREA.SA_INGAME, text, InGameMusicAndAmbienceObject))
					{
						return false;
					}
					m_RoutinesMusic[soundEvent] = MusicState.Stopped;
					return true;
				}
				m_RoutinesMusic[soundEvent] = MusicState.Paused;
				return true;
			}
		}
		return false;
	}

	public static bool StopRoutineMusic(Events soundEvent)
	{
		MusicState value = MusicState.Stopped;
		if (m_RoutinesMusic.TryGetValue(soundEvent, out value))
		{
			if (value == MusicState.Playing || value == MusicState.Resumed || value == MusicState.Paused)
			{
				string text = soundEvent.ToString();
				if (text.Contains("Play_"))
				{
					text = text.Replace("Play_", "Stop_");
					if (!SendEvent(SOUND_AREA.SA_INGAME, text, InGameMusicAndAmbienceObject))
					{
						return false;
					}
					m_RoutinesMusic[soundEvent] = MusicState.Stopped;
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public static bool PlayPrisonAmbience(Events soundEvent)
	{
		string text = soundEvent.ToString();
		if (text.Contains("Stop_"))
		{
			text = text.Replace("Stop_", "Play_");
		}
		if (SendEvent(SOUND_AREA.SA_INGAME, text, InGameMusicAndAmbienceObject))
		{
			return true;
		}
		return false;
	}

	public static bool StopPrisonAmbience(Events soundEvent)
	{
		string text = soundEvent.ToString();
		if (text.Contains("Play_"))
		{
			text = text.Replace("Play_", "Stop_");
		}
		if (SendEvent(SOUND_AREA.SA_INGAME, text, InGameMusicAndAmbienceObject))
		{
			return true;
		}
		return false;
	}

	public static void ClearRoutinesMusic()
	{
		m_RoutinesMusic.Clear();
	}

	public static bool SetSwitch(Switch_Group group, string switchState, GameObject objectThatRequested)
	{
		AKRESULT aKRESULT = AkSoundEngine.SetSwitch(group.ToString(), switchState, objectThatRequested);
		if (aKRESULT != AKRESULT.AK_Success)
		{
			return false;
		}
		return true;
	}

	public static bool SetState(State_Group stateGroup, string state)
	{
		AKRESULT aKRESULT = AkSoundEngine.SetState(stateGroup.ToString(), state);
		if (aKRESULT != AKRESULT.AK_Success)
		{
			return false;
		}
		return true;
	}

	public void OnPrisonAlertnessChanged(PrisonAlertness alertness)
	{
		switch (alertness)
		{
		case PrisonAlertness.OneStar:
		case PrisonAlertness.OneAndAHalfStars:
			SetState(State_Group.Music_Threat_Level, Music_Threat_Level.Music_Threat_Level_01.ToString());
			break;
		case PrisonAlertness.TwoStars:
		case PrisonAlertness.TwoAndAHalfStars:
			SetState(State_Group.Music_Threat_Level, Music_Threat_Level.Music_Threat_Level_02.ToString());
			break;
		case PrisonAlertness.ThreeStars:
		case PrisonAlertness.ThreeAndAHalfStars:
			SetState(State_Group.Music_Threat_Level, Music_Threat_Level.Music_Threat_Level_03.ToString());
			break;
		case PrisonAlertness.FourStars:
		case PrisonAlertness.FourAndAHalfStars:
			SetState(State_Group.Music_Threat_Level, Music_Threat_Level.Music_Threat_Level_04.ToString());
			break;
		case PrisonAlertness.FiveStars:
			SetState(State_Group.Music_Threat_Level, Music_Threat_Level.Music_Threat_Level_05.ToString());
			break;
		default:
			SetState(State_Group.Music_Threat_Level, Music_Threat_Level.None.ToString());
			break;
		}
	}

	public static void InitialiseForBootFlow()
	{
		float num = PlayerPrefs.GetInt("BootFlowAudio:SFXVol", 100);
		float num2 = PlayerPrefs.GetInt("BootFlowAudio:MusicVol", 100);
		SetParameter(Game_Parameter.SFX_Volume, num / 100f);
		SetParameter(Game_Parameter.Music_Volume, num2 / 100f);
	}

	public static bool SetParameter(Game_Parameter param, float value, GameObject objectThatRequested)
	{
		if (param == Game_Parameter.Character_Outfit)
		{
			value = Mathf.Clamp(value, 0f, 73f);
			AKRESULT aKRESULT = AkSoundEngine.SetRTPCValue(param.ToString(), value, objectThatRequested);
			if (aKRESULT != AKRESULT.AK_Success)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public static bool SetParameter(Game_Parameter param, float value)
	{
		switch (param)
		{
		case Game_Parameter.SFX_Volume:
		{
			value = Mathf.Clamp01(value);
			float num2 = value * 100f;
			Instance.m_SFXVolume = (int)num2;
			break;
		}
		case Game_Parameter.Music_Volume:
		{
			value = Mathf.Clamp01(value);
			float num = value * 100f;
			Instance.m_MusicVolume = (int)num;
			break;
		}
		case Game_Parameter.Reverb_Vent:
		case Game_Parameter.Reverb_Underground:
			value = Mathf.Clamp01(value);
			break;
		case Game_Parameter.Time_Of_Day:
			value = Mathf.Clamp(value, 0f, 24f);
			break;
		}
		AKRESULT aKRESULT = AkSoundEngine.SetRTPCValue(param.ToString(), value);
		if (aKRESULT != AKRESULT.AK_Success)
		{
			return false;
		}
		return true;
	}

	public static bool IsBankLoaded(string bankName)
	{
		return AkBankManager.IsBankLoaded(bankName);
	}

	public static void LoadBank(string bankName)
	{
		AkBankManager.LoadBank(bankName, decodeBank: false, saveDecodedBank: false);
	}

	public static void UnloadBank(string bankName)
	{
		AkBankManager.UnloadBank(bankName);
	}

	public static int GetCurrentMusicVolume()
	{
		return (Instance != null) ? Instance.m_MusicVolume : 0;
	}

	public static int GetCurrentSFXVolume()
	{
		return (Instance != null) ? Instance.m_SFXVolume : 0;
	}

	public static void Reset()
	{
		Instance.m_MusicVolume = 80;
		Instance.m_SFXVolume = 80;
		Instance.m_bStartSFXVolumeSet = false;
		Instance.m_bStartMusicVolumeSet = false;
	}
}
