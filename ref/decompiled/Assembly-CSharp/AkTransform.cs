using System;

public class AkTransform : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	internal AkTransform(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkTransform()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkTransform(), cMemoryOwn: true)
	{
	}

	internal static IntPtr getCPtr(AkTransform obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	~AkTransform()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkTransform(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public AkVector Position()
	{
		return new AkVector(AkSoundEnginePINVOKE.CSharp_AkTransform_Position(swigCPtr), cMemoryOwn: false);
	}

	public AkVector OrientationFront()
	{
		return new AkVector(AkSoundEnginePINVOKE.CSharp_AkTransform_OrientationFront(swigCPtr), cMemoryOwn: false);
	}

	public AkVector OrientationTop()
	{
		return new AkVector(AkSoundEnginePINVOKE.CSharp_AkTransform_OrientationTop(swigCPtr), cMemoryOwn: false);
	}

	public void Set(AkVector in_position, AkVector in_orientationFront, AkVector in_orientationTop)
	{
		AkSoundEnginePINVOKE.CSharp_AkTransform_Set__SWIG_0(swigCPtr, AkVector.getCPtr(in_position), AkVector.getCPtr(in_orientationFront), AkVector.getCPtr(in_orientationTop));
	}

	public void Set(float in_positionX, float in_positionY, float in_positionZ, float in_orientFrontX, float in_orientFrontY, float in_orientFrontZ, float in_orientTopX, float in_orientTopY, float in_orientTopZ)
	{
		AkSoundEnginePINVOKE.CSharp_AkTransform_Set__SWIG_1(swigCPtr, in_positionX, in_positionY, in_positionZ, in_orientFrontX, in_orientFrontY, in_orientFrontZ, in_orientTopX, in_orientTopY, in_orientTopZ);
	}

	public void SetPosition(AkVector in_position)
	{
		AkSoundEnginePINVOKE.CSharp_AkTransform_SetPosition__SWIG_0(swigCPtr, AkVector.getCPtr(in_position));
	}

	public void SetPosition(float in_x, float in_y, float in_z)
	{
		AkSoundEnginePINVOKE.CSharp_AkTransform_SetPosition__SWIG_1(swigCPtr, in_x, in_y, in_z);
	}

	public void SetOrientation(AkVector in_orientationFront, AkVector in_orientationTop)
	{
		AkSoundEnginePINVOKE.CSharp_AkTransform_SetOrientation__SWIG_0(swigCPtr, AkVector.getCPtr(in_orientationFront), AkVector.getCPtr(in_orientationTop));
	}

	public void SetOrientation(float in_orientFrontX, float in_orientFrontY, float in_orientFrontZ, float in_orientTopX, float in_orientTopY, float in_orientTopZ)
	{
		AkSoundEnginePINVOKE.CSharp_AkTransform_SetOrientation__SWIG_1(swigCPtr, in_orientFrontX, in_orientFrontY, in_orientFrontZ, in_orientTopX, in_orientTopY, in_orientTopZ);
	}
}
