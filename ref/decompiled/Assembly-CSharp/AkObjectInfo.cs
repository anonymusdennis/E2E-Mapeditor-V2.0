using System;

public class AkObjectInfo : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public uint objID
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkObjectInfo_objID_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkObjectInfo_objID_set(swigCPtr, value);
		}
	}

	public uint parentID
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkObjectInfo_parentID_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkObjectInfo_parentID_set(swigCPtr, value);
		}
	}

	public int iDepth
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkObjectInfo_iDepth_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkObjectInfo_iDepth_set(swigCPtr, value);
		}
	}

	internal AkObjectInfo(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkObjectInfo()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkObjectInfo(), cMemoryOwn: true)
	{
	}

	internal static IntPtr getCPtr(AkObjectInfo obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~AkObjectInfo()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkObjectInfo(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
