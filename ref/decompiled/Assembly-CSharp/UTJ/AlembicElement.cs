using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UTJ;

[ExecuteInEditMode]
public abstract class AlembicElement : MonoBehaviour
{
	public AlembicStream m_abcStream;

	public AbcAPI.aiObject m_abcObj;

	public AbcAPI.aiSchema m_abcSchema;

	public GCHandle m_thisHandle;

	protected Transform m_trans;

	private bool m_verbose;

	private bool m_pendingUpdate;

	private static void ConfigCallback(IntPtr __this, ref AbcAPI.aiConfig config)
	{
		AlembicElement alembicElement = GCHandle.FromIntPtr(__this).Target as AlembicElement;
		alembicElement.AbcGetConfig(ref config);
	}

	private static void SampleCallback(IntPtr __this, AbcAPI.aiSample sample, bool topologyChanged)
	{
		AlembicElement alembicElement = GCHandle.FromIntPtr(__this).Target as AlembicElement;
		alembicElement.AbcSampleUpdated(sample, topologyChanged);
	}

	public T GetOrAddComponent<T>() where T : Component
	{
		T val = base.gameObject.GetComponent<T>();
		if (val == null)
		{
			val = base.gameObject.AddComponent<T>();
		}
		return val;
	}

	public virtual void OnDestroy()
	{
		m_thisHandle.Free();
		if (!Application.isPlaying)
		{
			AbcDestroy();
			if (m_abcStream != null)
			{
				m_abcStream.AbcRemoveElement(this);
			}
		}
	}

	public virtual void AbcSetup(AlembicStream abcStream, AbcAPI.aiObject abcObj, AbcAPI.aiSchema abcSchema)
	{
		m_abcStream = abcStream;
		m_abcObj = abcObj;
		m_abcSchema = abcSchema;
		m_thisHandle = GCHandle.Alloc(this);
		m_trans = GetComponent<Transform>();
		IntPtr arg = GCHandle.ToIntPtr(m_thisHandle);
		AbcAPI.aiSchemaSetConfigCallback(abcSchema, ConfigCallback, arg);
		AbcAPI.aiSchemaSetSampleCallback(abcSchema, SampleCallback, arg);
	}

	public virtual void AbcDestroy()
	{
	}

	public virtual void AbcGetConfig(ref AbcAPI.aiConfig config)
	{
	}

	public abstract void AbcSampleUpdated(AbcAPI.aiSample sample, bool topologyChanged);

	public abstract void AbcUpdate();

	protected void AbcVerboseLog(string msg)
	{
		if (m_abcStream != null && m_abcStream.m_verbose)
		{
			Debug.Log(msg);
		}
	}

	protected void AbcDirty()
	{
		m_pendingUpdate = true;
	}

	protected void AbcClean()
	{
		m_pendingUpdate = false;
	}

	protected bool AbcIsDirty()
	{
		return m_pendingUpdate;
	}
}
