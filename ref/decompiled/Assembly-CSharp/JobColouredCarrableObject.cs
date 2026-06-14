using UnityEngine;

public class JobColouredCarrableObject : HazardousCarryableObjectInteraction
{
	[Header("JobColouredCarrableObject")]
	public GameObject Visual_A;

	public GameObject Visual_B;

	public GameObject Visual_C;

	protected override void Awake()
	{
		base.Awake();
		SetTag(m_Tag);
	}

	public override void SetTag(uint newTag)
	{
		base.SetTag(newTag);
		if (Visual_A != null)
		{
			Visual_A.gameObject.SetActive(value: false);
		}
		if (Visual_B != null)
		{
			Visual_B.gameObject.SetActive(value: false);
		}
		if (Visual_C != null)
		{
			Visual_C.gameObject.SetActive(value: false);
		}
		switch (newTag)
		{
		case 1u:
			if (Visual_A != null)
			{
				Visual_A.gameObject.SetActive(value: true);
			}
			break;
		case 2u:
			if (Visual_B != null)
			{
				Visual_B.gameObject.SetActive(value: true);
			}
			break;
		case 3u:
			if (Visual_C != null)
			{
				Visual_C.gameObject.SetActive(value: true);
			}
			break;
		}
		ReassignReferences();
	}
}
