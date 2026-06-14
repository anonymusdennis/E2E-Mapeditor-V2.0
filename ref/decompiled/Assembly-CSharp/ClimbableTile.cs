using UnityEngine;

public class ClimbableTile : MonoBehaviour
{
	public enum ClimbAction
	{
		Invaid,
		Down,
		Up,
		UpAndDown
	}

	public ClimbAction m_ClimbAction = ClimbAction.UpAndDown;
}
