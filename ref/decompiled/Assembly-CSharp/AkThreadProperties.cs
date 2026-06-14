using System;

public class AkThreadProperties : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public int nPriority
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkThreadProperties_nPriority_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkThreadProperties_nPriority_set(swigCPtr, value);
		}
	}

	public uint uStackSize
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkThreadProperties_uStackSize_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkThreadProperties_uStackSize_set(swigCPtr, value);
		}
	}

	public int uSchedPolicy
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkThreadProperties_uSchedPolicy_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkThreadProperties_uSchedPolicy_set(swigCPtr, value);
		}
	}

	public uint dwAffinityMask
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkThreadProperties_dwAffinityMask_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkThreadProperties_dwAffinityMask_set(swigCPtr, value);
		}
	}

	internal AkThreadProperties(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkThreadProperties()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkThreadProperties(), cMemoryOwn: true)
	{
	}

	internal static IntPtr getCPtr(AkThreadProperties obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~AkThreadProperties()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkThreadProperties(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
