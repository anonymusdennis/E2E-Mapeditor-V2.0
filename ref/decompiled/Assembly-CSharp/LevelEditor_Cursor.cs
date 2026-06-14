using System;
using UnityEngine;

public class LevelEditor_Cursor : MonoBehaviour
{
	[Serializable]
	public class CursorTypes
	{
		public enum CursorType
		{
			Nothing,
			Standard,
			Grabber,
			Delete,
			Marquee,
			Stamp,
			Paint,
			MarqueeLine,
			MovingBlock,
			Copy,
			Copy_Add,
			Copy_Delete,
			Copy_Edit,
			Zone_Add,
			Zone_Delete,
			Zone_Edit,
			InvalidCursor
		}

		public CursorType m_Type;

		public Texture2D m_Cursor_Texture;

		public Vector2 m_Cursor_HotSpot = Vector2.zero;
	}

	public delegate void BrushVisibilityChanged(bool bVisible);

	public delegate void ControlsVisibilityChanged(bool bVisible);

	public delegate void OverControlChanged(bool bOver);

	private static LevelEditor_Cursor m_Instance;

	public CursorTypes[] m_Cursors = new CursorTypes[0];

	private CursorTypes.CursorType m_CurrentCursorType;

	private CursorTypes.CursorType m_CurrentBlockCursorType = CursorTypes.CursorType.Paint;

	private int m_NumberOfControlsOver;

	private bool m_BrushVisible;

	private bool m_ControlsVisible;

	private bool m_OverControl;

	private float m_TimeToAnimOff;

	public float m_Move_HideDelay = 0.05f;

	public float m_Marquee_HideDelay = 0.2f;

	public float m_Delete_HideDelay = 0.3f;

	public float m_Freedraw_HideDelay = 0.1f;

	private bool m_bUpdateCursor = true;

	private LevelEditor_Controller m_CachedController;

	private LevelEditor_Controller.EditMode m_CurrentEditorMode;

	private BaseLevelManager m_LevelManager;

	private BaseLevelManager.LevelLayers m_CurrentLayer = BaseLevelManager.LevelLayers.TOTAL;

	private event BrushVisibilityChanged OnBrushVisibilityChanged;

	private event ControlsVisibilityChanged OnControlsVisibilityChanged;

	private event OverControlChanged OnOverControlChanged;

	public static LevelEditor_Cursor GetInstance()
	{
		return m_Instance;
	}

	private void Awake()
	{
		m_Instance = this;
	}

	protected virtual void OnDestroy()
	{
		if (this == m_Instance)
		{
			m_Instance = null;
			ShowHardwareCursor(CursorTypes.CursorType.Standard, bVisible: true);
		}
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (hasFocus)
		{
			m_bUpdateCursor = true;
		}
	}

	private void Start()
	{
		m_CachedController = LevelEditor_Controller.GetInstance();
		if (m_CachedController == null)
		{
			base.enabled = false;
			return;
		}
		m_CachedController.RegisterEditModeChange(EditModeChanged);
		m_CachedController.RegisterBlockChange(BlockChanged);
	}

	private void Update()
	{
		if (m_TimeToAnimOff != 0f && m_TimeToAnimOff < Time.timeSinceLevelLoad)
		{
			m_ControlsVisible = false;
			m_TimeToAnimOff = 0f;
			if (this.OnControlsVisibilityChanged != null)
			{
				this.OnControlsVisibilityChanged(m_ControlsVisible);
			}
		}
		if ((m_LevelManager != null || (m_LevelManager = BaseLevelManager.GetInstance()) != null) && m_LevelManager.m_CurrentLayer != m_CurrentLayer)
		{
			m_CurrentLayer = m_LevelManager.m_CurrentLayer;
			m_bUpdateCursor = true;
		}
		if (m_bUpdateCursor)
		{
			UpdateCursor();
		}
	}

	private void UpdateCursor()
	{
		m_bUpdateCursor = false;
		bool flag = false;
		bool flag2 = true;
		bool flag3 = false;
		float num = 0f;
		CursorTypes.CursorType cursorType = CursorTypes.CursorType.Standard;
		switch (m_CurrentEditorMode)
		{
		default:
			return;
		case LevelEditor_Controller.EditMode.INVALID:
			flag = true;
			flag2 = true;
			flag3 = false;
			cursorType = CursorTypes.CursorType.Standard;
			break;
		case LevelEditor_Controller.EditMode.NoBrush:
		case LevelEditor_Controller.EditMode.SelectingObjectInLevel:
		case LevelEditor_Controller.EditMode.SelectedObjectInLevel:
		case LevelEditor_Controller.EditMode.CopySelectedObjectInLevel:
		case LevelEditor_Controller.EditMode.Zone_Selected:
			flag = true;
			flag2 = true;
			flag3 = false;
			cursorType = CursorTypes.CursorType.Standard;
			break;
		case LevelEditor_Controller.EditMode.CopySelectedObjectInLevel_Edit:
			flag = true;
			flag2 = true;
			flag3 = false;
			cursorType = CursorTypes.CursorType.Copy_Edit;
			break;
		case LevelEditor_Controller.EditMode.BlockSelected:
			flag2 = true;
			flag3 = m_NumberOfControlsOver == 0;
			flag = true;
			cursorType = ((!flag3) ? CursorTypes.CursorType.Standard : m_CurrentBlockCursorType);
			break;
		case LevelEditor_Controller.EditMode.MovingBlock:
			flag2 = true;
			flag3 = m_NumberOfControlsOver == 0;
			flag = true;
			cursorType = ((!flag3) ? CursorTypes.CursorType.Standard : CursorTypes.CursorType.MovingBlock);
			break;
		case LevelEditor_Controller.EditMode.FreeDrawing:
			num = m_Freedraw_HideDelay;
			flag3 = true;
			flag = true;
			cursorType = CursorTypes.CursorType.Paint;
			break;
		case LevelEditor_Controller.EditMode.Marquee:
			num = m_Marquee_HideDelay;
			flag3 = false;
			flag = true;
			cursorType = CursorTypes.CursorType.Marquee;
			break;
		case LevelEditor_Controller.EditMode.MarqueeLine:
			num = m_Marquee_HideDelay;
			flag3 = false;
			flag = true;
			cursorType = CursorTypes.CursorType.MarqueeLine;
			break;
		case LevelEditor_Controller.EditMode.Deleting:
			num = m_Delete_HideDelay;
			flag3 = false;
			flag = true;
			cursorType = CursorTypes.CursorType.Delete;
			break;
		case LevelEditor_Controller.EditMode.MovingCamera:
			num = m_Move_HideDelay;
			flag3 = false;
			flag = true;
			cursorType = CursorTypes.CursorType.Grabber;
			break;
		case LevelEditor_Controller.EditMode.CopyMarquee:
			num = m_Marquee_HideDelay;
			flag3 = false;
			flag = true;
			cursorType = CursorTypes.CursorType.Copy;
			break;
		case LevelEditor_Controller.EditMode.CopyMarqueeAdd:
			num = m_Marquee_HideDelay;
			flag3 = false;
			flag = true;
			cursorType = CursorTypes.CursorType.Copy_Add;
			break;
		case LevelEditor_Controller.EditMode.CopyMarqueeDelete:
			num = m_Marquee_HideDelay;
			flag3 = false;
			flag = true;
			cursorType = CursorTypes.CursorType.Copy_Delete;
			break;
		case LevelEditor_Controller.EditMode.Zone_Editing:
			num = m_Marquee_HideDelay;
			flag3 = false;
			flag = true;
			cursorType = ((m_NumberOfControlsOver != 0) ? CursorTypes.CursorType.Standard : CursorTypes.CursorType.Zone_Edit);
			break;
		case LevelEditor_Controller.EditMode.Zone_Creating:
		case LevelEditor_Controller.EditMode.Zone_Adding:
			num = m_Marquee_HideDelay;
			flag3 = false;
			flag = true;
			cursorType = CursorTypes.CursorType.Zone_Add;
			break;
		case LevelEditor_Controller.EditMode.Zone_WaitingToCreate:
		{
			num = m_Marquee_HideDelay;
			flag3 = false;
			flag = true;
			if (m_NumberOfControlsOver != 0)
			{
				cursorType = CursorTypes.CursorType.Standard;
				break;
			}
			BaseLevelManager instance = BaseLevelManager.GetInstance();
			cursorType = ((!(instance != null) || (instance.m_CurrentLayer != BaseLevelManager.LevelLayers.FirstFloor_Vent && instance.m_CurrentLayer != BaseLevelManager.LevelLayers.GroundFloor_Vent && instance.m_CurrentLayer != BaseLevelManager.LevelLayers.Roof)) ? CursorTypes.CursorType.Zone_Add : CursorTypes.CursorType.InvalidCursor);
			break;
		}
		case LevelEditor_Controller.EditMode.Zone_Deleting:
			num = m_Marquee_HideDelay;
			flag3 = false;
			flag = true;
			cursorType = CursorTypes.CursorType.Zone_Delete;
			break;
		}
		if (flag2 != m_ControlsVisible || (flag2 && m_TimeToAnimOff != 0f))
		{
			if (flag2)
			{
				m_TimeToAnimOff = 0f;
				m_ControlsVisible = flag2;
				if (this.OnControlsVisibilityChanged != null)
				{
					this.OnControlsVisibilityChanged(flag2);
				}
			}
			else if (m_TimeToAnimOff == 0f)
			{
				m_TimeToAnimOff = Time.timeSinceLevelLoad + num;
			}
		}
		if (flag3 != m_BrushVisible)
		{
			m_BrushVisible = flag3;
			if (this.OnBrushVisibilityChanged != null)
			{
				this.OnBrushVisibilityChanged(flag3);
			}
		}
		if (m_OverControl != m_NumberOfControlsOver > 0)
		{
			m_OverControl = m_NumberOfControlsOver > 0;
			if (this.OnOverControlChanged != null)
			{
				this.OnOverControlChanged(m_OverControl);
			}
		}
		ShowHardwareCursor(cursorType, flag);
	}

	public void RegisterForBrushVisibityChange(BrushVisibilityChanged brushVisibilityChangedDelegate)
	{
		if (brushVisibilityChangedDelegate != null)
		{
			OnBrushVisibilityChanged += brushVisibilityChangedDelegate;
			brushVisibilityChangedDelegate(m_BrushVisible);
		}
	}

	public void RegisterForControlVisibityChange(ControlsVisibilityChanged controlVisibilityChangedDelegate)
	{
		if (controlVisibilityChangedDelegate != null)
		{
			OnControlsVisibilityChanged += controlVisibilityChangedDelegate;
			controlVisibilityChangedDelegate(m_ControlsVisible);
		}
	}

	public void RegisterForOverControllerChange(OverControlChanged overControlDelegate)
	{
		if (overControlDelegate != null)
		{
			OnOverControlChanged += overControlDelegate;
			overControlDelegate(m_OverControl);
		}
	}

	public void EnteringControlArea()
	{
		if (m_NumberOfControlsOver == 0)
		{
			m_bUpdateCursor = true;
		}
		m_NumberOfControlsOver++;
	}

	public void LeavingControlArea()
	{
		if (m_NumberOfControlsOver == 1)
		{
			m_bUpdateCursor = true;
		}
		m_NumberOfControlsOver--;
	}

	private void ShowHardwareCursor(CursorTypes.CursorType cursorType, bool bVisible)
	{
		if (cursorType != m_CurrentCursorType)
		{
			int num = m_Cursors.Length;
			for (int i = 0; i < num; i++)
			{
				if (m_Cursors[i] != null && m_Cursors[i].m_Type == cursorType && m_Cursors[i].m_Cursor_Texture != null)
				{
					Cursor.SetCursor(m_Cursors[i].m_Cursor_Texture, m_Cursors[i].m_Cursor_HotSpot, CursorMode.Auto);
					m_CurrentCursorType = cursorType;
					break;
				}
			}
		}
		if (Cursor.visible != bVisible)
		{
			Cursor.visible = bVisible;
		}
	}

	public void EditModeChanged(LevelEditor_Controller.EditMode newMode)
	{
		m_bUpdateCursor = true;
		m_CurrentEditorMode = newMode;
	}

	public void BlockChanged(int iNewBlock)
	{
		m_bUpdateCursor = true;
		BaseBuildingBlock block = BuildingBlockManager.GetBlock(iNewBlock);
		if (block == null)
		{
			m_CurrentBlockCursorType = CursorTypes.CursorType.Standard;
			return;
		}
		if (block.m_DrawingTool == BaseBuildingBlock.BuildingBlockDrawingMode.INVALID)
		{
			switch (block.BlockType)
			{
			case BaseBuildingBlock.BuildingBlockType.Decoration:
			case BaseBuildingBlock.BuildingBlockType.Object:
			case BaseBuildingBlock.BuildingBlockType.Complex:
			case BaseBuildingBlock.BuildingBlockType.Room:
				block.m_DrawingTool = BaseBuildingBlock.BuildingBlockDrawingMode.Stamp;
				break;
			case BaseBuildingBlock.BuildingBlockType.Wall:
				block.m_DrawingTool = BaseBuildingBlock.BuildingBlockDrawingMode.MarqueeLine;
				break;
			case BaseBuildingBlock.BuildingBlockType.Tile:
				block.m_DrawingTool = BaseBuildingBlock.BuildingBlockDrawingMode.Marquee;
				break;
			default:
				block.m_DrawingTool = BaseBuildingBlock.BuildingBlockDrawingMode.INVALID;
				break;
			}
		}
		switch (block.m_DrawingTool)
		{
		case BaseBuildingBlock.BuildingBlockDrawingMode.Marquee:
			m_CurrentBlockCursorType = CursorTypes.CursorType.Marquee;
			break;
		case BaseBuildingBlock.BuildingBlockDrawingMode.MarqueeLine:
			m_CurrentBlockCursorType = CursorTypes.CursorType.MarqueeLine;
			break;
		case BaseBuildingBlock.BuildingBlockDrawingMode.Paint:
			m_CurrentBlockCursorType = CursorTypes.CursorType.Paint;
			break;
		case BaseBuildingBlock.BuildingBlockDrawingMode.Stamp:
			m_CurrentBlockCursorType = CursorTypes.CursorType.Stamp;
			break;
		default:
			m_CurrentBlockCursorType = CursorTypes.CursorType.Standard;
			break;
		}
	}
}
