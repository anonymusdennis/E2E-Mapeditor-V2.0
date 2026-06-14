using System;
using System.Collections.Generic;
using UnityEngine;

public class WeatherEffectData : MonoBehaviour
{
	public enum RotationMode
	{
		None,
		VelocityBased,
		Fixed
	}

	public enum AudioEffectTriggerMode
	{
		None,
		OnStart,
		OnAlphaThreshold
	}

	public bool m_EffectEnabled;

	public RotationMode m_RotationMode;

	public float m_Rotation;

	public Texture2D m_Texture;

	public bool m_AnimatedTextureMode;

	public float m_AnimationInterval;

	public List<Texture2D> m_AnimationTextures = new List<Texture2D>();

	public AnimationCurve m_XScrollCurve;

	public AnimationCurve m_YScrollCurve;

	public AnimationCurve m_EffectAlphaCurve;

	public float m_EffectScale = 1f;

	public int m_EffectSpacingX = 1;

	public int m_EffectSpacingY = 1;

	public string m_AudioEffectOn = string.Empty;

	public string m_AudioEffectOff = string.Empty;

	public AudioEffectTriggerMode m_AudioEffectTriggerMode;

	public float m_AlphaThreshold;

	public bool m_bRumbleEnabled;

	public Platform.RumbleController m_RumbleSettings = new Platform.RumbleController();

	public bool m_bLightbarEnabled;

	public Platform.LightBarEffect m_LightbarSettings = new Platform.LightBarEffect();

	[NonSerialized]
	public bool m_bControllerEffectsPlayed;

	[HideInInspector]
	public float m_CurrentXOffset;

	[HideInInspector]
	public float m_PreviousXOffset;

	[HideInInspector]
	public float m_DeltaXOffset;

	[HideInInspector]
	public float m_CurrentYOffset;

	[HideInInspector]
	public float m_PreviousYOffset;

	[HideInInspector]
	public float m_DeltaYOffset;

	[HideInInspector]
	public float m_CurrentEffectTime;

	[HideInInspector]
	public int m_CurrentTextureIndex;

	[HideInInspector]
	public bool m_bAudioEffectActive;

	public void ResetDataToDefault()
	{
		m_EffectEnabled = false;
		m_RotationMode = RotationMode.None;
		m_Rotation = 0f;
		m_Texture = null;
		m_AnimatedTextureMode = false;
		m_AnimationInterval = 0f;
		m_AnimationTextures = new List<Texture2D>();
		m_XScrollCurve = null;
		m_YScrollCurve = null;
		m_EffectAlphaCurve = null;
		m_EffectScale = 1f;
		m_EffectSpacingX = 1;
		m_EffectSpacingY = 1;
		m_AudioEffectOn = string.Empty;
		m_AudioEffectOff = string.Empty;
		m_AudioEffectTriggerMode = AudioEffectTriggerMode.None;
		m_AlphaThreshold = 0f;
		m_CurrentXOffset = 0f;
		m_PreviousXOffset = 0f;
		m_DeltaXOffset = 0f;
		m_CurrentYOffset = 0f;
		m_PreviousYOffset = 0f;
		m_DeltaYOffset = 0f;
		m_CurrentEffectTime = 0f;
		m_CurrentTextureIndex = 0;
		m_bAudioEffectActive = false;
	}

	public void Copy(WeatherEffectData rhs)
	{
		m_Rotation = rhs.m_Rotation;
		m_Texture = rhs.m_Texture;
		m_XScrollCurve = rhs.m_XScrollCurve;
		m_YScrollCurve = rhs.m_YScrollCurve;
		m_EffectAlphaCurve = rhs.m_EffectAlphaCurve;
		m_EffectScale = rhs.m_EffectScale;
		m_EffectEnabled = rhs.m_EffectEnabled;
		m_EffectSpacingX = rhs.m_EffectSpacingX;
		m_EffectSpacingY = rhs.m_EffectSpacingY;
		m_CurrentXOffset = rhs.m_CurrentXOffset;
		m_PreviousXOffset = rhs.m_PreviousXOffset;
		m_DeltaXOffset = rhs.m_DeltaXOffset;
		m_CurrentYOffset = rhs.m_CurrentYOffset;
		m_PreviousYOffset = rhs.m_PreviousYOffset;
		m_DeltaYOffset = rhs.m_DeltaYOffset;
		m_CurrentEffectTime = rhs.m_CurrentEffectTime;
		m_CurrentTextureIndex = rhs.m_CurrentTextureIndex;
	}

	public void CopyEffectData(WeatherEffectData rhs)
	{
		m_Rotation = rhs.m_Rotation;
		m_Texture = rhs.m_Texture;
		m_XScrollCurve = rhs.m_XScrollCurve;
		m_YScrollCurve = rhs.m_YScrollCurve;
		m_EffectAlphaCurve = rhs.m_EffectAlphaCurve;
		m_EffectScale = rhs.m_EffectScale;
		m_EffectEnabled = rhs.m_EffectEnabled;
		m_EffectSpacingX = rhs.m_EffectSpacingX;
		m_EffectSpacingY = rhs.m_EffectSpacingY;
		m_CurrentXOffset = 0f;
		m_PreviousXOffset = 0f;
		m_DeltaXOffset = 0f;
		m_CurrentYOffset = 0f;
		m_PreviousYOffset = 0f;
		m_DeltaYOffset = 0f;
		m_CurrentEffectTime = 0f;
		m_CurrentTextureIndex = 0;
	}
}
