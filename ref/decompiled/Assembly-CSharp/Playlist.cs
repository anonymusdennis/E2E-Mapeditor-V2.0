using System;

public class Playlist : AkPlaylistArray
{
	private IntPtr swigCPtr;

	internal Playlist(IntPtr cPtr, bool cMemoryOwn)
		: base(AkSoundEnginePINVOKE.CSharp_Playlist_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = cPtr;
	}

	public Playlist()
		: this(AkSoundEnginePINVOKE.CSharp_new_Playlist(), cMemoryOwn: true)
	{
	}

	internal static IntPtr getCPtr(Playlist obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~Playlist()
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
					AkSoundEnginePINVOKE.CSharp_delete_Playlist(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public AKRESULT Enqueue(uint in_audioNodeID, int in_msDelay, IntPtr in_pCustomInfo, uint in_cExternals, AkExternalSourceInfo in_pExternalSources)
	{
		return (AKRESULT)AkSoundEnginePINVOKE.CSharp_Playlist_Enqueue__SWIG_0(swigCPtr, in_audioNodeID, in_msDelay, in_pCustomInfo, in_cExternals, AkExternalSourceInfo.getCPtr(in_pExternalSources));
	}

	public AKRESULT Enqueue(uint in_audioNodeID, int in_msDelay, IntPtr in_pCustomInfo, uint in_cExternals)
	{
		return (AKRESULT)AkSoundEnginePINVOKE.CSharp_Playlist_Enqueue__SWIG_1(swigCPtr, in_audioNodeID, in_msDelay, in_pCustomInfo, in_cExternals);
	}

	public AKRESULT Enqueue(uint in_audioNodeID, int in_msDelay, IntPtr in_pCustomInfo)
	{
		return (AKRESULT)AkSoundEnginePINVOKE.CSharp_Playlist_Enqueue__SWIG_2(swigCPtr, in_audioNodeID, in_msDelay, in_pCustomInfo);
	}

	public AKRESULT Enqueue(uint in_audioNodeID, int in_msDelay)
	{
		return (AKRESULT)AkSoundEnginePINVOKE.CSharp_Playlist_Enqueue__SWIG_3(swigCPtr, in_audioNodeID, in_msDelay);
	}

	public AKRESULT Enqueue(uint in_audioNodeID)
	{
		return (AKRESULT)AkSoundEnginePINVOKE.CSharp_Playlist_Enqueue__SWIG_4(swigCPtr, in_audioNodeID);
	}
}
