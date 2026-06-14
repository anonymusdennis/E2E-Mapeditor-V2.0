using UnityEngine;

[CreateAssetMenu(fileName = "OBJECT_InteractAnimData", menuName = "Team17/CreateInteractAnimData")]
public class InteractObjAnimData : ScriptableObject
{
	public AnimState enterAnimation;

	public AnimState interactingAnimation = AnimState.COUNT;

	public AnimState exitAnimation;

	public float duration;

	public float lerpDuration = 0.1f;

	public bool walkWhilstLerping;
}
