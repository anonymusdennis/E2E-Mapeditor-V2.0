using System;

public class _ArrayPoolDefault : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	internal _ArrayPoolDefault(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public _ArrayPoolDefault()
		: this(AkSoundEnginePINVOKE.CSharp_new__ArrayPoolDefault(), cMemoryOwn: true)
	{
	}

	internal static IntPtr getCPtr(_ArrayPoolDefault obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~_ArrayPoolDefault()
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
					AkSoundEnginePINVOKE.CSharp_delete__ArrayPoolDefault(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public static int Get()
	{
		return AkSoundEnginePINVOKE.CSharp__ArrayPoolDefault_Get();
	}
}
