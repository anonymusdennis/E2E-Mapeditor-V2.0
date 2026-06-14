using UnityEngine;

public class LevelSetup_ShiftZOffsetForPosition : BaseComponentSetup
{
	public float m_MinPossibleZAddition;

	public float m_MaxPossbileZAddition = 0.025f;

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_8;
	}

	public override SetupReturnState Setup()
	{
		Vector3 position = base.transform.position;
		float num = Mathf.Abs(position.x % 10f);
		float num2 = num * 0.1f;
		if (position.x < 0f)
		{
			num2 = 1f - num2;
		}
		float num3 = Mathf.Lerp(m_MinPossibleZAddition, m_MaxPossbileZAddition, num2);
		position.z += num3;
		base.transform.position = position;
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		return Setup();
	}
}
