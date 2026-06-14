using System;

public class AkPositioningInfo : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public float fCenterPct
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_fCenterPct_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_fCenterPct_set(swigCPtr, value);
		}
	}

	public AkPannerType pannerType
	{
		get
		{
			return (AkPannerType)AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_pannerType_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_pannerType_set(swigCPtr, (int)value);
		}
	}

	public AkPositionSourceType posSourceType
	{
		get
		{
			return (AkPositionSourceType)AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_posSourceType_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_posSourceType_set(swigCPtr, (int)value);
		}
	}

	public bool bUpdateEachFrame
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_bUpdateEachFrame_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_bUpdateEachFrame_set(swigCPtr, value);
		}
	}

	public bool bUseSpatialization
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_bUseSpatialization_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_bUseSpatialization_set(swigCPtr, value);
		}
	}

	public bool bUseAttenuation
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_bUseAttenuation_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_bUseAttenuation_set(swigCPtr, value);
		}
	}

	public bool bUseConeAttenuation
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_bUseConeAttenuation_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_bUseConeAttenuation_set(swigCPtr, value);
		}
	}

	public float fInnerAngle
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_fInnerAngle_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_fInnerAngle_set(swigCPtr, value);
		}
	}

	public float fOuterAngle
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_fOuterAngle_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_fOuterAngle_set(swigCPtr, value);
		}
	}

	public float fConeMaxAttenuation
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_fConeMaxAttenuation_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_fConeMaxAttenuation_set(swigCPtr, value);
		}
	}

	public float LPFCone
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_LPFCone_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_LPFCone_set(swigCPtr, value);
		}
	}

	public float HPFCone
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_HPFCone_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_HPFCone_set(swigCPtr, value);
		}
	}

	public float fMaxDistance
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_fMaxDistance_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_fMaxDistance_set(swigCPtr, value);
		}
	}

	public float fVolDryAtMaxDist
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_fVolDryAtMaxDist_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_fVolDryAtMaxDist_set(swigCPtr, value);
		}
	}

	public float fVolAuxGameDefAtMaxDist
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_fVolAuxGameDefAtMaxDist_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_fVolAuxGameDefAtMaxDist_set(swigCPtr, value);
		}
	}

	public float fVolAuxUserDefAtMaxDist
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_fVolAuxUserDefAtMaxDist_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_fVolAuxUserDefAtMaxDist_set(swigCPtr, value);
		}
	}

	public float LPFValueAtMaxDist
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_LPFValueAtMaxDist_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_LPFValueAtMaxDist_set(swigCPtr, value);
		}
	}

	public float HPFValueAtMaxDist
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_HPFValueAtMaxDist_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPositioningInfo_HPFValueAtMaxDist_set(swigCPtr, value);
		}
	}

	internal AkPositioningInfo(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkPositioningInfo()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkPositioningInfo(), cMemoryOwn: true)
	{
	}

	internal static IntPtr getCPtr(AkPositioningInfo obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~AkPositioningInfo()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkPositioningInfo(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
