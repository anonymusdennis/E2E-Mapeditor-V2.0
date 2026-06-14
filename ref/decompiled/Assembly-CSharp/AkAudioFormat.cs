using System;

public class AkAudioFormat : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public uint uSampleRate
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkAudioFormat_uSampleRate_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkAudioFormat_uSampleRate_set(swigCPtr, value);
		}
	}

	public AkChannelConfig channelConfig
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkAudioFormat_channelConfig_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new AkChannelConfig(intPtr, cMemoryOwn: false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkAudioFormat_channelConfig_set(swigCPtr, AkChannelConfig.getCPtr(value));
		}
	}

	public uint uBitsPerSample
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkAudioFormat_uBitsPerSample_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkAudioFormat_uBitsPerSample_set(swigCPtr, value);
		}
	}

	public uint uBlockAlign
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkAudioFormat_uBlockAlign_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkAudioFormat_uBlockAlign_set(swigCPtr, value);
		}
	}

	public uint uTypeID
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkAudioFormat_uTypeID_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkAudioFormat_uTypeID_set(swigCPtr, value);
		}
	}

	public uint uInterleaveID
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkAudioFormat_uInterleaveID_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkAudioFormat_uInterleaveID_set(swigCPtr, value);
		}
	}

	internal AkAudioFormat(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkAudioFormat()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkAudioFormat(), cMemoryOwn: true)
	{
	}

	internal static IntPtr getCPtr(AkAudioFormat obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~AkAudioFormat()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkAudioFormat(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public uint GetNumChannels()
	{
		return AkSoundEnginePINVOKE.CSharp_AkAudioFormat_GetNumChannels(swigCPtr);
	}

	public uint GetBitsPerSample()
	{
		return AkSoundEnginePINVOKE.CSharp_AkAudioFormat_GetBitsPerSample(swigCPtr);
	}

	public uint GetBlockAlign()
	{
		return AkSoundEnginePINVOKE.CSharp_AkAudioFormat_GetBlockAlign(swigCPtr);
	}

	public uint GetTypeID()
	{
		return AkSoundEnginePINVOKE.CSharp_AkAudioFormat_GetTypeID(swigCPtr);
	}

	public uint GetInterleaveID()
	{
		return AkSoundEnginePINVOKE.CSharp_AkAudioFormat_GetInterleaveID(swigCPtr);
	}

	public void SetAll(uint in_uSampleRate, AkChannelConfig in_channelConfig, uint in_uBitsPerSample, uint in_uBlockAlign, uint in_uTypeID, uint in_uInterleaveID)
	{
		AkSoundEnginePINVOKE.CSharp_AkAudioFormat_SetAll(swigCPtr, in_uSampleRate, AkChannelConfig.getCPtr(in_channelConfig), in_uBitsPerSample, in_uBlockAlign, in_uTypeID, in_uInterleaveID);
	}

	public bool IsChannelConfigSupported()
	{
		return AkSoundEnginePINVOKE.CSharp_AkAudioFormat_IsChannelConfigSupported(swigCPtr);
	}
}
