using UnityEngine;

[ExecuteInEditMode]
public class CutsceneFlooredMonobehaviour : T17MonoBehaviour
{
	public int m_FloorIndex;

	private Transform m_Transform;

	private FloorManager.Floor[] m_CachedFloors;

	protected override void Awake()
	{
		base.Awake();
		m_Transform = GetComponent<Transform>();
	}

	protected void Start()
	{
		if (FloorManager.GetInstance() != null)
		{
			m_CachedFloors = FloorManager.GetInstance().m_PrisonFloors;
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_CachedFloors != null)
		{
			m_CachedFloors = null;
		}
	}

	protected void LateUpdate()
	{
		if (Application.isPlaying)
		{
			if (m_CachedFloors == null && FloorManager.GetInstance() != null)
			{
				m_CachedFloors = FloorManager.GetInstance().m_PrisonFloors;
			}
			if (m_CachedFloors != null)
			{
				m_Transform.position = GetPosition();
			}
		}
		else if (Application.isEditor)
		{
			FloorManager instance = FloorManager.GetInstance();
			if (instance != null)
			{
				int floorIndex = instance.FindFloorIndexAtZ(m_Transform.position.z);
				m_FloorIndex = floorIndex;
			}
		}
	}

	public Vector3 GetPosition()
	{
		if (m_Transform == null || m_CachedFloors == null)
		{
			string text = "Cutscene floored monobeheviour Start init failed to do anything: " + base.transform.name + " child of " + base.transform.parent.name;
			PrisonData.LevelInfo currentLevelInfo = LevelScript.GetCurrentLevelInfo();
			if (currentLevelInfo != null)
			{
				text = text + " in level " + currentLevelInfo.m_PrisonEnum;
			}
			CutsceneManagerBase instance = CutsceneManagerBase.GetInstance();
			if (instance != null && instance.GetCurrentPlayingCutscene() != null)
			{
				text = text + " playing cutscene " + instance.GetCurrentPlayingCutscene().name;
			}
			T17NetManager.LogGoogleException(text);
			return base.transform.position;
		}
		Vector3 position = m_Transform.position;
		if (m_CachedFloors != null)
		{
			position.z = m_CachedFloors[m_FloorIndex].m_zPos;
		}
		return position;
	}
}
