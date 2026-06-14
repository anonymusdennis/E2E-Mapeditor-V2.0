using UnityEngine;

public class Direction
{
	public static Vector3 m_vDown = Vector2.down;

	public static Vector3 m_vRight = Vector2.right;

	public static Vector3 m_vUp = Vector2.up;

	public static Vector3 m_vLeft = Vector2.left;

	public static Vector3 m_vDownRight = (Vector2.down + Vector2.right).normalized;

	public static Vector3 m_vUpRight = (Vector2.up + Vector2.right).normalized;

	public static Vector3 m_vDownLeft = (Vector2.down + Vector2.left).normalized;

	public static Vector3 m_vUpLeft = (Vector2.up + Vector2.left).normalized;

	public static Quaternion m_rotationRight = Quaternion.Euler(0f, 90f, 0f);

	public static Quaternion m_rotationDown = Quaternion.Euler(90f, 90f, 0f);

	public static Quaternion m_rotationLeft = Quaternion.Euler(180f, 90f, 0f);

	public static Quaternion m_rotationUp = Quaternion.Euler(270f, 90f, 0f);

	public static Quaternion m_rotationDownRight = Quaternion.Euler(45f, 90f, 0f);

	public static Quaternion m_rotationDownLeft = Quaternion.Euler(135f, 90f, 0f);

	public static Quaternion m_rotationUpLeft = Quaternion.Euler(225f, 90f, 0f);

	public static Quaternion m_rotationUpRight = Quaternion.Euler(315f, 90f, 0f);

	public static Directionx8[] AllDirections = new Directionx8[8]
	{
		Directionx8.Up,
		Directionx8.UpLeft,
		Directionx8.Left,
		Directionx8.DownLeft,
		Directionx8.Down,
		Directionx8.DownRight,
		Directionx8.Right,
		Directionx8.UpRight
	};

	public static Directionx8[] FourDirections = new Directionx8[4]
	{
		Directionx8.Up,
		Directionx8.Left,
		Directionx8.Down,
		Directionx8.Right
	};

	public static Vector3 DirectionToVector(Directionx4 direction)
	{
		return direction switch
		{
			Directionx4.Down => m_vDown, 
			Directionx4.Left => m_vLeft, 
			Directionx4.Up => m_vUp, 
			Directionx4.Right => m_vRight, 
			_ => m_vUp, 
		};
	}

	public static Vector3 DirectionToVector(Directionx8 direction)
	{
		return direction switch
		{
			Directionx8.Up => m_vUp, 
			Directionx8.UpLeft => m_vUpLeft, 
			Directionx8.Left => m_vLeft, 
			Directionx8.DownLeft => m_vDownLeft, 
			Directionx8.Down => m_vDown, 
			Directionx8.DownRight => m_vDownRight, 
			Directionx8.Right => m_vRight, 
			Directionx8.UpRight => m_vUpRight, 
			_ => m_vUp, 
		};
	}

	public static Directionx8 VectorToNearestDirection(Vector2 directionVector)
	{
		return VectorToNearestDirection(directionVector, AllDirections);
	}

	public static Directionx4 VectorToNearestDirectionx4(Vector2 directionVector)
	{
		return (Directionx4)VectorToNearestDirection(directionVector, FourDirections);
	}

	public static Directionx8 VectorToNearestDirection(Vector2 directionVector, Directionx8[] validDirections)
	{
		if (validDirections == null || validDirections.Length == 0)
		{
			return Directionx8.Down;
		}
		float num = float.MaxValue;
		Directionx8 result = validDirections[0];
		Vector2 vector = Vector2.ClampMagnitude(directionVector, 1f);
		for (int i = 0; i < validDirections.Length; i++)
		{
			Vector2 vector2 = DirectionToVector(validDirections[i]);
			float num2 = Vector3.Distance(vector2, vector);
			if (num2 < num)
			{
				num = num2;
				result = validDirections[i];
			}
		}
		return result;
	}

	public static Quaternion DirectionToRotation(Directionx4 direction)
	{
		return direction switch
		{
			Directionx4.Down => m_rotationDown, 
			Directionx4.Left => m_rotationLeft, 
			Directionx4.Up => m_rotationUp, 
			Directionx4.Right => m_rotationRight, 
			_ => Quaternion.identity, 
		};
	}

	public static Quaternion DirectionToRotation(Directionx8 direction)
	{
		return direction switch
		{
			Directionx8.Down => m_rotationDown, 
			Directionx8.Left => m_rotationLeft, 
			Directionx8.Up => m_rotationUp, 
			Directionx8.Right => m_rotationRight, 
			Directionx8.DownRight => m_rotationDownRight, 
			Directionx8.DownLeft => m_rotationDownLeft, 
			Directionx8.UpRight => m_rotationUpRight, 
			Directionx8.UpLeft => m_rotationUpLeft, 
			_ => Quaternion.identity, 
		};
	}
}
