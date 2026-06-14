using System;

public class PlaylistItem : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public uint audioNodeID
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_PlaylistItem_audioNodeID_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_PlaylistItem_audioNodeID_set(swigCPtr, value);
		}
	}

	public int msDelay
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_PlaylistItem_msDelay_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_PlaylistItem_msDelay_set(swigCPtr, value);
		}
	}

	public IntPtr pCustomInfo
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_PlaylistItem_pCustomInfo_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_PlaylistItem_pCustomInfo_set(swigCPtr, value);
		}
	}

	internal PlaylistItem(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public PlaylistItem()
		: this(AkSoundEnginePINVOKE.CSharp_new_PlaylistItem__SWIG_0(), cMemoryOwn: true)
	{
	}

	public PlaylistItem(PlaylistItem in_rCopy)
		: this(AkSoundEnginePINVOKE.CSharp_new_PlaylistItem__SWIG_1(getCPtr(in_rCopy)), cMemoryOwn: true)
	{
	}

	internal static IntPtr getCPtr(PlaylistItem obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~PlaylistItem()
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
					AkSoundEnginePINVOKE.CSharp_delete_PlaylistItem(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public PlaylistItem Assign(PlaylistItem in_rCopy)
	{
		return new PlaylistItem(AkSoundEnginePINVOKE.CSharp_PlaylistItem_Assign(swigCPtr, getCPtr(in_rCopy)), cMemoryOwn: false);
	}

	public bool IsEqualTo(PlaylistItem in_rCopy)
	{
		return AkSoundEnginePINVOKE.CSharp_PlaylistItem_IsEqualTo(swigCPtr, getCPtr(in_rCopy));
	}

	public AKRESULT SetExternalSources(uint in_nExternalSrc, AkExternalSourceInfo in_pExternalSrc)
	{
		return (AKRESULT)AkSoundEnginePINVOKE.CSharp_PlaylistItem_SetExternalSources(swigCPtr, in_nExternalSrc, AkExternalSourceInfo.getCPtr(in_pExternalSrc));
	}
}
