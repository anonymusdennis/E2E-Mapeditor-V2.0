using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct StencilInterfaceComparer : IEqualityComparer<StencilInterface>
{
	public bool Equals(StencilInterface x, StencilInterface y)
	{
		int characterListIndex = x.GetCharacterListIndex();
		int characterListIndex2 = y.GetCharacterListIndex();
		if (characterListIndex == -1 || characterListIndex2 == -1)
		{
			return x == y;
		}
		return characterListIndex == characterListIndex2;
	}

	private static int CompareCarry(StencilInterface carrier, StencilInterface carried, StencilInterface other)
	{
		Vector3 cachedCurrentPosition = carrier.GetCachedCurrentPosition();
		Vector3 cachedCurrentPosition2 = carried.GetCachedCurrentPosition();
		float num = ((!(cachedCurrentPosition.y > cachedCurrentPosition2.y)) ? cachedCurrentPosition2.y : cachedCurrentPosition.y);
		if (other.GetCachedCurrentPosition().y > num)
		{
			return -1;
		}
		return 1;
	}

	private static int CompareCarry(StencilInterface x, StencilInterface y)
	{
		if (x.GetPickedUpBy() == y)
		{
			if (y.GetFacingDirectionEnum() == Directionx4.Up)
			{
				return 1;
			}
			return -1;
		}
		if (x.GetCarrying() == y)
		{
			if (x.GetFacingDirectionEnum() == Directionx4.Up)
			{
				return -1;
			}
			return 1;
		}
		if (x.GetPickedUpBy() != null)
		{
			return CompareCarry(x.GetPickedUpBy(), x, y);
		}
		if (x.GetCarrying() != null)
		{
			return CompareCarry(x, x.GetCarrying(), y);
		}
		return 0;
	}

	public static int Compare(StencilInterface x, StencilInterface y, ref int path)
	{
		if (x == null)
		{
			return 1;
		}
		if (y == null)
		{
			return -1;
		}
		if (x == y)
		{
			return 0;
		}
		if (x.GetFloorIndex() != y.GetFloorIndex())
		{
			return -x.GetFloorIndex().CompareTo(y.GetFloorIndex());
		}
		if (x.GetPickedUpBy() != null || x.GetCarrying() != null)
		{
			return CompareCarry(x, y);
		}
		if (y.GetPickedUpBy() != null || y.GetCarrying() != null)
		{
			return -CompareCarry(y, x);
		}
		if (x.GetCachedCurrentPosition().y < y.GetCachedCurrentPosition().y - 0.025f)
		{
			return -1;
		}
		if (x.GetCachedCurrentPosition().y > y.GetCachedCurrentPosition().y + 0.025f)
		{
			return 1;
		}
		return x.GetCharacterID().CompareTo(y.GetCharacterID());
	}

	public int GetHashCode(StencilInterface a)
	{
		int characterListIndex = a.GetCharacterListIndex();
		if (characterListIndex == -1)
		{
			return a.GetHashCode();
		}
		return characterListIndex;
	}
}
