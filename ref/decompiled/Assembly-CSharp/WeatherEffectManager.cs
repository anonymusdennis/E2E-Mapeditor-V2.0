using Rewired;
using UnityEngine;

public class WeatherEffectManager : MonoBehaviour
{
	private static WeatherEffectManager m_Instance;

	private static int kMaxFullscreenWeatherEffects = 5;

	public WeatherEffectData[] m_TiledFullScreenWeatherEffects = new WeatherEffectData[MaxFullscreenWeatherEffects];

	public Camera[] m_PlayerCameras = new Camera[4];

	public GameObject m_WeatherRendererPrefab;

	private GameObject[] m_WeatherRenders = new GameObject[4];

	public static WeatherEffectManager Instance => m_Instance;

	public static int MaxFullscreenWeatherEffects => kMaxFullscreenWeatherEffects;

	public void Awake()
	{
		if (m_Instance != null && m_Instance != this)
		{
			m_Instance.OnDestroy();
		}
		m_Instance = this;
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance.CleanUpWeatherAssets();
			m_Instance = null;
		}
	}

	public static void Enable()
	{
		if (m_Instance != null)
		{
			m_Instance.gameObject.SetActive(value: true);
			m_Instance.Start();
		}
	}

	public static void Disable()
	{
		if (m_Instance != null)
		{
			m_Instance.CleanUpWeatherAssets();
			m_Instance.gameObject.SetActive(value: false);
		}
	}

	private void CleanUpWeatherAssets()
	{
		for (int num = m_WeatherRenders.Length - 1; num >= 0; num--)
		{
			if (m_WeatherRenders[num] != null)
			{
				Object.Destroy(m_WeatherRenders[num]);
			}
		}
		for (int num2 = m_TiledFullScreenWeatherEffects.Length - 1; num2 >= 0; num2--)
		{
			WeatherEffectData weatherEffectData = m_TiledFullScreenWeatherEffects[num2];
			if (weatherEffectData != null && weatherEffectData.m_bAudioEffectActive)
			{
				if (!string.IsNullOrEmpty(weatherEffectData.m_AudioEffectOff))
				{
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, weatherEffectData.m_AudioEffectOff, AudioController.InGameMusicAndAmbienceObject);
				}
				weatherEffectData.m_bAudioEffectActive = false;
			}
		}
	}

	public void Start()
	{
		for (int num = 3; num >= 0; num--)
		{
			if (m_PlayerCameras[num] != null)
			{
				m_WeatherRenders[num] = Object.Instantiate(m_WeatherRendererPrefab);
				WeatherObjectRenderer component = m_WeatherRenders[num].GetComponent<WeatherObjectRenderer>();
				if (component == null)
				{
					Object.Destroy(m_WeatherRenders[num]);
				}
				else
				{
					component.m_ParentCam = m_PlayerCameras[num];
					m_WeatherRenders[num].SetActive(value: true);
				}
			}
		}
		for (int num2 = MaxFullscreenWeatherEffects - 1; num2 >= 0; num2--)
		{
			WeatherEffectData effectData = GetEffectData(num2);
			if (!(effectData == null) && effectData.m_AudioEffectTriggerMode == WeatherEffectData.AudioEffectTriggerMode.OnStart && !effectData.m_bAudioEffectActive)
			{
				if (!string.IsNullOrEmpty(effectData.m_AudioEffectOn))
				{
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, effectData.m_AudioEffectOn, AudioController.InGameMusicAndAmbienceObject);
				}
				effectData.m_bAudioEffectActive = true;
			}
		}
	}

	public void Update()
	{
		if (Time.timeScale == 0f)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		for (int num = MaxFullscreenWeatherEffects - 1; num >= 0; num--)
		{
			WeatherEffectData effectData = GetEffectData(num);
			if (!(effectData == null))
			{
				if (effectData.m_AnimatedTextureMode && effectData.m_AnimationTextures != null && effectData.m_AnimationTextures.Count > 0)
				{
					if (Mathf.Repeat(effectData.m_CurrentEffectTime, effectData.m_AnimationInterval) + deltaTime > effectData.m_AnimationInterval)
					{
						effectData.m_CurrentTextureIndex++;
						if (effectData.m_CurrentTextureIndex >= effectData.m_AnimationTextures.Count)
						{
							effectData.m_CurrentTextureIndex = 0;
						}
					}
					effectData.m_Texture = effectData.m_AnimationTextures[effectData.m_CurrentTextureIndex];
				}
				effectData.m_CurrentEffectTime += deltaTime;
				if (effectData.m_XScrollCurve.length > 0)
				{
					effectData.m_PreviousXOffset = effectData.m_CurrentXOffset;
					effectData.m_CurrentXOffset += effectData.m_XScrollCurve.Evaluate(effectData.m_CurrentEffectTime) * deltaTime;
					effectData.m_DeltaXOffset = effectData.m_CurrentXOffset - effectData.m_PreviousXOffset;
				}
				if (effectData.m_YScrollCurve.length > 0)
				{
					effectData.m_PreviousYOffset = effectData.m_CurrentYOffset;
					effectData.m_CurrentYOffset += effectData.m_YScrollCurve.Evaluate(effectData.m_CurrentEffectTime) * deltaTime;
					effectData.m_DeltaYOffset = effectData.m_CurrentYOffset - effectData.m_PreviousYOffset;
				}
				if (effectData.m_AudioEffectTriggerMode == WeatherEffectData.AudioEffectTriggerMode.OnAlphaThreshold)
				{
					if (effectData.m_EffectAlphaCurve.Evaluate(effectData.m_CurrentEffectTime) > effectData.m_AlphaThreshold && !effectData.m_bAudioEffectActive)
					{
						if (!string.IsNullOrEmpty(effectData.m_AudioEffectOn))
						{
							AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, effectData.m_AudioEffectOn, AudioController.InGameMusicAndAmbienceObject);
						}
						effectData.m_bAudioEffectActive = true;
					}
					else if (effectData.m_EffectAlphaCurve.Evaluate(effectData.m_CurrentEffectTime) < effectData.m_AlphaThreshold && effectData.m_bAudioEffectActive)
					{
						if (!string.IsNullOrEmpty(effectData.m_AudioEffectOff))
						{
							AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, effectData.m_AudioEffectOff, AudioController.InGameMusicAndAmbienceObject);
						}
						effectData.m_bAudioEffectActive = false;
					}
				}
				bool flag = true;
				if (SolitaryManager.GetInstance() != null && SolitaryManager.GetInstance().IsLockdownActive())
				{
					flag = false;
				}
				if (flag && (effectData.m_bRumbleEnabled || effectData.m_bLightbarEnabled))
				{
					if (effectData.m_EffectAlphaCurve.Evaluate(effectData.m_CurrentEffectTime) > effectData.m_AlphaThreshold)
					{
						effectData.m_bControllerEffectsPlayed = true;
						ReInput.PlayerHelper players = ReInput.players;
						int i = 0;
						for (int playerCount = players.playerCount; i < playerCount; i++)
						{
							Rewired.Player rewiredPlayer = players.GetPlayer(i);
							if (rewiredPlayer == null)
							{
								continue;
							}
							Gamer[] allGamers = Gamer.GetAllGamers();
							if (allGamers == null || allGamers.Length <= 0)
							{
								continue;
							}
							int num2 = allGamers.FindIndex((Gamer x) => x != null && x.m_RewiredPlayer == rewiredPlayer);
							if (num2 == -1)
							{
								continue;
							}
							Gamer gamer = allGamers[num2];
							if (gamer != null && gamer.m_PlayerObject != null && gamer.IsLocal() && gamer.m_eCharacterSelectionStage == Gamer.CharacterSelectionStage.EnabledInGame)
							{
								if (effectData.m_bRumbleEnabled)
								{
									Platform.GetInstance().DoControllerRumble(effectData.m_RumbleSettings, i);
								}
								if (effectData.m_bLightbarEnabled)
								{
									Platform.GetInstance().StartLightBarEffect(effectData.m_LightbarSettings, i);
								}
							}
						}
					}
					else if (effectData.m_bControllerEffectsPlayed)
					{
						effectData.m_bControllerEffectsPlayed = false;
						ReInput.PlayerHelper players2 = ReInput.players;
						int j = 0;
						for (int playerCount2 = players2.playerCount; j < playerCount2; j++)
						{
							if (players2.GetPlayer(j) != null && effectData.m_bLightbarEnabled)
							{
								Platform.GetInstance().StopLightBarEffect(j);
							}
						}
					}
				}
			}
		}
	}

	public WeatherEffectData GetEffectData(int index)
	{
		if (index >= 0 && index < kMaxFullscreenWeatherEffects && m_TiledFullScreenWeatherEffects[index] != null && m_TiledFullScreenWeatherEffects[index].m_EffectEnabled)
		{
			return m_TiledFullScreenWeatherEffects[index];
		}
		return null;
	}
}
