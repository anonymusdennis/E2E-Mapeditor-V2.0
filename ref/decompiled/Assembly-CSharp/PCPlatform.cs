using System;
using System.Text;
using UnityEngine;

public class PCPlatform : Platform
{
	private bool m_bRunDiscovery;

	private int m_PlayerDisPadIndex;

	private bool m_bPlayerDiscovered;

	private bool m_bDiscoveryMain;

	protected override void Awake()
	{
		Debug.Log("  ************  PCPlatform       Awake *******  ");
		base.Awake();
		Debug.Log("  ************  PCPlatform       Awake 2 *******  ");
		if (!m_bInitialized)
		{
			m_bInitialized = true;
			UnityEngine.Object.DontDestroyOnLoad(base.transform.gameObject);
			GlobalStart.EnteredLevelEvent += GlobalStart_EnteredLevelEvent;
		}
	}

	protected override void Start()
	{
		base.Start();
		m_bRunDiscovery = false;
	}

	protected override void Update()
	{
		base.Update();
		if (!m_bRunDiscovery)
		{
			return;
		}
		int padIndex = -1;
		if (DetectAnyPadStart(ref padIndex) && (m_bDiscoveryMain || padIndex != m_PlayerDisPadIndex))
		{
			m_bRunDiscovery = false;
			m_bPlayerDiscovered = true;
			m_PlayerDisPadIndex = padIndex;
			if (!m_bDiscoveryMain)
			{
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		GlobalStart.EnteredLevelEvent -= GlobalStart_EnteredLevelEvent;
	}

	protected virtual void GlobalStart_EnteredLevelEvent()
	{
		int value = 0;
		GlobalSave.GetInstance().Get("Settings:VSync", out value, 0);
		QualityManager.SetVsyncCount(value);
		if (CameraManager.GetInstance() != null)
		{
			GlobalSave.GetInstance().Get("Settings:Blur", out value, 1);
			if ((float)value < 0.5f)
			{
				CameraManager.GetInstance().SetBlurEffectEnabled(bEnable: false);
			}
			else
			{
				CameraManager.GetInstance().SetBlurEffectEnabled(bEnable: true);
			}
		}
	}

	private bool DetectAnyPadStart(ref int padIndex)
	{
		bool result = false;
		KeyCode[] array = new KeyCode[2]
		{
			KeyCode.Joystick1Button0,
			KeyCode.Joystick2Button0
		};
		for (int i = 0; i < array.Length; i++)
		{
			if (Input.GetKeyDown(array[i]))
			{
				padIndex = i;
				return true;
			}
		}
		padIndex = -1;
		return result;
	}

	private void OnSaveLoadBusy(bool busy)
	{
	}

	public override bool StartDiscovery(bool bMainUser)
	{
		m_bRunDiscovery = true;
		m_bPlayerDiscovered = false;
		m_bDiscoveryMain = bMainUser;
		return true;
	}

	public override bool FinishedDiscovery(int gamePadIndex)
	{
		return m_bPlayerDiscovered;
	}

	public override void SetPresenceTag(string text)
	{
	}

	public override string GetUniqueID()
	{
		return Convert.ToBase64String(Encoding.ASCII.GetBytes(SystemInfo.deviceUniqueIdentifier));
	}

	public override void OnlineAreaEntryCheckRequest(bool isLeaderboard, OnlineAreaEntryCheckCallback callback, bool isModal = true, bool bShowSystemDialogue = true)
	{
		callback?.Invoke(bAllowedToProgress: true, OnlineAccessErrorCode.OnlineAccessOK, failureHandledPlatformside: false);
	}

	public override bool SupportsDifferentQualitySettings()
	{
		return true;
	}
}
