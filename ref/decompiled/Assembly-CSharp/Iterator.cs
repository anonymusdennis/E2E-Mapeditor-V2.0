using System;

public class Iterator : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public PlaylistItem pItem
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_Iterator_pItem_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new PlaylistItem(intPtr, cMemoryOwn: false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_Iterator_pItem_set(swigCPtr, PlaylistItem.getCPtr(value));
		}
	}

	internal Iterator(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public Iterator()
		: this(AkSoundEnginePINVOKE.CSharp_new_Iterator(), cMemoryOwn: true)
	{
	}

	internal static IntPtr getCPtr(Iterator obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~Iterator()
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
					AkSoundEnginePINVOKE.CSharp_delete_Iterator(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public Iterator NextIter()
	{
		return new Iterator(AkSoundEnginePINVOKE.CSharp_Iterator_NextIter(swigCPtr), cMemoryOwn: false);
	}

	public Iterator PrevIter()
	{
		return new Iterator(AkSoundEnginePINVOKE.CSharp_Iterator_PrevIter(swigCPtr), cMemoryOwn: false);
	}

	public PlaylistItem GetItem()
	{
		return new PlaylistItem(AkSoundEnginePINVOKE.CSharp_Iterator_GetItem(swigCPtr), cMemoryOwn: false);
	}

	public bool IsEqualTo(Iterator in_rOp)
	{
		return AkSoundEnginePINVOKE.CSharp_Iterator_IsEqualTo(swigCPtr, getCPtr(in_rOp));
	}

	public bool IsDifferentFrom(Iterator in_rOp)
	{
		return AkSoundEnginePINVOKE.CSharp_Iterator_IsDifferentFrom(swigCPtr, getCPtr(in_rOp));
	}
}
