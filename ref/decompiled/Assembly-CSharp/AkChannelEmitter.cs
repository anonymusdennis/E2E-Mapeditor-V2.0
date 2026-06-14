using System;

public class AkChannelEmitter : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public AkTransform position
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkChannelEmitter_position_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new AkTransform(intPtr, cMemoryOwn: false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkChannelEmitter_position_set(swigCPtr, AkTransform.getCPtr(value));
		}
	}

	public uint uInputChannels
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkChannelEmitter_uInputChannels_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkChannelEmitter_uInputChannels_set(swigCPtr, value);
		}
	}

	internal AkChannelEmitter(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkChannelEmitter()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkChannelEmitter(), cMemoryOwn: true)
	{
	}

	internal static IntPtr getCPtr(AkChannelEmitter obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~AkChannelEmitter()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkChannelEmitter(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
