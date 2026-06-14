using System;

public class AkInitSettings : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public int pfnAssertHook
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public uint uMaxNumPaths
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkInitSettings_uMaxNumPaths_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkInitSettings_uMaxNumPaths_set(swigCPtr, value);
		}
	}

	public uint uDefaultPoolSize
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkInitSettings_uDefaultPoolSize_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkInitSettings_uDefaultPoolSize_set(swigCPtr, value);
		}
	}

	public float fDefaultPoolRatioThreshold
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkInitSettings_fDefaultPoolRatioThreshold_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkInitSettings_fDefaultPoolRatioThreshold_set(swigCPtr, value);
		}
	}

	public uint uCommandQueueSize
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkInitSettings_uCommandQueueSize_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkInitSettings_uCommandQueueSize_set(swigCPtr, value);
		}
	}

	public int uPrepareEventMemoryPoolID
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkInitSettings_uPrepareEventMemoryPoolID_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkInitSettings_uPrepareEventMemoryPoolID_set(swigCPtr, value);
		}
	}

	public bool bEnableGameSyncPreparation
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkInitSettings_bEnableGameSyncPreparation_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkInitSettings_bEnableGameSyncPreparation_set(swigCPtr, value);
		}
	}

	public uint uContinuousPlaybackLookAhead
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkInitSettings_uContinuousPlaybackLookAhead_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkInitSettings_uContinuousPlaybackLookAhead_set(swigCPtr, value);
		}
	}

	public uint uNumSamplesPerFrame
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkInitSettings_uNumSamplesPerFrame_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkInitSettings_uNumSamplesPerFrame_set(swigCPtr, value);
		}
	}

	public uint uMonitorPoolSize
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkInitSettings_uMonitorPoolSize_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkInitSettings_uMonitorPoolSize_set(swigCPtr, value);
		}
	}

	public uint uMonitorQueuePoolSize
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkInitSettings_uMonitorQueuePoolSize_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkInitSettings_uMonitorQueuePoolSize_set(swigCPtr, value);
		}
	}

	public AkOutputSettings settingsMainOutput
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkInitSettings_settingsMainOutput_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new AkOutputSettings(intPtr, cMemoryOwn: false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkInitSettings_settingsMainOutput_set(swigCPtr, AkOutputSettings.getCPtr(value));
		}
	}

	public uint uMaxHardwareTimeoutMs
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkInitSettings_uMaxHardwareTimeoutMs_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkInitSettings_uMaxHardwareTimeoutMs_set(swigCPtr, value);
		}
	}

	public bool bUseSoundBankMgrThread
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkInitSettings_bUseSoundBankMgrThread_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkInitSettings_bUseSoundBankMgrThread_set(swigCPtr, value);
		}
	}

	public bool bUseLEngineThread
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkInitSettings_bUseLEngineThread_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkInitSettings_bUseLEngineThread_set(swigCPtr, value);
		}
	}

	public string szPluginDLLPath
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkInitSettings_szPluginDLLPath_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkInitSettings_szPluginDLLPath_set(swigCPtr, value);
		}
	}

	internal AkInitSettings(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkInitSettings()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkInitSettings(), cMemoryOwn: true)
	{
	}

	internal static IntPtr getCPtr(AkInitSettings obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~AkInitSettings()
	{
		Dispose();
	}

	public virtual void Dispose()
	{
		lock (this)
		{
			if (swigCPtr != IntPtr.Zero)
			{
				if (swigCMemOwn)
				{
					swigCMemOwn = false;
					AkSoundEnginePINVOKE.CSharp_delete_AkInitSettings(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
