using UnityEngine;

public class BaseOptionItem
{
	protected MonoBehaviour m_theUIObject;

	protected float m_InitialValue;

	protected float m_CurrentValue;

	protected float m_DefaultValue;

	protected string m_SaveKey;

	protected bool m_bDirty;

	public bool isDirty => m_bDirty;

	public BaseOptionItem(float defaultValue)
	{
		m_DefaultValue = defaultValue;
	}

	public BaseOptionItem(MonoBehaviour theUIObject, float defaultValue)
	{
		m_theUIObject = theUIObject;
		m_DefaultValue = defaultValue;
	}

	public virtual void Initialise()
	{
		m_CurrentValue = m_InitialValue;
		SyncUIObject(bForce: true);
		m_bDirty = false;
	}

	public virtual void OnValueChanged()
	{
		m_bDirty = m_CurrentValue != m_InitialValue;
	}

	protected virtual void SyncUIObject(bool bForce = false)
	{
	}

	public virtual void OnApply()
	{
		m_InitialValue = m_CurrentValue;
		m_bDirty = false;
		if (GlobalSave.GetInstance() != null && !string.IsNullOrEmpty(m_SaveKey))
		{
			GlobalSave.GetInstance().Set(m_SaveKey, (int)m_CurrentValue);
		}
		AudioController instance = AudioController.Instance;
		if (instance != null)
		{
			PlayerPrefs.SetInt("BootFlowAudio:SFXVol", instance.m_SFXVolume);
			PlayerPrefs.SetInt("BootFlowAudio:MusicVol", instance.m_MusicVolume);
		}
	}

	protected float GetValueFromGlobalSave(float defaultValue)
	{
		float value = defaultValue;
		if (GlobalSave.GetInstance() != null)
		{
			GlobalSave.GetInstance().Get(m_SaveKey, out value, defaultValue);
		}
		return value;
	}

	public string GetSaveKey()
	{
		return m_SaveKey;
	}

	public virtual void ResetToDefault()
	{
		m_CurrentValue = m_DefaultValue;
		SyncUIObject();
		m_bDirty = true;
	}

	public void ResetToInitialValue()
	{
		m_CurrentValue = m_InitialValue;
		SyncUIObject();
		m_bDirty = false;
	}

	public bool IsUIObject(GameObject theObject)
	{
		if (theObject == null || m_theUIObject == null)
		{
			return false;
		}
		if (theObject == m_theUIObject.gameObject || theObject.transform.parent.gameObject == m_theUIObject.gameObject)
		{
			return true;
		}
		return false;
	}
}
