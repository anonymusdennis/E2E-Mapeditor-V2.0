using System;

public class AkMIDIPost : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public uint uOffset
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMIDIPost_uOffset_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkMIDIPost_uOffset_set(swigCPtr, value);
		}
	}

	internal AkMIDIPost(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkMIDIPost()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkMIDIPost(), cMemoryOwn: true)
	{
	}

	internal static IntPtr getCPtr(AkMIDIPost obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~AkMIDIPost()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkMIDIPost(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
