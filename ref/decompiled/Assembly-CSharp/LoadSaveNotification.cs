using UnityEngine;

public class LoadSaveNotification : MonoBehaviour
{
	private enum NotificationState
	{
		WaitingToShow,
		Shown,
		WaitingToHide,
		Hidden
	}

	private NotificationState m_NotificationCurrentState = NotificationState.Hidden;

	private float m_TimeForTransition = -1f;

	private float m_TimeShown = -1f;

	[Tooltip("This is the GameObject we will turn on and off")]
	public GameObject m_NotificationObject;

	[Tooltip("How many seconds to wait before showing the icon when Load/Save is happening, if we stop saving/loading before this expires it wont be shown.")]
	public float m_SecondsBeforeIconIsShown = 1f;

	[Tooltip("How many seconds to wait before hiding the icon when Load/Save is happening, if we start saving/loading before this expires it wont be hidden.")]
	public float m_SecondsBeforeIconIsHidden = 1f;

	public float m_MinSecondsIconIsShownFor = 1f;

	public bool m_bAlwaysShow = true;

	private void Awake()
	{
		if ((bool)PlatformIO.GetInstance())
		{
			PlatformIO.GetInstance().IsIOBusy(bSaveOnly: true);
		}
	}

	private void OnEnable()
	{
		if (m_NotificationObject == null)
		{
			base.enabled = false;
		}
		else if (m_NotificationObject.activeSelf)
		{
			m_NotificationCurrentState = NotificationState.Shown;
		}
		else
		{
			m_NotificationCurrentState = NotificationState.Hidden;
		}
	}

	private void Update()
	{
		if (!(PlatformIO.GetInstance() != null))
		{
			return;
		}
		bool flag = PlatformIO.GetInstance().IsIOBusy(bSaveOnly: true) || GlobalSave.GetInstance().IsBusy(bSaveOnly: true);
		switch (m_NotificationCurrentState)
		{
		case NotificationState.Hidden:
			if (flag)
			{
				m_NotificationCurrentState = NotificationState.WaitingToShow;
				m_TimeForTransition = Time.realtimeSinceStartup + m_SecondsBeforeIconIsShown;
			}
			break;
		case NotificationState.Shown:
			if (!flag && Time.realtimeSinceStartup - m_TimeShown >= m_MinSecondsIconIsShownFor)
			{
				m_TimeForTransition = Time.realtimeSinceStartup + m_SecondsBeforeIconIsHidden;
				m_NotificationCurrentState = NotificationState.WaitingToHide;
			}
			break;
		case NotificationState.WaitingToHide:
			if (flag)
			{
				m_NotificationCurrentState = NotificationState.Shown;
			}
			else if (m_TimeForTransition <= Time.realtimeSinceStartup)
			{
				m_NotificationObject.SetActive(value: false);
				m_NotificationCurrentState = NotificationState.Hidden;
			}
			break;
		case NotificationState.WaitingToShow:
			if (!m_bAlwaysShow && !flag)
			{
				m_NotificationCurrentState = NotificationState.Hidden;
			}
			else if (m_TimeForTransition <= Time.realtimeSinceStartup)
			{
				m_NotificationObject.SetActive(value: true);
				m_NotificationCurrentState = NotificationState.Shown;
				m_TimeShown = Time.realtimeSinceStartup;
			}
			break;
		}
	}
}
