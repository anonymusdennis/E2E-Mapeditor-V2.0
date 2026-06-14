using UnityEngine;

public class LevelEditor_AutoScale : MonoBehaviour
{
	public enum ScaleWhat
	{
		Scale_X,
		Scale_Y
	}

	public float m_MinScale = 1f;

	public float m_MaxScale = 1f;

	public ScaleWhat m_Scale;

	private void Start()
	{
		if (LevelEditor_Controller.GetInstance() != null)
		{
			LevelEditor_Controller.GetInstance().RegisterZoomChange(ZoomLevelChanged);
		}
		else
		{
			base.enabled = false;
		}
	}

	public void ZoomLevelChanged(float fLevel)
	{
		Vector3 localScale = base.transform.localScale;
		float num = Mathf.Lerp(m_MinScale, m_MaxScale, fLevel);
		switch (m_Scale)
		{
		case ScaleWhat.Scale_X:
			localScale.x = num;
			break;
		case ScaleWhat.Scale_Y:
			localScale.y = num;
			break;
		}
		base.transform.localScale = localScale;
	}
}
