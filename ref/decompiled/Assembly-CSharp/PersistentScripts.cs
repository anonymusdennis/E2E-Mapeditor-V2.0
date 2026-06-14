using System;
using UnityEngine;

public class PersistentScripts : MonoBehaviour
{
	public class PersistentScriptComponent<T> where T : MonoBehaviour
	{
		private T m_value;

		public void Awake(Component parent)
		{
			m_value = parent.GetComponent<T>();
		}

		public T value(bool silentFail = false)
		{
			if (m_value == null && !silentFail && !IsApplicationQuitting)
			{
				Debug.LogErrorFormat("PersistentScripts.{0} - value is null", typeof(T).Name);
			}
			return m_value;
		}

		public static explicit operator T(PersistentScriptComponent<T> psc)
		{
			return psc.value();
		}
	}

	public int GCCollectFreq;

	private static GameObject m_nullGameObject;

	public static PersistentScriptComponent<PrimitiveDrawer> PrimitiveDrawer = new PersistentScriptComponent<PrimitiveDrawer>();

	private static PersistentScripts m_instance = null;

	public static bool IsApplicationQuitting { get; set; }

	public static Transform nullTransform
	{
		get
		{
			if (m_nullGameObject == null)
			{
				m_nullGameObject = new GameObject("nullobj");
				m_nullGameObject.transform.position = Vector3.zero;
				m_nullGameObject.transform.rotation = Quaternion.identity;
				m_nullGameObject.transform.localScale = Vector3.one;
			}
			return m_nullGameObject.transform;
		}
	}

	public static PersistentScripts Instance => m_instance;

	public void OnApplicationQuit()
	{
		IsApplicationQuitting = true;
	}

	private void Awake()
	{
		if (m_instance != null)
		{
			Debug.LogError("More than one Bootstrap instance has been created, it expects to be a singleton.", this);
			return;
		}
		m_instance = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		PrimitiveDrawer.Awake(this);
	}

	protected virtual void OnDestroy()
	{
		if (m_instance == this)
		{
			m_instance = null;
			if (m_nullGameObject != null)
			{
				UnityEngine.Object.Destroy(m_nullGameObject);
				m_nullGameObject = null;
			}
		}
	}

	public void Update()
	{
		if (GCCollectFreq > 0 && Time.frameCount % GCCollectFreq == 0)
		{
			GC.Collect();
		}
	}
}
