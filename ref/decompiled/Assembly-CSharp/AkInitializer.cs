using System.IO;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

[RequireComponent(typeof(AkTerminator))]
[AddComponentMenu("Wwise/AkInitializer")]
public class AkInitializer : MonoBehaviour
{
	public static readonly string c_DefaultBasePath = Path.Combine("Audio", "GeneratedSoundBanks");

	public string basePath = c_DefaultBasePath;

	public const string c_Language = "English(US)";

	public string language = "English(US)";

	public const int c_DefaultPoolSize = 4096;

	public int defaultPoolSize = 4096;

	public const int c_LowerPoolSize = 2048;

	public int lowerPoolSize = 2048;

	public const int c_StreamingPoolSize = 1024;

	public int streamingPoolSize = 1024;

	public const int c_PreparePoolSize = 0;

	public int preparePoolSize;

	public const float c_MemoryCutoffThreshold = 0.95f;

	public float memoryCutoffThreshold = 0.95f;

	public const bool c_EngineLogging = true;

	public bool engineLogging = true;

	private static AkInitializer ms_Instance;

	public static string GetBasePath()
	{
		return ms_Instance.basePath;
	}

	public static string GetDecodedBankFolder()
	{
		return "DecodedBanks";
	}

	public static string GetDecodedBankFullPath()
	{
		return Path.Combine(AkBasePathGetter.GetPlatformBasePath(), GetDecodedBankFolder());
	}

	public static string GetCurrentLanguage()
	{
		return ms_Instance.language;
	}

	private void Awake()
	{
		Initialize();
	}

	public void Initialize()
	{
		if (ms_Instance != null)
		{
			if (ms_Instance != this)
			{
				Object.DestroyImmediate(base.gameObject);
			}
			return;
		}
		Debug.Log("WwiseUnity: Initialize sound engine ...");
		AkMemSettings akMemSettings = new AkMemSettings();
		akMemSettings.uMaxNumPools = 20u;
		AkDeviceSettings akDeviceSettings = new AkDeviceSettings();
		AkSoundEngine.GetDefaultDeviceSettings(akDeviceSettings);
		AkStreamMgrSettings akStreamMgrSettings = new AkStreamMgrSettings();
		akStreamMgrSettings.uMemorySize = (uint)(streamingPoolSize * 1024);
		AkInitSettings akInitSettings = new AkInitSettings();
		AkSoundEngine.GetDefaultInitSettings(akInitSettings);
		akInitSettings.uDefaultPoolSize = (uint)(defaultPoolSize * 1024);
		akInitSettings.szPluginDLLPath = Path.Combine(Application.dataPath, "Plugins" + Path.DirectorySeparatorChar);
		AkPlatformInitSettings akPlatformInitSettings = new AkPlatformInitSettings();
		AkSoundEngine.GetDefaultPlatformInitSettings(akPlatformInitSettings);
		akPlatformInitSettings.uLEngineDefaultPoolSize = (uint)(lowerPoolSize * 1024);
		akPlatformInitSettings.fLEngineDefaultPoolRatioThreshold = memoryCutoffThreshold;
		AkMusicSettings akMusicSettings = new AkMusicSettings();
		AkSoundEngine.GetDefaultMusicSettings(akMusicSettings);
		AkSoundEngine.SetGameName(Application.productName);
		AKRESULT aKRESULT = AkSoundEngine.Init(akMemSettings, akStreamMgrSettings, akDeviceSettings, akInitSettings, akPlatformInitSettings, akMusicSettings, (uint)(preparePoolSize * 1024));
		if (aKRESULT != AKRESULT.AK_Success)
		{
			Debug.LogError("WwiseUnity: Failed to initialize the sound engine. Abort.");
			return;
		}
		ms_Instance = this;
		string validBasePath = AkBasePathGetter.GetValidBasePath();
		if (string.IsNullOrEmpty(validBasePath))
		{
			return;
		}
		aKRESULT = AkSoundEngine.SetBasePath(validBasePath);
		if (aKRESULT != AKRESULT.AK_Success)
		{
			return;
		}
		AkSoundEngine.SetDecodedBankPath(GetDecodedBankFullPath());
		AkSoundEngine.SetCurrentLanguage(language);
		aKRESULT = AkCallbackManager.Init();
		if (aKRESULT != AKRESULT.AK_Success)
		{
			Debug.LogError("WwiseUnity: Failed to initialize Callback Manager. Terminate sound engine.");
			AkSoundEngine.Term();
			ms_Instance = null;
			return;
		}
		AkBankManager.Reset();
		Debug.Log("WwiseUnity: Sound engine initialized.");
		Object.DontDestroyOnLoad(this);
		aKRESULT = AkSoundEngine.LoadBank("Init.bnk", -1, out var _);
		if (aKRESULT != AKRESULT.AK_Success)
		{
			Debug.LogError("WwiseUnity: Failed load Init.bnk with result: " + aKRESULT);
		}
	}

	private void OnDestroy()
	{
		if (ms_Instance == this)
		{
			AkCallbackManager.SetMonitoringCallback((ErrorLevel)0, null);
			ms_Instance = null;
		}
	}

	private void OnEnable()
	{
		if (ms_Instance == null && AkSoundEngine.IsInitialized())
		{
			ms_Instance = this;
		}
	}

	private void LateUpdate()
	{
		if (ms_Instance != null)
		{
			AkCallbackManager.PostCallbacks();
			AkBankManager.DoUnloadBanks();
			AkSoundEngine.RenderAudio();
		}
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		if (ms_Instance != null)
		{
			if (!pauseStatus)
			{
				AudioController.SetState(State_Group.Master_Volume, Master_Volume.Volume_Up.ToString());
			}
			else
			{
				AudioController.SetState(State_Group.Master_Volume, Master_Volume.Volume_Down.ToString());
			}
			AkSoundEngine.RenderAudio();
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		if (ms_Instance != null)
		{
			if (focus)
			{
				AudioController.SetState(State_Group.Master_Volume, Master_Volume.Volume_Up.ToString());
			}
			else
			{
				AudioController.SetState(State_Group.Master_Volume, Master_Volume.Volume_Down.ToString());
			}
			AkSoundEngine.RenderAudio();
		}
	}
}
