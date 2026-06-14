using Rewired;

public class InputMapData
{
	public delegate void InputMapDataDelegate(InputMapData mapData);

	public InputMapDataDelegate OnInformationUpdated;

	public Rewired.Player m_RewiredPlayer;

	public ControllerMap m_ControllerMap;

	public ControllerType m_ControllerType = ControllerType.Custom;

	public ControllerPollingInfo m_PollingInfo;

	public int m_ControllerId = -1;

	public bool m_bIsAxisAction;

	public string m_AxisSuffix = string.Empty;

	public string m_ActionName = string.Empty;

	public ActionElementMap m_ActionElementMap;

	public bool m_bDataUpdated;

	private int m_InputMapDataID = -1;

	public int InputMapDataID => m_InputMapDataID;

	public AxisRange PolarisedAxisRange
	{
		get
		{
			AxisRange result = AxisRange.Positive;
			if (m_PollingInfo.elementType == ControllerElementType.Axis)
			{
				result = ((m_ActionElementMap.axisRange != 0) ? ((m_PollingInfo.axisPole == Pole.Positive) ? AxisRange.Positive : AxisRange.Negative) : AxisRange.Full);
			}
			return result;
		}
	}

	public InputMapData(int uniqueID)
	{
		m_InputMapDataID = uniqueID;
	}

	public InputMapData(int uniqueID, Rewired.Player rewiredPlayer, string actionName, ControllerMap controllerMap, ActionElementMap actionElementMap, ControllerType controllerType, bool bIsAxisAction, string negativeAxisSuffix = "-", string positiveAxisSuffix = "+")
	{
		m_InputMapDataID = uniqueID;
		SetMapInfo(rewiredPlayer, actionName, controllerMap, actionElementMap, controllerType, bIsAxisAction, negativeAxisSuffix, positiveAxisSuffix);
	}

	public void SetMapInfo(Rewired.Player rewiredPlayer, string actionName, ControllerMap controllerMap, ActionElementMap actionElementMap, ControllerType controllerType, bool bIsAxisAction, string negativeAxisSuffix = "-", string positiveAxisSuffix = "+")
	{
		m_RewiredPlayer = rewiredPlayer;
		m_ActionName = actionName;
		m_ControllerMap = controllerMap;
		m_ControllerId = controllerMap.id;
		m_ActionElementMap = actionElementMap;
		m_ControllerType = controllerType;
		m_bIsAxisAction = bIsAxisAction;
		m_AxisSuffix = string.Empty;
		if (bIsAxisAction)
		{
			if (m_ActionElementMap.axisContribution == Pole.Positive)
			{
				m_AxisSuffix = positiveAxisSuffix;
			}
			else if (m_ActionElementMap.axisContribution == Pole.Negative)
			{
				m_AxisSuffix = negativeAxisSuffix;
			}
		}
		if (OnInformationUpdated != null)
		{
			OnInformationUpdated(this);
		}
	}
}
