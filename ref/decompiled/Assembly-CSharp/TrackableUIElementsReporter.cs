using System;
using System.Collections.Generic;

public class TrackableUIElementsReporter : T17MonoBehaviour, IControlledUpdate
{
	private const int LIST_COUNT = 4;

	public float m_ProximityPenalty = 1f;

	public ProximityPriorityLayers m_ProximityLayer;

	protected Dictionary<CameraManager.PlayerBindingID, T17TrackedUIElement> m_TrackedUIElements = new Dictionary<CameraManager.PlayerBindingID, T17TrackedUIElement>(4);

	protected T17TrackedUIElement m_ToggleWorldTrackedUIElement;

	protected T17TrackedUIElement m_AlwaysVisibleWorldTrackedUIElement;

	private string m_NearNameplateText = string.Empty;

	private string m_FarAwayNameplateText = string.Empty;

	private CharacterIconHandler.IconType m_IconTypeOfOwner = CharacterIconHandler.IconType.MAX;

	private Func<bool> m_ShouldShowNearbyNameplatesDelegator;

	private Character m_CharacterOwner;

	private bool m_bAllowedVisible = true;

	private int m_LastKnownWorldFloorIndex = -1;

	private bool m_bAnyAttached;

	public Character CharacterOwner => m_CharacterOwner;

	public bool NeedsUpdate => m_bAnyAttached;

	protected override void Awake()
	{
		base.Awake();
		m_CharacterOwner = GetComponentInParent<Character>();
	}

	private void Start()
	{
		if (UpdateManager.GetInstance() != null)
		{
			UpdateManager.GetInstance().Register(this, UpdateCategory.UITrackedElementReporters);
		}
	}

	protected virtual void OnDestroy()
	{
		if (UpdateManager.GetInstance() != null)
		{
			UpdateManager.GetInstance().Unregister(this, UpdateCategory.UITrackedElementReporters);
		}
		foreach (KeyValuePair<CameraManager.PlayerBindingID, T17TrackedUIElement> trackedUIElement in m_TrackedUIElements)
		{
			T17TrackedUIElement value = trackedUIElement.Value;
			if (value.Flags != 0)
			{
				value.SafeDisableAll();
			}
		}
		m_TrackedUIElements.Clear();
		m_ToggleWorldTrackedUIElement = null;
		m_AlwaysVisibleWorldTrackedUIElement = null;
		m_CharacterOwner = null;
	}

	public void ControlledUpdate()
	{
		bool flag = m_CharacterOwner != null && m_CharacterOwner.CurrentFloor.m_FloorIndex != m_LastKnownWorldFloorIndex;
		if (m_AlwaysVisibleWorldTrackedUIElement != null && (m_AlwaysVisibleWorldTrackedUIElement.Flags == 0 || flag))
		{
			ReleaseTrackedElement(ref m_AlwaysVisibleWorldTrackedUIElement, wasAlwaysVisble: true);
		}
		if (m_ToggleWorldTrackedUIElement != null && (m_ToggleWorldTrackedUIElement.Flags == 0 || flag))
		{
			ReleaseTrackedElement(ref m_ToggleWorldTrackedUIElement, wasAlwaysVisble: false);
		}
	}

	public void ControlledFixedUpdate()
	{
	}

	public bool AttachUITrackedElement(ref T17TrackedUIElement element, bool isFarAway)
	{
		CameraManager.PlayerBindingID cameraBindingID = element.m_CameraBindingID;
		bool bAnyAttached = m_bAnyAttached;
		switch (cameraBindingID)
		{
		case CameraManager.PlayerBindingID.CM_PBID_WORLD_CAM_VISIBLE:
			SetElementsNameAndIcon(element, isFarAway);
			m_AlwaysVisibleWorldTrackedUIElement = element;
			m_bAnyAttached = true;
			break;
		case CameraManager.PlayerBindingID.CM_PBID_WORLD_CAM_TOGGLE:
			SetElementsNameAndIcon(element, isFarAway);
			m_ToggleWorldTrackedUIElement = element;
			m_bAnyAttached = true;
			break;
		default:
			if (!m_TrackedUIElements.ContainsKey(cameraBindingID))
			{
				SetElementsNameAndIcon(element, isFarAway);
				m_TrackedUIElements.Add(cameraBindingID, element);
				m_bAnyAttached = true;
			}
			else
			{
				SetElementsNameAndIcon(element, isFarAway);
				m_bAnyAttached = true;
			}
			break;
		}
		if (m_bAnyAttached && !bAnyAttached)
		{
			TrackedElementReporterUpdateController.GetInstance()?.AddToUpdateList(this);
		}
		return m_bAnyAttached;
	}

	private void SetElementsNameAndIcon(T17TrackedUIElement element, bool isFarAway)
	{
		string namePlateText = ((!isFarAway || string.IsNullOrEmpty(m_FarAwayNameplateText)) ? m_NearNameplateText : m_FarAwayNameplateText);
		element.SetNamePlateText(namePlateText);
		if (m_IconTypeOfOwner != CharacterIconHandler.IconType.MAX)
		{
			element.SetIconImage(m_IconTypeOfOwner);
		}
	}

	public T17TrackedUIElement GetUITrackedElement(CameraManager.PlayerBindingID cameraPlayerBindingID)
	{
		return cameraPlayerBindingID switch
		{
			CameraManager.PlayerBindingID.CM_PBID_WORLD_CAM_VISIBLE => m_AlwaysVisibleWorldTrackedUIElement, 
			CameraManager.PlayerBindingID.CM_PBID_WORLD_CAM_TOGGLE => m_ToggleWorldTrackedUIElement, 
			_ => (!m_TrackedUIElements.ContainsKey(cameraPlayerBindingID)) ? null : m_TrackedUIElements[cameraPlayerBindingID], 
		};
	}

	public bool RemoveTrackerUIElement(T17TrackedUIElement element)
	{
		CameraManager.PlayerBindingID cameraBindingID = element.m_CameraBindingID;
		switch (cameraBindingID)
		{
		case CameraManager.PlayerBindingID.CM_PBID_WORLD_CAM_VISIBLE:
			m_AlwaysVisibleWorldTrackedUIElement = null;
			CheckIfAnyAttached();
			return true;
		case CameraManager.PlayerBindingID.CM_PBID_WORLD_CAM_TOGGLE:
			m_ToggleWorldTrackedUIElement = null;
			CheckIfAnyAttached();
			return true;
		default:
			m_TrackedUIElements.Remove(cameraBindingID);
			CheckIfAnyAttached();
			return true;
		case CameraManager.PlayerBindingID.CM_PBID_UNSET:
			return false;
		}
	}

	private void CheckIfAnyAttached()
	{
		if (m_AlwaysVisibleWorldTrackedUIElement == null && m_ToggleWorldTrackedUIElement == null && m_TrackedUIElements.Count == 0)
		{
			m_bAnyAttached = false;
			TrackedElementReporterUpdateController.GetInstance()?.RemoveFromUpdateList(this);
		}
	}

	public void SetDisplayName(string name, bool localise = false)
	{
		if (!string.IsNullOrEmpty(name))
		{
			string localized = name;
			if (localise)
			{
				Localization.Get(name, out localized);
			}
			m_NearNameplateText = localized;
		}
	}

	public string GetCurrentDisplayName()
	{
		return m_NearNameplateText;
	}

	public void SetFarAwayDisplayName(string name, bool localise = false)
	{
		if (!string.IsNullOrEmpty(name))
		{
			string localized = name;
			if (localise)
			{
				Localization.Get(name, out localized);
			}
			m_FarAwayNameplateText = localized;
		}
		else
		{
			m_FarAwayNameplateText = null;
		}
	}

	public string GetCurrentFarAwayDisplayName()
	{
		return m_FarAwayNameplateText;
	}

	public void SetNameplateIcon(CharacterIconHandler.IconType iconType)
	{
		if (iconType == CharacterIconHandler.IconType.MAX || iconType < m_IconTypeOfOwner)
		{
			m_IconTypeOfOwner = iconType;
			UpdateTrackedElementsIcons();
		}
	}

	public void UnsetNameplateIcon(CharacterIconHandler.IconType iconType)
	{
		if (m_IconTypeOfOwner == iconType)
		{
			m_IconTypeOfOwner = CharacterIconHandler.IconType.MAX;
			UpdateTrackedElementsIcons();
		}
	}

	private void UpdateTrackedElementsIcons()
	{
		if (m_ToggleWorldTrackedUIElement != null)
		{
			m_ToggleWorldTrackedUIElement.SetIconImage(m_IconTypeOfOwner);
		}
		if (m_AlwaysVisibleWorldTrackedUIElement != null)
		{
			m_AlwaysVisibleWorldTrackedUIElement.SetIconImage(m_IconTypeOfOwner);
		}
		foreach (KeyValuePair<CameraManager.PlayerBindingID, T17TrackedUIElement> trackedUIElement in m_TrackedUIElements)
		{
			trackedUIElement.Value.SetIconImage(m_IconTypeOfOwner);
		}
	}

	public void UpdateTrackedElementPositions()
	{
		if (m_ToggleWorldTrackedUIElement != null && m_ToggleWorldTrackedUIElement.m_OwnerCanvas != null)
		{
			m_ToggleWorldTrackedUIElement.m_OwnerCanvas.UpdatePosition(m_ToggleWorldTrackedUIElement);
		}
		if (m_AlwaysVisibleWorldTrackedUIElement != null && m_AlwaysVisibleWorldTrackedUIElement.m_OwnerCanvas != null)
		{
			m_AlwaysVisibleWorldTrackedUIElement.m_OwnerCanvas.UpdatePosition(m_AlwaysVisibleWorldTrackedUIElement);
		}
	}

	public bool HasDisplayName()
	{
		return !string.IsNullOrEmpty(m_NearNameplateText);
	}

	public bool HasFarAwayNameplateText()
	{
		return !string.IsNullOrEmpty(m_FarAwayNameplateText);
	}

	public bool HasIcon()
	{
		return m_IconTypeOfOwner != CharacterIconHandler.IconType.MAX;
	}

	public void SetAllowedToRender(bool allowed)
	{
		m_bAllowedVisible = allowed;
		if (!m_bAllowedVisible)
		{
			ReleaseTrackedElement(ref m_AlwaysVisibleWorldTrackedUIElement, wasAlwaysVisble: true);
			ReleaseTrackedElement(ref m_ToggleWorldTrackedUIElement, wasAlwaysVisble: false);
		}
	}

	public T17TrackedUIElement AssignAlwaysVisibleWorldCanvasUIElement()
	{
		T17TrackedUIElement t17TrackedUIElement = AssignTrackedElement(ref m_AlwaysVisibleWorldTrackedUIElement, alwaysVisible: true);
		if (t17TrackedUIElement != null)
		{
			CameraManager.ForceSpeechBubbleShiftCheck();
			t17TrackedUIElement.m_SpeechBubble.ResetFacadeOffset();
		}
		return AssignTrackedElement(ref m_AlwaysVisibleWorldTrackedUIElement, alwaysVisible: true);
	}

	public T17TrackedUIElement AssignToggleWorldCanvasUIElement()
	{
		return AssignTrackedElement(ref m_ToggleWorldTrackedUIElement, alwaysVisible: false);
	}

	private T17TrackedUIElement AssignTrackedElement(ref T17TrackedUIElement elementToAssign, bool alwaysVisible)
	{
		if (!m_bAllowedVisible)
		{
			return null;
		}
		if (elementToAssign != null && m_CharacterOwner != null && m_CharacterOwner.CurrentFloor.m_FloorIndex != m_LastKnownWorldFloorIndex)
		{
			ReleaseTrackedElement(ref elementToAssign, alwaysVisible);
		}
		if (m_CharacterOwner != null)
		{
			m_LastKnownWorldFloorIndex = m_CharacterOwner.CurrentFloor.m_FloorIndex;
		}
		else
		{
			m_LastKnownWorldFloorIndex = FloorManager.GetInstance().FindFloorIndexAtZ(base.transform.position.z);
		}
		if (elementToAssign == null && HUDMenuFlow.Instance != null)
		{
			WorldCanvasTrackedUIElements uIElementsWorldCanvas = HUDMenuFlow.Instance.GetUIElementsWorldCanvas(m_LastKnownWorldFloorIndex);
			if (uIElementsWorldCanvas != null)
			{
				elementToAssign = uIElementsWorldCanvas.AttachFirstUnusedElementToReporter(this, alwaysVisible);
			}
		}
		return elementToAssign;
	}

	private void ReleaseTrackedElement(ref T17TrackedUIElement element, bool wasAlwaysVisble)
	{
		if (element != null && m_LastKnownWorldFloorIndex != -1)
		{
			WorldCanvasTrackedUIElements uIElementsWorldCanvas = HUDMenuFlow.Instance.GetUIElementsWorldCanvas(m_LastKnownWorldFloorIndex);
			if (uIElementsWorldCanvas != null)
			{
				element.SafeDisableAll();
				uIElementsWorldCanvas.ReleaseTrackedUIElement(element, wasAlwaysVisble);
				element = null;
			}
		}
	}

	public void SetDisplayFlagsForElement(T17TrackedUIElement element, bool isFarAway)
	{
		if (element == null)
		{
			return;
		}
		if (!isFarAway)
		{
			if (HasDisplayName() && ShouldShowNearbyNameplateToPlayer())
			{
				if (!element.HasFlag(1u))
				{
					element.EnableNamePlate();
				}
			}
			else if (element.HasFlag(1u))
			{
				element.DisableNamePlate();
			}
		}
		else if (HasFarAwayNameplateText())
		{
			if (!element.HasFlag(1u))
			{
				element.EnableNamePlate();
			}
		}
		else if (HasDisplayName())
		{
			if (!element.HasFlag(1u))
			{
				element.EnableNamePlate();
			}
		}
		else if (element.HasFlag(1u))
		{
			element.DisableNamePlate();
		}
		if (HasIcon())
		{
			if (!element.HasFlag(4u))
			{
				element.EnableIcon();
			}
		}
		else if (element.HasFlag(4u))
		{
			element.DisableIcon();
		}
	}

	public void SetProximityPriority(ProximityPriorityLayers layer, float penalty)
	{
		m_ProximityLayer = layer;
		m_ProximityPenalty = penalty;
	}

	public void ControlledLateUpdate()
	{
	}

	public void ControlledPreFixedUpdate()
	{
	}

	public void ControlledPreUpdate()
	{
	}

	public bool RequiresControlledUpdate()
	{
		return true;
	}

	public bool RequiresControlledFixedUpdate()
	{
		return false;
	}

	public bool RequiresControlledLateUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreFixedUpdate()
	{
		return false;
	}

	public void SetDelegatorForWhenNearbyNameplatesShouldShow(Func<bool> delegator)
	{
		m_ShouldShowNearbyNameplatesDelegator = delegator;
	}

	public bool ShouldShowNearbyNameplateToPlayer()
	{
		if (m_ShouldShowNearbyNameplatesDelegator == null)
		{
			return true;
		}
		return m_ShouldShowNearbyNameplatesDelegator();
	}
}
