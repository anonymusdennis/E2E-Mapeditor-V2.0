using UnityEngine;

public class RoomMarker : MonoBehaviour
{
	public enum MarkerType
	{
		Normal,
		Escape,
		RollCall,
		Interactive,
		KeepClearSpot,
		BlockEscape
	}

	public MarkerType m_Type;
}
