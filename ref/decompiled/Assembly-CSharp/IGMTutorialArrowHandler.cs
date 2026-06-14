using System.Collections.Generic;
using UnityEngine;

public abstract class IGMTutorialArrowHandler : MonoBehaviour
{
	public abstract IGMTutorialArrowController.IGMTutorial GetTutorialType();

	public abstract Transform GetTutorialTargetTransform();

	public virtual Sprite GetOverrideSprite()
	{
		return null;
	}

	public abstract bool IsActive();

	public abstract void ClearData();

	public abstract void TutorialInit();

	public abstract void SetTutorialTarget(List<ItemData> targets);
}
