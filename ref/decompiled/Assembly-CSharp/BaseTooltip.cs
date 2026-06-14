using UnityEngine;

public class BaseTooltip : MonoBehaviour
{
	public Vector2 m_FullScreenSplitScale = Vector2.one;

	public Vector2 m_HalfScreenSplitScale = Vector2.one;

	public Vector2 m_QuarterScreenSplitScale = Vector2.one;

	protected CameraManager.PlayerBindingID m_Binding;

	[HideInInspector]
	public Platform.PlatformOverride m_PlatformOverride;

	public static bool SmallHudNeeded()
	{
		return false;
	}

	protected Vector3 GetSplitscreenScale()
	{
		CameraManager instance = CameraManager.GetInstance();
		Vector3 one = Vector3.one;
		if (instance != null && m_Binding != 0)
		{
			switch (instance.GetUsedCameraCount())
			{
			case 1:
				one.x = m_FullScreenSplitScale.x;
				one.y = m_FullScreenSplitScale.y;
				return one;
			case 2:
				one.x = m_HalfScreenSplitScale.x;
				one.y = m_HalfScreenSplitScale.y;
				return one;
			case 3:
				if (m_Binding == CameraManager.PlayerBindingID.CM_PBID_PLAYER_ALPHA)
				{
					one.x = m_HalfScreenSplitScale.x;
					one.y = m_HalfScreenSplitScale.y;
					return one;
				}
				one.x = m_QuarterScreenSplitScale.x;
				one.y = m_QuarterScreenSplitScale.y;
				return one;
			case 4:
				one.x = m_QuarterScreenSplitScale.x;
				one.y = m_QuarterScreenSplitScale.y;
				return one;
			}
		}
		return Vector3.one;
	}

	public virtual void SetLocalScaleSplit()
	{
		base.transform.localScale = GetSplitscreenScale();
	}

	public void ResetCameraBinding()
	{
		m_Binding = CameraManager.PlayerBindingID.CM_PBID_UNSET;
	}

	public void SetCameraBinding(CameraManager.PlayerBindingID binding)
	{
		m_Binding = binding;
	}
}
