using System;

public class AkOutputSettings : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public AkPanningRule ePanningRule
	{
		get
		{
			return (AkPanningRule)AkSoundEnginePINVOKE.CSharp_AkOutputSettings_ePanningRule_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkOutputSettings_ePanningRule_set(swigCPtr, (int)value);
		}
	}

	public AkChannelConfig channelConfig
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkOutputSettings_channelConfig_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new AkChannelConfig(intPtr, cMemoryOwn: false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkOutputSettings_channelConfig_set(swigCPtr, AkChannelConfig.getCPtr(value));
		}
	}

	public uint audioDeviceShareset
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkOutputSettings_audioDeviceShareset_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkOutputSettings_audioDeviceShareset_set(swigCPtr, value);
		}
	}

	internal AkOutputSettings(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkOutputSettings()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkOutputSettings(), cMemoryOwn: true)
	{
	}

	internal static IntPtr getCPtr(AkOutputSettings obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~AkOutputSettings()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkOutputSettings(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
