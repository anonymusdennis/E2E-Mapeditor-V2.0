using UnityEngine;

public class PCVideoDrone : VideoDrone
{
	private AudioSource m_AudSource;

	private bool m_bPaused;

	protected override void Awake()
	{
		m_AudSource = base.gameObject.AddComponent<AudioSource>();
		base.gameObject.AddComponent<AudioListener>();
	}

	protected override void Update()
	{
		base.Update();
		if (m_bPlaying && !m_Settings.m_MovieTexture.isPlaying && !m_bPaused)
		{
			m_bPlaying = false;
			m_Settings.m_MovieTexture = null;
			SignalVideoEnded();
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_Settings.m_MovieTexture != null)
		{
			m_Settings.m_MovieTexture.Stop();
		}
		if (m_AudSource != null)
		{
			m_AudSource.Stop();
		}
		m_OutputTexture = null;
		m_OutputMaterial = null;
	}

	public override bool Play(bool videoLoops = false, bool audioOn = true)
	{
		if (base.Play(videoLoops, audioOn) && m_AudSource != null)
		{
			if (m_Settings.m_MovieTexture != null)
			{
				float num = ((!m_Settings.m_PlaysMusic) ? AudioController.Instance.m_SFXVolume : AudioController.Instance.m_MusicVolume);
				m_AudSource.clip = m_Settings.m_MovieTexture.audioClip;
				m_AudSource.volume = num / 100f;
				m_Settings.m_MovieTexture.loop = videoLoops;
				m_OutputTexture = m_Settings.m_MovieTexture;
				m_Settings.m_MovieTexture.Play();
				m_AudSource.Play();
				m_bPlaying = true;
			}
			else
			{
				m_OutputTexture = m_Settings.m_MovieTexture;
			}
		}
		return m_bPlaying;
	}

	public override void StopVideo()
	{
		if (m_Settings.m_MovieTexture != null)
		{
			m_Settings.m_MovieTexture.Stop();
		}
		if (m_AudSource != null)
		{
			m_AudSource.Stop();
		}
		m_bPlaying = false;
	}

	private void OnApplicationFocus(bool bFocus)
	{
		if (!(m_Settings != null) || !(m_Settings.m_MovieTexture != null) || !(m_AudSource != null))
		{
			return;
		}
		bool flag = PlayerPrefs.GetInt("Settings:BackgroundVideoEnabled", 1) == 1;
		if (!(GlobalStart.GetInstance() != null) || GlobalStart.GetInstance().CurrentGlobalStartMode != GlobalStart.GLOBALSTART_MODE.SHOW_FRONTEND || flag)
		{
			if (bFocus)
			{
				m_Settings.m_MovieTexture.Play();
				m_AudSource.UnPause();
				m_bPaused = false;
			}
			else
			{
				m_Settings.m_MovieTexture.Pause();
				m_AudSource.Pause();
				m_bPaused = true;
			}
		}
	}
}
