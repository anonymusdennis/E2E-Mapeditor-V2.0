using System;

public class EnvelopePoint : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public uint uPosition
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_EnvelopePoint_uPosition_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_EnvelopePoint_uPosition_set(swigCPtr, value);
		}
	}

	public ushort uAttenuation
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_EnvelopePoint_uAttenuation_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_EnvelopePoint_uAttenuation_set(swigCPtr, value);
		}
	}

	internal EnvelopePoint(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public EnvelopePoint()
		: this(AkSoundEnginePINVOKE.CSharp_new_EnvelopePoint(), cMemoryOwn: true)
	{
	}

	internal static IntPtr getCPtr(EnvelopePoint obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~EnvelopePoint()
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
					AkSoundEnginePINVOKE.CSharp_delete_EnvelopePoint(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
