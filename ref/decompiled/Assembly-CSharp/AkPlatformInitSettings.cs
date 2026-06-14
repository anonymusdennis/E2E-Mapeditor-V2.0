using System;

public class AkPlatformInitSettings : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public AkThreadProperties threadLEngine
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkPlatformInitSettings_threadLEngine_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new AkThreadProperties(intPtr, cMemoryOwn: false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPlatformInitSettings_threadLEngine_set(swigCPtr, AkThreadProperties.getCPtr(value));
		}
	}

	public AkThreadProperties threadBankManager
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkPlatformInitSettings_threadBankManager_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new AkThreadProperties(intPtr, cMemoryOwn: false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPlatformInitSettings_threadBankManager_set(swigCPtr, AkThreadProperties.getCPtr(value));
		}
	}

	public AkThreadProperties threadMonitor
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkPlatformInitSettings_threadMonitor_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new AkThreadProperties(intPtr, cMemoryOwn: false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPlatformInitSettings_threadMonitor_set(swigCPtr, AkThreadProperties.getCPtr(value));
		}
	}

	public float fLEngineDefaultPoolRatioThreshold
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPlatformInitSettings_fLEngineDefaultPoolRatioThreshold_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPlatformInitSettings_fLEngineDefaultPoolRatioThreshold_set(swigCPtr, value);
		}
	}

	public uint uLEngineDefaultPoolSize
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPlatformInitSettings_uLEngineDefaultPoolSize_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPlatformInitSettings_uLEngineDefaultPoolSize_set(swigCPtr, value);
		}
	}

	public uint uSampleRate
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPlatformInitSettings_uSampleRate_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPlatformInitSettings_uSampleRate_set(swigCPtr, value);
		}
	}

	public ushort uNumRefillsInVoice
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPlatformInitSettings_uNumRefillsInVoice_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPlatformInitSettings_uNumRefillsInVoice_set(swigCPtr, value);
		}
	}

	internal AkPlatformInitSettings(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkPlatformInitSettings()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkPlatformInitSettings(), cMemoryOwn: true)
	{
	}

	internal static IntPtr getCPtr(AkPlatformInitSettings obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~AkPlatformInitSettings()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkPlatformInitSettings(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
