using Pathfinding;
using UnityEngine;

public class T17_ABPath : ABPath
{
	public delegate void PathCallback();

	public delegate void PathDistanceCallback(int distance, GameObject toTarget, AICharacter caller);

	public delegate void ArrowPathCallback(Vector3 targetPosition, bool pathChangesFloor);

	public PathCallback path_OnTargetReached;

	public PathCallback path_OnPathCancelled;

	public ArrowPathCallback path_OnArrowTargetFound;

	public float path_fCloseEnoughDistance;

	public bool path_bAllowTeleport;

	public Vector3 path_RealTargetPos;

	public bool path_bSkipLastNode;

	public GameObject path_TargetGO;

	public PathDistanceCallback path_Distance;

	public new static T17_ABPath Construct(Vector3 start, Vector3 end, OnPathDelegate callback = null)
	{
		T17_ABPath t17_ABPath = PathPool.GetPath<T17_ABPath>();
		t17_ABPath.Setup(start, end, callback);
		return t17_ABPath;
	}

	public override void Reset()
	{
		path_OnTargetReached = null;
		path_OnPathCancelled = null;
		path_OnArrowTargetFound = null;
		path_TargetGO = null;
		path_Distance = null;
		base.Reset();
	}
}
