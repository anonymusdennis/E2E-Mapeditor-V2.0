using UnityEngine;

namespace Slate;

public class GameCamera : MonoBehaviour, IDirectableCamera
{
	private Camera _cam;

	public Camera cam => (!(_cam != null)) ? (_cam = GetComponent<Camera>()) : _cam;

	public Vector3 position
	{
		get
		{
			return base.transform.position;
		}
		set
		{
		}
	}

	public Quaternion rotation
	{
		get
		{
			return base.transform.rotation;
		}
		set
		{
		}
	}

	public float fieldOfView
	{
		get
		{
			return (!cam.orthographic) ? cam.fieldOfView : cam.orthographicSize;
		}
		set
		{
		}
	}

	public float focalPoint
	{
		get
		{
			return 10f;
		}
		set
		{
		}
	}

	public float focalRange
	{
		get
		{
			return 100f;
		}
		set
		{
		}
	}

	GameObject IDirectableCamera.get_gameObject()
	{
		return base.gameObject;
	}
}
