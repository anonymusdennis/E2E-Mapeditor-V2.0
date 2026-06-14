using System;

public class WwiseObjectID : WwiseObjectIDext
{
	private IntPtr swigCPtr;

	internal WwiseObjectID(IntPtr cPtr, bool cMemoryOwn)
		: base(AkSoundEnginePINVOKE.CSharp_WwiseObjectID_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = cPtr;
	}

	public WwiseObjectID()
		: this(AkSoundEnginePINVOKE.CSharp_new_WwiseObjectID__SWIG_0(), cMemoryOwn: true)
	{
	}

	public WwiseObjectID(uint in_ID)
		: this(AkSoundEnginePINVOKE.CSharp_new_WwiseObjectID__SWIG_1(in_ID), cMemoryOwn: true)
	{
	}

	public WwiseObjectID(uint in_ID, bool in_bIsBus)
		: this(AkSoundEnginePINVOKE.CSharp_new_WwiseObjectID__SWIG_2(in_ID, in_bIsBus), cMemoryOwn: true)
	{
	}

	public WwiseObjectID(uint in_ID, AkNodeType in_eNodeType)
		: this(AkSoundEnginePINVOKE.CSharp_new_WwiseObjectID__SWIG_3(in_ID, (int)in_eNodeType), cMemoryOwn: true)
	{
	}

	internal static IntPtr getCPtr(WwiseObjectID obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~WwiseObjectID()
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
					AkSoundEnginePINVOKE.CSharp_delete_WwiseObjectID(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}
}
