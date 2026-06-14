using System;

public class WwiseObjectIDext : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public uint id
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_WwiseObjectIDext_id_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_WwiseObjectIDext_id_set(swigCPtr, value);
		}
	}

	public bool bIsBus
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_WwiseObjectIDext_bIsBus_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_WwiseObjectIDext_bIsBus_set(swigCPtr, value);
		}
	}

	internal WwiseObjectIDext(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public WwiseObjectIDext()
		: this(AkSoundEnginePINVOKE.CSharp_new_WwiseObjectIDext(), cMemoryOwn: true)
	{
	}

	internal static IntPtr getCPtr(WwiseObjectIDext obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~WwiseObjectIDext()
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
					AkSoundEnginePINVOKE.CSharp_delete_WwiseObjectIDext(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public bool IsEqualTo(WwiseObjectIDext in_rOther)
	{
		return AkSoundEnginePINVOKE.CSharp_WwiseObjectIDext_IsEqualTo(swigCPtr, getCPtr(in_rOther));
	}

	public AkNodeType GetNodeType()
	{
		return (AkNodeType)AkSoundEnginePINVOKE.CSharp_WwiseObjectIDext_GetNodeType(swigCPtr);
	}
}
