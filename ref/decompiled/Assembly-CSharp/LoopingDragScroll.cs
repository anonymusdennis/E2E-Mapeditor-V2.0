using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class LoopingDragScroll : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IEventSystemHandler
{
	public delegate void DragEvent();

	[Tooltip("If false, will Init automatically, otherwise you need to call Init() method")]
	public bool InitByUser;

	private ScrollRect m_ScrollRect;

	private ContentSizeFitter m_ContentSizeFitter;

	private VerticalLayoutGroup m_VerticalLayoutGroup;

	private HorizontalLayoutGroup m_HorizontalLayoutGroup;

	private GridLayoutGroup m_GridLayoutGroup;

	private bool m_bIsVertical = true;

	private bool m_bIsHorizontal;

	private float m_DisableMarginX;

	private float m_DisableMarginY;

	private bool m_HasDisabledGridComponents;

	private List<RectTransform> m_Items = new List<RectTransform>();

	private Vector2 m_NewAnchoredPosition = Vector2.zero;

	public float m_Treshold = 100f;

	private int m_ItemCount;

	private float m_RecordOffsetX;

	private float m_RecordOffsetY;

	private GridElementSnap m_GridSnap;

	public DragEvent OnDragStart;

	public DragEvent OnDragEnd;

	public float m_ExtraSwitchPadding = -10f;

	[HideInInspector]
	public bool m_bBeingDragged;

	public GridElementSnap GridSnap => m_GridSnap;

	private void Awake()
	{
		m_GridSnap = GetComponentInChildren<GridElementSnap>();
		if (!InitByUser)
		{
			Init();
		}
	}

	public void Init()
	{
		if (GetComponent<ScrollRect>() != null)
		{
			m_ScrollRect = GetComponent<ScrollRect>();
			m_ScrollRect.onValueChanged.AddListener(OnScroll);
			m_ScrollRect.movementType = ScrollRect.MovementType.Unrestricted;
			for (int i = 0; i < m_ScrollRect.content.childCount; i++)
			{
				m_Items.Add(m_ScrollRect.content.GetChild(i).GetComponent<RectTransform>());
			}
			if (m_ScrollRect.content.GetComponent<VerticalLayoutGroup>() != null)
			{
				m_VerticalLayoutGroup = m_ScrollRect.content.GetComponent<VerticalLayoutGroup>();
			}
			if (m_ScrollRect.content.GetComponent<HorizontalLayoutGroup>() != null)
			{
				m_HorizontalLayoutGroup = m_ScrollRect.content.GetComponent<HorizontalLayoutGroup>();
			}
			if (m_ScrollRect.content.GetComponent<GridLayoutGroup>() != null)
			{
				m_GridLayoutGroup = m_ScrollRect.content.GetComponent<GridLayoutGroup>();
			}
			if (m_ScrollRect.content.GetComponent<ContentSizeFitter>() != null)
			{
				m_ContentSizeFitter = m_ScrollRect.content.GetComponent<ContentSizeFitter>();
			}
			m_bIsHorizontal = m_ScrollRect.horizontal;
			m_bIsVertical = m_ScrollRect.vertical;
			if (m_bIsHorizontal && m_bIsVertical)
			{
				Debug.LogError("UI_InfiniteScroll doesn't support scrolling in both directions, plase choose one direction (horizontal or vertical)");
			}
			m_ItemCount = m_ScrollRect.content.childCount;
		}
		else
		{
			Debug.LogError("UI_InfiniteScroll => No ScrollRect component found");
		}
	}

	private void DisableGridComponents()
	{
		if (m_bIsVertical)
		{
			m_RecordOffsetY = m_Items[0].GetComponent<RectTransform>().anchoredPosition.y - m_Items[1].GetComponent<RectTransform>().anchoredPosition.y;
			m_DisableMarginY = m_RecordOffsetY * (float)m_ItemCount / 2f;
			m_Treshold = m_RecordOffsetY / 2f;
		}
		if (m_bIsHorizontal)
		{
			m_RecordOffsetX = m_Items[1].GetComponent<RectTransform>().anchoredPosition.x - m_Items[0].GetComponent<RectTransform>().anchoredPosition.x;
			m_DisableMarginX = m_RecordOffsetX * (float)m_ItemCount / 2f;
			m_Treshold = m_RecordOffsetY / 2f;
		}
		if (m_VerticalLayoutGroup != null)
		{
			m_VerticalLayoutGroup.enabled = false;
		}
		if (m_HorizontalLayoutGroup != null)
		{
			m_HorizontalLayoutGroup.enabled = false;
		}
		if (m_ContentSizeFitter != null)
		{
			m_ContentSizeFitter.enabled = false;
		}
		if (m_GridLayoutGroup != null)
		{
			m_GridLayoutGroup.enabled = false;
		}
		m_HasDisabledGridComponents = true;
	}

	public void OnScroll(Vector2 pos)
	{
		if (!m_HasDisabledGridComponents)
		{
			DisableGridComponents();
		}
		for (int i = 0; i < m_Items.Count; i++)
		{
			if (m_bIsHorizontal)
			{
				if (m_ScrollRect.transform.InverseTransformPoint(m_Items[i].gameObject.transform.position).x > m_DisableMarginX + m_Treshold - m_ExtraSwitchPadding)
				{
					m_NewAnchoredPosition = m_Items[i].anchoredPosition;
					m_NewAnchoredPosition.x -= (float)m_ItemCount * m_RecordOffsetX;
					m_Items[i].anchoredPosition = m_NewAnchoredPosition;
					m_ScrollRect.content.GetChild(m_ItemCount - 1).transform.SetAsFirstSibling();
				}
				else if (m_ScrollRect.transform.InverseTransformPoint(m_Items[i].gameObject.transform.position).x < 0f - m_DisableMarginX + m_Treshold + m_ExtraSwitchPadding)
				{
					m_NewAnchoredPosition = m_Items[i].anchoredPosition;
					m_NewAnchoredPosition.x += (float)m_ItemCount * m_RecordOffsetX;
					m_Items[i].anchoredPosition = m_NewAnchoredPosition;
					m_ScrollRect.content.GetChild(0).transform.SetAsLastSibling();
				}
			}
			if (m_bIsVertical)
			{
				if (m_ScrollRect.transform.InverseTransformPoint(m_Items[i].gameObject.transform.position).y > m_DisableMarginY + m_Treshold)
				{
					m_NewAnchoredPosition = m_Items[i].anchoredPosition;
					m_NewAnchoredPosition.y -= (float)m_ItemCount * m_RecordOffsetY;
					m_Items[i].anchoredPosition = m_NewAnchoredPosition;
					m_ScrollRect.content.GetChild(m_ItemCount - 1).transform.SetAsFirstSibling();
				}
				else if (m_ScrollRect.transform.InverseTransformPoint(m_Items[i].gameObject.transform.position).y < 0f - m_DisableMarginY)
				{
					m_NewAnchoredPosition = m_Items[i].anchoredPosition;
					m_NewAnchoredPosition.y += (float)m_ItemCount * m_RecordOffsetY;
					m_Items[i].anchoredPosition = m_NewAnchoredPosition;
					m_ScrollRect.content.GetChild(0).transform.SetAsLastSibling();
				}
			}
		}
	}

	public void OnBeginDrag(PointerEventData data)
	{
		m_bBeingDragged = true;
		if (OnDragStart != null)
		{
			OnDragStart();
		}
	}

	public void OnEndDrag(PointerEventData data)
	{
		m_bBeingDragged = false;
		if (m_GridSnap != null)
		{
			Debug.Log("LoopingDragScroll OnEndDrag Called");
			m_GridSnap.SnapToNearest(m_ScrollRect);
		}
		if (OnDragEnd != null)
		{
			OnDragEnd();
		}
	}

	public void ResetForNewItems()
	{
		if (m_ScrollRect == null)
		{
			m_ScrollRect = GetComponent<ScrollRect>();
			if (m_ScrollRect == null)
			{
				return;
			}
		}
		if (m_VerticalLayoutGroup != null)
		{
			m_VerticalLayoutGroup.enabled = true;
		}
		else if (m_ScrollRect.content.GetComponent<VerticalLayoutGroup>() != null)
		{
			m_VerticalLayoutGroup = m_ScrollRect.content.GetComponent<VerticalLayoutGroup>();
			m_VerticalLayoutGroup.enabled = true;
		}
		if (m_HorizontalLayoutGroup != null)
		{
			m_HorizontalLayoutGroup.enabled = true;
		}
		else if (m_ScrollRect.content.GetComponent<HorizontalLayoutGroup>() != null)
		{
			m_HorizontalLayoutGroup = m_ScrollRect.content.GetComponent<HorizontalLayoutGroup>();
			m_HorizontalLayoutGroup.enabled = true;
		}
		if (m_ContentSizeFitter != null)
		{
			m_ContentSizeFitter.enabled = true;
		}
		else if (m_ScrollRect.content.GetComponent<ContentSizeFitter>() != null)
		{
			m_ContentSizeFitter = m_ScrollRect.content.GetComponent<ContentSizeFitter>();
			m_ContentSizeFitter.enabled = true;
		}
		if (m_GridLayoutGroup != null)
		{
			m_GridLayoutGroup.enabled = true;
		}
		else if (m_ScrollRect.content.GetComponent<GridLayoutGroup>() != null)
		{
			m_GridLayoutGroup = m_ScrollRect.content.GetComponent<GridLayoutGroup>();
			m_GridLayoutGroup.enabled = true;
		}
	}
}
