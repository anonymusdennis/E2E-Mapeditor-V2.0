using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Slate.ActionClips;

[Description("For this clip to work you only need to have a baked NavMesh. The actor does NOT need, or use a NavMeshAgent Component. The length of the clip is determined by the path's length and the speed parameter set, while the Blend In parameter is used only for the initial look ahead blending")]
[Category("Paths")]
public class PathfindTo : ActorActionClip
{
	[HideInInspector]
	[SerializeField]
	private float _blendIn = 0.5f;

	[Min(0.01f)]
	public float speed = 3f;

	public PositionParameter targetPosition;

	private NavMeshPath navPath;

	private Vector3[] pathPoints;

	private Vector3 originalPos;

	private Quaternion originalRot;

	private Vector3 lastFrom;

	private Vector3 lastTo;

	private bool lockCalc;

	public override string info => $"Pathfind To\n{targetPosition.ToString()}";

	public override float length
	{
		get
		{
			if (isValid)
			{
				TryCalculatePath();
				return Path.GetLength(pathPoints) / speed;
			}
			return 0f;
		}
	}

	public override float blendIn
	{
		get
		{
			return (!(length > 0f)) ? 0f : _blendIn;
		}
		set
		{
			_blendIn = value;
		}
	}

	protected override void OnEnter()
	{
		lockCalc = false;
		TryCalculatePath();
		lockCalc = true;
		originalPos = base.actor.transform.position;
		originalRot = base.actor.transform.rotation;
		if (pathPoints == null || pathPoints.Length == 0)
		{
			Debug.LogWarning($"Actor '{base.actor.name}' can't pathfind to '{targetPosition.value}'", base.actor);
		}
	}

	protected override void OnUpdate(float deltaTime)
	{
		if (pathPoints == null || pathPoints.Length <= 1)
		{
			return;
		}
		if (length == 0f)
		{
			base.actor.transform.position = Path.GetPoint(0f, pathPoints);
			return;
		}
		base.actor.transform.position = Path.GetPoint(deltaTime / length, pathPoints);
		Vector3 point = Path.GetPoint(deltaTime / length + 0.01f, pathPoints);
		if (blendIn > 0f && deltaTime <= blendIn)
		{
			Quaternion to = Quaternion.LookRotation(point - base.actor.transform.position);
			base.actor.transform.rotation = Easing.Ease(EaseType.QuadraticInOut, originalRot, to, deltaTime / blendIn);
		}
		else
		{
			base.actor.transform.LookAt(point);
		}
	}

	protected override void OnReverse()
	{
		base.actor.transform.position = originalPos;
		base.actor.transform.rotation = originalRot;
		lockCalc = false;
	}

	private void TryCalculatePath()
	{
		Vector3 vector = TransformPoint(targetPosition.value, targetPosition.space);
		if (!lockCalc && (navPath == null || lastFrom != base.actor.transform.position || lastTo != vector))
		{
			navPath = new NavMeshPath();
			NavMesh.CalculatePath(base.actor.transform.position, vector, -1, navPath);
			pathPoints = navPath.corners.ToArray();
		}
		lastFrom = base.actor.transform.position;
		lastTo = vector;
	}
}
