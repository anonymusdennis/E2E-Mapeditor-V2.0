using System;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraManager : MonoBehaviour, IControlledUpdate
{
	public enum CameraOpModes
	{
		Unassigned,
		Game,
		Cutscene
	}

	public enum PlayerBindingID
	{
		CM_PBID_UNSET = 0,
		CM_PBID_PLAYER_ALPHA = 1,
		CM_PBID_FIRST = 1,
		CM_PBID_PLAYER_BETA = 2,
		CM_PBID_PLAYER_GAMMA = 3,
		CM_PBID_PLAYER_DELTA = 4,
		CM_PBID_LAST = 4,
		CM_PBID_WORLD_CAM_TOGGLE = 666,
		CM_PBID_WORLD_CAM_VISIBLE = 999
	}

	public delegate void CameraManagerHandler();

	public delegate void CameraManagerModeChangeHandler(CameraOpModes newOpMode);

	public delegate void ManagerCreationHandler(CameraManager sender);

	[Serializable]
	public class CameraBinding
	{
		public PlayerBindingID m_PlayerBinding;

		public Camera m_Camera;

		public bool initedfov;

		public float initialfov;

		private Character _m_Character;

		[HideInInspector]
		public Vector3 m_TargetPosition = Vector3.zero;

		[HideInInspector]
		public Vector3 m_NewTargetPosition = Vector3.zero;

		[HideInInspector]
		public uint m_ListenerIndex = uint.MaxValue;

		[HideInInspector]
		public int m_CameraID;

		[HideInInspector]
		public Vector2 m_MaxCameraPos;

		[HideInInspector]
		public Vector2 m_MinCameraPos;

		[HideInInspector]
		public float m_NormalizedViewportHeight = 1f;

		[HideInInspector]
		public CameraView m_CameraView;

		[HideInInspector]
		public CullerUpdateMode m_CullerUpdateMode;

		[HideInInspector]
		public CharacterStencilRenderer m_CharacterStencilRenderer;

		[HideInInspector]
		public BlurOptimized m_Blur;

		public VectorShake m_CombatHitShake = new VectorShake();

		[HideInInspector]
		public Vector3 m_FascadeTargetPos = Vector3.zero;

		private OverscanCamera m_OverscanCamera;

		[HideInInspector]
		public PreCuller m_CameraPreCuller;

		[HideInInspector]
		public Character m_Character
		{
			get
			{
				return _m_Character;
			}
			set
			{
				if (_m_Character != null)
				{
					Character character = _m_Character;
					character.OnCharacterTookDamage = (Character.CharacterToCharacterEvent)Delegate.Remove(character.OnCharacterTookDamage, new Character.CharacterToCharacterEvent(OnBoundCharacterAttacked));
				}
				_m_Character = value;
				if (_m_Character != null && _m_Character.ShouldBoundCameraDoShakes())
				{
					Character character2 = _m_Character;
					character2.OnCharacterTookDamage = (Character.CharacterToCharacterEvent)Delegate.Combine(character2.OnCharacterTookDamage, new Character.CharacterToCharacterEvent(OnBoundCharacterAttacked));
				}
			}
		}

		public OverscanCamera GetOverscanCamera()
		{
			if (m_OverscanCamera != null)
			{
				return m_OverscanCamera;
			}
			if (m_Camera != null)
			{
				m_OverscanCamera = m_Camera.GetComponent<OverscanCamera>();
				return m_OverscanCamera;
			}
			return null;
		}

		private void OnBoundCharacterAttacked(Character sender, Character attacker)
		{
			m_CombatHitShake.Init();
		}

		~CameraBinding()
		{
			if (_m_Character != null)
			{
				Character character = _m_Character;
				character.OnCharacterTookDamage = (Character.CharacterToCharacterEvent)Delegate.Remove(character.OnCharacterTookDamage, new Character.CharacterToCharacterEvent(OnBoundCharacterAttacked));
			}
		}
	}

	public delegate void CameraViewChangedHandler(CameraBinding binding);

	private CameraOpModes m_OpMode = CameraOpModes.Game;

	private CutsceneCameraTrackableObject m_CachedTrackableObject;

	public const int CUTSCENE_DEFAULT_FLOOR_OFFSET = 2;

	public Vector2 m_FollowDeadZone = new Vector2(0.5f, 0.5f);

	public Vector2 m_Smooth = new Vector2(3f, 3f);

	private float[] m_ZOffsetsFromPlayer = new float[4];

	public int m_ZOffsetFromPlayerFullScreen = -822;

	public int m_ZOffsetFromPlayerHalfScreen = -822;

	public int m_ZOffsetFromPlayerQuarterScreen = -822;

	public int m_ZOffsetPIP = -822;

	private float m_ZOffsetFromPlayer = -20f;

	private int m_ZOffsetForFacade;

	public float m_SplitThreshold = 10f;

	[Tooltip("When the camera is told to shake, how much should it move around it's target position?")]
	public float m_CombatHitShakeIntensity = 0.5f;

	[Tooltip("How long should the camera shake for when told to?")]
	public float m_CombatHitShakeDuration = 0.1f;

	public bool m_bAlwaysSplit;

	public Vector2 m_MapBoundsBR;

	public Vector2 m_MapBoundsTL;

	private bool m_bRun;

	private bool m_bAllowXCamera = true;

	private bool m_bAllowYCamera = true;

	public bool m_bShowFacade = true;

	public bool m_ForceRefresh;

	private bool m_bResolutionChanged;

	private bool m_bIsFullscreen;

	private bool m_BlurEffectEnabled = true;

	private bool m_BlurEffectAllowed = true;

	public Vector3 m_PlayerSelectCameraPosition = new Vector3(62f, 16f, 0f);

	private const int m_SpeechBubbleShiftInterval = 1;

	private int m_NumIntervalsBeforeBubbleCheck;

	private float m_fFract = 1f;

	private int m_CachedCameraCount;

	public bool EDITOR_SimulateConsoleSplitscreen;

	private Rect[] m_FixedCamereaViewportRects_1;

	private Rect[] m_FixedCamereaViewportRects_2;

	private Rect[] m_FixedCamereaViewportRects_3;

	private Rect[] m_FixedCamereaViewportRects_4;

	private static CameraManager m_Instance;

	public CameraManagerHandler OnTargetsUpdated;

	public CameraManagerHandler OnActiveCamerasUpdated;

	public CameraManagerHandler OnVsyncOptionChanged;

	public CameraManagerModeChangeHandler OnCameraOpModeChanged;

	public CameraBinding[] m_CameraBindings = new CameraBinding[4];

	public CameraBinding[] m_PIPBindings = new CameraBinding[5];

	private float kCamWidthFromFarPlaneCentre = 512.978f;

	private const float schmepsilon = 5E-06f;

	public Character[] m_DebugCharactersForPIP = new Character[4];

	public int m_ActiveCameraCount;

	private const float HALF_SCREEN_BUCKET_SPLIT_WIDTH = 0.5f;

	private const float MULTI_SCREEN_BUCKET_SPLIT_WIDTH = 0.79f;

	private const float MULTI_SCREEN_BUCKET_SPLIT_HEIGHT = 0.76f;

	public static event ManagerCreationHandler CreatedEvent;

	public static event ManagerCreationHandler DestroyedEvent;

	public static event CameraViewChangedHandler CameraViewChangedEvent;

	public static CameraManager GetInstance()
	{
		return m_Instance;
	}

	private void Awake()
	{
		m_Instance = this;
		m_bRun = false;
		if (CameraManager.CreatedEvent != null)
		{
			CameraManager.CreatedEvent(this);
		}
		SetUpCameraViewportRects();
	}

	protected virtual void OnDestroy()
	{
		DestoryCameraViewportRects();
		UpdateManager instance = UpdateManager.GetInstance();
		if (instance != null)
		{
			instance.Unregister(this, UpdateCategory.RapidPeriodic);
		}
		if (m_Instance == this)
		{
			if (CameraManager.DestroyedEvent != null)
			{
				CameraManager.DestroyedEvent(this);
			}
			m_Instance = null;
		}
		Canvas.willRenderCanvases -= Canvas_willRenderCanvases;
	}

	private void Start()
	{
		UpdateManager instance = UpdateManager.GetInstance();
		if (instance != null)
		{
			instance.Register(this, UpdateCategory.RapidPeriodic);
		}
		int num = 0;
		if (m_CameraBindings != null && m_CameraBindings.Length > 0)
		{
			m_bRun = true;
			for (int i = 0; i < m_CameraBindings.Length; i++)
			{
				if (m_CameraBindings[i] != null && m_CameraBindings[i].m_Camera != null)
				{
					if (i == 0)
					{
						m_CameraBindings[i].m_Camera.rect = new Rect(0f, 0f, 1f, 1f);
						m_CameraBindings[i].m_NormalizedViewportHeight = 1f;
					}
					else
					{
						m_CameraBindings[i].m_Camera.enabled = false;
					}
					m_CameraBindings[i].m_CameraPreCuller = m_CameraBindings[i].m_Camera.GetComponent<PreCuller>();
					if (m_CameraBindings[i].m_CameraPreCuller != null)
					{
						m_CameraBindings[i].m_CameraID = num;
						m_CameraBindings[i].m_CameraPreCuller.CurrentCameraID = num;
					}
					CharacterStencilRenderer componentInChildren = m_CameraBindings[i].m_Camera.GetComponentInChildren<CharacterStencilRenderer>();
					if (componentInChildren != null)
					{
						m_CameraBindings[i].m_CharacterStencilRenderer = componentInChildren;
					}
					BlurOptimized component = m_CameraBindings[i].m_Camera.GetComponent<BlurOptimized>();
					if (component != null)
					{
						m_CameraBindings[i].m_Blur = component;
						if (m_BlurEffectEnabled)
						{
							component.enabled = true;
						}
					}
					num++;
				}
				m_CameraBindings[i].m_CombatHitShake.m_EffectTime = m_CombatHitShakeDuration;
				m_CameraBindings[i].m_CombatHitShake.m_MaxRadius = m_CombatHitShakeIntensity;
				AkSoundEngine.SetListenerPosition(0f, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 1000f, (uint)(i + 1));
			}
			SetCameraBounds(m_MapBoundsBR, m_MapBoundsTL);
			int index = FloorManager.GetInstance().GetCurrentMaxFloor() - 1;
			m_ZOffsetForFacade = FloorManager.GetInstance().FindFloorbyIndex(index).m_zPos;
		}
		Canvas.willRenderCanvases += Canvas_willRenderCanvases;
		m_bIsFullscreen = Screen.fullScreen;
	}

	private void Canvas_willRenderCanvases()
	{
		if (m_NumIntervalsBeforeBubbleCheck-- <= 0)
		{
			if (HUDMenuFlow.Instance != null)
			{
				HUDMenuFlow.Instance.ShiftWorldElementsForAnyFacade();
			}
			m_NumIntervalsBeforeBubbleCheck = 1;
		}
	}

	public void SetBlurEffectEnabled(bool bEnable)
	{
		m_BlurEffectEnabled = bEnable;
		if ((!m_BlurEffectAllowed && bEnable) || m_OpMode == CameraOpModes.Cutscene)
		{
			return;
		}
		for (int i = 0; i < m_CameraBindings.Length; i++)
		{
			if (m_CameraBindings[i] != null && m_CameraBindings[i].m_Camera != null && (bool)m_CameraBindings[i].m_Blur)
			{
				m_CameraBindings[i].m_Blur.enabled = bEnable;
			}
		}
	}

	public void SetBlurEffectAllowed(bool bAllowed)
	{
		m_BlurEffectAllowed = bAllowed;
		if (!m_BlurEffectEnabled)
		{
			return;
		}
		for (int i = 0; i < m_CameraBindings.Length; i++)
		{
			if (m_CameraBindings[i] != null && m_CameraBindings[i].m_Camera != null && (bool)m_CameraBindings[i].m_Blur)
			{
				m_CameraBindings[i].m_Blur.enabled = bAllowed;
			}
		}
	}

	public void RecalculateCameraIndexOfBlurEffect(PlayerBindingID bindingId)
	{
		for (int i = 0; i < m_CameraBindings.Length; i++)
		{
			if (m_CameraBindings[i] != null && m_CameraBindings[i].m_PlayerBinding == bindingId && m_CameraBindings[i].m_Blur != null)
			{
				m_CameraBindings[i].m_Blur.RecalculateIndexOfCamera(this);
			}
		}
	}

	public bool GetBlurEffectEnabled()
	{
		return m_BlurEffectEnabled;
	}

	public static bool WillBubbleShiftRunThisFrame()
	{
		if (m_Instance == null)
		{
			return false;
		}
		return m_Instance.m_NumIntervalsBeforeBubbleCheck <= 0;
	}

	public Camera GetCamera(PlayerBindingID bindingID)
	{
		for (int i = 0; i < m_CameraBindings.Length; i++)
		{
			if (m_CameraBindings[i].m_PlayerBinding == bindingID)
			{
				return m_CameraBindings[i].m_Camera;
			}
		}
		return null;
	}

	public int GetCameraID(PlayerBindingID bindingID)
	{
		for (int i = 0; i < m_CameraBindings.Length; i++)
		{
			if (m_CameraBindings[i].m_PlayerBinding == bindingID)
			{
				return m_CameraBindings[i].m_CameraID;
			}
		}
		return -1;
	}

	public Camera GetCameraFromID(int CameraID)
	{
		for (int i = 0; i < m_CameraBindings.Length; i++)
		{
			if (m_CameraBindings[i].m_CameraID == CameraID)
			{
				return m_CameraBindings[i].m_Camera;
			}
		}
		return null;
	}

	public CameraBinding GetCameraBinding(Camera camera)
	{
		if (camera != null)
		{
			for (int i = 0; i < m_CameraBindings.Length; i++)
			{
				if (m_CameraBindings[i].m_Camera == camera)
				{
					return m_CameraBindings[i];
				}
			}
		}
		return null;
	}

	public int GetUsedCameraCountForCulling()
	{
		return m_CachedCameraCount;
	}

	public int GetUsedCameraCount(bool setCameraTargetPosToCharPos = false)
	{
		if (m_OpMode == CameraOpModes.Cutscene)
		{
			m_CachedCameraCount = 1;
			if (m_CameraBindings[0] != null && m_CameraBindings[0].m_Camera != null)
			{
				m_CameraBindings[0].m_NewTargetPosition = GetCameraCullTarget(m_CameraBindings[0].m_Camera);
			}
			return 1;
		}
		int num = 0;
		if (m_CameraBindings != null)
		{
			for (int i = 0; i < m_CameraBindings.Length; i++)
			{
				if (!(m_CameraBindings[i].m_Camera != null) || (!(m_CameraBindings[i].m_Character != null) && !(m_CameraBindings[i].m_TargetPosition != Vector3.zero)))
				{
					continue;
				}
				if (setCameraTargetPosToCharPos)
				{
					if (m_CameraBindings[i].m_Character != null)
					{
						m_CameraBindings[i].m_NewTargetPosition = m_CameraBindings[i].m_Character.m_CachedCurrentPosition;
						if (m_CameraBindings[i].m_Character.GetInteractiveObject() != null)
						{
							m_CameraBindings[i].m_NewTargetPosition += m_CameraBindings[i].m_Character.GetInteractiveObject().GetCameraOffset();
						}
						if (m_CameraBindings[i].m_Character.IsBeingCarried())
						{
							m_CameraBindings[i].m_NewTargetPosition = m_CameraBindings[i].m_Character.GetPickedUpByCharacter().m_CachedCurrentPosition;
						}
					}
					else if (m_CameraBindings[i].m_TargetPosition != Vector3.zero)
					{
						m_CameraBindings[i].m_NewTargetPosition = m_CameraBindings[i].m_TargetPosition;
					}
				}
				num++;
			}
		}
		m_CachedCameraCount = num;
		return num;
	}

	public int GetUsedPIPCameraCount()
	{
		if (m_OpMode == CameraOpModes.Cutscene)
		{
			return 0;
		}
		return 0;
	}

	public static bool ToggleFacade(bool bPos, bool bJustRead)
	{
		if (m_Instance != null)
		{
			if (!bJustRead)
			{
				m_Instance.m_bShowFacade = bPos;
				m_Instance.m_ForceRefresh = true;
			}
			return m_Instance.m_bShowFacade;
		}
		return false;
	}

	private float CalculatePixelPerfectOffset(int index)
	{
		if (index < 0 && index >= m_CameraBindings.Length)
		{
			return m_ZOffsetFromPlayerFullScreen;
		}
		CameraBinding cameraBinding = m_CameraBindings[index];
		if (cameraBinding == null)
		{
			return m_ZOffsetFromPlayerFullScreen;
		}
		Camera camera = cameraBinding.m_Camera;
		if (camera == null)
		{
			return m_ZOffsetFromPlayerFullScreen;
		}
		float num = CalculatePcTilesToDisplay(cameraBinding, camera);
		m_fFract = (float)camera.pixelHeight / num;
		if (cameraBinding.m_CameraPreCuller != null)
		{
			cameraBinding.m_CameraPreCuller.CalculateActualCameraSize(num * ((float)camera.pixelWidth / (float)camera.pixelHeight), num);
		}
		float num2 = 1f;
		float num3 = num * num2;
		float num4 = 0.5f * num3 / Mathf.Tan(0.5f * (camera.fieldOfView * ((float)Math.PI / 180f)));
		return 0f - num4;
	}

	private static float CalculatePcTilesToDisplay(CameraBinding binding, Camera currCam)
	{
		float num = 20.25f;
		bool flag = false;
		float num2 = 0f;
		float num3 = currCam.pixelHeight;
		float num4 = currCam.pixelWidth;
		float num5 = 3f;
		if (num4 / num3 > 1.8f)
		{
			flag = true;
			num *= 1.7777778f;
			num2 = num4 * binding.m_NormalizedViewportHeight;
		}
		else
		{
			num2 = num3 * binding.m_NormalizedViewportHeight;
		}
		float num6 = num2 / 32f;
		if (num6 > num)
		{
			switch (Screen.height)
			{
			case 1080:
			case 2160:
				num6 = 20.25f;
				break;
			case 1600:
				num6 = 20f;
				break;
			case 1440:
				num6 = 18f;
				break;
			case 1200:
				num6 = 22.5f;
				break;
			case 1152:
				num6 = 21.54f;
				break;
			case 1050:
				num6 = 19.6875f;
				break;
			case 1024:
				num6 = 19.1875f;
				break;
			case 960:
				num6 = 18f;
				break;
			case 900:
				num6 = 16.875f;
				break;
			case 800:
				num6 = 15f;
				break;
			case 768:
				num6 = 14.45f;
				break;
			case 720:
				num6 = 13.5f;
				break;
			default:
			{
				int i;
				for (i = (int)(num6 / num); num6 / (float)i > num + num5; i++)
				{
				}
				num6 /= (float)i;
				break;
			}
			}
		}
		else
		{
			for (float num7 = num6; num6 + num7 <= num + num5; num6 += num7)
			{
			}
		}
		if (flag)
		{
			num6 *= num3 / num4;
		}
		return num6;
	}

	private static float CalculateConsoleTilesToDisplay(CameraBinding binding, Camera currCam)
	{
		float num = (float)currCam.pixelHeight * binding.m_NormalizedViewportHeight;
		float num2 = num / 32f;
		float num3 = 20f;
		float num4 = ((!(num2 < num3)) ? (num2 * 0.5f) : (num2 * 2f));
		if (Mathf.Abs(num4 - num3) < Mathf.Abs(num2 - num3))
		{
			num2 = num4;
		}
		return num2;
	}

	private void CameraUpdate(int index)
	{
		if (index < 0 && index >= m_CameraBindings.Length)
		{
			return;
		}
		CameraBinding cameraBinding = m_CameraBindings[index];
		if (cameraBinding == null)
		{
			return;
		}
		Camera camera = cameraBinding.m_Camera;
		if (camera == null)
		{
			return;
		}
		if (!cameraBinding.initedfov)
		{
			cameraBinding.initedfov = true;
			cameraBinding.initialfov = camera.fieldOfView;
		}
		camera.fieldOfView = cameraBinding.initialfov * 0.125f;
		CameraView cameraView = cameraBinding.m_CameraView;
		cameraBinding.m_CameraView = CameraView.Normal;
		Character character = cameraBinding.m_Character;
		if (character != null)
		{
			cameraBinding.m_FascadeTargetPos = character.m_Transform.position;
			if (FacadesManager.GetInstance() != null && FacadesManager.GetInstance().ShouldShowFacade(cameraBinding.m_CameraID, cameraBinding.m_FascadeTargetPos, bAdvanceTimers: true))
			{
				if (m_bShowFacade)
				{
					cameraBinding.m_CameraView = CameraView.Facade;
				}
			}
			else if (character.m_bIsStandingOnDesk)
			{
				FloorManager.Floor currentFloor = character.CurrentFloor;
				FloorManager.Floor floor = FloorManager.GetInstance().UpAFloor(currentFloor);
				if (floor != currentFloor && floor.IsVent())
				{
					cameraBinding.m_CameraView = CameraView.VentLayerAbove;
				}
			}
		}
		if (cameraBinding.m_CameraView != cameraView)
		{
			m_NumIntervalsBeforeBubbleCheck = 0;
			cameraBinding.m_CullerUpdateMode = CullerUpdateMode.ForcedNextFrameOnly;
			if (CameraManager.CameraViewChangedEvent != null)
			{
				CameraManager.CameraViewChangedEvent(cameraBinding);
			}
		}
		m_ZOffsetsFromPlayer[index] = CalculatePixelPerfectOffset(index);
		camera.enabled = true;
		float x = cameraBinding.m_MinCameraPos.x;
		float x2 = cameraBinding.m_MaxCameraPos.x;
		float x3 = camera.transform.position.x;
		float y = camera.transform.position.y;
		if (!m_bAllowXCamera)
		{
			x3 = Mathf.Clamp((cameraBinding.m_MinCameraPos.x + cameraBinding.m_MaxCameraPos.x) / 2f, cameraBinding.m_MinCameraPos.x, cameraBinding.m_MaxCameraPos.x);
		}
		else
		{
			x3 = cameraBinding.m_NewTargetPosition.x;
			x3 = Mathf.Clamp(x3, x, x2);
		}
		if (!m_bAllowYCamera)
		{
			y = Mathf.Clamp((cameraBinding.m_MinCameraPos.y + cameraBinding.m_MaxCameraPos.y) / 2f, cameraBinding.m_MinCameraPos.y, cameraBinding.m_MaxCameraPos.y);
		}
		else
		{
			y = cameraBinding.m_NewTargetPosition.y;
			float num = cameraBinding.m_MaxCameraPos.y;
			if (character != null && LevelScript.GetCurrentLevelInfo() != null && LevelScript.GetCurrentLevelInfo().m_PrisonEnum == LevelScript.PRISON_ENUM.CustomPrison)
			{
				switch (character.CurrentFloor.m_FloorIndex)
				{
				case 0:
				case 1:
				case 2:
					num -= 2f;
					break;
				case 3:
				case 4:
					num -= 1f;
					break;
				}
			}
			y = Mathf.Clamp(y, cameraBinding.m_MinCameraPos.y, num);
		}
		if (cameraBinding.m_CombatHitShake.IsActive())
		{
			cameraBinding.m_CombatHitShake.Update(UpdateManager.deltaTime);
			Vector3 vector = cameraBinding.m_CombatHitShake.GetVector();
			x3 += vector.x;
			y += vector.y;
		}
		float x4 = RoundToPixelBoundary(x3, cameraBinding);
		float y2 = RoundToPixelBoundary(y, cameraBinding);
		camera.transform.position = new Vector3(x4, y2, cameraBinding.m_NewTargetPosition.z + m_ZOffsetsFromPlayer[index]);
		float num2 = 0f;
		if (cameraBinding.m_CameraView == CameraView.Facade)
		{
			num2 += (float)m_ZOffsetForFacade;
		}
		else if (cameraBinding.m_CameraView == CameraView.VentLayerAbove)
		{
			num2 -= 3f;
		}
		camera.nearClipPlane = m_ZOffsetsFromPlayer[index] * -1f + (float)FloorManager.GetInstance().m_FloorOffset + num2;
		if (LevelScript.GetCurrentLevelInfo() != null && LevelScript.GetCurrentLevelInfo().m_PrisonEnum == LevelScript.PRISON_ENUM.CustomPrison)
		{
			camera.nearClipPlane += -0.025f;
		}
		camera.farClipPlane = camera.nearClipPlane + 120f;
		if (cameraBinding.m_Camera != null && cameraBinding.m_ListenerIndex != uint.MaxValue)
		{
			AkSoundEngine.SetListenerPosition(0f, 0f, 1f, 0f, 1f, 0f, cameraBinding.m_NewTargetPosition.x, cameraBinding.m_NewTargetPosition.y, cameraBinding.m_NewTargetPosition.z, cameraBinding.m_ListenerIndex);
		}
	}

	public void SetClipPlanesForRoof(Camera cam)
	{
		cam.nearClipPlane += m_ZOffsetForFacade;
		cam.farClipPlane = cam.nearClipPlane + 120f;
	}

	private void DisableCamerasAboveIndex(int index)
	{
		if (m_CameraBindings == null)
		{
			return;
		}
		for (int i = index + 1; i < m_CameraBindings.Length; i++)
		{
			if (m_CameraBindings[i] != null && m_CameraBindings[i].m_Camera != null)
			{
				m_CameraBindings[i].m_Camera.enabled = false;
			}
		}
	}

	private void SetStencilRendererActive(int index, bool active)
	{
		int num = m_CameraBindings.Length;
		index %= num;
		int num2 = 0;
		while (num2 < num)
		{
			CameraBinding cameraBinding = m_CameraBindings[num2];
			if (cameraBinding != null && cameraBinding.m_CharacterStencilRenderer != null && index == 0)
			{
				cameraBinding.m_CharacterStencilRenderer.m_isActive = active;
				break;
			}
			num2++;
			index--;
		}
	}

	private void SetStencilRendererIdActive(int index)
	{
		int num = m_CameraBindings.Length;
		int num2 = 0;
		while (num2 < num)
		{
			CameraBinding cameraBinding = m_CameraBindings[num2];
			if (cameraBinding != null && cameraBinding.m_CharacterStencilRenderer != null)
			{
				if (index == 0)
				{
					cameraBinding.m_CharacterStencilRenderer.m_isActive = true;
				}
				else
				{
					cameraBinding.m_CharacterStencilRenderer.m_isActive = false;
				}
			}
			num2++;
			index--;
		}
	}

	private void SetAllStencilRenderersActive()
	{
		int num = m_CameraBindings.Length;
		for (int i = 0; i < num; i++)
		{
			CameraBinding cameraBinding = m_CameraBindings[i];
			if (cameraBinding != null && cameraBinding.m_CharacterStencilRenderer != null)
			{
				cameraBinding.m_CharacterStencilRenderer.m_isActive = true;
			}
		}
	}

	public void ControlledUpdate()
	{
	}

	public void ControlledFixedUpdate()
	{
	}

	public void ControlledLateUpdate()
	{
		if (m_bIsFullscreen != Screen.fullScreen)
		{
			RenderTargetManager.CheckForLostRTs();
			OnScreenResolutionChanged();
			m_bIsFullscreen = Screen.fullScreen;
		}
		if (m_bResolutionChanged)
		{
			m_bResolutionChanged = false;
			CharacterStencilRenderer.StartFrame();
			SetAllStencilRenderersActive();
		}
		else
		{
			SetAllStencilRenderersActive();
		}
		if (m_bRun)
		{
			switch (m_OpMode)
			{
			case CameraOpModes.Game:
				GameCamerasUpdate();
				break;
			case CameraOpModes.Cutscene:
				CutsceneCameraUpdate();
				break;
			}
		}
	}

	public void ControlledPreFixedUpdate()
	{
	}

	public void ControlledPreUpdate()
	{
	}

	public bool RequiresControlledUpdate()
	{
		return false;
	}

	public bool RequiresControlledFixedUpdate()
	{
		return false;
	}

	public bool RequiresControlledLateUpdate()
	{
		return true;
	}

	public bool RequiresControlledPreUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreFixedUpdate()
	{
		return false;
	}

	private void CutsceneCameraUpdate()
	{
		int num = 0;
		GetUsedCameraCount(setCameraTargetPosToCharPos: true);
		CameraBinding cameraBinding = m_CameraBindings[num];
		if (cameraBinding == null)
		{
			return;
		}
		Camera camera = cameraBinding.m_Camera;
		if (!(camera == null))
		{
			m_ZOffsetsFromPlayer[num] = CalculatePixelPerfectOffset(num);
			camera.enabled = true;
			Vector3 position = m_CachedTrackableObject.GetPosition();
			float x = position.x;
			float y = position.y;
			float x2 = RoundToPixelBoundary(x, cameraBinding);
			float y2 = RoundToPixelBoundary(y, cameraBinding);
			camera.transform.position = new Vector3(x2, y2, position.z + m_ZOffsetsFromPlayer[num]);
			float num2 = 0f;
			if (m_CachedTrackableObject.m_CameraView == CameraView.Facade)
			{
				num2 = m_ZOffsetForFacade;
			}
			num2 += m_CachedTrackableObject.m_NearClippingFloorOffset;
			float defaultCameraCutsceneOffset = GetDefaultCameraCutsceneOffset();
			camera.nearClipPlane = m_ZOffsetsFromPlayer[num] * -1f + defaultCameraCutsceneOffset + num2;
			camera.farClipPlane = camera.nearClipPlane + 120f;
			if (cameraBinding.m_Camera != null && cameraBinding.m_ListenerIndex != uint.MaxValue)
			{
				AkSoundEngine.SetListenerPosition(0f, 0f, 1f, 0f, 1f, 0f, x, y, cameraBinding.m_NewTargetPosition.z, cameraBinding.m_ListenerIndex);
			}
		}
	}

	private void GameCamerasUpdate()
	{
		int num = GetUsedCameraCount(setCameraTargetPosToCharPos: true);
		if (T17NetManager.NetOnlineMode)
		{
			num = Gamer.GetNumLocalGamers();
		}
		if (num != m_ActiveCameraCount || m_ForceRefresh)
		{
			m_ForceRefresh = false;
			PrisonData.LevelInfo currentLevelInfo = LevelScript.GetCurrentLevelInfo();
			if (currentLevelInfo.m_PrisonEnum == LevelScript.PRISON_ENUM.Transport_Plane)
			{
				if (num == 1)
				{
					m_bAllowXCamera = false;
				}
				else
				{
					m_bAllowXCamera = true;
				}
			}
			if (num < m_ActiveCameraCount)
			{
				RearrangeBindings();
			}
			m_ActiveCameraCount = num;
			if (OnActiveCamerasUpdated != null)
			{
				OnActiveCamerasUpdated();
			}
			switch (num)
			{
			case 1:
				SetupCameraBoundsForOneActive();
				break;
			case 2:
				if (m_CameraBindings[0] != null && m_CameraBindings[1] != null)
				{
					CameraBinding cameraBinding5 = m_CameraBindings[0];
					CameraBinding cameraBinding6 = m_CameraBindings[1];
					if (!(cameraBinding5.m_Camera == null) && !(cameraBinding6.m_Camera == null))
					{
						cameraBinding5.m_Camera.rect = new Rect(0f, 0f, 0.5f, 1f);
						cameraBinding5.m_NormalizedViewportHeight = 1f;
						cameraBinding6.m_Camera.rect = new Rect(0.5f, 0f, 0.5f, 1f);
						cameraBinding6.m_NormalizedViewportHeight = 1f;
						m_ZOffsetsFromPlayer[0] = CalculatePixelPerfectOffset(0);
						m_ZOffsetsFromPlayer[1] = CalculatePixelPerfectOffset(1);
						DisableCamerasAboveIndex(1);
						SetCameraBounds(m_MapBoundsBR, m_MapBoundsTL);
					}
				}
				break;
			case 3:
				if (m_CameraBindings[0] != null && m_CameraBindings[1] != null && m_CameraBindings[2] != null)
				{
					CameraBinding cameraBinding7 = m_CameraBindings[0];
					CameraBinding cameraBinding8 = m_CameraBindings[1];
					CameraBinding cameraBinding9 = m_CameraBindings[2];
					if (!(cameraBinding7.m_Camera == null) && !(cameraBinding8.m_Camera == null) && !(cameraBinding9.m_Camera == null))
					{
						cameraBinding7.m_Camera.rect = new Rect(0f, 0f, 0.5f, 1f);
						cameraBinding7.m_NormalizedViewportHeight = 1f;
						cameraBinding8.m_Camera.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
						cameraBinding8.m_NormalizedViewportHeight = 0.25f;
						cameraBinding9.m_Camera.rect = new Rect(0.5f, 0f, 0.5f, 0.5f);
						cameraBinding9.m_NormalizedViewportHeight = 0.25f;
						m_ZOffsetsFromPlayer[0] = m_ZOffsetFromPlayerHalfScreen;
						m_ZOffsetsFromPlayer[1] = m_ZOffsetFromPlayerQuarterScreen;
						m_ZOffsetsFromPlayer[2] = m_ZOffsetFromPlayerQuarterScreen;
						m_ZOffsetsFromPlayer[0] = CalculatePixelPerfectOffset(0);
						m_ZOffsetsFromPlayer[1] = CalculatePixelPerfectOffset(1);
						m_ZOffsetsFromPlayer[2] = CalculatePixelPerfectOffset(2);
						DisableCamerasAboveIndex(2);
						SetCameraBounds(m_MapBoundsBR, m_MapBoundsTL);
					}
				}
				break;
			case 4:
				if (m_CameraBindings[0] != null && m_CameraBindings[1] != null && m_CameraBindings[2] != null && m_CameraBindings[3] != null)
				{
					CameraBinding cameraBinding = m_CameraBindings[0];
					CameraBinding cameraBinding2 = m_CameraBindings[1];
					CameraBinding cameraBinding3 = m_CameraBindings[2];
					CameraBinding cameraBinding4 = m_CameraBindings[3];
					if (!(cameraBinding.m_Camera == null) && !(cameraBinding2.m_Camera == null) && !(cameraBinding3.m_Camera == null) && !(cameraBinding4.m_Camera == null))
					{
						cameraBinding.m_Camera.rect = new Rect(0f, 0.5f, 0.5f, 0.5f);
						cameraBinding.m_NormalizedViewportHeight = 0.25f;
						cameraBinding2.m_Camera.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
						cameraBinding2.m_NormalizedViewportHeight = 0.25f;
						cameraBinding3.m_Camera.rect = new Rect(0f, 0f, 0.5f, 0.5f);
						cameraBinding3.m_NormalizedViewportHeight = 0.25f;
						cameraBinding4.m_Camera.rect = new Rect(0.5f, 0f, 0.5f, 0.5f);
						cameraBinding4.m_NormalizedViewportHeight = 0.25f;
						m_ZOffsetsFromPlayer[0] = CalculatePixelPerfectOffset(0);
						m_ZOffsetsFromPlayer[1] = CalculatePixelPerfectOffset(1);
						m_ZOffsetsFromPlayer[2] = CalculatePixelPerfectOffset(2);
						m_ZOffsetsFromPlayer[3] = CalculatePixelPerfectOffset(3);
						SetCameraBounds(m_MapBoundsBR, m_MapBoundsTL);
					}
				}
				break;
			}
		}
		for (int i = 0; i < m_ActiveCameraCount; i++)
		{
			CameraUpdate(i);
		}
	}

	private void SetupCameraBoundsForOneActive()
	{
		m_ZOffsetsFromPlayer[0] = CalculatePixelPerfectOffset(0);
		m_CameraBindings[0].m_Camera.rect = new Rect(0f, 0f, 1f, 1f);
		m_CameraBindings[0].m_NormalizedViewportHeight = 1f;
		DisableCamerasAboveIndex(0);
		SetCameraBounds(m_MapBoundsBR, m_MapBoundsTL);
	}

	public void SetCameraBounds(Vector2 BottomRight, Vector2 TopLeft)
	{
		for (int i = 0; i < m_CameraBindings.Length; i++)
		{
			if (m_CameraBindings[i] != null && m_CameraBindings[i].m_Camera != null)
			{
				Camera camera = m_CameraBindings[i].m_Camera;
				if (!camera.orthographic)
				{
					float num = 2f * m_ZOffsetsFromPlayer[i] * Mathf.Tan(camera.fieldOfView * 0.5f * ((float)Math.PI / 180f)) * -1f;
					float num2 = num * camera.aspect;
					m_CameraBindings[i].m_MaxCameraPos.x = BottomRight.x - num2 / 2f;
					m_CameraBindings[i].m_MinCameraPos.y = BottomRight.y + num / 2f;
					m_CameraBindings[i].m_MinCameraPos.x = TopLeft.x + num2 / 2f;
					m_CameraBindings[i].m_MaxCameraPos.y = TopLeft.y - num / 2f;
				}
				else
				{
					m_CameraBindings[i].m_MaxCameraPos.x = BottomRight.x - camera.aspect * camera.orthographicSize;
					m_CameraBindings[i].m_MaxCameraPos.y = 0f - camera.orthographicSize;
					m_CameraBindings[i].m_MinCameraPos.x = camera.aspect * camera.orthographicSize;
					m_CameraBindings[i].m_MinCameraPos.y = camera.orthographicSize - BottomRight.y;
				}
			}
		}
	}

	private void SetUpCamBinding(int index, Vector3 position)
	{
		AkTransform akTransform = new AkTransform();
		akTransform.SetPosition(position.x, position.y, position.z);
		uint num = (uint)(index + 1);
		m_CameraBindings[index].m_ListenerIndex = num;
		AkSoundEngine.SetListenerPosition(0f, 0f, 1f, 0f, 1f, 0f, position.x, position.y, position.z, num);
		AkTransform out_rPosition = new AkTransform();
		AkSoundEngine.GetListenerPosition(num, out_rPosition);
	}

	public PlayerBindingID AssignABindingToGetUnUsedBindingID()
	{
		PlayerBindingID unUsedBindingID = GetUnUsedBindingID();
		for (int i = 0; i < m_CameraBindings.Length; i++)
		{
			if (m_CameraBindings[i].m_Character == null && m_CameraBindings[i].m_TargetPosition == Vector3.zero)
			{
				m_CameraBindings[i].m_PlayerBinding = unUsedBindingID;
				break;
			}
		}
		return unUsedBindingID;
	}

	public PlayerBindingID GetUsedBindingID(int i)
	{
		int num = 0;
		for (int j = 0; j < m_CameraBindings.Length; j++)
		{
			if (m_CameraBindings[j].m_Character != null || m_CameraBindings[j].m_TargetPosition != Vector3.zero)
			{
				if (num == i)
				{
					return m_CameraBindings[j].m_PlayerBinding;
				}
				num++;
			}
		}
		return PlayerBindingID.CM_PBID_UNSET;
	}

	private PlayerBindingID GetUnUsedBindingID()
	{
		for (int i = 1; i <= 4; i++)
		{
			bool flag = false;
			for (int j = 0; j < m_CameraBindings.Length; j++)
			{
				if (m_CameraBindings[j].m_PlayerBinding == (PlayerBindingID)i)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return (PlayerBindingID)i;
			}
		}
		return PlayerBindingID.CM_PBID_UNSET;
	}

	public int GetCameraIDFromBinding(PlayerBindingID bindingID)
	{
		for (int i = 0; i < m_CameraBindings.Length; i++)
		{
			if ((m_CameraBindings[i].m_Character != null || m_CameraBindings[i].m_TargetPosition != Vector3.zero) && m_CameraBindings[i].m_PlayerBinding == bindingID)
			{
				return m_CameraBindings[i].m_CameraID;
			}
		}
		return -1;
	}

	public PlayerBindingID SetTarget(Character character)
	{
		PlayerBindingID playerBindingID = PlayerBindingID.CM_PBID_UNSET;
		for (int i = 0; i < m_CameraBindings.Length; i++)
		{
			if (m_CameraBindings[i].m_Character == null)
			{
				m_CameraBindings[i].m_Character = character;
				playerBindingID = GetUnUsedBindingID();
				m_CameraBindings[i].m_PlayerBinding = playerBindingID;
				if (OnTargetsUpdated != null)
				{
					OnTargetsUpdated();
				}
				if (m_CameraBindings[i].m_Camera != null)
				{
					SetUpCamBinding(i, character.transform.position);
				}
				break;
			}
		}
		PrisonData.LevelInfo currentLevelInfo = LevelScript.GetCurrentLevelInfo();
		m_bAllowXCamera = currentLevelInfo.m_PrisonEnum != LevelScript.PRISON_ENUM.Transport_Plane || (currentLevelInfo.m_PrisonEnum == LevelScript.PRISON_ENUM.Transport_Plane && m_ActiveCameraCount > 1);
		m_bAllowYCamera = currentLevelInfo.m_PrisonEnum != LevelScript.PRISON_ENUM.Transport_Train;
		return playerBindingID;
	}

	public PlayerBindingID SetTarget(Vector3 pos)
	{
		PlayerBindingID playerBindingID = PlayerBindingID.CM_PBID_UNSET;
		for (int i = 0; i < m_CameraBindings.Length; i++)
		{
			if (m_CameraBindings[i].m_Character == null && m_CameraBindings[i].m_TargetPosition == Vector3.zero)
			{
				m_CameraBindings[i].m_TargetPosition = pos;
				playerBindingID = GetUnUsedBindingID();
				m_CameraBindings[i].m_PlayerBinding = playerBindingID;
				if (OnTargetsUpdated != null)
				{
					OnTargetsUpdated();
				}
				if (m_CameraBindings[i].m_Camera != null)
				{
					SetUpCamBinding(i, pos);
				}
				break;
			}
		}
		PrisonData.LevelInfo currentLevelInfo = LevelScript.GetCurrentLevelInfo();
		m_bAllowXCamera = currentLevelInfo.m_PrisonEnum != LevelScript.PRISON_ENUM.Transport_Plane || (currentLevelInfo.m_PrisonEnum == LevelScript.PRISON_ENUM.Transport_Plane && m_ActiveCameraCount > 1);
		m_bAllowYCamera = currentLevelInfo.m_PrisonEnum != LevelScript.PRISON_ENUM.Transport_Train;
		return playerBindingID;
	}

	public bool SetTarget(Vector3 pos, PlayerBindingID playerBindingID)
	{
		for (int i = 0; i < m_CameraBindings.Length; i++)
		{
			if (m_CameraBindings[i].m_PlayerBinding == playerBindingID)
			{
				if (m_CameraBindings[i].m_Character == null)
				{
					m_CameraBindings[i].m_TargetPosition = pos;
					if (OnTargetsUpdated != null)
					{
						OnTargetsUpdated();
					}
					if (m_CameraBindings[i].m_Camera != null)
					{
						PrisonData.LevelInfo currentLevelInfo = LevelScript.GetCurrentLevelInfo();
						SetUpCamBinding(i, pos);
						m_bAllowXCamera = currentLevelInfo.m_PrisonEnum != LevelScript.PRISON_ENUM.Transport_Plane || (currentLevelInfo.m_PrisonEnum == LevelScript.PRISON_ENUM.Transport_Plane && m_ActiveCameraCount > 1);
						m_bAllowYCamera = currentLevelInfo.m_PrisonEnum != LevelScript.PRISON_ENUM.Transport_Train;
						return true;
					}
					break;
				}
				break;
			}
		}
		return false;
	}

	public bool SetTarget(Character character, PlayerBindingID playerBindingID)
	{
		for (int i = 0; i < m_CameraBindings.Length; i++)
		{
			if (m_CameraBindings[i].m_PlayerBinding != 0 && m_CameraBindings[i].m_PlayerBinding == playerBindingID)
			{
				m_CameraBindings[i].m_Character = character;
				if (OnTargetsUpdated != null)
				{
					OnTargetsUpdated();
				}
				if (m_CameraBindings[i].m_Camera != null)
				{
					SetUpCamBinding(i, character.transform.position);
					m_CameraBindings[i].m_TargetPosition = Vector3.zero;
					PrisonData.LevelInfo currentLevelInfo = LevelScript.GetCurrentLevelInfo();
					m_bAllowXCamera = currentLevelInfo.m_PrisonEnum != LevelScript.PRISON_ENUM.Transport_Plane || (currentLevelInfo.m_PrisonEnum == LevelScript.PRISON_ENUM.Transport_Plane && m_ActiveCameraCount > 1);
					m_bAllowYCamera = currentLevelInfo.m_PrisonEnum != LevelScript.PRISON_ENUM.Transport_Train;
					return true;
				}
			}
		}
		return false;
	}

	public void RemoveTarget(Character character)
	{
		for (int i = 0; i < m_CameraBindings.Length; i++)
		{
			if (m_CameraBindings[i].m_Character == character)
			{
				m_CameraBindings[i].m_Character = null;
				m_CameraBindings[i].m_TargetPosition = Vector3.zero;
				m_CameraBindings[i].m_PlayerBinding = PlayerBindingID.CM_PBID_UNSET;
				if (OnTargetsUpdated != null)
				{
					OnTargetsUpdated();
				}
			}
		}
	}

	public void RemoveTarget(PlayerBindingID binding, Vector3 playerHidePosition)
	{
		for (int i = 0; i < m_CameraBindings.Length; i++)
		{
			if (m_CameraBindings[i].m_PlayerBinding == binding)
			{
				AkSoundEngine.SetListenerPosition(0f, 0f, 1f, 0f, 1f, 0f, playerHidePosition.x, playerHidePosition.y, playerHidePosition.z, m_CameraBindings[i].m_ListenerIndex);
				m_CameraBindings[i].m_Character = null;
				m_CameraBindings[i].m_TargetPosition = Vector3.zero;
				m_CameraBindings[i].m_PlayerBinding = PlayerBindingID.CM_PBID_UNSET;
				if (OnTargetsUpdated != null)
				{
					OnTargetsUpdated();
				}
			}
		}
	}

	public Camera GetTargetCharactersCamera(Character target)
	{
		if (target != null)
		{
			for (int i = 0; i < m_CameraBindings.Length; i++)
			{
				if (m_CameraBindings[i].m_Character == target)
				{
					return m_CameraBindings[i].m_Camera;
				}
			}
		}
		return null;
	}

	public Character GetCameraTargetCharacter(Camera camera)
	{
		if (camera == null)
		{
			return null;
		}
		for (int i = 0; i < m_CameraBindings.Length; i++)
		{
			if (m_CameraBindings[i].m_Camera == camera)
			{
				return m_CameraBindings[i].m_Character;
			}
		}
		return null;
	}

	public Vector3 GetCameraTargetPosition(Camera camera)
	{
		if (camera == null)
		{
			return Vector3.zero;
		}
		for (int i = 0; i < m_CameraBindings.Length; i++)
		{
			if (m_CameraBindings[i].m_Camera == camera)
			{
				return m_CameraBindings[i].m_TargetPosition;
			}
		}
		return Vector3.zero;
	}

	public Vector3 GetCameraCullTarget(Camera camera, int cameraIndex)
	{
		if (camera == m_CameraBindings[0].m_Camera && m_OpMode == CameraOpModes.Cutscene && m_CachedTrackableObject != null)
		{
			return m_CachedTrackableObject.GetPosition();
		}
		return new Vector3(camera.transform.position.x, camera.transform.position.y, m_CameraBindings[cameraIndex].m_NewTargetPosition.z);
	}

	public Vector3 GetCameraCullTarget(Camera camera)
	{
		if (camera == m_CameraBindings[0].m_Camera && m_OpMode == CameraOpModes.Cutscene && m_CachedTrackableObject != null)
		{
			return m_CachedTrackableObject.GetPosition();
		}
		for (int i = 0; i < m_CameraBindings.Length; i++)
		{
			if (m_CameraBindings[i].m_Camera == camera)
			{
				return new Vector3(camera.transform.position.x, camera.transform.position.y, m_CameraBindings[i].m_NewTargetPosition.z);
			}
		}
		return Vector3.zero;
	}

	public Vector3 GetCameraFacadeTarget(Camera camera, int cameraIndex)
	{
		if (camera == m_CameraBindings[0].m_Camera && m_OpMode == CameraOpModes.Cutscene)
		{
			return m_CachedTrackableObject.GetPosition();
		}
		return m_CameraBindings[cameraIndex].m_FascadeTargetPos;
	}

	public Vector3 GetCameraFacadeTarget(Camera camera)
	{
		if (camera == m_CameraBindings[0].m_Camera && m_OpMode == CameraOpModes.Cutscene)
		{
			return m_CachedTrackableObject.GetPosition();
		}
		for (int i = 0; i < m_CameraBindings.Length; i++)
		{
			if (m_CameraBindings[i].m_Camera == camera)
			{
				return m_CameraBindings[i].m_FascadeTargetPos;
			}
		}
		return Vector3.zero;
	}

	public Vector3 GetCameraTrackableTarget(Camera camera)
	{
		if (camera == m_CameraBindings[0].m_Camera && m_OpMode == CameraOpModes.Cutscene)
		{
			return m_CachedTrackableObject.GetPosition();
		}
		return Vector3.zero;
	}

	public int GetCameraIndexInManager(Camera camera)
	{
		for (int i = 0; i < m_CameraBindings.Length; i++)
		{
			if (m_CameraBindings[i].m_Camera == camera)
			{
				return i;
			}
		}
		return -1;
	}

	public float GetFloorZOfCamera(int index)
	{
		FloorManager.Floor floor = FloorManager.GetInstance().FindFloorAtZ(m_CameraBindings[index].m_NewTargetPosition.z);
		return floor.m_zPos;
	}

	public CameraView GetCameraView(Camera camera, int cameraIndex)
	{
		if (camera == m_CameraBindings[0].m_Camera && m_OpMode == CameraOpModes.Cutscene)
		{
			return m_CachedTrackableObject.m_CameraView;
		}
		return m_CameraBindings[cameraIndex].m_CameraView;
	}

	public CameraView GetCameraView(Camera camera)
	{
		if (camera == m_CameraBindings[0].m_Camera && m_OpMode == CameraOpModes.Cutscene)
		{
			return m_CachedTrackableObject.m_CameraView;
		}
		for (int i = 0; i < m_CameraBindings.Length; i++)
		{
			if (m_CameraBindings[i].m_Camera == camera)
			{
				return m_CameraBindings[i].m_CameraView;
			}
		}
		return CameraView.Normal;
	}

	public CullerUpdateMode GetCullerUpdateMode(Camera camera, int cameraIndex)
	{
		CullerUpdateMode cullerUpdateMode = CullerUpdateMode.Optimal;
		cullerUpdateMode = m_CameraBindings[cameraIndex].m_CullerUpdateMode;
		if (cullerUpdateMode == CullerUpdateMode.ForcedNextFrameOnly)
		{
			m_CameraBindings[cameraIndex].m_CullerUpdateMode = CullerUpdateMode.Optimal;
		}
		return cullerUpdateMode;
	}

	public CullerUpdateMode GetCullerUpdateMode(Camera camera)
	{
		CullerUpdateMode cullerUpdateMode = CullerUpdateMode.Optimal;
		CameraBinding cameraBinding = GetCameraBinding(camera);
		if (cameraBinding != null)
		{
			cullerUpdateMode = cameraBinding.m_CullerUpdateMode;
			if (cullerUpdateMode == CullerUpdateMode.ForcedNextFrameOnly)
			{
				cameraBinding.m_CullerUpdateMode = CullerUpdateMode.Optimal;
			}
		}
		return cullerUpdateMode;
	}

	public void SetCullerUpdateMode(Camera camera, CullerUpdateMode mode)
	{
		if (camera != null)
		{
			CameraBinding cameraBinding = GetCameraBinding(camera);
			if (cameraBinding != null)
			{
				cameraBinding.m_CullerUpdateMode = mode;
			}
			if (mode == CullerUpdateMode.ForcedNextFrameOnly)
			{
				m_NumIntervalsBeforeBubbleCheck = 0;
			}
		}
	}

	public static void ForceSpeechBubbleShiftCheck()
	{
		if (m_Instance != null)
		{
			m_Instance.m_NumIntervalsBeforeBubbleCheck = 0;
		}
	}

	public void ForceAnUpdateForActiveCameras()
	{
		for (int i = 0; i < m_CameraBindings.Length; i++)
		{
			if (m_CameraBindings[i].m_Camera != null && m_CameraBindings[i].m_Camera.enabled)
			{
				SetCullerUpdateMode(m_CameraBindings[i].m_Camera, CullerUpdateMode.ForcedNextFrameOnly);
			}
		}
	}

	public CharacterStencilRenderer GetCharacterStencilRenderer(Camera camera, int cameraIndex)
	{
		return m_CameraBindings[cameraIndex].m_CharacterStencilRenderer;
	}

	public CharacterStencilRenderer GetCharacterStencilRenderer(Camera camera)
	{
		return GetCameraBinding(camera)?.m_CharacterStencilRenderer;
	}

	public float RoundToPixelBoundary(float unityUnit, CameraBinding binding)
	{
		return CalculatePcRoundToPixelBoundary(unityUnit, binding);
	}

	private float CalculatePcRoundToPixelBoundary(float unityUnit, CameraBinding binding)
	{
		return Mathf.Floor(unityUnit * m_fFract) / m_fFract;
	}

	private float CalculateConsoleRoundToPixelBoundary(float unityUnit, CameraBinding binding)
	{
		return Mathf.Floor(unityUnit * 64f) / 64f;
	}

	private void SetUpCameraViewportRects()
	{
		m_FixedCamereaViewportRects_1 = SetCameraViewportRects(1);
		m_FixedCamereaViewportRects_2 = SetCameraViewportRects(2);
		m_FixedCamereaViewportRects_3 = SetCameraViewportRects(3);
		m_FixedCamereaViewportRects_4 = SetCameraViewportRects(4);
	}

	private void DestoryCameraViewportRects()
	{
		m_FixedCamereaViewportRects_1 = null;
		m_FixedCamereaViewportRects_2 = null;
		m_FixedCamereaViewportRects_3 = null;
		m_FixedCamereaViewportRects_4 = null;
	}

	public Rect[] GetCameraViewportRects(int activeCameraCountOverride = -1)
	{
		if (activeCameraCountOverride == -1)
		{
			activeCameraCountOverride = m_ActiveCameraCount;
		}
		return activeCameraCountOverride switch
		{
			1 => m_FixedCamereaViewportRects_1, 
			2 => m_FixedCamereaViewportRects_2, 
			3 => m_FixedCamereaViewportRects_3, 
			4 => m_FixedCamereaViewportRects_4, 
			_ => m_FixedCamereaViewportRects_1, 
		};
	}

	private Rect[] SetCameraViewportRects(int activeCameraCountOverride = -1)
	{
		if (activeCameraCountOverride == -1)
		{
			activeCameraCountOverride = m_ActiveCameraCount;
		}
		Rect[] array = new Rect[activeCameraCountOverride];
		switch (activeCameraCountOverride)
		{
		case 1:
		{
			ref Rect reference10 = ref array[0];
			reference10 = new Rect(0f, 0f, 1f, 1f);
			break;
		}
		case 2:
		{
			ref Rect reference8 = ref array[0];
			reference8 = new Rect(0f, 0f, 0.5f, 1f);
			ref Rect reference9 = ref array[1];
			reference9 = new Rect(0.5f, 0f, 0.5f, 1f);
			break;
		}
		case 3:
		{
			ref Rect reference5 = ref array[0];
			reference5 = new Rect(0f, 0f, 0.5f, 1f);
			ref Rect reference6 = ref array[1];
			reference6 = new Rect(0.5f, 0f, 0.5f, 0.5f);
			ref Rect reference7 = ref array[2];
			reference7 = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
			break;
		}
		case 4:
		{
			ref Rect reference = ref array[0];
			reference = new Rect(0f, 0f, 0.5f, 0.5f);
			ref Rect reference2 = ref array[1];
			reference2 = new Rect(0.5f, 0f, 0.5f, 0.5f);
			ref Rect reference3 = ref array[3];
			reference3 = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
			ref Rect reference4 = ref array[2];
			reference4 = new Rect(0f, 0.5f, 0.5f, 0.5f);
			break;
		}
		default:
			array = new Rect[1]
			{
				new Rect(0f, 0f, 1f, 1f)
			};
			break;
		}
		return array;
	}

	public Rect[] GetCombineCameraViewportRects()
	{
		Rect[] array = new Rect[m_ActiveCameraCount];
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore)
		{
			switch (m_ActiveCameraCount)
			{
			case 1:
			{
				ref Rect reference10 = ref array[0];
				reference10 = new Rect(0f, 0f, 1f, 1f);
				break;
			}
			case 2:
			{
				ref Rect reference8 = ref array[0];
				reference8 = new Rect(0f, 0f, 0.5f, 1f);
				ref Rect reference9 = ref array[1];
				reference9 = new Rect(0.5f, 0f, 0.5f, 1f);
				break;
			}
			case 3:
			{
				ref Rect reference5 = ref array[0];
				reference5 = new Rect(0f, 0f, 0.5f, 1f);
				ref Rect reference6 = ref array[1];
				reference6 = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
				ref Rect reference7 = ref array[2];
				reference7 = new Rect(0.5f, 0f, 0.5f, 0.5f);
				break;
			}
			case 4:
			{
				ref Rect reference = ref array[0];
				reference = new Rect(0f, 0.5f, 0.5f, 0.5f);
				ref Rect reference2 = ref array[1];
				reference2 = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
				ref Rect reference3 = ref array[2];
				reference3 = new Rect(0f, 0f, 0.5f, 0.5f);
				ref Rect reference4 = ref array[3];
				reference4 = new Rect(0.5f, 0f, 0.5f, 0.5f);
				break;
			}
			default:
				array = new Rect[1]
				{
					new Rect(0f, 0f, 1f, 1f)
				};
				break;
			}
		}
		else
		{
			switch (m_ActiveCameraCount)
			{
			case 1:
			{
				ref Rect reference20 = ref array[0];
				reference20 = new Rect(0f, 0f, 1f, 1f);
				break;
			}
			case 2:
			{
				ref Rect reference18 = ref array[0];
				reference18 = new Rect(0f, 0f, 0.5f, 1f);
				ref Rect reference19 = ref array[1];
				reference19 = new Rect(0.5f, 0f, 0.5f, 1f);
				break;
			}
			case 3:
			{
				ref Rect reference15 = ref array[0];
				reference15 = new Rect(0f, 0f, 0.5f, 1f);
				ref Rect reference16 = ref array[1];
				reference16 = new Rect(0.5f, 0f, 0.5f, 0.5f);
				ref Rect reference17 = ref array[2];
				reference17 = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
				break;
			}
			case 4:
			{
				ref Rect reference11 = ref array[0];
				reference11 = new Rect(0f, 0f, 0.5f, 0.5f);
				ref Rect reference12 = ref array[1];
				reference12 = new Rect(0.5f, 0f, 0.5f, 0.5f);
				ref Rect reference13 = ref array[2];
				reference13 = new Rect(0f, 0.5f, 0.5f, 0.5f);
				ref Rect reference14 = ref array[3];
				reference14 = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
				break;
			}
			default:
				array = new Rect[1]
				{
					new Rect(0f, 0f, 1f, 1f)
				};
				break;
			}
		}
		return array;
	}

	public void SwitchToCutsceneMode()
	{
		m_OpMode = CameraOpModes.Cutscene;
		m_ActiveCameraCount = 1;
		SetupCameraBoundsForOneActive();
		m_CachedTrackableObject = CutsceneManagerBase.GetCameraTrackableObject();
		m_CachedTrackableObject.Reset();
		if (OnActiveCamerasUpdated != null)
		{
			OnActiveCamerasUpdated();
		}
		if (!m_CachedTrackableObject.gameObject.activeSelf)
		{
			m_CachedTrackableObject.gameObject.SetActive(value: true);
		}
		SetBlurEffectAllowed(bAllowed: false);
		if (OnCameraOpModeChanged != null)
		{
			OnCameraOpModeChanged(m_OpMode);
		}
	}

	public void SwitchToGameMode()
	{
		m_OpMode = CameraOpModes.Game;
		m_ForceRefresh = true;
		SetBlurEffectAllowed(bAllowed: true);
		if (OnCameraOpModeChanged != null)
		{
			OnCameraOpModeChanged(m_OpMode);
		}
	}

	public void OnScreenResolutionChanged()
	{
		m_bResolutionChanged = true;
		m_ForceRefresh = true;
		if (OnTargetsUpdated != null)
		{
			OnTargetsUpdated();
		}
		m_bIsFullscreen = Screen.fullScreen;
	}

	public void VSyncOptionChanged()
	{
		if (OnVsyncOptionChanged != null)
		{
			OnVsyncOptionChanged();
		}
	}

	private void RearrangeBindings()
	{
		for (int i = 0; i < m_CameraBindings.Length; i++)
		{
			if (!(m_CameraBindings[i].m_Character == null) || !(m_CameraBindings[i].m_TargetPosition == Vector3.zero))
			{
				continue;
			}
			for (int j = i; j < m_CameraBindings.Length; j++)
			{
				if (m_CameraBindings[j].m_Character != null || m_CameraBindings[j].m_TargetPosition != Vector3.zero)
				{
					Character character = m_CameraBindings[i].m_Character;
					Vector3 targetPosition = m_CameraBindings[i].m_TargetPosition;
					PlayerBindingID playerBinding = m_CameraBindings[i].m_PlayerBinding;
					m_CameraBindings[i].m_Character = m_CameraBindings[j].m_Character;
					m_CameraBindings[i].m_TargetPosition = m_CameraBindings[j].m_TargetPosition;
					m_CameraBindings[i].m_PlayerBinding = m_CameraBindings[j].m_PlayerBinding;
					m_CameraBindings[j].m_Character = character;
					m_CameraBindings[j].m_TargetPosition = targetPosition;
					m_CameraBindings[j].m_PlayerBinding = playerBinding;
					break;
				}
			}
		}
	}

	public int GetZOffsetForFacade()
	{
		return m_ZOffsetForFacade;
	}

	public float GetDefaultCameraCutsceneOffset()
	{
		return 2 * FloorManager.GetInstance().m_FloorOffset;
	}

	public static Vector3 AdjustSplitscreenScaleForPlatform(Vector3 downScale)
	{
		if (m_Instance == null)
		{
			return downScale;
		}
		return new Vector3(1f / downScale.x, 1f / downScale.y, 1f / downScale.z);
	}

	public static T GetCorrectHudScaleForPlatform<T>(T downscaleValue, T upscaleValue)
	{
		if (m_Instance == null)
		{
			return downscaleValue;
		}
		return upscaleValue;
	}

	public CameraOpModes GetCameraManagerOpMode()
	{
		return m_OpMode;
	}
}
