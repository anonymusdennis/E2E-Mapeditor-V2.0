using UnityEngine;

public class ResourcesItemDataManager : MonoBehaviour
{
	private static ResourcesItemDataManager m_Instance;

	[SerializeField]
	private int[] m_RandomItemGroupIDs;

	[SerializeField]
	private string[] m_RandomItemGroupPaths;

	[SerializeField]
	private int[] m_ItemDataIDs;

	[SerializeField]
	private string[] m_ItemDataPaths;

	public static ResourcesItemDataManager GetInstance()
	{
		return m_Instance;
	}

	private void Start()
	{
		if (m_Instance != null && !object.ReferenceEquals(this, m_Instance))
		{
			Object.Destroy(m_Instance);
		}
		m_Instance = this;
	}

	public string GetRandomItemGroupResourcePath(int id)
	{
		int num = m_RandomItemGroupIDs.FindIndex((int x) => x == id);
		if (num != -1)
		{
			return m_RandomItemGroupPaths[num];
		}
		return "::ERROR BAD PATH::";
	}

	public string GetItemDataResourcePath(int id)
	{
		int num = m_ItemDataIDs.FindIndex((int x) => x == id);
		if (num != -1)
		{
			return m_ItemDataPaths[num];
		}
		return "::ERROR BAD PATH::";
	}
}
