using System;
using System.Linq;
using UnityEngine;

namespace Slate;

[UniqueElement]
[Icon("Camera Icon")]
[Attachable(new Type[] { typeof(DirectorGroup) })]
[Description("The Camera Track is the track within wich you create your camera shots and moves. Once the Camera Track becomes active, the Director Camera will be enabled. You can control when the Director Camera takes effect by setting the 'Active Time Offset', while the Blend In/Out parameters control the ammount of blending there will be from the game camera to the first and the last shot of the track. If you don't want a cinematic letterbox effect, you can set it's time to 0.")]
public class CameraTrack : CutsceneTrack
{
	private static CameraTrack activeCameraTrack;

	[SerializeField]
	[HideInInspector]
	private float _startTimeOffset;

	[HideInInspector]
	[SerializeField]
	private float _endTimeOffset;

	[HideInInspector]
	public float _blendIn;

	[HideInInspector]
	public float _blendOut;

	[HideInInspector]
	public EaseType interpolation = EaseType.QuarticInOut;

	[HideInInspector]
	public float cineBoxFadeTime = 0.5f;

	[HideInInspector]
	public float appliedSmoothing;

	[HideInInspector]
	public Camera exitCameraOverride;

	private GameCamera entryCamera;

	public CameraShot firstShot { get; private set; }

	public CameraShot lastShot { get; private set; }

	public CameraShot currentShot { get; set; }

	public override string info => $"Blend In {_blendIn.ToString()} / Out {_blendOut.ToString()}";

	public override float startTime
	{
		get
		{
			return _startTimeOffset;
		}
		set
		{
			_startTimeOffset = Mathf.Clamp(value, 0f, base.parent.endTime / 2f);
		}
	}

	public override float endTime
	{
		get
		{
			return base.parent.endTime - _endTimeOffset;
		}
		set
		{
			_endTimeOffset = Mathf.Clamp(base.parent.endTime - value, 0f, base.parent.endTime / 2f);
		}
	}

	public override float blendIn
	{
		get
		{
			if (_blendIn == 0f)
			{
				return 0f;
			}
			return (!(firstShot != null)) ? 0f : (firstShot.startTime - startTime + _blendIn);
		}
		set
		{
			_blendIn = value;
		}
	}

	public override float blendOut
	{
		get
		{
			if (_blendOut == 0f)
			{
				return 0f;
			}
			return (!(lastShot != null)) ? 0f : (endTime - lastShot.endTime + _blendOut);
		}
		set
		{
			_blendOut = value;
		}
	}

	protected override void OnEnter()
	{
		if (!(activeCameraTrack != null))
		{
			activeCameraTrack = this;
			firstShot = (CameraShot)base.actions.FirstOrDefault((ActionClip s) => s.startTime >= startTime);
			lastShot = (CameraShot)base.actions.LastOrDefault((ActionClip s) => s.endTime <= endTime);
			currentShot = firstShot;
			DirectorCamera.Enable();
		}
	}

	protected override void OnUpdate(float time, float previousTime)
	{
		if (activeCameraTrack != this)
		{
			return;
		}
		if (cineBoxFadeTime > 0f)
		{
			if (time <= cineBoxFadeTime)
			{
				DirectorGUI.UpdateLetterbox(time / cineBoxFadeTime);
			}
			else if (time >= endTime - startTime - cineBoxFadeTime)
			{
				DirectorGUI.UpdateLetterbox((endTime - startTime - time) / cineBoxFadeTime);
			}
			else
			{
				DirectorGUI.UpdateLetterbox(1f);
			}
		}
		else
		{
			DirectorGUI.UpdateLetterbox(0f);
		}
		if (exitCameraOverride != null)
		{
			if (time > blendIn && entryCamera == null)
			{
				entryCamera = DirectorCamera.gameCamera;
				GameCamera gameCamera = exitCameraOverride.GetComponent<GameCamera>();
				if (gameCamera == null)
				{
					gameCamera = exitCameraOverride.gameObject.AddComponent<GameCamera>();
				}
				DirectorCamera.gameCamera = gameCamera;
			}
			if (time <= blendIn && entryCamera != null)
			{
				DirectorCamera.gameCamera = entryCamera;
				entryCamera = null;
			}
		}
		float num = GetTrackWeight(time);
		IDirectableCamera source = null;
		IDirectableCamera target = null;
		if (currentShot != null && currentShot.targetShot != null)
		{
			target = currentShot.targetShot;
			if (currentShot.blendInEffect == CameraShot.BlendInEffectType.EaseIn && currentShot != firstShot && time < lastShot.startTime + lastShot.blendIn - startTime)
			{
				num *= currentShot.GetClipWeight(time - currentShot.startTime + startTime) * num;
				source = currentShot.previousShot.targetShot;
			}
		}
		DirectorCamera.Update(source, target, interpolation, num, appliedSmoothing);
	}

	protected override void OnExit()
	{
		if (activeCameraTrack == this)
		{
			activeCameraTrack = null;
			DirectorCamera.Disable();
		}
	}

	protected override void OnReverseEnter()
	{
		if (activeCameraTrack == null)
		{
			activeCameraTrack = this;
			DirectorCamera.Enable();
		}
	}

	protected override void OnReverse()
	{
		if (activeCameraTrack == this)
		{
			activeCameraTrack = null;
			DirectorCamera.Disable();
		}
	}
}
