using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class T17BehaviourManager : T17MonoBehaviour
{
	public enum INITSTATE
	{
		IS_NONE,
		IS_FINISHED,
		IS_ONGOING,
		IS_DEPS,
		IS_REQUIRES_LAST
	}

	private static T17BehaviourManager m_TheInstance;

	private List<T17NetworkBehaviour> m_NetworkBehaviourClasses;

	private List<T17MonoBehaviour> m_MonoBehaviourClasses;

	private int m_LastLoggedNetCount;

	private int m_LastLoggedMonoCount;

	private int m_IndexInList;

	private bool m_bCurrentListIsNet;

	private Stopwatch m_StopWatch = new Stopwatch();

	private int m_MSTimeForInits = 30;

	private int m_MaxInitsPerCall;

	private bool m_BehaviourScanDone;

	private bool m_bBuildAListAndInitedAndPurged;

	protected override void Awake()
	{
		Init();
		base.Awake();
	}

	protected virtual void OnDestroy()
	{
		if (m_TheInstance == this)
		{
			m_TheInstance = null;
		}
	}

	public void Init()
	{
		m_TheInstance = this;
		m_NetworkBehaviourClasses = new List<T17NetworkBehaviour>();
		m_MonoBehaviourClasses = new List<T17MonoBehaviour>();
		m_LastLoggedNetCount = 0;
		m_LastLoggedMonoCount = 0;
		m_IndexInList = 0;
		m_BehaviourScanDone = false;
		m_bBuildAListAndInitedAndPurged = false;
	}

	public static T17BehaviourManager GetInstance()
	{
		return m_TheInstance;
	}

	public void PreScan()
	{
		m_BehaviourScanDone = false;
		m_bBuildAListAndInitedAndPurged = false;
	}

	public void PostScan()
	{
		m_BehaviourScanDone = true;
		UnityEngine.Debug.LogError(" **** behaviours nets = " + m_NetworkBehaviourClasses.Count + "  monos =  " + m_MonoBehaviourClasses.Count);
	}

	public bool ScanCompleted()
	{
		return m_BehaviourScanDone;
	}

	public bool ListFullyInitedAndPurged()
	{
		return m_bBuildAListAndInitedAndPurged;
	}

	private void Update()
	{
		if (m_NetworkBehaviourClasses.Count != m_LastLoggedNetCount || m_MonoBehaviourClasses.Count != m_LastLoggedMonoCount)
		{
			m_LastLoggedNetCount = m_NetworkBehaviourClasses.Count;
			m_LastLoggedMonoCount = m_MonoBehaviourClasses.Count;
		}
	}

	private void AddClassesToLists(Transform trans)
	{
		if (m_BehaviourScanDone)
		{
		}
		if (trans == null)
		{
			throw new Exception(" AddClassesToLists - Looking for classes on a null ");
		}
		m_NetworkBehaviourClasses.AddRange(trans.GetComponents<T17NetworkBehaviour>());
		m_MonoBehaviourClasses.AddRange(trans.GetComponents<T17MonoBehaviour>());
		int childCount = trans.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = trans.GetChild(i);
			if (child.gameObject.activeInHierarchy)
			{
				AddClassesToLists(trans.GetChild(i));
			}
		}
		m_IndexInList = 0;
		m_bCurrentListIsNet = true;
	}

	public void AddClassesFromRoot(Transform root)
	{
		AddClassesToLists(root);
	}

	public bool RunClassInit()
	{
		int num = 0;
		m_StopWatch.Reset();
		m_StopWatch.Start();
		while (m_StopWatch.ElapsedMilliseconds < m_MSTimeForInits && (m_NetworkBehaviourClasses.Count > 0 || m_MonoBehaviourClasses.Count > 0))
		{
			T17NetManager.Service();
			num++;
			int count;
			if (m_bCurrentListIsNet)
			{
				count = m_NetworkBehaviourClasses.Count;
				if (m_IndexInList < count)
				{
					if (!m_NetworkBehaviourClasses[m_IndexInList].IsInited())
					{
						switch (m_NetworkBehaviourClasses[m_IndexInList].StartInit())
						{
						case INITSTATE.IS_FINISHED:
							m_NetworkBehaviourClasses.RemoveAt(m_IndexInList);
							break;
						case INITSTATE.IS_DEPS:
							m_IndexInList++;
							break;
						}
					}
				}
				else
				{
					m_bCurrentListIsNet = false;
					m_IndexInList = 0;
				}
				continue;
			}
			count = m_MonoBehaviourClasses.Count;
			if (m_IndexInList < count)
			{
				if (!m_MonoBehaviourClasses[m_IndexInList].IsInited())
				{
					switch (m_MonoBehaviourClasses[m_IndexInList].StartInit())
					{
					case INITSTATE.IS_FINISHED:
						m_MonoBehaviourClasses.RemoveAt(m_IndexInList);
						break;
					case INITSTATE.IS_DEPS:
						m_IndexInList++;
						break;
					case INITSTATE.IS_REQUIRES_LAST:
						if (count == 1)
						{
							m_MonoBehaviourClasses[m_IndexInList].StartInitLast();
						}
						m_IndexInList++;
						break;
					}
				}
				else
				{
					m_MonoBehaviourClasses.RemoveAt(m_IndexInList);
				}
			}
			else
			{
				m_bCurrentListIsNet = true;
				m_IndexInList = 0;
			}
		}
		if (num > m_MaxInitsPerCall)
		{
			m_MaxInitsPerCall = num;
		}
		if (m_NetworkBehaviourClasses.Count == 0 && m_MonoBehaviourClasses.Count == 0)
		{
			return true;
		}
		return false;
	}

	public void PurgeClasses()
	{
		m_NetworkBehaviourClasses.Clear();
		m_MonoBehaviourClasses.Clear();
		m_bBuildAListAndInitedAndPurged = true;
	}

	public void GetHoldOfLists(ref List<T17NetworkBehaviour> nets, ref List<T17MonoBehaviour> monos)
	{
		nets = m_NetworkBehaviourClasses;
		monos = m_MonoBehaviourClasses;
	}

	public void InjectClasses(T17NetworkBehaviour[] nets, T17MonoBehaviour[] monos)
	{
		for (int i = 0; i < nets.Length; i++)
		{
			m_NetworkBehaviourClasses.Add(nets[i]);
		}
		for (int i = 0; i < monos.Length; i++)
		{
			m_MonoBehaviourClasses.Add(monos[i]);
		}
	}
}
