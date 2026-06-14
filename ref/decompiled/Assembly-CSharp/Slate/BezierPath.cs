using System.Collections.Generic;
using UnityEngine;

namespace Slate;

[AddComponentMenu("SLATE/Path")]
public class BezierPath : Path
{
	public bool constantSpeedInterpolation = true;

	public int resolution = 30;

	public Color drawColor = Color.white;

	[HideInInspector]
	[SerializeField]
	private List<BezierPoint> _points = new List<BezierPoint>();

	private Vector3[] _sampledPathPoints;

	private float _length;

	private bool _closed;

	public List<BezierPoint> points => _points;

	public bool closed
	{
		get
		{
			return _closed;
		}
		set
		{
			if (_closed != value)
			{
				_closed = value;
				SetDirty();
			}
		}
	}

	public BezierPoint this[int index] => points[index];

	public int pointCount => points.Count;

	public override float length => _length;

	public void SetDirty()
	{
		ComputeSampledPathPoints();
		ComputeLength();
	}

	private void ComputeLength()
	{
		if (constantSpeedInterpolation)
		{
			_length = Path.GetLength(_sampledPathPoints);
			return;
		}
		_length = 0f;
		for (int i = 0; i < points.Count - 1; i++)
		{
			_length += ApproximateLength(points[i], points[i + 1], resolution);
		}
		if (closed)
		{
			_length += ApproximateLength(points[points.Count - 1], points[0], resolution);
		}
	}

	private void ComputeSampledPathPoints()
	{
		if (points.Count == 0)
		{
			_sampledPathPoints = new Vector3[0];
			return;
		}
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i < points.Count - 1; i++)
		{
			BezierPoint p = points[i];
			BezierPoint p2 = points[i + 1];
			list.AddRange(GetSampledPathPoints(p, p2, resolution));
		}
		_sampledPathPoints = list.ToArray();
	}

	private void Reset()
	{
		AddPointAt(base.transform.position + new Vector3(-3f, 0f, 0f));
		AddPointAt(base.transform.position + new Vector3(3f, 0f, 0f));
		SetDirty();
	}

	private void Awake()
	{
		SetDirty();
	}

	private void OnValidate()
	{
		SetDirty();
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = drawColor;
		if (points.Count > 1)
		{
			for (int i = 0; i < points.Count - 1; i++)
			{
				DrawPath(points[i], points[i + 1], resolution);
			}
			if (closed)
			{
				DrawPath(points[points.Count - 1], points[0], resolution);
			}
		}
		Gizmos.color = Color.white;
	}

	public static BezierPath Create(Transform targetParent = null)
	{
		string text = "[ PATHS ]";
		GameObject gameObject = null;
		if (targetParent == null)
		{
			gameObject = GameObject.Find(text);
			if (gameObject == null)
			{
				gameObject = new GameObject(text);
			}
		}
		else
		{
			Transform transform = targetParent.Find(text);
			gameObject = ((!(transform != null)) ? new GameObject(text) : transform.gameObject);
		}
		gameObject.transform.SetParent(targetParent, worldPositionStays: false);
		BezierPath bezierPath = new GameObject("Path").AddComponent<BezierPath>();
		bezierPath.transform.SetParent(gameObject.transform, worldPositionStays: false);
		bezierPath.transform.localPosition = Vector3.zero;
		bezierPath.transform.localRotation = Quaternion.identity;
		return bezierPath;
	}

	public BezierPoint AddPointAt(Vector3 position, int index = -1)
	{
		BezierPoint bezierPoint = new BezierPoint(this, position);
		if (index == -1)
		{
			points.Add(bezierPoint);
		}
		else
		{
			points.Insert(index, bezierPoint);
		}
		SetDirty();
		return bezierPoint;
	}

	public void RemovePoint(BezierPoint point)
	{
		points.Remove(point);
		SetDirty();
	}

	public int GetPointIndex(BezierPoint point)
	{
		int result = -1;
		for (int i = 0; i < points.Count; i++)
		{
			if (points[i] == point)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	public override Vector3 GetPointAt(float t)
	{
		if (constantSpeedInterpolation)
		{
			return GetUniformPointAt(t);
		}
		return GetApproximatePointAt(t);
	}

	public Vector3 GetApproximatePointAt(float t)
	{
		if (t <= 0f)
		{
			return points[0].position;
		}
		if (t >= 1f)
		{
			return points[points.Count - 1].position;
		}
		float num = 0f;
		float num2 = 0f;
		BezierPoint bezierPoint = null;
		BezierPoint p = null;
		for (int i = 0; i < points.Count - 1; i++)
		{
			num2 = ApproximateLength(points[i], points[i + 1]) / length;
			if (num + num2 > t)
			{
				bezierPoint = points[i];
				p = points[i + 1];
				break;
			}
			num += num2;
		}
		if (bezierPoint == null)
		{
			bezierPoint = points[points.Count - 1];
			p = points[0];
		}
		t -= num;
		return GetPoint(bezierPoint, p, t / num2);
	}

	public Vector3 GetUniformPointAt(float t)
	{
		if (t <= 0f)
		{
			return points[0].position;
		}
		if (t >= 1f)
		{
			return points[points.Count - 1].position;
		}
		return Path.GetPoint(t, _sampledPathPoints);
	}

	public static Vector3[] GetSampledPathPoints(BezierPoint p1, BezierPoint p2, int resolution)
	{
		List<Vector3> list = new List<Vector3>();
		int num = resolution + 1;
		float num2 = resolution;
		for (int i = 1; i < num; i++)
		{
			Vector3 point = GetPoint(p1, p2, (float)i / num2);
			list.Add(point);
		}
		return list.ToArray();
	}

	public static void DrawPath(BezierPoint p1, BezierPoint p2, int resolution)
	{
		int num = resolution + 1;
		float num2 = resolution;
		Vector3 from = p1.position;
		Vector3 zero = Vector3.zero;
		for (int i = 1; i < num; i++)
		{
			zero = GetPoint(p1, p2, (float)i / num2);
			Gizmos.DrawLine(from, zero);
			from = zero;
		}
	}

	public static Vector3 GetPoint(BezierPoint p1, BezierPoint p2, float t)
	{
		if (p1.handle2 != Vector3.zero)
		{
			if (p2.handle1 != Vector3.zero)
			{
				return Path.GetCubicCurvePoint(p1.position, p1.globalHandle2, p2.globalHandle1, p2.position, t);
			}
			return Path.GetQuadraticCurvePoint(p1.position, p1.globalHandle2, p2.position, t);
		}
		if (p2.handle1 != Vector3.zero)
		{
			return Path.GetQuadraticCurvePoint(p1.position, p2.globalHandle1, p2.position, t);
		}
		return Path.GetLinearPoint(p1.position, p2.position, t);
	}

	public static float ApproximateLength(BezierPoint p1, BezierPoint p2, int resolution = 10)
	{
		float num = resolution;
		float num2 = 0f;
		Vector3 vector = p1.position;
		for (int i = 0; i < resolution + 1; i++)
		{
			Vector3 point = GetPoint(p1, p2, (float)i / num);
			num2 += (point - vector).magnitude;
			vector = point;
		}
		return num2;
	}
}
