using System;

public class AkSegmentInfo : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public int iCurrentPosition
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSegmentInfo_iCurrentPosition_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSegmentInfo_iCurrentPosition_set(swigCPtr, value);
		}
	}

	public int iPreEntryDuration
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSegmentInfo_iPreEntryDuration_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSegmentInfo_iPreEntryDuration_set(swigCPtr, value);
		}
	}

	public int iActiveDuration
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSegmentInfo_iActiveDuration_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSegmentInfo_iActiveDuration_set(swigCPtr, value);
		}
	}

	public int iPostExitDuration
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSegmentInfo_iPostExitDuration_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSegmentInfo_iPostExitDuration_set(swigCPtr, value);
		}
	}

	public int iRemainingLookAheadTime
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSegmentInfo_iRemainingLookAheadTime_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSegmentInfo_iRemainingLookAheadTime_set(swigCPtr, value);
		}
	}

	public float fBeatDuration
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSegmentInfo_fBeatDuration_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSegmentInfo_fBeatDuration_set(swigCPtr, value);
		}
	}

	public float fBarDuration
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSegmentInfo_fBarDuration_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSegmentInfo_fBarDuration_set(swigCPtr, value);
		}
	}

	public float fGridDuration
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSegmentInfo_fGridDuration_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSegmentInfo_fGridDuration_set(swigCPtr, value);
		}
	}

	public float fGridOffset
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSegmentInfo_fGridOffset_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSegmentInfo_fGridOffset_set(swigCPtr, value);
		}
	}

	internal AkSegmentInfo(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkSegmentInfo()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkSegmentInfo(), cMemoryOwn: true)
	{
	}

	internal static IntPtr getCPtr(AkSegmentInfo obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~AkSegmentInfo()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkSegmentInfo(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
