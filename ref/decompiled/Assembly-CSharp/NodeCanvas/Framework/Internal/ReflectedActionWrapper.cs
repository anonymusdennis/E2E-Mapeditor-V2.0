using System;
using System.Linq;
using System.Reflection;
using ParadoxNotion;
using ParadoxNotion.Serialization;

namespace NodeCanvas.Framework.Internal;

public abstract class ReflectedActionWrapper : ReflectedWrapper
{
	public new static ReflectedActionWrapper Create(MethodInfo method, IBlackboard bb)
	{
		if (method == null)
		{
			return null;
		}
		Type type = null;
		Type[] array = null;
		ParameterInfo[] parameters = method.GetParameters();
		if (parameters.Length == 0)
		{
			type = typeof(ReflectedAction);
		}
		if (parameters.Length == 1)
		{
			type = typeof(ReflectedAction<>);
		}
		if (parameters.Length == 2)
		{
			type = typeof(ReflectedAction<, >);
		}
		if (parameters.Length == 3)
		{
			type = typeof(ReflectedAction<, , >);
		}
		if (parameters.Length == 4)
		{
			type = typeof(ReflectedAction<, , , >);
		}
		if (parameters.Length == 5)
		{
			type = typeof(ReflectedAction<, , , , >);
		}
		if (parameters.Length == 6)
		{
			type = typeof(ReflectedAction<, , , , , >);
		}
		array = parameters.Select((ParameterInfo p) => p.ParameterType).ToArray();
		ReflectedActionWrapper reflectedActionWrapper = (ReflectedActionWrapper)Activator.CreateInstance((array.Length <= 0) ? type : type.RTMakeGenericType(array));
		reflectedActionWrapper._targetMethod = new SerializedMethodInfo(method);
		BBParameter.SetBBFields(reflectedActionWrapper, bb);
		BBParameter[] variables = reflectedActionWrapper.GetVariables();
		for (int i = 0; i < parameters.Length; i++)
		{
			ParameterInfo parameterInfo = parameters[i];
			if (parameterInfo.IsOptional)
			{
				variables[i].value = parameterInfo.DefaultValue;
			}
		}
		return reflectedActionWrapper;
	}

	public abstract void Call();
}
