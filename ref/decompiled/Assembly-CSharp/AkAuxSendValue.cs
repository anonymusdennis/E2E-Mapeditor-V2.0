using System;

public class AkAuxSendValue : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public uint auxBusID
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_auxBusID_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_auxBusID_set(swigCPtr, value);
		}
	}

	public float fControlValue
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_fControlValue_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_fControlValue_set(swigCPtr, value);
		}
	}

	internal AkAuxSendValue(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkAuxSendValue obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~AkAuxSendValue()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkAuxSendValue(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
