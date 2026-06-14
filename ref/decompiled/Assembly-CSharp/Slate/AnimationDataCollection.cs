using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Slate;

[Serializable]
public class AnimationDataCollection : IAnimatableData
{
	[SerializeField]
	private List<AnimatedParameter> _animatedParameters;

	public List<AnimatedParameter> animatedParameters
	{
		get
		{
			return _animatedParameters;
		}
		private set
		{
			_animatedParameters = value;
		}
	}

	public bool isValid => animatedParameters != null && animatedParameters.Count > 0;

	public AnimationDataCollection()
	{
	}

	public AnimationDataCollection(MemberInfo[] memberInfoParameters, object obj, Transform root)
	{
		foreach (MemberInfo member in memberInfoParameters)
		{
			TryAddParameter(member, obj, root);
		}
	}

	public bool TryAddParameter(MemberInfo member, object obj, Transform root)
	{
		if (animatedParameters == null)
		{
			animatedParameters = new List<AnimatedParameter>();
		}
		AnimatedParameter newParam = new AnimatedParameter(member, obj, root);
		if (newParam.isValid)
		{
			AnimatedParameter animatedParameter = animatedParameters.Find((AnimatedParameter p) => p.CompareTo(newParam));
			if (animatedParameter != null)
			{
				if (animatedParameter.parameterType != newParam.parameterType)
				{
					animatedParameter.ChangeMemberType(newParam.parameterType);
				}
				return false;
			}
		}
		animatedParameters.Add(newParam);
		return true;
	}

	public AnimatedParameter GetParameterOfName(string name)
	{
		if (animatedParameters == null)
		{
			return null;
		}
		return animatedParameters.Find((AnimatedParameter d) => d.parameterName.ToLower() == name.ToLower());
	}

	public AnimationCurve[] GetCurves()
	{
		return Internal_GetCurves(enabledParamsOnly: true);
	}

	public AnimationCurve[] GetCurvesAll()
	{
		return Internal_GetCurves(enabledParamsOnly: false);
	}

	private AnimationCurve[] Internal_GetCurves(bool enabledParamsOnly)
	{
		if (animatedParameters == null)
		{
			return new AnimationCurve[0];
		}
		List<AnimationCurve> list = new List<AnimationCurve>();
		for (int i = 0; i < animatedParameters.Count; i++)
		{
			if (!enabledParamsOnly || animatedParameters[i].enabled)
			{
				AnimationCurve[] curves = animatedParameters[i].GetCurves();
				if (curves != null)
				{
					list.AddRange(curves);
				}
			}
		}
		return list.ToArray();
	}

	public void SetTransformContext(Transform context)
	{
		if (animatedParameters != null)
		{
			for (int i = 0; i < animatedParameters.Count; i++)
			{
				animatedParameters[i].SetTransformContext(context);
			}
		}
	}

	public void SetSnapshot(object obj)
	{
		if (animatedParameters != null)
		{
			for (int i = 0; i < animatedParameters.Count; i++)
			{
				animatedParameters[i].SetSnapshot(obj);
			}
		}
	}

	public void SetEvaluatedValues(object obj, float time, float previousTime)
	{
		if (animatedParameters != null)
		{
			for (int i = 0; i < animatedParameters.Count; i++)
			{
				animatedParameters[i].SetEvaluatedValues(obj, time, previousTime);
			}
		}
	}

	public void RestoreSnapshot(object obj)
	{
		if (animatedParameters != null)
		{
			for (int i = 0; i < animatedParameters.Count; i++)
			{
				animatedParameters[i].RestoreSnapshot(obj);
			}
		}
	}

	public bool TryAutoKey(object obj, float time)
	{
		if (animatedParameters != null)
		{
			bool result = false;
			for (int i = 0; i < animatedParameters.Count; i++)
			{
				if (animatedParameters[i].TryAutoKey(obj, time))
				{
					result = true;
				}
			}
			return result;
		}
		return false;
	}

	public bool TryKeyIdentity(object obj, float time)
	{
		if (animatedParameters != null)
		{
			bool result = false;
			for (int i = 0; i < animatedParameters.Count; i++)
			{
				if (animatedParameters[i].TryKeyIdentity(obj, time))
				{
					result = true;
				}
			}
			return result;
		}
		return false;
	}

	public void RemoveKey(float time)
	{
		if (animatedParameters != null)
		{
			for (int i = 0; i < animatedParameters.Count; i++)
			{
				animatedParameters[i].RemoveKey(time);
			}
		}
	}

	public bool HasChanged(object obj)
	{
		if (animatedParameters != null)
		{
			for (int i = 0; i < animatedParameters.Count; i++)
			{
				if (animatedParameters[i].HasChanged(obj))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasKey(float time)
	{
		if (animatedParameters != null)
		{
			for (int i = 0; i < animatedParameters.Count; i++)
			{
				if (animatedParameters[i].HasKey(time))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasAnyKey()
	{
		if (animatedParameters != null)
		{
			for (int i = 0; i < animatedParameters.Count; i++)
			{
				if (animatedParameters[i].HasAnyKey())
				{
					return true;
				}
			}
		}
		return false;
	}

	public void SetKeyCurrent(object obj, float time)
	{
		if (animatedParameters != null)
		{
			for (int i = 0; i < animatedParameters.Count; i++)
			{
				animatedParameters[i].SetKeyCurrent(obj, time);
			}
		}
	}

	public float GetKeyNext(float time)
	{
		if (animatedParameters != null)
		{
			return (from p in animatedParameters
				select p.GetKeyNext(time) into t
				orderby t
				select t).FirstOrDefault((float t) => t > time);
		}
		return 0f;
	}

	public float GetKeyPrevious(float time)
	{
		if (animatedParameters != null)
		{
			return (from p in animatedParameters
				select p.GetKeyPrevious(time) into t
				orderby t
				select t).LastOrDefault((float t) => t < time);
		}
		return 0f;
	}

	public string GetKeyLabel(float time)
	{
		if (animatedParameters != null && animatedParameters.Count == 1)
		{
			return animatedParameters[0].GetKeyLabel(time);
		}
		return string.Empty;
	}

	public void SetPreWrapMode(WrapMode mode)
	{
		if (animatedParameters != null)
		{
			for (int i = 0; i < animatedParameters.Count; i++)
			{
				animatedParameters[i].SetPreWrapMode(mode);
			}
		}
	}

	public void SetPostWrapMode(WrapMode mode)
	{
		if (animatedParameters != null)
		{
			for (int i = 0; i < animatedParameters.Count; i++)
			{
				animatedParameters[i].SetPostWrapMode(mode);
			}
		}
	}

	public void Reset()
	{
		if (animatedParameters != null)
		{
			for (int i = 0; i < animatedParameters.Count; i++)
			{
				animatedParameters[i].Reset();
			}
		}
	}

	public override string ToString()
	{
		if (animatedParameters == null || animatedParameters.Count == 0)
		{
			return "No Parameters";
		}
		return (animatedParameters.Count != 1) ? "Multiple Parameters" : animatedParameters[0].ToString();
	}
}
