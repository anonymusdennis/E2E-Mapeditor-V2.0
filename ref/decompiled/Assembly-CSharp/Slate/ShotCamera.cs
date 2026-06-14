using UnityEngine;

namespace Slate;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("SLATE/Shot Camera")]
public class ShotCamera : MonoBehaviour, IDirectableCamera
{
	[SerializeField]
	private float _focalPoint = 10f;

	[SerializeField]
	private float _focalRange = 15f;

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
			base.transform.position = value;
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
			base.transform.rotation = value;
		}
	}

	public Vector3 localPosition
	{
		get
		{
			return base.transform.localPosition;
		}
		set
		{
			base.transform.localPosition = value;
		}
	}

	public Vector3 localEulerAngles
	{
		get
		{
			return base.transform.GetLocalEulerAngles();
		}
		set
		{
			base.transform.SetLocalEulerAngles(value);
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
			cam.fieldOfView = value;
			cam.orthographicSize = value;
		}
	}

	public float focalPoint
	{
		get
		{
			return _focalPoint;
		}
		set
		{
			_focalPoint = value;
		}
	}

	public float focalRange
	{
		get
		{
			return _focalRange;
		}
		set
		{
			_focalRange = value;
		}
	}

	private void Awake()
	{
		cam.enabled = false;
		if (cam.targetTexture != null)
		{
			cam.targetTexture.Release();
			Object.DestroyImmediate(cam.targetTexture);
		}
	}

	public RenderTexture GetRenderTexture(int width, int height)
	{
		RenderTexture renderTexture = cam.targetTexture;
		if (renderTexture == null)
		{
			renderTexture = new RenderTexture(width, height, 24);
		}
		if (renderTexture.width != width || renderTexture.height != height)
		{
			renderTexture.Release();
			Object.DestroyImmediate(renderTexture, allowDestroyingAssets: true);
			renderTexture = new RenderTexture(width, height, 24);
		}
		cam.targetTexture = renderTexture;
		cam.Render();
		return renderTexture;
	}

	GameObject IDirectableCamera.get_gameObject()
	{
		return base.gameObject;
	}
}
