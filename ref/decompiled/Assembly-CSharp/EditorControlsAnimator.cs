using UnityEngine;

public class EditorControlsAnimator : MonoBehaviour
{
	public enum ControlAnimState
	{
		InView,
		Out
	}

	public Animator m_Animator;

	public string m_In_Trigger = string.Empty;

	public string m_Out_Trigger = string.Empty;

	private ControlAnimState m_ManualState;

	private ControlAnimState m_AutoState;

	private ControlAnimState m_CurrentState;

	private void Start()
	{
		if (string.IsNullOrEmpty(m_In_Trigger))
		{
			base.enabled = false;
		}
		if (string.IsNullOrEmpty(m_Out_Trigger))
		{
			base.enabled = false;
		}
		if (m_Animator == null)
		{
			m_Animator = GetComponent<Animator>();
		}
		if (m_Animator == null)
		{
			base.enabled = false;
		}
		else if (LevelEditor_Cursor.GetInstance() == null)
		{
			base.enabled = false;
		}
		else
		{
			LevelEditor_Cursor.GetInstance().RegisterForControlVisibityChange(OnControlsVisibilityChanged);
		}
	}

	private void OnControlsVisibilityChanged(bool bVisible)
	{
		bool flag = m_AutoState == ControlAnimState.InView;
		if (flag != bVisible)
		{
			if (bVisible)
			{
				m_AutoState = ControlAnimState.InView;
			}
			else
			{
				m_AutoState = ControlAnimState.Out;
			}
			UpdateAnimation();
		}
	}

	public void ToggleVisibility()
	{
		if (m_ManualState == ControlAnimState.InView)
		{
			m_ManualState = ControlAnimState.Out;
			if (LevelEditor_Controller.GetInstance() != null)
			{
				LevelEditor_Controller.GetInstance().PlayAudio(LevelEditor_Controller.AudioTypes.TabOut);
			}
		}
		else
		{
			m_ManualState = ControlAnimState.InView;
			if (LevelEditor_Controller.GetInstance() != null)
			{
				LevelEditor_Controller.GetInstance().PlayAudio(LevelEditor_Controller.AudioTypes.TabIn);
			}
		}
		UpdateAnimation();
	}

	public void SetManualState(ControlAnimState newState)
	{
		if (m_ManualState != newState)
		{
			m_ManualState = newState;
			UpdateAnimation();
		}
	}

	public ControlAnimState GetManualState()
	{
		return m_ManualState;
	}

	private void UpdateAnimation()
	{
		ControlAnimState controlAnimState = m_AutoState;
		if (m_ManualState == ControlAnimState.Out)
		{
			controlAnimState = ControlAnimState.Out;
		}
		if (controlAnimState != m_CurrentState)
		{
			m_CurrentState = controlAnimState;
			switch (m_CurrentState)
			{
			case ControlAnimState.InView:
				m_Animator.SetTrigger(m_In_Trigger);
				break;
			case ControlAnimState.Out:
				m_Animator.SetTrigger(m_Out_Trigger);
				break;
			}
		}
	}
}
