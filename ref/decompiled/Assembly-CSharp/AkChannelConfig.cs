using System;

public class AkChannelConfig : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public uint uNumChannels
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkChannelConfig_uNumChannels_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkChannelConfig_uNumChannels_set(swigCPtr, value);
		}
	}

	public uint eConfigType
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkChannelConfig_eConfigType_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkChannelConfig_eConfigType_set(swigCPtr, value);
		}
	}

	public uint uChannelMask
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkChannelConfig_uChannelMask_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkChannelConfig_uChannelMask_set(swigCPtr, value);
		}
	}

	internal AkChannelConfig(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkChannelConfig()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkChannelConfig__SWIG_0(), cMemoryOwn: true)
	{
	}

	public AkChannelConfig(uint in_uNumChannels, uint in_uChannelMask)
		: this(AkSoundEnginePINVOKE.CSharp_new_AkChannelConfig__SWIG_1(in_uNumChannels, in_uChannelMask), cMemoryOwn: true)
	{
	}

	internal static IntPtr getCPtr(AkChannelConfig obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~AkChannelConfig()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkChannelConfig(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public void Clear()
	{
		AkSoundEnginePINVOKE.CSharp_AkChannelConfig_Clear(swigCPtr);
	}

	public void SetStandard(uint in_uChannelMask)
	{
		AkSoundEnginePINVOKE.CSharp_AkChannelConfig_SetStandard(swigCPtr, in_uChannelMask);
	}

	public void SetStandardOrAnonymous(uint in_uNumChannels, uint in_uChannelMask)
	{
		AkSoundEnginePINVOKE.CSharp_AkChannelConfig_SetStandardOrAnonymous(swigCPtr, in_uNumChannels, in_uChannelMask);
	}

	public void SetAnonymous(uint in_uNumChannels)
	{
		AkSoundEnginePINVOKE.CSharp_AkChannelConfig_SetAnonymous(swigCPtr, in_uNumChannels);
	}

	public void SetAmbisonic(uint in_uNumChannels)
	{
		AkSoundEnginePINVOKE.CSharp_AkChannelConfig_SetAmbisonic(swigCPtr, in_uNumChannels);
	}

	public bool IsValid()
	{
		return AkSoundEnginePINVOKE.CSharp_AkChannelConfig_IsValid(swigCPtr);
	}

	public uint Serialize()
	{
		return AkSoundEnginePINVOKE.CSharp_AkChannelConfig_Serialize(swigCPtr);
	}

	public void Deserialize(uint in_uChannelConfig)
	{
		AkSoundEnginePINVOKE.CSharp_AkChannelConfig_Deserialize(swigCPtr, in_uChannelConfig);
	}

	public AkChannelConfig RemoveLFE()
	{
		return new AkChannelConfig(AkSoundEnginePINVOKE.CSharp_AkChannelConfig_RemoveLFE(swigCPtr), cMemoryOwn: true);
	}

	public AkChannelConfig RemoveCenter()
	{
		return new AkChannelConfig(AkSoundEnginePINVOKE.CSharp_AkChannelConfig_RemoveCenter(swigCPtr), cMemoryOwn: true);
	}

	public bool IsChannelConfigSupported()
	{
		return AkSoundEnginePINVOKE.CSharp_AkChannelConfig_IsChannelConfigSupported(swigCPtr);
	}
}
