using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WorldCanvasTrackedUIElements : MonoBehaviour
{
	public int m_PoolSize = 150;

	public GameObject m_TrackedItemPrefab;

	public Transform m_TrackedItemsParent;

	public Vector2 m_TrackedElementOffsetFromObject = new Vector2(0f, 0.8f);

	public Canvas m_VisibleWorldSpaceCanvas;

	public Canvas m_ToggledWorldSpaceCanvas;

	[SerializeField]
	private T17TrackedUIElement[] m_TrackedItemPool;

	private List<T17TrackedUIElement> m_UsedToggleElements;

	private RectTransform m_ToggledWorldSpaceCanvasRect;

	private List<T17TrackedUIElement> m_UsedAlwaysVisibleElements;

	private RectTransform m_VisibleWorldSpaceCanvasRect;

	private int m_AnyUsed;

	private int m_Global_Inside;

	private static List<Character> m_AllCharacters;

	private int m_LastUsedIndex;

	public const int WORLD_CAM_UNUSED = 333;

	public const int WORLD_CAM_TOGGLE = 666;

	public const int WORLD_CAM_VISIBLE = 999;

	private CameraManager.PlayerBindingID[] m_CamerasInUse = new CameraManager.PlayerBindingID[8];

	private float m_RecheckTime = 0.2f;

	private float m_ElapsedRecheckTime;

	private void Awake()
	{
		if (m_VisibleWorldSpaceCanvas != null)
		{
			m_VisibleWorldSpaceCanvasRect = m_VisibleWorldSpaceCanvas.GetComponent<RectTransform>();
		}
		if (m_ToggledWorldSpaceCanvas != null)
		{
			m_ToggledWorldSpaceCanvasRect = m_ToggledWorldSpaceCanvas.GetComponent<RectTransform>();
		}
		m_Global_Inside = LayerMask.NameToLayer("Global_Inside_TrackedTags");
		m_ToggledWorldSpaceCanvas.gameObject.layer = m_Global_Inside;
	}

	public static void Cleanup()
	{
		if (m_AllCharacters != null)
		{
			m_AllCharacters.Clear();
			m_AllCharacters = null;
		}
	}

	public void GeneratePool()
	{
		m_UsedToggleElements = new List<T17TrackedUIElement>();
		m_UsedAlwaysVisibleElements = new List<T17TrackedUIElement>();
		m_TrackedItemPool = GetComponentsInChildren<T17TrackedUIElement>(includeInactive: true);
		if (m_TrackedItemPool == null || m_TrackedItemPool.Length == 0)
		{
			m_TrackedItemPool = new T17TrackedUIElement[m_PoolSize];
			for (int i = 0; i < m_PoolSize; i++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(m_TrackedItemPrefab);
				gameObject.transform.SetParent(m_TrackedItemsParent, worldPositionStays: true);
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localScale = Vector3.one;
				gameObject.name = base.gameObject.name + "_TrIt_" + i;
				gameObject.SetActive(value: false);
				T17TrackedUIElement componentInChildren = gameObject.GetComponentInChildren<T17TrackedUIElement>(includeInactive: true);
				componentInChildren.m_OwnerCanvas = this;
				m_TrackedItemPool[i] = componentInChildren;
			}
		}
		else
		{
			m_AnyUsed = 0;
			for (int j = 0; j < m_PoolSize; j++)
			{
				m_TrackedItemPool[j].ResetAll();
				m_TrackedItemPool[j].gameObject.SetActive(value: false);
				m_TrackedItemPool[j].m_OwnerCanvas = this;
			}
		}
	}

	public void PreGeneratePoolFixUp()
	{
		m_UsedToggleElements = new List<T17TrackedUIElement>();
		m_UsedAlwaysVisibleElements = new List<T17TrackedUIElement>();
	}

	private void Update()
	{
		if (m_AnyUsed <= 0)
		{
			return;
		}
		for (int i = 0; i < m_UsedToggleElements.Count; i++)
		{
			if (m_UsedToggleElements[i].Flags != 0 && m_UsedToggleElements[i].AttachedTo != null)
			{
				UpdatePosition(m_UsedToggleElements[i]);
			}
		}
		for (int j = 0; j < m_UsedAlwaysVisibleElements.Count; j++)
		{
			if (m_UsedAlwaysVisibleElements[j].Flags != 0 && m_UsedAlwaysVisibleElements[j].AttachedTo != null)
			{
				UpdatePosition(m_UsedAlwaysVisibleElements[j]);
			}
		}
		if (m_ElapsedRecheckTime >= m_RecheckTime)
		{
			m_ElapsedRecheckTime = 0f;
			RecheckCurrentToggledCameras();
		}
		m_ElapsedRecheckTime += UpdateManager.deltaTime;
	}

	private void RecheckCurrentToggledCameras()
	{
		for (int i = 0; i < m_CamerasInUse.Length; i++)
		{
			if (m_CamerasInUse[i] != 0)
			{
				EnableAllNameplatesForCamera(m_CamerasInUse[i]);
			}
		}
	}

	public void EnableAllNameplatesForCamera(CameraManager.PlayerBindingID playerBindingID)
	{
		m_CamerasInUse[(int)playerBindingID] = playerBindingID;
		Camera camera = CameraManager.GetInstance().GetCamera(playerBindingID);
		int count = m_AllCharacters.Count;
		for (int i = 0; i < count; i++)
		{
			T17TrackedUIElement t17TrackedUIElement = m_AllCharacters[i].m_TrackableElementReporter.AssignToggleWorldCanvasUIElement();
			if (t17TrackedUIElement != null)
			{
				m_AllCharacters[i].m_TrackableElementReporter.SetDisplayFlagsForElement(t17TrackedUIElement, isFarAway: false);
			}
		}
		camera.cullingMask |= 1 << m_Global_Inside;
	}

	public void DisableAllNameplatesForCamera(CameraManager.PlayerBindingID playerBindingID)
	{
		if (CameraManager.GetInstance() == null || m_CamerasInUse == null)
		{
			return;
		}
		if (playerBindingID >= CameraManager.PlayerBindingID.CM_PBID_UNSET && (int)playerBindingID < m_CamerasInUse.Length)
		{
			m_CamerasInUse[(int)playerBindingID] = CameraManager.PlayerBindingID.CM_PBID_UNSET;
			Camera camera = CameraManager.GetInstance().GetCamera(playerBindingID);
			if (camera != null)
			{
				camera.cullingMask &= ~(1 << m_Global_Inside);
			}
		}
	}

	private T17TrackedUIElement GetFirstUnusedElement()
	{
		bool flag = true;
		bool flag2 = false;
		int lastUsedIndex = m_LastUsedIndex;
		while (flag)
		{
			if (m_LastUsedIndex == lastUsedIndex && flag2)
			{
				flag = false;
			}
			if (m_LastUsedIndex >= m_PoolSize - 1)
			{
				m_LastUsedIndex = 0;
				flag2 = true;
			}
			if (m_TrackedItemPool[m_LastUsedIndex].AttachedTo == null)
			{
				return m_TrackedItemPool[m_LastUsedIndex];
			}
			m_LastUsedIndex++;
		}
		return null;
	}

	public T17TrackedUIElement AttachFirstUnusedElementToReporter(TrackableUIElementsReporter reporter, bool alwaysVisible)
	{
		if (reporter == null)
		{
			return null;
		}
		CameraManager.PlayerBindingID playerBindingID = ((!alwaysVisible) ? CameraManager.PlayerBindingID.CM_PBID_WORLD_CAM_TOGGLE : CameraManager.PlayerBindingID.CM_PBID_WORLD_CAM_VISIBLE);
		T17TrackedUIElement uITrackedElement = reporter.GetUITrackedElement(playerBindingID);
		if (uITrackedElement != null)
		{
			return uITrackedElement;
		}
		uITrackedElement = GetFirstUnusedElement();
		if (uITrackedElement != null)
		{
			uITrackedElement.ResetAll();
			if (alwaysVisible)
			{
				uITrackedElement.transform.SetParent(m_VisibleWorldSpaceCanvasRect, worldPositionStays: true);
				uITrackedElement.transform.localScale = Vector3.one;
			}
			else
			{
				uITrackedElement.transform.SetParent(m_ToggledWorldSpaceCanvasRect, worldPositionStays: true);
				uITrackedElement.transform.localScale = Vector3.one;
			}
			uITrackedElement.gameObject.SetActive(value: true);
			uITrackedElement.m_CameraBindingID = playerBindingID;
			uITrackedElement.SetAttachedToReporter(reporter);
			uITrackedElement.SetIsPlateFarAway(isFarAway: false);
			UpdatePosition(uITrackedElement);
			m_AnyUsed++;
		}
		if (uITrackedElement == null)
		{
			return null;
		}
		reporter.AttachUITrackedElement(ref uITrackedElement, isFarAway: false);
		if (alwaysVisible)
		{
			m_UsedAlwaysVisibleElements.Add(uITrackedElement);
		}
		else
		{
			m_UsedToggleElements.Add(uITrackedElement);
		}
		return uITrackedElement;
	}

	public bool ReleaseTrackedUIElement(T17TrackedUIElement element, bool wasAlwaysVisible)
	{
		for (int i = 0; i < m_PoolSize; i++)
		{
			if (m_TrackedItemPool[i] == element)
			{
				if (m_TrackedItemPool[i].Flags == 0)
				{
					m_TrackedItemPool[i].ResetAll();
					m_TrackedItemPool[i].transform.SetParent(m_TrackedItemsParent, worldPositionStays: true);
					m_TrackedItemPool[i].gameObject.SetActive(value: false);
					if (wasAlwaysVisible)
					{
						m_UsedAlwaysVisibleElements.Remove(element);
					}
					else
					{
						m_UsedToggleElements.Remove(element);
					}
					m_AnyUsed--;
					if (m_AnyUsed < 0)
					{
					}
					return true;
				}
				return false;
			}
		}
		return false;
	}

	public void UpdatePosition(T17TrackedUIElement element)
	{
		Vector3 attachedPosition = element.AttachedPosition;
		attachedPosition.z = element.transform.position.z;
		attachedPosition.y += m_TrackedElementOffsetFromObject.y;
		attachedPosition.x += m_TrackedElementOffsetFromObject.x;
		element.transform.position = attachedPosition;
	}

	public static void RegisterCharacter(Character character)
	{
		if (m_AllCharacters == null)
		{
			m_AllCharacters = new List<Character>(50);
		}
		m_AllCharacters.Add(character);
	}

	public static void UnRegisterCharacter(Character character)
	{
		if (m_AllCharacters == null)
		{
			m_AllCharacters = new List<Character>(50);
		}
		m_AllCharacters.Remove(character);
	}

	public List<T17TrackedUIElement> GetUsedAlwaysVisibleElements()
	{
		return m_UsedAlwaysVisibleElements;
	}
}
