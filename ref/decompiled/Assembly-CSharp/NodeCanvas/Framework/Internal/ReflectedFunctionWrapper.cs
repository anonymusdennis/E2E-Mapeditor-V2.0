using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ParadoxNotion;
using ParadoxNotion.Serialization;

namespace NodeCanvas.Framework.Internal;

public abstract class ReflectedFunctionWrapper : ReflectedWrapper
{
	public new static ReflectedFunctionWrapper Create(MethodInfo method, IBlackboard bb)
	{
		if (method == null)
		{
			return null;
		}
		Type type = null;
		List<Type> list = new List<Type>();
		list.Add(method.ReturnType);
		List<Type> list2 = list;
		ParameterInfo[] parameters = method.GetParameters();
		if (parameters.Length == 0)
		{
			type = typeof(ReflectedFunction<>);
		}
		if (parameters.Length == 1)
		{
			type = typeof(ReflectedFunction<, >);
		}
		if (parameters.Length == 2)
		{
			type = typeof(ReflectedFunction<, , >);
		}
		if (parameters.Length == 3)
		{
			type = typeof(ReflectedFunction<, , , >);
		}
		if (parameters.Length == 4)
		{
			type = typeof(ReflectedFunction<, , , , >);
		}
		if (parameters.Length == 5)
		{
			type = typeof(ReflectedFunction<, , , , , >);
		}
		if (parameters.Length == 6)
		{
			type = typeof(ReflectedFunction<, , , , , , >);
		}
		list2.AddRange(parameters.Select((ParameterInfo p) => p.ParameterType));
		ReflectedFunctionWrapper reflectedFunctionWrapper = (ReflectedFunctionWrapper)Activator.CreateInstance(type.RTMakeGenericType(list2.ToArray()));
		reflectedFunctionWrapper._targetMethod = new SerializedMethodInfo(method);
		BBParameter.SetBBFields(reflectedFunctionWrapper, bb);
		BBParameter[] variables = reflectedFunctionWrapper.GetVariables();
		for (int i = 0; i < parameters.Length; i++)
		{
			ParameterInfo parameterInfo = parameters[i];
			if (parameterInfo.IsOptional)
			{
				variables[i + 1].value = parameterInfo.DefaultValue;
			}
		}
		return reflectedFunctionWrapper;
	}

	public abstract object Call();
}
