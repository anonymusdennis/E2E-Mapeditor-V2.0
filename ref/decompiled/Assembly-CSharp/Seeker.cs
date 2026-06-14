using System;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;

[HelpURL("http://arongranberg.com/astar/docs/class_seeker.php")]
[AddComponentMenu("Pathfinding/Seeker")]
public class Seeker : MonoBehaviour, ISerializationCallbackReceiver
{
	public enum ModifierPass
	{
		PreProcess = 0,
		PostProcess = 2
	}

	public bool drawGizmos = true;

	public bool detailedGizmos;

	public StartEndModifier startEndModifier = new StartEndModifier();

	[HideInInspector]
	public int traversableTags = -1;

	[FormerlySerializedAs("traversableTags")]
	[SerializeField]
	[HideInInspector]
	protected TagMask traversableTagsCompatibility = new TagMask(-1, -1);

	[HideInInspector]
	public int[] tagPenalties = new int[32];

	public OnPathDelegate pathCallback;

	public OnPathDelegate preProcessPath;

	public OnPathDelegate postProcessPath;

	[NonSerialized]
	private List<Vector3> lastCompletedVectorPath;

	[NonSerialized]
	private List<GraphNode> lastCompletedNodePath;

	[NonSerialized]
	protected Path path;

	[NonSerialized]
	private Path prevPath;

	private readonly OnPathDelegate onPathDelegate;

	private readonly OnPathDelegate onPartialPathDelegate;

	private OnPathDelegate tmpPathCallback;

	protected uint lastPathID;

	private readonly List<IPathModifier> modifiers = new List<IPathModifier>();

	public Seeker()
	{
		onPathDelegate = OnPathComplete;
		onPartialPathDelegate = OnPartialPathComplete;
	}

	private void Awake()
	{
		startEndModifier.Awake(this);
	}

	public Path GetCurrentPath()
	{
		return path;
	}

	public void OnDestroy()
	{
		ReleaseClaimedPath();
		startEndModifier.OnDestroy(this);
		tmpPathCallback = null;
	}

	public void ReleaseClaimedPath()
	{
		if (prevPath != null)
		{
			prevPath.Release(this, silent: true);
			prevPath = null;
		}
	}

	public void RegisterModifier(IPathModifier mod)
	{
		modifiers.Add(mod);
		modifiers.Sort((IPathModifier a, IPathModifier b) => a.Order.CompareTo(b.Order));
	}

	public void DeregisterModifier(IPathModifier mod)
	{
		modifiers.Remove(mod);
	}

	public void PostProcess(Path p)
	{
		RunModifiers(ModifierPass.PostProcess, p);
	}

	public void RunModifiers(ModifierPass pass, Path p)
	{
		if (pass == ModifierPass.PreProcess && preProcessPath != null)
		{
			preProcessPath(p);
		}
		else if (pass == ModifierPass.PostProcess && postProcessPath != null)
		{
			postProcessPath(p);
		}
		for (int i = 0; i < modifiers.Count; i++)
		{
			MonoModifier monoModifier = modifiers[i] as MonoModifier;
			if (!(monoModifier != null) || monoModifier.enabled)
			{
				switch (pass)
				{
				case ModifierPass.PreProcess:
					modifiers[i].PreProcess(p);
					break;
				case ModifierPass.PostProcess:
					modifiers[i].Apply(p);
					break;
				}
			}
		}
	}

	public bool IsDone()
	{
		return path == null || path.GetState() >= PathState.Returned;
	}

	private void OnPathComplete(Path p)
	{
		p.callback = (OnPathDelegate)Delegate.Remove(p.callback, new OnPathDelegate(OnPathComplete));
		OnPathComplete(p, runModifiers: true, sendCallbacks: true);
	}

	private void OnPathComplete(Path p, bool runModifiers, bool sendCallbacks)
	{
		if ((p != null && p != path && sendCallbacks) || this == null || p == null || p != path)
		{
			return;
		}
		if (!path.error && runModifiers)
		{
			RunModifiers(ModifierPass.PostProcess, path);
		}
		if (sendCallbacks)
		{
			p.Claim(this);
			lastCompletedNodePath = p.path;
			lastCompletedVectorPath = p.vectorPath;
			if (tmpPathCallback != null)
			{
				tmpPathCallback(p);
			}
			if (pathCallback != null)
			{
				pathCallback(p);
			}
			if (prevPath != null)
			{
				prevPath.Release(this, silent: true);
			}
			prevPath = p;
			if (!drawGizmos)
			{
				ReleaseClaimedPath();
			}
		}
	}

	private void OnPartialPathComplete(Path p)
	{
		OnPathComplete(p, runModifiers: true, sendCallbacks: false);
	}

	private void OnMultiPathComplete(Path p)
	{
		p.callback = (OnPathDelegate)Delegate.Remove(p.callback, new OnPathDelegate(OnMultiPathComplete));
		OnPathComplete(p, runModifiers: false, sendCallbacks: true);
	}

	public ABPath GetNewPath(Vector3 start, Vector3 end)
	{
		return ABPath.Construct(start, end);
	}

	public Path StartPath(Vector3 start, Vector3 end)
	{
		return StartPath(start, end, null, -1);
	}

	public Path StartPath(Vector3 start, Vector3 end, OnPathDelegate callback)
	{
		return StartPath(start, end, callback, -1);
	}

	public Path StartPath(Vector3 start, Vector3 end, OnPathDelegate callback, int graphMask)
	{
		return StartPath(GetNewPath(start, end), callback, graphMask);
	}

	public Path StartPath(Path p, OnPathDelegate callback = null, int graphMask = -1)
	{
		if (p is MultiTargetPath multiTargetPath)
		{
			OnPathDelegate[] array = new OnPathDelegate[multiTargetPath.targetPoints.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = onPartialPathDelegate;
			}
			multiTargetPath.callbacks = array;
			p.callback = (OnPathDelegate)Delegate.Combine(p.callback, new OnPathDelegate(OnMultiPathComplete));
		}
		else
		{
			p.callback = (OnPathDelegate)Delegate.Combine(p.callback, onPathDelegate);
		}
		p.enabledTags = traversableTags;
		p.tagPenalties = tagPenalties;
		p.nnConstraint.graphMask = graphMask;
		StartPathInternal(p, callback);
		return p;
	}

	private void StartPathInternal(Path p, OnPathDelegate callback)
	{
		if (path != null && path.GetState() <= PathState.Processing && lastPathID == path.pathID)
		{
			path.Error();
		}
		path = p;
		tmpPathCallback = callback;
		lastPathID = path.pathID;
		RunModifiers(ModifierPass.PreProcess, path);
		AstarPath.StartPath(path);
	}

	public MultiTargetPath StartMultiTargetPath(Vector3 start, Vector3[] endPoints, bool pathsForAll, OnPathDelegate callback = null, int graphMask = -1)
	{
		MultiTargetPath multiTargetPath = MultiTargetPath.Construct(start, endPoints, null);
		multiTargetPath.pathsForAll = pathsForAll;
		StartPath(multiTargetPath, callback, graphMask);
		return multiTargetPath;
	}

	public MultiTargetPath StartMultiTargetPath(Vector3[] startPoints, Vector3 end, bool pathsForAll, OnPathDelegate callback = null, int graphMask = -1)
	{
		MultiTargetPath multiTargetPath = MultiTargetPath.Construct(startPoints, end, null);
		multiTargetPath.pathsForAll = pathsForAll;
		StartPath(multiTargetPath, callback, graphMask);
		return multiTargetPath;
	}

	[Obsolete("You can use StartPath instead of this method now. It will behave identically.")]
	public MultiTargetPath StartMultiTargetPath(MultiTargetPath p, OnPathDelegate callback = null, int graphMask = -1)
	{
		StartPath(p, callback, graphMask);
		return p;
	}

	public void OnDrawGizmos()
	{
		if (lastCompletedNodePath == null || !drawGizmos)
		{
			return;
		}
		if (detailedGizmos)
		{
			Gizmos.color = new Color(0.7f, 0.5f, 0.1f, 0.5f);
			if (lastCompletedNodePath != null)
			{
				Vector3 vector = default(Vector3);
				Vector3 vector2 = default(Vector3);
				for (int i = 0; i < lastCompletedNodePath.Count - 1; i++)
				{
					vector = (Vector3)lastCompletedNodePath[i].position;
					GridGraph gridGraph = (GridGraph)AstarPath.active.graphs[lastCompletedNodePath[i].GraphIndex];
					vector.z = gridGraph.center.z - 1.2f;
					vector2 = (Vector3)lastCompletedNodePath[i + 1].position;
					gridGraph = (GridGraph)AstarPath.active.graphs[lastCompletedNodePath[i + 1].GraphIndex];
					vector2.z = gridGraph.center.z - 1.2f;
					Gizmos.DrawLine(vector, vector2);
				}
			}
		}
		Gizmos.color = new Color(0f, 1f, 0f, 1f);
		if (lastCompletedVectorPath != null)
		{
			Vector3 vector3 = default(Vector3);
			Vector3 vector4 = default(Vector3);
			for (int j = 0; j < lastCompletedNodePath.Count - 1; j++)
			{
				vector3 = (Vector3)lastCompletedNodePath[j].position;
				GridGraph gridGraph2 = (GridGraph)AstarPath.active.graphs[lastCompletedNodePath[j].GraphIndex];
				vector3.z = gridGraph2.center.z - 1.2f;
				vector4 = (Vector3)lastCompletedNodePath[j + 1].position;
				gridGraph2 = (GridGraph)AstarPath.active.graphs[lastCompletedNodePath[j + 1].GraphIndex];
				vector4.z = gridGraph2.center.z - 1.2f;
				Gizmos.DrawLine(vector3, vector4);
			}
		}
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		if (traversableTagsCompatibility != null && traversableTagsCompatibility.tagsChange != -1)
		{
			traversableTags = traversableTagsCompatibility.tagsChange;
			traversableTagsCompatibility = new TagMask(-1, -1);
		}
	}
}
