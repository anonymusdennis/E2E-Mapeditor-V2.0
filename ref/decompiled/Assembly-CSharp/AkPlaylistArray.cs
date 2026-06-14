using System;

public class AkPlaylistArray : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	internal AkPlaylistArray(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkPlaylistArray()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkPlaylistArray(), cMemoryOwn: true)
	{
	}

	internal static IntPtr getCPtr(AkPlaylistArray obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~AkPlaylistArray()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkPlaylistArray(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public Iterator Begin()
	{
		return new Iterator(AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Begin(swigCPtr), cMemoryOwn: true);
	}

	public Iterator End()
	{
		return new Iterator(AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_End(swigCPtr), cMemoryOwn: true);
	}

	public Iterator FindEx(PlaylistItem in_Item)
	{
		return new Iterator(AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_FindEx(swigCPtr, PlaylistItem.getCPtr(in_Item)), cMemoryOwn: true);
	}

	public Iterator Erase(Iterator in_rIter)
	{
		return new Iterator(AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Erase__SWIG_0(swigCPtr, Iterator.getCPtr(in_rIter)), cMemoryOwn: true);
	}

	public void Erase(uint in_uIndex)
	{
		AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Erase__SWIG_1(swigCPtr, in_uIndex);
	}

	public Iterator EraseSwap(Iterator in_rIter)
	{
		return new Iterator(AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_EraseSwap(swigCPtr, Iterator.getCPtr(in_rIter)), cMemoryOwn: true);
	}

	public AKRESULT Reserve(uint in_ulReserve)
	{
		return (AKRESULT)AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Reserve(swigCPtr, in_ulReserve);
	}

	public uint Reserved()
	{
		return AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Reserved(swigCPtr);
	}

	public void Term()
	{
		AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Term(swigCPtr);
	}

	public uint Length()
	{
		return AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Length(swigCPtr);
	}

	public bool IsEmpty()
	{
		return AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_IsEmpty(swigCPtr);
	}

	public PlaylistItem Exists(PlaylistItem in_Item)
	{
		IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Exists(swigCPtr, PlaylistItem.getCPtr(in_Item));
		return (!(intPtr == IntPtr.Zero)) ? new PlaylistItem(intPtr, cMemoryOwn: false) : null;
	}

	public PlaylistItem AddLast()
	{
		IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_AddLast__SWIG_0(swigCPtr);
		return (!(intPtr == IntPtr.Zero)) ? new PlaylistItem(intPtr, cMemoryOwn: false) : null;
	}

	public PlaylistItem AddLast(PlaylistItem in_rItem)
	{
		IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_AddLast__SWIG_1(swigCPtr, PlaylistItem.getCPtr(in_rItem));
		return (!(intPtr == IntPtr.Zero)) ? new PlaylistItem(intPtr, cMemoryOwn: false) : null;
	}

	public PlaylistItem Last()
	{
		return new PlaylistItem(AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Last(swigCPtr), cMemoryOwn: false);
	}

	public void RemoveLast()
	{
		AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_RemoveLast(swigCPtr);
	}

	public AKRESULT Remove(PlaylistItem in_rItem)
	{
		return (AKRESULT)AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Remove(swigCPtr, PlaylistItem.getCPtr(in_rItem));
	}

	public AKRESULT RemoveSwap(PlaylistItem in_rItem)
	{
		return (AKRESULT)AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_RemoveSwap(swigCPtr, PlaylistItem.getCPtr(in_rItem));
	}

	public void RemoveAll()
	{
		AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_RemoveAll(swigCPtr);
	}

	public PlaylistItem ItemAtIndex(uint uiIndex)
	{
		return new PlaylistItem(AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_ItemAtIndex(swigCPtr, uiIndex), cMemoryOwn: false);
	}

	public PlaylistItem Insert(uint in_uIndex)
	{
		IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Insert(swigCPtr, in_uIndex);
		return (!(intPtr == IntPtr.Zero)) ? new PlaylistItem(intPtr, cMemoryOwn: false) : null;
	}

	public bool GrowArray(uint in_uGrowBy)
	{
		return AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_GrowArray__SWIG_0(swigCPtr, in_uGrowBy);
	}

	public bool GrowArray()
	{
		return AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_GrowArray__SWIG_1(swigCPtr);
	}

	public bool Resize(uint in_uiSize)
	{
		return AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Resize(swigCPtr, in_uiSize);
	}

	public void Transfer(AkPlaylistArray in_rSource)
	{
		AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Transfer(swigCPtr, getCPtr(in_rSource));
	}
}
