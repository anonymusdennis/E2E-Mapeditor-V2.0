using System.Collections.Generic;
using UnityEngine;

public class MenuDebugger : MonoBehaviour
{
	private class MenuItemContainer
	{
		public MenuDebugObject menuItem;

		public BaseMenuBehaviour linkedObject;

		public int m_ID;

		public int m_LinkedToID;
	}

	public static MenuDebugger Instance;

	public GameObject m_ScrollContentObject;

	public GameObject m_MenuItemPrefab;

	public GameObject m_MenuWithChildsPrefab;

	public T17Text m_AdditionalInfo;

	private List<MenuItemContainer> m_HierarchyTree = new List<MenuItemContainer>();

	private float m_UpdateDelay = 0.5f;

	private const float DELAY = 0.5f;

	private static int HIGHEST_ID;

	private bool show;

	private void Awake()
	{
		if (Instance != null)
		{
			Object.Destroy(this);
		}
		else
		{
			Instance = this;
		}
	}

	protected virtual void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void Start()
	{
		HideHierarchyTree();
	}

	public void HideHierarchyTree()
	{
		show = false;
		foreach (Transform item in base.transform)
		{
			item.gameObject.SetActive(value: false);
		}
	}
}
