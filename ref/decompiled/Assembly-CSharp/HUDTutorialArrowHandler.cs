using System.Collections.Generic;
using UnityEngine;

public abstract class HUDTutorialArrowHandler : MonoBehaviour
{
	public Vector2 m_ArrowPositionOffset = Vector2.zero;

	[Range(0f, 360f)]
	public float m_ArrowRotation;

	public abstract HUDTutorialArrowController.HUDTutorial GetTutorialType();

	public abstract Transform GetTutorialTargetTransform();

	public abstract bool IsActive();

	public abstract void ClearData();

	public abstract void TutorialInit();

	public abstract void SetTutorialTarget(List<ItemData> targets);
}
