using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class AudioProximityDetector : MonoBehaviour
{
	public Player m_Player;

	public SphereCollider m_SphereCollider;

	private float m_fTimeTillNextCheck;

	private static bool ms_bPhysicsTick;

	private static bool ms_bUpdateAudioPending;

	private static int ms_NearbyElectricFences;

	private static bool ms_ElectricFencesEnabled;

	private static float ms_NearestFenceDistance = float.MaxValue;

	private const float WWISE_FENCE_PARAM_MAX = 10f;

	private const string ELECTRIC_FENCE_TAG = "ElectricFence";

	private void Awake()
	{
		if (m_SphereCollider == null)
		{
			m_SphereCollider = GetComponent<SphereCollider>();
		}
	}

	private void FixedUpdate()
	{
		if (!ms_bPhysicsTick)
		{
			ms_bPhysicsTick = true;
			if (m_fTimeTillNextCheck > UpdateManager.fixedDeltaTime)
			{
				m_fTimeTillNextCheck -= UpdateManager.fixedDeltaTime;
			}
			else
			{
				ms_bUpdateAudioPending = true;
			}
		}
	}

	private void Update()
	{
		ms_bPhysicsTick = false;
		if (!ms_bUpdateAudioPending)
		{
			return;
		}
		m_fTimeTillNextCheck = 0.5f;
		ms_bUpdateAudioPending = false;
		bool flag = ms_NearbyElectricFences > 0;
		if (flag != ms_ElectricFencesEnabled)
		{
			ms_ElectricFencesEnabled = flag;
			if (ms_ElectricFencesEnabled)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Electric_Fence, base.gameObject);
			}
			else
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_Electric_Fence, base.gameObject);
				AudioController.SetParameter(Game_Parameter.Distance_From_Fence, 0f);
			}
		}
		if (ms_ElectricFencesEnabled)
		{
			float num = Mathf.Sqrt(ms_NearestFenceDistance);
			num /= m_SphereCollider.radius;
			num = 10f - Mathf.Clamp(num * 10f, 0f, 10f);
			AudioController.SetParameter(Game_Parameter.Distance_From_Fence, num);
		}
		ms_NearestFenceDistance = float.MaxValue;
		ms_NearbyElectricFences = 0;
	}

	private void OnTriggerStay(Collider collider)
	{
		if (ms_bUpdateAudioPending && !(m_Player == null) && !m_Player.GetIsDisabled() && m_Player.m_NetView.isMine && collider.CompareTag("ElectricFence"))
		{
			ms_NearbyElectricFences++;
			Vector3 cachedCurrentPosition = m_Player.m_CachedCurrentPosition;
			float num = Vector2.SqrMagnitude(cachedCurrentPosition - collider.gameObject.transform.position);
			if (num < ms_NearestFenceDistance)
			{
				ms_NearestFenceDistance = num;
			}
		}
	}

	protected virtual void OnDestroy()
	{
		ms_NearbyElectricFences = 0;
		ms_ElectricFencesEnabled = false;
		ms_bPhysicsTick = false;
	}
}
