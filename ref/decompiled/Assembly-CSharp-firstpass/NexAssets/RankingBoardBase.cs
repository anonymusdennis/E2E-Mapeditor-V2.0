using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NexAssets;

public abstract class RankingBoardBase : MonoBehaviour
{
	public enum SIZE_TYPE
	{
		PIXEL,
		PERCENTAGE
	}

	public enum POSITION_TYPE
	{
		ALIGN,
		INPUT
	}

	public enum POSITION_ALIGN_V
	{
		TOP,
		CENTER,
		BOTTOM
	}

	public enum POSITION_ALIGN_H
	{
		LEFT,
		CENTER,
		RIGHT
	}

	[Serializable]
	public struct COLUMN_ARG
	{
		public int indexType;

		public int priority;

		public int param;

		public int size;

		public string title;

		public bool disp;

		public COLUMN_ARG(int indexType_, string title_)
		{
			title = title_;
			indexType = indexType_;
			priority = 0;
			size = 48;
			disp = false;
			param = 0;
		}
	}

	[Serializable]
	public class RankingBoardParam
	{
		public bool GuiFold;

		public GUISkin GuiSkin;

		public string ScoreboardTitle = "title";

		public int TitleHeight = 24;

		public SIZE_TYPE GuiSizeType;

		public POSITION_TYPE GuiPosType = POSITION_TYPE.INPUT;

		public POSITION_ALIGN_V GuiPosAlign_v = POSITION_ALIGN_V.CENTER;

		public POSITION_ALIGN_H GuiPosAlign_h = POSITION_ALIGN_H.CENTER;

		public int GuiPosMargin_v;

		public int GuiPosMargin_h;

		public Vector2 RankingBoardPosition = new Vector2(0f, 0f);

		public Vector2 RankingBoardSize = new Vector2(320f, 240f);

		public Vector2 RankingBoardPercentage = new Vector2(100f, 100f);
	}

	protected const int MENU_WIDTH = 320;

	protected const int MENU_HEIGHT = 240;

	protected const int COLUMN_SIZE = 48;

	protected const float GUI_BASE_ALPHA = 1f;

	private Image m_RankingPanel;

	private Image m_RankingNoneDataPanel;

	private Image m_HeaderScrollArea;

	private Image m_HeaderScrollPanel;

	private Image m_ScrollArea;

	private Image m_ScrollPanel;

	private Scrollbar m_VScrollbar;

	private Scrollbar m_HScrollbar;

	private GameObject[] m_HeaderObj;

	private GameObject[] m_CellObj;

	protected List<COLUMN_ARG> m_ColumnList = new List<COLUMN_ARG>();

	public RankingBoardParam m_RankingBoardParam = new RankingBoardParam();

	private Image RankingPanel
	{
		get
		{
			if (m_RankingPanel == null)
			{
				m_RankingPanel = GameObject.Find(base.name + "/Panel").GetComponent<Image>();
			}
			return m_RankingPanel;
		}
	}

	private Image RankingNoneDataPanel
	{
		get
		{
			if (m_RankingNoneDataPanel == null)
			{
				m_RankingNoneDataPanel = GameObject.Find(base.name + "/NoneDataPanel").GetComponent<Image>();
			}
			return m_RankingNoneDataPanel;
		}
	}

	private Image HeaderScrollArea
	{
		get
		{
			if (m_HeaderScrollArea == null)
			{
				m_HeaderScrollArea = GameObject.Find(base.name + "/Panel/HeaderScrollArea").GetComponent<Image>();
			}
			return m_HeaderScrollArea;
		}
	}

	private Image HeaderScrollPanel
	{
		get
		{
			if (m_HeaderScrollPanel == null)
			{
				m_HeaderScrollPanel = GameObject.Find(base.name + "/Panel/HeaderScrollArea/ScrollPanel").GetComponent<Image>();
			}
			return m_HeaderScrollPanel;
		}
	}

	private Image ScrollArea
	{
		get
		{
			if (m_ScrollArea == null)
			{
				m_ScrollArea = GameObject.Find(base.name + "/Panel/ScrollArea").GetComponent<Image>();
			}
			return m_ScrollArea;
		}
	}

	private Image ScrollPanel
	{
		get
		{
			if (m_ScrollPanel == null)
			{
				m_ScrollPanel = GameObject.Find(base.name + "/Panel/ScrollArea/ScrollPanel").GetComponent<Image>();
			}
			return m_ScrollPanel;
		}
	}

	private Scrollbar VScrollbar
	{
		get
		{
			if (m_VScrollbar == null)
			{
				m_VScrollbar = GameObject.Find(base.name + "/Panel/VScrollbar").GetComponent<Scrollbar>();
			}
			return m_VScrollbar;
		}
	}

	private Scrollbar HScrollbar
	{
		get
		{
			if (m_HScrollbar == null)
			{
				m_HScrollbar = GameObject.Find(base.name + "/Panel/HScrollbar").GetComponent<Scrollbar>();
			}
			return m_HScrollbar;
		}
	}

	private GameObject[] HeaderObj
	{
		get
		{
			if (m_HeaderObj == null)
			{
				m_HeaderObj = GameObject.FindGameObjectsWithTag(HeaderTextTag);
			}
			return m_HeaderObj;
		}
	}

	private GameObject[] CellObj
	{
		get
		{
			if (m_CellObj == null)
			{
				m_CellObj = GameObject.FindGameObjectsWithTag(ParamTextTag);
			}
			return m_CellObj;
		}
	}

	public float VScroll
	{
		get
		{
			return VScrollbar.GetComponentInChildren<Scrollbar>().value;
		}
		set
		{
			VScrollbar.GetComponentInChildren<Scrollbar>().value = Math.Min(Math.Max(0f, value), 1f);
		}
	}

	public float HScroll
	{
		get
		{
			return HScrollbar.GetComponentInChildren<Scrollbar>().value;
		}
		set
		{
			HScrollbar.GetComponentInChildren<Scrollbar>().value = Math.Min(Math.Max(0f, value), 1f);
		}
	}

	protected abstract string HeaderTextTag { get; }

	protected abstract string ParamTextTag { get; }

	protected void Start()
	{
		CreateColumnIndex();
		SetRankingBoardEnable(enable: false);
	}

	public void SetRankingBoardEnable(bool enable)
	{
		if (!enable)
		{
			RankingPanel.GetComponent<CanvasRenderer>().SetAlpha(0f);
			RankingNoneDataPanel.GetComponent<CanvasRenderer>().SetAlpha(0f);
		}
		else if (GetDataCount() == 0)
		{
			RankingPanel.GetComponent<CanvasRenderer>().SetAlpha(0f);
			RankingNoneDataPanel.GetComponent<CanvasRenderer>().SetAlpha(1f);
		}
		else
		{
			RankingPanel.GetComponent<CanvasRenderer>().SetAlpha(1f);
			RankingNoneDataPanel.GetComponent<CanvasRenderer>().SetAlpha(0f);
		}
	}

	protected abstract void CreateColumnIndex();

	protected abstract string GetContent(COLUMN_ARG columnArg);

	protected abstract int GetDataCount();

	protected void CreateColumn(COLUMN_ARG columnArg)
	{
		bool flag = false;
		for (int i = 0; i < m_ColumnList.Count; i++)
		{
			if (m_ColumnList[i].priority > columnArg.priority)
			{
				m_ColumnList.Insert(i, columnArg);
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			m_ColumnList.Add(columnArg);
		}
	}

	protected void RankingBoard(bool enable = true)
	{
		float num = Screen.width;
		float num2 = Screen.height;
		RectTransform component = GetComponent<RectTransform>();
		Vector3 localScale = component.localScale;
		num /= localScale.x;
		num2 /= localScale.y;
		CreateColumnIndex();
		SetRankingBoardEnable(enable);
		if (!enable || GetDataCount() == 0)
		{
			return;
		}
		if (m_RankingBoardParam.GuiSkin != null)
		{
			GUI.skin = m_RankingBoardParam.GuiSkin;
		}
		float x = 0f;
		float y = 0f;
		float num3 = 0f;
		float num4 = 0f;
		if (m_RankingBoardParam.GuiSizeType == SIZE_TYPE.PIXEL)
		{
			num3 = m_RankingBoardParam.RankingBoardSize.x;
			num4 = m_RankingBoardParam.RankingBoardSize.y;
		}
		else if (m_RankingBoardParam.GuiSizeType == SIZE_TYPE.PERCENTAGE)
		{
			num3 = num * (m_RankingBoardParam.RankingBoardPercentage.x / 100f);
			num4 = num2 * (m_RankingBoardParam.RankingBoardPercentage.y / 100f);
		}
		if (m_RankingBoardParam.GuiPosType == POSITION_TYPE.ALIGN)
		{
			if (m_RankingBoardParam.GuiPosAlign_v == POSITION_ALIGN_V.TOP)
			{
				y = -m_RankingBoardParam.GuiPosMargin_v;
			}
			else if (m_RankingBoardParam.GuiPosAlign_v == POSITION_ALIGN_V.CENTER)
			{
				y = (0f - (num2 - num4)) / 2f - (float)m_RankingBoardParam.GuiPosMargin_v;
			}
			else if (m_RankingBoardParam.GuiPosAlign_v == POSITION_ALIGN_V.BOTTOM)
			{
				y = 0f - (num2 - num4) + (float)m_RankingBoardParam.GuiPosMargin_v;
			}
			if (m_RankingBoardParam.GuiPosAlign_h == POSITION_ALIGN_H.LEFT)
			{
				x = m_RankingBoardParam.GuiPosMargin_h;
			}
			else if (m_RankingBoardParam.GuiPosAlign_h == POSITION_ALIGN_H.CENTER)
			{
				x = num / 2f - num3 / 2f + (float)m_RankingBoardParam.GuiPosMargin_h;
			}
			else if (m_RankingBoardParam.GuiPosAlign_h == POSITION_ALIGN_H.RIGHT)
			{
				x = num - num3 - (float)m_RankingBoardParam.GuiPosMargin_h;
			}
		}
		else
		{
			x = m_RankingBoardParam.RankingBoardPosition.x;
			y = 0f - m_RankingBoardParam.RankingBoardPosition.y;
		}
		RectTransform component2 = RankingPanel.GetComponent<RectTransform>();
		component2.sizeDelta = new Vector2(num3, num4);
		component2.anchoredPosition = new Vector2(x, y);
		Text componentInChildren = GameObject.Find(base.name + "/Panel/ScoreboardTitle").GetComponentInChildren<Text>();
		componentInChildren.text = m_RankingBoardParam.ScoreboardTitle;
		float num5 = 5f;
		float num6 = 0f;
		float num7 = 0f;
		for (int i = 0; i < HeaderObj.Length; i++)
		{
			HeaderObj[i].GetComponentInChildren<Text>().text = string.Empty;
		}
		for (int j = 0; j < CellObj.Length; j++)
		{
			CellObj[j].GetComponentInChildren<Text>().text = string.Empty;
		}
		for (int k = 0; k < m_ColumnList.Count; k++)
		{
			if (m_ColumnList[k].disp)
			{
				Text componentInChildren2 = HeaderObj[k].GetComponentInChildren<Text>();
				Text componentInChildren3 = CellObj[k].GetComponentInChildren<Text>();
				componentInChildren2.text = m_ColumnList[k].title;
				num7 = 16f;
				component2 = componentInChildren2.GetComponent<RectTransform>();
				component2.sizeDelta = new Vector2(m_ColumnList[k].size, 0f);
				component2.anchoredPosition = new Vector2(num5, 0f);
				component2 = componentInChildren3.GetComponent<RectTransform>();
				component2.sizeDelta = new Vector2(m_ColumnList[k].size, 0f);
				component2.anchoredPosition = new Vector2(num5, 0f);
				num5 += (float)m_ColumnList[k].size;
				componentInChildren3.text = GetContent(m_ColumnList[k]).Trim();
				num6 = Math.Max(num6, componentInChildren3.preferredHeight);
			}
		}
		component2 = HeaderScrollArea.GetComponent<RectTransform>();
		component2.sizeDelta = new Vector2(num3 - VScrollbar.GetComponent<RectTransform>().sizeDelta.x, num7);
		component2.anchoredPosition = new Vector2(0f, -m_RankingBoardParam.TitleHeight);
		component2 = HeaderScrollPanel.GetComponent<RectTransform>();
		component2.sizeDelta = new Vector2(Math.Max(num3 - VScrollbar.GetComponent<RectTransform>().sizeDelta.x, num5), num7);
		component2 = ScrollArea.GetComponent<RectTransform>();
		component2.sizeDelta = new Vector2(num3 - VScrollbar.GetComponent<RectTransform>().sizeDelta.x, num4 - (float)m_RankingBoardParam.TitleHeight - num7 - HScrollbar.GetComponent<RectTransform>().sizeDelta.y);
		component2.anchoredPosition = new Vector2(0f, (float)(-m_RankingBoardParam.TitleHeight) - num7);
		component2 = ScrollPanel.GetComponent<RectTransform>();
		component2.sizeDelta = new Vector2(Math.Max(num3 - VScrollbar.GetComponent<RectTransform>().sizeDelta.x, num5), Math.Max(num4 - (float)m_RankingBoardParam.TitleHeight - num7 - HScrollbar.GetComponent<RectTransform>().sizeDelta.y, num6));
		component2 = VScrollbar.GetComponent<RectTransform>();
		component2.sizeDelta = new Vector2(component2.sizeDelta.x, (float)(-m_RankingBoardParam.TitleHeight) - num7 - HScrollbar.GetComponent<RectTransform>().sizeDelta.y);
		component2.anchoredPosition = new Vector2(0f, HScrollbar.GetComponent<RectTransform>().sizeDelta.y);
	}
}
