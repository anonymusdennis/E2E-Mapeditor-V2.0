using UnityEngine;

public abstract class BaseLevelEditorKeepers : MonoBehaviour
{
	public enum AfterSetup
	{
		Keep,
		Disable,
		Remove
	}

	private bool m_Valid;

	public abstract AfterSetup Setup();

	protected virtual void Awake()
	{
		if (Application.isPlaying && BaseBuildingBlock.m_VisualRepBeingMade == 0 && LevelEditor_Controller.GetInstance() != null && LevelEditor_Controller.GetInstance().IsActivated())
		{
			m_Valid = true;
		}
	}

	public virtual void Start()
	{
		if (m_Valid)
		{
			switch (Setup())
			{
			case AfterSetup.Disable:
				base.enabled = false;
				break;
			case AfterSetup.Remove:
				Object.Destroy(this);
				break;
			}
		}
	}
}
