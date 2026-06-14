using System;
using System.Collections.Generic;
using UnityEngine;

public class ArrowManager : T17MonoBehaviour
{
	public enum ArrowType
	{
		RoutineArrow,
		ObjectiveArrow
	}

	[Serializable]
	public struct arrowData
	{
		public ArrowType type;

		public int total;

		public Sprite OffscreenArrowSprite;

		public Sprite OnscreenArrowSprite;

		public float ArrowMaxOnScreenDistance;

		public float ArrowDisableDistance;

		public Sprite[] FloorIndicatorSprites;

		public Sprite ChangeFloorIndicatorSprite;

		[Range(0f, 1f)]
		public float ChangeFloorIndicatorDistance;
	}

	private static ArrowManager m_Instance;

	public GameObject m_ArrowPrefab;

	public List<arrowData> m_ArrowData = new List<arrowData>();

	private List<GuideArrow>[] m_Arrows = new List<GuideArrow>[4];

	private CameraManager.PlayerBindingID[] m_GuideArrowsBindingID = new CameraManager.PlayerBindingID[4];

	private T17NetView m_NetView;

	private int m_ArrowID;

	private int m_PlayerCount;

	private const float c_MaxDistToTransitionPoint = 5.5f;

	public static ArrowManager GetInstance()
	{
		return m_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		m_Instance = this;
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
		m_NetView = null;
	}

	public void CheckArrowInstances()
	{
		if (m_NetView == null)
		{
			m_NetView = GetComponent<T17NetView>();
			if (m_NetView == null)
			{
				return;
			}
		}
		PhotonView component = GetComponent<PhotonView>();
		if (component.viewID != T17NetConfig.GetReservedNetID(T17NetConfig.ReservedNetID.HUDParentView))
		{
			component.viewID = 0;
			m_NetView.viewID = T17NetConfig.GetReservedNetID(T17NetConfig.ReservedNetID.HUDParentView);
		}
		int usedCameraCount = CameraManager.GetInstance().GetUsedCameraCount();
		if (m_PlayerCount < usedCameraCount)
		{
			for (int i = m_PlayerCount; i < usedCameraCount; i++)
			{
				CreateNewArrows(i);
			}
		}
		else if (m_PlayerCount > usedCameraCount)
		{
			ClearArrows();
			for (int j = 0; j < usedCameraCount; j++)
			{
				CreateNewArrows(j);
			}
		}
		m_PlayerCount = usedCameraCount;
	}

	private void CreateNewArrows(int playerID)
	{
		m_Arrows[playerID] = new List<GuideArrow>();
		m_GuideArrowsBindingID[playerID] = CameraManager.GetInstance().GetUsedBindingID(playerID);
		for (int i = 0; i < m_ArrowData.Count; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_ArrowPrefab);
			gameObject.transform.SetParent(HUDMenuFlow.Instance.m_PlayersHUDData[playerID].m_ArrowParent.transform, worldPositionStays: false);
			GuideArrow component = gameObject.GetComponent<GuideArrow>();
			component.m_Type = m_ArrowData[i].type;
			component.m_ArrowDisableDistance = m_ArrowData[i].ArrowDisableDistance;
			component.m_ArrowMaxOnScreenDistance = m_ArrowData[i].ArrowMaxOnScreenDistance;
			component.m_TargetCameraIndex = playerID;
			component.m_SafeSpace = gameObject.transform.parent.GetComponent<RectTransform>();
			component.m_ArrowRect = gameObject.GetComponent<RectTransform>();
			component.gameObject.SetActive(value: false);
			component.m_OffscreenArrowSprite = m_ArrowData[i].OffscreenArrowSprite;
			component.m_OnscreenArrowSprite = m_ArrowData[i].OnscreenArrowSprite;
			if (component.m_ArrowImage != null)
			{
				component.m_ArrowImage.sprite = m_ArrowData[i].OffscreenArrowSprite;
			}
			component.m_FloorIndicatorSprites = m_ArrowData[i].FloorIndicatorSprites;
			component.m_ChangeFloorIndicatorDistance = m_ArrowData[i].ChangeFloorIndicatorDistance;
			component.SetChangeFloorIndicator(m_ArrowData[i].ChangeFloorIndicatorSprite);
			m_Arrows[playerID].Add(component);
		}
		Player.GetAllPlayers()[playerID].HandleRoutineArrow();
	}

	public int SetArrowTargetRPC(T17NetView playerNetView, ArrowType type, RoomBlob targetRoom, int targetArrowID = -1, bool bShowOnscreenIndicator = true, int targetfloorindex = -1)
	{
		if (m_NetView != null && targetRoom != null)
		{
			byte b = (byte)type;
			int num = ((targetArrowID == -1) ? m_ArrowID++ : targetArrowID);
			if (num != -1)
			{
				m_NetView.RPC("RPC_SetArrowTargetRoom", playerNetView, b, targetRoom.m_ID, playerNetView.viewID, num, bShowOnscreenIndicator, targetfloorindex);
				return num;
			}
		}
		return -1;
	}

	public int SetArrowTargetRPC(T17NetView playerNetView, ArrowType type, Vector3 targetPos, int targetArrowID = -1, bool bShowOnscreenIndicator = true, int targetfloorindex = -1, bool changesFloor = false)
	{
		if (m_NetView != null)
		{
			byte b = (byte)type;
			int num = ((targetArrowID == -1) ? m_ArrowID++ : targetArrowID);
			if (num != -1)
			{
				m_NetView.RPC("RPC_SetArrowTargetPos", playerNetView, b, targetPos.x, targetPos.y, targetPos.z, playerNetView.viewID, num, bShowOnscreenIndicator, targetfloorindex, changesFloor);
				return num;
			}
		}
		return -1;
	}

	public int SetArrowTargetRPC(T17NetView playerNetView, ArrowType type, T17NetView targetObject, int targetArrowID = -1, bool bShowOnscreenIndicator = true, int targetfloorindex = -1)
	{
		if (m_NetView != null && targetObject != null)
		{
			byte b = (byte)type;
			int num = ((targetArrowID == -1) ? m_ArrowID++ : targetArrowID);
			if (num != -1)
			{
				m_NetView.RPC("RPC_SetArrowTargetObj", playerNetView, b, targetObject.viewID, playerNetView.viewID, num, bShowOnscreenIndicator, targetfloorindex);
				return num;
			}
		}
		return -1;
	}

	[PunRPC]
	public void RPC_SetArrowTargetRoom(byte aTypeAsByte, int roomID, int playerNetViewID, int arrowID, bool bShowOnscreenIndicator, int targetFloorIndex, PhotonMessageInfo info)
	{
		Gamer gamerByViewID = Gamer.GetGamerByViewID(playerNetViewID);
		RoomBlob roomBlob = RoomManager.GetInstance().LookUpRoom(roomID);
		if (gamerByViewID != null && gamerByViewID.IsLocal() && roomBlob != null)
		{
			SetArrowTarget((ArrowType)aTypeAsByte, roomBlob, Vector3.zero, null, gamerByViewID.m_PlayerObject.m_PlayerCameraManagerBindingID, arrowID, bShowOnscreenIndicator, targetFloorIndex);
		}
	}

	[PunRPC]
	public void RPC_SetArrowTargetPos(byte aTypeAsByte, float x, float y, float z, int playerNetViewID, int arrowID, bool bShowOnscreenIndicator, int targetFloorIndex, bool changesFloor, PhotonMessageInfo info)
	{
		Vector3 vector = default(Vector3);
		vector.x = x;
		vector.y = y;
		vector.z = z;
		Gamer gamerByViewID = Gamer.GetGamerByViewID(playerNetViewID);
		if (gamerByViewID != null && gamerByViewID.IsLocal() && vector != Vector3.zero)
		{
			SetArrowTarget((ArrowType)aTypeAsByte, null, vector, null, gamerByViewID.m_PlayerObject.m_PlayerCameraManagerBindingID, arrowID, bShowOnscreenIndicator, targetFloorIndex, changesFloor);
		}
	}

	[PunRPC]
	public void RPC_SetArrowTargetObj(byte aTypeAsByte, int objNetViewID, int playerNetViewID, int arrowID, bool bShowOnscreenIndicator, int targetFloorIndex, PhotonMessageInfo info)
	{
		Gamer gamerByViewID = Gamer.GetGamerByViewID(playerNetViewID);
		Transform transform = T17NetView.Find<Transform>(objNetViewID);
		if (gamerByViewID != null && gamerByViewID.IsLocal() && transform != null)
		{
			SetArrowTarget((ArrowType)aTypeAsByte, null, Vector3.zero, transform, gamerByViewID.m_PlayerObject.m_PlayerCameraManagerBindingID, arrowID, bShowOnscreenIndicator, targetFloorIndex);
		}
	}

	private void SetArrowTarget(ArrowType type, RoomBlob targetRoom, Vector3 targetPos, Transform targetTransform, CameraManager.PlayerBindingID bindingID, int arrowID, bool bShowOnscreenIndicator, int destinationFloorIndex, bool changesFloor = false)
	{
		if (arrowID > m_ArrowID)
		{
			m_ArrowID = arrowID;
		}
		for (int i = 0; i < 4; i++)
		{
			if (bindingID != 0 && m_GuideArrowsBindingID[i] != bindingID)
			{
				continue;
			}
			if (m_Arrows[i] == null)
			{
				m_Arrows[i] = new List<GuideArrow>();
			}
			List<GuideArrow> list = m_Arrows[i];
			GuideArrow arrow = null;
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j] != null && list[j].m_Type == type && list[j].m_ID == arrowID)
				{
					arrow = list[j];
					break;
				}
			}
			if (arrow == null)
			{
				for (int k = 0; k < list.Count; k++)
				{
					if (list[k] != null && list[k].m_Type == type && !list[k].gameObject.activeSelf)
					{
						arrow = list[k];
						break;
					}
				}
			}
			if (arrow != null)
			{
				SetArrowValues(ref arrow, targetRoom, targetPos, targetTransform, arrowID, bShowOnscreenIndicator, destinationFloorIndex, changesFloor);
			}
		}
	}

	private void SetArrowValues(ref GuideArrow arrow, RoomBlob targetRoom, Vector3 targetPos, Transform targetTransform, int m_ArrowID, bool showOnscreenIndicator, int destinationFloorIndex, bool changesFloor)
	{
		arrow.gameObject.SetActive(value: true);
		arrow.TargetTransform = targetTransform;
		arrow.TargetRoom = targetRoom;
		arrow.TargetPos = targetPos;
		arrow.m_ID = m_ArrowID;
		arrow.m_bShowOnscreenIndicator = showOnscreenIndicator;
		arrow.m_TargetFloorIndex = destinationFloorIndex;
		arrow.m_bShowChangeFloorIndicator = false;
		arrow.m_bHideArrowChangeFloor = false;
		if (changesFloor)
		{
			TransitionPoint transitionPoint = TransitionPoint.FindClosest(targetPos, 5.5f);
			if (transitionPoint != null)
			{
				arrow.TargetPos = transitionPoint.GetHighlightPosition();
				arrow.m_bShowChangeFloorIndicator = true;
			}
			else
			{
				arrow.m_bHideArrowChangeFloor = true;
			}
		}
	}

	public void CancelArrow(T17NetView playerNetView, int arrowID)
	{
		if (m_NetView != null)
		{
			m_NetView.RPC("RPC_CancelArrow", playerNetView, playerNetView.viewID, arrowID);
		}
	}

	[PunRPC]
	public void RPC_CancelArrow(int playerNetViewID, int arrowID, PhotonMessageInfo info)
	{
		Gamer gamerByViewID = Gamer.GetGamerByViewID(playerNetViewID);
		if (gamerByViewID == null || !(gamerByViewID.m_PlayerObject != null))
		{
			return;
		}
		CameraManager.PlayerBindingID playerCameraManagerBindingID = gamerByViewID.m_PlayerObject.m_PlayerCameraManagerBindingID;
		bool flag = false;
		int i;
		for (i = 0; i < 4; i++)
		{
			if (m_GuideArrowsBindingID[i] == playerCameraManagerBindingID)
			{
				flag = true;
				break;
			}
		}
		if (!flag || m_Arrows[i] == null)
		{
			return;
		}
		for (int j = 0; j < m_Arrows[i].Count; j++)
		{
			GuideArrow guideArrow = m_Arrows[i][j];
			if (guideArrow != null && arrowID == guideArrow.m_ID)
			{
				if (guideArrow.gameObject != null)
				{
					guideArrow.gameObject.SetActive(value: false);
				}
				guideArrow.m_ID = -1;
				guideArrow.TargetRoom = null;
			}
		}
	}

	private void ClearArrows()
	{
		for (int i = 0; i < m_Arrows.Length; i++)
		{
			if (m_Arrows[i] == null)
			{
				continue;
			}
			for (int j = 0; j < m_Arrows[i].Count; j++)
			{
				if ((bool)m_Arrows[i][j])
				{
					UnityEngine.Object.Destroy(m_Arrows[i][j].gameObject);
				}
			}
			m_Arrows[i].Clear();
		}
	}
}
