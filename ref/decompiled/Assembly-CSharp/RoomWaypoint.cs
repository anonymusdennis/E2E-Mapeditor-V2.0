using UnityEngine;

public class RoomWaypoint : MonoBehaviour
{
	public GameObject m_ChildToRemove;

	public Directionx4 m_FacingDirection;

	public Directionx4 m_WaypointSide;

	public Character m_Reservation;

	public bool m_bReservable = true;

	private Vector3 m_position;

	private bool m_bPositionSet;

	private void Awake()
	{
		if (m_ChildToRemove != null)
		{
			Object.Destroy(m_ChildToRemove);
		}
		m_position = base.transform.position;
		m_bPositionSet = true;
	}

	public Vector3 GetPosition()
	{
		if (!m_bPositionSet)
		{
			m_position = base.transform.position;
		}
		return m_position;
	}
}
