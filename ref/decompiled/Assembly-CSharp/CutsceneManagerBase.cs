using System;
using System.Collections;
using System.Collections.Generic;
using Slate;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(T17NetView))]
[ExecuteInEditMode]
[RequireComponent(typeof(T17NetViewReserved))]
public abstract class CutsceneManagerBase : T17MonoBehaviour, Saveable
{
	public delegate void PrepareForCutsceneHandler(float timeUntilStart);

	public delegate void CutsceneFinishedHandler(float timeUntilCurtainRaised);

	public enum States
	{
		Unassigned,
		WaitingForStart,
		Playing,
		SkippingCurrent,
		Idle
	}

	[Serializable]
	private class SaveData_CutsceneManager_V1 : PrisonSnapshotIO.SnapshotData_Base
	{
		public bool P;

		public SaveData_CutsceneManager_V1()
		{
			m_Version = 1;
		}
	}

	public static CutsceneManagerBase s_Instance;

	[Tooltip("The gameobject we use to direct the game camera around the level")]
	public CutsceneCameraTrackableObject m_CameraTrackableObject;

	[Tooltip("The amount of time for fading in/out of a cutscene")]
	private float m_FadeTime = 1f;

	[Tooltip("The amount of time at the start of the cutscene where you can't skip via user input")]
	public float m_NoSkipDuration = 1f;

	[Tooltip("The amount of time since pressing a button mid cutscene, before you can actually skip")]
	public float m_ButtonsPressedDeadtime = 0.3f;

	[Tooltip("The length of time before the 'skip button gui/functionality' resets after the initial button press")]
	public float m_CutsceneSkipButtonResetTime = 3f;

	[Header("Generic cutscenes")]
	public Cutscene m_Intro;

	public Cutscene m_GenericEscapeCutscene;

	public Cutscene m_TimeServedCutscene;

	public Cutscene m_EscapeSinglePlayerFirst;

	public Cutscene m_EscapeMultiplayerFirst;

	public Cutscene m_TimesUpCutscene;

	[HideInInspector]
	public CutsceneCanvas m_OverarchingCanvas;

	private List<Cutscene> m_AllCutscenes;

	private States m_State;

	private Cutscene m_SelectedCutscene;

	private float m_TimeUntilCutscenePlay;

	private float m_TimeUntilCutsceneSkipFinished;

	private float m_TimeUntilSkipDeadtimeFinishes;

	private float m_TimeUntilSkipButtonResets;

	private UIAnimatedEffectController.Effects m_EntryEffect;

	private UIAnimatedEffectController.Effects m_ExitEffect;

	private UIAnimatedEffectController.Effects m_SkipEffect;

	private T17NetView m_NetView;

	private System.Random m_RandomGenerator;

	private Dictionary<int, PlayerMapsSnapshot> m_PreCutsceneMapSnapshots;

	private bool m_bPreventIntroCutscene;

	private Player m_ScriptedPlayerOverride;

	private SaveDataRegister m_SaveData;

	private static bool m_bDebugAllHudsDisabled;

	public static event PrepareForCutsceneHandler PrepareForCutsceneEvent;

	public static event CutsceneFinishedHandler CutsceneFinishedEvent;

	public static CutsceneManagerBase GetInstance()
	{
		return s_Instance;
	}

	public static CutsceneCameraTrackableObject GetCameraTrackableObject()
	{
		return GetInstance().m_CameraTrackableObject;
	}

	protected abstract void AddLevelSpecificScenesToList(List<Cutscene> cutscenes);

	protected override void Awake()
	{
		base.Awake();
		m_NetView = GetComponent<T17NetView>();
		m_RandomGenerator = new System.Random();
		m_PreCutsceneMapSnapshots = new Dictionary<int, PlayerMapsSnapshot>();
		m_AllCutscenes = new List<Cutscene>();
		AddAllPrisons();
		if (m_NetView == null)
		{
		}
		m_NetView.viewID = T17NetConfig.GetReservedNetID(T17NetConfig.ReservedNetID.CutsceneManager);
		if (m_CameraTrackableObject == null)
		{
			m_CameraTrackableObject = UnityEngine.Object.FindObjectOfType<CutsceneCameraTrackableObject>();
		}
		if (m_CameraTrackableObject == null)
		{
			GameObject gameObject = new GameObject("CameraTrackableObject", typeof(CutsceneCameraTrackableObject));
			m_CameraTrackableObject = gameObject.GetComponent<CutsceneCameraTrackableObject>();
		}
		s_Instance = this;
		m_State = States.Idle;
	}

	protected virtual void OnDestroy()
	{
		StopAllCutscenes();
		if (s_Instance == this)
		{
			s_Instance = null;
		}
		if (m_SaveData != null)
		{
			m_SaveData.Dispose();
		}
		m_NetView = null;
	}

	public void StopAllCutscenes()
	{
		if (IsACutscenePlaying())
		{
			if (m_SelectedCutscene != null)
			{
				m_SelectedCutscene.SkipAll();
			}
			if (m_SelectedCutscene != null)
			{
				NotifyRespondersOfCutsceneEnd(m_SelectedCutscene);
			}
		}
	}

	public void RegisterWithCullingObject()
	{
	}

	protected void Start()
	{
		DisableCutsceneObjects();
		if ((bool)(s_Instance = this))
		{
			m_SaveData = new SaveDataRegister(this, m_NetView.viewID, bIsMajorManagerComponent: false);
		}
	}

	private void AddAllPrisons()
	{
		if (m_Intro != null)
		{
			m_AllCutscenes.Add(m_Intro);
		}
		if (m_GenericEscapeCutscene != null)
		{
			m_AllCutscenes.Add(m_GenericEscapeCutscene);
		}
		if (m_TimeServedCutscene != null)
		{
			m_AllCutscenes.Add(m_TimeServedCutscene);
		}
		if (m_EscapeSinglePlayerFirst != null)
		{
			m_AllCutscenes.Add(m_EscapeSinglePlayerFirst);
		}
		if (m_EscapeMultiplayerFirst != null)
		{
			m_AllCutscenes.Add(m_EscapeMultiplayerFirst);
		}
		if (m_TimesUpCutscene != null)
		{
			m_AllCutscenes.Add(m_TimesUpCutscene);
		}
		AddLevelSpecificScenesToList(m_AllCutscenes);
		for (int num = m_AllCutscenes.Count - 1; num >= 0; num--)
		{
			ObjectiveSceneElement[] componentsInChildren = m_AllCutscenes[num].GetComponentsInChildren<ObjectiveSceneElement>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].RegisterWithGlobalSceneReferences();
			}
		}
	}

	protected virtual void Update()
	{
		switch (m_State)
		{
		case States.WaitingForStart:
			UpdateWaitForStartState();
			break;
		case States.SkippingCurrent:
			UpdateSkipState();
			break;
		case States.Playing:
			UpdatePlayingState();
			break;
		}
	}

	private void UpdatePlayingState()
	{
		m_TimeUntilSkipDeadtimeFinishes -= UpdateManager.deltaTime;
		m_TimeUntilSkipButtonResets -= UpdateManager.deltaTime;
		if (m_TimeUntilSkipButtonResets <= 0f)
		{
			m_TimeUntilSkipDeadtimeFinishes = m_ButtonsPressedDeadtime;
		}
		if (!(m_SelectedCutscene.currentTime > m_NoSkipDuration))
		{
			return;
		}
		Gamer[] allGamers = Gamer.GetAllGamers();
		for (int i = 0; i < allGamers.Length; i++)
		{
			if (allGamers[i] == null || !allGamers[i].IsLocal())
			{
				continue;
			}
			if (allGamers[i].m_RewiredPlayer.GetAnyButtonUp())
			{
				m_OverarchingCanvas.ShowSkipText();
				m_TimeUntilSkipButtonResets = m_CutsceneSkipButtonResetTime;
			}
			if (allGamers[i].m_RewiredPlayer.GetButtonUp("Skip"))
			{
				if (m_TimeUntilSkipDeadtimeFinishes <= 0f)
				{
					SkipLocally();
				}
				break;
			}
		}
	}

	private void UpdateSkipState()
	{
		m_TimeUntilCutsceneSkipFinished -= UpdateManager.deltaTime;
		if (m_TimeUntilCutsceneSkipFinished <= 0f)
		{
			m_SelectedCutscene.SkipAll();
		}
	}

	private void UpdateWaitForStartState()
	{
		m_TimeUntilCutscenePlay -= UpdateManager.deltaTime;
		if (m_TimeUntilCutscenePlay <= 0f)
		{
			m_State = States.Playing;
			CameraManager.GetInstance().SwitchToCutsceneMode();
			HUDMenuFlow.Instance.HideAllHUDs();
			InGameMenuFlow.Instance.HideAllMenus();
			m_SelectedCutscene.gameObject.SetActive(value: true);
			NotifyRespondersOfCutsceneStart(m_SelectedCutscene);
			m_OverarchingCanvas.ResetEffects();
			m_SelectedCutscene.Play(0f, CutsceneFinishedCallback);
			m_TimeUntilSkipDeadtimeFinishes = m_ButtonsPressedDeadtime;
		}
	}

	protected virtual void CutsceneFinishedCallback()
	{
		NotifyRespondersOfCutsceneEnd(m_SelectedCutscene);
		if (m_ExitEffect != UIAnimatedEffectController.Effects.FadeToOpaque_Hold)
		{
			CameraManager.GetInstance().SwitchToGameMode();
			StartCoroutine(DelayedResetHUDEffect());
			StartCoroutine(DelayedFadeHudsToOpaque(m_FadeTime));
		}
		m_OverarchingCanvas.PlayEffect(m_ExitEffect, m_FadeTime);
		m_OverarchingCanvas.OnCutsceneFinished();
		SetLocalInputForCutsceneEnabled(state: false);
		m_SelectedCutscene.gameObject.SetActive(value: false);
		m_SelectedCutscene = null;
		m_State = States.Idle;
		LightingManager.GetInstance().ReleaseTimeOverride();
		RoutineManager.GetInstance().SetTimeFrozenRPC(bFrozen: false);
		m_ScriptedPlayerOverride = null;
		if (CutsceneManagerBase.CutsceneFinishedEvent != null)
		{
			CutsceneManagerBase.CutsceneFinishedEvent(m_FadeTime);
		}
	}

	private IEnumerator DelayedFadeHudsToOpaque(float delayTime)
	{
		yield return new WaitForSeconds(delayTime);
		CutsceneHUDManager.GetInstance().FadeAllHUDsToOpaque(m_FadeTime);
		if (PauseMenu.GetOpenPauseMenuInstance() == null)
		{
			HUDMenuFlow.Instance.ShowAllHUDs();
			InGameMenuFlow.Instance.ShowAllMenus();
		}
	}

	public void EffectHoldFinalBit()
	{
		CameraManager.GetInstance().SwitchToGameMode();
		StartCoroutine(DelayedResetHUDEffect());
		CutsceneHUDManager.GetInstance().FadeAllHUDsToOpaque(m_FadeTime);
		HUDMenuFlow.Instance.ShowAllHUDs();
		InGameMenuFlow.Instance.ShowAllMenus();
	}

	private IEnumerator DelayedResetHUDEffect()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		HUDMenuFlow.Instance.m_GlobalEfectController.ResetAllEffects();
	}

	public void MasterPlayMultiplayerCutsceneRPC(Cutscene scene, UIAnimatedEffectController.Effects entryEffect = UIAnimatedEffectController.Effects.FadeToOpaque, UIAnimatedEffectController.Effects exitEffect = UIAnimatedEffectController.Effects.FadeToTransparent, Player scriptedPlayerOverride = null)
	{
		if (scene == null)
		{
		}
		if (T17NetManager.IsMasterClient)
		{
			int cutsceneIndex = GetCutsceneIndex(scene);
			int hashCode = Guid.NewGuid().GetHashCode();
			int num = ((!(scriptedPlayerOverride == null)) ? scriptedPlayerOverride.m_NetView.viewID : (-1));
			m_NetView.RPC("RPC_PlayMultiplayerCutscene", NetTargets.All, cutsceneIndex, (int)entryEffect, (int)exitEffect, hashCode, -1f, num);
		}
	}

	[PunRPC]
	public void RPC_PlayMultiplayerCutscene(int sceneIndex, int entryEffect, int exitEffect, int newRandomSeed, float fadeTimeOverride, int scriptedPlayerOverrideId)
	{
		Cutscene cutsceneAtIndex = GetCutsceneAtIndex(sceneIndex);
		if (!(cutsceneAtIndex == null))
		{
			m_RandomGenerator = new System.Random(newRandomSeed);
			float? num = null;
			if (fadeTimeOverride >= 0f)
			{
				num = fadeTimeOverride;
			}
			Player player = null;
			if (scriptedPlayerOverrideId != -1)
			{
				player = T17NetView.Find<Player>(scriptedPlayerOverrideId);
			}
			Cutscene scene = cutsceneAtIndex;
			float? fadeTimeOverride2 = num;
			Player scriptedPlayerOverride = player;
			PlayCutsceneSetupRPC(scene, (UIAnimatedEffectController.Effects)entryEffect, (UIAnimatedEffectController.Effects)exitEffect, fadeTimeOverride2, null, scriptedPlayerOverride);
		}
	}

	public void PlayCutsceneSetupRPC(Cutscene scene, UIAnimatedEffectController.Effects entryEffect = UIAnimatedEffectController.Effects.FadeToOpaque, UIAnimatedEffectController.Effects exitEffect = UIAnimatedEffectController.Effects.FadeToTransparent, float? fadeTimeOverride = null, UIAnimatedEffectController.Effects? skipEffect = null, Player scriptedPlayerOverride = null)
	{
		if (!(scene == null) && m_State == States.Idle)
		{
			m_ScriptedPlayerOverride = scriptedPlayerOverride;
			float num = (fadeTimeOverride.HasValue ? fadeTimeOverride.Value : m_FadeTime);
			RoutineManager.GetInstance().SetTimeFrozenRPC(bFrozen: true);
			m_SelectedCutscene = scene;
			m_TimeUntilCutscenePlay = num;
			m_OverarchingCanvas.m_SkipTextDuration = m_CutsceneSkipButtonResetTime;
			CutsceneHUDManager.GetInstance().StartFadeAllHUDsToTransparent(num);
			m_EntryEffect = entryEffect;
			m_ExitEffect = exitEffect;
			m_SkipEffect = ((!skipEffect.HasValue) ? entryEffect : skipEffect.Value);
			HUDMenuFlow.Instance.PlayGlobalEffect(m_EntryEffect, num);
			m_State = States.WaitingForStart;
			SetLocalInputForCutsceneEnabled(state: true);
			if (CutsceneManagerBase.PrepareForCutsceneEvent != null)
			{
				CutsceneManagerBase.PrepareForCutsceneEvent(num);
			}
		}
	}

	public void SkipLocally()
	{
		if (m_State == States.Playing && m_SelectedCutscene != null)
		{
			if (!(m_SelectedCutscene.playTimeEnd - m_SelectedCutscene.currentTime < m_FadeTime))
			{
				m_OverarchingCanvas.PlayEffect(m_SkipEffect, m_FadeTime);
				m_State = States.SkippingCurrent;
				m_TimeUntilCutsceneSkipFinished = m_FadeTime;
			}
		}
		else if (m_State == States.SkippingCurrent)
		{
		}
	}

	public void PlayQuestCompleteSequenceForPlayer(Player player, float letterboxTime, float letterboxHoldTime, AnimatedEffectPingPong.StartingHoldHandler startingHoldCallback = null, AnimatedEffectPingPong.FinishedHoldHandler finishedHoldCallback = null)
	{
		CutsceneHUDManager.GetInstance().StartLetterBoxInOut(player.m_PlayerCameraManagerBindingID, letterboxTime, letterboxHoldTime, delegate
		{
			if (startingHoldCallback != null)
			{
				startingHoldCallback();
			}
		}, delegate
		{
			if (finishedHoldCallback != null)
			{
				finishedHoldCallback();
			}
		});
	}

	private void SetLocalInputForCutsceneEnabled(bool state)
	{
		Gamer[] localGamers = Gamer.GetLocalGamers();
		for (int i = 0; i < localGamers.Length; i++)
		{
			if (state)
			{
				SaveAndDisableGamerControllerStates(localGamers[i]);
				T17EventSystem.ApplyCategories(localGamers[i].m_RewiredPlayer, T17EventSystem.InputCateogryStates.Cutscenes);
			}
			else
			{
				RestoreGamerControllerStates(localGamers[i]);
			}
		}
	}

	private void SaveAndDisableGamerControllerStates(Gamer gamer)
	{
		int netViewID = gamer.m_NetViewID;
		PlayerMapsSnapshot value = PlayerMapsSnapshot.CreateSnapshotForGamer(gamer, disableAllMaps: true);
		if (!m_PreCutsceneMapSnapshots.ContainsKey(netViewID))
		{
			m_PreCutsceneMapSnapshots.Add(netViewID, value);
		}
		else
		{
			m_PreCutsceneMapSnapshots[netViewID] = value;
		}
	}

	private void RestoreGamerControllerStates(Gamer gamer)
	{
		int netViewID = gamer.m_NetViewID;
		if (m_PreCutsceneMapSnapshots.ContainsKey(netViewID))
		{
			m_PreCutsceneMapSnapshots[netViewID].RestoreControllerMaps();
		}
	}

	private void DisableCutsceneObjects()
	{
		if (m_AllCutscenes == null)
		{
			return;
		}
		for (int num = m_AllCutscenes.Count - 1; num >= 0; num--)
		{
			if (m_AllCutscenes[num] != null)
			{
				m_AllCutscenes[num].gameObject.SetActive(value: false);
			}
		}
	}

	private void NotifyRespondersOfCutsceneStart(Cutscene theScene)
	{
		ICutsceneStartEndResponder[] componentsInChildren = theScene.GetComponentsInChildren<ICutsceneStartEndResponder>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].CutsceneStarted();
		}
		List<ICutsceneActorResettable> resettingActorsInCutscene = GetResettingActorsInCutscene(theScene);
		for (int num = resettingActorsInCutscene.Count - 1; num >= 0; num--)
		{
			resettingActorsInCutscene[num].Cutscene_PrepareForUse();
		}
	}

	private void NotifyRespondersOfCutsceneEnd(Cutscene theScene)
	{
		ICutsceneStartEndResponder[] componentsInChildren = theScene.GetComponentsInChildren<ICutsceneStartEndResponder>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].CutsceneEnded();
		}
		List<ICutsceneActorResettable> resettingActorsInCutscene = GetResettingActorsInCutscene(theScene);
		for (int num = resettingActorsInCutscene.Count - 1; num >= 0; num--)
		{
			resettingActorsInCutscene[num].Cutscene_FinishedUse();
		}
	}

	private List<ICutsceneActorResettable> GetResettingActorsInCutscene(Cutscene theScene)
	{
		CutsceneGroup[] componentsInChildren = theScene.GetComponentsInChildren<CutsceneGroup>();
		List<ICutsceneActorResettable> list = new List<ICutsceneActorResettable>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].actor != null)
			{
				ICutsceneActorResettable component = componentsInChildren[i].actor.GetComponent<ICutsceneActorResettable>();
				if (component != null)
				{
					list.Add(component);
				}
			}
		}
		return list;
	}

	public Player GetOverridePlayer()
	{
		return m_ScriptedPlayerOverride;
	}

	public int GetRandomInt(int min, int max)
	{
		return m_RandomGenerator.Next(min, max);
	}

	public int GetCutsceneIndex(Cutscene cutscene)
	{
		return m_AllCutscenes.IndexOf(cutscene);
	}

	public Cutscene GetCutsceneAtIndex(int index)
	{
		if (index < 0 || index >= m_AllCutscenes.Count)
		{
			return null;
		}
		return m_AllCutscenes[index];
	}

	public static bool IsACutscenePlaying()
	{
		CutsceneManagerBase instance = GetInstance();
		if (instance == null)
		{
			return false;
		}
		return instance.m_State == States.Playing || instance.m_State == States.SkippingCurrent || instance.m_State == States.WaitingForStart;
	}

	public static int GetCutsceneCount()
	{
		CutsceneManagerBase instance = GetInstance();
		if (instance != null)
		{
			return instance.m_AllCutscenes.Count;
		}
		return 0;
	}

	public static States GetState()
	{
		CutsceneManagerBase instance = GetInstance();
		if (instance != null)
		{
			return instance.m_State;
		}
		return States.Unassigned;
	}

	public Cutscene GetCurrentPlayingCutscene()
	{
		return m_SelectedCutscene;
	}

	public bool ConsiderPlayingIntroCutscene(float? fadeTimeOverride = null, UIAnimatedEffectController.Effects entryEffect = UIAnimatedEffectController.Effects.FadeToOpaque, UIAnimatedEffectController.Effects? skipEffect = null, Player scriptedPlayerOverride = null)
	{
		if (m_bPreventIntroCutscene)
		{
			return false;
		}
		if (T17NetManager.NetOnlineMode)
		{
			m_bPreventIntroCutscene = true;
			return false;
		}
		if (!T17NetManager.IsMasterClient)
		{
			m_bPreventIntroCutscene = true;
			return false;
		}
		ConfigManager instance = ConfigManager.GetInstance();
		if (instance != null && (instance.gameType == PrisonConfig.ConfigType.Cooperative || instance.gameType == PrisonConfig.ConfigType.Singleplayer) && m_Intro != null && T17NetManager.IsMasterClient)
		{
			Debug.LogWarning("I want to play intro cutscene");
			m_bPreventIntroCutscene = true;
			PlayCutsceneSetupRPC(m_Intro, entryEffect, UIAnimatedEffectController.Effects.FadeToTransparent, fadeTimeOverride, skipEffect, scriptedPlayerOverride);
			return true;
		}
		string text = "CUTSCENE: Not playing intro cutscene. ";
		if (instance == null)
		{
			text += "COnfig manager was null. ";
		}
		if (instance.gameType != 0 && instance.gameType != PrisonConfig.ConfigType.Singleplayer)
		{
			text += "Game type was not cooperative / singleplayer. ";
		}
		if (m_Intro == null)
		{
			text += "Intro cutscene was null. ";
		}
		Debug.LogWarning(text);
		return false;
	}

	public string CreateSnapshot()
	{
		SaveData_CutsceneManager_V1 saveData_CutsceneManager_V = new SaveData_CutsceneManager_V1();
		saveData_CutsceneManager_V.P = m_bPreventIntroCutscene;
		return JsonUtility.ToJson(saveData_CutsceneManager_V);
	}

	public void StartedFromSnapshot()
	{
		if (string.IsNullOrEmpty(m_SaveData.GetSaveData()))
		{
			return;
		}
		m_bPreventIntroCutscene = true;
		PrisonSnapshotIO.SnapshotData_Base snapshotData_Base = null;
		try
		{
			snapshotData_Base = JsonUtility.FromJson<PrisonSnapshotIO.SnapshotData_Base>(m_SaveData.GetSaveData());
		}
		catch
		{
		}
		if (snapshotData_Base != null && snapshotData_Base.m_Version == 1)
		{
			SaveData_CutsceneManager_V1 saveData_CutsceneManager_V = null;
			try
			{
				saveData_CutsceneManager_V = JsonUtility.FromJson<SaveData_CutsceneManager_V1>(m_SaveData.GetSaveData());
			}
			catch
			{
			}
			if (saveData_CutsceneManager_V != null)
			{
				m_bPreventIntroCutscene = saveData_CutsceneManager_V.P;
			}
		}
	}

	public static string DebugCutsceneIndexSelect(int i, bool bReadOnly)
	{
		CutsceneManagerBase instance = GetInstance();
		if (instance != null)
		{
			if (instance.m_AllCutscenes.Count == 0)
			{
				return "NO CUTSCENES LINKED UP";
			}
			if (i >= instance.m_AllCutscenes.Count || i < 0)
			{
				return "Index " + i + " is out of bounds";
			}
			if (bReadOnly)
			{
				return instance.m_AllCutscenes[i].name;
			}
			instance.RPC_PlayMultiplayerCutscene(i, 1, 2, 1, -1f, -1);
			return string.Empty;
		}
		return "NO CUTSCENE MANAGER IN SCENE";
	}

	public static bool DebugToggleHuds(bool bEnable, bool bJustRead)
	{
		if (s_Instance != null)
		{
			if (!bJustRead)
			{
				m_bDebugAllHudsDisabled = !m_bDebugAllHudsDisabled;
				if (!bEnable)
				{
					CutsceneHUDManager.GetInstance().FadeAllHUDsToOpaque(0.1f);
				}
				else
				{
					CutsceneHUDManager.GetInstance().StartFadeAllHUDsToTransparent(0.1f);
				}
			}
			return m_bDebugAllHudsDisabled;
		}
		return false;
	}
}
