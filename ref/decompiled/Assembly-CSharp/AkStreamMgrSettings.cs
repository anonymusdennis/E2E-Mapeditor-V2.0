using System;

public class AkStreamMgrSettings : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public uint uMemorySize
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkStreamMgrSettings_uMemorySize_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkStreamMgrSettings_uMemorySize_set(swigCPtr, value);
		}
	}

	internal AkStreamMgrSettings(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkStreamMgrSettings()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkStreamMgrSettings(), cMemoryOwn: true)
	{
	}

	internal static IntPtr getCPtr(AkStreamMgrSettings obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~AkStreamMgrSettings()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkStreamMgrSettings(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
