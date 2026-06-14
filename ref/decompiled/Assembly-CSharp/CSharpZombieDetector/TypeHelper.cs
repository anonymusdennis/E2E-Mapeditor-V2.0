using System;
using UnityEngine;

namespace CSharpZombieDetector;

public class TypeHelper
{
	public static bool IsType(string typeName)
	{
		Type type = null;
		try
		{
			type = Type.GetType(typeName);
			if (type == null)
			{
				Debug.LogErrorFormat("Type not found: {0}", typeName);
				return false;
			}
		}
		catch (Exception ex)
		{
			Debug.LogErrorFormat("Type name not qualified: {0}\n {1}", typeName, ex);
			return false;
		}
		return true;
	}

	public static bool IsZombieType(Type type)
	{
		if (type == null)
		{
			return false;
		}
		switch (Type.GetTypeCode(type))
		{
		case TypeCode.Boolean:
		case TypeCode.Char:
		case TypeCode.SByte:
		case TypeCode.Byte:
		case TypeCode.Int16:
		case TypeCode.UInt16:
		case TypeCode.Int32:
		case TypeCode.UInt32:
		case TypeCode.Int64:
		case TypeCode.UInt64:
		case TypeCode.Single:
		case TypeCode.Double:
		case TypeCode.Decimal:
		case TypeCode.String:
			return false;
		case TypeCode.Object:
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				return IsZombieType(Nullable.GetUnderlyingType(type));
			}
			return true;
		default:
			return false;
		}
	}

	public static void TestIsZombieType()
	{
		DateTime? dateTime = new DateTime(2009, 1, 1);
		bool? flag = true;
		char? c = ' ';
		uint num = 2u;
		byte? b = 12;
		decimal? num2 = 12.2m;
		double? num3 = 12.32;
		short? num4 = 12;
		short? num5 = 12;
		short? num6 = 12;
		sbyte? b2 = 12;
		float? num7 = 3.2f;
		ushort? num8 = 12;
		ushort? num9 = 12;
		ushort? num10 = 12;
		Debug.Log("Finished ZombieType Test");
	}
}
