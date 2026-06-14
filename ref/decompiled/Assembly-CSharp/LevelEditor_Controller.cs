using System;
using System.IO;
using AUTOGEN_T17Wwise_Enums;
using Rewired;
using UnityEngine;

public class LevelEditor_Controller : MonoBehaviour
{
	public enum AudioTypes
	{
		Ambient_Start,
		Ambient_Stop,
		NextVariation,
		DownFloor,
		UpFloor,
		Deleted,
		Marquee_Start,
		Marquee_Stop,
		Marquee_Finished,
		Placed,
		Zoom,
		Redo,
		Undo,
		Error,
		TabIn,
		TabOut,
		Save,
		Marquee_Start_NOCheck,
		Marquee_Stop_NOCheck
	}

	[Flags]
	public enum CopyEnum
	{
		Empty = 0,
		Marked = 1,
		MarkedRoom1 = 2,
		MarkedRoom2 = 4,
		MarkedRoom3 = 8,
		MarkedRoom4 = 0x10,
		MarkedAll = 0x1F,
		Scanned = 0x20,
		ScannedRoom1 = 0x40,
		ScannedRoom2 = 0x80,
		ScannedRoom3 = 0x100,
		ScannedRoom4 = 0x200,
		MarkedAndScanned = 0x21,
		MarkedAndScannedAll = 0x3FF,
		MarkedAndScannedRoom1 = 0x42,
		MarkedAndScannedRoom2 = 0x84,
		MarkedAndScannedRoom3 = 0x108,
		MarkedAndScannedRoom4 = 0x210
	}

	public delegate void SnapshotTakenResult(Texture2D texture);

	public delegate void SnapshotActions(bool bBefore);

	public enum EditMode
	{
		INVALID,
		NoBrush,
		BlockSelected,
		FreeDrawing,
		Marquee,
		MarqueeLine,
		Deleting,
		MovingCamera,
		SelectingObjectInLevel,
		SelectedObjectInLevel,
		MovingBlock,
		CopyMarquee,
		CopyMarqueeAdd,
		CopyMarqueeDelete,
		CopySelectedObjectInLevel,
		CopySelectedObjectInLevel_Edit,
		Zone_WaitingToCreate,
		Zone_Creating,
		Zone_Selected,
		Zone_Editing,
		Zone_Adding,
		Zone_Deleting
	}

	public enum DrawingStatus
	{
		Nothing,
		Painting,
		Marquee
	}

	public delegate void EditModeChanged(EditMode newMode);

	public delegate void BlockChanged(int iBrushID);

	public delegate void ZoomLevelChanged(float fLevel);

	[Flags]
	private enum MapMoveDirection
	{
		None = 0,
		Up = 1,
		Down = 2,
		Left = 4,
		Right = 8
	}

	[Flags]
	public enum ScanBits : byte
	{
		EMPTY = 1,
		OCCUPIED = 2,
		SCANISLAND = 4,
		SCANHOLE = 8,
		ADDED = 0x10
	}

	private static LevelEditor_Controller m_Instance;

	private BaseLevelManager m_LevelManager;

	private BuildingInstructionManager m_InstructionManager;

	private BuildingBlockManager m_BlockManager;

	private LevelEditorBrushController m_BrushController;

	private LevelEditor_UIController m_UIController;

	private LevelEditorHighLightManager m_HighlightManager;

	private LevelDetailsManager m_LevelDetailsMan;

	private LevelEditor_ZoneManager m_ZoneManager;

	private LevelEditor_Cursor m_Cursor;

	public Camera m_MainCamera;

	private Mouse m_Mouse;

	private Rewired.Player m_Player;

	public Texture2D m_PreviewTexture;

	private bool m_ValidPreviewTexture;

	private bool m_TriggerSnapshot;

	private bool m_bIsDefaultSnapshot;

	public T17Text m_RotationUI;

	private int m_CurrentVariation = 1;

	private int m_NumberOfVariations = 1;

	private int m_BlockWeWouldLikeToHave = -1;

	private int m_BadZone;

	public int m_TextureWidth = 716;

	public int m_TextureHeight = 326;

	private TextureFormat m_TextureFormat = TextureFormat.RGB24;

	private LevelDetailsManager.RequestResult m_SaveSnapshotCallback;

	public float m_fRecheckTimer = 0.25f;

	private float m_fCheckTimer = 0.1f;

	private float m_fOrthographicSize = 10f;

	private LevelEditor_ZoneManager.Zone m_CurrentZone;

	private LevelEditor_ZoneManager.Zone m_OverZone;

	private ZoneDetailsManager.ZoneTypes m_ZoneToCreate;

	private bool m_MarqueePlaying;

	private float m_fTimeUntilStopMarqueeSound;

	private float m_LastMarqueeValue;

	private BaseLevelEditor_BasePopout m_PopupControl;

	[SerializeField]
	private float m_fCloseCameraPanSpeed = 250f;

	[SerializeField]
	private float m_fFarCameraPanSpeed = 400f;

	[SerializeField]
	private float m_fCameraPanEdgeDistance = 0.05f;

	private bool[] m_CopyArea = new bool[14400];

	private CopyEnum[] m_CopyAreaFlags = new CopyEnum[14400];

	private SnapshotTakenResult m_SnapshotDelegate;

	private SnapshotActions m_SnapshotActionDelegate;

	private bool m_bMovingBlock;

	private EditMode m_EditMode = EditMode.NoBrush;

	private EditMode m_PreviousEditMode = EditMode.NoBrush;

	private bool m_CameraMoved;

	private BaseBuildingBlock.BuildingBlockDrawingMode m_DrawingMode;

	private bool m_AppHasFocus = true;

	private int m_CurrentBlock = -1;

	public T17Button m_UndoButton;

	public T17Button m_RedoButton;

	private bool m_bCanUndo;

	private bool m_bCanRedo;

	private float m_RepeatTimerMapMove;

	private MapMoveDirection m_MoveDirection;

	public float m_DelayBeforeRepeat = 0.5f;

	public float m_DelayBetweenRepeats = 0.1f;

	public Vector2 m_RawMouseToScreen = Vector2.zero;

	private GameObject m_Brush;

	private bool m_BrushVisibility_FromCursor;

	private int m_Block_X_Position;

	private int m_Block_Y_Position;

	private int m_LastSelected_X_Position;

	private int m_LastSelected_Y_Position;

	private int m_LastSelected_Block = -1;

	private GameObject m_LastSelected_Object;

	private GameObject m_SelectedBorder;

	private bool m_bSelectedBlockMenuShown;

	private Vector2 m_LastDrawnPosition = Vector2.zero;

	private bool m_OverControl;

	private bool m_bUpdateBrushPosition = true;

	private bool m_bForceValidation;

	private Vector2 m_BrushOffset = Vector2.zero;

	private bool m_bUpdateBrushPositionAfterRefresh;

	public float m_MarqueeAudioTimeout = 0.2f;

	private int m_LastMarqueePos_X;

	private int m_LastMarqueePos_Y;

	public UnityEngine.Object m_NewMarqueeResource;

	private LevelEditor_Marquee m_NewMarqueeBrush;

	private int m_iLastMarqueWidth = -1;

	private int m_iLastMarqueHeight = -1;

	private int m_iLastMarqueLeft = -1;

	private int m_iLastMarqueBottom = -1;

	private string m_strLastMarqueColor = "X";

	public bool m_MarqueeBrushActive;

	public int m_CurrentZoomLevel;

	public float[] m_ZoomLevels = new float[1] { 17f };

	private string m_AmbientsLoaded = string.Empty;

	private string m_EffectsLoaded = string.Empty;

	private bool m_bPlayAmbientWhenLoaded;

	public LevelEditor_SavingIcon m_SavingIcon;

	public GameObject m_Grid;

	public LevelEditor_ZoneControl m_ZoneControl;

	private bool m_bGridWasActiveBeforeSnapShot;

	private bool m_PushedSelectedBorder;

	public float m_ZoneErrorExpireIn = 3.5f;

	public LevelEditor_ZoneManager.Zone CurrentZone => m_CurrentZone;

	public ZoneDetailsManager.ZoneTypes ZoneToCreate => m_ZoneToCreate;

	private event EditModeChanged OnEditModeChanged;

	private event BlockChanged OnBlockChanged;

	public event ZoomLevelChanged OnZoomChanged;

	public static LevelEditor_Controller GetInstance()
	{
		return m_Instance;
	}

	private void Awake()
	{
		m_Instance = this;
		base.enabled = false;
	}

	protected virtual void OnDestroy()
	{
		if (this == m_Instance)
		{
			m_Instance = null;
		}
		if (m_NewMarqueeBrush != null)
		{
			UnityEngine.Object.Destroy(m_NewMarqueeBrush);
			m_NewMarqueeBrush = null;
		}
		UnloadAudioBanks();
	}

	public void Activate()
	{
		base.enabled = true;
	}

	public bool IsActivated()
	{
		return base.enabled;
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		m_AppHasFocus = hasFocus;
	}

	private void CreatePreviewTexture()
	{
		if (m_PreviewTexture == null)
		{
			m_PreviewTexture = new Texture2D(m_TextureWidth, m_TextureHeight, m_TextureFormat, mipmap: false);
		}
	}

	private void Start()
	{
		CreatePreviewTexture();
		if ((m_Cursor = LevelEditor_Cursor.GetInstance()) == null)
		{
			base.enabled = false;
			return;
		}
		m_Cursor.RegisterForBrushVisibityChange(OnBrushVisibilityChanged);
		m_Cursor.RegisterForOverControllerChange(OnOverControlChange);
		if (m_LevelManager == null)
		{
			m_LevelManager = BaseLevelManager.GetInstance();
			if (m_LevelManager == null)
			{
				base.enabled = false;
				return;
			}
		}
		if (m_LevelDetailsMan == null)
		{
			m_LevelDetailsMan = LevelDetailsManager.GetInstance();
			if (m_LevelDetailsMan == null)
			{
				base.enabled = false;
				return;
			}
		}
		if (m_ZoneManager == null)
		{
			m_ZoneManager = LevelEditor_ZoneManager.GetInstance();
			if (m_ZoneManager == null)
			{
				base.enabled = false;
				return;
			}
		}
		if (m_InstructionManager == null)
		{
			m_InstructionManager = BuildingInstructionManager.GetInstance();
			if (m_InstructionManager == null)
			{
				base.enabled = false;
				return;
			}
		}
		if (m_BlockManager == null)
		{
			m_BlockManager = BuildingBlockManager.GetInstance();
			if (m_BlockManager == null)
			{
				base.enabled = false;
				return;
			}
			m_BlockManager.RegisterLimitationChange(LimitationGroupChanged);
			LoadAudioBanks();
			Texture2D defaultLevelImage = m_BlockManager.GetDefaultLevelImage();
			if (defaultLevelImage == null)
			{
				defaultLevelImage = Resources.Load("LevelPreview") as Texture2D;
			}
			m_bIsDefaultSnapshot = true;
			SetPreviewSnapShot(m_BlockManager.GetDefaultLevelImage());
			LoadSnapshot();
		}
		if (m_UIController == null)
		{
			m_UIController = LevelEditor_UIController.GetInstance();
			if (m_UIController == null)
			{
				base.enabled = false;
				return;
			}
		}
		if (m_HighlightManager == null)
		{
			m_HighlightManager = LevelEditorHighLightManager.GetInstance();
			if (m_HighlightManager == null)
			{
				base.enabled = false;
				return;
			}
		}
		if (m_MainCamera == null)
		{
			m_MainCamera = Camera.main;
			if (m_MainCamera == null)
			{
				base.enabled = false;
			}
		}
		if (m_ZoomLevels.Length == 0)
		{
			base.enabled = false;
		}
		if (m_CurrentZoomLevel >= m_ZoomLevels.Length)
		{
			m_CurrentZoomLevel = 0;
		}
		if (base.enabled)
		{
			SetOrthagraphicSize(m_CurrentZoomLevel);
			if (this.OnZoomChanged != null)
			{
				this.OnZoomChanged(GetZoomPerc());
			}
		}
		if (!GetInputDevice())
		{
			base.enabled = false;
		}
		else
		{
			Mouse mouse = ReInput.controllers.Mouse;
			if (mouse == null)
			{
				base.enabled = false;
			}
			else
			{
				m_Player.controllers.AddController(mouse, removeFromOtherPlayers: true);
				T17EventSystem.ApplyCategories(m_Player, T17EventSystem.InputCateogryStates.LevelEditor);
			}
		}
		base.gameObject.AddComponent<T17BlockKeyboardAutoFocus>();
		UpdateLayerChanges();
	}

	private void Update()
	{
		if (!m_AppHasFocus)
		{
			return;
		}
		if (!m_OverControl && !IsPopupShown())
		{
			switch (m_EditMode)
			{
			case EditMode.NoBrush:
				UpdateState_NothingSelected();
				break;
			case EditMode.MovingCamera:
				UpdateState_MovingCamera();
				break;
			case EditMode.BlockSelected:
				UpdateState_BlockSelected();
				break;
			case EditMode.FreeDrawing:
				UpdateState_FreeDrawing();
				break;
			case EditMode.Marquee:
				UpdateState_Marquee();
				break;
			case EditMode.MarqueeLine:
				UpdateState_MarqueeLine();
				break;
			case EditMode.Deleting:
				UpdateState_Deleting();
				break;
			case EditMode.SelectingObjectInLevel:
				UpdateState_SelectingObjectInLevel();
				break;
			case EditMode.SelectedObjectInLevel:
				UpdateState_SelectedObjectInLevel();
				break;
			case EditMode.MovingBlock:
				UpdateState_MovingBlock();
				break;
			case EditMode.CopyMarquee:
				UpdateState_Copy();
				break;
			case EditMode.CopyMarqueeDelete:
				UpdateState_Copy_Delete();
				break;
			case EditMode.CopyMarqueeAdd:
				UpdateState_Copy_Add();
				break;
			case EditMode.CopySelectedObjectInLevel:
				UpdateState_CopySelectedObjectInLevel();
				break;
			case EditMode.CopySelectedObjectInLevel_Edit:
				UpdateState_CopySelectedObjectInLevel_Edit();
				break;
			case EditMode.Zone_WaitingToCreate:
				UpdateState_Zone_WaitingToCreate();
				break;
			case EditMode.Zone_Creating:
				UpdateState_Zone_Creating();
				break;
			case EditMode.Zone_Adding:
				UpdateState_Zone_Adding();
				break;
			case EditMode.Zone_Deleting:
				UpdateState_Zone_Deleting();
				break;
			case EditMode.Zone_Editing:
				UpdateState_Zone_Editing();
				break;
			case EditMode.Zone_Selected:
				UpdateState_Zone_Selected();
				break;
			}
			ZoneHouseKeeping();
			CheckSafeKeys();
		}
		else if (IsPopupShown())
		{
			UpdateState_PopupShown();
		}
		else
		{
			ZoneHouseKeeping();
			UpdateState_OverControl();
		}
		UpdateBlockPosition();
		UpdateButtons();
		if (m_bUpdateBrushPosition)
		{
			m_bUpdateBrushPosition = false;
			UpdateBrushPosition();
		}
		if (m_bUpdateBrushPositionAfterRefresh)
		{
			m_bUpdateBrushPosition = true;
			m_bForceValidation = true;
			m_bUpdateBrushPositionAfterRefresh = false;
		}
		UpdateAudio();
		if (m_fCheckTimer > 0f)
		{
			m_fCheckTimer -= Time.deltaTime;
			if (m_fCheckTimer <= 0f)
			{
				m_LevelDetailsMan.UpdateReachableFlags();
			}
		}
	}

	public void ResetTimer(bool bOnlyIfNotZero = false)
	{
		if (!bOnlyIfNotZero || m_fCheckTimer > 0f)
		{
			m_fCheckTimer = m_fRecheckTimer;
		}
	}

	private void UpdateLayerChanges()
	{
		BaseLevelManager.LevelLayers levelLayers = BaseLevelManager.LevelLayers.GroundFloor;
		while ((int)levelLayers < 6)
		{
			m_HighlightManager.m_MasterLayers[(uint)levelLayers].SetActive((int)levelLayers <= (int)m_LevelManager.m_CurrentLayer);
			levelLayers++;
		}
	}

	private void UpdateUI()
	{
		m_UIController.OnSomethingChanged();
	}

	private void ClearTimers()
	{
		m_RepeatTimerMapMove = 0f;
	}

	private bool ProcessPossibleCameraMove(bool bAllowLayerChange = true)
	{
		if (!GetInputDevice())
		{
			return false;
		}
		Vector2 zero = Vector2.zero;
		bool flag = false;
		bool flag2 = false;
		if (m_Player.GetButtonDown("MapUp"))
		{
			m_MoveDirection |= MapMoveDirection.Up;
			flag = true;
		}
		else if (!m_Player.GetButton("MapUp"))
		{
			m_MoveDirection &= ~MapMoveDirection.Up;
			flag = true;
		}
		if (m_Player.GetButtonDown("MapDown"))
		{
			m_MoveDirection |= MapMoveDirection.Down;
			flag = true;
		}
		else if (!m_Player.GetButton("MapDown"))
		{
			m_MoveDirection &= ~MapMoveDirection.Down;
			flag = true;
		}
		if (m_Player.GetButtonDown("MapLeft"))
		{
			m_MoveDirection |= MapMoveDirection.Left;
			flag = true;
		}
		else if (!m_Player.GetButton("MapLeft"))
		{
			m_MoveDirection &= ~MapMoveDirection.Left;
			flag = true;
		}
		if (m_Player.GetButtonDown("MapRight"))
		{
			m_MoveDirection |= MapMoveDirection.Right;
			flag = true;
		}
		else if (!m_Player.GetButton("MapRight"))
		{
			m_MoveDirection &= ~MapMoveDirection.Right;
			flag = true;
		}
		if (flag)
		{
			if (m_MoveDirection == MapMoveDirection.None)
			{
				m_RepeatTimerMapMove = 0f;
			}
			else if (m_RepeatTimerMapMove == 0f)
			{
				flag2 = true;
				m_RepeatTimerMapMove = Time.timeSinceLevelLoad + m_DelayBeforeRepeat;
			}
		}
		while (flag2 || (m_RepeatTimerMapMove != 0f && m_RepeatTimerMapMove < Time.timeSinceLevelLoad))
		{
			if (m_RepeatTimerMapMove < Time.timeSinceLevelLoad)
			{
				flag2 = true;
				m_RepeatTimerMapMove += m_DelayBetweenRepeats;
			}
			if (flag2)
			{
				flag2 = false;
				if ((m_MoveDirection & MapMoveDirection.Up) == MapMoveDirection.Up)
				{
					zero.y += 1f;
				}
				if ((m_MoveDirection & MapMoveDirection.Down) == MapMoveDirection.Down)
				{
					zero.y -= 1f;
				}
				if ((m_MoveDirection & MapMoveDirection.Left) == MapMoveDirection.Left)
				{
					zero.x -= 1f;
				}
				if ((m_MoveDirection & MapMoveDirection.Right) == MapMoveDirection.Right)
				{
					zero.x += 1f;
				}
			}
		}
		if (m_MoveDirection == MapMoveDirection.None)
		{
			Vector3 vector = m_MainCamera.ScreenToViewportPoint(m_Mouse.screenPosition);
			if (vector.x >= 0f && vector.y >= 0f && vector.x <= 1f && vector.y <= 1f)
			{
				float num = Mathf.Lerp(m_fCloseCameraPanSpeed, m_fFarCameraPanSpeed, (float)m_CurrentZoomLevel / (float)m_ZoomLevels.Length);
				float fCameraPanEdgeDistance = m_fCameraPanEdgeDistance;
				float num2 = 1f - m_fCameraPanEdgeDistance;
				if (vector.x < fCameraPanEdgeDistance)
				{
					zero.x -= (fCameraPanEdgeDistance - vector.x) * num * Time.deltaTime;
				}
				if (vector.x > num2)
				{
					zero.x += (0f - (num2 - vector.x)) * num * Time.deltaTime;
				}
				if (vector.y < fCameraPanEdgeDistance)
				{
					zero.y -= (fCameraPanEdgeDistance - vector.y) * num * Time.deltaTime;
				}
				if (vector.y > num2)
				{
					zero.y += (0f - (num2 - vector.y)) * num * Time.deltaTime;
				}
			}
		}
		flag = false;
		if (zero.x != 0f || zero.y != 0f)
		{
			flag = true;
			Vector3 localPosition = m_MainCamera.transform.localPosition;
			localPosition.x = Mathf.Clamp(localPosition.x + zero.x, -60f, 60f);
			localPosition.y = Mathf.Clamp(localPosition.y + zero.y, -60f, 59f);
			m_MainCamera.transform.localPosition = GetCorrectedPosition(localPosition);
		}
		if (m_Player.GetButtonUp("Zoom_In"))
		{
			if (m_CurrentZoomLevel > 0)
			{
				m_CurrentZoomLevel--;
				SetOrthagraphicSize(m_CurrentZoomLevel);
				flag = true;
				PlayAudio(AudioTypes.Zoom);
				if (this.OnZoomChanged != null)
				{
					this.OnZoomChanged(GetZoomPerc());
				}
			}
		}
		else if (m_Player.GetAxis("Zoom") > 0f)
		{
			if (m_CurrentZoomLevel > 0)
			{
				Vector2 vector2 = m_MainCamera.ScreenToWorldPoint(new Vector3(m_Mouse.screenPosition.x, m_Mouse.screenPosition.y, m_MainCamera.nearClipPlane));
				m_CurrentZoomLevel--;
				SetOrthagraphicSize(m_CurrentZoomLevel);
				flag = true;
				PlayAudio(AudioTypes.Zoom);
				Vector2 vector3 = m_MainCamera.ScreenToWorldPoint(new Vector3(m_Mouse.screenPosition.x, m_Mouse.screenPosition.y, m_MainCamera.nearClipPlane));
				Vector3 vector4 = new Vector3(m_MainCamera.transform.localPosition.x + (vector2.x - vector3.x), m_MainCamera.transform.localPosition.y + (vector2.y - vector3.y), m_MainCamera.transform.localPosition.z);
				vector4.x = Mathf.Clamp(vector4.x, -60f, 60f);
				vector4.y = Mathf.Clamp(vector4.y, -60f, 59f);
				m_MainCamera.transform.localPosition = GetCorrectedPosition(new Vector3(m_MainCamera.transform.localPosition.x + (vector2.x - vector3.x), m_MainCamera.transform.localPosition.y + (vector2.y - vector3.y), m_MainCamera.transform.localPosition.z));
				m_bUpdateBrushPosition = true;
				if (this.OnZoomChanged != null)
				{
					this.OnZoomChanged(GetZoomPerc());
				}
			}
		}
		else if (m_Player.GetButtonUp("Zoom_Out"))
		{
			if (m_CurrentZoomLevel < m_ZoomLevels.Length - 1)
			{
				m_CurrentZoomLevel++;
				SetOrthagraphicSize(m_CurrentZoomLevel);
				flag = true;
				PlayAudio(AudioTypes.Zoom);
				if (this.OnZoomChanged != null)
				{
					this.OnZoomChanged(GetZoomPerc());
				}
			}
		}
		else if (m_Player.GetAxis("Zoom") < 0f)
		{
			if (m_CurrentZoomLevel < m_ZoomLevels.Length - 1)
			{
				Vector2 vector5 = m_MainCamera.ScreenToWorldPoint(new Vector3(m_Mouse.screenPosition.x, m_Mouse.screenPosition.y, m_MainCamera.nearClipPlane));
				m_CurrentZoomLevel++;
				SetOrthagraphicSize(m_CurrentZoomLevel);
				flag = true;
				PlayAudio(AudioTypes.Zoom);
				Vector2 vector6 = m_MainCamera.ScreenToWorldPoint(new Vector3(m_Mouse.screenPosition.x, m_Mouse.screenPosition.y, m_MainCamera.nearClipPlane));
				Vector3 vPos = new Vector3(m_MainCamera.transform.localPosition.x + (vector5.x - vector6.x), m_MainCamera.transform.localPosition.y + (vector5.y - vector6.y), m_MainCamera.transform.localPosition.z);
				vPos.x = Mathf.Clamp(vPos.x, -60f, 60f);
				vPos.y = Mathf.Clamp(vPos.y, -60f, 59f);
				m_MainCamera.transform.localPosition = GetCorrectedPosition(vPos);
				m_bUpdateBrushPosition = true;
				if (this.OnZoomChanged != null)
				{
					this.OnZoomChanged(GetZoomPerc());
				}
			}
		}
		else if (m_Player.GetButtonUp("Layer_Up"))
		{
			IncLayer();
		}
		else if (m_Player.GetButtonUp("Layer_Down"))
		{
			DecLayer();
		}
		else if (m_Player.GetButtonUp("Layer_Ground"))
		{
			ChangeLayer(BaseLevelManager.LevelLayers.GroundFloor);
		}
		else if (m_Player.GetButtonUp("Layer_GroundVent"))
		{
			ChangeLayer(BaseLevelManager.LevelLayers.GroundFloor_Vent);
		}
		else if (m_Player.GetButtonUp("Layer_First"))
		{
			ChangeLayer(BaseLevelManager.LevelLayers.FirstFloor);
		}
		else if (m_Player.GetButtonUp("Layer_FirstVent"))
		{
			ChangeLayer(BaseLevelManager.LevelLayers.FirstFloor_Vent);
		}
		else if (m_Player.GetButtonUp("Layer_Roof"))
		{
			ChangeLayer(BaseLevelManager.LevelLayers.Roof);
		}
		else if (m_Player.GetButtonUp("Exit"))
		{
			m_UIController.OnExitButtonClicked();
			UpdateUI();
		}
		else if (m_Player.GetButtonUp("TogglePanels"))
		{
			m_UIController.ToggleAllMovablePanels();
		}
		else if (m_Player.GetButtonUp("Toggle_Grid") && m_Grid != null)
		{
			m_Grid.SetActive(!m_Grid.activeSelf);
		}
		return flag;
	}

	public float GetZoomPerc()
	{
		float num = m_ZoomLevels[0];
		float num2 = m_ZoomLevels[m_ZoomLevels.Length - 1];
		float num3 = m_ZoomLevels[m_CurrentZoomLevel];
		return (num3 - num) / (num2 - num);
	}

	private void CheckSafeKeys()
	{
		if (m_Player.GetButtonUp("Tab_Outside"))
		{
			m_UIController.ExternalChangePaleteTab(LevelEditor_UIController.BuildingBlockCategory.Outside);
		}
		else if (m_Player.GetButtonUp("Tab_Inside"))
		{
			m_UIController.ExternalChangePaleteTab(LevelEditor_UIController.BuildingBlockCategory.Inside);
		}
		else if (m_Player.GetButtonUp("Tab_Rooms"))
		{
			m_UIController.ExternalChangePaleteTab(LevelEditor_UIController.BuildingBlockCategory.Room);
		}
		else if (m_Player.GetButtonUp("Tab_Objects"))
		{
			m_UIController.ExternalChangePaleteTab(LevelEditor_UIController.BuildingBlockCategory.Object);
		}
	}

	private void UpdateState_OverControl()
	{
		if (!GetInputDevice())
		{
			return;
		}
		if (m_Player.GetButtonUp("Layer_Up"))
		{
			IncLayer();
		}
		else if (m_Player.GetButtonUp("Layer_Down"))
		{
			DecLayer();
		}
		else if (m_Player.GetButtonUp("Layer_Ground"))
		{
			ChangeLayer(BaseLevelManager.LevelLayers.GroundFloor);
		}
		else if (m_Player.GetButtonUp("Layer_GroundVent"))
		{
			ChangeLayer(BaseLevelManager.LevelLayers.GroundFloor_Vent);
		}
		else if (m_Player.GetButtonUp("Layer_First"))
		{
			ChangeLayer(BaseLevelManager.LevelLayers.FirstFloor);
		}
		else if (m_Player.GetButtonUp("Layer_FirstVent"))
		{
			ChangeLayer(BaseLevelManager.LevelLayers.FirstFloor_Vent);
		}
		else if (m_Player.GetButtonUp("Layer_Roof"))
		{
			ChangeLayer(BaseLevelManager.LevelLayers.Roof);
		}
		else if (m_Player.GetButtonUp("Exit"))
		{
			switch (m_EditMode)
			{
			case EditMode.BlockSelected:
				ExitBlockSelected();
				break;
			case EditMode.Deleting:
				ExitDeleting();
				break;
			case EditMode.FreeDrawing:
				ExitFreeDrawing();
				break;
			case EditMode.MovingCamera:
				break;
			case EditMode.NoBrush:
				if (!ExitNothingSelected())
				{
					m_UIController.OnExitButtonClicked();
				}
				break;
			case EditMode.Marquee:
				ExitMarquee();
				break;
			case EditMode.MarqueeLine:
				ExitMarqueeLine();
				break;
			case EditMode.SelectedObjectInLevel:
				ExitSelectedObjectInLevel();
				break;
			case EditMode.Zone_WaitingToCreate:
				ExitZoneWaitingToCreate();
				break;
			case EditMode.Zone_Editing:
				ExitZoneEditing();
				break;
			case EditMode.Zone_Selected:
				ExitZone_Selected();
				break;
			case EditMode.CopySelectedObjectInLevel:
			case EditMode.CopySelectedObjectInLevel_Edit:
				SetEditMode(EditMode.NoBrush);
				break;
			case EditMode.SelectingObjectInLevel:
				ExitSelectingObjectInLevel();
				break;
			case EditMode.MovingBlock:
				ExitMovingBlock();
				break;
			case EditMode.CopyMarquee:
			case EditMode.CopyMarqueeAdd:
			case EditMode.CopyMarqueeDelete:
			case EditMode.Zone_Creating:
				break;
			}
		}
		else if (!m_Player.GetButtonUp("Load"))
		{
			if (m_Player.GetButtonUp("Save"))
			{
				SaveTheLevel(bForceNew: false);
			}
			else if (m_Player.GetButtonUp("TogglePanels"))
			{
				m_UIController.ToggleAllMovablePanels();
			}
			else if (m_Player.GetButtonUp("Center"))
			{
				m_MainCamera.transform.localPosition = new Vector3(0f, 0f, m_MainCamera.transform.localPosition.z);
			}
			else if (m_Player.GetButtonUp("Undo"))
			{
				UndoLast();
			}
			else if (m_Player.GetButtonUp("Redo"))
			{
				RedoLast();
			}
			else if (m_Player.GetButtonUp("Toggle_Grid") && m_Grid != null)
			{
				m_Grid.SetActive(!m_Grid.activeSelf);
			}
		}
	}

	private void UpdateState_PopupShown()
	{
		if (!GetInputDevice())
		{
			return;
		}
		if (m_Player.GetButtonUp("Layer_Up"))
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.LayerChange))
			{
				IncLayer();
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.LayerChange))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetButtonUp("Layer_Down"))
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.LayerChange))
			{
				DecLayer();
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.LayerChange))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetButtonUp("Layer_Ground"))
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.LayerChange))
			{
				ChangeLayer(BaseLevelManager.LevelLayers.GroundFloor);
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.LayerChange))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetButtonUp("Layer_GroundVent"))
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.LayerChange))
			{
				ChangeLayer(BaseLevelManager.LevelLayers.GroundFloor_Vent);
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.LayerChange))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetButtonUp("Layer_First"))
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.LayerChange))
			{
				ChangeLayer(BaseLevelManager.LevelLayers.FirstFloor);
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.LayerChange))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetButtonUp("Layer_FirstVent"))
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.LayerChange))
			{
				ChangeLayer(BaseLevelManager.LevelLayers.FirstFloor_Vent);
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.LayerChange))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetButtonUp("Layer_Roof"))
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.LayerChange))
			{
				ChangeLayer(BaseLevelManager.LevelLayers.Roof);
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.LayerChange))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetButtonUp("Tab_Outside"))
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.TabChange))
			{
				m_UIController.ExternalChangePaleteTab(LevelEditor_UIController.BuildingBlockCategory.Outside);
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.TabChange))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetButtonUp("Tab_Inside"))
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.TabChange))
			{
				m_UIController.ExternalChangePaleteTab(LevelEditor_UIController.BuildingBlockCategory.Inside);
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.TabChange))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetButtonUp("Tab_Rooms"))
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.TabChange))
			{
				m_UIController.ExternalChangePaleteTab(LevelEditor_UIController.BuildingBlockCategory.Room);
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.TabChange))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetButtonUp("Tab_Objects"))
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.TabChange))
			{
				m_UIController.ExternalChangePaleteTab(LevelEditor_UIController.BuildingBlockCategory.Object);
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.TabChange))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetButtonUp("Exit"))
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.Escape))
			{
				switch (m_EditMode)
				{
				case EditMode.BlockSelected:
					ExitBlockSelected();
					return;
				case EditMode.Deleting:
					ExitDeleting();
					break;
				case EditMode.NoBrush:
					ExitNothingSelected();
					break;
				case EditMode.Marquee:
					ExitMarquee();
					break;
				case EditMode.MarqueeLine:
					ExitMarqueeLine();
					break;
				case EditMode.SelectedObjectInLevel:
					ExitSelectedObjectInLevel();
					break;
				case EditMode.CopySelectedObjectInLevel:
				case EditMode.CopySelectedObjectInLevel_Edit:
					SetEditMode(EditMode.NoBrush);
					UpdateUI();
					break;
				case EditMode.Zone_Editing:
					ExitZoneEditing();
					break;
				case EditMode.Zone_Selected:
					ExitZone_Selected();
					break;
				case EditMode.SelectingObjectInLevel:
					ExitSelectingObjectInLevel();
					break;
				case EditMode.MovingBlock:
					SetEditMode(EditMode.NoBrush);
					break;
				}
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.Escape))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetButtonUp("Load"))
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.File))
			{
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.File))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetButtonUp("Save"))
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.File))
			{
				SaveTheLevel(bForceNew: false);
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.File))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetButtonUp("TogglePanels"))
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.HideUI))
			{
				m_UIController.ToggleAllMovablePanels();
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.HideUI))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetButtonUp("Center"))
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.Center))
			{
				m_MainCamera.transform.localPosition = new Vector3(0f, 0f, m_MainCamera.transform.localPosition.z);
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.Center))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetButtonUp("Undo"))
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.UndoRedo))
			{
				UndoLast();
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.UndoRedo))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetButtonUp("Redo"))
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.UndoRedo))
			{
				RedoLast();
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.UndoRedo))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetButtonUp("Toggle_Grid"))
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.GridToggle) && m_Grid != null)
			{
				m_Grid.SetActive(!m_Grid.activeSelf);
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.GridToggle))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetButtonDown("MoveMap"))
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.Move))
			{
				SetEditMode(EditMode.MovingCamera);
				m_LastDrawnPosition = GetRawMousePosition();
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.Move))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetButtonDown("Delete"))
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.Delete))
			{
				m_LastDrawnPosition.x = m_Block_X_Position;
				m_LastDrawnPosition.y = m_Block_Y_Position;
				SetEditMode(EditMode.Deleting);
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.Delete))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetButtonUp("Zoom_In"))
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.Zoom) && m_CurrentZoomLevel > 0)
			{
				m_CurrentZoomLevel--;
				SetOrthagraphicSize(m_CurrentZoomLevel);
				PlayAudio(AudioTypes.Zoom);
				if (this.OnZoomChanged != null)
				{
					this.OnZoomChanged(GetZoomPerc());
				}
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.Zoom))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetAxis("Zoom") > 0f)
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.Zoom) && m_CurrentZoomLevel > 0)
			{
				Vector2 vector = m_MainCamera.ScreenToWorldPoint(new Vector3(m_Mouse.screenPosition.x, m_Mouse.screenPosition.y, m_MainCamera.nearClipPlane));
				m_CurrentZoomLevel--;
				SetOrthagraphicSize(m_CurrentZoomLevel);
				PlayAudio(AudioTypes.Zoom);
				Vector2 vector2 = m_MainCamera.ScreenToWorldPoint(new Vector3(m_Mouse.screenPosition.x, m_Mouse.screenPosition.y, m_MainCamera.nearClipPlane));
				Vector3 vector3 = new Vector3(m_MainCamera.transform.localPosition.x + (vector.x - vector2.x), m_MainCamera.transform.localPosition.y + (vector.y - vector2.y), m_MainCamera.transform.localPosition.z);
				vector3.x = Mathf.Clamp(vector3.x, -60f, 60f);
				vector3.y = Mathf.Clamp(vector3.y, -60f, 59f);
				m_MainCamera.transform.localPosition = GetCorrectedPosition(new Vector3(m_MainCamera.transform.localPosition.x + (vector.x - vector2.x), m_MainCamera.transform.localPosition.y + (vector.y - vector2.y), m_MainCamera.transform.localPosition.z));
				m_bUpdateBrushPosition = true;
				if (this.OnZoomChanged != null)
				{
					this.OnZoomChanged(GetZoomPerc());
				}
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.Zoom))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetButtonUp("Zoom_Out"))
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.Zoom) && m_CurrentZoomLevel < m_ZoomLevels.Length - 1)
			{
				m_CurrentZoomLevel++;
				SetOrthagraphicSize(m_CurrentZoomLevel);
				PlayAudio(AudioTypes.Zoom);
				if (this.OnZoomChanged != null)
				{
					this.OnZoomChanged(GetZoomPerc());
				}
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.Zoom))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetAxis("Zoom") < 0f)
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.Zoom) && m_CurrentZoomLevel < m_ZoomLevels.Length - 1)
			{
				Vector2 vector4 = m_MainCamera.ScreenToWorldPoint(new Vector3(m_Mouse.screenPosition.x, m_Mouse.screenPosition.y, m_MainCamera.nearClipPlane));
				m_CurrentZoomLevel++;
				SetOrthagraphicSize(m_CurrentZoomLevel);
				PlayAudio(AudioTypes.Zoom);
				Vector2 vector5 = m_MainCamera.ScreenToWorldPoint(new Vector3(m_Mouse.screenPosition.x, m_Mouse.screenPosition.y, m_MainCamera.nearClipPlane));
				Vector3 vPos = new Vector3(m_MainCamera.transform.localPosition.x + (vector4.x - vector5.x), m_MainCamera.transform.localPosition.y + (vector4.y - vector5.y), m_MainCamera.transform.localPosition.z);
				vPos.x = Mathf.Clamp(vPos.x, -60f, 60f);
				vPos.y = Mathf.Clamp(vPos.y, -60f, 59f);
				m_MainCamera.transform.localPosition = GetCorrectedPosition(vPos);
				m_bUpdateBrushPosition = true;
				if (this.OnZoomChanged != null)
				{
					this.OnZoomChanged(GetZoomPerc());
				}
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.Zoom))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetButtonDown("MapUp"))
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.Move))
			{
				m_MoveDirection |= MapMoveDirection.Up;
				if (m_RepeatTimerMapMove == 0f)
				{
					m_RepeatTimerMapMove = Time.timeSinceLevelLoad + 0.001f;
				}
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.Move))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetButtonDown("MapDown"))
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.Move))
			{
				m_MoveDirection |= MapMoveDirection.Down;
				if (m_RepeatTimerMapMove == 0f)
				{
					m_RepeatTimerMapMove = Time.timeSinceLevelLoad + 0.001f;
				}
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.Move))
			{
				HidePopup();
			}
		}
		else if (m_Player.GetButtonDown("MapLeft"))
		{
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.Move))
			{
				m_MoveDirection |= MapMoveDirection.Left;
				if (m_RepeatTimerMapMove == 0f)
				{
					m_RepeatTimerMapMove = Time.timeSinceLevelLoad + 0.001f;
				}
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.Move))
			{
				HidePopup();
			}
		}
		else
		{
			if (!m_Player.GetButtonDown("MapRight"))
			{
				return;
			}
			if (!m_PopupControl.IsActionBlocked(BaseLevelEditor_BasePopout.Action.Move))
			{
				m_MoveDirection |= MapMoveDirection.Right;
				if (m_RepeatTimerMapMove == 0f)
				{
					m_RepeatTimerMapMove = Time.timeSinceLevelLoad + 0.001f;
				}
			}
			if (m_PopupControl.IsActionAHideAction(BaseLevelEditor_BasePopout.Action.Move))
			{
				HidePopup();
			}
		}
	}

	private void IncLayer()
	{
		if (m_LevelManager.m_CurrentLayer != BaseLevelManager.LevelLayers.Roof)
		{
			ChangeLayer(m_LevelManager.m_CurrentLayer + 1);
		}
	}

	private void DecLayer()
	{
		if (m_LevelManager.m_CurrentLayer != BaseLevelManager.LevelLayers.GroundFloor)
		{
			ChangeLayer(m_LevelManager.m_CurrentLayer - 1);
		}
	}

	public void ChangeLayer(BaseLevelManager.LevelLayers layer, bool bPlayAudio = true)
	{
		if (m_EditMode == EditMode.Zone_Editing)
		{
			ExitZoneEditing();
		}
		if (m_LevelManager.m_CurrentLayer == layer)
		{
			return;
		}
		ResetSelectedMode(bResetMode: true);
		bool flag = (int)layer < (int)m_LevelManager.m_CurrentLayer;
		BuildingInstructionManager.GetInstance().ChangeLayer(layer);
		ValidateBrush();
		if (bPlayAudio)
		{
			if (flag)
			{
				PlayAudio(AudioTypes.DownFloor);
			}
			else
			{
				PlayAudio(AudioTypes.UpFloor);
			}
		}
		UpdateLayerChanges();
		UpdateUI();
	}

	private void ValidateBrush()
	{
		BaseBuildingBlock block = BuildingBlockManager.GetBlock(m_CurrentBlock);
		if (block == null && m_BlockWeWouldLikeToHave != -1)
		{
			block = BuildingBlockManager.GetBlock(m_BlockWeWouldLikeToHave);
		}
		if (!(block != null))
		{
			return;
		}
		if (!block.IsValidForLayer(m_LevelManager.m_CurrentLayer))
		{
			int num = -1;
			if (block.BlockType == BaseBuildingBlock.BuildingBlockType.Complex || block.BlockType == BaseBuildingBlock.BuildingBlockType.Room)
			{
				BuildingBlock_Room buildingBlock_Room = block as BuildingBlock_Room;
				if (buildingBlock_Room != null && buildingBlock_Room.m_AlternateBlocks.Count == 6)
				{
					BaseBuildingBlock block2 = BuildingBlockManager.GetBlock(buildingBlock_Room.m_AlternateBlocks[(int)m_LevelManager.m_CurrentLayer]);
					if (block2 != null && block2.IsValidForLayer(m_LevelManager.m_CurrentLayer))
					{
						num = buildingBlock_Room.m_AlternateBlocks[(int)m_LevelManager.m_CurrentLayer];
					}
				}
			}
			SetCurrentBlock(num, num != -1);
		}
		else if (m_CurrentBlock != block.m_ID)
		{
			SetCurrentBlock(block.m_ID);
		}
	}

	private bool NextVariation(bool bNext)
	{
		BaseBuildingBlock block = BuildingBlockManager.GetBlock(m_CurrentBlock);
		if (block != null && block.m_Variation != -1)
		{
			if (bNext)
			{
				SetCurrentBlock(block.m_Variation);
			}
			else
			{
				int iID = m_CurrentBlock;
				BaseBuildingBlock block2 = BuildingBlockManager.GetBlock(block.m_Variation);
				while (block2 != null)
				{
					iID = block2.m_ID;
					if (block2.m_Variation == m_CurrentBlock)
					{
						break;
					}
					block2 = BuildingBlockManager.GetBlock(block2.m_Variation);
				}
				SetCurrentBlock(iID);
			}
			PlayAudio(AudioTypes.NextVariation);
			return true;
		}
		return false;
	}

	private bool GetInputDevice()
	{
		if (m_Mouse == null)
		{
			if (m_Player == null)
			{
				Gamer primaryGamer = Gamer.GetPrimaryGamer();
				if (primaryGamer == null)
				{
					return false;
				}
				m_Player = primaryGamer.m_RewiredPlayer;
				if (m_Player == null)
				{
					return false;
				}
			}
			if (m_Player.controllers == null)
			{
				return false;
			}
			m_Mouse = m_Player.controllers.Mouse;
		}
		return m_Mouse != null;
	}

	private void UpdateBlockPosition()
	{
		Vector2 mousePosition = GetMousePosition();
		int num = Mathf.Clamp((int)mousePosition.x, 0, 119);
		int num2 = Mathf.Clamp((int)mousePosition.y, 0, 117);
		if (m_Block_X_Position != num)
		{
			m_bUpdateBrushPosition = true;
			m_Block_X_Position = num;
		}
		if (m_Block_Y_Position != num2)
		{
			m_bUpdateBrushPosition = true;
			m_Block_Y_Position = num2;
		}
	}

	private Vector2 GetMousePosition()
	{
		Vector2 result = Vector2.zero;
		if (GetInputDevice())
		{
			result = (m_RawMouseToScreen = m_MainCamera.ScreenToWorldPoint(new Vector3(m_Mouse.screenPosition.x, m_Mouse.screenPosition.y, m_MainCamera.nearClipPlane)));
			Vector3 vector = new Vector3(result.x, result.y, -40f);
			Debug.DrawLine(vector + new Vector3(-30f, 0f, 0f), vector + new Vector3(30f, 0f, 0f), Color.red);
			Debug.DrawLine(vector + new Vector3(0f, -30f, 0f), vector + new Vector3(0f, 30f, 0f), Color.red);
			result.x += 60f;
			result.y += 60f;
			result.x = Mathf.Clamp(result.x, 0f, (float)Screen.width - 1f);
			result.y = Mathf.Clamp(result.y, 0f, (float)Screen.height - 1f);
		}
		return result;
	}

	public Vector2 GetRawMousePosition()
	{
		Vector2 result = Vector2.zero;
		if (GetInputDevice())
		{
			result = m_Mouse.screenPosition;
		}
		return result;
	}

	public void RegisterEditModeChange(EditModeChanged editModeChangedDelegate)
	{
		if (editModeChangedDelegate != null)
		{
			OnEditModeChanged += editModeChangedDelegate;
		}
	}

	public void RegisterBlockChange(BlockChanged blockChangedDelegate)
	{
		if (blockChangedDelegate != null)
		{
			OnBlockChanged += blockChangedDelegate;
		}
	}

	public void RegisterZoomChange(ZoomLevelChanged zoomChangedDelegate)
	{
		if (zoomChangedDelegate != null)
		{
			OnZoomChanged += zoomChangedDelegate;
			zoomChangedDelegate(GetZoomPerc());
		}
	}

	private void SetEditMode(EditMode newMode)
	{
		if (m_EditMode == newMode)
		{
			return;
		}
		switch (m_EditMode)
		{
		case EditMode.MovingCamera:
			if (m_PreviousEditMode == newMode)
			{
			}
			break;
		case EditMode.Zone_WaitingToCreate:
		{
			FloatingZoneIcon instance = FloatingZoneIcon.GetInstance();
			if (instance != null)
			{
				instance.Show(bShow: false);
			}
			break;
		}
		}
		if (this.OnEditModeChanged != null)
		{
			this.OnEditModeChanged(newMode);
		}
		m_bUpdateBrushPosition = true;
		ClearTimers();
		switch (newMode)
		{
		case EditMode.Marquee:
		case EditMode.MarqueeLine:
		case EditMode.Deleting:
		case EditMode.CopyMarquee:
		case EditMode.CopyMarqueeAdd:
		case EditMode.CopyMarqueeDelete:
		case EditMode.Zone_Creating:
		case EditMode.Zone_Adding:
		case EditMode.Zone_Deleting:
			PlayAudio(AudioTypes.Marquee_Start);
			break;
		case EditMode.SelectingObjectInLevel:
			m_LastSelected_X_Position = -1;
			m_LastSelected_Y_Position = -1;
			m_LastSelected_Object = null;
			m_LastSelected_Block = -1;
			break;
		case EditMode.BlockSelected:
			if (m_bMovingBlock)
			{
				m_EditMode = EditMode.MovingBlock;
			}
			break;
		case EditMode.NoBrush:
			SetZoneToCreate(ZoneDetailsManager.ZoneTypes.INVALID);
			break;
		case EditMode.MovingCamera:
			m_PreviousEditMode = m_EditMode;
			m_CameraMoved = false;
			break;
		case EditMode.Zone_WaitingToCreate:
		{
			FloatingZoneIcon instance2 = FloatingZoneIcon.GetInstance();
			if (instance2 != null)
			{
				instance2.SetTheZoneType(m_ZoneToCreate);
				instance2.SetTheLayer(m_LevelManager.m_CurrentLayer);
				instance2.SetTheTilePosition(m_Block_X_Position, m_Block_Y_Position);
				instance2.Show(bShow: true);
			}
			break;
		}
		}
		m_EditMode = newMode;
	}

	public EditMode GetEditMode()
	{
		return m_EditMode;
	}

	public EditMode GetNoneTemporaryEditMode()
	{
		if (m_EditMode == EditMode.MovingCamera)
		{
			return m_PreviousEditMode;
		}
		return m_EditMode;
	}

	public void OnBrushVisibilityChanged(bool bVisible)
	{
		if (m_BrushVisibility_FromCursor != bVisible)
		{
			m_BrushVisibility_FromCursor = bVisible;
			m_bUpdateBrushPosition = true;
		}
	}

	public void OnOverControlChange(bool bOver)
	{
		if (m_OverControl != bOver)
		{
			m_OverControl = bOver;
			m_bUpdateBrushPosition = true;
		}
	}

	private void SetCurrentBlock(int iID, bool bStoreAsAlternate = true)
	{
		if (m_CurrentBlock == iID || m_LevelManager == null)
		{
			return;
		}
		ResetSelectedMode(bResetMode: false);
		m_bUpdateBrushPosition = true;
		m_bForceValidation = true;
		m_CurrentBlock = iID;
		m_LevelManager.SetCurrentBuildingBlock(iID);
		if (bStoreAsAlternate)
		{
			m_BlockWeWouldLikeToHave = iID;
		}
		m_CurrentVariation = 1;
		m_NumberOfVariations = 1;
		if (iID == -1)
		{
			if (m_RotationUI != null)
			{
				m_RotationUI.gameObject.SetActive(value: false);
			}
			SetEditMode(EditMode.NoBrush);
		}
		else
		{
			SetEditMode(EditMode.BlockSelected);
		}
		if (m_Brush != null)
		{
			UnityEngine.Object.Destroy(m_Brush);
			m_Brush = null;
			m_BrushController = null;
		}
		if (iID != -1)
		{
			GameObject blockBrush = BuildingBlockManager.GetBlockBrush(iID);
			if (blockBrush != null)
			{
				m_Brush = UnityEngine.Object.Instantiate(blockBrush, m_HighlightManager.m_LevelBase.transform);
				m_BrushController = m_Brush.GetComponent<LevelEditorBrushController>();
			}
			BaseBuildingBlock block = BuildingBlockManager.GetBlock(iID);
			if (block != null)
			{
				BaseBuildingBlock baseBuildingBlock = block;
				while (baseBuildingBlock != null && !baseBuildingBlock.m_VariationSelectable)
				{
					baseBuildingBlock = BuildingBlockManager.GetBlock(baseBuildingBlock.m_Variation);
				}
				bool flag = !block.m_VariationSelectable;
				BaseBuildingBlock block2 = BuildingBlockManager.GetBlock(baseBuildingBlock.m_Variation);
				while (block2 != null && block2 != baseBuildingBlock)
				{
					m_NumberOfVariations++;
					if (flag)
					{
						m_CurrentVariation++;
					}
					if (block2 == block)
					{
						flag = false;
					}
					block2 = BuildingBlockManager.GetBlock(block2.m_Variation);
				}
				m_BrushOffset.Set(0f, 0f);
				if (block.m_Footprint != null)
				{
					float num = (float)((block.m_Footprint.m_iW - 1) / 2) + (float)block.m_Footprint.m_iLeft;
					float num2 = (float)((block.m_Footprint.m_iH - 1) / 2) + (float)block.m_Footprint.m_iBottom;
					m_BrushOffset.Set(0f - num, 0f - num2);
				}
				if (block.m_DrawingTool == BaseBuildingBlock.BuildingBlockDrawingMode.INVALID)
				{
					switch (block.BlockType)
					{
					case BaseBuildingBlock.BuildingBlockType.Decoration:
					case BaseBuildingBlock.BuildingBlockType.Object:
					case BaseBuildingBlock.BuildingBlockType.Complex:
					case BaseBuildingBlock.BuildingBlockType.Room:
						m_DrawingMode = BaseBuildingBlock.BuildingBlockDrawingMode.Stamp;
						break;
					case BaseBuildingBlock.BuildingBlockType.Wall:
						m_DrawingMode = BaseBuildingBlock.BuildingBlockDrawingMode.MarqueeLine;
						break;
					case BaseBuildingBlock.BuildingBlockType.Tile:
						m_DrawingMode = BaseBuildingBlock.BuildingBlockDrawingMode.Marquee;
						break;
					default:
						m_DrawingMode = BaseBuildingBlock.BuildingBlockDrawingMode.INVALID;
						break;
					}
				}
				else
				{
					m_DrawingMode = block.m_DrawingTool;
				}
			}
			else
			{
				m_DrawingMode = BaseBuildingBlock.BuildingBlockDrawingMode.INVALID;
			}
		}
		if (this.OnBlockChanged != null)
		{
			this.OnBlockChanged(iID);
		}
	}

	private void UpdateBrushPosition()
	{
		bool flag = false;
		bool flag2 = false;
		switch (m_EditMode)
		{
		case EditMode.BlockSelected:
		case EditMode.FreeDrawing:
		case EditMode.MovingBlock:
			flag2 = true;
			break;
		case EditMode.Deleting:
			flag = true;
			break;
		case EditMode.Marquee:
		case EditMode.MarqueeLine:
		case EditMode.CopyMarquee:
		case EditMode.CopyMarqueeAdd:
		case EditMode.CopyMarqueeDelete:
		case EditMode.Zone_Creating:
		case EditMode.Zone_Adding:
		case EditMode.Zone_Deleting:
			flag = true;
			break;
		}
		if (m_Brush != null)
		{
			m_Brush.SetActive(flag2);
			m_Brush.transform.localPosition = new Vector3((float)m_Block_X_Position - 59.5f + m_BrushOffset.x, (float)m_Block_Y_Position - 59.5f + m_BrushOffset.y, -50f);
			if (m_RotationUI != null)
			{
				BaseBuildingBlock block = BuildingBlockManager.GetBlock(m_CurrentBlock);
				if (block != null && block.m_Footprint != null)
				{
					bool active = flag2 && m_BrushVisibility_FromCursor && m_NumberOfVariations > 1;
					m_RotationUI.gameObject.SetActive(active);
					Vector3 zero = Vector3.zero;
					zero = m_Brush.transform.localPosition;
					zero.x += (float)block.m_Footprint.m_iLeft + (float)block.m_Footprint.GetWidth() / 2f - 0.5f;
					zero.y += (float)(block.m_Footprint.GetHeight() + block.m_Footprint.m_iBottom) + 1f;
					zero.z -= 30f;
					m_RotationUI.transform.position = zero;
					Localization.Get("Text.Editor.Controls.Rotation", out var localized);
					m_RotationUI.text = "[" + localized + "]\t" + m_CurrentVariation + " /" + m_NumberOfVariations;
				}
			}
			if (m_BrushController != null)
			{
				m_BrushController.ValidateElements(IsBlockOutOfStock(), m_bForceValidation);
				m_UIController.SetBrushError(m_BrushController.GetAllErrors());
				m_bForceValidation = false;
				m_BrushController.SetElementVisability(m_BrushVisibility_FromCursor);
				if (m_BrushController.m_VisualRep != null && m_BrushController.m_VisualRep.activeSelf != m_BrushVisibility_FromCursor)
				{
					m_BrushController.m_VisualRep.SetActive(m_BrushVisibility_FromCursor);
				}
			}
		}
		if (flag)
		{
			m_MarqueeBrushActive = true;
			int num = Mathf.Min((int)m_LastDrawnPosition.x, m_Block_X_Position);
			int num2 = Mathf.Min((int)m_LastDrawnPosition.y, m_Block_Y_Position);
			int num3 = Mathf.Abs(m_Block_X_Position - (int)m_LastDrawnPosition.x) + 1;
			int num4 = Mathf.Abs(m_Block_Y_Position - (int)m_LastDrawnPosition.y) + 1;
			if (m_EditMode == EditMode.MarqueeLine)
			{
				if (num3 <= num4)
				{
					num3 = 1;
					num = (int)m_LastDrawnPosition.x;
				}
				else
				{
					num4 = 1;
					num2 = (int)m_LastDrawnPosition.y;
				}
			}
			if (num < 0)
			{
				num3 += num;
				num = 0;
			}
			else if (num + num3 > 120)
			{
				num3 = 120 - num;
			}
			if (num2 < 0)
			{
				num4 += num2;
				num2 = 0;
			}
			else if (num2 + num4 > 118)
			{
				num4 = 118 - num2;
			}
			switch (m_EditMode)
			{
			case EditMode.CopyMarqueeAdd:
			case EditMode.Zone_Adding:
				CreateMarquee(num3, num4, num, num2, "add");
				break;
			case EditMode.Zone_Creating:
				CreateMarquee(num3, num4, num, num2, "createzone");
				break;
			case EditMode.Deleting:
			case EditMode.CopyMarqueeDelete:
			case EditMode.Zone_Deleting:
				CreateMarquee(num3, num4, num, num2, "delete");
				break;
			default:
				CreateMarquee(num3, num4, num, num2, "draw");
				break;
			}
		}
		else
		{
			m_MarqueeBrushActive = false;
			HideMarquee();
		}
	}

	private void UpdateButtons()
	{
		m_bCanRedo = m_InstructionManager.CanRedo();
		m_bCanUndo = m_InstructionManager.CanUndo();
		if (m_UndoButton != null)
		{
			m_UndoButton.interactable = m_bCanUndo;
		}
		if (m_RedoButton != null)
		{
			m_RedoButton.interactable = m_bCanRedo;
		}
	}

	public void UndoLast()
	{
		if (m_bCanUndo)
		{
			ResetTimer();
			ResetSelectedMode(bResetMode: true);
			m_InstructionManager.Undo();
			UpdateLayerChanges();
			UpdateUI();
			m_bUpdateBrushPositionAfterRefresh = true;
			m_HighlightManager.RequestRescan();
			PlayAudio(AudioTypes.Undo);
			ValidateBrush();
		}
	}

	public void RedoLast()
	{
		if (m_bCanRedo)
		{
			ResetTimer();
			ResetSelectedMode(bResetMode: true);
			m_InstructionManager.Redo();
			UpdateLayerChanges();
			UpdateUI();
			m_bUpdateBrushPositionAfterRefresh = true;
			PlayAudio(AudioTypes.Redo);
			ValidateBrush();
		}
	}

	private bool CanBlockBePlaced(BaseBuildingBlock thisBlock = null)
	{
		if (m_CurrentBlock != -1 && m_BrushController != null)
		{
			bool flag = m_BrushController.AreWeValid();
			m_UIController.SetBrushError(m_BrushController.GetAllErrors());
			if (flag)
			{
				return !IsBlockOutOfStock(thisBlock);
			}
		}
		return false;
	}

	private bool IsBlockOutOfStock(BaseBuildingBlock thisBlock = null)
	{
		if (m_CurrentBlock != -1)
		{
			if (thisBlock == null)
			{
				thisBlock = BuildingBlockManager.GetBlock(m_CurrentBlock);
			}
			if (thisBlock != null && (thisBlock.m_LimitationGroup == -1 || m_BlockManager.m_LimitationGroups[thisBlock.m_LimitationGroup].m_Max == 0 || m_BlockManager.m_LimitationGroups[thisBlock.m_LimitationGroup].m_Max >= m_BlockManager.m_LimitationGroups[thisBlock.m_LimitationGroup].m_CurrentTotal + thisBlock.m_LimitationCount))
			{
				return false;
			}
		}
		return true;
	}

	private void UpdateState_NothingSelected()
	{
		if (m_Player.GetButtonDown("MoveMap"))
		{
			SetEditMode(EditMode.MovingCamera);
			m_LastDrawnPosition = GetRawMousePosition();
		}
		else if (m_Player.GetButtonDown("Delete"))
		{
			m_LastDrawnPosition.x = m_Block_X_Position;
			m_LastDrawnPosition.y = m_Block_Y_Position;
			SetEditMode(EditMode.Deleting);
		}
		else if (m_Player.GetButtonUp("Center"))
		{
			m_MainCamera.transform.localPosition = new Vector3(0f, 0f, m_MainCamera.transform.localPosition.z);
		}
		else if (m_Player.GetButtonUp("Undo"))
		{
			UndoLast();
		}
		else if (m_Player.GetButtonUp("Redo"))
		{
			RedoLast();
		}
		else if (m_Player.GetButtonUp("Exit"))
		{
			if (ExitNothingSelected())
			{
				return;
			}
		}
		else if (m_Player.GetButtonDown("Act"))
		{
			SetEditMode(EditMode.SelectingObjectInLevel);
		}
		else if (m_Player.GetButtonUp("Save"))
		{
			SaveTheLevel(bForceNew: false);
		}
		ProcessPossibleCameraMove();
	}

	private bool ExitNothingSelected()
	{
		if (m_CurrentZone != null)
		{
			ClearCurrentZone();
			return true;
		}
		return false;
	}

	public int GetBlockWeAreOn(int iX, int iY, ref GameObject blockObject, ref int iRoom, bool bCareAboutLayer = true)
	{
		iRoom = 0;
		int result = -1;
		BaseLevelManager.TileIDData tileIDData = BaseLevelManager.TileIDData.IDMask;
		blockObject = null;
		if (iX < 0 || iY < 0 || iX >= 120 || iY >= 118)
		{
			return result;
		}
		int num = iY * 120 + iX;
		BaseLevelManager.TileProperty tileProperty = m_LevelManager.m_BuildingLayers[(uint)m_LevelManager.m_CurrentLayer].m_TileProperties[num];
		if ((tileProperty & BaseLevelManager.TileProperty.ObjectMask) == BaseLevelManager.TileProperty.ObjectMask || (tileProperty & BaseLevelManager.TileProperty.DecorationMask) == BaseLevelManager.TileProperty.DecorationMask)
		{
			tileIDData = m_LevelManager.m_BuildingLayers[(uint)m_LevelManager.m_CurrentLayer].m_ObjectTileIDs[num] & BaseLevelManager.TileIDData.IDMask;
			if (tileIDData == BaseLevelManager.TileIDData.IDMask)
			{
				return result;
			}
			blockObject = m_LevelManager.m_BuildingLayers[(uint)m_LevelManager.m_CurrentLayer].m_ObjectTileObjects[num];
			result = (int)tileIDData;
		}
		else if ((tileProperty & BaseLevelManager.TileProperty.WallMask) == BaseLevelManager.TileProperty.WallMask)
		{
			tileIDData = m_LevelManager.m_BuildingLayers[(uint)m_LevelManager.m_CurrentLayer].m_WallTileIDs[num] & BaseLevelManager.TileIDData.IDMask;
			if (tileIDData == BaseLevelManager.TileIDData.IDMask)
			{
				return result;
			}
			blockObject = m_LevelManager.m_BuildingLayers[(uint)m_LevelManager.m_CurrentLayer].m_WallTileObjects[num];
			result = (int)tileIDData;
		}
		else
		{
			if ((tileProperty & BaseLevelManager.TileProperty.TileMask) != BaseLevelManager.TileProperty.TileMask)
			{
				return -1;
			}
			tileIDData = m_LevelManager.m_BuildingLayers[(uint)m_LevelManager.m_CurrentLayer].m_TileTileIDs[num] & BaseLevelManager.TileIDData.IDMask;
			if (tileIDData == BaseLevelManager.TileIDData.IDMask)
			{
				return result;
			}
			blockObject = m_LevelManager.m_BuildingLayers[(uint)m_LevelManager.m_CurrentLayer].m_TileTileObjects[num];
			result = (int)tileIDData;
		}
		BaseLevelManager.RoomProperty roomProperty = m_LevelManager.m_BuildingLayers[(uint)m_LevelManager.m_CurrentLayer].m_RoomPropertiesMasks[num];
		if (roomProperty != 0)
		{
			if (BaseLevelManager.TotalRoomNumbersInProperty(roomProperty) > 1)
			{
				blockObject = null;
				return -1;
			}
			iRoom = BaseLevelManager.GetRoomNumberFromProperty(ref m_LevelManager.m_BuildingLayers[(uint)m_LevelManager.m_CurrentLayer], num);
			result = m_LevelManager.GetBlockIDFromComplexAllocation(iRoom);
		}
		if (result != -1)
		{
			BaseBuildingBlock block = BuildingBlockManager.GetBlock(result);
			if (block == null || block.m_AutomaticBlock)
			{
				blockObject = null;
				iRoom = 0;
				return -1;
			}
			if (bCareAboutLayer && !block.IsValidForLayer(m_LevelManager.m_CurrentLayer))
			{
				bool flag = false;
				if (block.BlockType == BaseBuildingBlock.BuildingBlockType.Complex || block.BlockType == BaseBuildingBlock.BuildingBlockType.Room)
				{
					BuildingBlock_Room buildingBlock_Room = block as BuildingBlock_Room;
					if (buildingBlock_Room != null && buildingBlock_Room.m_AlternateBlocks.Count == 6)
					{
						int num2 = buildingBlock_Room.m_AlternateBlocks[(int)m_LevelManager.m_CurrentLayer];
						BaseBuildingBlock block2 = BuildingBlockManager.GetBlock(num2);
						if (block2 != null && block2.IsValidForLayer(m_LevelManager.m_CurrentLayer))
						{
							result = num2;
							flag = true;
						}
					}
				}
				if (!flag)
				{
					blockObject = null;
					iRoom = 0;
					result = -1;
				}
			}
		}
		return result;
	}

	private void UpdateState_SelectingObjectInLevel()
	{
		if (m_Block_X_Position != m_LastSelected_X_Position || m_Block_Y_Position != m_LastSelected_Y_Position)
		{
			m_LastSelected_X_Position = m_Block_X_Position;
			m_LastSelected_Y_Position = m_Block_Y_Position;
			GameObject blockObject = null;
			int iRoom = 0;
			int blockWeAreOn = GetBlockWeAreOn(m_LastSelected_X_Position, m_LastSelected_Y_Position, ref blockObject, ref iRoom, bCareAboutLayer: false);
			if (blockObject != m_LastSelected_Object || blockWeAreOn != m_LastSelected_Block)
			{
				if (m_SelectedBorder != null)
				{
					UnityEngine.Object.Destroy(m_SelectedBorder);
					m_SelectedBorder = null;
				}
				m_LastSelected_Object = blockObject;
				m_LastSelected_Block = blockWeAreOn;
				if (m_LastSelected_Block != -1)
				{
					BaseBuildingBlock block = BuildingBlockManager.GetBlock(m_LastSelected_Block);
					if (block != null)
					{
						switch (block.BlockType)
						{
						default:
							return;
						case BaseBuildingBlock.BuildingBlockType.Tile:
						case BaseBuildingBlock.BuildingBlockType.Wall:
						case BaseBuildingBlock.BuildingBlockType.Decoration:
						case BaseBuildingBlock.BuildingBlockType.Object:
						{
							m_SelectedBorder = new GameObject("Border");
							m_SelectedBorder.transform.SetParent(m_LastSelected_Object.transform);
							float num = block.m_Footprint.m_iLeft * -1;
							num -= (float)(block.m_Footprint.m_iW - 1) / 2f;
							float num2 = block.m_Footprint.m_iBottom * -1;
							num2 -= (float)(block.m_Footprint.m_iH - 1) / 2f;
							Vector3 localPosition = new Vector3(num, num2, -5f);
							m_SelectedBorder.transform.localPosition = localPosition;
							LevelEditorBorderElement.CreateBorderPieces(m_SelectedBorder.transform, ref block.m_Footprint, null, LevelEditorBorderElement.BorderState.Freeze);
							break;
						}
						case BaseBuildingBlock.BuildingBlockType.Complex:
						case BaseBuildingBlock.BuildingBlockType.Room:
							m_SelectedBorder = new GameObject("Border");
							m_SelectedBorder.transform.parent = m_LevelDetailsMan.m_LevelMasterGameObject.transform;
							m_SelectedBorder.transform.localPosition = new Vector3(0.5f, 0.5f, m_LastSelected_Object.transform.position.z - 5f);
							LevelEditorBorderElement.CreateBorderPiecesForRoom(m_SelectedBorder.transform, iRoom, m_LevelManager.m_CurrentLayer, null, LevelEditorBorderElement.BorderState.Freeze);
							break;
						}
					}
				}
			}
		}
		if (!m_Player.GetButton("Act"))
		{
			if (m_SelectedBorder == null)
			{
				SetEditMode(EditMode.NoBrush);
				return;
			}
			BaseBuildingBlock block2 = BuildingBlockManager.GetBlock(m_LastSelected_Block);
			if (block2 != null)
			{
				bool bShowDelete = false;
				bool bShowMove = false;
				bool bShowCopy = false;
				switch (block2.BlockType)
				{
				case BaseBuildingBlock.BuildingBlockType.Tile:
					bShowCopy = true;
					break;
				case BaseBuildingBlock.BuildingBlockType.Wall:
				case BaseBuildingBlock.BuildingBlockType.Decoration:
				case BaseBuildingBlock.BuildingBlockType.Object:
					bShowMove = true;
					bShowCopy = true;
					bShowDelete = true;
					break;
				case BaseBuildingBlock.BuildingBlockType.Complex:
				case BaseBuildingBlock.BuildingBlockType.Room:
					bShowDelete = true;
					if (!block2.IsValidForLayer(m_LevelManager.m_CurrentLayer))
					{
						BuildingBlock_Room buildingBlock_Room = block2 as BuildingBlock_Room;
						if (buildingBlock_Room != null && buildingBlock_Room.m_FlipBlock != -1)
						{
							bShowMove = true;
							bShowCopy = true;
						}
					}
					else
					{
						bShowMove = true;
						bShowCopy = true;
					}
					break;
				default:
					ResetSelectedMode(bResetMode: true);
					break;
				}
				ShowSelectedBlockMenu(bShowMove, bShowDelete, bShowCopy);
				SetEditMode(EditMode.SelectedObjectInLevel);
			}
			else
			{
				ResetSelectedMode(bResetMode: true);
			}
		}
		else if (m_Player.GetButtonUp("Exit"))
		{
			ExitSelectingObjectInLevel();
		}
		else if (m_Player.GetButtonUp("TogglePanels"))
		{
			m_UIController.ToggleAllMovablePanels();
		}
		else if (m_Player.GetButtonUp("Toggle_Grid") && m_Grid != null)
		{
			m_Grid.SetActive(!m_Grid.activeSelf);
		}
	}

	private void ExitSelectingObjectInLevel()
	{
		SetEditMode(EditMode.NoBrush);
		UpdateUI();
	}

	private void ShowSelectedBlockMenu(bool bShowMove, bool bShowDelete, bool bShowCopy)
	{
		if (m_bSelectedBlockMenuShown != bShowMove || bShowDelete || bShowCopy)
		{
			m_bSelectedBlockMenuShown = bShowMove || bShowDelete || bShowCopy;
			if (m_bSelectedBlockMenuShown)
			{
				m_UIController.ShowSelectedBlockMenu(new Vector3((float)(m_LastSelected_X_Position - 60) + 0.5f, (float)(m_LastSelected_Y_Position - 60) + 1f, -10f), bShowMove, bShowDelete, bShowCopy);
			}
			else
			{
				m_UIController.HideSelectedBlockMenu();
			}
		}
	}

	private void UpdateState_SelectedObjectInLevel()
	{
		if (m_Player.GetButtonDown("Act"))
		{
			GameObject blockObject = null;
			int iRoom = 0;
			int blockWeAreOn = GetBlockWeAreOn(m_Block_X_Position, m_Block_Y_Position, ref blockObject, ref iRoom);
			if (blockObject != m_LastSelected_Object || blockWeAreOn != m_LastSelected_Block)
			{
				ResetSelectedMode(bResetMode: false);
				SetEditMode(EditMode.SelectingObjectInLevel);
			}
			else
			{
				ResetSelectedMode(bResetMode: false);
			}
		}
		else if (m_Player.GetButtonDown("MoveMap"))
		{
			SetEditMode(EditMode.MovingCamera);
			m_LastDrawnPosition = GetRawMousePosition();
		}
		else if (m_Player.GetButtonUp("Center"))
		{
			m_MainCamera.transform.localPosition = new Vector3(0f, 0f, m_MainCamera.transform.localPosition.z);
		}
		else if (m_Player.GetButtonUp("Undo"))
		{
			UndoLast();
		}
		else if (m_Player.GetButtonUp("Redo"))
		{
			RedoLast();
		}
		else if (m_Player.GetButtonUp("Exit") || m_Player.GetButtonUp("Delete"))
		{
			ExitSelectedObjectInLevel();
			return;
		}
		ProcessPossibleCameraMove();
	}

	private void ExitSelectedObjectInLevel()
	{
		ResetSelectedMode(bResetMode: true);
		UpdateUI();
	}

	public void ExternalSelectBlock(int iBlockID)
	{
		if (m_EditMode != EditMode.NoBrush && m_EditMode != EditMode.BlockSelected && m_EditMode != EditMode.SelectedObjectInLevel && m_EditMode != EditMode.CopySelectedObjectInLevel && m_EditMode != EditMode.Zone_Editing && m_EditMode != EditMode.Zone_WaitingToCreate)
		{
			return;
		}
		if (iBlockID != -1)
		{
			if (m_CurrentBlock != -1 && BuildingBlockManager.GetInstance().IsBlockVarientOfThisBlock(m_CurrentBlock, iBlockID))
			{
				SetCurrentBlock(-1);
			}
			else if (BuildingBlockManager.GetBlock(iBlockID) != null)
			{
				m_bMovingBlock = false;
				SetCurrentBlock(iBlockID);
				SetEditMode(EditMode.BlockSelected);
			}
		}
		else
		{
			SetCurrentBlock(iBlockID);
		}
	}

	public int GetSelectedBlock()
	{
		return m_CurrentBlock;
	}

	private void UpdateState_BlockSelected()
	{
		if (m_Player.GetButtonDown("MoveMap"))
		{
			SetEditMode(EditMode.MovingCamera);
			m_LastDrawnPosition = GetRawMousePosition();
		}
		else if (m_Player.GetButtonDown("Delete"))
		{
			m_LastDrawnPosition.x = m_Block_X_Position;
			m_LastDrawnPosition.y = m_Block_Y_Position;
			SetEditMode(EditMode.Deleting);
		}
		else if (m_Player.GetButtonDown("Act"))
		{
			switch (m_DrawingMode)
			{
			case BaseBuildingBlock.BuildingBlockDrawingMode.Stamp:
				if (CanBlockBePlaced())
				{
					ResetTimer();
					m_InstructionManager.AddBlockOnce(m_CurrentBlock, (sbyte)((float)m_Block_X_Position + m_BrushOffset.x), (sbyte)((float)m_Block_Y_Position + m_BrushOffset.y), UnityEngine.Random.Range(0, 100000));
					m_InstructionManager.UpdateLevel();
					m_LastDrawnPosition.x = m_Block_X_Position;
					m_LastDrawnPosition.y = m_Block_Y_Position;
					m_bUpdateBrushPosition = true;
					m_bForceValidation = true;
					PlayAudio(AudioTypes.Placed);
					BaseBuildingBlock block = BuildingBlockManager.GetBlock(m_CurrentBlock);
					if (block == null || block.BlockType != BaseBuildingBlock.BuildingBlockType.Room)
					{
						LevelEditor_ZoneManager.Zone zoneAt = m_ZoneManager.GetZoneAt(m_Block_X_Position, m_Block_Y_Position, m_LevelManager.m_CurrentLayer);
						if (zoneAt != null && zoneAt.m_bActive)
						{
							ExternalSetCurrentZone(zoneAt.m_ID, bSelectZoneTab: false);
						}
					}
					if (IsBlockOutOfStock())
					{
						m_UIController.DisplayOutOfBlocksToolTip(m_CurrentBlock);
					}
				}
				else
				{
					if (IsBlockOutOfStock())
					{
						m_UIController.DisplayOutOfBlocksToolTip(m_CurrentBlock);
					}
					PlayAudio(AudioTypes.Error);
				}
				break;
			case BaseBuildingBlock.BuildingBlockDrawingMode.Paint:
				if (CanBlockBePlaced())
				{
					ResetTimer();
					m_InstructionManager.AddBlockOnce(m_CurrentBlock, (sbyte)((float)m_Block_X_Position + m_BrushOffset.x), (sbyte)((float)m_Block_Y_Position + m_BrushOffset.y), UnityEngine.Random.Range(0, 100000));
					m_InstructionManager.UpdateLevel();
					m_LastDrawnPosition.x = m_Block_X_Position;
					m_LastDrawnPosition.y = m_Block_Y_Position;
					m_bUpdateBrushPosition = true;
				}
				SetEditMode(EditMode.FreeDrawing);
				break;
			case BaseBuildingBlock.BuildingBlockDrawingMode.Marquee:
				m_LastDrawnPosition.x = m_Block_X_Position;
				m_LastDrawnPosition.y = m_Block_Y_Position;
				SetEditMode(EditMode.Marquee);
				break;
			case BaseBuildingBlock.BuildingBlockDrawingMode.MarqueeLine:
				m_LastDrawnPosition.x = m_Block_X_Position;
				m_LastDrawnPosition.y = m_Block_Y_Position;
				SetEditMode(EditMode.MarqueeLine);
				break;
			}
		}
		else if (m_Player.GetButtonUp("Variation"))
		{
			NextVariation(bNext: true);
		}
		else if (m_Player.GetButtonUp("SelectBlock"))
		{
			int num = m_CurrentBlock + 1;
			for (int i = 0; i < 100; i++)
			{
				if (num > 100)
				{
					num = 1;
				}
				if (BuildingBlockManager.GetBlock(num) != null)
				{
					SetCurrentBlock(num);
					break;
				}
				num++;
			}
		}
		else if (m_Player.GetButtonUp("Center"))
		{
			m_MainCamera.transform.localPosition = new Vector3(0f, 0f, m_MainCamera.transform.localPosition.z);
		}
		else if (m_Player.GetButtonUp("Undo"))
		{
			UndoLast();
		}
		else if (m_Player.GetButtonUp("Redo"))
		{
			RedoLast();
		}
		else
		{
			if (m_Player.GetButtonUp("Exit"))
			{
				ExitBlockSelected();
				return;
			}
			if (m_Player.GetButtonUp("Save"))
			{
				SaveTheLevel(bForceNew: false);
			}
		}
		ProcessPossibleCameraMove();
	}

	private void ExitBlockSelected()
	{
		SetCurrentBlock(-1);
		SetEditMode(EditMode.NoBrush);
		UpdateUI();
	}

	private void UpdateState_MovingBlock()
	{
		if (m_Player.GetButtonDown("MoveMap"))
		{
			SetEditMode(EditMode.MovingCamera);
			m_LastDrawnPosition = GetRawMousePosition();
		}
		else if (m_Player.GetButtonDown("Delete"))
		{
			m_LastDrawnPosition.x = m_Block_X_Position;
			m_LastDrawnPosition.y = m_Block_Y_Position;
			SetEditMode(EditMode.Deleting);
		}
		else if (m_Player.GetButtonDown("Act"))
		{
			if (CanBlockBePlaced())
			{
				ResetTimer();
				m_InstructionManager.AddBlockOnce(m_CurrentBlock, (sbyte)((float)m_Block_X_Position + m_BrushOffset.x), (sbyte)((float)m_Block_Y_Position + m_BrushOffset.y), UnityEngine.Random.Range(0, 100000));
				m_InstructionManager.UpdateLevel();
				m_LastDrawnPosition.x = m_Block_X_Position;
				m_LastDrawnPosition.y = m_Block_Y_Position;
				m_bUpdateBrushPosition = true;
				m_bForceValidation = true;
				PlayAudio(AudioTypes.Placed);
				m_bMovingBlock = false;
				SetCurrentBlock(-1);
				LevelEditor_ZoneManager.Zone zoneAt = m_ZoneManager.GetZoneAt(m_Block_X_Position, m_Block_Y_Position, m_LevelManager.m_CurrentLayer);
				if (zoneAt != null && zoneAt.m_bActive)
				{
					ExternalSetCurrentZone(zoneAt.m_ID, bSelectZoneTab: false);
				}
			}
			else
			{
				PlayAudio(AudioTypes.Error);
			}
		}
		else if (m_Player.GetButtonUp("Center"))
		{
			m_MainCamera.transform.localPosition = new Vector3(0f, 0f, m_MainCamera.transform.localPosition.z);
		}
		else
		{
			if (m_Player.GetButtonUp("Exit"))
			{
				ExitMovingBlock();
				return;
			}
			if (m_Player.GetButtonUp("Variation"))
			{
				NextVariation(bNext: true);
			}
			else if (m_Player.GetButtonUp("Center"))
			{
				m_MainCamera.transform.localPosition = new Vector3(0f, 0f, m_MainCamera.transform.localPosition.z);
			}
			else if (m_Player.GetButtonUp("Undo"))
			{
				UndoLast();
			}
			else if (m_Player.GetButtonUp("Redo"))
			{
				RedoLast();
			}
			else if (m_Player.GetButtonUp("Save"))
			{
				SaveTheLevel(bForceNew: false);
			}
		}
		ProcessPossibleCameraMove();
	}

	private void ExitMovingBlock()
	{
		m_bMovingBlock = false;
		SetCurrentBlock(-1);
		UpdateUI();
		SetEditMode(EditMode.NoBrush);
	}

	private void UpdateState_MovingCamera()
	{
		if (!GetInputDevice())
		{
			return;
		}
		Vector2 rawMousePosition = GetRawMousePosition();
		ResetTimer(bOnlyIfNotZero: true);
		if (!m_Player.GetButton("MoveMap"))
		{
			SetEditMode(m_PreviousEditMode);
			if (m_CameraMoved)
			{
				return;
			}
			LevelEditor_ZoneManager.Zone zoneAt = m_ZoneManager.GetZoneAt(m_Block_X_Position, m_Block_Y_Position, m_LevelManager.m_CurrentLayer);
			if (zoneAt != null)
			{
				if (object.ReferenceEquals(zoneAt, m_CurrentZone))
				{
					ExternalSetCurrentZone(-1);
				}
				else
				{
					ExternalSetCurrentZone(zoneAt.m_ID);
				}
			}
			else
			{
				ExternalSetCurrentZone(-1);
			}
		}
		else if (m_LastDrawnPosition.x != rawMousePosition.x || m_LastDrawnPosition.y != rawMousePosition.y)
		{
			float num = (float)Screen.width / (float)Screen.height;
			float num2 = m_MainCamera.orthographicSize * 2f;
			float num3 = (rawMousePosition.y - m_LastDrawnPosition.y) / (float)Screen.height * num2;
			float num4 = (rawMousePosition.x - m_LastDrawnPosition.x) / (float)Screen.width * num2 * num;
			m_LastDrawnPosition = rawMousePosition;
			Vector3 localPosition = m_MainCamera.transform.localPosition;
			localPosition.x = Mathf.Clamp(localPosition.x - num4, -60f, 60f);
			localPosition.y = Mathf.Clamp(localPosition.y - num3, -60f, 59f);
			m_MainCamera.transform.localPosition = GetCorrectedPosition(localPosition);
			m_CameraMoved = true;
		}
	}

	private void UpdateState_FreeDrawing()
	{
		if (m_Player.GetButton("Act"))
		{
			int num = Mathf.Max(Mathf.Abs(m_Block_X_Position - (int)m_LastDrawnPosition.x), Mathf.Abs(m_Block_Y_Position - (int)m_LastDrawnPosition.y));
			if (num > 0)
			{
				float num2 = (float)(m_Block_X_Position - (int)m_LastDrawnPosition.x) / (float)num;
				float num3 = (float)(m_Block_Y_Position - (int)m_LastDrawnPosition.y) / (float)num;
				float num4 = m_LastDrawnPosition.x;
				float num5 = m_LastDrawnPosition.y;
				for (int i = 0; i < num; i++)
				{
					num4 += num2;
					num5 += num3;
					m_Block_X_Position = (int)num4;
					m_Block_Y_Position = (int)num5;
					UpdateBrushPosition();
					if (CanBlockBePlaced())
					{
						ResetTimer();
						m_InstructionManager.AddBlockOnce(m_CurrentBlock, (sbyte)m_Block_X_Position, (sbyte)m_Block_Y_Position, UnityEngine.Random.Range(0, 100000));
					}
				}
				m_LastDrawnPosition.x = m_Block_X_Position;
				m_LastDrawnPosition.y = m_Block_Y_Position;
				m_InstructionManager.UpdateLevel();
				UpdateBrushPosition();
			}
		}
		else
		{
			ExitFreeDrawing();
		}
		ProcessPossibleCameraMove();
	}

	private void ExitFreeDrawing()
	{
		SetEditMode(EditMode.BlockSelected);
	}

	private void UpdateState_Marquee()
	{
		ResetTimer(bOnlyIfNotZero: true);
		if (!m_Player.GetButton("Act"))
		{
			int num = Mathf.Min((int)m_LastDrawnPosition.x, m_Block_X_Position);
			int num2 = Mathf.Min((int)m_LastDrawnPosition.y, m_Block_Y_Position);
			int num3 = Mathf.Abs(m_Block_X_Position - (int)m_LastDrawnPosition.x) + 1;
			int num4 = Mathf.Abs(m_Block_Y_Position - (int)m_LastDrawnPosition.y) + 1;
			if (num < 0)
			{
				num3 += num;
				num = 0;
			}
			else if (num + num3 > 120)
			{
				num3 = 120 - num;
			}
			if (num2 < 0)
			{
				num4 += num2;
				num2 = 0;
			}
			else if (num2 + num4 > 118)
			{
				num4 = 118 - num2;
			}
			ResetTimer();
			m_InstructionManager.AddBlockArea(m_CurrentBlock, (sbyte)num, (sbyte)num2, (sbyte)num3, (sbyte)num4, UnityEngine.Random.Range(0, 100000));
			m_InstructionManager.UpdateLevel();
			SetEditMode(EditMode.BlockSelected);
			PlayAudio(AudioTypes.Placed);
		}
		else if (m_Player.GetButtonUp("Exit"))
		{
			ExitMarquee();
		}
		ProcessPossibleCameraMove();
	}

	private void ExitMarquee()
	{
		SetEditMode(EditMode.BlockSelected);
		PlayAudio(AudioTypes.Marquee_Stop);
		UpdateUI();
	}

	private void MarkCopyArea(bool bCopyFlag, bool bScan = true)
	{
		CopyEnum copyEnum = CopyEnum.Empty;
		if (bCopyFlag)
		{
			copyEnum = CopyEnum.MarkedAll;
		}
		int num = Mathf.Min((int)m_LastDrawnPosition.x, m_Block_X_Position);
		int num2 = Mathf.Min((int)m_LastDrawnPosition.y, m_Block_Y_Position);
		int num3 = Mathf.Abs(m_Block_X_Position - (int)m_LastDrawnPosition.x) + 1;
		int num4 = Mathf.Abs(m_Block_Y_Position - (int)m_LastDrawnPosition.y) + 1;
		if (num < 0)
		{
			num3 += num;
			num = 0;
		}
		else if (num + num3 > 120)
		{
			num3 = 120 - num;
		}
		if (num2 < 0)
		{
			num4 += num2;
			num2 = 0;
		}
		else if (num2 + num4 > 118)
		{
			num4 = 118 - num2;
		}
		int num5 = 120 - num3;
		int num6 = num2 * 120 + num;
		for (int i = 0; i < num4; i++)
		{
			for (int j = 0; j < num3; j++)
			{
				m_CopyArea[num6] = bCopyFlag;
				m_CopyAreaFlags[num6++] = copyEnum;
			}
			num6 += num5;
		}
		if (bScan)
		{
			ScanForOverlappingRooms();
			ScanForOverlappingBlocks();
			CreateBorderForCopy();
		}
	}

	private void ScanForOverlappingBlocks()
	{
		BaseLevelManager.LayerDataCollection layerDataCollection = BaseLevelManager.GetInstance().m_BuildingLayers[(uint)m_LevelManager.m_CurrentLayer];
		int num = 0;
		for (int i = 0; i < 120; i++)
		{
			for (int j = 0; j < 120; j++)
			{
				if ((m_CopyAreaFlags[num] & CopyEnum.MarkedAndScannedAll) == CopyEnum.MarkedAll && (layerDataCollection.m_ObjectTileIDs[num] & BaseLevelManager.TileIDData.IDMask) != BaseLevelManager.TileIDData.IDMask && layerDataCollection.m_ObjectTileObjects[num] != null)
				{
					BaseBuildingBlock block = BuildingBlockManager.GetBlock(layerDataCollection.m_ObjectTileIDs[num]);
					if (block != null)
					{
						Vector3 vector = layerDataCollection.m_ObjectTileObjects[num].transform.localPosition - block.GetVisualRep(0).transform.localPosition;
						vector.x += block.m_Footprint.m_iLeft;
						vector.y += block.m_Footprint.m_iBottom;
						float num2 = vector.x - m_LevelManager.m_fPositionOffsetsX[(int)block.BlockType];
						float num3 = vector.y - m_LevelManager.m_fPositionOffsetsY[(int)block.BlockType];
						int num4 = (int)num2;
						int num5 = (int)num3;
						int num6 = num5 * 120 + num4;
						int num7 = 120 - block.m_Footprint.m_iW;
						int num8 = 0;
						for (int k = 0; k < block.m_Footprint.m_iH; k++)
						{
							for (int l = 0; l < block.m_Footprint.m_iW; l++)
							{
								if (block.m_Footprint.m_UsedTiles[num8++] != 0)
								{
									m_CopyArea[num6] = true;
									m_CopyAreaFlags[num6] = CopyEnum.MarkedAndScanned;
								}
								num6++;
							}
							num6 += num7;
						}
					}
				}
				num++;
			}
		}
	}

	private void ScanForOverlappingRooms()
	{
		BaseLevelManager.LayerDataCollection layerDataCollection = BaseLevelManager.GetInstance().m_BuildingLayers[(uint)m_LevelManager.m_CurrentLayer];
		int num = 0;
		for (int i = 0; i < 120; i++)
		{
			for (int j = 0; j < 120; j++)
			{
				if ((m_CopyAreaFlags[num] & CopyEnum.MarkedAndScannedRoom1) == CopyEnum.MarkedRoom1 && layerDataCollection.m_RoomIDs[num] != 0)
				{
					ScanForOverlappingRoom(layerDataCollection, layerDataCollection.m_RoomIDs[num], CopyEnum.MarkedAndScannedRoom1);
				}
				if ((m_CopyAreaFlags[num] & CopyEnum.MarkedAndScannedRoom2) == CopyEnum.MarkedRoom2 && layerDataCollection.m_RoomIDs[num + 14400] != 0)
				{
					ScanForOverlappingRoom(layerDataCollection, layerDataCollection.m_RoomIDs[num + 14400], CopyEnum.MarkedAndScannedRoom2);
				}
				if ((m_CopyAreaFlags[num] & CopyEnum.MarkedAndScannedRoom3) == CopyEnum.MarkedRoom3 && layerDataCollection.m_RoomIDs[num + 28800] != 0)
				{
					ScanForOverlappingRoom(layerDataCollection, layerDataCollection.m_RoomIDs[num + 28800], CopyEnum.MarkedAndScannedRoom3);
				}
				if ((m_CopyAreaFlags[num] & CopyEnum.MarkedAndScannedRoom4) == CopyEnum.MarkedRoom4 && layerDataCollection.m_RoomIDs[num + 43200] != 0)
				{
					ScanForOverlappingRoom(layerDataCollection, layerDataCollection.m_RoomIDs[num + 43200], CopyEnum.MarkedAndScannedRoom4);
				}
				num++;
			}
		}
	}

	private void ScanForOverlappingRoom(BaseLevelManager.LayerDataCollection layer, int iRoomID, CopyEnum maskToSet)
	{
		for (int i = 0; i < 14400; i++)
		{
			if (layer.m_RoomIDs[i] == iRoomID || layer.m_RoomIDs[i + 14400] == iRoomID || layer.m_RoomIDs[i + 28800] == iRoomID || layer.m_RoomIDs[i + 43200] == iRoomID)
			{
				CopyEnum[] copyAreaFlags;
				int num;
				(copyAreaFlags = m_CopyAreaFlags)[num = i] = copyAreaFlags[num] | (maskToSet | CopyEnum.Scanned);
				m_CopyArea[i] = true;
			}
		}
	}

	private void UpdateState_Copy()
	{
		if (!m_Player.GetButton("Act"))
		{
			if ((int)m_LastDrawnPosition.x == m_Block_X_Position && (int)m_LastDrawnPosition.y == m_Block_Y_Position)
			{
				SelectBlockAtPosition(m_Block_X_Position, m_Block_Y_Position);
				PlayAudio(AudioTypes.Placed);
				return;
			}
			MarkCopyArea(bCopyFlag: true);
			SetEditMode(EditMode.CopySelectedObjectInLevel);
			PlayAudio(AudioTypes.Placed);
		}
		ProcessPossibleCameraMove();
	}

	private void UpdateState_Zone_WaitingToCreate()
	{
		if (m_ZoneToCreate == ZoneDetailsManager.ZoneTypes.INVALID)
		{
			SetEditMode(EditMode.NoBrush);
			return;
		}
		if (m_Player.GetButtonDown("MoveMap"))
		{
			SetEditMode(EditMode.MovingCamera);
			m_LastDrawnPosition = GetRawMousePosition();
		}
		else
		{
			if (m_Player.GetButtonDown("Delete"))
			{
				ExitZoneWaitingToCreate();
				return;
			}
			if (m_Player.GetButtonDown("Act"))
			{
				if (m_LevelManager.m_CurrentLayer == BaseLevelManager.LevelLayers.FirstFloor || m_LevelManager.m_CurrentLayer == BaseLevelManager.LevelLayers.GroundFloor)
				{
					m_LastDrawnPosition.x = m_Block_X_Position;
					m_LastDrawnPosition.y = m_Block_Y_Position;
					SetEditMode(EditMode.Zone_Creating);
				}
				else
				{
					PlayAudio(AudioTypes.Error);
				}
			}
			else if (m_Player.GetButtonUp("Center"))
			{
				m_MainCamera.transform.localPosition = new Vector3(0f, 0f, m_MainCamera.transform.localPosition.z);
			}
			else if (m_Player.GetButtonUp("Undo"))
			{
				UndoLast();
			}
			else if (m_Player.GetButtonUp("Redo"))
			{
				RedoLast();
			}
			else
			{
				if (m_Player.GetButtonUp("Exit"))
				{
					ExitZoneWaitingToCreate();
					return;
				}
				if (m_Player.GetButtonUp("Save"))
				{
					SaveTheLevel(bForceNew: false);
				}
			}
		}
		ProcessPossibleCameraMove();
		FloatingZoneIcon instance = FloatingZoneIcon.GetInstance();
		if (instance != null)
		{
			instance.SetTheLayer(m_LevelManager.m_CurrentLayer);
			instance.SetTheTilePosition(m_Block_X_Position, m_Block_Y_Position);
		}
	}

	private void ExitZoneWaitingToCreate()
	{
		SetEditMode(EditMode.NoBrush);
	}

	private void UpdateState_Zone_Creating()
	{
		ResetTimer(bOnlyIfNotZero: true);
		if (!m_Player.GetButton("Act"))
		{
			int iOrriginalStartX = Mathf.Min((int)m_LastDrawnPosition.x, m_Block_X_Position);
			int iOrriginalStartY = Mathf.Min((int)m_LastDrawnPosition.y, m_Block_Y_Position);
			int iOrriginalWidth = Mathf.Abs(m_Block_X_Position - (int)m_LastDrawnPosition.x) + 1;
			int iOrriginalHeight = Mathf.Abs(m_Block_Y_Position - (int)m_LastDrawnPosition.y) + 1;
			if (iOrriginalStartX < 0)
			{
				iOrriginalWidth += iOrriginalStartX;
				iOrriginalStartX = 0;
			}
			else if (iOrriginalStartX + iOrriginalWidth > 120)
			{
				iOrriginalWidth = 120 - iOrriginalStartX;
			}
			if (iOrriginalStartY < 0)
			{
				iOrriginalHeight += iOrriginalStartY;
				iOrriginalStartY = 0;
			}
			else if (iOrriginalStartY + iOrriginalHeight > 118)
			{
				iOrriginalHeight = 118 - iOrriginalStartY;
			}
			if (iOrriginalWidth > 1 && iOrriginalHeight > 1)
			{
				byte[] zonePrint = new byte[0];
				if (IsZoneAreaValid(ref iOrriginalStartX, ref iOrriginalStartY, ref iOrriginalWidth, ref iOrriginalHeight, m_LevelManager.m_CurrentLayer, ref zonePrint))
				{
					ResetTimer();
					m_InstructionManager.CreateZone(m_ZoneToCreate, (sbyte)iOrriginalStartX, (sbyte)iOrriginalStartY, (sbyte)iOrriginalWidth, (sbyte)iOrriginalHeight, zonePrint);
					SetCurrentZone(m_ZoneManager.GetLastCreatedZone());
					if (m_CurrentZone != null)
					{
						PlayAudio(AudioTypes.Placed);
						SetEditMode(EditMode.Zone_Editing);
						return;
					}
				}
			}
			SetEditMode(EditMode.Zone_WaitingToCreate);
			PlayAudio(AudioTypes.Error);
		}
		else if (m_Player.GetButtonUp("Exit"))
		{
			ExitZoneCreating();
		}
		ProcessPossibleCameraMove();
	}

	private void ExitZoneCreating()
	{
		SetEditMode(EditMode.BlockSelected);
		PlayAudio(AudioTypes.Marquee_Stop);
		UpdateUI();
	}

	private void UpdateState_Zone_Editing()
	{
		if (m_CurrentZone == null || !m_CurrentZone.m_bActive)
		{
			SetEditMode(EditMode.NoBrush);
			return;
		}
		ResetTimer(bOnlyIfNotZero: true);
		if (m_Player.GetButtonDown("MoveMap"))
		{
			SetEditMode(EditMode.MovingCamera);
			m_LastDrawnPosition = GetRawMousePosition();
		}
		else if (m_Player.GetButtonDown("Delete"))
		{
			m_LastDrawnPosition.x = m_Block_X_Position;
			m_LastDrawnPosition.y = m_Block_Y_Position;
			SetEditMode(EditMode.Zone_Deleting);
		}
		else if (m_Player.GetButtonDown("Act"))
		{
			m_LastDrawnPosition.x = m_Block_X_Position;
			m_LastDrawnPosition.y = m_Block_Y_Position;
			SetEditMode(EditMode.Zone_Adding);
		}
		else if (m_Player.GetButtonUp("Center"))
		{
			m_MainCamera.transform.localPosition = new Vector3(0f, 0f, m_MainCamera.transform.localPosition.z);
		}
		else if (m_Player.GetButtonUp("Undo"))
		{
			UndoLast();
			if (m_CurrentZone == null || !m_CurrentZone.m_bActive)
			{
				SetEditMode(EditMode.NoBrush);
			}
		}
		else if (m_Player.GetButtonUp("Redo"))
		{
			RedoLast();
		}
		else if (m_Player.GetButtonUp("Exit"))
		{
			ExitZoneEditing();
			return;
		}
		ProcessPossibleCameraMove(bAllowLayerChange: false);
	}

	private void ExitZoneEditing()
	{
		SetEditMode(EditMode.NoBrush);
		UpdateUI();
	}

	private void UpdateState_Zone_Adding()
	{
		ResetTimer(bOnlyIfNotZero: true);
		if (!m_Player.GetButton("Act"))
		{
			int num = Mathf.Min((int)m_LastDrawnPosition.x, m_Block_X_Position);
			int num2 = Mathf.Min((int)m_LastDrawnPosition.y, m_Block_Y_Position);
			int num3 = Mathf.Abs(m_Block_X_Position - (int)m_LastDrawnPosition.x) + 1;
			int num4 = Mathf.Abs(m_Block_Y_Position - (int)m_LastDrawnPosition.y) + 1;
			if (num < 0)
			{
				num3 += num;
				num = 0;
			}
			if (num + num3 > 120)
			{
				num3 = 120 - num;
			}
			if (num2 < 0)
			{
				num4 += num2;
				num2 = 0;
			}
			if (num2 + num4 > 118)
			{
				num4 = 118 - num2;
			}
			if (num3 >= 1 && num4 >= 1)
			{
				int num5 = Mathf.Min(num, m_CurrentZone.m_Left);
				int num6 = Mathf.Min(num2, m_CurrentZone.m_Bottom);
				int num7 = Mathf.Max(num + num3, m_CurrentZone.m_Left + m_CurrentZone.m_Width) - num5;
				int num8 = Mathf.Max(num2 + num4, m_CurrentZone.m_Bottom + m_CurrentZone.m_Height) - num6;
				int num9 = num7 * num8;
				int iD = m_CurrentZone.m_ID;
				int num10 = num6 * 120 + num5;
				int num11 = 120 - num7;
				ScanBits[] map = new ScanBits[num9];
				int[] map2 = m_ZoneManager.GetZoneMap(m_CurrentZone.m_Layer).m_Map;
				int num12 = 0;
				for (int i = 0; i < num8; i++)
				{
					for (int j = 0; j < num7; j++)
					{
						if (map2[num10++] == iD)
						{
							map[num12++] = ScanBits.OCCUPIED;
						}
						else
						{
							map[num12++] = ScanBits.EMPTY;
						}
					}
					num10 += num11;
				}
				BaseLevelManager.TileProperty[] tileProperties = m_LevelManager.m_BuildingLayers[(uint)m_CurrentZone.m_Layer].m_TileProperties;
				int num13 = num2 - num6;
				int num14 = num - num5;
				int num15 = num13 + num4;
				int num16 = num14 + num3;
				int num17 = num13 * num7 + num14;
				int num18 = num7 - num3;
				num10 = (num6 + num13) * 120 + num5 + num14;
				num11 = 120 - num3;
				bool flag = false;
				bool flag2 = false;
				bool flag3 = false;
				for (int k = num13; k < num15; k++)
				{
					for (int l = num14; l < num16; l++)
					{
						if (map[num17] == ScanBits.EMPTY)
						{
							if (map2[num10] != -1)
							{
								flag3 = true;
							}
							else if ((tileProperties[num10] & BaseLevelManager.TileProperty.TileMask) != BaseLevelManager.TileProperty.TileMask)
							{
								flag2 = true;
							}
							else
							{
								map[num17] = ScanBits.OCCUPIED | ScanBits.ADDED;
								flag = true;
							}
						}
						num17++;
						num10++;
					}
					num10 += num11;
					num17 += num18;
				}
				if (!flag)
				{
					if (flag3)
					{
						m_UIController.SetBrushError(BaseLevelManager.BrushError.eZoneOverAnotherZone, m_ZoneErrorExpireIn, bClearOnExpiry: true);
					}
					else if (flag2)
					{
						m_UIController.SetBrushError(BaseLevelManager.BrushError.eZoneOverEmptySpace, m_ZoneErrorExpireIn, bClearOnExpiry: true);
					}
					num10 = num2 * 120 + num;
					num11 = 120 - num3;
					map = new ScanBits[num3 * num4];
					num12 = 0;
					for (int m = 0; m < num4; m++)
					{
						for (int n = 0; n < num3; n++)
						{
							if (map2[num10++] == iD)
							{
								map[num12++] = ScanBits.OCCUPIED;
							}
							else
							{
								map[num12++] = ScanBits.EMPTY;
							}
						}
						num10 += num11;
					}
					LevelEditor_FlashingMarquee.CreateFlashingMarquee(m_ZoneManager.m_FlashingErrorMarqueePrefab, map, num3, num, num2, m_CurrentZone.m_Layer, ScanBits.EMPTY);
					PlayAudio(AudioTypes.Error);
					SetEditMode(EditMode.Zone_Editing);
					return;
				}
				int num19 = -1;
				for (int num20 = 0; num20 < num9; num20++)
				{
					if (map[num20] != ScanBits.EMPTY)
					{
						num19 = num20;
						break;
					}
				}
				FloodSetMap(ref map, num7, num19 % num7, num19 / num7, ScanBits.OCCUPIED, ScanBits.SCANISLAND);
				for (int num21 = 0; num21 < num9; num21++)
				{
					if ((map[num21] & ScanBits.OCCUPIED) == ScanBits.OCCUPIED && (map[num21] & ScanBits.SCANISLAND) != ScanBits.SCANISLAND)
					{
						m_UIController.SetBrushError(BaseLevelManager.BrushError.eZoneNoIslands, m_ZoneErrorExpireIn, bClearOnExpiry: true);
						LevelEditor_FlashingMarquee.CreateFlashingMarquee(m_ZoneManager.m_FlashingErrorMarqueePrefab, map, num7, num5, num6, m_CurrentZone.m_Layer, ScanBits.OCCUPIED | ScanBits.ADDED);
						PlayAudio(AudioTypes.Error);
						SetEditMode(EditMode.Zone_Editing);
						return;
					}
				}
				if (AreThereAnyHoles(ref map, num7))
				{
					LevelEditor_FlashingMarquee.CreateFlashingMarquee(m_ZoneManager.m_SolidErrorMarqueePrefab, map, num7, num5, num6, m_CurrentZone.m_Layer, ScanBits.OCCUPIED | ScanBits.SCANISLAND | ScanBits.ADDED);
					LevelEditor_FlashingMarquee.CreateFlashingMarquee(m_ZoneManager.m_FlashingErrorMarqueePrefab, map, num7, num5, num6, m_CurrentZone.m_Layer, ScanBits.EMPTY);
					m_UIController.SetBrushError(BaseLevelManager.BrushError.eZoneNoDoughnuts, m_ZoneErrorExpireIn, bClearOnExpiry: true);
					PlayAudio(AudioTypes.Error);
					SetEditMode(EditMode.Zone_Editing);
				}
				else
				{
					ResetTimer();
					m_InstructionManager.AddToZone(iD, (sbyte)num, (sbyte)num2, (sbyte)num3, (sbyte)num4);
					PlayAudio(AudioTypes.Placed);
					SetEditMode(EditMode.Zone_Editing);
				}
				return;
			}
			SetEditMode(EditMode.Zone_Editing);
			PlayAudio(AudioTypes.Error);
		}
		else if (m_Player.GetButtonUp("Exit"))
		{
			ExitZoneAdding();
		}
		ProcessPossibleCameraMove();
	}

	private void ExitZoneAdding()
	{
		SetEditMode(EditMode.Zone_Editing);
		PlayAudio(AudioTypes.Marquee_Stop);
		UpdateUI();
	}

	private void UpdateState_Zone_Deleting()
	{
		ResetTimer(bOnlyIfNotZero: true);
		if (!m_Player.GetButton("Delete"))
		{
			int num = Mathf.Min((int)m_LastDrawnPosition.x, m_Block_X_Position);
			int num2 = Mathf.Min((int)m_LastDrawnPosition.y, m_Block_Y_Position);
			int num3 = Mathf.Abs(m_Block_X_Position - (int)m_LastDrawnPosition.x) + 1;
			int num4 = Mathf.Abs(m_Block_Y_Position - (int)m_LastDrawnPosition.y) + 1;
			if (num < m_CurrentZone.m_Left)
			{
				num3 += num - m_CurrentZone.m_Left;
				num = m_CurrentZone.m_Left;
			}
			if (num + num3 > m_CurrentZone.m_Left + m_CurrentZone.m_Width)
			{
				num3 = m_CurrentZone.m_Left + m_CurrentZone.m_Width - num;
			}
			if (num2 < m_CurrentZone.m_Bottom)
			{
				num4 += num2 - m_CurrentZone.m_Bottom;
				num2 = m_CurrentZone.m_Bottom;
			}
			if (num2 + num4 > m_CurrentZone.m_Bottom + m_CurrentZone.m_Height)
			{
				num4 = m_CurrentZone.m_Bottom + m_CurrentZone.m_Height - num2;
			}
			if (num3 >= 1 && num4 >= 1)
			{
				int iD = m_CurrentZone.m_ID;
				if (!m_ZoneManager.IsZoneWithinArea(iD, num, num2, num3, num4, m_CurrentZone.m_Layer))
				{
					PlayAudio(AudioTypes.Error);
					SetEditMode(EditMode.Zone_Editing);
					return;
				}
				if (num == m_CurrentZone.m_Left && num2 == m_CurrentZone.m_Bottom && num3 == m_CurrentZone.m_Width && num4 == m_CurrentZone.m_Height)
				{
					ResetTimer();
					m_InstructionManager.DeleteZone(m_CurrentZone);
					SetCurrentZone(null);
					PlayAudio(AudioTypes.Deleted);
					SetEditMode(EditMode.Zone_Editing);
					return;
				}
				int num5 = m_CurrentZone.m_Width * m_CurrentZone.m_Height;
				ScanBits[] map = new ScanBits[num5];
				bool[] array = new bool[num5];
				int num6 = 0;
				byte b = 0;
				int num7 = 0;
				for (int i = 0; i < m_CurrentZone.m_Height; i++)
				{
					for (int j = 0; j < m_CurrentZone.m_Width; j++)
					{
						if ((m_CurrentZone.m_ZonePrint[num6] & (1 << (int)b)) != 0)
						{
							map[num7++] = ScanBits.OCCUPIED;
						}
						else
						{
							map[num7++] = ScanBits.EMPTY;
						}
						if (++b == 8)
						{
							b = 0;
							num6++;
						}
					}
				}
				int num8 = num - m_CurrentZone.m_Left;
				int num9 = num2 - m_CurrentZone.m_Bottom;
				int num10 = m_CurrentZone.m_Width - num3;
				num7 = num9 * m_CurrentZone.m_Width + num8;
				for (int k = 0; k < num4; k++)
				{
					for (int l = 0; l < num3; l++)
					{
						if (map[num7] != ScanBits.EMPTY)
						{
							array[num7] = true;
						}
						map[num7++] = ScanBits.EMPTY;
					}
					num7 += num10;
				}
				int num11 = -1;
				for (int m = 0; m < num5; m++)
				{
					if (map[m] == ScanBits.OCCUPIED)
					{
						num11 = m;
						break;
					}
				}
				if (num11 == -1)
				{
					PlayAudio(AudioTypes.Error);
					SetEditMode(EditMode.Zone_Editing);
					return;
				}
				FloodSetMap(ref map, m_CurrentZone.m_Width, num11 % m_CurrentZone.m_Width, num11 / m_CurrentZone.m_Width, ScanBits.OCCUPIED, ScanBits.SCANISLAND);
				for (int n = 0; n < num5; n++)
				{
					if (map[n] == ScanBits.OCCUPIED)
					{
						LevelEditor_FlashingMarquee.CreateFlashingMarquee(m_ZoneManager.m_FlashingErrorMarqueePrefab, array, m_CurrentZone.m_Width, m_CurrentZone.m_Left, m_CurrentZone.m_Bottom, m_CurrentZone.m_Layer);
						m_UIController.SetBrushError(BaseLevelManager.BrushError.eZoneNoIslands, m_ZoneErrorExpireIn, bClearOnExpiry: true);
						PlayAudio(AudioTypes.Error);
						SetEditMode(EditMode.Zone_Editing);
						return;
					}
				}
				if (AreThereAnyHoles(ref map, m_CurrentZone.m_Width))
				{
					LevelEditor_FlashingMarquee.CreateFlashingMarquee(m_ZoneManager.m_FlashingErrorMarqueePrefab, array, m_CurrentZone.m_Width, m_CurrentZone.m_Left, m_CurrentZone.m_Bottom, m_CurrentZone.m_Layer);
					m_UIController.SetBrushError(BaseLevelManager.BrushError.eZoneNoDoughnuts, m_ZoneErrorExpireIn, bClearOnExpiry: true);
					PlayAudio(AudioTypes.Error);
					SetEditMode(EditMode.Zone_Editing);
				}
				else
				{
					m_InstructionManager.SubtractFromZone(iD, (sbyte)num, (sbyte)num2, (sbyte)num3, (sbyte)num4);
					PlayAudio(AudioTypes.Placed);
					SetEditMode(EditMode.Zone_Editing);
				}
				return;
			}
			SetEditMode(EditMode.Zone_Editing);
			PlayAudio(AudioTypes.Error);
		}
		else if (m_Player.GetButtonUp("Exit"))
		{
			ExitZoneDeleting();
		}
		ProcessPossibleCameraMove();
	}

	private void ExitZoneDeleting()
	{
		SetEditMode(EditMode.Zone_Editing);
		PlayAudio(AudioTypes.Marquee_Stop);
		UpdateUI();
	}

	private void FloodSetMap(ref ScanBits[] map, int iMapWidth, int iStartX, int iStartY, ScanBits eLookingFor, ScanBits eSetTo)
	{
		int num = map.Length / iMapWidth;
		if (num * iMapWidth != map.Length || iStartX >= iMapWidth || iStartX < 0 || iStartY >= num || iStartY < 0)
		{
			return;
		}
		int num2 = iStartY * iMapWidth + iStartX;
		if ((map[num2] & eLookingFor) != eLookingFor || (map[num2] & eSetTo) == eSetTo)
		{
			return;
		}
		int num3;
		ScanBits[] array;
		(array = map)[num3 = num2] = array[num3] | eSetTo;
		int num4 = 200;
		int[] array2 = new int[num4];
		array2[0] = num2;
		int num5 = 0;
		int num6 = 1;
		int num7 = 0;
		while (num5 != num6)
		{
			num2 = array2[num5++];
			if (num5 >= num4)
			{
				num5 = 0;
			}
			int num8 = num2 % iMapWidth;
			int num9 = num2 / iMapWidth;
			num7 = num2 - iMapWidth;
			if (num9 > 0 && (map[num7] & eLookingFor) == eLookingFor && (map[num7] & eSetTo) != eSetTo)
			{
				int num10;
				(array = map)[num10 = num7] = array[num10] | eSetTo;
				array2[num6++] = num7;
				if (num6 >= num4)
				{
					num6 = 0;
				}
			}
			num7 = num2 - 1;
			if (num8 > 0 && (map[num7] & eLookingFor) == eLookingFor && (map[num7] & eSetTo) != eSetTo)
			{
				int num11;
				(array = map)[num11 = num7] = array[num11] | eSetTo;
				array2[num6++] = num7;
				if (num6 >= num4)
				{
					num6 = 0;
				}
			}
			num7 = num2 + iMapWidth;
			if (num9 + 1 < num && (map[num7] & eLookingFor) == eLookingFor && (map[num7] & eSetTo) != eSetTo)
			{
				int num12;
				(array = map)[num12 = num7] = array[num12] | eSetTo;
				array2[num6++] = num7;
				if (num6 >= num4)
				{
					num6 = 0;
				}
			}
			num7 = num2 + 1;
			if (num8 + 1 < iMapWidth && (map[num7] & eLookingFor) == eLookingFor && (map[num7] & eSetTo) != eSetTo)
			{
				int num13;
				(array = map)[num13 = num7] = array[num13] | eSetTo;
				array2[num6++] = num7;
				if (num6 >= num4)
				{
					num6 = 0;
				}
			}
		}
	}

	private bool AreThereAnyHoles(ref ScanBits[] map, int iMapWidth)
	{
		int num = map.Length / iMapWidth;
		int num2 = num * iMapWidth;
		if (num2 != map.Length)
		{
			return false;
		}
		int num3 = 0;
		int num4 = 0;
		int i;
		for (i = 0; i < 2; i++)
		{
			for (num4 = 1; num4 < iMapWidth - 1; num4++)
			{
				FloodSetMap(ref map, iMapWidth, num4, num3, ScanBits.EMPTY, ScanBits.SCANHOLE);
			}
			num3 += num - 1;
		}
		i = 0;
		num4 = 0;
		for (; i < 2; i++)
		{
			for (num3 = 0; num3 < num; num3++)
			{
				FloodSetMap(ref map, iMapWidth, num4, num3, ScanBits.EMPTY, ScanBits.SCANHOLE);
			}
			num4 += iMapWidth - 1;
		}
		for (int j = 0; j < num2; j++)
		{
			if (map[j] == ScanBits.EMPTY)
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateState_Zone_Selected()
	{
		if (m_CurrentZone == null || !m_CurrentZone.m_bActive)
		{
			SetEditMode(EditMode.NoBrush);
			return;
		}
		ResetTimer(bOnlyIfNotZero: true);
		if (m_Player.GetButtonDown("MoveMap"))
		{
			SetEditMode(EditMode.MovingCamera);
			m_LastDrawnPosition = GetRawMousePosition();
		}
		else if (m_Player.GetButtonUp("Center"))
		{
			m_MainCamera.transform.localPosition = new Vector3(0f, 0f, m_MainCamera.transform.localPosition.z);
		}
		else if (m_Player.GetButtonUp("Undo"))
		{
			UndoLast();
			if (m_CurrentZone == null || !m_CurrentZone.m_bActive)
			{
				SetEditMode(EditMode.NoBrush);
			}
		}
		else if (m_Player.GetButtonUp("Redo"))
		{
			RedoLast();
			if (m_CurrentZone == null || !m_CurrentZone.m_bActive)
			{
				SetEditMode(EditMode.NoBrush);
			}
		}
		else if (m_Player.GetButtonUp("Exit"))
		{
			ExitZone_Selected();
			return;
		}
		ProcessPossibleCameraMove();
	}

	private void ExitZone_Selected()
	{
		SetEditMode(EditMode.NoBrush);
		UpdateUI();
	}

	private void SelectBlockAtPosition(int iX, int iY)
	{
		GameObject blockObject = null;
		int iRoom = 0;
		int blockWeAreOn = GetBlockWeAreOn(iX, iY, ref blockObject, ref iRoom, bCareAboutLayer: false);
		if (blockObject != m_LastSelected_Object || blockWeAreOn != m_LastSelected_Block)
		{
			if (m_SelectedBorder != null)
			{
				UnityEngine.Object.Destroy(m_SelectedBorder);
				m_SelectedBorder = null;
			}
			m_LastSelected_Object = blockObject;
			m_LastSelected_Block = blockWeAreOn;
			if (m_LastSelected_Block != -1)
			{
				BaseBuildingBlock block = BuildingBlockManager.GetBlock(m_LastSelected_Block);
				if (block != null)
				{
					switch (block.BlockType)
					{
					default:
						return;
					case BaseBuildingBlock.BuildingBlockType.Tile:
					case BaseBuildingBlock.BuildingBlockType.Wall:
					case BaseBuildingBlock.BuildingBlockType.Decoration:
					case BaseBuildingBlock.BuildingBlockType.Object:
					{
						m_SelectedBorder = new GameObject("Border");
						m_SelectedBorder.transform.SetParent(m_LastSelected_Object.transform);
						float num = block.m_Footprint.m_iLeft * -1;
						num -= (float)(block.m_Footprint.m_iW - 1) / 2f;
						float num2 = block.m_Footprint.m_iBottom * -1;
						num2 -= (float)(block.m_Footprint.m_iH - 1) / 2f;
						Vector3 localPosition = new Vector3(num, num2, -5f);
						m_SelectedBorder.transform.localPosition = localPosition;
						LevelEditorBorderElement.CreateBorderPieces(m_SelectedBorder.transform, ref block.m_Footprint, null);
						break;
					}
					case BaseBuildingBlock.BuildingBlockType.Complex:
					case BaseBuildingBlock.BuildingBlockType.Room:
						m_SelectedBorder = new GameObject("Border");
						m_SelectedBorder.transform.SetParent(m_LevelDetailsMan.m_LevelMasterGameObject.transform);
						m_SelectedBorder.transform.localPosition = new Vector3(0.5f, 0.5f, m_LastSelected_Object.transform.position.z - 5f);
						LevelEditorBorderElement.CreateBorderPiecesForRoom(m_SelectedBorder.transform, iRoom, m_LevelManager.m_CurrentLayer, null);
						break;
					}
				}
			}
		}
		if (m_SelectedBorder == null)
		{
			SetEditMode(EditMode.NoBrush);
			return;
		}
		BaseBuildingBlock block2 = BuildingBlockManager.GetBlock(m_LastSelected_Block);
		if (block2 != null)
		{
			bool bShowDelete = false;
			bool bShowMove = false;
			bool bShowCopy = false;
			switch (block2.BlockType)
			{
			case BaseBuildingBlock.BuildingBlockType.Tile:
				bShowCopy = true;
				break;
			case BaseBuildingBlock.BuildingBlockType.Wall:
			case BaseBuildingBlock.BuildingBlockType.Decoration:
			case BaseBuildingBlock.BuildingBlockType.Object:
				bShowMove = true;
				bShowCopy = true;
				bShowDelete = true;
				break;
			case BaseBuildingBlock.BuildingBlockType.Complex:
			case BaseBuildingBlock.BuildingBlockType.Room:
				bShowDelete = true;
				if (!block2.IsValidForLayer(m_LevelManager.m_CurrentLayer))
				{
					BuildingBlock_Room buildingBlock_Room = block2 as BuildingBlock_Room;
					if (buildingBlock_Room != null && buildingBlock_Room.m_FlipBlock != -1)
					{
						bShowMove = true;
						bShowCopy = true;
					}
				}
				else
				{
					bShowMove = true;
					bShowCopy = true;
				}
				break;
			default:
				ResetSelectedMode(bResetMode: true);
				break;
			}
			ShowSelectedBlockMenu(bShowMove, bShowDelete, bShowCopy);
			SetEditMode(EditMode.SelectedObjectInLevel);
			MarkCopyArea(bCopyFlag: true, bScan: false);
		}
		else
		{
			ResetSelectedMode(bResetMode: true);
		}
	}

	private void UpdateState_Copy_Add()
	{
		if (!m_Player.GetButton("Act"))
		{
			MarkCopyArea(bCopyFlag: true);
			SetEditMode(EditMode.CopySelectedObjectInLevel);
			PlayAudio(AudioTypes.Placed);
		}
		ProcessPossibleCameraMove();
	}

	private void UpdateState_Copy_Delete()
	{
		if (!m_Player.GetButton("Delete"))
		{
			MarkCopyArea(bCopyFlag: false);
			SetEditMode(EditMode.CopySelectedObjectInLevel);
			PlayAudio(AudioTypes.Placed);
		}
		ProcessPossibleCameraMove();
	}

	private void UpdateState_CopySelectedObjectInLevel()
	{
		if (m_Player.GetButtonDown("Act"))
		{
			ResetSelectedMode(bResetMode: false);
			SetEditMode(EditMode.SelectingObjectInLevel);
		}
		else
		{
			if (m_Player.GetButtonUp("Exit") || m_Player.GetButtonUp("Delete"))
			{
				ResetSelectedMode(bResetMode: true);
				UpdateUI();
				return;
			}
			UpdateState_CopySelectedObject();
		}
		ProcessPossibleCameraMove();
	}

	private void UpdateState_CopySelectedObjectInLevel_Edit()
	{
		if (!m_Player.GetButton("Copy_Add"))
		{
			SetEditMode(EditMode.CopySelectedObjectInLevel);
		}
		else if (m_Player.GetButtonDown("Copy_Delete"))
		{
			StartCopyDelete();
		}
		else if (m_Player.GetButtonDown("Act"))
		{
			StartCopyAdd();
		}
		else
		{
			if (m_Player.GetButtonUp("Exit"))
			{
				ResetSelectedMode(bResetMode: true);
				UpdateUI();
				return;
			}
			UpdateState_CopySelectedObject();
		}
		ProcessPossibleCameraMove();
	}

	private void UpdateState_CopySelectedObject()
	{
		if (m_Player.GetButtonDown("MoveMap"))
		{
			SetEditMode(EditMode.MovingCamera);
			m_LastDrawnPosition = GetRawMousePosition();
		}
		else if (m_Player.GetButtonUp("Center"))
		{
			m_MainCamera.transform.localPosition = new Vector3(0f, 0f, m_MainCamera.transform.localPosition.z);
		}
		else if (m_Player.GetButtonUp("Undo"))
		{
			UndoLast();
		}
		else if (m_Player.GetButtonUp("Redo"))
		{
			RedoLast();
		}
		else if (m_Player.GetButtonUp("Exit"))
		{
			ResetSelectedMode(bResetMode: true);
			UpdateUI();
			return;
		}
		ProcessPossibleCameraMove();
	}

	private void UpdateState_MarqueeLine()
	{
		ResetTimer(bOnlyIfNotZero: true);
		if (!m_Player.GetButton("Act"))
		{
			int num = Mathf.Min((int)m_LastDrawnPosition.x, m_Block_X_Position);
			int num2 = Mathf.Min((int)m_LastDrawnPosition.y, m_Block_Y_Position);
			int num3 = Mathf.Abs(m_Block_X_Position - (int)m_LastDrawnPosition.x) + 1;
			int num4 = Mathf.Abs(m_Block_Y_Position - (int)m_LastDrawnPosition.y) + 1;
			if (m_EditMode == EditMode.MarqueeLine)
			{
				if (num3 <= num4)
				{
					num3 = 1;
					num = (int)m_LastDrawnPosition.x;
				}
				else
				{
					num4 = 1;
					num2 = (int)m_LastDrawnPosition.y;
				}
			}
			if (num < 0)
			{
				num3 += num;
				num = 0;
			}
			else if (num + num3 > 120)
			{
				num3 = 120 - num;
			}
			if (num2 < 0)
			{
				num4 += num2;
				num2 = 0;
			}
			else if (num2 + num4 > 118)
			{
				num4 = 120 - num2 - 2;
			}
			ResetTimer();
			m_InstructionManager.AddBlockArea(m_CurrentBlock, (sbyte)num, (sbyte)num2, (sbyte)num3, (sbyte)num4, UnityEngine.Random.Range(0, 100000));
			m_InstructionManager.UpdateLevel();
			SetEditMode(EditMode.BlockSelected);
			PlayAudio(AudioTypes.Placed);
		}
		else if (m_Player.GetButtonUp("Exit"))
		{
			ExitMarqueeLine();
		}
		ProcessPossibleCameraMove();
	}

	private void ExitMarqueeLine()
	{
		SetEditMode(EditMode.BlockSelected);
		PlayAudio(AudioTypes.Marquee_Stop);
		UpdateUI();
	}

	private void UpdateState_Deleting()
	{
		ResetTimer(bOnlyIfNotZero: true);
		if (!m_Player.GetButton("Delete"))
		{
			int num = Mathf.Min((int)m_LastDrawnPosition.x, m_Block_X_Position);
			int num2 = Mathf.Min((int)m_LastDrawnPosition.y, m_Block_Y_Position);
			int num3 = Mathf.Abs(m_Block_X_Position - (int)m_LastDrawnPosition.x) + 1;
			int num4 = Mathf.Abs(m_Block_Y_Position - (int)m_LastDrawnPosition.y) + 1;
			if (num < 0)
			{
				num3 += num;
				num = 0;
			}
			else if (num + num3 > 120)
			{
				num3 = 120 - num;
			}
			if (num2 < 0)
			{
				num4 += num2;
				num2 = 0;
			}
			else if (num2 + num4 > 118)
			{
				num4 = 120 - num2 - 2;
			}
			ResetTimer();
			bool flag = m_InstructionManager.DeleteArea((sbyte)num, (sbyte)num2, num3, num4);
			m_InstructionManager.UpdateLevel();
			m_HighlightManager.RequestRescan();
			if (m_CurrentBlock == -1)
			{
				SetEditMode(EditMode.NoBrush);
			}
			else
			{
				SetEditMode(EditMode.BlockSelected);
				m_bUpdateBrushPositionAfterRefresh = true;
			}
			if (flag)
			{
				PlayAudio(AudioTypes.Deleted);
			}
			else
			{
				PlayAudio(AudioTypes.Error);
			}
		}
		else if (m_Player.GetButtonUp("Exit"))
		{
			ExitDeleting();
		}
		ProcessPossibleCameraMove();
	}

	private void ExitDeleting()
	{
		if (m_CurrentBlock == -1)
		{
			SetEditMode(EditMode.NoBrush);
		}
		else
		{
			SetEditMode(EditMode.BlockSelected);
		}
		UpdateUI();
		PlayAudio(AudioTypes.Marquee_Stop);
	}

	public void SaveTheLevel(bool bForceNew, LevelDetailsManager.RequestResult resultCallback = null)
	{
		if (m_SavingIcon != null)
		{
			m_SavingIcon.ShowSavingIcon();
		}
		bool flag = m_LevelDetailsMan.StoreTheLevel(null, bForceNew);
		bool flag2 = SaveSnapshot();
		PlayAudio(AudioTypes.Save);
		if (flag)
		{
			if (flag2)
			{
				resultCallback?.Invoke(LevelDetailsManager.RequestResultEnum.Success);
			}
			else
			{
				m_SaveSnapshotCallback = (LevelDetailsManager.RequestResult)Delegate.Combine(m_SaveSnapshotCallback, resultCallback);
			}
		}
		else
		{
			resultCallback?.Invoke(LevelDetailsManager.RequestResultEnum.Failed);
		}
	}

	private void SaveSnapShotAfterTrigger(Texture2D texture)
	{
		m_SnapshotDelegate = (SnapshotTakenResult)Delegate.Remove(m_SnapshotDelegate, new SnapshotTakenResult(SaveSnapShotAfterTrigger));
		SaveSnapshot();
		if (m_SaveSnapshotCallback != null)
		{
			m_SaveSnapshotCallback(LevelDetailsManager.RequestResultEnum.Success);
		}
		m_SaveSnapshotCallback = null;
	}

	public bool SaveSnapshot()
	{
		if (m_bIsDefaultSnapshot)
		{
			TriggerSnapshot();
			m_SnapshotDelegate = (SnapshotTakenResult)Delegate.Combine(m_SnapshotDelegate, new SnapshotTakenResult(SaveSnapShotAfterTrigger));
			return false;
		}
		if (m_ValidPreviewTexture)
		{
			string stringDirectory = "ESC2U" + m_LevelDetailsMan.GetLevelDirectory();
			string path = PlatformIO.GetInstance().GetPath(stringDirectory);
			if (!string.IsNullOrEmpty(path) && m_PreviewTexture != null)
			{
				try
				{
					byte[] bytes = m_PreviewTexture.EncodeToPNG();
					File.WriteAllBytes(path + "Level_snap.png", bytes);
				}
				catch (Exception)
				{
				}
			}
		}
		return true;
	}

	public void LoadSnapshot()
	{
		string levelDirectory = m_LevelDetailsMan.GetLevelDirectory();
		if (string.IsNullOrEmpty(levelDirectory))
		{
			return;
		}
		string path = PlatformIO.GetInstance().GetPath("ESC2U" + levelDirectory);
		if (string.IsNullOrEmpty(path))
		{
			return;
		}
		string path2 = path + "Level_snap.png";
		if (!File.Exists(path2))
		{
			return;
		}
		try
		{
			byte[] data = File.ReadAllBytes(path2);
			CreatePreviewTexture();
			Texture2D texture2D = new Texture2D(m_TextureWidth, m_TextureHeight, m_TextureFormat, mipmap: false);
			texture2D.LoadImage(data);
			if (texture2D.width == m_TextureWidth && texture2D.height == m_TextureHeight)
			{
				Color32[] pixels = texture2D.GetPixels32(0);
				m_PreviewTexture.SetPixels32(pixels);
				m_PreviewTexture.Apply();
				m_ValidPreviewTexture = true;
				m_bIsDefaultSnapshot = false;
			}
		}
		catch (Exception)
		{
		}
	}

	public void LoadTheLevel()
	{
		if (File.Exists(Application.persistentDataPath + "\\UserLevels\\Level.dat"))
		{
			m_LevelDetailsMan.SetupFromSave(Application.persistentDataPath + "\\UserLevels\\Level.dat");
			LoadSnapshot();
		}
		SetCurrentBlock(-1);
		m_UIController.ChangeLayer(BaseLevelManager.LevelLayers.GroundFloor);
		UpdateLayerChanges();
	}

	public Texture2D GetPreviewSnapShot()
	{
		if (!m_ValidPreviewTexture)
		{
			TriggerSnapshot();
		}
		return m_PreviewTexture;
	}

	public void SetPreviewSnapShot(Texture2D thePreview)
	{
		if (thePreview != null && thePreview.width == m_TextureWidth && thePreview.height == m_TextureHeight && thePreview.format == m_TextureFormat)
		{
			Color[] pixels = thePreview.GetPixels(0);
			CreatePreviewTexture();
			m_PreviewTexture.SetPixels(pixels, 0);
			m_PreviewTexture.Apply();
			m_ValidPreviewTexture = true;
		}
	}

	public void TakeSnapshot()
	{
		if (m_MainCamera != null)
		{
			CreatePreviewTexture();
			int depth = 24;
			RenderTexture renderTexture = new RenderTexture(m_TextureWidth, m_TextureHeight, depth);
			m_MainCamera.targetTexture = renderTexture;
			m_MainCamera.Render();
			RenderTexture.active = renderTexture;
			m_PreviewTexture.ReadPixels(new Rect(0f, 0f, m_TextureWidth, m_TextureHeight), 0, 0);
			if (!m_ValidPreviewTexture)
			{
				m_ValidPreviewTexture = true;
			}
			m_MainCamera.targetTexture = null;
			RenderTexture.active = null;
			UnityEngine.Object.Destroy(renderTexture);
			m_TriggerSnapshot = false;
			m_bIsDefaultSnapshot = false;
			m_PreviewTexture.Apply();
			if (m_SnapshotDelegate != null)
			{
				m_SnapshotDelegate(m_PreviewTexture);
			}
		}
	}

	public void TriggerSnapshot()
	{
		m_TriggerSnapshot = true;
		PushClutterforSnapshot();
	}

	public void LateUpdate()
	{
		if (m_TriggerSnapshot)
		{
			TakeSnapshot();
			PopClutterforSnapshot();
		}
	}

	public void RegisterOnSnapshotDelegate(SnapshotTakenResult onSnapshot)
	{
		if (onSnapshot != null)
		{
			m_SnapshotDelegate = (SnapshotTakenResult)Delegate.Combine(m_SnapshotDelegate, onSnapshot);
		}
	}

	public void RegisterSnapshotAction(SnapshotActions onAction)
	{
		if (onAction != null)
		{
			m_SnapshotActionDelegate = (SnapshotActions)Delegate.Combine(m_SnapshotActionDelegate, onAction);
		}
	}

	public void UnregisterSnapshotAction(SnapshotActions onAction)
	{
		if (onAction != null)
		{
			m_SnapshotActionDelegate = (SnapshotActions)Delegate.Remove(m_SnapshotActionDelegate, onAction);
		}
	}

	private void LoadAudioBanks()
	{
		if (m_BlockManager == null || AudioController.Instance == null)
		{
			return;
		}
		string audioBanks = m_BlockManager.GetAudioBanks(BuildingBlockManager.AudioBankType.Ambience);
		if (m_AmbientsLoaded != audioBanks)
		{
			if (!string.IsNullOrEmpty(m_AmbientsLoaded))
			{
				AudioController.UnloadBank(m_AmbientsLoaded);
			}
			m_AmbientsLoaded = audioBanks;
			AudioController.LoadBank(m_AmbientsLoaded);
			m_bPlayAmbientWhenLoaded = true;
		}
		audioBanks = m_BlockManager.GetAudioBanks(BuildingBlockManager.AudioBankType.UI);
		if (m_EffectsLoaded != audioBanks)
		{
			if (!string.IsNullOrEmpty(m_EffectsLoaded))
			{
				AudioController.UnloadBank(m_EffectsLoaded);
			}
			m_EffectsLoaded = audioBanks;
			AudioController.LoadBank(m_EffectsLoaded);
		}
	}

	private void UnloadAudioBanks()
	{
		if (!(AudioController.Instance == null))
		{
			if (!string.IsNullOrEmpty(m_AmbientsLoaded))
			{
				PlayAudio(AudioTypes.Ambient_Stop);
				AudioController.UnloadBank(m_AmbientsLoaded);
				m_AmbientsLoaded = string.Empty;
			}
			if (!string.IsNullOrEmpty(m_EffectsLoaded))
			{
				AudioController.UnloadBank(m_EffectsLoaded);
				m_EffectsLoaded = string.Empty;
			}
		}
	}

	public void PlayAudio(AudioTypes audioType)
	{
		if (AudioController.Instance == null)
		{
			return;
		}
		if (audioType == AudioTypes.Ambient_Start || audioType == AudioTypes.Ambient_Stop)
		{
			if (!string.IsNullOrEmpty(m_AmbientsLoaded))
			{
				if (audioType == AudioTypes.Ambient_Start)
				{
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Amb_Editor, AudioController.InGameMusicAndAmbienceObject);
				}
				else
				{
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Stop_UI_Amb_Editor, AudioController.InGameMusicAndAmbienceObject);
				}
			}
		}
		else
		{
			if (string.IsNullOrEmpty(m_EffectsLoaded))
			{
				return;
			}
			switch (audioType)
			{
			case AudioTypes.NextVariation:
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Editor_Cycle, AudioController.UI_Audio_GO);
				break;
			case AudioTypes.DownFloor:
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Editor_Floor_Down, AudioController.UI_Audio_GO);
				break;
			case AudioTypes.UpFloor:
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Editor_Floor_Up, AudioController.UI_Audio_GO);
				break;
			case AudioTypes.Deleted:
				if (m_MarqueePlaying)
				{
					PlayAudio(AudioTypes.Marquee_Stop);
				}
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Editor_Object_Delete, AudioController.UI_Audio_GO);
				break;
			case AudioTypes.Marquee_Start:
				if (!m_MarqueePlaying)
				{
					m_fTimeUntilStopMarqueeSound = Time.timeSinceLevelLoad + m_MarqueeAudioTimeout;
					m_LastMarqueeValue = 0.5f;
					AudioController.SetParameter(Game_Parameter.MiniMap_Zoom, m_LastMarqueeValue);
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Editor_Object_Marquee_In, AudioController.UI_Audio_GO);
					m_MarqueePlaying = true;
				}
				break;
			case AudioTypes.Marquee_Start_NOCheck:
				m_fTimeUntilStopMarqueeSound = Time.timeSinceLevelLoad + m_MarqueeAudioTimeout;
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Editor_Object_Marquee_In, AudioController.UI_Audio_GO);
				break;
			case AudioTypes.Marquee_Stop:
				if (m_MarqueePlaying)
				{
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Stop_UI_Editor_Object_Marquee_Loop, AudioController.UI_Audio_GO);
					m_MarqueePlaying = false;
				}
				break;
			case AudioTypes.Marquee_Stop_NOCheck:
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Stop_UI_Editor_Object_Marquee_Loop, AudioController.UI_Audio_GO);
				break;
			case AudioTypes.Marquee_Finished:
				if (m_MarqueePlaying)
				{
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Editor_Object_Marquee_Out, AudioController.UI_Audio_GO);
					m_MarqueePlaying = false;
				}
				break;
			case AudioTypes.Placed:
				if (m_MarqueePlaying)
				{
					PlayAudio(AudioTypes.Marquee_Finished);
				}
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Editor_Object_Place, AudioController.UI_Audio_GO);
				break;
			case AudioTypes.Zoom:
			{
				float value = (float)m_CurrentZoomLevel / (float)(m_ZoomLevels.Length - 1);
				AudioController.SetParameter(Game_Parameter.Editor_Zoom, value);
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Editor_Object_Zoom, AudioController.UI_Audio_GO);
				break;
			}
			case AudioTypes.Redo:
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Editor_Redo, AudioController.UI_Audio_GO);
				break;
			case AudioTypes.Undo:
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Editor_Undo, AudioController.UI_Audio_GO);
				break;
			case AudioTypes.TabIn:
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Prison_Editor_Window_In, AudioController.UI_Audio_GO);
				break;
			case AudioTypes.TabOut:
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Prison_Editor_Window_Out, AudioController.UI_Audio_GO);
				break;
			case AudioTypes.Save:
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Prison_Editor_SaveSuccess, AudioController.UI_Audio_GO);
				break;
			case AudioTypes.Error:
				if (m_MarqueePlaying)
				{
					PlayAudio(AudioTypes.Marquee_Stop);
				}
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Editor_Reject, AudioController.UI_Audio_GO);
				break;
			}
		}
	}

	private void UpdateAudio()
	{
		if (AudioController.Instance == null)
		{
			return;
		}
		if (m_MarqueePlaying && m_NewMarqueeBrush != null)
		{
			float num = Mathf.Max(m_iLastMarqueWidth, m_iLastMarqueHeight);
			float num2 = (num - 1f) / (m_ZoomLevels[m_CurrentZoomLevel] * 2f) / 2f + 0.5f;
			if (m_Block_X_Position != m_LastMarqueePos_X || m_Block_Y_Position != m_LastMarqueePos_Y)
			{
				m_LastMarqueePos_X = m_Block_X_Position;
				m_LastMarqueePos_Y = m_Block_Y_Position;
				if (m_fTimeUntilStopMarqueeSound == 0f)
				{
					PlayAudio(AudioTypes.Marquee_Start_NOCheck);
				}
				else
				{
					m_fTimeUntilStopMarqueeSound = Time.realtimeSinceStartup + m_MarqueeAudioTimeout;
				}
			}
			else if (Time.realtimeSinceStartup > m_fTimeUntilStopMarqueeSound)
			{
				m_fTimeUntilStopMarqueeSound = 0f;
				PlayAudio(AudioTypes.Marquee_Stop_NOCheck);
			}
			if (m_LastMarqueeValue != num2)
			{
				m_LastMarqueeValue = num2;
				AudioController.SetParameter(Game_Parameter.MiniMap_Zoom, num2);
			}
		}
		if (m_bPlayAmbientWhenLoaded && !string.IsNullOrEmpty(m_AmbientsLoaded) && AudioController.IsBankLoaded(m_AmbientsLoaded))
		{
			PlayAudio(AudioTypes.Ambient_Start);
			m_bPlayAmbientWhenLoaded = false;
		}
	}

	public void MoveButtonPressed()
	{
		ResetTimer();
		m_InstructionManager.DeleteArea((sbyte)m_LastSelected_X_Position, (sbyte)m_LastSelected_Y_Position, 1, 1);
		m_InstructionManager.UpdateLevel();
		int flipBlockIfRequired = GetFlipBlockIfRequired(m_LastSelected_Block);
		ResetSelectedMode(bResetMode: false);
		SetCurrentBlock(flipBlockIfRequired);
		m_bMovingBlock = true;
		SetEditMode(EditMode.MovingBlock);
	}

	public void CopyButtonPressed()
	{
		int flipBlockIfRequired = GetFlipBlockIfRequired(m_LastSelected_Block);
		ResetSelectedMode(bResetMode: false);
		SetCurrentBlock(flipBlockIfRequired);
	}

	public void DeleteButtonPressed()
	{
		ResetTimer();
		m_InstructionManager.DeleteArea((sbyte)m_LastSelected_X_Position, (sbyte)m_LastSelected_Y_Position, 1, 1);
		m_InstructionManager.UpdateLevel();
		ResetSelectedMode(bResetMode: true);
	}

	public int GetFlipBlockIfRequired(int iBlock)
	{
		BuildingBlock_Room buildingBlock_Room = BuildingBlockManager.GetBlock(iBlock) as BuildingBlock_Room;
		if (buildingBlock_Room != null && !buildingBlock_Room.IsValidForLayer(m_LevelManager.m_CurrentLayer))
		{
			return buildingBlock_Room.m_FlipBlock;
		}
		return iBlock;
	}

	public void ResetSelectedMode(bool bResetMode)
	{
		if (m_EditMode == EditMode.SelectedObjectInLevel || m_EditMode == EditMode.SelectingObjectInLevel || m_EditMode == EditMode.CopySelectedObjectInLevel || m_EditMode == EditMode.CopySelectedObjectInLevel_Edit)
		{
			if (m_SelectedBorder != null)
			{
				UnityEngine.Object.Destroy(m_SelectedBorder);
				m_SelectedBorder = null;
			}
			m_LastSelected_X_Position = 0;
			m_LastSelected_Y_Position = 0;
			m_LastSelected_Block = -1;
			m_LastSelected_Object = null;
			ShowSelectedBlockMenu(bShowMove: false, bShowDelete: false, bShowCopy: false);
			if (bResetMode)
			{
				SetEditMode(EditMode.NoBrush);
			}
		}
	}

	public void PreviewLevel()
	{
		SaveTheLevel(bForceNew: false, PreviewSaveLevelComplete);
	}

	private void PreviewSaveLevelComplete(LevelDetailsManager.RequestResultEnum eResult)
	{
		if (eResult == LevelDetailsManager.RequestResultEnum.Success && (bool)GlobalStart.GetInstance() && m_LevelDetailsMan != null)
		{
			PrisonData prisonDataForPrison = LevelDataManager.GetInstance().GetPrisonDataForPrison(LevelScript.PRISON_ENUM.Centre_Perks);
			PrisonData prisonData = UnityEngine.Object.Instantiate(prisonDataForPrison);
			prisonData.m_Configs.Clear();
			for (int i = 0; i < LevelDataManager.GetInstance().m_CustomPrisonConfigs.Length; i++)
			{
				PrisonConfig prisonConfig = UnityEngine.Object.Instantiate(LevelDataManager.GetInstance().m_CustomPrisonConfigs[i]);
				prisonConfig.m_ConfigType = PrisonConfig.ConfigType.Singleplayer;
				prisonData.m_Configs.Add(prisonConfig);
			}
			prisonData.m_NameLocalizationKey = m_LevelDetailsMan.GetLevelName();
			prisonData.m_DescriptionLocalizationKey = m_LevelDetailsMan.GetLevelDecription();
			prisonData.m_ImagePath = PlatformIO.GetInstance().GetPath(m_LevelDetailsMan.GetLevelDirectory()) + "Level_snap.png";
			prisonData.m_LevelInfo.m_PrisonType = LevelScript.PRISON_TYPE.Normal;
			prisonData.m_LevelInfo.m_PrisonEnum = LevelScript.PRISON_ENUM.CustomPrison;
			prisonData.m_LevelInfo.m_AssociatedFile = "ESC2U" + m_LevelDetailsMan.GetLevelDirectory();
			prisonData.m_CustomisableRoles = new int[2];
			prisonData.m_CustomisableRoles[0] = m_LevelDetailsMan.GetNumberOfInmates();
			prisonData.m_CustomisableRoles[1] = m_LevelDetailsMan.GetNumberOfGuards();
			PlaylistData playlistData = ScriptableObject.CreateInstance(typeof(PlaylistData)) as PlaylistData;
			playlistData.m_DescriptionLocalisationKey = prisonData.m_DescriptionLocalizationKey;
			playlistData.m_NameLocalisationKey = prisonData.m_NameLocalizationKey;
			playlistData.m_ImagePath = prisonData.m_ImagePath;
			playlistData.m_Prisons.Add(new PlaylistData.PrisonSetup(prisonData, (int)m_LevelDetailsMan.GetLevelDifficulty()));
			playlistData.m_GUID = "T17_User_Level_test";
			GlobalStart.GetInstance().PreviewEditorLevel();
			GlobalStart.GetInstance().SetSelectedPlaylist(playlistData);
			if (Gamer.GetPrimaryGamer() != null)
			{
				T17EventSystem.ApplyLastRequestedStateSinceDialogboxWasUp(Gamer.GetPrimaryGamer().m_RewiredPlayer);
				T17EventSystem.ApplyCategories(Gamer.GetPrimaryGamer().m_RewiredPlayer, T17EventSystem.InputCateogryStates.InGame);
			}
		}
	}

	private void PushClutterforSnapshot()
	{
		if (m_UIController != null)
		{
			m_UIController.PushSelectedBlockMenu();
		}
		if (m_SelectedBorder != null && m_SelectedBorder.gameObject.activeSelf)
		{
			m_PushedSelectedBorder = true;
			m_SelectedBorder.gameObject.SetActive(value: false);
		}
		else
		{
			m_PushedSelectedBorder = false;
		}
		if (m_Grid != null && m_Grid.activeSelf)
		{
			m_bGridWasActiveBeforeSnapShot = true;
			m_Grid.SetActive(value: false);
		}
		else
		{
			m_bGridWasActiveBeforeSnapShot = false;
		}
		if (m_SnapshotActionDelegate != null)
		{
			m_SnapshotActionDelegate(bBefore: true);
		}
	}

	private void PopClutterforSnapshot()
	{
		if (m_UIController != null)
		{
			m_UIController.PopSelectedBlockMenu();
		}
		if (m_SelectedBorder != null && m_PushedSelectedBorder)
		{
			m_SelectedBorder.gameObject.SetActive(value: true);
		}
		if (m_Grid != null && m_bGridWasActiveBeforeSnapShot)
		{
			m_Grid.SetActive(value: true);
		}
		if (m_SnapshotActionDelegate != null)
		{
			m_SnapshotActionDelegate(bBefore: false);
		}
		m_bGridWasActiveBeforeSnapShot = false;
		m_PushedSelectedBorder = false;
	}

	private Vector3 GetCorrectedPosition(Vector3 vPos)
	{
		float num = m_MainCamera.pixelHeight;
		float num2 = m_MainCamera.pixelWidth;
		float num3 = num2 / num;
		float num4 = m_MainCamera.orthographicSize * 2f;
		float num5 = num4 * num3;
		float num6 = num5 / num2;
		float num7 = num4 / num;
		int num8 = (int)(vPos.x / num6);
		int num9 = (int)(vPos.y / num7);
		vPos.x = (float)num8 * num6;
		vPos.y = (float)num9 * num7;
		return vPos;
	}

	private void SetOrthagraphicSize(int iZoomLevel)
	{
		float num = m_ZoomLevels[iZoomLevel] * 2f;
		float num2 = m_MainCamera.pixelHeight;
		int num3 = (int)(num2 / num);
		float num4 = (float)num3 * num;
		m_fOrthographicSize = num * (num2 / num4) / 2f;
		m_MainCamera.orthographicSize = m_fOrthographicSize;
		m_MainCamera.transform.localPosition = GetCorrectedPosition(m_MainCamera.transform.localPosition);
	}

	public Vector3 ConvertLevelToUIPosition(Vector3 vPos)
	{
		if (m_MainCamera != null)
		{
			return m_MainCamera.WorldToScreenPoint(vPos);
		}
		return vPos;
	}

	public float GetCameraOrthographicSize()
	{
		return m_fOrthographicSize;
	}

	public static void CreateTempate(int FloorBlock, int MainBlock, int GapBlock, int SurroundBlock)
	{
		LevelEditor_Controller instance = GetInstance();
		if (instance == null || instance.m_InstructionManager == null)
		{
			return;
		}
		int num = 512;
		int num2 = 4;
		int num3 = 0;
		if (SurroundBlock != -1)
		{
			num2 = 6;
			num3 = 1;
		}
		int num4 = 120 / num2;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int[] array = new int[9] { 2, 2, 2, 1, 1, 1, 0, 0, 0 };
		int[] array2 = new int[9] { 0, 1, 2, 0, 1, 2, 0, 1, 2 };
		for (int i = 0; i < num; i++)
		{
			if (num6 + num2 - 2 >= 120 || num7 + num2 - 2 >= 118)
			{
				continue;
			}
			instance.m_InstructionManager.DeleteArea((sbyte)num6, (sbyte)num7, num2 - 1, num2 - 1);
			if (FloorBlock != -1)
			{
				instance.m_InstructionManager.AddBlockArea(FloorBlock, (sbyte)(num6 + num3), (sbyte)(num7 + num3), 3, 3, 0);
			}
			if (SurroundBlock != -1)
			{
				instance.m_InstructionManager.AddBlockArea(SurroundBlock, (sbyte)num6, (sbyte)num7, 5, 1, 0);
				instance.m_InstructionManager.AddBlockArea(SurroundBlock, (sbyte)num6, (sbyte)(num7 + 4), 5, 1, 0);
				instance.m_InstructionManager.AddBlockArea(SurroundBlock, (sbyte)num6, (sbyte)(num7 + 1), 1, 3, 0);
				instance.m_InstructionManager.AddBlockArea(SurroundBlock, (sbyte)(num6 + 4), (sbyte)(num7 + 1), 1, 3, 0);
			}
			int num8 = 1;
			for (int j = 0; j < 9; j++)
			{
				if ((num8 & i) != 0)
				{
					if (MainBlock != -1)
					{
						instance.m_InstructionManager.AddBlockOnce(MainBlock, (sbyte)(num6 + array2[j] + num3), (sbyte)(num7 + array[j] + num3), 0);
					}
				}
				else if (GapBlock != -1)
				{
					instance.m_InstructionManager.AddBlockOnce(GapBlock, (sbyte)(num6 + array2[j] + num3), (sbyte)(num7 + array[j] + num3), 0);
				}
				num8 <<= 1;
			}
			if (++num5 >= num4)
			{
				num5 = 0;
				num6 = 0;
				num7 += num2;
			}
			else
			{
				num6 += num2;
			}
		}
		instance.m_InstructionManager.UpdateLevel();
	}

	private void StartCopyMarquee()
	{
		ResetCopyData(bPartial: false);
		m_LastDrawnPosition.x = m_LastSelected_X_Position;
		m_LastDrawnPosition.y = m_LastSelected_Y_Position;
		SetEditMode(EditMode.CopyMarquee);
	}

	private void StartCopyAdd()
	{
		ResetCopyData(bPartial: true);
		m_LastDrawnPosition.x = m_Block_X_Position;
		m_LastDrawnPosition.y = m_Block_Y_Position;
		SetEditMode(EditMode.CopyMarqueeAdd);
	}

	private void StartCopyDelete()
	{
		ResetCopyData(bPartial: true);
		m_LastDrawnPosition.x = m_Block_X_Position;
		m_LastDrawnPosition.y = m_Block_Y_Position;
		SetEditMode(EditMode.CopyMarqueeDelete);
	}

	private void ResetCopyData(bool bPartial)
	{
		CopyEnum copyEnum = CopyEnum.Empty;
		if (bPartial)
		{
			copyEnum = CopyEnum.MarkedAll;
		}
		for (int i = 0; i < 14400; i++)
		{
			CopyEnum[] copyAreaFlags;
			int num;
			(copyAreaFlags = m_CopyAreaFlags)[num = i] = copyAreaFlags[num] & copyEnum;
			if (!bPartial)
			{
				m_CopyArea[i] = false;
			}
		}
	}

	private void CreateBorderForCopy()
	{
		if (m_SelectedBorder != null)
		{
			UnityEngine.Object.Destroy(m_SelectedBorder);
			m_SelectedBorder = null;
		}
		m_SelectedBorder = new GameObject("Border");
		m_SelectedBorder.transform.SetParent(m_LevelDetailsMan.m_LevelMasterGameObject.transform);
		m_SelectedBorder.transform.localPosition = new Vector3(-59.5f, -59.5f, -40f);
		LevelEditorBorderElement.CreateBorderPiecesFromMap(m_SelectedBorder.transform, ref m_CopyArea, null, 120, 0, 0);
	}

	public void OnPopupShown(BaseLevelEditor_BasePopout popup)
	{
		if (m_PopupControl != popup)
		{
			HidePopup();
			m_PopupControl = popup;
		}
	}

	public void OnPopupHidden(BaseLevelEditor_BasePopout popup)
	{
		if (m_PopupControl == popup)
		{
			m_PopupControl = null;
		}
	}

	public void HidePopup()
	{
		if (m_PopupControl != null)
		{
			m_PopupControl.Hide();
		}
	}

	public bool IsPopupShown()
	{
		return m_PopupControl != null;
	}

	public void GotoBadZone(BaseLevelManager.LevelLayers eLayer)
	{
		LevelEditor_ZoneManager instance = LevelEditor_ZoneManager.GetInstance();
		if (!(instance != null))
		{
			return;
		}
		LevelEditor_ZoneManager.Zone invalidZonesInLayer = instance.GetInvalidZonesInLayer(eLayer, ref m_BadZone);
		if (invalidZonesInLayer != null)
		{
			if (m_LevelManager.m_CurrentLayer != eLayer)
			{
				m_UIController.OnLayerButtonClicked((int)eLayer);
			}
			int num = invalidZonesInLayer.m_Left + invalidZonesInLayer.m_Width / 2;
			int num2 = invalidZonesInLayer.m_Bottom + invalidZonesInLayer.m_Height / 2;
			m_MainCamera.transform.localPosition = new Vector3((float)num - 60f, (float)num2 - 60f, m_MainCamera.transform.localPosition.z);
			ExternalSetCurrentZone(invalidZonesInLayer.m_ID);
		}
	}

	public bool EnterCreateZone(ZoneDetailsManager.ZoneTypes zoneType)
	{
		if (zoneType == ZoneDetailsManager.ZoneTypes.INVALID || zoneType >= ZoneDetailsManager.ZoneTypes.TOTAL)
		{
			return false;
		}
		switch (m_EditMode)
		{
		case EditMode.BlockSelected:
		case EditMode.MovingBlock:
			SetCurrentBlock(-1);
			break;
		case EditMode.SelectingObjectInLevel:
		case EditMode.SelectedObjectInLevel:
		case EditMode.CopySelectedObjectInLevel:
		case EditMode.CopySelectedObjectInLevel_Edit:
			ResetSelectedMode(bResetMode: false);
			break;
		case EditMode.Zone_WaitingToCreate:
		{
			FloatingZoneIcon instance = FloatingZoneIcon.GetInstance();
			if (instance != null)
			{
				instance.SetTheZoneType(m_ZoneToCreate);
			}
			break;
		}
		default:
			return false;
		case EditMode.NoBrush:
		case EditMode.Zone_Creating:
		case EditMode.Zone_Editing:
			break;
		}
		ClearCurrentZone();
		ClearOverZoneZone();
		SetZoneToCreate(zoneType);
		SetEditMode(EditMode.Zone_WaitingToCreate);
		return true;
	}

	public bool EnterEditZone(int iZoneID)
	{
		LevelEditor_ZoneManager.Zone zone = m_ZoneManager.GetZone(iZoneID);
		if (zone == null)
		{
			return false;
		}
		switch (m_EditMode)
		{
		case EditMode.BlockSelected:
		case EditMode.MovingBlock:
			SetCurrentBlock(-1);
			break;
		case EditMode.SelectingObjectInLevel:
		case EditMode.SelectedObjectInLevel:
		case EditMode.CopySelectedObjectInLevel:
		case EditMode.CopySelectedObjectInLevel_Edit:
			ResetSelectedMode(bResetMode: false);
			break;
		default:
			return false;
		case EditMode.NoBrush:
			break;
		}
		SetZoneToCreate(zone.m_ZoneType);
		SetCurrentZone(zone);
		SetEditMode(EditMode.Zone_Editing);
		return true;
	}

	public void DeleteZone(LevelEditor_ZoneManager.Zone zone)
	{
		ResetTimer();
		m_InstructionManager.DeleteZone(zone);
		PlayAudio(AudioTypes.Deleted);
		if (object.ReferenceEquals(zone, m_CurrentZone))
		{
			SetCurrentZone(null);
			SetEditMode(EditMode.NoBrush);
		}
	}

	public void ExternalSetCurrentZone(int zoneID, bool bSelectZoneTab = true)
	{
		LevelEditor_ZoneManager.Zone objB = null;
		if (zoneID != -1)
		{
			objB = m_ZoneManager.GetZone(zoneID);
		}
		if (!object.ReferenceEquals(m_CurrentZone, objB))
		{
			switch (m_EditMode)
			{
			case EditMode.Zone_Adding:
				ExitZoneAdding();
				break;
			case EditMode.Zone_Creating:
				ExitZoneCreating();
				break;
			case EditMode.Zone_Deleting:
				ExitZoneDeleting();
				break;
			case EditMode.Zone_Editing:
				ExitZoneEditing();
				break;
			case EditMode.Zone_WaitingToCreate:
				ExitZoneWaitingToCreate();
				break;
			}
			SetCurrentZone(zoneID, bSelectZoneTab);
			CurrentZoneHouseKeeping();
		}
	}

	public bool SetCurrentZone(int zoneID, bool bSelectZoneTab = true)
	{
		if (zoneID == -1)
		{
			return SetCurrentZone(null, bClearIfSetSame: false, bSelectZoneTab);
		}
		LevelEditor_ZoneManager.Zone zone = m_ZoneManager.GetZone(zoneID);
		if (zone == null)
		{
			return false;
		}
		return SetCurrentZone(zone, bClearIfSetSame: false, bSelectZoneTab);
	}

	public bool SetCurrentZone(LevelEditor_ZoneManager.Zone zone, bool bClearIfSetSame = false, bool bSelectZoneTab = true)
	{
		if (!object.ReferenceEquals(m_CurrentZone, zone))
		{
			if (m_CurrentZone != null)
			{
				ClearCurrentZone();
			}
			if (zone != null)
			{
				m_CurrentZone = zone;
				if (!object.ReferenceEquals(m_CurrentZone, m_OverZone))
				{
					ClearOverZoneZone();
				}
				GameObject[] palletBackerTabs = m_UIController.m_PalletBackerTabs;
				if (bSelectZoneTab)
				{
					int i = 0;
					for (int num = palletBackerTabs.Length; i < num; i++)
					{
						if (!palletBackerTabs[i].activeSelf && palletBackerTabs[i].GetComponentInChildren<LevelEditor_ZoneTab>() != null)
						{
							m_UIController.m_PaletteTabGroup.SetTabIndex(i);
						}
					}
				}
			}
			return true;
		}
		if (bClearIfSetSame && zone != null)
		{
			ClearCurrentZone();
			if (m_CurrentZone == null)
			{
				SetOverZone(zone);
				SetOverZoneMode(LevelEditor_ZoneIconControl.Mode.MouseOverButton);
			}
			return true;
		}
		return false;
	}

	public bool SetOverZone(int zoneID)
	{
		if (zoneID == -1)
		{
			return SetOverZone(null);
		}
		LevelEditor_ZoneManager.Zone zone = m_ZoneManager.GetZone(zoneID);
		if (zone == null)
		{
			return false;
		}
		return SetOverZone(zone);
	}

	public bool SetOverZone(LevelEditor_ZoneManager.Zone zone)
	{
		bool flag = !object.ReferenceEquals(m_OverZone, zone);
		if (flag)
		{
			m_OverZone = zone;
		}
		return flag;
	}

	private void ClearCurrentZone()
	{
		if (m_CurrentZone != null)
		{
			SetCurrentZoneMode(LevelEditor_ZoneIconControl.Mode.NotOverZone, bForce: false);
			m_CurrentZone = null;
		}
	}

	private void ClearOverZoneZone()
	{
		if (m_OverZone != null)
		{
			SetOverZoneMode(LevelEditor_ZoneIconControl.Mode.NotOverZone);
			m_OverZone = null;
		}
	}

	public void SetZoneToCreate(ZoneDetailsManager.ZoneTypes zoneType)
	{
		m_ZoneToCreate = zoneType;
	}

	private void SetOverZoneMode(LevelEditor_ZoneIconControl.Mode zoneMode)
	{
		if (m_OverZone != null && m_OverZone.m_ZoneIcon != null && !object.ReferenceEquals(m_OverZone, m_CurrentZone))
		{
			m_OverZone.m_ZoneIcon.SetMode(zoneMode);
		}
	}

	private void SetCurrentZoneMode(LevelEditor_ZoneIconControl.Mode zoneMode, bool bForce)
	{
		if (m_CurrentZone != null && m_CurrentZone.m_ZoneIcon != null && (bForce || !object.ReferenceEquals(m_OverZone, m_CurrentZone)))
		{
			m_CurrentZone.m_ZoneIcon.SetMode(zoneMode);
		}
	}

	private void ZoneHouseKeeping()
	{
		if (m_OverControl)
		{
			bool flag = true;
			if (m_OverZone != null && m_OverZone.m_ZoneIcon != null && m_OverZone.m_ZoneIcon.IsDisplayingMenu())
			{
				flag = false;
			}
			if (m_CurrentZone != null && m_CurrentZone.m_ZoneIcon != null && m_CurrentZone.m_ZoneIcon.IsDisplayingMenu())
			{
				flag = false;
			}
			if (flag)
			{
				return;
			}
		}
		CurrentZoneHouseKeeping();
		if (m_OverZone != null && !m_OverZone.m_bActive)
		{
			ClearOverZoneZone();
		}
		bool flag2 = false;
		LevelEditor_ZoneManager.Zone zone = null;
		LevelEditor_ZoneIconControl.OverIconEnum overIconEnum = LevelEditor_ZoneIconControl.OverIconEnum.NotNear;
		if (m_OverZone != null)
		{
			overIconEnum = m_OverZone.m_ZoneIcon.IsOverIcon(m_RawMouseToScreen.x, m_RawMouseToScreen.y);
		}
		switch (overIconEnum)
		{
		case LevelEditor_ZoneIconControl.OverIconEnum.NotNear:
			zone = m_ZoneManager.GetZoneAt(m_Block_X_Position, m_Block_Y_Position, m_LevelManager.m_CurrentLayer);
			if (zone != null && zone.m_ZoneIcon != null && zone.m_ZoneIcon.IsOverIcon(m_RawMouseToScreen.x, m_RawMouseToScreen.y) == LevelEditor_ZoneIconControl.OverIconEnum.Over)
			{
				flag2 = true;
			}
			break;
		case LevelEditor_ZoneIconControl.OverIconEnum.Near:
			zone = m_OverZone;
			break;
		case LevelEditor_ZoneIconControl.OverIconEnum.Over:
			zone = m_OverZone;
			flag2 = true;
			break;
		}
		if (zone == null)
		{
			if (m_OverZone != null)
			{
				ClearOverZoneZone();
			}
			return;
		}
		if (!object.ReferenceEquals(m_OverZone, zone))
		{
			ClearOverZoneZone();
			if (m_CurrentZone == null || m_CurrentZone.m_ZoneIcon == null || !m_CurrentZone.m_ZoneIcon.IsInMode(LevelEditor_ZoneIconControl.Mode.ZoneSelectedOverButton, bCheckCurrent: true, bCheckTarget: true))
			{
				SetOverZone(zone);
			}
		}
		if (m_OverZone == null || !(m_OverZone.m_ZoneIcon != null))
		{
			return;
		}
		EditMode editMode = m_EditMode;
		if (editMode == EditMode.MovingCamera)
		{
			editMode = m_PreviousEditMode;
		}
		switch (editMode)
		{
		case EditMode.Zone_WaitingToCreate:
		case EditMode.Zone_Creating:
		case EditMode.Zone_Editing:
		case EditMode.Zone_Adding:
		case EditMode.Zone_Deleting:
			SetOverZoneMode(LevelEditor_ZoneIconControl.Mode.NotOverZone);
			return;
		case EditMode.FreeDrawing:
		case EditMode.Marquee:
		case EditMode.MarqueeLine:
		case EditMode.Deleting:
		case EditMode.CopyMarquee:
		case EditMode.CopyMarqueeAdd:
		case EditMode.CopyMarqueeDelete:
			SetOverZoneMode(LevelEditor_ZoneIconControl.Mode.MouseOverZone);
			return;
		}
		if (flag2)
		{
			SetOverZoneMode(LevelEditor_ZoneIconControl.Mode.MouseOverButton);
		}
		else
		{
			SetOverZoneMode(LevelEditor_ZoneIconControl.Mode.MouseOverZone);
		}
	}

	private void CurrentZoneHouseKeeping()
	{
		if (m_CurrentZone != null && !m_CurrentZone.m_bActive)
		{
			ClearCurrentZone();
		}
		EditMode editMode = m_EditMode;
		if (editMode == EditMode.MovingCamera)
		{
			editMode = m_PreviousEditMode;
		}
		if (m_CurrentZone == null || !m_CurrentZone.m_bActive || !(m_CurrentZone.m_ZoneIcon != null))
		{
			return;
		}
		switch (editMode)
		{
		case EditMode.Zone_Editing:
		case EditMode.Zone_Adding:
		case EditMode.Zone_Deleting:
			SetCurrentZoneMode(LevelEditor_ZoneIconControl.Mode.EditingTheZone, bForce: true);
			return;
		case EditMode.FreeDrawing:
		case EditMode.Marquee:
		case EditMode.MarqueeLine:
		case EditMode.Deleting:
		case EditMode.CopyMarquee:
		case EditMode.CopyMarqueeAdd:
		case EditMode.CopyMarqueeDelete:
			SetCurrentZoneMode(LevelEditor_ZoneIconControl.Mode.ZoneSelected, bForce: true);
			return;
		}
		if (m_CurrentZone.m_Layer == m_LevelManager.m_CurrentLayer && m_CurrentZone.m_ZoneIcon.IsOverIcon(m_RawMouseToScreen.x, m_RawMouseToScreen.y) == LevelEditor_ZoneIconControl.OverIconEnum.Over)
		{
			SetCurrentZoneMode(LevelEditor_ZoneIconControl.Mode.ZoneSelectedOverButton, bForce: true);
			if (m_OverZone != null && !object.ReferenceEquals(m_OverZone, m_CurrentZone))
			{
				ClearOverZoneZone();
			}
		}
		else
		{
			SetCurrentZoneMode(LevelEditor_ZoneIconControl.Mode.ZoneSelected, bForce: true);
		}
	}

	public void DisplayZoneMenu(bool bDisplay, LevelEditor_ZoneManager.Zone zone)
	{
		if (m_ZoneControl != null)
		{
			if (!bDisplay || zone == null)
			{
				m_ZoneControl.SetZone(null);
			}
			else
			{
				m_ZoneControl.SetZone(zone);
			}
		}
	}

	private void CreateMarquee(int iWidth, int iHeight, int iLeft, int iBottom, string strColor)
	{
		if (iWidth == 0 || iHeight == 0)
		{
			HideMarquee();
			return;
		}
		if (m_NewMarqueeBrush == null)
		{
			if (m_NewMarqueeResource != null)
			{
				int num = iWidth * iHeight;
				bool[] array = new bool[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = true;
				}
				m_NewMarqueeBrush = LevelEditor_Marquee.CreateMarqueeFromMap(m_NewMarqueeResource, "marquee", strColor, array, iWidth, iLeft, iBottom, BaseLevelManager.LevelLayers.GroundFloor);
				m_iLastMarqueWidth = iWidth;
				m_iLastMarqueHeight = iHeight;
				m_iLastMarqueLeft = iLeft;
				m_iLastMarqueBottom = iBottom;
				m_strLastMarqueColor = strColor;
				m_NewMarqueeBrush.transform.localPosition = new Vector3(-60f + (float)m_iLastMarqueLeft, -60f + (float)m_iLastMarqueBottom, -40f);
			}
			return;
		}
		if (string.CompareOrdinal(m_strLastMarqueColor, strColor) != 0)
		{
			m_NewMarqueeBrush.SetColourState(strColor);
			m_strLastMarqueColor = strColor;
		}
		if (iHeight != m_iLastMarqueHeight || iWidth != m_iLastMarqueWidth)
		{
			m_iLastMarqueWidth = iWidth;
			m_iLastMarqueHeight = iHeight;
			int num2 = iWidth * iHeight;
			bool[] array2 = new bool[num2];
			for (int j = 0; j < num2; j++)
			{
				array2[j] = true;
			}
			m_NewMarqueeBrush.RegenerateFromMap(array2, m_iLastMarqueWidth);
		}
		if (m_iLastMarqueLeft != iLeft || m_iLastMarqueBottom != iBottom)
		{
			m_iLastMarqueLeft = iLeft;
			m_iLastMarqueBottom = iBottom;
			m_NewMarqueeBrush.transform.localPosition = new Vector3(-60f + (float)m_iLastMarqueLeft, -60f + (float)m_iLastMarqueBottom, -40f);
		}
		if (m_NewMarqueeBrush != null)
		{
			m_NewMarqueeBrush.ShowMarquee(bShow: false);
		}
		m_NewMarqueeBrush.ShowMarquee(bShow: true);
	}

	private void HideMarquee()
	{
		if (m_NewMarqueeBrush != null)
		{
			m_NewMarqueeBrush.ShowMarquee(bShow: false);
		}
	}

	public bool IsMenuNearButtons()
	{
		switch (m_ZoneControl.GetState())
		{
		case LevelEditor_ZoneControl.State.NotInUse:
			return false;
		default:
			return false;
		case LevelEditor_ZoneControl.State.Selected:
		case LevelEditor_ZoneControl.State.SelectedInvalid:
		{
			if (m_EditMode != EditMode.SelectingObjectInLevel && m_EditMode != EditMode.SelectedObjectInLevel)
			{
				return false;
			}
			for (int i = 0; i < 3; i++)
			{
				Rect area = Rect.zero;
				if (!m_UIController.GetButtonArea((LevelEditor_UIController.ButtonTypes)i, ref area))
				{
					continue;
				}
				for (int j = 0; j < 3; j++)
				{
					Rect area2 = Rect.zero;
					if (m_ZoneControl.GetButtonArea((LevelEditor_ZoneControl.MenuButtonTypes)j, ref area2) && area2.Overlaps(area))
					{
						return true;
					}
				}
			}
			return false;
		}
		}
	}

	private bool IsZoneAreaValid(ref int iOrriginalStartX, ref int iOrriginalStartY, ref int iOrriginalWidth, ref int iOrriginalHeight, BaseLevelManager.LevelLayers layer, ref byte[] zonePrint)
	{
		int num = iOrriginalStartX;
		int num2 = iOrriginalStartY;
		int num3 = iOrriginalWidth;
		int num4 = iOrriginalHeight;
		if (num3 >= 1 && num4 >= 1)
		{
			int num5 = num2 * 120 + num;
			int num6 = 120 - num3;
			int num7 = num3 * num4;
			ScanBits[] map = new ScanBits[num7];
			int[] map2 = m_ZoneManager.GetZoneMap(layer).m_Map;
			BaseLevelManager.TileProperty[] tileProperties = m_LevelManager.m_BuildingLayers[(uint)layer].m_TileProperties;
			int num8 = 100000;
			int num9 = -1000;
			int num10 = 100000;
			int num11 = -1000;
			int num12 = 0;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			for (int i = 0; i < num4; i++)
			{
				for (int j = 0; j < num3; j++)
				{
					if (map2[num5] != -1)
					{
						flag3 = true;
						map[num12++] = ScanBits.EMPTY;
					}
					else if ((tileProperties[num5] & BaseLevelManager.TileProperty.TileMask) != BaseLevelManager.TileProperty.TileMask)
					{
						flag2 = true;
						map[num12++] = ScanBits.EMPTY;
					}
					else
					{
						map[num12++] = ScanBits.OCCUPIED | ScanBits.ADDED;
						flag = true;
						if (j < num8)
						{
							num8 = j;
						}
						if (j > num9)
						{
							num9 = j;
						}
						if (i < num10)
						{
							num10 = i;
						}
						if (i > num11)
						{
							num11 = i;
						}
					}
					num5++;
				}
				num5 += num6;
			}
			if (!flag)
			{
				if (flag3)
				{
					m_UIController.SetBrushError(BaseLevelManager.BrushError.eZoneOverAnotherZone, m_ZoneErrorExpireIn, bClearOnExpiry: true);
				}
				else if (flag2)
				{
					m_UIController.SetBrushError(BaseLevelManager.BrushError.eZoneOverEmptySpace, m_ZoneErrorExpireIn, bClearOnExpiry: true);
				}
				LevelEditor_FlashingMarquee.CreateFlashingMarquee(m_ZoneManager.m_FlashingErrorMarqueePrefab, map, num3, num, num2, layer, ScanBits.EMPTY);
				return false;
			}
			int num13 = -1;
			for (int k = 0; k < num7; k++)
			{
				if (map[k] != ScanBits.EMPTY)
				{
					num13 = k;
					break;
				}
			}
			FloodSetMap(ref map, num3, num13 % num3, num13 / num3, ScanBits.OCCUPIED, ScanBits.SCANISLAND);
			for (int l = 0; l < num7; l++)
			{
				if ((map[l] & ScanBits.OCCUPIED) == ScanBits.OCCUPIED && (map[l] & ScanBits.SCANISLAND) != ScanBits.SCANISLAND)
				{
					m_UIController.SetBrushError(BaseLevelManager.BrushError.eZoneNoIslands, m_ZoneErrorExpireIn, bClearOnExpiry: true);
					LevelEditor_FlashingMarquee.CreateFlashingMarquee(m_ZoneManager.m_FlashingErrorMarqueePrefab, map, num3, num, num2, layer, ScanBits.OCCUPIED | ScanBits.ADDED);
					return false;
				}
			}
			if (AreThereAnyHoles(ref map, num3))
			{
				m_UIController.SetBrushError(BaseLevelManager.BrushError.eZoneNoDoughnuts, m_ZoneErrorExpireIn, bClearOnExpiry: true);
				LevelEditor_FlashingMarquee.CreateFlashingMarquee(m_ZoneManager.m_SolidErrorMarqueePrefab, map, num3, num, num2, layer, ScanBits.OCCUPIED | ScanBits.SCANISLAND | ScanBits.ADDED);
				LevelEditor_FlashingMarquee.CreateFlashingMarquee(m_ZoneManager.m_FlashingErrorMarqueePrefab, map, num3, num, num2, layer, ScanBits.EMPTY);
				return false;
			}
			iOrriginalStartX += num8;
			iOrriginalWidth = num9 - num8 + 1;
			iOrriginalStartY += num10;
			iOrriginalHeight = num11 - num10 + 1;
			int num14 = (iOrriginalWidth * iOrriginalHeight + 7) / 8;
			zonePrint = new byte[num14];
			int num15 = 0;
			int num16 = 0;
			int num17 = num3 - iOrriginalWidth;
			int num18 = num3 * num10 + num8;
			for (int m = 0; m < iOrriginalHeight; m++)
			{
				for (int n = 0; n < iOrriginalWidth; n++)
				{
					if ((map[num18++] & ScanBits.OCCUPIED) == ScanBits.OCCUPIED)
					{
						zonePrint[num15] |= (byte)(1 << num16++);
					}
					else
					{
						zonePrint[num15] &= (byte)(~(1 << num16++));
					}
					if (num16 == 8)
					{
						num16 = 0;
						num15++;
					}
				}
				num18 += num17;
			}
			return true;
		}
		return false;
	}

	public void RemoteStopZoneEdit()
	{
		ExitZoneEditing();
	}

	public void LimitationGroupChanged(int iLimitGroup)
	{
		m_ZoneManager.LimitationGroupChanged(iLimitGroup);
	}
}
