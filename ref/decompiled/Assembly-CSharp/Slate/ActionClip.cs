using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Slate;

[Attachable(new Type[] { typeof(ActionTrack) })]
public abstract class ActionClip : MonoBehaviour, IDirectable, IKeyable
{
	[SerializeField]
	[HideInInspector]
	private float _startTime;

	[HideInInspector]
	[SerializeField]
	private AnimationDataCollection _animationData;

	private MemberInfo[] _cachedParamsInfo;

	IEnumerable<IDirectable> IDirectable.children => null;

	public IDirector root => (parent == null) ? null : parent.root;

	public IDirectable parent { get; private set; }

	public GameObject actor => (parent == null) ? null : parent.actor;

	public AnimationDataCollection animationData
	{
		get
		{
			return _animationData;
		}
		private set
		{
			_animationData = value;
		}
	}

	public float startTime
	{
		get
		{
			return _startTime;
		}
		set
		{
			if (_startTime != value)
			{
				_startTime = Mathf.Max(value, 0f);
				blendIn = Mathf.Clamp(blendIn, 0f, length - blendOut);
				blendOut = Mathf.Clamp(blendOut, 0f, length - blendIn);
			}
		}
	}

	public float endTime
	{
		get
		{
			return startTime + length;
		}
		set
		{
			if (startTime + length != value)
			{
				length = Mathf.Max(value - startTime, 0f);
				blendOut = Mathf.Clamp(blendOut, 0f, length - blendIn);
				blendIn = Mathf.Clamp(blendIn, 0f, length - blendOut);
			}
		}
	}

	public bool isActive => parent != null && parent.isActive && isValid;

	public bool isCollapsed => parent != null && parent.isCollapsed;

	public virtual float length
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	public virtual float blendIn
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	public virtual float blendOut
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	public virtual string info
	{
		get
		{
			NameAttribute nameAttribute = GetType().RTGetAttribute<NameAttribute>(inherited: true);
			if (nameAttribute != null)
			{
				return nameAttribute.name;
			}
			return StringExtensions.SplitCamelCase(GetType().Name);
		}
	}

	public virtual bool isValid => actor != null;

	public virtual TransformSpace defaultTransformSpace => TransformSpace.WorldSpace;

	private MemberInfo[] animatedParametersInfo => (_cachedParamsInfo == null) ? (_cachedParamsInfo = (from p in GetType().RTGetPropsAndFields()
		where p.RTGetAttribute<AnimatableParameterAttribute>(inherited: true) != null
		select p).ToArray()) : _cachedParamsInfo;

	private bool handleParametersRegistrationManually => !object.ReferenceEquals(animatedParametersTarget, this);

	public virtual object animatedParametersTarget => this;

	public bool hasParameters => animationData != null && animationData.isValid;

	public bool hasActiveParameters
	{
		get
		{
			if (!hasParameters || !isValid)
			{
				return false;
			}
			for (int i = 0; i < animationData.animatedParameters.Count; i++)
			{
				if (animationData.animatedParameters[i].enabled)
				{
					return true;
				}
			}
			return false;
		}
	}

	bool IDirectable.Initialize()
	{
		return OnInitialize();
	}

	void IDirectable.Enter()
	{
		SetAnimParamsSnapshot();
		OnEnter();
	}

	void IDirectable.Update(float time, float previousTime)
	{
		UpdateAnimParams(time, previousTime);
		OnUpdate(time, previousTime);
	}

	void IDirectable.Exit()
	{
		OnExit();
	}

	void IDirectable.ReverseEnter()
	{
		OnReverseEnter();
	}

	void IDirectable.Reverse()
	{
		RestoreAnimParamsSnapshot();
		OnReverse();
	}

	public virtual bool HasNecessaryComponent()
	{
		return true;
	}

	protected virtual bool OnInitialize()
	{
		return true;
	}

	protected virtual void OnEnter()
	{
	}

	protected virtual void OnUpdate(float time, float previousTime)
	{
		OnUpdate(time);
	}

	protected virtual void OnUpdate(float time)
	{
	}

	protected virtual void OnExit()
	{
	}

	protected virtual void OnReverse()
	{
	}

	protected virtual void OnReverseEnter()
	{
	}

	protected virtual void OnDrawGizmosSelected()
	{
	}

	protected virtual void OnSceneGUI()
	{
	}

	protected virtual void OnCreate()
	{
	}

	protected virtual void OnAfterValidate()
	{
	}

	public void PostCreate(IDirectable parent)
	{
		this.parent = parent;
		CreateAnimationDataCollection();
		OnCreate();
	}

	public void Validate()
	{
		OnAfterValidate();
	}

	public void Validate(IDirector root, IDirectable parent)
	{
		this.parent = parent;
		ValidateAnimParams();
		base.hideFlags = HideFlags.HideInHierarchy;
		OnAfterValidate();
	}

	public bool RootTimeWithinRange()
	{
		return root.currentTime >= startTime && root.currentTime <= endTime && root.currentTime > 0f;
	}

	public Vector3 TransformPoint(Vector3 point, TransformSpace space)
	{
		return (parent == null) ? point : parent.TransformPoint(point, space);
	}

	public Vector3 InverseTransformPoint(Vector3 point, TransformSpace space)
	{
		return (parent == null) ? point : parent.InverseTransformPoint(point, space);
	}

	public Vector3 ActorPositionInSpace(TransformSpace space)
	{
		return (parent == null) ? Vector3.zero : parent.ActorPositionInSpace(space);
	}

	public Transform GetSpaceTransform(TransformSpace space)
	{
		return (parent == null) ? null : parent.GetSpaceTransform(space);
	}

	public float GetClipWeight()
	{
		return GetClipWeight(root.currentTime - startTime);
	}

	public float GetClipWeight(float time)
	{
		return GetClipWeight(time, blendIn, blendOut);
	}

	public float GetClipWeight(float time, float blendInOut)
	{
		return GetClipWeight(time, blendInOut, blendInOut);
	}

	public float GetClipWeight(float time, float blendIn, float blendOut)
	{
		if (time <= 0f)
		{
			return (blendIn == 0f) ? 1 : 0;
		}
		if (time >= length)
		{
			return (blendOut == 0f) ? 1 : 0;
		}
		if (time < blendIn)
		{
			return time / blendIn;
		}
		if (time > length - blendOut)
		{
			return (length - time) / blendOut;
		}
		return 1f;
	}

	public AnimatedParameter GetParameter(string paramName)
	{
		return (animationData == null) ? null : animationData.GetParameterOfName(paramName);
	}

	public void SetParameterEnabled(string paramName, bool enabled)
	{
		GetParameter(paramName)?.SetEnabled(enabled, animatedParametersTarget, root.currentTime - startTime);
	}

	public void ResetAnimatedParameters()
	{
		if (animationData != null)
		{
			animationData.Reset();
		}
	}

	private void CreateAnimationDataCollection()
	{
		if (!handleParametersRegistrationManually && animatedParametersInfo != null && animatedParametersInfo.Length != 0)
		{
			animationData = new AnimationDataCollection(animatedParametersInfo, animatedParametersTarget, null);
		}
	}

	private void ValidateAnimParams()
	{
		if (Application.isPlaying || handleParametersRegistrationManually)
		{
			return;
		}
		if (animatedParametersInfo == null || animatedParametersInfo.Length == 0)
		{
			animationData = null;
			return;
		}
		MemberInfo[] array = animatedParametersInfo;
		foreach (MemberInfo memberInfo in array)
		{
			if (memberInfo != null)
			{
				animationData.TryAddParameter(memberInfo, animatedParametersTarget, null);
			}
		}
		AnimatedParameter[] array2 = animationData.animatedParameters.ToArray();
		foreach (AnimatedParameter animatedParameter in array2)
		{
			if (!animatedParameter.isValid)
			{
				animationData.animatedParameters.Remove(animatedParameter);
			}
			else if (!animatedParametersInfo.Select((MemberInfo m) => m.Name).Contains(animatedParameter.GetMemberInfo().Name))
			{
				animationData.animatedParameters.Remove(animatedParameter);
			}
		}
	}

	private void SetAnimParamsSnapshot()
	{
		if (hasParameters)
		{
			animationData.SetTransformContext(GetSpaceTransform(TransformSpace.CutsceneSpace));
			animationData.SetSnapshot(animatedParametersTarget);
		}
	}

	private void UpdateAnimParams(float time, float previousTime)
	{
		if (hasParameters)
		{
			animationData.SetEvaluatedValues(animatedParametersTarget, time, previousTime);
		}
	}

	private void RestoreAnimParamsSnapshot()
	{
		if (hasParameters)
		{
			animationData.RestoreSnapshot(animatedParametersTarget);
		}
	}

	string IDirectable.get_name()
	{
		return base.name;
	}
}
