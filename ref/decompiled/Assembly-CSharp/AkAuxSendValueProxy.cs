using System;

public class AkAuxSendValueProxy : AkAuxSendValue
{
	private IntPtr swigCPtr;

	internal AkAuxSendValueProxy(IntPtr cPtr, bool cMemoryOwn)
		: base(AkSoundEnginePINVOKE.CSharp_AkAuxSendValueProxy_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkAuxSendValueProxy obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~AkAuxSendValueProxy()
	{
		Dispose();
	}

	public override void Dispose()
	{
		lock (this)
		{
			if (swigCPtr != IntPtr.Zero)
			{
				if (swigCMemOwn)
				{
					swigCMemOwn = false;
					AkSoundEnginePINVOKE.CSharp_delete_AkAuxSendValueProxy(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public void set(uint id, float value)
	{
		AkSoundEnginePINVOKE.CSharp_AkAuxSendValueProxy_set(swigCPtr, id, value);
	}
}
