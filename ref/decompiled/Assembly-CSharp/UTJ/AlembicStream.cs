using System;
using System.Collections.Generic;
using UnityEngine;

namespace UTJ;

[ExecuteInEditMode]
public class AlembicStream : MonoBehaviour
{
	public enum CycleType
	{
		Hold,
		Loop,
		Reverse,
		Bounce
	}

	[Header("Abc")]
	[HideInInspector]
	public string m_pathToAbc;

	[HideInInspector]
	public float m_time;

	[HideInInspector]
	[Header("Playback")]
	public float m_startTime;

	[HideInInspector]
	public float m_endTime;

	[HideInInspector]
	public float m_timeOffset;

	[HideInInspector]
	public float m_timeScale = 1f;

	[HideInInspector]
	public bool m_preserveStartTime = true;

	[HideInInspector]
	public CycleType m_cycle;

	[Header("Data")]
	public bool m_swapHandedness;

	public bool m_swapFaceWinding;

	public bool m_submeshPerUVTile = true;

	public AbcAPI.aiNormalsMode m_normalsMode = AbcAPI.aiNormalsMode.ComputeIfMissing;

	public AbcAPI.aiTangentsMode m_tangentsMode;

	public AbcAPI.aiAspectRatioMode m_aspectRatioMode;

	[HideInInspector]
	[Header("Diagnostic")]
	public bool m_verbose;

	[HideInInspector]
	public bool m_logToFile;

	[HideInInspector]
	public string m_logPath = string.Empty;

	[HideInInspector]
	[Header("Advanced")]
	public bool m_useThreads;

	[HideInInspector]
	public int m_sampleCacheSize;

	[HideInInspector]
	public bool m_forceRefresh;

	[HideInInspector]
	public HashSet<AlembicElement> m_elements = new HashSet<AlembicElement>();

	[HideInInspector]
	public AbcAPI.aiConfig m_config;

	private bool m_loaded;

	private float m_lastAbcTime;

	private bool m_lastSwapHandedness;

	private bool m_lastSwapFaceWinding;

	private bool m_lastSubmeshPerUVTile;

	private AbcAPI.aiNormalsMode m_lastNormalsMode;

	private AbcAPI.aiTangentsMode m_lastTangentsMode;

	private float m_lastAspectRatio = -1f;

	private bool m_lastLogToFile;

	private string m_lastLogPath = string.Empty;

	private float m_timeEps = 0.001f;

	private AbcAPI.aiContext m_abc;

	private Transform m_trans;

	private string m_lastPathToAbc;

	private bool m_updateBegan;

	public bool AbcIsValid()
	{
		return m_abc.ptr != (IntPtr)0;
	}

	private void AbcSyncConfig()
	{
		m_config.swapHandedness = m_swapHandedness;
		m_config.swapFaceWinding = m_swapFaceWinding;
		m_config.normalsMode = m_normalsMode;
		m_config.tangentsMode = m_tangentsMode;
		m_config.cacheTangentsSplits = true;
		m_config.aspectRatio = AbcAPI.GetAspectRatio(m_aspectRatioMode);
		m_config.forceUpdate = false;
		m_config.useThreads = m_useThreads;
		m_config.cacheSamples = m_sampleCacheSize;
		m_config.submeshPerUVTile = m_submeshPerUVTile;
		if (AbcIsValid())
		{
			AbcAPI.aiSetConfig(m_abc, ref m_config);
		}
	}

	public float AbcTime(float inTime)
	{
		float num = 0f;
		if (m_preserveStartTime)
		{
			num = m_startTime * (m_timeScale - 1f);
		}
		float num2 = m_endTime - m_startTime;
		float num3 = m_timeScale * (inTime - m_timeOffset) - num;
		if (m_cycle == CycleType.Hold)
		{
			if (num3 < m_startTime - m_timeEps)
			{
				num3 = m_startTime;
			}
			else if (num3 > m_endTime + m_timeEps)
			{
				num3 = m_endTime;
			}
		}
		else
		{
			float num4 = (num3 - m_startTime) / num2;
			float num5 = (float)Math.Floor(num4);
			float num6 = Math.Abs(num4 - num5);
			if (m_cycle == CycleType.Reverse)
			{
				num3 = ((num3 > m_startTime + m_timeEps && num3 < m_endTime - m_timeEps) ? (m_endTime - num6 * num2) : ((!(num3 < m_startTime + m_timeEps)) ? m_startTime : m_endTime));
			}
			else if (num3 < m_startTime - m_timeEps || num3 > m_endTime + m_timeEps)
			{
				num3 = ((m_cycle != CycleType.Loop && (int)num5 % 2 != 0) ? (m_endTime - num6 * num2) : (m_startTime + num6 * num2));
			}
		}
		return num3;
	}

	private bool AbcUpdateRequired(float abcTime, float aspectRatio)
	{
		if (m_forceRefresh || m_swapHandedness != m_lastSwapHandedness || m_swapFaceWinding != m_lastSwapFaceWinding || m_submeshPerUVTile != m_lastSubmeshPerUVTile || m_normalsMode != m_lastNormalsMode || m_tangentsMode != m_lastTangentsMode || Math.Abs(abcTime - m_lastAbcTime) > m_timeEps || aspectRatio != m_lastAspectRatio || m_pathToAbc != m_lastPathToAbc)
		{
			return true;
		}
		return false;
	}

	private void AbcSetLastUpdateState(float abcTime, float aspectRatio)
	{
		m_lastAbcTime = abcTime;
		m_lastSwapHandedness = m_swapHandedness;
		m_lastSwapFaceWinding = m_swapFaceWinding;
		m_lastSubmeshPerUVTile = m_submeshPerUVTile;
		m_lastNormalsMode = m_normalsMode;
		m_lastTangentsMode = m_tangentsMode;
		m_lastAspectRatio = aspectRatio;
		m_forceRefresh = false;
		m_lastPathToAbc = m_pathToAbc;
	}

	private void AbcUpdateElements()
	{
		if (m_verbose)
		{
			Debug.Log("AlembicStream.AbcUpdateElement: " + m_elements.Count + " element(s).");
		}
		foreach (AlembicElement element in m_elements)
		{
			if (element != null)
			{
				element.AbcUpdate();
			}
		}
	}

	private void AbcDetachElements()
	{
		if (m_verbose)
		{
			Debug.Log("AlembicStream.AbcDetachElement: " + m_elements.Count + " element(s).");
		}
		foreach (AlembicElement element in m_elements)
		{
			if (element != null)
			{
				element.m_abcStream = null;
			}
		}
	}

	private void AbcCleanupSubTree(Transform tr, ref List<GameObject> objsToDelete)
	{
		AlembicElement component = tr.gameObject.GetComponent<AlembicMesh>();
		if (component == null)
		{
			component = tr.gameObject.GetComponent<AlembicXForm>();
			if (component == null)
			{
				component = tr.gameObject.GetComponent<AlembicCamera>();
				if (component == null)
				{
					component = tr.gameObject.GetComponent<AlembicLight>();
				}
			}
		}
		if (component != null && !m_elements.Contains(component))
		{
			if (m_verbose)
			{
				Debug.Log("Alembic.AbcCleanupSubTree: Node \"" + tr.gameObject.name + "\" no longer in alembic tree");
			}
			objsToDelete.Add(tr.gameObject);
			return;
		}
		foreach (Transform item in tr)
		{
			AbcCleanupSubTree(item, ref objsToDelete);
		}
	}

	private void AbcCleanupTree()
	{
		List<GameObject> objsToDelete = new List<GameObject>();
		foreach (Transform item in base.gameObject.transform)
		{
			AbcCleanupSubTree(item, ref objsToDelete);
		}
		foreach (GameObject item2 in objsToDelete)
		{
			UnityEngine.Object.DestroyImmediate(item2);
		}
	}

	private bool AbcRecoverContext()
	{
		if (!AbcIsValid())
		{
			if (m_verbose)
			{
				Debug.Log("AlembicStream.AbcRecoverContext: Try to recover alembic context");
			}
			m_abc = AbcAPI.aiCreateContext(base.gameObject.GetInstanceID());
			if (AbcIsValid())
			{
				m_startTime = AbcAPI.aiGetStartTime(m_abc);
				m_endTime = AbcAPI.aiGetEndTime(m_abc);
				m_preserveStartTime = true;
				m_forceRefresh = true;
				m_trans = GetComponent<Transform>();
				m_elements.Clear();
				AbcSyncConfig();
				AbcAPI.UpdateAbcTree(m_abc, m_trans, AbcTime(m_time));
				if (m_verbose)
				{
					Debug.Log("AlembicStream.AbcRecoverContext: Succeeded (" + m_elements.Count + " element(s))");
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public void AbcUpdateBegin(float time)
	{
		if (m_lastLogToFile != m_logToFile || m_lastLogPath != m_logPath)
		{
			AbcAPI.aiEnableFileLog(m_logToFile, m_logPath);
			m_lastLogToFile = m_logToFile;
			m_lastLogPath = m_logPath;
		}
		if (!m_loaded && m_pathToAbc != null)
		{
			m_loaded = AbcRecoverContext();
		}
		if (!m_loaded)
		{
			return;
		}
		if (!AbcIsValid())
		{
			m_loaded = AbcRecoverContext();
			if (!m_loaded)
			{
				Debug.LogWarning("AlembicStream.AbcUpdate: Lost alembic context");
				return;
			}
		}
		m_time = time;
		float num = AbcTime(m_time);
		float aspectRatio = AbcAPI.GetAspectRatio(m_aspectRatioMode);
		if (!AbcUpdateRequired(num, aspectRatio))
		{
			return;
		}
		if (m_verbose)
		{
			Debug.Log("AlembicStream.AbcUpdate: t=" + m_time + " (t'=" + num + ")");
		}
		if (m_pathToAbc != m_lastPathToAbc)
		{
			if (m_verbose)
			{
				Debug.Log("AlembicStream.AbcUpdate: Path to alembic file changed");
			}
			AbcDetachElements();
			AbcAPI.aiDestroyContext(m_abc);
			m_elements.Clear();
			AbcLoad(createMissingNodes: true);
			AbcCleanupTree();
		}
		else
		{
			AbcSyncConfig();
			if (m_useThreads)
			{
				AbcAPI.aiUpdateSamplesBegin(m_abc, num);
				m_updateBegan = true;
			}
			else
			{
				AbcAPI.aiUpdateSamples(m_abc, num);
				AbcUpdateElements();
			}
		}
		AbcSetLastUpdateState(num, aspectRatio);
	}

	public void AbcUpdateEnd()
	{
		if (m_updateBegan)
		{
			AbcAPI.aiUpdateSamplesEnd(m_abc);
			AbcUpdateElements();
		}
	}

	public void AbcAddElement(AlembicElement e)
	{
		if (e != null)
		{
			if (m_verbose)
			{
				Debug.Log("AlembicStream.AbcAddElement: \"" + e.gameObject.name + "\"");
			}
			m_elements.Add(e);
		}
	}

	public void AbcRemoveElement(AlembicElement e)
	{
		if (e != null)
		{
			if (m_verbose)
			{
				Debug.Log("AlembicStream.AbcRemoveElement: \"" + e.gameObject.name + "\"");
			}
			AbcAPI.aiDestroyObject(m_abc, e.m_abcObj);
			m_elements.Remove(e);
		}
	}

	public void AbcLoad(bool createMissingNodes = false)
	{
		if (m_pathToAbc != null)
		{
			m_trans = GetComponent<Transform>();
			m_abc = AbcAPI.aiCreateContext(base.gameObject.GetInstanceID());
			m_loaded = AbcAPI.aiLoad(m_abc, Application.streamingAssetsPath + "/" + m_pathToAbc);
			if (m_loaded)
			{
				m_startTime = AbcAPI.aiGetStartTime(m_abc);
				m_endTime = AbcAPI.aiGetEndTime(m_abc);
				m_preserveStartTime = true;
				m_forceRefresh = true;
				AbcSyncConfig();
				AbcAPI.UpdateAbcTree(m_abc, m_trans, AbcTime(m_time), createMissingNodes);
			}
		}
	}

	[ContextMenu("REFRESH")]
	public void Refresh()
	{
		AbcDetachElements();
		AbcAPI.aiDestroyContext(m_abc);
		m_elements.Clear();
		AbcLoad(createMissingNodes: true);
		AbcCleanupTree();
	}

	private void OnEnable()
	{
		Refresh();
	}

	private void Awake()
	{
		AbcLoad();
		AbcAPI.aiEnableFileLog(m_logToFile, m_logPath);
	}

	private void Start()
	{
		m_forceRefresh = true;
		AbcSetLastUpdateState(AbcTime(0f), AbcAPI.GetAspectRatio(m_aspectRatioMode));
	}

	private void OnApplicationQuit()
	{
		AbcAPI.aiCleanup();
	}

	private void OnDestroy()
	{
		if (AbcIsValid())
		{
			AbcDetachElements();
			AbcAPI.aiDestroyContext(m_abc);
			m_abc = default(AbcAPI.aiContext);
		}
	}

	private void LateUpdate()
	{
		AbcUpdateEnd();
	}
}
