using System.Collections.Generic;
using UnityEngine;

public abstract class EventManager : T17MonoBehaviour
{
	public T17NetView m_NetView;

	public bool m_bVisibleFromBelow;

	private bool m_bBucketPositionsLocked;

	private List<AIEventManager.EventManagerPosition> m_BucketPositions = new List<AIEventManager.EventManagerPosition>();

	private List<AIEventManager.EventManagerPosition> m_QueueToAdd = new List<AIEventManager.EventManagerPosition>();

	private List<AIEventManager.EventManagerPosition> m_QueueToRemove = new List<AIEventManager.EventManagerPosition>();

	protected List<Vector3> m_TargetOffsets = new List<Vector3>();

	private static bool m_bAIDebugOn = GlobalStart.m_bShowDebugElements;

	private Texture2D _tex;

	private Texture2D tex
	{
		get
		{
			if (_tex == null)
			{
				_tex = new Texture2D(1, 1);
				_tex.SetPixel(0, 0, Color.white);
				_tex.Apply();
			}
			return _tex;
		}
	}

	public abstract List<AIEvent> GetVisibleEvents();

	public abstract uint GetEventManagerID();

	protected virtual void OnManagerDestroyed()
	{
	}

	public abstract AIEvent GetEventByType(AIEvent.EventType eventType);

	public void Start()
	{
		if (T17BehaviourManager.GetInstance().ScanCompleted() && T17BehaviourManager.GetInstance().ListFullyInitedAndPurged() && !IsInited())
		{
			StartInit();
		}
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		AIEventManager.GetInstance().RegisterManager(this);
		m_TargetOffsets.Add(Vector3.zero);
		return base.StartInit();
	}

	public virtual void OnDestroy()
	{
		OnManagerDestroyed();
		if (AIEventManager.GetInstance() != null)
		{
			AIEventManager.GetInstance().UnRegisterManager(this);
			AIEventManager.GetInstance().RemoveManager(this);
		}
		m_BucketPositions.Clear();
		m_QueueToAdd.Clear();
		m_QueueToRemove.Clear();
		m_NetView = null;
	}

	public List<Vector3> GetTargetOffsets()
	{
		return m_TargetOffsets;
	}

	public List<AIEventManager.EventManagerPosition> LockBucketPositions()
	{
		m_bBucketPositionsLocked = true;
		return m_BucketPositions;
	}

	public void UnlockBucketPositions()
	{
		m_bBucketPositionsLocked = false;
		while (m_QueueToAdd.Count > 0)
		{
			AIEventManager.EventManagerPosition position = m_QueueToAdd[0];
			m_QueueToAdd.RemoveAt(0);
			AddBucketPosition(position);
		}
		while (m_QueueToRemove.Count > 0)
		{
			AIEventManager.EventManagerPosition position2 = m_QueueToRemove[0];
			m_QueueToRemove.RemoveAt(0);
			RemoveBucketPosition(position2);
		}
	}

	public void AddBucketPosition(AIEventManager.EventManagerPosition position)
	{
		if (m_bBucketPositionsLocked)
		{
			m_QueueToAdd.Add(position);
		}
		else
		{
			m_BucketPositions.Add(position);
		}
	}

	public void RemoveBucketPosition(AIEventManager.EventManagerPosition position)
	{
		if (m_bBucketPositionsLocked)
		{
			m_QueueToRemove.Add(position);
		}
		else
		{
			m_BucketPositions.Remove(position);
		}
	}

	public bool VisibleFromBelow()
	{
		return m_bVisibleFromBelow;
	}

	public virtual Vector3 GetWorldPosition()
	{
		return base.transform.position;
	}

	public static bool AIDebug(bool bPos, bool bJustRead)
	{
		if (!bJustRead)
		{
			m_bAIDebugOn = bPos;
		}
		return m_bAIDebugOn;
	}
}
