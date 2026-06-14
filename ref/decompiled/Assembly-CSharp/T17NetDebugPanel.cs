public class T17NetDebugPanel
{
	private static float m_netGui_Height = 140f;

	private static float m_netGui_YOffset = 0f;

	private static float m_netGui_ConnectPanelOffset = m_netGui_YOffset;

	private static float m_netGui_ConnectPanelWidth = 180f;

	private static float m_netGui_RoomPanelOffset = m_netGui_ConnectPanelOffset + m_netGui_ConnectPanelWidth;

	private static float m_netGui_RoomPanelWidth = 390f;

	private static float m_netGui_ThirdPanelOffset = m_netGui_RoomPanelOffset + m_netGui_RoomPanelWidth;

	private static float m_netGui_PhotonPlayerPanelOffset = m_netGui_ThirdPanelOffset;

	private static float m_netGui_PhotonPlayerPanelWidth = 240f;

	private static float m_netGui_RoomListPanelOffset = m_netGui_ThirdPanelOffset;

	private static float m_netGui_RoomListPanelWidth = m_netGui_PhotonPlayerPanelWidth;

	private static float m_netGui_DebugPanelOffset = m_netGui_ThirdPanelOffset + m_netGui_RoomListPanelWidth;

	private static float m_netGui_DebugPanelWidth = 600f;

	private static float m_netGui_DebugPanelHeight = 300f;

	public static float NetGuiWindowHeight => m_netGui_Height;

	public static float NetGuiWindowYOffset => m_netGui_YOffset;

	public static float NetGuiConnectPanelOffset => m_netGui_ConnectPanelOffset;

	public static float NetGuiConnectPanelWidth => m_netGui_ConnectPanelWidth;

	public static float NetGuiRoomPanelOffset => m_netGui_RoomPanelOffset;

	public static float NetGuiRoomPanelWidth => m_netGui_RoomPanelWidth;

	public static float NetGuiRoomListPanelOffset => m_netGui_RoomListPanelOffset;

	public static float NetGuiRoomListPanelWidth => m_netGui_RoomListPanelWidth;

	public static float NetGuiPhotonPlayerPanelOffset => m_netGui_PhotonPlayerPanelOffset;

	public static float NetGuiPhotonPlayerPanelWidth => m_netGui_PhotonPlayerPanelWidth;

	public static float NetGuiDebugPanelOffset => m_netGui_DebugPanelOffset;

	public static float NetGuiDebugPanelWidth => m_netGui_DebugPanelWidth;

	public static float NetGuiDebugPanelHeight => m_netGui_DebugPanelHeight;
}
