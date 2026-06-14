using UnityEngine;

public class MapPinComponent : T17MonoBehaviour
{
	public Sprite m_MapSprite;

	public bool m_bForMainMap = true;

	public bool m_bForMiniMap = true;

	public bool m_bUpdatePosition;

	public PinManager.Pin.PinFilterType m_FilterType;

	public bool m_bEdgable;

	public bool m_bFloorTrackable;

	public bool m_bDirectional;

	[Localization]
	public string m_ToolTipTag = string.Empty;

	public bool m_LocalizeToolTipTag = true;

	public bool m_bOverrideIconScale;

	public Vector3 m_OverrideIconScale = default(Vector3);

	public SpriteAnimation m_SpriteAnimation;

	public Vector3 m_StaticTargetPosition = default(Vector3);

	private int m_PinID = -1;

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		PinManager instance = PinManager.GetInstance();
		if (instance != null)
		{
			PinManager instance2 = PinManager.GetInstance();
			bool bForMainMap = m_bForMainMap;
			bool bForMiniMap = m_bForMiniMap;
			GameObject target = base.gameObject;
			Sprite mapSprite = m_MapSprite;
			bool bUpdatePosition = m_bUpdatePosition;
			PinManager.Pin.PinFilterType filterType = m_FilterType;
			bool bEdgable = m_bEdgable;
			m_PinID = instance2.CreatePin(bForMainMap, bForMiniMap, target, mapSprite, bUpdatePosition, null, null, filterType, bEdgable, m_bFloorTrackable, m_bDirectional, m_ToolTipTag, m_LocalizeToolTipTag, m_bOverrideIconScale, m_OverrideIconScale, m_SpriteAnimation, m_StaticTargetPosition);
		}
		return base.StartInit();
	}

	protected virtual void OnDestroy()
	{
		RemovePin();
	}

	public void RemovePin()
	{
		if (m_PinID != -1)
		{
			PinManager instance = PinManager.GetInstance();
			if (instance != null)
			{
				instance.RemovePin(m_PinID);
				m_PinID = -1;
			}
		}
	}
}
