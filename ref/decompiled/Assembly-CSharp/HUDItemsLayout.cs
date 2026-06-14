using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class HUDItemsLayout
{
	public Vector2 m_MiniMapPosition;

	public Vector2 m_MiniMapScale;

	public Vector2 m_StatsPosition;

	public Vector2 m_StatsScale;

	public Vector2 m_QuestPosition;

	public Vector2 m_QuestScale;

	public Vector2 m_EmotePosition;

	public Vector2 m_EmoteScale;

	public Vector2 m_IGMPosition;

	public Vector2 m_IGMScale;

	[FormerlySerializedAs("m_MainMapPosition")]
	public Vector2 m_MaskContainerPosition;

	[FormerlySerializedAs("m_MainMapSize")]
	public Vector2 m_MaskContainerSize;

	public Vector2 m_CalendarPosition;

	public Vector2 m_CalendarScale;

	public Vector2 m_PayphonePosition;

	public Vector2 m_PayphoneScale;

	public Vector2 m_JobsBoardPosition;

	public Vector2 m_JobsBoardScale;

	public Vector2 m_PlayerSelectPosition;

	public Vector2 m_PlayerSelectScale;

	public Vector2 m_TutorialPopupPosition;

	public Vector2 m_TutorialPopupScale;

	public Vector2 m_FadingCanvasPosition;

	public Vector2 m_FadingCanvasScale;

	public Vector2 m_BottomRightPosition = Vector2.one;

	public Vector2 m_BottomRightScale = Vector2.one;

	public Vector2 m_VoiceChatFeedPosition = Vector2.one;

	public Vector2 m_VoiceChatFeedScale = Vector2.one;

	[FormerlySerializedAs("m_JobTutorialPosition")]
	public Vector2 m_InformationMenuPosition;

	[FormerlySerializedAs("m_JobTutorialScale")]
	public Vector2 m_InformationMenuScale;

	public Vector2 m_ChatFeedPosition;

	public Vector2 m_ChatFeedScale;

	public Vector2 m_CentreCanvasPosition;

	public Vector2 m_CentreCanvasScale;

	public Vector2 m_FullScreenMapKeyOffset;

	public Vector2 m_FullScreenMapKeyScale;

	public Vector2 m_FullScreenMapFloorsOffset;

	public Vector2 m_FullScreenMapFloorsScale;

	public Vector2 m_FullScreenMapLegendOffset;

	public Vector2 m_FullScreenMapLegendScale;

	public Vector2 m_BedSaveMenuPosition;

	public Vector2 m_BedSaveMenuScale;

	private HUDItemsLayout()
	{
		m_MiniMapPosition = Vector2.zero;
		m_MiniMapScale = Vector2.one;
		m_StatsPosition = Vector2.zero;
		m_StatsScale = Vector2.one;
		m_QuestPosition = Vector2.zero;
		m_QuestScale = Vector2.one;
	}
}
