using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterUtil : T17MonoBehaviour
{
	public struct CollisionData
	{
		public Vector3 m_vCollisionPoint;

		public Collider m_Collider;
	}

	public Character m_Character;

	public Vector3 m_vEyeHeight = new Vector3(0f, 0f, -0.2f);

	private LayerMask m_TypicalVisionMask;

	private CollisionData m_LastCollisionData;

	protected override void Awake()
	{
		m_LastCollisionData = default(CollisionData);
		string[] layerNames = new string[4] { "Wall", "AI_Event", "BlockVision", "Door" };
		m_TypicalVisionMask = LayerMask.GetMask(layerNames);
		base.Awake();
	}

	protected virtual void OnDestroy()
	{
		m_Character = null;
	}

	public bool LineOfSight(GameObject objectOfInterest, out bool haveCollisionData, bool useFoV = true)
	{
		if (objectOfInterest == null)
		{
			haveCollisionData = false;
			return false;
		}
		Vector3 position = objectOfInterest.transform.position;
		return LineOfSight(position, out haveCollisionData, objectOfInterest, useFoV);
	}

	public bool LineOfSight(Vector3 toPosition, out bool haveCollisionData, GameObject objectOfInterest = null, bool useFoV = true)
	{
		Vector3 vector = m_Character.m_Transform.position + m_vEyeHeight;
		Vector3 vector2 = toPosition - vector;
		if (Math.Abs(vector2.z) > 1f)
		{
			haveCollisionData = false;
			return false;
		}
		Vector3 normalized = vector2.normalized;
		float distance = Vector3.Distance(vector, toPosition);
		if (m_Character.GetIsSleeping())
		{
			haveCollisionData = false;
			return false;
		}
		useFoV &= m_Character.m_CharacterRole != CharacterRole.Dog;
		if (!InFieldofViewCheck(normalized, distance, useFoV))
		{
			haveCollisionData = false;
			return false;
		}
		return LineOfSight(vector, normalized, distance, out haveCollisionData, objectOfInterest);
	}

	public bool InFieldofViewCheck(Vector2 direction, float distance, bool useFoV = true)
	{
		if (distance < m_Character.m_fTouchingVisionRadius)
		{
			return true;
		}
		if (distance > m_Character.m_fVisionDistance)
		{
			return false;
		}
		if (!useFoV)
		{
			return true;
		}
		Vector2 normalized = m_Character.GetFacingDirection().normalized;
		float f = Mathf.Clamp(Vector2.Dot(direction, normalized), -1f, 1f);
		float num = Mathf.Acos(f);
		return num < m_Character.m_fFoV * 0.5f;
	}

	private bool LineOfSight(Vector3 fromPosition, Vector3 direction, float distance, out bool haveCollisionData, GameObject objectOfInterest = null, Vector2 facingDirection = default(Vector2), float FoV = 0f)
	{
		int num = EscapistsRaycast.RaycastAll(fromPosition, direction, distance, m_TypicalVisionMask, QueryTriggerInteraction.Ignore);
		if (objectOfInterest != null && (m_TypicalVisionMask.value & (1 << objectOfInterest.layer)) > 0)
		{
			SortHitList(fromPosition, num);
		}
		bool result = true;
		haveCollisionData = false;
		for (int i = 0; i < num; i++)
		{
			Collider collider = EscapistsRaycast.RaycastHitList[i].collider;
			if (objectOfInterest != null && collider.gameObject.Equals(objectOfInterest))
			{
				m_LastCollisionData.m_Collider = collider;
				m_LastCollisionData.m_vCollisionPoint = EscapistsRaycast.RaycastHitList[i].point;
				haveCollisionData = true;
				break;
			}
			if (collider.CompareTag("Door"))
			{
				Door component = collider.GetComponent<Door>();
				if (component != null && component.GetDoorOpen())
				{
					continue;
				}
			}
			if (collider.isTrigger)
			{
				continue;
			}
			m_LastCollisionData.m_Collider = collider;
			m_LastCollisionData.m_vCollisionPoint = EscapistsRaycast.RaycastHitList[i].point;
			haveCollisionData = true;
			result = false;
			break;
		}
		return result;
	}

	private static void SortHitList(Vector3 fromPosition, int count)
	{
		RaycastHit[] array = new RaycastHit[count];
		Array.Copy(EscapistsRaycast.RaycastHitList, array, count);
		Array.Sort(array, (RaycastHit c1, RaycastHit c2) => (fromPosition - c1.point).sqrMagnitude.CompareTo((fromPosition - c2.point).sqrMagnitude));
		Array.Copy(array, EscapistsRaycast.RaycastHitList, count);
	}

	public void GetVisionConeVertexPairs(ref List<Vector2> vertexPairs)
	{
		Vector2 normalized = m_Character.GetFacingDirection().normalized;
		float fVisionDistance = m_Character.m_fVisionDistance;
		float fTouchingVisionRadius = m_Character.m_fTouchingVisionRadius;
		float foV = m_Character.m_fFoV;
		if (m_Character.m_CharacterRole == CharacterRole.Dog)
		{
			foV = (float)Math.PI * 2f;
		}
		GetVisionConeVertexPairs(ref vertexPairs, normalized, foV, fVisionDistance, fTouchingVisionRadius);
	}

	public void GetVisionConeVertexPairs(ref List<Vector2> vertexPairs, Vector2 normalisedFacingDirection, float FoV, float maxDistance, float touchingDistance)
	{
		if (vertexPairs == null)
		{
			vertexPairs = new List<Vector2>();
		}
		vertexPairs.Clear();
		float x = normalisedFacingDirection.x;
		float y = normalisedFacingDirection.y;
		float num = 7f;
		for (int i = -(int)num; (float)i < num; i++)
		{
			float f = FoV * 0.5f * ((float)i / num);
			float f2 = FoV * 0.5f * ((float)(i + 1) / num);
			Vector2 vector = new Vector2(x * Mathf.Cos(f) - y * Mathf.Sin(f), x * Mathf.Sin(f) + y * Mathf.Cos(f));
			Vector2 vector2 = new Vector2(x * Mathf.Cos(f2) - y * Mathf.Sin(f2), x * Mathf.Sin(f2) + y * Mathf.Cos(f2));
			vertexPairs.Add(vector * maxDistance);
			vertexPairs.Add(vector2 * maxDistance);
		}
		float f3 = FoV * 0.5f;
		Vector2 vector3 = new Vector2(x * Mathf.Cos(f3) - y * Mathf.Sin(f3), x * Mathf.Sin(f3) + y * Mathf.Cos(f3));
		Vector2 vector4 = new Vector2(x * Mathf.Cos(f3) + y * Mathf.Sin(f3), x * (0f - Mathf.Sin(f3)) + y * Mathf.Cos(f3));
		vertexPairs.Add(Vector2.zero);
		vertexPairs.Add(vector3 * maxDistance);
		vertexPairs.Add(Vector2.zero);
		vertexPairs.Add(vector4 * maxDistance);
		float num2 = 7f;
		for (int j = 0; (float)j < num2; j++)
		{
			float f4 = (float)Math.PI * 2f * ((float)j / num2);
			float f5 = (float)Math.PI * 2f * ((float)(j + 1) / num2);
			Vector2 vector5 = new Vector2(Mathf.Cos(f4) - Mathf.Sin(f4), Mathf.Sin(f4) + Mathf.Cos(f4));
			Vector2 vector6 = new Vector2(Mathf.Cos(f5) - Mathf.Sin(f5), Mathf.Sin(f5) + Mathf.Cos(f5));
			vertexPairs.Add(vector5 * touchingDistance);
			vertexPairs.Add(vector6 * touchingDistance);
		}
	}
}
