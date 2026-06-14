using System;

public class AkMusicSettings : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public float fStreamingLookAheadRatio
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMusicSettings_fStreamingLookAheadRatio_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkMusicSettings_fStreamingLookAheadRatio_set(swigCPtr, value);
		}
	}

	internal AkMusicSettings(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkMusicSettings()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkMusicSettings(), cMemoryOwn: true)
	{
	}

	internal static IntPtr getCPtr(AkMusicSettings obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~AkMusicSettings()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkMusicSettings(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
