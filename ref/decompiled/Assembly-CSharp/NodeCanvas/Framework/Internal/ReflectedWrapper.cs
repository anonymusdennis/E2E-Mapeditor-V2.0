using System.Reflection;
using ParadoxNotion.Serialization;
using UnityEngine;

namespace NodeCanvas.Framework.Internal;

public abstract class ReflectedWrapper
{
	protected delegate void ActionCall();

	protected delegate void ActionCall<T1>(T1 a);

	protected delegate void ActionCall<T1, T2>(T1 a, T2 b);

	protected delegate void ActionCall<T1, T2, T3>(T1 a, T2 b, T3 c);

	protected delegate void ActionCall<T1, T2, T3, T4>(T1 a, T2 b, T3 c, T4 d);

	protected delegate void ActionCall<T1, T2, T3, T4, T5>(T1 a, T2 b, T3 c, T4 d, T5 e);

	protected delegate void ActionCall<T1, T2, T3, T4, T5, T6>(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f);

	protected delegate TResult FunctionCall<TResult>();

	protected delegate TResult FunctionCall<T1, TResult>(T1 a);

	protected delegate TResult FunctionCall<T1, T2, TResult>(T1 a, T2 b);

	protected delegate TResult FunctionCall<T1, T2, T3, TResult>(T1 a, T2 b, T3 c);

	protected delegate TResult FunctionCall<T1, T2, T3, T4, TResult>(T1 a, T2 b, T3 c, T4 d);

	protected delegate TResult FunctionCall<T1, T2, T3, T4, T5, TResult>(T1 a, T2 b, T3 c, T4 d, T5 e);

	protected delegate TResult FunctionCall<T1, T2, T3, T4, T5, T6, TResult>(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f);

	[SerializeField]
	protected SerializedMethodInfo _targetMethod;

	public ReflectedWrapper()
	{
	}

	public static ReflectedWrapper Create(MethodInfo method, IBlackboard bb)
	{
		if (method == null)
		{
			return null;
		}
		if (method.ReturnType == typeof(void))
		{
			return ReflectedActionWrapper.Create(method, bb);
		}
		return ReflectedFunctionWrapper.Create(method, bb);
	}

	public void SetVariablesBB(IBlackboard bb)
	{
		BBParameter[] variables = GetVariables();
		foreach (BBParameter bBParameter in variables)
		{
			bBParameter.bb = bb;
		}
	}

	public bool HasChanged()
	{
		return _targetMethod != null && _targetMethod.HasChanged();
	}

	public MethodInfo GetMethod()
	{
		return (_targetMethod == null) ? null : _targetMethod.Get();
	}

	public string GetMethodString()
	{
		return (_targetMethod == null) ? null : _targetMethod.GetMethodString();
	}

	public abstract BBParameter[] GetVariables();

	public abstract void Init(object instance);
}
