using UnityEngine;

namespace ParadoxNotion;

public static class OperationTools
{
	public static string GetOperationString(OperationMethod om)
	{
		return om switch
		{
			OperationMethod.Set => " = ", 
			OperationMethod.Add => " += ", 
			OperationMethod.Subtract => " -= ", 
			OperationMethod.Multiply => " *= ", 
			OperationMethod.Divide => " /= ", 
			_ => string.Empty, 
		};
	}

	public static float Operate(float a, float b, OperationMethod om)
	{
		return om switch
		{
			OperationMethod.Set => b, 
			OperationMethod.Add => a + b, 
			OperationMethod.Subtract => a - b, 
			OperationMethod.Multiply => a * b, 
			OperationMethod.Divide => a / b, 
			_ => a, 
		};
	}

	public static int Operate(int a, int b, OperationMethod om)
	{
		return om switch
		{
			OperationMethod.Set => b, 
			OperationMethod.Add => a + b, 
			OperationMethod.Subtract => a - b, 
			OperationMethod.Multiply => a * b, 
			OperationMethod.Divide => a / b, 
			_ => a, 
		};
	}

	public static Vector3 Operate(Vector3 a, Vector3 b, OperationMethod om)
	{
		return om switch
		{
			OperationMethod.Set => b, 
			OperationMethod.Add => a + b, 
			OperationMethod.Subtract => a - b, 
			OperationMethod.Multiply => Vector3.Scale(a, b), 
			OperationMethod.Divide => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z), 
			_ => a, 
		};
	}

	public static string GetCompareString(CompareMethod cm)
	{
		return cm switch
		{
			CompareMethod.EqualTo => " == ", 
			CompareMethod.GreaterThan => " > ", 
			CompareMethod.LessThan => " < ", 
			CompareMethod.GreaterOrEqualTo => " >= ", 
			CompareMethod.LessOrEqualTo => " <= ", 
			_ => string.Empty, 
		};
	}

	public static bool Compare(float a, float b, CompareMethod cm, float floatingPoint)
	{
		return cm switch
		{
			CompareMethod.EqualTo => Mathf.Abs(a - b) <= floatingPoint, 
			CompareMethod.GreaterThan => a > b, 
			CompareMethod.LessThan => a < b, 
			CompareMethod.GreaterOrEqualTo => a >= b, 
			CompareMethod.LessOrEqualTo => a <= b, 
			_ => true, 
		};
	}

	public static bool Compare(int a, int b, CompareMethod cm)
	{
		return cm switch
		{
			CompareMethod.EqualTo => a == b, 
			CompareMethod.GreaterThan => a > b, 
			CompareMethod.LessThan => a < b, 
			CompareMethod.GreaterOrEqualTo => a >= b, 
			CompareMethod.LessOrEqualTo => a <= b, 
			_ => true, 
		};
	}
}
