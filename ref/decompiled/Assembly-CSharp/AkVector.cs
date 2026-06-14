using System;

public class AkVector : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public float X
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkVector_X_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkVector_X_set(swigCPtr, value);
		}
	}

	public float Y
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkVector_Y_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkVector_Y_set(swigCPtr, value);
		}
	}

	public float Z
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkVector_Z_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkVector_Z_set(swigCPtr, value);
		}
	}

	internal AkVector(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkVector()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkVector(), cMemoryOwn: true)
	{
	}

	internal static IntPtr getCPtr(AkVector obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~AkVector()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkVector(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
