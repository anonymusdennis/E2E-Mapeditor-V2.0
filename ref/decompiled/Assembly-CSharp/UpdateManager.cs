using System;
using UnityEngine;

public class UpdateManager : T17MonoBehaviour
{
	private static UpdateManager m_Instance;

	private IUpdateController[] m_UpdateControllers;

	private string[] m_UpdateControllerStrings;

	private bool m_bInited;

	private IUpdateController[] m_UpdateControllers_RunPreUpdates;

	private IUpdateController[] m_UpdateControllers_RunUpdates;

	private IUpdateController[] m_UpdateControllers_RunPreFixedUpdates;

	private IUpdateController[] m_UpdateControllers_RunFixedUpdates;

	private IUpdateController[] m_UpdateControllers_RunLateUpdates;

	public static float systemDeltaTime = 1f / 30f;

	public static float deltaTime = 1f / 30f;

	public static float fixedDeltaTime = 1f / 30f;

	public static float deltaTimeSinceStart;

	public static float fixedDeltaTimeSinceStart;

	public static float time;

	public static float smoothTime;

	public static float fixedTime;

	public static int frameCount;

	public static float unscaledTimeSinceStart;

	public static float unscaledDeltaTime;

	private static int numHeavyCpuLoad;

	private static int numHeavyLoadsDeferred;

	private static int numDeferredSafetyTicks = 30;

	public static int uScheduleGCFrames = -1;

	public static bool bGarbageCollectReq;

	public static UpdateManager GetInstance()
	{
		return m_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		if (m_Instance != null)
		{
			UnityEngine.Object.Destroy(m_Instance);
		}
		m_Instance = this;
		m_bInited = false;
		time = Time.time;
		smoothTime = Time.time;
		fixedTime = Time.fixedTime;
		frameCount = Time.frameCount;
		unscaledTimeSinceStart = Time.unscaledTime;
		unscaledDeltaTime = Time.unscaledDeltaTime;
		deltaTimeSinceStart = 0f;
		fixedDeltaTimeSinceStart = 0f;
		numDeferredSafetyTicks = 30;
		m_UpdateControllers = new IUpdateController[15]
		{
			new PeriodicUpdateController(0),
			new PeriodicUpdateController(250),
			new PeriodicUpdateController(1000),
			new PeriodicUpdateController(2000),
			new CharacterUpdateController(),
			new ItemUpdateController(),
			new SoundUpdateController(),
			new TrackedElementReporterUpdateController(),
			new StagedSheduledUpdateController(0.5f, "NodeCanvas"),
			new StagedSheduledUpdateController(0.2f, "FastInteractions"),
			new StagedSheduledUpdateController(0.5f, "AI_EVents"),
			new StagedSheduledUpdateController(0.25f, "AI_Events_slow"),
			new StagedSheduledUpdateController(0.1f, "World_Slow"),
			new FakeCharacterUpdateController(),
			new CrowdCharacterUpdateController()
		};
		m_UpdateControllerStrings = new string[15];
		for (int i = 0; i < 15; i++)
		{
			string[] updateControllerStrings = m_UpdateControllerStrings;
			int num = i;
			UpdateCategory updateCategory = (UpdateCategory)i;
			updateControllerStrings[num] = updateCategory.ToString();
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		return T17BehaviourManager.INITSTATE.IS_REQUIRES_LAST;
	}

	public override void StartInitLast()
	{
		IUpdateController[] array = new IUpdateController[m_UpdateControllers.Length];
		base.StartInit();
		m_bInited = true;
		int num = 0;
		for (int i = 0; i < m_UpdateControllers.Length; i++)
		{
			if (m_UpdateControllers[i].RequiresRunPreUpdates())
			{
				array[num++] = m_UpdateControllers[i];
			}
		}
		m_UpdateControllers_RunPreUpdates = new IUpdateController[num];
		for (int i = 0; i < num; i++)
		{
			m_UpdateControllers_RunPreUpdates[i] = array[i];
		}
		num = 0;
		for (int i = 0; i < m_UpdateControllers.Length; i++)
		{
			if (m_UpdateControllers[i].RequiresRunUpdates())
			{
				array[num++] = m_UpdateControllers[i];
			}
		}
		m_UpdateControllers_RunUpdates = new IUpdateController[num];
		for (int i = 0; i < num; i++)
		{
			m_UpdateControllers_RunUpdates[i] = array[i];
		}
		num = 0;
		for (int i = 0; i < m_UpdateControllers.Length; i++)
		{
			if (m_UpdateControllers[i].RequiresPreFixedUpdate())
			{
				array[num++] = m_UpdateControllers[i];
			}
		}
		m_UpdateControllers_RunPreFixedUpdates = new IUpdateController[num];
		for (int i = 0; i < num; i++)
		{
			m_UpdateControllers_RunPreFixedUpdates[i] = array[i];
		}
		num = 0;
		for (int i = 0; i < m_UpdateControllers.Length; i++)
		{
			if (m_UpdateControllers[i].RequiresFixedUpdate())
			{
				array[num++] = m_UpdateControllers[i];
			}
		}
		m_UpdateControllers_RunFixedUpdates = new IUpdateController[num];
		for (int i = 0; i < num; i++)
		{
			m_UpdateControllers_RunFixedUpdates[i] = array[i];
		}
		num = 0;
		for (int i = 0; i < m_UpdateControllers.Length; i++)
		{
			if (m_UpdateControllers[i].RequiresLateUpdates())
			{
				array[num++] = m_UpdateControllers[i];
			}
		}
		m_UpdateControllers_RunLateUpdates = new IUpdateController[num];
		for (int i = 0; i < num; i++)
		{
			m_UpdateControllers_RunLateUpdates[i] = array[i];
		}
	}

	private void Update()
	{
		if (!m_bInited)
		{
			return;
		}
		time = Time.time;
		frameCount = Time.frameCount;
		systemDeltaTime = Time.deltaTime;
		deltaTimeSinceStart += systemDeltaTime;
		smoothTime += Time.smoothDeltaTime;
		unscaledTimeSinceStart = Time.unscaledTime;
		unscaledDeltaTime = Time.unscaledDeltaTime;
		numHeavyCpuLoad = 0;
		if (numHeavyLoadsDeferred > 0)
		{
			if (numHeavyLoadsDeferred > numDeferredSafetyTicks)
			{
				numHeavyCpuLoad = -numHeavyLoadsDeferred;
				if (numDeferredSafetyTicks > 0)
				{
					numDeferredSafetyTicks--;
				}
			}
			numHeavyLoadsDeferred = 0;
		}
		int num = m_UpdateControllers_RunPreUpdates.Length;
		for (int i = 0; i < num; i++)
		{
			m_UpdateControllers_RunPreUpdates[i].RunPreUpdates();
			deltaTime = systemDeltaTime;
		}
		num = m_UpdateControllers_RunUpdates.Length;
		for (int i = 0; i < num; i++)
		{
			m_UpdateControllers_RunUpdates[i].RunUpdates();
			deltaTime = systemDeltaTime;
		}
	}

	private void FixedUpdate()
	{
		if (m_bInited)
		{
			fixedTime = Time.fixedTime;
			fixedDeltaTimeSinceStart += Time.fixedDeltaTime;
			int num = m_UpdateControllers_RunPreFixedUpdates.Length;
			for (int i = 0; i < num; i++)
			{
				m_UpdateControllers_RunPreFixedUpdates[i].RunPreFixedUpdates();
			}
			num = m_UpdateControllers_RunFixedUpdates.Length;
			for (int i = 0; i < num; i++)
			{
				m_UpdateControllers_RunFixedUpdates[i].RunFixedUpdates();
			}
		}
	}

	private void LateUpdate()
	{
		if (m_bInited)
		{
			time = Time.time;
			frameCount = Time.frameCount;
			systemDeltaTime = Time.deltaTime;
			unscaledTimeSinceStart = Time.unscaledTime;
			unscaledDeltaTime = Time.unscaledDeltaTime;
			int num = m_UpdateControllers_RunLateUpdates.Length;
			for (int i = 0; i < num; i++)
			{
				m_UpdateControllers_RunLateUpdates[i].RunLateUpdates();
			}
		}
		if (bGarbageCollectReq)
		{
			if (AquireHeavyCpuLock())
			{
				bGarbageCollectReq = false;
				GC.Collect();
			}
		}
		else if (uScheduleGCFrames > 0)
		{
			uScheduleGCFrames--;
			if (uScheduleGCFrames == 0)
			{
				bGarbageCollectReq = true;
			}
		}
	}

	public void Register(IControlledUpdate behaviour, UpdateCategory category)
	{
		if (behaviour != null && category != UpdateCategory.Count)
		{
			m_UpdateControllers[(int)category].Register(behaviour);
		}
	}

	public void Unregister(IControlledUpdate behaviour, UpdateCategory category)
	{
		if (behaviour != null && category != UpdateCategory.Count)
		{
			m_UpdateControllers[(int)category].Unregister(behaviour);
		}
	}

	public void LevelUnload()
	{
		for (int i = 0; i < m_UpdateControllers.Length; i++)
		{
			m_UpdateControllers[i].UnregisterAll();
		}
	}

	public static bool AquireHeavyCpuLock()
	{
		if (numHeavyCpuLoad <= 0)
		{
			numHeavyCpuLoad++;
			return true;
		}
		numHeavyLoadsDeferred++;
		return false;
	}

	public static bool IsHeavyCpuLocked()
	{
		return numHeavyCpuLoad > 0;
	}
}
