using UnityEngine;

[RequireComponent(typeof(T17NetView))]
public class T17NetHighlightMasterClient : MonoBehaviour
{
	public GameObject PointerPrefab;

	public float Offset = 2f;

	private Transform markerTransform;

	public static bool ShowHighlight = true;

	private T17NetView m_netView;

	public void Start()
	{
		m_netView = GetComponent<T17NetView>();
	}

	protected virtual void OnDestroy()
	{
		m_netView = null;
	}

	private void Update()
	{
		bool flag = false;
		if (ShowHighlight && T17NetManager.NetOnlineMode && m_netView != null && m_netView.isOwnerTheMasterClient)
		{
			Gamer gamer = Gamer.FindGamer(-1, m_netView.ownerId, m_netView.viewID, Gamer.Location.LOCAL);
			flag = gamer != null;
		}
		if (flag)
		{
			if (markerTransform == null)
			{
				GameObject gameObject = Object.Instantiate(PointerPrefab);
				gameObject.transform.parent = base.gameObject.transform;
				markerTransform = gameObject.transform;
			}
			Vector3 position = base.gameObject.transform.position;
			markerTransform.position = new Vector3(position.x, position.y + Offset, position.z);
			markerTransform.rotation = Quaternion.identity;
		}
		else if (markerTransform != null)
		{
			Object.Destroy(markerTransform.gameObject);
			markerTransform = null;
		}
	}
}
