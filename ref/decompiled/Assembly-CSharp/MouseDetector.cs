using Rewired;
using UnityEngine;

public class MouseDetector : MonoBehaviour
{
	private Rewired.Player m_RewiredPlayer;

	private Mouse m_PlayerMouse;

	private Item m_CurrentItem;

	private NetObjectLock m_CurrentInteractiveObject;

	private Character m_CurrentCharacter;

	private Character m_HighlightedCharacter;

	private NetObjectLock m_HighlightedInteractiveObject;

	private Item m_HighlightedItem;

	protected Player m_Owner;

	private RaycastHit[] m_RayHits = new RaycastHit[16];

	private int m_CharacterMask;

	private int m_AllMask;

	private const int m_HitSize = 16;

	private void Awake()
	{
		m_Owner = GetComponentInParent<Player>();
		m_CharacterMask = LayerMask.GetMask("Characters");
		m_AllMask = LayerMask.GetMask("StaticMapObject", "DynamicMapObject", "CharacterCollision", "Characters", "Items");
	}

	private void Update()
	{
		if (!Cursor.visible || m_Owner == null)
		{
			return;
		}
		if (m_PlayerMouse == null && m_Owner.m_Gamer != null)
		{
			m_RewiredPlayer = m_Owner.m_Gamer.m_RewiredPlayer;
			if (m_RewiredPlayer != null && m_RewiredPlayer.controllers.hasMouse)
			{
				m_PlayerMouse = m_RewiredPlayer.controllers.Mouse;
			}
		}
		if (m_PlayerMouse == null)
		{
			return;
		}
		m_CurrentCharacter = null;
		m_CurrentItem = null;
		m_CurrentInteractiveObject = null;
		FloorManager.Floor currentFloor = m_Owner.CurrentFloor;
		Camera camera = CameraManager.GetInstance().GetCamera(m_Owner.m_PlayerCameraManagerBindingID);
		if (camera != null)
		{
			Vector2 screenPosition = m_PlayerMouse.screenPosition;
			Vector2 offset = default(Vector2);
			GetMouseToCameraOffset(camera, ref offset);
			screenPosition += offset;
			Vector3 position = new Vector3(screenPosition.x, screenPosition.y, camera.nearClipPlane);
			position = camera.ScreenToWorldPoint(position);
			position.z = currentFloor.m_zPos;
			float num = 1.5f;
			Vector3 direction = new Vector3(0f, 0f, 1f);
			position.z -= num;
			int a = Physics.RaycastNonAlloc(position, direction, m_RayHits, num, m_AllMask, QueryTriggerInteraction.Collide);
			a = Mathf.Min(a, 16);
			for (int i = 0; i < a; i++)
			{
				if (m_RayHits[i].transform != null)
				{
					Transform transform = m_RayHits[i].transform;
					Character component = transform.gameObject.GetComponent<Character>();
					if (component != null && m_CurrentCharacter == null)
					{
						m_CurrentCharacter = component;
					}
					Item component2 = transform.gameObject.GetComponent<Item>();
					if (component2 != null && m_CurrentItem == null)
					{
						m_CurrentItem = component2;
					}
					InteractiveObject component3 = transform.gameObject.GetComponent<InteractiveObject>();
					if (component3 != null && m_CurrentInteractiveObject == null && component3.InteractionVisibility())
					{
						m_CurrentInteractiveObject = component3.m_NetObjectLock;
					}
				}
			}
		}
		m_HighlightedCharacter = m_CurrentCharacter;
		m_HighlightedInteractiveObject = m_CurrentInteractiveObject;
		m_HighlightedItem = m_CurrentItem;
	}

	public void GetCurrentItem(ref Item item)
	{
		item = m_HighlightedItem;
	}

	public void GetCurrentInteractiveObject(ref NetObjectLock interactiveObject)
	{
		interactiveObject = m_HighlightedInteractiveObject;
	}

	public void GetCurrentCharacter(ref TrackableUIElementsReporter character)
	{
		if (m_HighlightedCharacter == null)
		{
			character = null;
		}
		else
		{
			character = m_HighlightedCharacter.m_TrackableElementReporter;
		}
	}

	public static void GetMouseToCameraOffset(Camera cam, ref Vector2 offset)
	{
		if (!(cam == null))
		{
			offset.x = (float)cam.pixelWidth - (float)Screen.width * cam.rect.size.x;
			offset.y = (float)cam.pixelHeight - (float)Screen.height * cam.rect.size.y;
			offset *= 0.5f;
			if (cam.rect.center.y > 0.5f)
			{
				offset.y += offset.y * (1f / cam.rect.size.y);
			}
		}
	}

	public void MouseOverCharacterManualUpdate()
	{
		if (!Cursor.visible || m_Owner == null)
		{
			return;
		}
		if (m_RewiredPlayer == null)
		{
			if (m_Owner.m_Gamer != null)
			{
				m_RewiredPlayer = m_Owner.m_Gamer.m_RewiredPlayer;
				if (m_RewiredPlayer != null && m_RewiredPlayer.controllers.hasMouse)
				{
					m_PlayerMouse = m_RewiredPlayer.controllers.Mouse;
				}
			}
			if (m_RewiredPlayer == null)
			{
				return;
			}
		}
		if (m_PlayerMouse == null)
		{
			return;
		}
		FloorManager.Floor currentFloor = m_Owner.CurrentFloor;
		Camera camera = CameraManager.GetInstance().GetCamera(m_Owner.m_PlayerCameraManagerBindingID);
		if (camera != null)
		{
			Vector2 screenPosition = m_PlayerMouse.screenPosition;
			Vector2 offset = default(Vector2);
			GetMouseToCameraOffset(camera, ref offset);
			screenPosition += offset;
			Vector3 position = new Vector3(screenPosition.x, screenPosition.y, camera.nearClipPlane);
			position = camera.ScreenToWorldPoint(position);
			position.z = currentFloor.m_zPos;
			float num = 1.5f;
			Vector3 direction = new Vector3(0f, 0f, 1f);
			position.z -= num;
			int a = Physics.RaycastNonAlloc(position, direction, m_RayHits, num, m_CharacterMask, QueryTriggerInteraction.Collide);
			a = Mathf.Min(a, 16);
			for (int i = 0; i < a; i++)
			{
				if (m_RayHits[i].transform != null)
				{
					Character component = m_RayHits[i].transform.gameObject.GetComponent<Character>();
					if (component != null)
					{
						m_CurrentCharacter = component;
						break;
					}
				}
			}
		}
		m_HighlightedCharacter = m_CurrentCharacter;
		m_CurrentCharacter = null;
	}
}
