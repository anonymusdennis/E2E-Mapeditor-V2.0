using UnityEngine;

[AddComponentMenu("Wwise/AkGameObj")]
[ExecuteInEditMode]
public class AkGameObj : MonoBehaviour
{
	private const int ALL_LISTENER_MASK = 255;

	public AkGameObjPositionOffsetData m_positionOffsetData;

	private bool isPlayer;

	private AkGameObjEnvironmentData m_envData;

	public int listenerMask = 1;

	private const int kScheduledSoundSpread = 30;

	private const int kScheduledSoundSpreadPlayer = 4;

	private int ms_updatePositionScheduler;

	private int m_updatePositionTicks;

	[SerializeField]
	private bool isStaticObject;

	private AkGameObjPositionData m_posData;

	private Collider m_Collider;

	[SerializeField]
	private AkGameObjPosOffsetData m_posOffsetData;

	public bool isEnvironmentAware
	{
		get
		{
			return isPlayer;
		}
		set
		{
			isPlayer = value;
		}
	}

	private void Awake()
	{
		if (!isStaticObject)
		{
			m_posData = new AkGameObjPositionData();
		}
		m_Collider = GetComponent<Collider>();
		AKRESULT aKRESULT = AkSoundEngine.RegisterGameObj(base.gameObject, base.gameObject.name, (uint)listenerMask & 0xFFu);
		if (aKRESULT == AKRESULT.AK_Success)
		{
			Vector3 position = GetPosition();
			AkSoundEngine.SetObjectPosition(base.gameObject, position.x, position.y, position.z, base.transform.forward.x, base.transform.forward.y, base.transform.forward.z, base.transform.up.x, base.transform.up.y, base.transform.up.z);
			ms_updatePositionScheduler++;
			m_updatePositionTicks = ms_updatePositionScheduler % 30;
			Player component = base.gameObject.GetComponent<Player>();
			isPlayer = component != null;
			if (isEnvironmentAware)
			{
				m_envData = new AkGameObjEnvironmentData();
				m_envData.AddAkEnvironment(base.gameObject, base.gameObject);
			}
		}
	}

	private void CheckStaticStatus()
	{
	}

	private void OnEnable()
	{
		base.enabled = !isStaticObject;
	}

	private void OnDestroy()
	{
		AkUnityEventHandler[] components = base.gameObject.GetComponents<AkUnityEventHandler>();
		AkUnityEventHandler[] array = components;
		foreach (AkUnityEventHandler akUnityEventHandler in array)
		{
			if (akUnityEventHandler.triggerList.Contains(-358577003))
			{
				akUnityEventHandler.DoDestroy();
			}
		}
		if (AkSoundEngine.IsInitialized())
		{
			AkSoundEngine.UnregisterGameObj(base.gameObject);
		}
	}

	private void Update()
	{
		if (isEnvironmentAware && m_envData != null)
		{
			m_envData.UpdateAuxSend(base.gameObject, base.transform.position);
		}
		if (!isStaticObject && --m_updatePositionTicks <= 0)
		{
			if (isPlayer)
			{
				m_updatePositionTicks = 4;
			}
			else
			{
				m_updatePositionTicks = 30;
			}
			Vector3 position = GetPosition();
			Vector3 forward = base.transform.forward;
			Vector3 up = base.transform.up;
			if (m_posData.position != position || m_posData.forward != forward || m_posData.up != up)
			{
				m_posData.position = position;
				m_posData.forward = forward;
				m_posData.up = up;
				AkSoundEngine.SetObjectPosition(base.gameObject, position.x, position.y, position.z, forward.x, forward.y, forward.z, up.x, up.y, up.z);
			}
		}
	}

	public Vector3 GetPosition()
	{
		if (m_positionOffsetData != null)
		{
			Vector3 vector = base.transform.rotation * m_positionOffsetData.positionOffset;
			return base.transform.position + vector;
		}
		return base.transform.position;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (isEnvironmentAware && m_envData != null)
		{
			m_envData.AddAkEnvironment(other.gameObject, base.gameObject);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (isEnvironmentAware && m_envData != null)
		{
			m_envData.RemoveAkEnvironment(other.gameObject, base.gameObject, m_Collider);
		}
	}
}
