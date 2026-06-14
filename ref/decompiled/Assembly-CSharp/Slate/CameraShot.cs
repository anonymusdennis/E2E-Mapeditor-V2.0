using System;
using UTJ;
using UnityEngine;

namespace Slate;

[Description("Camera Shots can be keyframed directly within this clip. You don't need to create an Actor Group to animate the shot.")]
[Attachable(new Type[] { typeof(CameraTrack) })]
public class CameraShot : DirectorActionClip
{
	public enum BlendInEffectType
	{
		None,
		FadeIn,
		CrossDissolve,
		EaseIn
	}

	public enum BlendOutEffectType
	{
		None,
		FadeOut
	}

	public enum ShotAnimationMode
	{
		UseInternal,
		UseExternalAnimationClip
	}

	[HideInInspector]
	[SerializeField]
	private float _length = 5f;

	[HideInInspector]
	[SerializeField]
	private float _blendIn = 0.5f;

	[SerializeField]
	[HideInInspector]
	private float _blendOut = 0.5f;

	[HideInInspector]
	[SerializeField]
	private ShotCamera _targetShot;

	[HideInInspector]
	public BlendInEffectType blendInEffect;

	[HideInInspector]
	public BlendOutEffectType blendOutEffect;

	[HideInInspector]
	[Range(0f, 1f)]
	public float steadyCamEffect;

	[HideInInspector]
	public ShotAnimationMode shotAnimationMode;

	[HideInInspector]
	public AnimationClip externalAnimationClip;

	private Color lastColor;

	private Vector3 posOffset;

	private Vector3 rotOffset;

	private float steadyCamTimer;

	private Vector3 targetPosOffset;

	private Vector3 targetRotOffset;

	private Vector3 steadyCamPosVel;

	private Vector3 steadyCamRotVel;

	public CameraShot previousShot { get; private set; }

	public ShotCamera targetShot
	{
		get
		{
			return _targetShot;
		}
		set
		{
			if (_targetShot != value)
			{
				_targetShot = value;
				ResetAnimatedParameters();
			}
		}
	}

	public override string info
	{
		get
		{
			if (targetShot != null && shotAnimationMode == ShotAnimationMode.UseExternalAnimationClip && externalAnimationClip != null)
			{
				return externalAnimationClip.name;
			}
			return (!(targetShot != null)) ? "No Shot Selected" : targetShot.gameObject.name;
		}
	}

	public override bool isValid => targetShot != null;

	public override float length
	{
		get
		{
			return _length;
		}
		set
		{
			_length = value;
		}
	}

	public override float blendIn
	{
		get
		{
			return (blendInEffect == BlendInEffectType.None) ? (-1f) : _blendIn;
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
			return (blendOutEffect == BlendOutEffectType.None) ? (-1f) : _blendOut;
		}
		set
		{
			_blendOut = value;
		}
	}

	public new GameObject actor => (!targetShot) ? base.actor : targetShot.gameObject;

	private CameraTrack track => (CameraTrack)base.parent;

	[AnimatableParameter]
	public Vector3 position
	{
		get
		{
			return (!targetShot) ? Vector3.zero : targetShot.localPosition;
		}
		set
		{
			if (targetShot != null)
			{
				targetShot.localPosition = value;
			}
		}
	}

	[AnimatableParameter]
	public Vector3 rotation
	{
		get
		{
			return (!targetShot) ? Vector3.zero : targetShot.localEulerAngles;
		}
		set
		{
			if (targetShot != null)
			{
				targetShot.localEulerAngles = value;
			}
		}
	}

	[AnimatableParameter(0.01f, 170f)]
	public float fieldOfView
	{
		get
		{
			return (!targetShot) ? 60f : targetShot.fieldOfView;
		}
		set
		{
			if (targetShot != null)
			{
				targetShot.fieldOfView = Mathf.Clamp(value, 0.01f, 170f);
			}
		}
	}

	[AnimatableParameter]
	public float focalPoint
	{
		get
		{
			return (!targetShot) ? 10f : targetShot.focalPoint;
		}
		set
		{
			if (targetShot != null)
			{
				targetShot.focalPoint = Mathf.Max(value, 0f);
			}
		}
	}

	[AnimatableParameter]
	public float focalRange
	{
		get
		{
			return (!targetShot) ? 15f : targetShot.focalRange;
		}
		set
		{
			if (targetShot != null)
			{
				targetShot.focalRange = Mathf.Max(value, 0f);
			}
		}
	}

	protected override void OnAfterValidate()
	{
		bool flag = false;
		flag = targetShot != null && targetShot.gameObject.GetComponent<AlembicCamera>() != null;
		bool flag2 = shotAnimationMode == ShotAnimationMode.UseExternalAnimationClip && externalAnimationClip != null;
		SetParameterEnabled("position", !flag2 && !flag);
		SetParameterEnabled("rotation", !flag2 && !flag);
		SetParameterEnabled("fieldOfView", !flag2 && !flag);
	}

	protected override void OnEnter()
	{
		previousShot = track.currentShot;
		track.currentShot = this;
		lastColor = DirectorGUI.fadeColor;
		DirectorGUI.UpdateFade(Color.clear);
	}

	protected override void OnUpdate(float time, float previousTime)
	{
		if (time != previousTime && steadyCamEffect > 0f)
		{
			float num = Mathf.Lerp(0.2f, 0.4f, steadyCamEffect);
			float num2 = Mathf.Lerp(5f, 10f, steadyCamEffect);
			float smoothTime = Mathf.Lerp(3f, 1f, steadyCamEffect);
			if (steadyCamTimer <= 0f)
			{
				steadyCamTimer = UnityEngine.Random.Range(0.2f, 0.3f);
				targetPosOffset = UnityEngine.Random.insideUnitSphere * num;
				targetRotOffset = UnityEngine.Random.insideUnitSphere * num2;
			}
			steadyCamTimer -= UpdateManager.deltaTime;
			posOffset = Vector3.SmoothDamp(posOffset, targetPosOffset, ref steadyCamPosVel, smoothTime);
			rotOffset = Vector3.SmoothDamp(rotOffset, targetRotOffset, ref steadyCamRotVel, smoothTime);
			DirectorCamera.renderCamera.transform.localPosition = Vector3.Lerp(Vector3.zero, posOffset, GetClipWeight(time, 1f));
			DirectorCamera.renderCamera.transform.localEulerAngles = Vector3.Lerp(Vector3.zero, rotOffset, GetClipWeight(time, 1f));
		}
		if (shotAnimationMode == ShotAnimationMode.UseExternalAnimationClip && externalAnimationClip != null)
		{
			externalAnimationClip.SampleAnimation(targetShot.gameObject, time);
		}
		if (blendInEffect == BlendInEffectType.FadeIn)
		{
			if (time <= blendIn)
			{
				Color black = Color.black;
				black.a = Easing.Ease(EaseType.QuadraticInOut, 1f, 0f, GetClipWeight(time));
				DirectorGUI.UpdateFade(black);
			}
			else if (time < length - blendOut)
			{
				DirectorGUI.UpdateFade(Color.clear);
			}
		}
		if (blendOutEffect == BlendOutEffectType.FadeOut)
		{
			if (time >= length - blendOut)
			{
				Color black2 = Color.black;
				black2.a = Easing.Ease(EaseType.QuadraticInOut, 1f, 0f, GetClipWeight(time));
				DirectorGUI.UpdateFade(black2);
			}
			else if (time > blendIn)
			{
				DirectorGUI.UpdateFade(Color.clear);
			}
		}
		if (blendInEffect == BlendInEffectType.CrossDissolve && previousShot != null && previousShot.targetShot != null)
		{
			if (time <= blendIn)
			{
				Vector2 vector = new Vector2(Screen.width, Screen.height);
				RenderTexture renderTexture = previousShot.targetShot.GetRenderTexture((int)vector.x, (int)vector.y);
				float completion = Easing.Ease(EaseType.QuadraticInOut, 0f, 1f, GetClipWeight(time));
				DirectorGUI.UpdateDissolve(renderTexture, completion);
			}
			else
			{
				DirectorGUI.UpdateDissolve(null, 0f);
			}
		}
	}

	protected override void OnReverse()
	{
		DirectorGUI.UpdateFade(lastColor);
		DirectorGUI.UpdateDissolve(null, 0f);
		track.currentShot = previousShot;
	}
}
