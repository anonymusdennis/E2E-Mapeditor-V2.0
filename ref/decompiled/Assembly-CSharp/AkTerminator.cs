using System;
using System.Threading;
using UnityEngine;

[AddComponentMenu("Wwise/AkTerminator")]
public class AkTerminator : MonoBehaviour
{
	private static AkTerminator ms_Instance;

	private void Awake()
	{
		if (ms_Instance != null)
		{
			if (ms_Instance != this)
			{
				UnityEngine.Object.DestroyImmediate(this);
			}
		}
		else
		{
			UnityEngine.Object.DontDestroyOnLoad(this);
			ms_Instance = this;
		}
	}

	private void OnApplicationQuit()
	{
		Terminate();
	}

	private void OnDestroy()
	{
		if (ms_Instance == this)
		{
			ms_Instance = null;
		}
	}

	private void Terminate()
	{
		if (ms_Instance == null || ms_Instance != this || !AkSoundEngine.IsInitialized())
		{
			return;
		}
		AkCallbackManager.SetMonitoringCallback((ErrorLevel)0, null);
		AkSoundEngine.StopAll();
		AkSoundEngine.ClearBanks();
		AkSoundEngine.RenderAudio();
		int num = 5;
		do
		{
			int num2 = 0;
			do
			{
				num2 = AkCallbackManager.PostCallbacks();
				using EventWaitHandle eventWaitHandle = new ManualResetEvent(initialState: false);
				eventWaitHandle.WaitOne(TimeSpan.FromMilliseconds(1.0));
			}
			while (num2 > 0);
			using (EventWaitHandle eventWaitHandle2 = new ManualResetEvent(initialState: false))
			{
				eventWaitHandle2.WaitOne(TimeSpan.FromMilliseconds(10.0));
			}
			num--;
		}
		while (num > 0);
		AkSoundEngine.Term();
		AkCallbackManager.PostCallbacks();
		ms_Instance = null;
		AkCallbackManager.Term();
		AkBankManager.Reset();
	}
}
