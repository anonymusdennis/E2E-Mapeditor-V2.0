using UnityEngine;

namespace UnityStandardAssets.CinematicEffects;

public abstract class DepthOfField : MonoBehaviour
{
	public struct FocusSettings
	{
		public float focusPlane;

		public float range;
	}

	public FocusSettings focus;
}
