using System;
using System.Collections.Generic;
using UnityEngine;

public class LightingManager : T17MonoBehaviour, IControlledUpdate, ISerializationCallbackReceiver
{
	[Serializable]
	public class LightGroup
	{
		[Serializable]
		public class TimeOnOff
		{
			public int m_StartHour = 7;

			public int m_StartMinutes = 30;

			public int m_EndHour = 10;

			public int m_EndMinutes = 30;

			private int m_StartInMinutes = -1;

			private int m_EndInMinutes = -1;

			public bool m_bAlwaysOn;

			public int StartInMinutes
			{
				get
				{
					if (m_StartInMinutes == -1)
					{
						m_StartInMinutes = m_StartHour * 60 + m_StartMinutes;
					}
					return m_StartInMinutes;
				}
			}

			public int EndInMinutes
			{
				get
				{
					if (m_EndInMinutes == -1)
					{
						m_EndInMinutes = m_EndHour * 60 + m_EndMinutes;
					}
					return m_EndInMinutes;
				}
			}
		}

		[Serializable]
		public class SerializedLightEffect
		{
			public int effectType = -1;

			public string data = string.Empty;
		}

		public string m_Name = "LightGroup";

		public List<SerializedLightEffect> m_SerializedEffects = new List<SerializedLightEffect>();

		public int m_ID = -1;

		public List<LightControl> m_Lights = new List<LightControl>();

		public List<LightEffect> m_Effects = new List<LightEffect>();

		public List<TimeOnOff> m_Times = new List<TimeOnOff>();

		public float m_ChangedMinutes;

		private bool bTurnedOn = true;

		public bool IsActive => bTurnedOn;

		public void Init()
		{
			bTurnedOn = true;
			TurnOffAllLights(0f);
			for (int i = 0; i < m_Effects.Count; i++)
			{
				m_Effects[i].Init(this);
			}
		}

		public void UpdateLights(float deltaTime)
		{
			for (int num = m_Lights.Count - 1; num >= 0; num--)
			{
				if (m_Lights[num] != null)
				{
					m_Lights[num].UpdateLight(deltaTime);
				}
				else
				{
					m_Lights.RemoveAt(num);
				}
			}
			for (int i = 0; i < m_Effects.Count; i++)
			{
				m_Effects[i].OnGroupUpdated(deltaTime);
			}
		}

		public void UpdateEffects(float deltaTime)
		{
			for (int i = 0; i < m_Effects.Count; i++)
			{
				if (m_Effects[i].IsActive)
				{
					m_Effects[i].UpdateEffect(deltaTime);
				}
			}
		}

		public void TurnOnAllLights(float changeMinutes)
		{
			if (bTurnedOn)
			{
				return;
			}
			m_ChangedMinutes = changeMinutes;
			for (int num = m_Lights.Count - 1; num >= 0; num--)
			{
				if (m_Lights[num] != null)
				{
					m_Lights[num].TurnOn();
				}
				else
				{
					m_Lights.RemoveAt(num);
				}
			}
			for (int i = 0; i < m_Effects.Count; i++)
			{
				m_Effects[i].OnGroupTurnedOn();
			}
			bTurnedOn = true;
		}

		public void TurnOffAllLights(float changeMinutes)
		{
			if (!bTurnedOn)
			{
				return;
			}
			m_ChangedMinutes = changeMinutes;
			for (int num = m_Lights.Count - 1; num >= 0; num--)
			{
				if (m_Lights[num] != null)
				{
					m_Lights[num].TurnOff();
				}
				else
				{
					m_Lights.RemoveAt(num);
				}
			}
			for (int i = 0; i < m_Effects.Count; i++)
			{
				m_Effects[i].OnGroupTurnedOff();
			}
			bTurnedOn = false;
		}

		public GameObject[] GetLightsAsGameObjects()
		{
			List<GameObject> list = new List<GameObject>();
			for (int i = 0; i < m_Lights.Count; i++)
			{
				list.Add(m_Lights[i].gameObject);
			}
			return list.ToArray();
		}
	}

	[Serializable]
	public struct LightingPeriod
	{
		public Color m_IndoorAmbientColor;

		public Color m_OutdoorAmbientColor;

		public float m_IndoorIntensity;

		public float m_OutdoorIntensity;

		public float m_IndoorAmbientIntensityDepthFactor;

		public float m_OutdoorAmbientIntensityDepthFactor;

		public float m_LightAngle;

		public float m_LightHeight;

		public Color m_DirectionalLightColour;

		public float m_DirectionalLightIntensity;

		public Color m_DirectionalShadowColour;

		public float m_DirectionalShadowIntensity;

		public Color m_FoggingColor;

		public float m_FogDensity;

		public float m_FogStartDistance;
	}

	public delegate void LightingUpdated();

	public delegate void TimeOverridden(bool overridden, int overrideHours, int overrideMins);

	public delegate void LightingPreCalc(ref Color[] ambientColours, ref float[] ambientIntensitie);

	public List<LightGroup> m_LightGroups = new List<LightGroup>();

	public Light m_DirectionalLight;

	public LightingPeriod m_DawnLight;

	public LightingPeriod m_DayLight;

	public LightingPeriod m_DuskLight;

	public LightingPeriod m_NightLight;

	public LightingPeriod m_UnderGroundLight;

	public LightingPeriod m_VentsLight;

	private LightingPeriod m_CurrentLight;

	private Vector3 m_CurrentLightDirection;

	private Color[] m_PreCalcAmbientColours;

	private float[] m_PreCalcAmbientIntensities;

	private RoutineManager m_RoutineMan;

	private float m_SunriseStartMins;

	private float m_SunriseEndMins;

	private float m_MiddayMins;

	private float m_SunsetStartMins;

	private float m_SunsetEndMins;

	private float m_SpotlightsStartMins;

	private float m_SpotlightsEndMins;

	private float m_CurrentMins;

	private bool m_TimeWasFrozen = true;

	public int m_CurrentID = 1;

	public LightingUpdated OnLightingUpdated;

	public TimeOverridden OnTimeOverridden;

	public LightingPreCalc OnLightingPreCalc;

	private GuardTowerManager m_GuardTowerManager;

	private bool m_bOverrideTime;

	private int m_OverrideHour;

	private int m_OverrideMin;

	[ReadOnly]
	public int m_CurrentLightID = 1;

	private static LightingManager m_Instance;

	public static LightingManager GetInstance()
	{
		return m_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		m_Instance = this;
		m_CurrentLightDirection = new Vector3(0f, 0f, 0.3f);
	}

	public void Init()
	{
		m_GuardTowerManager = GuardTowerManager.GetInstance();
		UpdateManager.GetInstance().Register(this, UpdateCategory.RapidPeriodic);
		m_CurrentLight = m_DayLight;
		m_RoutineMan = RoutineManager.GetInstance();
		if (m_RoutineMan.m_RoutinesData != null)
		{
			m_SunriseStartMins = m_RoutineMan.m_RoutinesData.GetSunriseStartInMins();
			m_SunriseEndMins = m_RoutineMan.m_RoutinesData.GetSunriseEndInMins();
			m_SunsetStartMins = m_RoutineMan.m_RoutinesData.GetSunsetStartInMins();
			m_SunsetEndMins = m_RoutineMan.m_RoutinesData.GetSunsetEndInMins();
			m_SpotlightsStartMins = m_RoutineMan.m_RoutinesData.GetSpotlightsStartInMins();
			m_SpotlightsEndMins = m_RoutineMan.m_RoutinesData.GetSpotlightsEndInMins();
		}
		else
		{
			m_SunriseStartMins = 420f;
			m_SunriseEndMins = 480f;
			m_SunsetStartMins = 1080f;
			m_SunsetEndMins = 1140f;
			m_SpotlightsStartMins = 1080f;
			m_SpotlightsEndMins = 420f;
		}
		m_MiddayMins = m_SunriseEndMins + (m_SunsetStartMins - m_SunriseEndMins) / 2f;
		for (int i = 0; i < m_LightGroups.Count; i++)
		{
			m_LightGroups[i].Init();
		}
		if (m_DirectionalLight == null)
		{
			GameObject gameObject = GameObject.Find("Directional light");
			if (gameObject != null)
			{
				m_DirectionalLight = gameObject.GetComponent<Light>();
			}
			if (m_DirectionalLight != null)
			{
				m_DirectionalLight.enabled = true;
			}
		}
		m_PreCalcAmbientColours = new Color[2] { m_CurrentLight.m_OutdoorAmbientColor, m_CurrentLight.m_IndoorAmbientColor };
		m_PreCalcAmbientIntensities = new float[2] { 1f, 1f };
	}

	protected virtual void OnDestroy()
	{
		if (null != UpdateManager.GetInstance())
		{
			UpdateManager.GetInstance().Unregister(this, UpdateCategory.RapidPeriodic);
		}
		m_GuardTowerManager = null;
		m_RoutineMan = null;
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	private bool CalcLightingPeriod(float mins, out LightingPeriod timeOfDayStart, out LightingPeriod timeOfDayEnd, out float lerpVal)
	{
		if (mins > m_SunsetEndMins || mins < m_SunriseStartMins)
		{
			timeOfDayStart = m_NightLight;
			timeOfDayEnd = m_NightLight;
			lerpVal = 1f;
		}
		else if (mins > m_SunsetStartMins)
		{
			timeOfDayStart = m_DuskLight;
			timeOfDayEnd = m_NightLight;
			lerpVal = (mins - m_SunsetStartMins) / (m_SunsetEndMins - m_SunsetStartMins);
		}
		else if (mins > m_MiddayMins)
		{
			timeOfDayStart = m_DayLight;
			timeOfDayEnd = m_DuskLight;
			lerpVal = (mins - m_MiddayMins) / (m_SunsetStartMins - m_MiddayMins);
		}
		else if (mins > m_SunriseEndMins)
		{
			timeOfDayStart = m_DawnLight;
			timeOfDayEnd = m_DayLight;
			lerpVal = (mins - m_SunriseEndMins) / (m_MiddayMins - m_SunriseEndMins);
		}
		else
		{
			if (!(mins > m_SunriseStartMins))
			{
				timeOfDayStart = m_DayLight;
				timeOfDayEnd = m_DayLight;
				lerpVal = 1f;
				return false;
			}
			timeOfDayStart = m_NightLight;
			timeOfDayEnd = m_DawnLight;
			lerpVal = (mins - m_SunriseStartMins) / (m_SunriseEndMins - m_SunriseStartMins);
		}
		return true;
	}

	public void ControlledUpdate()
	{
		float currentMins = m_CurrentMins;
		if (m_bOverrideTime)
		{
			m_CurrentMins = m_OverrideHour * 60 + m_OverrideMin;
			if (OnLightingUpdated != null)
			{
				OnLightingUpdated();
			}
		}
		else
		{
			m_CurrentMins = m_RoutineMan.GetCurrentMins();
		}
		if (m_CurrentMins != currentMins || (m_TimeWasFrozen != m_RoutineMan.IsTimeFrozen() && m_TimeWasFrozen))
		{
			if (CalcLightingPeriod(m_CurrentMins, out var timeOfDayStart, out var timeOfDayEnd, out var lerpVal))
			{
				TransitionLight(ref timeOfDayStart, ref timeOfDayEnd, lerpVal);
			}
			if (m_GuardTowerManager != null)
			{
				bool spotlightsActive_Timed = false;
				if (m_SpotlightsStartMins > m_SpotlightsEndMins)
				{
					if (m_CurrentMins > m_SpotlightsStartMins || m_CurrentMins < m_SpotlightsEndMins)
					{
						spotlightsActive_Timed = true;
					}
				}
				else if (m_CurrentMins > m_SpotlightsStartMins && m_CurrentMins < m_SpotlightsEndMins)
				{
					spotlightsActive_Timed = true;
				}
				m_GuardTowerManager.SetSpotlightsActive_Timed(spotlightsActive_Timed);
			}
			for (int i = 0; i < m_LightGroups.Count; i++)
			{
				if (m_LightGroups[i].m_Times.Count > 0)
				{
					for (int j = 0; j < m_LightGroups[i].m_Times.Count; j++)
					{
						if (!PrisonPowerManager.GetInstance().PowerIsActive())
						{
							m_LightGroups[i].TurnOffAllLights(m_CurrentMins);
						}
						else if (m_LightGroups[i].m_Times[j].m_bAlwaysOn)
						{
							m_LightGroups[i].TurnOnAllLights(m_CurrentMins);
						}
						else if (m_LightGroups[i].m_Times[j].EndInMinutes < m_LightGroups[i].m_Times[j].StartInMinutes)
						{
							if ((int)m_CurrentMins >= m_LightGroups[i].m_Times[j].StartInMinutes || (int)m_CurrentMins <= m_LightGroups[i].m_Times[j].EndInMinutes)
							{
								m_LightGroups[i].TurnOnAllLights(m_CurrentMins);
							}
							else
							{
								m_LightGroups[i].TurnOffAllLights(m_CurrentMins);
							}
						}
						else if ((int)m_CurrentMins >= m_LightGroups[i].m_Times[j].StartInMinutes && (int)m_CurrentMins <= m_LightGroups[i].m_Times[j].EndInMinutes)
						{
							m_LightGroups[i].TurnOnAllLights(m_CurrentMins);
						}
						else
						{
							m_LightGroups[i].TurnOffAllLights(m_CurrentMins);
						}
					}
				}
				else
				{
					m_LightGroups[i].TurnOnAllLights(m_CurrentMins);
				}
				float deltaTime = (m_CurrentMins - m_LightGroups[i].m_ChangedMinutes) / RoutineManager.GetInstance().m_RealLifeSecondPerGameMinute;
				m_LightGroups[i].UpdateLights(deltaTime);
			}
			if (CalcLightingPeriod(m_CurrentMins + 1f, out var timeOfDayStart2, out var timeOfDayEnd2, out var lerpVal2))
			{
				ref Color reference = ref m_PreCalcAmbientColours[0];
				reference = Color.Lerp(timeOfDayStart2.m_OutdoorAmbientColor, timeOfDayEnd2.m_OutdoorAmbientColor, lerpVal2);
				ref Color reference2 = ref m_PreCalcAmbientColours[1];
				reference2 = Color.Lerp(timeOfDayStart2.m_IndoorAmbientColor, timeOfDayEnd2.m_IndoorAmbientColor, lerpVal2);
				m_PreCalcAmbientIntensities[0] = Mathf.Lerp(timeOfDayStart2.m_OutdoorIntensity, timeOfDayEnd2.m_OutdoorIntensity, lerpVal2);
				m_PreCalcAmbientIntensities[1] = Mathf.Lerp(timeOfDayStart2.m_IndoorIntensity, timeOfDayEnd2.m_IndoorIntensity, lerpVal2);
			}
		}
		else if (OnLightingPreCalc != null)
		{
			OnLightingPreCalc(ref m_PreCalcAmbientColours, ref m_PreCalcAmbientIntensities);
		}
		m_TimeWasFrozen = m_RoutineMan.IsTimeFrozen();
		float deltaTime2 = UpdateManager.deltaTime;
		for (int k = 0; k < m_LightGroups.Count; k++)
		{
			if (m_LightGroups[k].IsActive)
			{
				m_LightGroups[k].UpdateEffects(deltaTime2);
			}
		}
	}

	public void ControlledFixedUpdate()
	{
	}

	public string[] GetGroupStringList()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < m_LightGroups.Count; i++)
		{
			list.Add(m_LightGroups[i].m_Name);
		}
		return list.ToArray();
	}

	public void NewLightGroup()
	{
		LightGroup lightGroup = new LightGroup();
		lightGroup.m_ID = m_CurrentID;
		lightGroup.m_Name = "LightGroup_" + m_CurrentID;
		m_LightGroups.Add(lightGroup);
		m_CurrentID++;
	}

	public int GetGroupIndex(string strName)
	{
		for (int num = m_LightGroups.Count - 1; num >= 0; num--)
		{
			if (string.CompareOrdinal(strName, m_LightGroups[num].m_Name) == 0)
			{
				return num;
			}
		}
		return -1;
	}

	public void DeleteGroup(int groupIndex)
	{
		LightGroup lightGroup = m_LightGroups[groupIndex];
		for (int i = 0; i < lightGroup.m_Lights.Count; i++)
		{
			lightGroup.m_Lights[i].m_Groups.Remove(lightGroup.m_ID);
		}
		m_LightGroups.RemoveAt(groupIndex);
	}

	public void AddLightToGroup_Index(LightControl lightControl, int groupIndex)
	{
		LightGroup lightGroup = m_LightGroups[groupIndex];
		if (!lightGroup.m_Lights.Contains(lightControl))
		{
			lightControl.m_Groups.Add(groupIndex);
			lightGroup.m_Lights.Add(lightControl);
		}
	}

	public void MoveLightToGroup_Index(LightControl lightControl, int groupIndex)
	{
		for (int i = 0; i < lightControl.m_Groups.Count; i++)
		{
			LightGroup lightGroup = m_LightGroups[lightControl.m_Groups[i]];
			if (lightGroup.m_Lights.Contains(lightControl))
			{
				lightGroup.m_Lights.Remove(lightControl);
			}
		}
		lightControl.m_Groups.Clear();
		AddLightToGroup_Index(lightControl, groupIndex);
	}

	public void RemoveLightFromGroup_Index(LightControl lightControl, int groupIndex)
	{
		LightGroup lightGroup = m_LightGroups[groupIndex];
		if (lightGroup.m_Lights.Contains(lightControl))
		{
			lightControl.m_Groups.Remove(groupIndex);
			lightGroup.m_Lights.Remove(lightControl);
		}
	}

	public void AddTimeLineToGroup_Index(int groupIndex)
	{
		LightGroup lightGroup = m_LightGroups[groupIndex];
		LightGroup.TimeOnOff item = new LightGroup.TimeOnOff();
		lightGroup.m_Times.Add(item);
	}

	public void AddEffectToGroup_Index(int groupIndex, LightEffect.Effects effect)
	{
		LightGroup lightGroup = m_LightGroups[groupIndex];
		LightEffect lightEffect = LightEffect.CreateNewEffectInstance(effect);
		if (lightEffect != null)
		{
			lightGroup.m_Effects.Add(lightEffect);
		}
	}

	public int GetCurrentLightID()
	{
		int currentLightID = m_CurrentLightID;
		m_CurrentLightID++;
		return currentLightID;
	}

	public void SetCurrentLight(ref LightingPeriod newLight)
	{
		m_CurrentLight = newLight;
	}

	public void TransitionLight(ref LightingPeriod fromLight, ref LightingPeriod towardLight, float transitionProgress)
	{
		m_CurrentLight.m_IndoorAmbientColor = Color.Lerp(fromLight.m_IndoorAmbientColor, towardLight.m_IndoorAmbientColor, transitionProgress);
		m_CurrentLight.m_OutdoorAmbientColor = Color.Lerp(fromLight.m_OutdoorAmbientColor, towardLight.m_OutdoorAmbientColor, transitionProgress);
		m_CurrentLight.m_DirectionalLightColour = Color.Lerp(fromLight.m_DirectionalLightColour, towardLight.m_DirectionalLightColour, transitionProgress);
		m_CurrentLight.m_DirectionalShadowColour = Color.Lerp(fromLight.m_DirectionalShadowColour, towardLight.m_DirectionalShadowColour, transitionProgress);
		m_CurrentLight.m_IndoorIntensity = Mathf.Lerp(fromLight.m_IndoorIntensity, towardLight.m_IndoorIntensity, transitionProgress);
		m_CurrentLight.m_OutdoorIntensity = Mathf.Lerp(fromLight.m_OutdoorIntensity, towardLight.m_OutdoorIntensity, transitionProgress);
		m_CurrentLight.m_DirectionalLightIntensity = Mathf.Lerp(fromLight.m_DirectionalLightIntensity, towardLight.m_DirectionalLightIntensity, transitionProgress);
		m_CurrentLight.m_DirectionalShadowIntensity = Mathf.Lerp(fromLight.m_DirectionalShadowIntensity, towardLight.m_DirectionalShadowIntensity, transitionProgress);
		m_CurrentLight.m_LightAngle = Mathf.LerpAngle(fromLight.m_LightAngle, towardLight.m_LightAngle, transitionProgress);
		Vector2 vector = Quaternion.Euler(0f, 0f, m_CurrentLight.m_LightAngle) * Vector2.right;
		m_CurrentLightDirection.x = vector.x;
		m_CurrentLightDirection.y = vector.y;
		m_CurrentLight.m_FoggingColor = Color.Lerp(fromLight.m_FoggingColor, towardLight.m_FoggingColor, transitionProgress);
		m_CurrentLight.m_FogDensity = Mathf.Lerp(fromLight.m_FogDensity, towardLight.m_FogDensity, transitionProgress);
		m_CurrentLight.m_FogStartDistance = Mathf.Lerp(fromLight.m_FogStartDistance, towardLight.m_FogStartDistance, transitionProgress);
		if (m_DirectionalLight != null)
		{
			Vector3 currentDirectionalLightVector = GetCurrentDirectionalLightVector();
			m_DirectionalLight.color = m_CurrentLight.m_DirectionalLightColour;
			m_DirectionalLight.intensity = m_CurrentLight.m_DirectionalLightIntensity;
			m_DirectionalLight.transform.rotation = new Quaternion(currentDirectionalLightVector.x, currentDirectionalLightVector.y, currentDirectionalLightVector.z, 1f);
		}
		if (OnLightingUpdated != null)
		{
			OnLightingUpdated();
		}
	}

	public Color GetCurrentAmbientColor(bool inside)
	{
		return (!inside) ? m_CurrentLight.m_OutdoorAmbientColor : m_CurrentLight.m_IndoorAmbientColor;
	}

	public float GetCurrentAmbientIntensity(bool inside)
	{
		return (!inside) ? m_CurrentLight.m_OutdoorIntensity : m_CurrentLight.m_IndoorIntensity;
	}

	public float GetCurrentAmbientDepthIntensityFactor(bool inside)
	{
		if (inside)
		{
			if (m_CurrentLight.m_IndoorAmbientIntensityDepthFactor < 0.01f)
			{
				return 1f;
			}
			return m_CurrentLight.m_IndoorAmbientIntensityDepthFactor;
		}
		if (m_CurrentLight.m_OutdoorAmbientIntensityDepthFactor < 0.01f)
		{
			return 1f;
		}
		return m_CurrentLight.m_OutdoorAmbientIntensityDepthFactor;
	}

	public Color GetCurrentDirectionalLightColor()
	{
		return m_CurrentLight.m_DirectionalLightColour;
	}

	public Color GetCurrentDirectionalShadowColor()
	{
		return m_CurrentLight.m_DirectionalShadowColour;
	}

	public float GetCurrentDirectionalLightIntensity()
	{
		return m_CurrentLight.m_DirectionalLightIntensity;
	}

	public float GetCurrentDirectionalShadowIntensity()
	{
		return m_CurrentLight.m_DirectionalShadowIntensity;
	}

	public Vector3 GetCurrentDirectionalLightVector()
	{
		return m_CurrentLightDirection;
	}

	public float GetCurrentDirectionalLightHeight()
	{
		return 0.3f;
	}

	public Color GetUnderGroundAmbientLightColour()
	{
		return m_UnderGroundLight.m_IndoorAmbientColor;
	}

	public float GetUnderGroundAmbientLightIntensity()
	{
		return m_UnderGroundLight.m_IndoorIntensity;
	}

	public void GetTripleOfCurrentDirectionalLightInfoWithShadowReverse(out float revShadowIntensity, out Vector3 lightDir, out Color shadowColour)
	{
		revShadowIntensity = 1f - m_CurrentLight.m_DirectionalShadowIntensity;
		lightDir = m_CurrentLightDirection;
		shadowColour = m_CurrentLight.m_DirectionalShadowColour;
	}

	public Color GetFogColour()
	{
		return m_CurrentLight.m_FoggingColor;
	}

	public float GetFogDensity()
	{
		return m_CurrentLight.m_FogDensity;
	}

	public float GetFogStartDistance()
	{
		return m_CurrentLight.m_FogStartDistance;
	}

	public void ForceSettings()
	{
		for (int i = 0; i < m_LightGroups.Count; i++)
		{
			int count = m_LightGroups[i].m_Lights.Count;
			for (int j = 0; j < count; j++)
			{
				if (m_LightGroups[i].m_Lights[j] != null)
				{
					m_LightGroups[i].m_Lights[j].ResetSettings();
				}
			}
		}
	}

	public void SetTimeOverride(int hour, int min)
	{
		m_bOverrideTime = true;
		m_OverrideHour = hour;
		m_OverrideMin = min;
		if (OnTimeOverridden != null)
		{
			OnTimeOverridden(overridden: true, m_OverrideHour, m_OverrideMin);
		}
	}

	public void ReleaseTimeOverride()
	{
		m_bOverrideTime = false;
		if (OnTimeOverridden != null)
		{
			OnTimeOverridden(overridden: false, 0, 0);
		}
	}

	public void OnBeforeSerialize()
	{
		for (int i = 0; i < m_LightGroups.Count; i++)
		{
			for (int j = 0; j < m_LightGroups[i].m_Effects.Count; j++)
			{
				LightGroup.SerializedLightEffect serializedLightEffect = new LightGroup.SerializedLightEffect();
				serializedLightEffect.effectType = (int)m_LightGroups[i].m_Effects[j].GetEffectType();
				serializedLightEffect.data = m_LightGroups[i].m_Effects[j].Serialize();
				if (m_LightGroups[i].m_SerializedEffects.Count <= j)
				{
					m_LightGroups[i].m_SerializedEffects.Add(serializedLightEffect);
				}
				else
				{
					m_LightGroups[i].m_SerializedEffects[j] = serializedLightEffect;
				}
			}
		}
	}

	public void OnAfterDeserialize()
	{
		for (int i = 0; i < m_LightGroups.Count; i++)
		{
			m_LightGroups[i].m_Effects.Clear();
			for (int j = 0; j < m_LightGroups[i].m_SerializedEffects.Count; j++)
			{
				int effectType = m_LightGroups[i].m_SerializedEffects[j].effectType;
				string data = m_LightGroups[i].m_SerializedEffects[j].data;
				if (effectType >= 0)
				{
					LightEffect lightEffect = LightEffect.CreateNewEffectInstance((LightEffect.Effects)effectType);
					if (!string.IsNullOrEmpty(data))
					{
						lightEffect.Deserialize(data);
					}
					m_LightGroups[i].m_Effects.Add(lightEffect);
				}
			}
		}
	}

	public void ControlledLateUpdate()
	{
	}

	public void ControlledPreFixedUpdate()
	{
	}

	public void ControlledPreUpdate()
	{
	}

	public bool RequiresControlledUpdate()
	{
		return true;
	}

	public bool RequiresControlledFixedUpdate()
	{
		return false;
	}

	public bool RequiresControlledLateUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreFixedUpdate()
	{
		return false;
	}
}
