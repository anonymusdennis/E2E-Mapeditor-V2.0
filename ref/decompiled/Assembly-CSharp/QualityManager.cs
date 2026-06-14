using System;
using System.Collections.Generic;
using UnityEngine;

public class QualityManager : MonoBehaviour
{
	public delegate void QualityHandler();

	[Serializable]
	public class QualityLevel
	{
		public int m_MinimumFps;

		public string m_QualityName;

		public float m_ResolutionScale = 1f;

		public ShadowLevel m_ShadowLevel = ShadowLevel.High;

		public bool m_BlurEnabled = true;

		public bool m_FrontendVideoEnabled = true;
	}

	private static QualityManager s_Instance;

	public RenderingBenchmark m_benchmarkCamera;

	public List<QualityLevel> m_QualityPresets = new List<QualityLevel>();

	public Vector2 m_SmallestSupportedResolution = new Vector2(1024f, 768f);

	private const string HAS_RAN_BEFORE_KEY = "Quality:HasRunTestBefore";

	private const string BLUR_SAVE_KEY = "Settings:Blur";

	private const string SHADOW_QUALITY_KEY = "Settings:ShadowQuality";

	private const string PLAYER_PREFS_TOGGLE_VIDEO = "Settings:BackgroundVideoEnabled";

	public static event QualityHandler PreScreenChangeEvent;

	public static event QualityHandler PostScreenChangeEvent;

	public static QualityManager GetInstance()
	{
		return s_Instance;
	}

	private void Awake()
	{
		if (s_Instance == null)
		{
			s_Instance = this;
			UnityEngine.Object.DontDestroyOnLoad(this);
		}
		if (m_benchmarkCamera == null)
		{
			m_benchmarkCamera = GetComponentInChildren<RenderingBenchmark>(includeInactive: true);
		}
		if (m_benchmarkCamera != null)
		{
			m_benchmarkCamera.OnBenchmarkComplete += BenchmarkComplete;
		}
		m_QualityPresets.Sort((QualityLevel x, QualityLevel y) => y.m_MinimumFps.CompareTo(x.m_MinimumFps));
		if (m_SmallestSupportedResolution == Vector2.zero)
		{
			m_SmallestSupportedResolution = new Vector2(1024f, 768f);
		}
	}

	protected virtual void OnDestroy()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
		}
		if (m_benchmarkCamera != null)
		{
			m_benchmarkCamera.OnBenchmarkComplete -= BenchmarkComplete;
		}
	}

	public static void SetVsyncCount(int count)
	{
		if (QualityManager.PreScreenChangeEvent != null)
		{
			QualityManager.PreScreenChangeEvent();
		}
		QualitySettings.vSyncCount = count;
		RenderTargetManager.CheckForLostRTs();
		if (QualityManager.PostScreenChangeEvent != null)
		{
			QualityManager.PostScreenChangeEvent();
		}
	}

	public static void SetFullScreen(bool fullScreen)
	{
		if (QualityManager.PreScreenChangeEvent != null)
		{
			QualityManager.PreScreenChangeEvent();
		}
		Screen.fullScreen = fullScreen;
		RenderTargetManager.DelayedCheckForRt();
		if (QualityManager.PostScreenChangeEvent != null)
		{
			QualityManager.PostScreenChangeEvent();
		}
	}

	public static void SetResolution(int width, int height, bool fullScreen)
	{
		if (QualityManager.PreScreenChangeEvent != null)
		{
			QualityManager.PreScreenChangeEvent();
		}
		Screen.SetResolution(width, height, fullScreen);
		RenderTargetManager.DelayedCheckForRt();
		if (QualityManager.PostScreenChangeEvent != null)
		{
			QualityManager.PostScreenChangeEvent();
		}
	}

	public bool AutoDetectQualitySettings()
	{
		if (m_benchmarkCamera != null)
		{
			if (m_benchmarkCamera.IsDone())
			{
				return true;
			}
			if (m_benchmarkCamera.IsRunning())
			{
				return false;
			}
		}
		if (Platform.GetInstance().SupportsDifferentQualitySettings())
		{
			GlobalSave instance = GlobalSave.GetInstance();
			if (instance != null)
			{
				bool value = false;
				GlobalSave.GetInstance().Get("Quality:HasRunTestBefore", out value, def: false);
				if (!value && m_benchmarkCamera != null)
				{
					m_benchmarkCamera.StartBenchmark();
					return false;
				}
			}
		}
		return true;
	}

	private void BenchmarkComplete(float benchmarkTime, int benchmarkFps)
	{
		Debug.Log("BENCHMARK: " + benchmarkTime + "s - " + benchmarkFps + "fps.");
		QualityLevel qualityLevelForBenchmarkFPS = GetQualityLevelForBenchmarkFPS(benchmarkFps);
		SetNewQualityLevel(qualityLevelForBenchmarkFPS);
	}

	private void SetNewQualityLevel(QualityLevel newQualityLevel)
	{
		if (newQualityLevel == null)
		{
			return;
		}
		float num = Display.main.systemWidth;
		float num2 = Display.main.systemHeight;
		num *= newQualityLevel.m_ResolutionScale;
		num2 *= newQualityLevel.m_ResolutionScale;
		FastList<Resolution> supportedResolutionList = ResolutionSelector.GetSupportedResolutionList(m_SmallestSupportedResolution, GlobalStart.GetInstance().m_SupportedAspectRatios);
		if (supportedResolutionList.Count > 0)
		{
			Resolution resolution = supportedResolutionList[0];
			for (int num3 = supportedResolutionList.Count - 1; num3 >= 0; num3--)
			{
				Resolution resolution2 = supportedResolutionList[num3];
				if ((float)resolution2.width <= num && (float)resolution2.height <= num2 && resolution2.width > resolution.width && resolution2.height > resolution.height)
				{
					resolution = resolution2;
				}
			}
			bool fullScreen = Screen.fullScreen;
			SetResolution(resolution.width, resolution.height, fullScreen);
		}
		if (CameraManager.GetInstance() != null)
		{
			CameraManager.GetInstance().SetBlurEffectEnabled(newQualityLevel.m_BlurEnabled);
		}
		if (newQualityLevel.m_ShadowLevel == ShadowLevel.Off)
		{
			QualitySettings.shadows = ShadowQuality.Disable;
		}
		else
		{
			QualitySettings.shadowResolution = ShadowDetailOptionItem.MapShadowLevelToShadowResolution(newQualityLevel.m_ShadowLevel);
			QualitySettings.shadows = ShadowQuality.All;
		}
		if (FrontEndFlow.Instance != null)
		{
			FrontEndFlow.Instance.ToggleBackgroundVideo(newQualityLevel.m_FrontendVideoEnabled);
		}
		PlayerPrefs.SetInt("Settings:BackgroundVideoEnabled", newQualityLevel.m_FrontendVideoEnabled ? 1 : 0);
		GlobalSave instance = GlobalSave.GetInstance();
		instance.Set("Settings:Blur", (!newQualityLevel.m_BlurEnabled) ? 0f : 1f);
		instance.Set("Settings:ShadowQuality", (int)newQualityLevel.m_ShadowLevel);
		instance.Set("Quality:HasRunTestBefore", value: true);
		instance.RequestSave();
	}

	private QualityLevel GetQualityLevelForBenchmarkFPS(int benchmarkFps)
	{
		int count = m_QualityPresets.Count;
		for (int i = 0; i < count; i++)
		{
			QualityLevel qualityLevel = m_QualityPresets[i];
			if (qualityLevel != null && benchmarkFps >= qualityLevel.m_MinimumFps)
			{
				return qualityLevel;
			}
		}
		return null;
	}
}
