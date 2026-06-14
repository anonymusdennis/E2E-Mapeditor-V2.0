using System;

public class AkCallbackSerializer : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	internal AkCallbackSerializer(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkCallbackSerializer()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkCallbackSerializer(), cMemoryOwn: true)
	{
	}

	internal static IntPtr getCPtr(AkCallbackSerializer obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~AkCallbackSerializer()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkCallbackSerializer(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public static AKRESULT Init(IntPtr in_pMemory, uint in_uSize)
	{
		return (AKRESULT)AkSoundEnginePINVOKE.CSharp_AkCallbackSerializer_Init(in_pMemory, in_uSize);
	}

	public static void Term()
	{
		AkSoundEnginePINVOKE.CSharp_AkCallbackSerializer_Term();
	}

	public static IntPtr Lock()
	{
		return AkSoundEnginePINVOKE.CSharp_AkCallbackSerializer_Lock();
	}

	public static void SetLocalOutput(uint in_uErrorLevel)
	{
		AkSoundEnginePINVOKE.CSharp_AkCallbackSerializer_SetLocalOutput(in_uErrorLevel);
	}

	public static void Unlock()
	{
		AkSoundEnginePINVOKE.CSharp_AkCallbackSerializer_Unlock();
	}

	public static AKRESULT AudioSourceChangeCallbackFunc(bool in_bOtherAudioPlaying, object in_pCookie)
	{
		return (AKRESULT)AkSoundEnginePINVOKE.CSharp_AkCallbackSerializer_AudioSourceChangeCallbackFunc(in_bOtherAudioPlaying, (in_pCookie == null) ? ((IntPtr)0) : ((IntPtr)in_pCookie.GetHashCode()));
	}
}
