using System;

public class AkDeviceSettings : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public IntPtr pIOMemory
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkDeviceSettings_pIOMemory_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkDeviceSettings_pIOMemory_set(swigCPtr, value);
		}
	}

	public uint uIOMemorySize
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkDeviceSettings_uIOMemorySize_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkDeviceSettings_uIOMemorySize_set(swigCPtr, value);
		}
	}

	public uint uIOMemoryAlignment
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkDeviceSettings_uIOMemoryAlignment_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkDeviceSettings_uIOMemoryAlignment_set(swigCPtr, value);
		}
	}

	public int ePoolAttributes
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkDeviceSettings_ePoolAttributes_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkDeviceSettings_ePoolAttributes_set(swigCPtr, value);
		}
	}

	public uint uGranularity
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkDeviceSettings_uGranularity_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkDeviceSettings_uGranularity_set(swigCPtr, value);
		}
	}

	public uint uSchedulerTypeFlags
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkDeviceSettings_uSchedulerTypeFlags_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkDeviceSettings_uSchedulerTypeFlags_set(swigCPtr, value);
		}
	}

	public AkThreadProperties threadProperties
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkDeviceSettings_threadProperties_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new AkThreadProperties(intPtr, cMemoryOwn: false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkDeviceSettings_threadProperties_set(swigCPtr, AkThreadProperties.getCPtr(value));
		}
	}

	public float fTargetAutoStmBufferLength
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkDeviceSettings_fTargetAutoStmBufferLength_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkDeviceSettings_fTargetAutoStmBufferLength_set(swigCPtr, value);
		}
	}

	public uint uMaxConcurrentIO
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkDeviceSettings_uMaxConcurrentIO_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkDeviceSettings_uMaxConcurrentIO_set(swigCPtr, value);
		}
	}

	public bool bUseStreamCache
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkDeviceSettings_bUseStreamCache_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkDeviceSettings_bUseStreamCache_set(swigCPtr, value);
		}
	}

	public uint uMaxCachePinnedBytes
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkDeviceSettings_uMaxCachePinnedBytes_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkDeviceSettings_uMaxCachePinnedBytes_set(swigCPtr, value);
		}
	}

	internal AkDeviceSettings(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkDeviceSettings()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkDeviceSettings(), cMemoryOwn: true)
	{
	}

	internal static IntPtr getCPtr(AkDeviceSettings obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~AkDeviceSettings()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkDeviceSettings(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
