using System;
using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class RoutineAndTimeTrackerHUD : T17MonoBehaviour, IEventCleaner
{
	public T17Text m_Routine;

	public T17Slider m_LockdownTimer;

	public T17Slider m_LockdownGraceTimer;

	public T17Slider m_TransportPrisonTimer;

	public Animator m_AlarmBellAnimator;

	[Tooltip("The most significant part of the hour")]
	[Header("Time References")]
	public T17Image m_Hour1;

	[Tooltip("The least significant part of the hour")]
	public T17Image m_Hour2;

	[Tooltip("The most significant part of the minutes")]
	public T17Image m_Min1;

	[Tooltip("The least significant part of the hour")]
	public T17Image m_Min2;

	[Tooltip("The colon that seperates the hour and the minutes")]
	public T17Image m_TimeColon;

	[Tooltip("Please put these in order from 0 - 9")]
	public List<Sprite> m_ImageReferences;

	private RoutineManager m_RoutineManager;

	private MiniMap m_MiniMap;

	private T17Button m_MiniMapButton;

	private Player m_CurrentPlayer;

	[Range(1f, 59f)]
	[Header("HUD Behaviours")]
	public int m_NumMinutesBeforeClockFlash = 10;

	[Range(1f, 59f)]
	public int m_NumMinutesBeforeTransportClockFlash = 60;

	[Range(1f, 59f)]
	public int m_NumMinutesBeforeBeepingSound = 5;

	[Range(1f, 59f)]
	public int m_NumMinutesBeforeTransportBeepingSound = 30;

	public float m_BlinkDuration = 0.3f;

	public float m_BellAnimationDuration = 2f;

	public float m_TemporaryRoutineDisplayDuration = 4f;

	public float m_RoutineSuffixBlinkTime = 0.5f;

	public float m_PIPDisplayBlinkTime = 0.5f;

	[Header("Routine Text")]
	[Localization]
	public string m_GraceTimerTextKey = string.Empty;

	[Localization]
	[Header("Time remaining text")]
	public string m_TimeRemainingTextKey = string.Empty;

	private int m_PreviousHour = -1;

	private int m_PreviousMin = -1;

	private bool m_bFlashDigitFigures;

	private bool m_bPlayBeepingSound;

	private float m_BlinkCountdown;

	private float m_BellAnimationCountdown;

	private float m_TemporaryRoutineDisplayCountdown;

	private float m_RoutineSuffixBlinkCountdown;

	private bool m_bIsRoutineSuffixVisible;

	private int m_RoutineSuffixMaxNumBlinks;

	private int m_RoutineSuffixNumBlinksDone;

	private bool m_bHoldRoutineSuffixAfterBlinks;

	private float m_PIPMessageBlinkCountdown;

	private bool m_bIsPIPMessageVisible;

	private bool m_bIsShowingPIPMessage;

	private bool m_bPIPMessageJustEnded;

	private RoutineManager.CallbackInGameTimer m_FlashCallback;

	private RoutineManager.CallbackInGameTimer m_BeepCallback;

	private bool m_bLockdownTimerActive;

	private bool m_bGraceTimerActive;

	private bool m_bTransportPrisonTimerActive;

	private bool m_bShowClock;

	private string m_RoutineTextKey = string.Empty;

	private string m_RoutineTextSuffix = string.Empty;

	private string m_OldRoutineTextSuffix = string.Empty;

	private string m_PIPLocalizationTag = string.Empty;

	private RoutinesData.Routine m_CurrentRoutine;

	private RoutinesData.Routine m_OldRoutine;

	private const string ROUTINE_TICK_SUFFIX = "  [TICON=Tick]";

	public Player CurrentPlayer => m_CurrentPlayer;

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		base.StartInit();
		if (m_ImageReferences.Count != 10)
		{
		}
		m_RoutineSuffixMaxNumBlinks = Mathf.RoundToInt(m_TemporaryRoutineDisplayDuration / m_RoutineSuffixBlinkTime);
		PostInit();
		return T17BehaviourManager.INITSTATE.IS_FINISHED;
	}

	public void PostInit()
	{
		m_bShowClock = LevelScript.GetCurrentLevelInfo() == null || LevelScript.GetCurrentLevelInfo().m_PrisonType != LevelScript.PRISON_TYPE.Tutorial;
		if (!m_bShowClock)
		{
			SetClockFiguresActive(state: false);
			m_TimeColon.enabled = false;
		}
		m_MiniMap = base.gameObject.GetComponentInChildren<MiniMap>();
		m_RoutineManager = RoutineManager.GetInstance();
		if (m_RoutineManager != null)
		{
			m_RoutineManager.OnRoutineChangedManagerStart += OnRoutineChanged;
			OnRoutineChanged(null, m_RoutineManager.GetCurrentRoutine(), forceEnd: false);
		}
		if (m_LockdownGraceTimer != null)
		{
			m_LockdownGraceTimer.value = 0f;
		}
	}

	protected virtual void OnDestroy()
	{
		UnsubscribeToEvents();
	}

	private void UnsubscribeToEvents()
	{
		if (CurrentPlayer != null)
		{
			CurrentPlayer.ReachedRoutineLocationEvent -= Player_ReachedRoutineLocationEvent;
			CurrentPlayer.MissedRoutineLocationEvent -= Player_MissedRoutineLocationEvent;
		}
		if (m_RoutineManager != null)
		{
			m_RoutineManager.RemoveCallbackTimer(m_FlashCallback);
			m_RoutineManager.RemoveCallbackTimer(m_BeepCallback);
			m_RoutineManager.OnRoutineChangedManagerStart -= OnRoutineChanged;
			m_RoutineManager = null;
		}
	}

	private void Update()
	{
		if (m_RoutineManager != null)
		{
			ProcessTimeComponent();
			ProcessTimersAndCountdowns();
		}
		if (m_RoutineSuffixBlinkCountdown > 0f)
		{
			RoutinesData.Routine routine = ((!(m_TemporaryRoutineDisplayCountdown > 0f)) ? m_CurrentRoutine : m_OldRoutine);
			if (routine != null)
			{
				m_RoutineSuffixBlinkCountdown -= UpdateManager.deltaTime;
				if (m_RoutineSuffixBlinkCountdown <= 0f)
				{
					string suffix = ((!(m_TemporaryRoutineDisplayCountdown > 0f)) ? m_RoutineTextSuffix : m_OldRoutineTextSuffix);
					m_RoutineSuffixNumBlinksDone++;
					if (m_bIsRoutineSuffixVisible)
					{
						SetRoutineText(routine.m_LocalizationTag, suffix);
					}
					else if (m_RoutineSuffixNumBlinksDone < m_RoutineSuffixMaxNumBlinks)
					{
						if (m_Routine != null)
						{
							m_Routine.gameObject.SetActive(value: false);
						}
					}
					else
					{
						SetRoutineText(routine.m_LocalizationTag);
					}
					m_bIsRoutineSuffixVisible = !m_bIsRoutineSuffixVisible;
					if (m_RoutineSuffixNumBlinksDone < m_RoutineSuffixMaxNumBlinks)
					{
						m_RoutineSuffixBlinkCountdown = m_RoutineSuffixBlinkTime;
					}
					else if (m_bHoldRoutineSuffixAfterBlinks)
					{
						SetRoutineText(routine.m_LocalizationTag, suffix);
					}
				}
			}
		}
		if ((m_TemporaryRoutineDisplayCountdown > 0f || m_bPIPMessageJustEnded) && m_CurrentRoutine != null)
		{
			if (m_bPIPMessageJustEnded)
			{
				m_bPIPMessageJustEnded = false;
			}
			m_TemporaryRoutineDisplayCountdown -= UpdateManager.deltaTime;
			if (m_TemporaryRoutineDisplayCountdown <= 0f)
			{
				SetRoutineText(m_CurrentRoutine.m_LocalizationTag, m_RoutineTextSuffix);
				if (m_bGraceTimerActive)
				{
					OverrideRoutineText(m_GraceTimerTextKey);
				}
			}
		}
		if (!m_bIsShowingPIPMessage || string.IsNullOrEmpty(m_PIPLocalizationTag))
		{
			return;
		}
		m_PIPMessageBlinkCountdown -= UpdateManager.deltaTime;
		if (m_PIPMessageBlinkCountdown <= 0f)
		{
			if (m_bIsPIPMessageVisible)
			{
				SetRoutineText(m_PIPLocalizationTag);
			}
			else if (m_Routine != null)
			{
				m_Routine.gameObject.SetActive(value: false);
			}
			m_bIsPIPMessageVisible = !m_bIsPIPMessageVisible;
			m_PIPMessageBlinkCountdown = m_PIPDisplayBlinkTime;
		}
	}

	private void ProcessTimersAndCountdowns()
	{
		if (m_BlinkCountdown > 0f)
		{
			m_BlinkCountdown -= UpdateManager.deltaTime;
			if (m_BlinkCountdown <= 0f)
			{
				m_TimeColon.enabled = false;
				if (m_bFlashDigitFigures)
				{
					SetClockFiguresActive(state: false);
				}
			}
		}
		if (m_AlarmBellAnimator != null && m_BellAnimationCountdown > 0f)
		{
			m_BellAnimationCountdown -= UpdateManager.deltaTime;
			if (m_BellAnimationCountdown <= 0f)
			{
				m_AlarmBellAnimator.ResetTrigger("Ring");
				m_AlarmBellAnimator.SetTrigger("StopRing");
			}
		}
		if (m_LockdownGraceTimer != null)
		{
			RoutineManager instance = RoutineManager.GetInstance();
			if (instance != null && m_CurrentPlayer != null)
			{
				float num = 0f;
				bool flag = false;
				RoutinesData.Routine currentRoutine = instance.GetCurrentRoutine();
				if (currentRoutine != null)
				{
					num = currentRoutine.m_TimeToGetToRoutine;
					flag = (currentRoutine.m_BaseRoutineType == Routines.Lockdown || currentRoutine.m_BaseRoutineType == Routines.LightsOut) && num > 0f;
					flag &= !m_CurrentPlayer.HasReachedRoutineLocation && !m_CurrentPlayer.m_bIsTardy;
					flag &= !m_CurrentPlayer.m_bIsWantedForSolitary;
				}
				if (m_bGraceTimerActive != flag)
				{
					m_bGraceTimerActive = flag;
					m_LockdownGraceTimer.gameObject.SetActive(flag);
					if (flag)
					{
						OverrideRoutineText(m_GraceTimerTextKey);
					}
					else
					{
						SetRoutineText(m_RoutineTextKey);
					}
				}
				if (flag)
				{
					m_LockdownGraceTimer.value = Mathf.Max(m_CurrentPlayer.GetToRoutineTimer / num, 0f);
				}
				else
				{
					m_LockdownGraceTimer.value = 0f;
				}
			}
		}
		if (m_LockdownTimer != null)
		{
			SolitaryManager instance2 = SolitaryManager.GetInstance();
			if (instance2 != null)
			{
				bool flag2 = !m_bGraceTimerActive && instance2.IsLockdownActive();
				if (flag2 != m_bLockdownTimerActive)
				{
					m_bLockdownTimerActive = flag2;
					m_LockdownTimer.gameObject.SetActive(flag2);
				}
				if (flag2)
				{
					m_LockdownTimer.value = 1f - instance2.GetLockdownProgress();
				}
			}
		}
		if (!(m_TransportPrisonTimer != null))
		{
			return;
		}
		RoutineManager instance3 = RoutineManager.GetInstance();
		if (!(instance3 != null) || !(instance3.m_RoutinesData != null) || !instance3.m_RoutinesData.m_bIsTimedPrison)
		{
			return;
		}
		bool flag3 = !m_bGraceTimerActive && !m_bLockdownTimerActive;
		if (flag3)
		{
			m_bTransportPrisonTimerActive = flag3;
			m_TransportPrisonTimer.gameObject.SetActive(flag3);
			if (!m_bIsShowingPIPMessage)
			{
				OverrideRoutineText(m_TimeRemainingTextKey);
			}
			int startOfTheDayHour = instance3.m_RoutinesData.m_StartOfTheDayHour;
			int startOfTheDayMinutes = instance3.m_RoutinesData.m_StartOfTheDayMinutes;
			float num2 = startOfTheDayHour * 60 * 60 + startOfTheDayMinutes * 60;
			float num3 = instance3.m_RoutinesData.m_TimedHoursDuration * 60 * 60 + instance3.m_RoutinesData.m_TimedMinutesDuration * 60;
			m_TransportPrisonTimer.value = 1f - (instance3.GetElapsedSeconds() - num2) / num3;
		}
	}

	private void ProcessTimeComponent()
	{
		int timeHourPart = m_RoutineManager.TimeHourPart;
		int timeMinutePart = m_RoutineManager.TimeMinutePart;
		if (timeHourPart != m_PreviousHour)
		{
			SetTimeGraphicsForNumber(timeHourPart, m_Hour1, m_Hour2);
			m_PreviousHour = timeHourPart;
		}
		if (timeMinutePart != m_PreviousMin)
		{
			SetTimeGraphicsForNumber(timeMinutePart, m_Min1, m_Min2);
			m_PreviousMin = timeMinutePart;
			m_BlinkCountdown = m_BlinkDuration;
			if (m_bShowClock)
			{
				SetClockFiguresActive(state: true);
				m_TimeColon.enabled = true;
			}
			if (m_bPlayBeepingSound)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_Routine_Bell_Alarm_Beep, base.gameObject);
			}
		}
	}

	private void OnRoutineChanged(RoutinesData.Routine oldRoutine, RoutinesData.Routine newRoutine, bool forceEnd)
	{
		if (!(m_Routine != null))
		{
			return;
		}
		if (newRoutine != null)
		{
			if (oldRoutine != null)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_Routine_Bell_Classic, base.gameObject);
				if (m_AlarmBellAnimator != null)
				{
					m_AlarmBellAnimator.SetTrigger("Ring");
					m_BellAnimationCountdown = m_BellAnimationDuration;
				}
				m_bFlashDigitFigures = false;
				if (m_bShowClock)
				{
					SetClockFiguresActive(state: true);
				}
			}
			m_OldRoutine = oldRoutine;
			m_CurrentRoutine = newRoutine;
			m_RoutineTextSuffix = string.Empty;
			m_bHoldRoutineSuffixAfterBlinks = false;
			bool flag = true;
			if (m_CurrentPlayer != null)
			{
				flag = m_CurrentPlayer.HasReachedRoutineLocation;
			}
			if (oldRoutine != null && oldRoutine.m_BaseRoutineType == Routines.JobTime && !flag && JobsManager.GetInstance() != null)
			{
				BaseJob charactersJob = JobsManager.GetInstance().GetCharactersJob(m_CurrentPlayer);
				if (charactersJob != null)
				{
					bool flag2 = charactersJob != null && JobsManager.GetInstance().WasJobInFirstTimeGracePeriod(oldRoutine, charactersJob);
					flag = m_CurrentPlayer.m_bHaveAnyQuotaDone || flag2;
				}
			}
			if (LevelScript.GetCurrentLevelInfo().m_PrisonEnum == LevelScript.PRISON_ENUM.Tutorial && oldRoutine != null && oldRoutine.m_BaseRoutineType == Routines.JobTime)
			{
				flag = true;
			}
			if (oldRoutine != null && m_CurrentPlayer != null && !flag && newRoutine.m_BaseRoutineType != Routines.Lockdown && oldRoutine.m_BaseRoutineType != Routines.Lockdown)
			{
				string text = ((oldRoutine.m_BaseRoutineType != Routines.Lockdown) ? "  [TICON=Cross]" : null);
				RoutineManager instance = RoutineManager.GetInstance();
				if (instance != null && instance.m_RoutinesData != null && !instance.m_RoutinesData.m_bIsTimedPrison)
				{
					m_bIsRoutineSuffixVisible = true;
					m_RoutineSuffixBlinkCountdown = m_RoutineSuffixBlinkTime;
					m_RoutineSuffixNumBlinksDone = 0;
					SetRoutineText(oldRoutine.m_LocalizationTag, text);
					m_OldRoutineTextSuffix = text;
					m_TemporaryRoutineDisplayCountdown = m_TemporaryRoutineDisplayDuration;
				}
			}
			else
			{
				RoutineManager instance2 = RoutineManager.GetInstance();
				if (instance2 != null && !instance2.m_RoutinesData.m_bIsTimedPrison)
				{
					string suffix = null;
					if (m_CurrentPlayer != null && oldRoutine == null && (m_CurrentPlayer.HasReachedRoutineLocation || (m_CurrentRoutine.m_BaseRoutineType == Routines.JobTime && JobsManager.GetInstance() != null && JobsManager.GetInstance().HasCharacterMetQuota(m_CurrentPlayer))))
					{
						suffix = "  [TICON=Tick]";
					}
					SetRoutineText(newRoutine.m_LocalizationTag, suffix);
				}
			}
			m_bPlayBeepingSound = false;
			if (m_RoutineManager.IsTimeSpedUp())
			{
				return;
			}
			TimeSpan timeSpan;
			TimeSpan timeSpan2;
			if (!m_RoutineManager.m_RoutinesData.m_bIsTimedPrison)
			{
				TimeSpan timeLeftInRoutine = m_RoutineManager.GetTimeLeftInRoutine();
				timeSpan = timeLeftInRoutine - TimeSpan.FromMinutes(m_NumMinutesBeforeClockFlash);
				if (m_FlashCallback != null)
				{
					m_RoutineManager.RemoveCallbackTimer(m_FlashCallback);
				}
				timeSpan2 = timeLeftInRoutine - TimeSpan.FromMinutes(m_NumMinutesBeforeBeepingSound);
			}
			else
			{
				TimeSpan timeSpan3 = new TimeSpan(m_RoutineManager.m_RoutinesData.m_TimedHoursDuration, m_RoutineManager.m_RoutinesData.m_TimedMinutesDuration, 0);
				timeSpan2 = timeSpan3 - TimeSpan.FromMinutes(m_NumMinutesBeforeTransportBeepingSound);
				timeSpan = timeSpan3 - TimeSpan.FromMinutes(m_NumMinutesBeforeTransportClockFlash);
			}
			if (m_BeepCallback != null)
			{
				m_RoutineManager.RemoveCallbackTimer(m_BeepCallback);
			}
			if (newRoutine.m_BaseRoutineType != Routines.Lockdown && (m_RoutineManager.m_RoutinesData.m_bIsTimedPrison || m_RoutineManager.GetRoutineCount() > 1))
			{
				m_FlashCallback = m_RoutineManager.CreateCallbackTimer(0, timeSpan.Hours, timeSpan.Minutes, OnPreRoutine_ClockFlash, relativeToStart: false);
				m_BeepCallback = m_RoutineManager.CreateCallbackTimer(0, timeSpan2.Hours, timeSpan2.Minutes, OnPreRoutine_ClockBeep, relativeToStart: false);
			}
		}
		else
		{
			SetRoutineText("!! No Routine !!");
		}
	}

	private void OnPreRoutine_ClockFlash()
	{
		if (m_RoutineManager != null && !m_RoutineManager.IsTimeSpedUp())
		{
			m_bFlashDigitFigures = true;
		}
	}

	private void OnPreRoutine_ClockBeep()
	{
		if (m_RoutineManager != null && !m_RoutineManager.IsTimeSpedUp())
		{
			m_bPlayBeepingSound = true;
		}
	}

	private void SetClockFiguresActive(bool state)
	{
		m_Hour1.enabled = state;
		m_Hour2.enabled = state;
		m_Min1.enabled = state;
		m_Min2.enabled = state;
	}

	private void SetTimeGraphicsForNumber(int num, T17Image significantImage, T17Image leastSignificantImage)
	{
		int index = num / 10;
		int index2 = num % 10;
		significantImage.sprite = m_ImageReferences[index];
		leastSignificantImage.sprite = m_ImageReferences[index2];
	}

	private void SetRoutineText(string key, string suffix = null)
	{
		m_RoutineTextKey = key;
		Localization.Get(key, out var localized);
		if (suffix != null)
		{
			localized += suffix;
		}
		if (m_Routine != null)
		{
			if (!m_Routine.gameObject.activeSelf)
			{
				m_Routine.gameObject.SetActive(value: true);
			}
			m_Routine.SetNewPlaceHolder(key);
			m_Routine.m_bNeedsLocalization = false;
			m_Routine.SetNewLocalizationTag(localized);
		}
	}

	private void OverrideRoutineText(string key)
	{
		if (m_Routine != null)
		{
			if (!m_Routine.gameObject.activeSelf)
			{
				m_Routine.gameObject.SetActive(value: true);
			}
			m_Routine.m_bNeedsLocalization = true;
			m_Routine.SetLocalisedTextCatchAll(key);
		}
	}

	public void SetGamePlayer(Player gamePlayer)
	{
		if (m_CurrentPlayer != null && m_CurrentPlayer != gamePlayer)
		{
			m_CurrentPlayer.ReachedRoutineLocationEvent -= Player_ReachedRoutineLocationEvent;
			m_CurrentPlayer.MissedRoutineLocationEvent -= Player_MissedRoutineLocationEvent;
		}
		m_CurrentPlayer = gamePlayer;
		SetMiniMapTarget();
		if (gamePlayer != null)
		{
			gamePlayer.ReachedRoutineLocationEvent -= Player_ReachedRoutineLocationEvent;
			gamePlayer.ReachedRoutineLocationEvent += Player_ReachedRoutineLocationEvent;
			gamePlayer.MissedRoutineLocationEvent -= Player_MissedRoutineLocationEvent;
			gamePlayer.MissedRoutineLocationEvent += Player_MissedRoutineLocationEvent;
		}
		if (m_CurrentPlayer != null && m_CurrentRoutine != null && !m_bTransportPrisonTimerActive && m_CurrentPlayer.HasReachedRoutineLocation)
		{
			SetRoutineText(m_CurrentRoutine.m_LocalizationTag, "  [TICON=Tick]");
		}
	}

	private void Player_ReachedRoutineLocationEvent(Character character, RoutinesData.Routine routine)
	{
		RoutineManager instance = RoutineManager.GetInstance();
		bool flag = instance.m_RoutinesData.m_bIsTimedPrison;
		if (LevelScript.GetCurrentLevelInfo().m_PrisonEnum == LevelScript.PRISON_ENUM.Tutorial && routine.m_BaseRoutineType == Routines.JobTime)
		{
			flag = true;
		}
		if (instance != null && instance.m_RoutinesData != null && !flag)
		{
			string text = (m_RoutineTextSuffix = ((routine.m_BaseRoutineType != Routines.Lockdown) ? "  [TICON=Tick]" : null));
			m_bIsRoutineSuffixVisible = true;
			m_RoutineSuffixBlinkCountdown = m_RoutineSuffixBlinkTime;
			m_RoutineSuffixNumBlinksDone = 0;
			m_bHoldRoutineSuffixAfterBlinks = true;
			if (m_TemporaryRoutineDisplayCountdown <= 0f)
			{
				SetRoutineText(routine.m_LocalizationTag, text);
			}
			if (routine != m_CurrentRoutine)
			{
				m_OldRoutineTextSuffix = text;
				m_TemporaryRoutineDisplayCountdown = m_TemporaryRoutineDisplayDuration;
			}
		}
	}

	private void Player_MissedRoutineLocationEvent(Character character, RoutinesData.Routine routine)
	{
		RoutineManager instance = RoutineManager.GetInstance();
		bool flag = instance.m_RoutinesData.m_bIsTimedPrison;
		if (LevelScript.GetCurrentLevelInfo().m_PrisonEnum == LevelScript.PRISON_ENUM.Tutorial && routine.m_BaseRoutineType == Routines.JobTime)
		{
			flag = true;
		}
		if (instance != null && instance.m_RoutinesData != null && !flag)
		{
			string text = ((routine.m_BaseRoutineType != Routines.Lockdown) ? "  [TICON=Cross]" : null);
			m_bIsRoutineSuffixVisible = true;
			m_RoutineSuffixBlinkCountdown = m_RoutineSuffixBlinkTime;
			m_RoutineSuffixNumBlinksDone = 0;
			m_bHoldRoutineSuffixAfterBlinks = false;
			SetRoutineText(routine.m_LocalizationTag, text);
			m_OldRoutineTextSuffix = text;
			m_TemporaryRoutineDisplayCountdown = m_TemporaryRoutineDisplayDuration;
		}
	}

	public void SetMiniMapTarget()
	{
		if (m_MiniMap == null)
		{
			m_MiniMap = base.gameObject.GetComponentInChildren<MiniMap>();
		}
		if (!(m_MiniMap != null) || !(CurrentPlayer != null))
		{
			return;
		}
		m_MiniMap.m_player = CurrentPlayer;
		m_MiniMap.m_Target = m_MiniMap.m_player.transform;
		if (m_MiniMapButton == null)
		{
			m_MiniMapButton = GetComponent<T17Button>();
		}
		if (m_MiniMapButton != null)
		{
			T17Button miniMapButton = m_MiniMapButton;
			miniMapButton.OnButtonPointerEnter = (T17Button.T17ButtonDelegate)Delegate.Remove(miniMapButton.OnButtonPointerEnter, new T17Button.T17ButtonDelegate(m_MiniMap.OnPointerEnter));
			T17Button miniMapButton2 = m_MiniMapButton;
			miniMapButton2.OnButtonPointerEnter = (T17Button.T17ButtonDelegate)Delegate.Combine(miniMapButton2.OnButtonPointerEnter, new T17Button.T17ButtonDelegate(m_MiniMap.OnPointerEnter));
			T17Button miniMapButton3 = m_MiniMapButton;
			miniMapButton3.OnButtonPointerExit = (T17Button.T17ButtonDelegate)Delegate.Remove(miniMapButton3.OnButtonPointerExit, new T17Button.T17ButtonDelegate(m_MiniMap.OnPointerExit));
			T17Button miniMapButton4 = m_MiniMapButton;
			miniMapButton4.OnButtonPointerExit = (T17Button.T17ButtonDelegate)Delegate.Combine(miniMapButton4.OnButtonPointerExit, new T17Button.T17ButtonDelegate(m_MiniMap.OnPointerExit));
			m_MiniMapButton.m_CanUIReselectDelegate = () => false;
		}
	}

	public void CleanUpEvents()
	{
		UnsubscribeToEvents();
	}

	public void SynchFromTracker(RoutineAndTimeTrackerHUD tracker)
	{
		if (tracker != null)
		{
			m_BlinkCountdown = tracker.m_BlinkCountdown;
			m_bFlashDigitFigures = tracker.m_bFlashDigitFigures;
			m_BellAnimationCountdown = tracker.m_BellAnimationCountdown;
			m_RoutineSuffixBlinkCountdown = tracker.m_RoutineSuffixBlinkCountdown;
			m_bIsRoutineSuffixVisible = tracker.m_bIsRoutineSuffixVisible;
			m_RoutineSuffixNumBlinksDone = tracker.m_RoutineSuffixNumBlinksDone;
			m_TemporaryRoutineDisplayCountdown = tracker.m_TemporaryRoutineDisplayCountdown;
			m_RoutineTextKey = tracker.m_RoutineTextKey;
			m_RoutineTextSuffix = tracker.m_RoutineTextSuffix;
			m_OldRoutineTextSuffix = tracker.m_OldRoutineTextSuffix;
			m_bHoldRoutineSuffixAfterBlinks = tracker.m_bHoldRoutineSuffixAfterBlinks;
			m_CurrentRoutine = tracker.m_CurrentRoutine;
			m_OldRoutine = tracker.m_OldRoutine;
			m_PIPMessageBlinkCountdown = tracker.m_PIPMessageBlinkCountdown;
			m_bIsPIPMessageVisible = tracker.m_bIsPIPMessageVisible;
			m_bIsShowingPIPMessage = tracker.m_bIsShowingPIPMessage;
			m_bPIPMessageJustEnded = tracker.m_bPIPMessageJustEnded;
			m_PIPLocalizationTag = tracker.m_PIPLocalizationTag;
			m_Routine.m_PlaceholderText = null;
			m_Routine.m_LocalizationTag = null;
			m_Routine.m_bNeedsLocalization = tracker.m_Routine.m_bNeedsLocalization;
			if (m_Routine.m_bNeedsLocalization)
			{
				m_Routine.SetLocalisedTextCatchAll(tracker.m_Routine.m_LocalizationTag);
			}
			else
			{
				m_Routine.SetNonLocalizedText(tracker.m_Routine.text);
			}
			if (m_Routine.gameObject.activeSelf != tracker.m_Routine.gameObject.activeSelf)
			{
				m_Routine.gameObject.SetActive(tracker.m_Routine.gameObject.activeSelf);
			}
			m_Routine.SetVerticesDirty();
			m_Routine.CheckMarkup();
		}
	}

	public void BeginPIPDisplay(string localizationTag)
	{
		m_PIPLocalizationTag = localizationTag;
		m_bIsShowingPIPMessage = true;
		m_bPIPMessageJustEnded = false;
	}

	public void EndPIPDisplay()
	{
		m_PIPLocalizationTag = null;
		m_bIsShowingPIPMessage = false;
		m_bPIPMessageJustEnded = true;
	}
}
