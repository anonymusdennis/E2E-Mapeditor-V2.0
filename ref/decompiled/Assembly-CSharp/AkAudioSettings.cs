using System;

public class AkAudioSettings : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public uint uNumSamplesPerFrame
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkAudioSettings_uNumSamplesPerFrame_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkAudioSettings_uNumSamplesPerFrame_set(swigCPtr, value);
		}
	}

	public uint uNumSamplesPerSecond
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkAudioSettings_uNumSamplesPerSecond_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkAudioSettings_uNumSamplesPerSecond_set(swigCPtr, value);
		}
	}

	internal AkAudioSettings(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkAudioSettings()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkAudioSettings(), cMemoryOwn: true)
	{
	}

	internal static IntPtr getCPtr(AkAudioSettings obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~AkAudioSettings()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkAudioSettings(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
