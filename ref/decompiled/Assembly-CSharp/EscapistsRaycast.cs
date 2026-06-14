using UnityEngine;

public class EscapistsRaycast
{
	public const int MAX_RAYCAST_ARRAYCOUNT = 32;

	public const int MAX_OVERLAP_ARRAYCOUNT = 32;

	public static RaycastHit[] RaycastHitList = new RaycastHit[32];

	public static Collider[] ColliderOverlapList = new Collider[32];

	public static void Cleanup()
	{
		for (int i = 0; i < RaycastHitList.Length; i++)
		{
			RaycastHitList[i] = default(RaycastHit);
		}
		for (int j = 0; j < ColliderOverlapList.Length; j++)
		{
			ColliderOverlapList[j] = null;
		}
	}

	public static int RaycastAll(Vector3 origin, Vector3 direction)
	{
		int num = Physics.RaycastNonAlloc(origin, direction, RaycastHitList);
		if (num > 32)
		{
			return 32;
		}
		return num;
	}

	public static int RaycastAll(Vector3 origin, Vector3 direction, float distance)
	{
		int num = Physics.RaycastNonAlloc(origin, direction, RaycastHitList, distance);
		if (num > 32)
		{
			return 32;
		}
		return num;
	}

	public static int RaycastAll(Vector3 origin, Vector3 direction, float distance, int layerMask)
	{
		int num = Physics.RaycastNonAlloc(origin, direction, RaycastHitList, distance, layerMask);
		if (num > 32)
		{
			return 32;
		}
		return num;
	}

	public static int RaycastAll(Vector3 origin, Vector3 direction, float distance, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		int num = Physics.RaycastNonAlloc(origin, direction, RaycastHitList, distance, layerMask, queryTriggerInteraction);
		if (num > 32)
		{
			return 32;
		}
		return num;
	}

	public static int OverlapSphereNonAlloc(Vector3 position, float radius, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		int num = Physics.OverlapSphereNonAlloc(position, radius, ColliderOverlapList, layerMask, queryTriggerInteraction);
		if (num > 32)
		{
			return 32;
		}
		return num;
	}

	public static int OverlapBoxNonAlloc(Vector3 center, Vector3 halfExtents, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		int num = Physics.OverlapBoxNonAlloc(center, halfExtents, ColliderOverlapList, Quaternion.identity, layerMask, queryTriggerInteraction);
		if (num > 32)
		{
			return 32;
		}
		return num;
	}

	public static int OverlapBoxNonAlloc(Vector3 center, Vector3 halfExtents, Quaternion orientation, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		int num = Physics.OverlapBoxNonAlloc(center, halfExtents, ColliderOverlapList, orientation, layerMask, queryTriggerInteraction);
		if (num > 32)
		{
			return 32;
		}
		return num;
	}
}
